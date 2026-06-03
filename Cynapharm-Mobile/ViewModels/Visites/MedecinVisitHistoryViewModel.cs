using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Field;
using Cynapharm_Mobile.Models.Inventory;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Visites;

/// <summary>
/// Read-only history of the visits a MEDECIN has received from delegates.
/// The backend (by-medecin/{id}) only returns visits whose Id_Medecin matches
/// the authenticated doctor, so a médecin can only ever see their own history.
/// </summary>
public partial class MedecinVisitHistoryViewModel : BaseViewModel
{
    private readonly VisiteService    _visiteService;
    private readonly UserService      _userService;
    private readonly InventoryService _inventoryService;
    private readonly ProductService   _productService;

    private CancellationTokenSource? _filterCts;
    private List<Visite> _allVisites = new();

    public ObservableCollection<Visite> Visites { get; } = new();

    [ObservableProperty] private DateTime _filterStartDate = DateTime.Today.AddDays(-90);
    [ObservableProperty] private DateTime _filterEndDate   = DateTime.Today;
    [ObservableProperty] private string   _searchQuery     = string.Empty;
    [ObservableProperty] private int      _visitCount;

    public MedecinVisitHistoryViewModel(
        VisiteService    visiteService,
        UserService      userService,
        InventoryService inventoryService,
        ProductService   productService)
    {
        _visiteService    = visiteService;
        _userService      = userService;
        _inventoryService = inventoryService;
        _productService   = productService;
        Title = "Mes visites";
    }

    partial void OnFilterStartDateChanged(DateTime value) => ScheduleDebouncedLoad();
    partial void OnFilterEndDateChanged(DateTime value)   => ScheduleDebouncedLoad();
    partial void OnSearchQueryChanged(string value)       => ApplySearch();

    private void ScheduleDebouncedLoad()
    {
        _filterCts?.Cancel();
        _filterCts = new CancellationTokenSource();
        var token = _filterCts.Token;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(400, token);
                await MainThread.InvokeOnMainThreadAsync(() => _ = LoadVisitesAsync());
            }
            catch (OperationCanceledException) { }
        }, token);
    }

    private void ApplySearch()
    {
        Visites.Clear();
        IEnumerable<Visite> filtered = _allVisites;
        if (!string.IsNullOrWhiteSpace(SearchQuery))
            filtered = filtered.Where(v =>
                v.DelegueNom.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)
                || v.DateVisite.ToString("dd/MM/yyyy").Contains(SearchQuery));
        foreach (var v in filtered) Visites.Add(v);
        VisitCount = Visites.Count;
    }

    [RelayCommand]
    private Task LoadVisitesAsync() => ExecuteAsync(async () =>
    {
        if (!await CheckConnectivityAsync()) return;

        var result = await _visiteService.GetVisitesForMedecinAsync(FilterStartDate, FilterEndDate, null);
        _allVisites = result ?? new List<Visite>();

        if (_allVisites.Count > 0)
        {
            // 1) Products discussed — parse the rapport JSON ([{ id, nom }]) into names.
            foreach (var v in _allVisites)
                v.ProduitsDiscutesNoms = ParseProduitsDiscutes(v.ProduitsDiscutes);

            // 2) Delegate display names. Resolve each delegate by id first (works for any
            //    role, incl. ADMIN/SUPERVISEUR acting as delegate); fall back to the DELEGUE
            //    role list for any id still unresolved. Each call is guarded so one failure
            //    (e.g. a 403) never aborts the whole resolution.
            try
            {
                var delegueIds = _allVisites.Select(v => v.IdDelegue).Where(id => id > 0).Distinct().ToList();
                var names = new Dictionary<int, string>();

                foreach (var id in delegueIds)
                {
                    try
                    {
                        var u = await _userService.GetUserByIdAsync(id);
                        if (u != null && !string.IsNullOrWhiteSpace(u.Name))
                            names[id] = u.Name;
                    }
                    catch { /* try the role-list fallback below */ }
                }

                if (names.Count < delegueIds.Count)
                {
                    try
                    {
                        var delegues = await _userService.GetUsersByRoleAsync("DELEGUE") ?? new();
                        foreach (var d in delegues)
                            if (d.Id > 0 && !names.ContainsKey(d.Id) && !string.IsNullOrWhiteSpace(d.Name))
                                names[d.Id] = d.Name;
                    }
                    catch { /* names stay empty — non-critical */ }
                }

                foreach (var v in _allVisites)
                    v.DelegueNom = names.TryGetValue(v.IdDelegue, out var n) ? n : string.Empty;
            }
            catch { /* names stay empty — non-critical */ }

            // 3) Samples received — load the médecin's distributions, resolve product
            //    names, then attach each sample to its visit (same delegate + same day).
            try
            {
                var echantillons = await _inventoryService.GetEchantillonsByMedecinAsync() ?? new();
                if (echantillons.Count > 0)
                {
                    var productNames = new Dictionary<int, string>();
                    foreach (var id in echantillons.Select(e => e.IdProduit).Where(id => id > 0).Distinct())
                    {
                        var product = await _productService.GetProductByIdAsync(id);
                        if (product != null) productNames[id] = product.Nom;
                    }
                    foreach (var e in echantillons)
                        e.ProduitNom = productNames.TryGetValue(e.IdProduit, out var n) ? n : string.Empty;

                    foreach (var v in _allVisites)
                        v.EchantillonsRecus = echantillons
                            .Where(e => e.IdDelegue == v.IdDelegue
                                     && e.DateDistribution.Date == v.DateVisite.Date)
                            .ToList();
                }
            }
            catch { /* samples stay empty — non-critical */ }
        }

        ApplySearch();
    });

    /// <summary>Parses the rapport "produitsDiscutes" JSON ([{ "id", "nom" }]) into product names.</summary>
    private static List<string> ParseProduitsDiscutes(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try
        {
            var elements = JsonSerializer.Deserialize<List<JsonElement>>(json);
            if (elements == null) return new();
            return elements
                .Where(e => e.ValueKind == JsonValueKind.Object && e.TryGetProperty("nom", out _))
                .Select(e => e.GetProperty("nom").GetString() ?? string.Empty)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
        }
        catch { return new(); }
    }

    [RelayCommand]
    private Task RefreshAsync()
    {
        _filterCts?.Cancel();
        return LoadVisitesAsync();
    }

    protected override Task RetryAsync() => LoadVisitesAsync();
}
