    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Cynapharm_Mobile.Models.Products;
    using Cynapharm_Mobile.Services;
    using Cynapharm_Mobile.ViewModels.Base;

    namespace Cynapharm_Mobile.ViewModels.Products;

    public partial class ProductListViewModel : BaseViewModel
    {
        private readonly ProductService     _productService;
        private readonly LocalDatabaseService _localDb;

        // Full unfiltered catalogue — kept in memory for instant local search
        private List<Product> _allProducts = new();

        // Debounce handle: cancels the pending search when the user keeps typing
        private CancellationTokenSource? _searchCts;

        public ObservableCollection<Product>  Products   { get; } = new();
        public ObservableCollection<string>   Categories { get; } = new();

        [ObservableProperty] private bool _canSeePrices = true;
        private bool _useVisibleEndpoint;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowSearchHint))]
        private string _searchQuery = string.Empty;

        // True when the user has typed 1-2 chars (not enough to trigger search yet)
        public bool ShowSearchHint => SearchQuery.Length is > 0 and < 3;
        [ObservableProperty] private string  _selectedCategory  = "Tous";
        [ObservableProperty] private int     _productCount;
        [ObservableProperty] private bool    _isSearching;

        public ProductListViewModel(ProductService productService, LocalDatabaseService localDb)
        {
            _productService = productService;
            _localDb        = localDb;
            Title = "Catalogue";

            Products.CollectionChanged += (_, _) => ProductCount = Products.Count;
        }

        // ── Auto-search as the user types (300 ms debounce, min 3 chars) ──────────
        partial void OnSearchQueryChanged(string value)
        {
            _searchCts?.Cancel();

            // Allow empty (show all) or 3+ chars; ignore 1-2 char intermediate input
            if (value.Length is > 0 and < 3)
                return;

            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(300, token);
                    await MainThread.InvokeOnMainThreadAsync(() => _ = ApplyFilterAsync());
                }
                catch (OperationCanceledException) { }
            }, token);
        }

        partial void OnSelectedCategoryChanged(string value) => _ = ApplyFilterAsync();

        // ── Commands ──────────────────────────────────────────────────────────────

        [RelayCommand]
        private async Task LoadAsync()
        {
            ClearError();
            var role = await SecureStorage.GetAsync(StorageKeys.UserRole) ?? string.Empty;
            CanSeePrices = role is not "MEDECIN";
            // CLIENT and MEDECIN both use /products/visible (active + non-archived, server-filtered).
            // DELEGUE/ADMIN use /products and filter archived client-side.
            _useVisibleEndpoint = role is "MEDECIN" or "PHARMACIEN" or "GROSSISTE" or "CLIENT";
            await LoadCatalogueAsync();
            await LoadCategoriesAsync();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            IsRefreshing = true;
            try
            {
                _allProducts.Clear();
                await LoadCatalogueAsync();
                await LoadCategoriesAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private void SetSelectedCategory(string? category)
            => SelectedCategory = category ?? "Tous";

        [RelayCommand]
        private void ClearSearch()
        {
            _searchCts?.Cancel();
            SearchQuery      = string.Empty;
            SelectedCategory = "Tous";
            Products.Clear();
            foreach (var p in _allProducts) Products.Add(p);
        }

    [RelayCommand]
    private async Task GoToDetailAsync(Product? product)
    {
        if (product == null) return;

        // Garantit l'exécution sur le thread UI (obligatoire sur Android)
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.GoToAsync($"//products/detail?productId={product.Id}");
        });
    }

    // ── Core logic ────────────────────────────────────────────────────────────

    private async Task LoadCatalogueAsync()
        {
            SetBusy(true);
            try
            {
                if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                {
                    await LoadFromSqliteAsync();
                    return;
                }

                // MEDECIN and CLIENT use /products/visible (active + non-archived, server-filtered).
                // DELEGUE/ADMIN use /products and filter archived client-side.
                List<Product>? result;
                if (_useVisibleEndpoint)
                {
                    result = await _productService.GetVisibleProductsAsync();
                    if (result != null)
                    {
                        _allProducts = result;
                        await _localDb.SeedProductsAsync(_allProducts);
                        IsOffline = false;
                    }
                }
                else
                {
                    result = await _productService.GetProductsAsync();
                    if (result != null)
                    {
                        _allProducts = result.Where(p => !p.IsArchived).ToList();
                        await _localDb.SeedProductsAsync(_allProducts);
                        IsOffline = false;
                    }
                }

                await ApplyFilterAsync(skipBusy: true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductListVM] {ex.Message}");
                if (_allProducts.Count == 0) await LoadFromSqliteAsync();
            }
            finally { SetBusy(false); }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var cats = await _productService.GetCategoriesAsync();
                Categories.Clear();
                Categories.Add("Tous");
                if (cats != null)
                    foreach (var c in cats.Where(c => !string.IsNullOrWhiteSpace(c)))
                        Categories.Add(c);
            }
            catch { /* categories are non-critical */ }
        }

        // Filter the in-memory list by search query and selected category
        private async Task ApplyFilterAsync(bool skipBusy = false)
        {
            if (!skipBusy) IsSearching = true;
            try
            {
                IEnumerable<Product> filtered = _allProducts;

                // Category filter
                if (!string.IsNullOrWhiteSpace(SelectedCategory) && SelectedCategory != "Tous")
                    filtered = filtered.Where(p =>
                        string.Equals(p.Categorie, SelectedCategory, StringComparison.OrdinalIgnoreCase));

                // Search filter (keyword matches name, reference, or description)
                var q = SearchQuery.Trim();
                if (!string.IsNullOrWhiteSpace(q))
                {
                    filtered = filtered.Where(p =>
                        (p.Nom?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)        ||
                        (p.Reference?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)  ||
                        (p.Description?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false));
                }

                var list = filtered.ToList();

                // If online search returns nothing locally, try server-side keyword search
                if (list.Count == 0 && !string.IsNullOrWhiteSpace(q) &&
                    Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    var remote = await _productService.GetProductsAsync(q);
                    if (remote != null) list = remote.Where(p => !p.IsArchived).ToList();
                }

                Products.Clear();
                foreach (var p in list) Products.Add(p);
            }
            finally { IsSearching = false; }
        }

        private async Task LoadFromSqliteAsync()
        {
            try
            {
                var entries = await _localDb.SearchProductsAsync(SearchQuery);
                _allProducts = entries.Select(e => new Product
                {
                    Id           = e.Id,
                    Reference    = e.Reference,
                    Nom          = e.Nom,
                    Description  = e.Description,
                    Categorie    = e.Categorie,
                    PrixUnitaire = e.PrixUnitaire,
                    ImageUrl     = e.ImageUrl,
                    Actif        = e.Actif
                }).ToList();

                Products.Clear();
                foreach (var p in _allProducts) Products.Add(p);

                IsOffline = true;
                ErrorMessage = _allProducts.Count > 0
                    ? "Mode hors ligne — catalogue du dernier chargement."
                    : "Aucune connexion et aucune donnée en cache.";
            }
            catch { ErrorMessage = "Impossible de charger le catalogue."; }
        }
    }
