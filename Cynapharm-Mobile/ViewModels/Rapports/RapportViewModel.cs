using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Cynapharm_Mobile.Messages;
using Cynapharm_Mobile.Models.Field;
using Cynapharm_Mobile.Models.Products;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Rapports;

[QueryProperty(nameof(LinkedVisiteId), "visiteId")]
[QueryProperty(nameof(RapportMode),    "rapportMode")]
public partial class RapportViewModel : BaseViewModel
{
    private readonly VisiteService        _visiteService;
    private readonly ProductService       _productService;
    private readonly LocalDatabaseService _localDb;

    // ── Query properties ──────────────────────────────────────────────────────
    [ObservableProperty] private int    _linkedVisiteId;
    /// <summary>"edit" = load existing rapport for update; "view" = read-only; empty = create.</summary>
    [ObservableProperty] private string _rapportMode = string.Empty;

    // ── Existing rapport id (0 = create, >0 = update) ─────────────────────────
    private int _existingRapportId;

    // ── Validated field: minimum 20 chars ─────────────────────────────────────
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Le contenu du rapport est requis.")]
    [MinLength(20, ErrorMessage = "Le rapport doit contenir au moins 20 caractères.")]
    [NotifyPropertyChangedFor(nameof(ContenuError))]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    private string _contenu = string.Empty;

    /// <summary>Selected value for the Résultat Picker.</summary>
    [ObservableProperty] private string _selectedResultat = "POSITIF";

    // ── Geolocation state ─────────────────────────────────────────────────────
    [ObservableProperty] private string _geoStatus = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    private double? _capturedLatitude;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    private double? _capturedLongitude;

    [ObservableProperty] private bool _isCapturingLocation;

    /// <summary>True when GPS is unavailable — shows the warning banner.</summary>
    [ObservableProperty] private bool _isGpsWarningVisible;

    /// <summary>Detailed message explaining why GPS is unavailable (shown inside the warning banner).</summary>
    [ObservableProperty] private string _gpsWarningMessage = string.Empty;

    // ── Read-only mode ────────────────────────────────────────────────────────

    /// <summary>True when the rapport is locked (superviseur has validated) — all inputs disabled.</summary>
    public bool IsReadOnly    => RapportMode == "view";
    /// <summary>Inverse of IsReadOnly — used for IsEnabled bindings.</summary>
    public bool IsNotReadOnly => !IsReadOnly;

    /// <summary>Label for the sticky submit button — varies by mode.</summary>
    public string SubmitButtonText => RapportMode == "edit"
        ? "Mettre à jour le rapport"
        : "Soumettre le rapport";

    // ── Products ──────────────────────────────────────────────────────────────
    public ObservableCollection<ProductCheckItem> ProduitsDiscutes { get; } = new();

    public List<string> ResultatOptions { get; } = new()
    {
        "POSITIF",
        "NÉGATIF",
        "À RELANCER",
        "SANS SUITE"
    };

    // ── Validation helpers ────────────────────────────────────────────────────

    public string ContenuError =>
        GetErrors(nameof(Contenu))
            .Cast<ValidationResult>()
            .FirstOrDefault()?.ErrorMessage ?? string.Empty;

    /// <summary>
    /// Submit is enabled only when the form is valid, the VM is not busy, the rapport is not
    /// locked, AND GPS coordinates have been captured (mandatory for business compliance).
    /// </summary>
    public bool CanSubmit =>
        !HasErrors
        && !IsBusy
        && !IsReadOnly
        && CapturedLatitude.HasValue
        && CapturedLongitude.HasValue;

