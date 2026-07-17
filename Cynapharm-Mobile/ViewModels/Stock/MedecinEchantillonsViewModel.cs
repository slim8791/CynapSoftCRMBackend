using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Inventory;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Stock;

/// <summary>
/// Displays all sample distributions (échantillons) received by the logged-in MEDECIN.
/// Uses the existing <see cref="InventoryService.GetEchantillonsByMedecinAsync"/> endpoint.
/// Product names are resolved via <see cref="ProductService.GetProductByIdAsync"/>.
/// </summary>
public partial class MedecinEchantillonsViewModel : BaseViewModel
{
    private readonly InventoryService _inventoryService;
    private readonly ProductService   _productService;

    private List<EchantillonRecu> _allEchantillons = new();

    public ObservableCollection<EchantillonRecu> Echantillons { get; } = new();

    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private int    _totalQuantity;
    [ObservableProperty] private int    _itemCount;

    public MedecinEchantillonsViewModel(
        InventoryService inventoryService,
        ProductService   productService)
    {
        _inventoryService = inventoryService;
        _productService   = productService;
        Title = "Mes échantillons";
    }

    partial void OnSearchQueryChanged(string value) => ApplyFilter();

    [RelayCommand]
    private Task LoadDataAsync() => ExecuteAsync(async () =>
    {
        if (!await CheckConnectivityAsync()) return;

        var result = await _inventoryService.GetEchantillonsByMedecinAsync();
        _allEchantillons = result ?? new List<EchantillonRecu>();

        // Resolve product names
        if (_allEchantillons.Count > 0)
        {
            var productNames = new Dictionary<int, string>();
            var productIds = _allEchantillons
                .Select(e => e.IdProduit)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            foreach (var id in productIds)
            {
                try
                {
                    var product = await _productService.GetProductByIdAsync(id);
                    if (product != null)
                        productNames[id] = product.Nom;
                }
                catch { /* non-critical — keep fallback label */ }
            }

            foreach (var e in _allEchantillons)
                e.ProduitNom = productNames.TryGetValue(e.IdProduit, out var n) ? n : string.Empty;
        }

        ApplyFilter();
    });

    private void ApplyFilter()
    {
        Echantillons.Clear();

        IEnumerable<EchantillonRecu> filtered = _allEchantillons;

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            filtered = filtered.Where(e =>
                e.ProduitLabel.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                e.NumeroLot.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                e.DateDistribution.ToString("dd/MM/yyyy").Contains(SearchQuery));
        }

        var list = filtered.OrderByDescending(e => e.DateDistribution).ToList();
        foreach (var item in list)
            Echantillons.Add(item);

        ItemCount     = Echantillons.Count;
        TotalQuantity = Echantillons.Sum(e => e.Quantite);
    }

    [RelayCommand]
    private Task RefreshAsync() => LoadDataAsync();

    protected override Task RetryAsync() => LoadDataAsync();
}
