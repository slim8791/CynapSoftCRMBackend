using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Field;
using Cynapharm_Mobile.Models.Products;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Rapports;

[QueryProperty(nameof(LinkedVisiteId), "visiteId")]
public partial class RapportViewModel : BaseViewModel
{
    private readonly VisiteService _visiteService;
    private readonly ProductService _productService;
    private readonly LocalDatabaseService _localDb;

    [ObservableProperty] private int _linkedVisiteId;

    // ── Validated field: minimum 20 chars ─────────────────────────────────────
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Le contenu du rapport est requis.")]
    [MinLength(20, ErrorMessage = "Le rapport doit contenir au moins 20 caractères.")]
    [NotifyPropertyChangedFor(nameof(ContenuError))]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    private string _contenu = string.Empty;

    [ObservableProperty] private string _resultat = "POSITIF";

    // ── Geolocation state ─────────────────────────────────────────────────────
    [ObservableProperty] private string _geoStatus = string.Empty;
    [ObservableProperty] private double? _capturedLatitude;
    [ObservableProperty] private double? _capturedLongitude;
    [ObservableProperty] private bool _isCapturingLocation;

    public ObservableCollection<ProductCheckItem> ProduitsDiscutes { get; } = new();
    public List<string> ResultatOptions { get; } = new() { "POSITIF", "NEGATIF", "EN_ATTENTE" };

    // ── Validation helpers ────────────────────────────────────────────────────

    public string ContenuError =>
        GetErrors(nameof(Contenu))
            .Cast<ValidationResult>()
            .FirstOrDefault()?.ErrorMessage ?? string.Empty;

    // Submit is allowed when there are no validation errors and the VM is not busy
    public bool CanSubmit => !HasErrors && !IsBusy;

    public RapportViewModel(
        VisiteService visiteService,
        ProductService productService,
        LocalDatabaseService localDb)
    {
        _visiteService = visiteService;
        _productService = productService;
        _localDb = localDb;
        Title = "Rapport de visite";

        // Re-evaluate CanSubmit whenever validation errors change
        ErrorsChanged += (_, _) => OnPropertyChanged(nameof(CanSubmit));
    }

    partial void OnLinkedVisiteIdChanged(int value)
    {
        if (value > 0) _ = LoadProduitsAsync();
    }

    [RelayCommand]
    private async Task LoadProduitsAsync()
    {
        try
        {
            ProduitsDiscutes.Clear();

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                var products = await _productService.GetProductsAsync(null, 100);
                if (products != null && products.Count > 0)
                {
                    foreach (var p in products.Where(p => p.Actif && !p.IsArchived))
                        ProduitsDiscutes.Add(new ProductCheckItem
                        {
                            ProductId        = p.Id,
                            ProductNom       = p.Nom,
                            ProductReference = p.Reference
                        });
                    return;
                }
            }

            // Offline fallback — SQLite cache
            var cached = await _localDb.SearchProductsAsync(null);
            foreach (var p in cached)
                ProduitsDiscutes.Add(new ProductCheckItem
                {
                    ProductId        = p.Id,
                    ProductNom       = p.Nom,
                    ProductReference = p.Reference
                });
        }
        catch { /* non-blocking */ }
    }

    // ── Geolocation capture ───────────────────────────────────────────────────

    /// <summary>
    /// Called from OnAppearing — uses GetLastKnownLocationAsync (fast, no permission dialog)
    /// to pre-populate GeoStatus so the user sees their GPS state before submitting.
    /// The full accurate fix still runs in CaptureLocationAsync at submit time.
    /// </summary>
    [RelayCommand]
    private async Task PreCaptureLocationAsync()
    {
        try
        {
            var last = await Geolocation.GetLastKnownLocationAsync();
            if (last != null)
            {
                CapturedLatitude  = last.Latitude;
                CapturedLongitude = last.Longitude;
                var age = DateTime.UtcNow - last.Timestamp.UtcDateTime;
                GeoStatus = $"📍 Dernière position : {last.Latitude:F4}, {last.Longitude:F4}" +
                            $" (il y a {(int)age.TotalMinutes} min)";
            }
            else
            {
                GeoStatus = "📍 En attente du signal GPS…";
            }
        }
        catch
        {
            GeoStatus = "⚠️ Géolocalisation indisponible";
        }
    }

    private async Task<(double? lat, double? lon)> CaptureLocationAsync()
    {
        try
        {
            IsCapturingLocation = true;
            GeoStatus = "📍 Localisation en cours…";

            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                GeoStatus = "⚠️ Permission de localisation refusée";
                return (null, null);
            }

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.GetLocationAsync(request);

            if (location != null)
            {
                GeoStatus = $"✅ Position capturée ({location.Latitude:F4}, {location.Longitude:F4})";
                return (location.Latitude, location.Longitude);
            }

            GeoStatus = "⚠️ Position non disponible";
            return (null, null);
        }
        catch (FeatureNotSupportedException)
        {
            GeoStatus = "⚠️ Géolocalisation non supportée";
            return (null, null);
        }
        catch (PermissionException)
        {
            GeoStatus = "⚠️ Permission refusée";
            return (null, null);
        }
        catch
        {
            GeoStatus = "⚠️ Erreur de localisation";
            return (null, null);
        }
        finally
        {
            IsCapturingLocation = false;
        }
    }

    // ── Submit ────────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task SubmitAsync()
    {
        ValidateAllProperties();
        if (HasErrors) return;

        SetBusy(true);
        OnPropertyChanged(nameof(CanSubmit));
        try
        {
            // Capture GPS coordinates silently
            var (lat, lon) = await CaptureLocationAsync();
            CapturedLatitude  = lat;
            CapturedLongitude = lon;

            var selectedIds = ProduitsDiscutes
                .Where(p => p.IsSelected)
                .Select(p => p.ProductId)
                .ToList();

            var rapport = new Rapport
            {
                VisiteId         = LinkedVisiteId,
                Contenu          = Contenu,
                Resultat         = Resultat,
                ProduitsDiscutes = selectedIds.Count > 0 ? JsonSerializer.Serialize(selectedIds) : null,
                DateSoumission   = DateTime.Now,
                Latitude         = lat,
                Longitude        = lon
            };

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                await _visiteService.CreateRapportAsync(rapport);
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                // Save to SQLite pending queue for later sync
                await _localDb.InsertPendingRapportAsync(new PendingRapportEntry
                {
                    VisiteId           = rapport.VisiteId,
                    Contenu            = rapport.Contenu,
                    ProduitsDiscutes   = rapport.ProduitsDiscutes,
                    Resultat           = rapport.Resultat,
                    DateSoumissionTicks = rapport.DateSoumission.Ticks,
                    Latitude           = lat,
                    Longitude          = lon,
                    IsSynced           = false
                });
                await Shell.Current.DisplayAlert(
                    "Enregistré hors ligne",
                    "Le rapport a été sauvegardé localement et sera synchronisé à la prochaine connexion.",
                    "OK");
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception)
        {
            ErrorMessage = "Erreur lors de la soumission du rapport.";
        }
        finally
        {
            SetBusy(false);
            OnPropertyChanged(nameof(CanSubmit));
        }
    }
}
