using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Orders;
using Cynapharm_Mobile.Models.Products;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Orders;

[QueryProperty(nameof(PreselectedProductId), "productId")]
public partial class CreateOrderViewModel : BaseViewModel
{
    private readonly OrderService _orderService;
    private readonly ProductService _productService;
    private readonly LocalDatabaseService _localDb;

    // Cart key is scoped per user so different accounts on the same device don't share drafts.
    // Set to the user-specific value in InitializeAsync once userId is available.
    private string _cartCacheKey = "draft_cart";

    [ObservableProperty] private int _preselectedProductId;
    [ObservableProperty] private int _currentStep = 1;
    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private Product? _selectedProduct;

    // ── Validated quantity field ──────────────────────────────────────────────
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(1, 9999, ErrorMessage = "La quantité doit être comprise entre 1 et 9 999.")]
    [NotifyPropertyChangedFor(nameof(QuantityError))]
    private int _quantity = 1;

    public string QuantityError =>
        GetErrors(nameof(Quantity))
            .Cast<ValidationResult>()
            .FirstOrDefault()?.ErrorMessage ?? string.Empty;


    public ObservableCollection<CartLine> CartLines { get; } = new();
    public ObservableCollection<Product> SearchResults { get; } = new();

    public decimal CartTotal      => CartLines.Sum(l => l.SousTotal);
    public decimal CartSavings    => CartLines.Sum(l => l.EconomieTotale);
    public bool    HasCartSavings => CartSavings > 0;

    public bool IsStep1    => CurrentStep == 1;
    public bool IsStep2    => CurrentStep == 2;
    public bool IsStep3    => CurrentStep == 3;
    public bool IsNotStep1 => CurrentStep > 1;
    public bool IsNotStep3 => CurrentStep < 3;

    public CreateOrderViewModel(
        OrderService orderService,
        ProductService productService,
        LocalDatabaseService localDb)
    {
        _orderService  = orderService;
        _productService = productService;
        _localDb       = localDb;
        Title = "Nouvelle commande";
        _ = InitializeAsync();
    }

    partial void OnPreselectedProductIdChanged(int value)
    {
        if (value > 0) _ = PreloadProductAsync(value);
    }

    partial void OnCurrentStepChanged(int value)
    {
        OnPropertyChanged(nameof(IsStep1));
        OnPropertyChanged(nameof(IsStep2));
        OnPropertyChanged(nameof(IsStep3));
        OnPropertyChanged(nameof(IsNotStep1));
        OnPropertyChanged(nameof(IsNotStep3));
    }

    private async Task PreloadProductAsync(int productId)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product != null) SelectedProduct = product;
    }

    [RelayCommand]
    private async Task SearchProductAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            SearchResults.Clear();
            return;
        }
        SetBusy(true);
        try
        {
            SearchResults.Clear();

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                var result = await _productService.GetProductsAsync(SearchQuery, 20);
                if (result != null)
                    foreach (var p in result.Where(p => p.Actif && !p.IsArchived))
                        SearchResults.Add(p);
            }
            else
            {
                // Offline: query SQLite cache
                var cached = await _localDb.SearchProductsAsync(SearchQuery);
                foreach (var e in cached)
                    SearchResults.Add(new Product
                    {
                        Id           = e.Id,
                        Reference    = e.Reference,
                        Nom          = e.Nom,
                        Categorie    = e.Categorie,
                        PrixUnitaire = e.PrixUnitaire,
                        ImageUrl     = e.ImageUrl,
                        Actif        = e.Actif
                    });
            }

            if (SearchResults.Count == 0)
                ErrorMessage = "Aucun produit trouvé pour cette recherche.";
        }
        catch (Exception) { ErrorMessage = "Erreur de recherche."; }
        finally { SetBusy(false); }
    }

    [RelayCommand]
    private void SelectProduct(Product product)
    {
        SelectedProduct = product;
        SearchResults.Clear();
        SearchQuery = product.Nom;
    }

    // ── Promotion engine ──────────────────────────────────────────────────────

    [RelayCommand]
    private async Task AddLineAsync()
    {
        ClearError();
        ValidateProperty(Quantity, nameof(Quantity));
        if (SelectedProduct == null || HasErrors)
        {
            ErrorMessage = SelectedProduct == null
                ? "Sélectionnez un produit avant d'ajouter."
                : QuantityError;
            return;
        }

        var prixOriginal  = SelectedProduct.PrixUnitaire;
        var prixEffectif  = prixOriginal;
        decimal remise    = 0;
        string? promoTitre = null;

        // Query local SQLite promotion cache (works offline too)
        try
        {
            var promo = await _localDb.GetActivePromotionAsync(SelectedProduct.Id);
            if (promo != null && promo.RemisePourcentage > 0)
            {
                remise       = (decimal)promo.RemisePourcentage;
                prixEffectif = prixOriginal * (1m - remise / 100m);
                promoTitre   = promo.Titre;
            }
        }
        catch { /* promo lookup failure is non-critical */ }

        var existing = CartLines.FirstOrDefault(l => l.ProductId == SelectedProduct.Id);
        if (existing != null)
        {
            existing.Quantite += Quantity;
        }
        else
        {
            CartLines.Add(new CartLine
            {
                ProductId         = SelectedProduct.Id,
                ProductNom        = SelectedProduct.Nom,
                Quantite          = Quantity,
                PrixOriginal      = prixOriginal,
                PrixUnitaire      = prixEffectif,
                RemisePourcentage = remise,
                PromoTitre        = promoTitre
            });
        }

        OnPropertyChanged(nameof(CartTotal));
        OnPropertyChanged(nameof(CartSavings));
        OnPropertyChanged(nameof(HasCartSavings));

        SelectedProduct = null;
        SearchQuery     = string.Empty;
        Quantity        = 1;
        _ = SaveCartAsync();
    }

    [RelayCommand]
    private void RemoveLine(CartLine? line)
    {
        if (line == null) return;
        CartLines.Remove(line);
        OnPropertyChanged(nameof(CartTotal));
        OnPropertyChanged(nameof(CartSavings));
        OnPropertyChanged(nameof(HasCartSavings));
    }

    [RelayCommand]
    private void NextStep()
    {
        if (CurrentStep == 1 && CartLines.Count == 0) { ErrorMessage = "Ajoutez au moins un produit."; return; }
        if (CurrentStep < 3) { ClearError(); CurrentStep++; }
    }

    [RelayCommand]
    private void PreviousStep() { if (CurrentStep > 1) CurrentStep--; }

    [RelayCommand]
    private async Task SubmitOrderAsync()
    {
        ClearError();

        // Client-side validation matching backend rules
        foreach (var line in CartLines)
        {
            if (line.Quantite < 1 || line.Quantite > 9999)
            {
                ErrorMessage = $"La quantité de « {line.ProductNom} » doit être entre 1 et 9 999.";
                return;
            }
            if (line.RemisePourcentage < 0 || line.RemisePourcentage > 100)
            {
                ErrorMessage = $"La remise de « {line.ProductNom} » doit être entre 0 et 100 %.";
                return;
            }
            if (line.PrixUnitaire <= 0)
            {
                ErrorMessage = $"Le prix unitaire de « {line.ProductNom} » doit être supérieur à 0.";
                return;
            }
        }

        if (!await CheckConnectivityAsync()) return;
        SetBusy(true);
        try
        {
            var payload = new
            {
                Lignes = CartLines.Select(l => new
                {
                    Id_Produit   = l.ProductId,
                    Quantite     = l.Quantite,
                    PrixUnitaire = l.PrixUnitaire,
                    Remise       = l.RemisePourcentage
                }).ToList(),
                IsFinalValidation = true
            };
            await _orderService.CreateOrderAsync(payload);
            ClearCartCache();
            await Shell.Current.DisplayAlert(
                "Succès",
                "Votre commande a été soumise et est en attente de confirmation.",
                "OK");
            await Shell.Current.GoToAsync("//orders");
        }
        catch (Exception) { ErrorMessage = "Erreur lors de la soumission de la commande."; }
        finally { SetBusy(false); }
    }

    private async Task InitializeAsync()
    {
        // MÉDECIN cannot create orders
        var role = await SecureStorage.GetAsync(StorageKeys.UserRole);
        if (role == "MEDECIN")
        {
            await Shell.Current.DisplayAlert(
                "Accès refusé",
                "Vous ne pouvez pas créer de commande.",
                "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        // Scope the cart to the current user before loading the draft
        var userId = await SecureStorage.GetAsync(StorageKeys.UserId) ?? "anonymous";
        _cartCacheKey = $"draft_cart_{userId}";

        await LoadCartAsync();

        // Seed promotions into SQLite for offline promo calculation
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            try
            {
                var promos = await _productService.GetPromotionsAsync(null);
                if (promos != null && promos.Count > 0)
                    await _localDb.SeedPromotionsAsync(promos);
            }
            catch { /* non-critical */ }
        }
    }

    private Task SaveCartAsync()
    {
        try
        {
            Preferences.Set(_cartCacheKey, System.Text.Json.JsonSerializer.Serialize(CartLines.ToList()));
        }
        catch { }
        return Task.CompletedTask;
    }

    private Task LoadCartAsync()
    {
        try
        {
            var json = Preferences.Get(_cartCacheKey, string.Empty);
            if (!string.IsNullOrEmpty(json))
            {
                var items = System.Text.Json.JsonSerializer.Deserialize<List<CartLine>>(json);
                if (items != null)
                {
                    CartLines.Clear();
                    foreach (var item in items) CartLines.Add(item);
                    OnPropertyChanged(nameof(CartTotal));
                    OnPropertyChanged(nameof(CartSavings));
                    OnPropertyChanged(nameof(HasCartSavings));
                }
            }
        }
        catch { }
        return Task.CompletedTask;
    }

    private void ClearCartCache() => Preferences.Remove(_cartCacheKey);
}