    public RapportViewModel(
        VisiteService visiteService,
        ProductService productService,
        LocalDatabaseService localDb)
    {
        _visiteService  = visiteService;
        _productService = productService;
        _localDb        = localDb;
        Title = "Rapport de visite";

        // Re-evaluate CanSubmit whenever validation errors change
        ErrorsChanged += (_, _) => OnPropertyChanged(nameof(CanSubmit));

        // Re-evaluate CanSubmit whenever IsBusy changes (managed by ExecuteAsync)
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(IsBusy) or nameof(RapportMode))
            {
                OnPropertyChanged(nameof(IsReadOnly));
                OnPropertyChanged(nameof(IsNotReadOnly));
                OnPropertyChanged(nameof(CanSubmit));
            }
        };
    }

    partial void OnRapportModeChanged(string value)
    {
        Title = value switch
        {
            "edit" => "Modifier le rapport",
            "view" => "Voir le rapport",
            _      => "Rapport de visite"
        };
        OnPropertyChanged(nameof(IsReadOnly));
        OnPropertyChanged(nameof(IsNotReadOnly));
        OnPropertyChanged(nameof(CanSubmit));
        OnPropertyChanged(nameof(SubmitButtonText));
    }

    [RelayCommand]
    private static async Task CloseAsync()
        => await Shell.Current.GoToAsync("..");

    // ── Products + existing rapport load ─────────────────────────────────────

    /// <summary>
    /// Loads the product list (online or offline cache).
    /// When in edit/view mode, also fetches the existing rapport and pre-fills the form.
    /// Called solely from OnAppearing — removing the OnLinkedVisiteIdChanged auto-trigger
    /// prevents the double-load race condition (LinkedVisiteId is set before RapportMode).
    /// </summary>
    [RelayCommand]
    private Task LoadProduitsAsync() => ExecuteAsync(async () =>
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

                // In edit/view mode: pre-fill from the existing rapport
                if (LinkedVisiteId > 0 && RapportMode is "edit" or "view")
                    await PreFillExistingRapportAsync();

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

        if (LinkedVisiteId > 0 && RapportMode is "edit" or "view")
            await PreFillExistingRapportAsync();
    });

    /// <summary>
    /// Fetches the existing rapport for the current visite and pre-fills all form fields.
    /// Called after products are loaded so checkboxes can be correctly marked.
    /// </summary>
    private async Task PreFillExistingRapportAsync()
    {
        try
        {
            var rapports = await _visiteService.GetRapportsAsync(LinkedVisiteId);
            var existing = rapports?.FirstOrDefault();
            if (existing == null) return;

            _existingRapportId = existing.Id;
            Contenu         = existing.Contenu ?? string.Empty;
            SelectedResultat = !string.IsNullOrWhiteSpace(existing.Resultat)
                ? existing.Resultat
                : "POSITIF";

            // Mark the products that were discussed in the original rapport
            if (!string.IsNullOrWhiteSpace(existing.ProduitsDiscutes))
            {
                try
                {
                    var elements = JsonSerializer.Deserialize<List<JsonElement>>(existing.ProduitsDiscutes);
                    if (elements != null)
                    {
                        var selectedIds = elements
                            .Where(e => e.TryGetProperty("id", out _))
                            .Select(e => e.GetProperty("id").GetInt32())
                            .ToHashSet();

                        foreach (var item in ProduitsDiscutes)
                            item.IsSelected = selectedIds.Contains(item.ProductId);
                    }
                }
                catch { /* silently ignore JSON parse errors */ }
            }
        }
        catch { /* non-critical — form stays empty if pre-fill fails */ }
    }

    // ── Geolocation capture ───────────────────────────────────────────────────

    /// <summary>
    /// Called from OnAppearing.
    /// Shows the system GPS permission dialog, then tries the last-known position (max 10 min old).
    /// Falls back to an active 8-second acquisition when the cached position is stale or absent.
    /// On denial → DisplayAlert; on GPS disabled → DisplayAlert offering to open Settings.
    /// In view/read-only mode this is skipped entirely.
    /// </summary>
    [RelayCommand]
    private async Task PreCaptureLocationAsync()
    {
        if (IsReadOnly) return;

        IsCapturingLocation = true;
        GeoStatus           = "🔍 Vérification de la permission GPS…";

        try
        {
            // Step 1: show system permission dialog (first run) or check current status
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                IsCapturingLocation = false;
                GeoStatus           = "⚠️ GPS requis — non autorisé";
                GpsWarningMessage   = "La localisation GPS est obligatoire pour soumettre " +
                                      "un rapport de visite. Autorisez l'accès dans les paramètres " +
                                      "de l'application.";
                IsGpsWarningVisible = true;
                await Shell.Current.DisplayAlert(
                    "Permission GPS requise",
                    "Veuillez autoriser l'accès à la localisation pour pouvoir soumettre un rapport de visite.",
                    "OK");
                return;
            }

            GeoStatus = "📍 Recherche de la position…";

            // Step 2: try last-known position — use it only when fresh (≤ 10 min)
            var last = await Geolocation.GetLastKnownLocationAsync();
            var age  = last != null
                ? DateTime.UtcNow - last.Timestamp.UtcDateTime
                : TimeSpan.MaxValue;

            if (last != null && age.TotalMinutes <= 10)
            {
                CapturedLatitude    = last.Latitude;
                CapturedLongitude   = last.Longitude;
                IsGpsWarningVisible = false;
                GpsWarningMessage   = string.Empty;
                GeoStatus = $"✅ Position détectée — {last.Latitude:F4}, {last.Longitude:F4}" +
                            $" (il y a {(int)age.TotalMinutes} min)";
                IsCapturingLocation = false;
                return;
            }

            // Step 3: active fallback — request a fresh fix with 8-second timeout
            GeoStatus = "📍 Acquisition du signal GPS…";
            var request  = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(8));
            var location = await Geolocation.GetLocationAsync(request);

            if (location != null)
            {
                CapturedLatitude    = location.Latitude;
                CapturedLongitude   = location.Longitude;
                IsGpsWarningVisible = false;
                GpsWarningMessage   = string.Empty;
                GeoStatus = $"✅ Position capturée — {location.Latitude:F4}, {location.Longitude:F4}";
                IsCapturingLocation = false;
                return;
            }

            // Active request returned null — signal too weak
            IsCapturingLocation = false;
            GeoStatus           = "⚠️ GPS activé mais signal absent";
            GpsWarningMessage   = "Aucun signal GPS détecté. Placez-vous à l'extérieur ou " +
                                  "près d'une fenêtre et appuyez sur « 🔄 Réessayer ».";
            IsGpsWarningVisible = true;
        }
        catch (FeatureNotEnabledException)
        {
            IsCapturingLocation = false;
            GeoStatus           = "⚠️ GPS désactivé dans les paramètres";
            GpsWarningMessage   = "Activez la localisation dans Paramètres → Localisation " +
                                  "et appuyez sur « 🔄 Réessayer ».";
            IsGpsWarningVisible = true;
            var openSettings = await Shell.Current.DisplayAlert(
                "GPS désactivé",
                "La localisation est désactivée sur votre appareil. Voulez-vous ouvrir les paramètres pour l'activer ?",
                "Ouvrir les paramètres", "Annuler");
            if (openSettings)
                AppInfo.ShowSettingsUI();
        }
        catch
        {
            IsCapturingLocation = false;
            GeoStatus           = "⚠️ Géolocalisation indisponible";
            GpsWarningMessage   = "Impossible d'accéder au GPS. Vérifiez les paramètres de localisation.";
            IsGpsWarningVisible = true;
        }
    }

    /// <summary>Retry GPS capture — bound to the "🔄 Réessayer" button in the warning banner.</summary>
    [RelayCommand]
    private Task RefreshGpsAsync() => PreCaptureLocationAsync();

    /// <summary>Opens the device's app settings so the user can enable Location permission.</summary>
    [RelayCommand]
    private static void OpenGpsSettings() => AppInfo.ShowSettingsUI();

    private async Task<(double? lat, double? lon)> CaptureLocationAsync()
    {
        try
        {
            IsCapturingLocation = true;
            GeoStatus = "📍 Localisation en cours…";

            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                GeoStatus = "⚠️ Permission de localisation refusée";
                return (null, null);
            }

            var request  = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.GetLocationAsync(request);

            if (location != null)
            {
                GeoStatus = $"✅ Position capturée ({location.Latitude:F4}, {location.Longitude:F4})";
                return (location.Latitude, location.Longitude);
            }

            GeoStatus = "⚠️ Position non disponible";
            return (null, null);
        }
        catch (FeatureNotSupportedException)  { GeoStatus = "⚠️ Géolocalisation non supportée";    return (null, null); }
        catch (FeatureNotEnabledException)    { GeoStatus = "⚠️ GPS désactivé — coordonnées non disponibles"; return (null, null); }
        catch (PermissionException)           { GeoStatus = "⚠️ Permission refusée";                return (null, null); }
        catch                                 { GeoStatus = "⚠️ Erreur de localisation";             return (null, null); }
        finally                               { IsCapturingLocation = false; }
    }

    // ── Submit ────────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task SubmitAsync()
    {
        // Guard: should never be called in view mode (button is hidden), but safety check
        if (IsReadOnly) return;

        // ── FIX-4: early client-side validation ──────────────────────────────

        ValidateAllProperties();
        if (HasErrors) return;

        if (LinkedVisiteId == 0)
        {
            await Shell.Current.DisplayAlert("Erreur",
                "Visite non identifiée. Revenez en arrière et réessayez.", "OK");
            return;
        }

        var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
        if (!int.TryParse(userIdStr, out var uid) || uid == 0)
        {
            await Shell.Current.DisplayAlert("Session expirée", "Veuillez vous reconnecter.", "OK");
            await Shell.Current.GoToAsync("//login");
            return;
        }

        if (string.IsNullOrWhiteSpace(Contenu) || Contenu.Length < 20)
        {
            await Shell.Current.DisplayAlert("Validation",
                "Le contenu du rapport doit contenir au moins 20 caractères.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedResultat))
        {
            await Shell.Current.DisplayAlert("Validation", "Veuillez sélectionner un résultat.", "OK");
            return;
        }

        // ── API call wrapped in ExecuteAsync so any ApiException shows a DisplayAlert ─

        await ExecuteAsync(async () =>
        {
            var (lat, lon) = await CaptureLocationAsync();

            if (lat == null || lon == null)
            {
                await Shell.Current.DisplayAlert(
                    "GPS requis",
                    "Votre position GPS est nécessaire pour certifier votre passage. " +
                    "Activez la localisation dans Paramètres → Localisation et réessayez.",
                    "OK");
                return;
            }

            CapturedLatitude  = lat;
            CapturedLongitude = lon;

            var selectedProducts = ProduitsDiscutes.Where(p => p.IsSelected).ToList();
            var produitsJson = selectedProducts.Count > 0
                ? JsonSerializer.Serialize(
                      selectedProducts.Select(p => new { id = p.ProductId, nom = p.ProductNom }))
                : null;

            var rapport = new Rapport
            {
                Id               = _existingRapportId,  // 0=create, >0=update
                VisiteId         = LinkedVisiteId,
                Contenu          = Contenu,
                Resultat         = !string.IsNullOrWhiteSpace(SelectedResultat) ? SelectedResultat : "POSITIF",
                DateSoumission   = DateTime.Now,
                IdDelegue        = uid,
                Latitude         = lat,
                Longitude        = lon,
                ProduitsDiscutes = produitsJson
            };

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                await _visiteService.CreateRapportAsync(rapport);

                // Notify VisitDetailViewModel that this visite now has a rapport.
                // (IsCompleted stays false — it's set by superviseur validation, not here.)
                WeakReferenceMessenger.Default.Send(new VisiteCompletedMessage(LinkedVisiteId));

                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await _localDb.InsertPendingRapportAsync(new PendingRapportEntry
                {
                    VisiteId            = rapport.VisiteId,
                    Contenu             = rapport.Contenu,
                    Resultat            = rapport.Resultat,
                    ProduitsDiscutes    = produitsJson,
                    DateSoumissionTicks = rapport.DateSoumission.Ticks,
                    Latitude            = lat,
                    Longitude           = lon,
                    IsSynced            = false
                });
                await Shell.Current.DisplayAlert(
                    "Enregistré hors ligne",
                    "Le rapport a été sauvegardé localement et sera synchronisé à la prochaine connexion.",
                    "OK");
                await Shell.Current.GoToAsync("..");
            }
        });
    }
}
