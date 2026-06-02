using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Field;
using Cynapharm_Mobile.Models.Inventory;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;
using RegionModel = Cynapharm_Mobile.Models.Field.Region;

namespace Cynapharm_Mobile.ViewModels.Dashboard;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly VisiteService    _visiteService;
    private readonly KpiService       _kpiService;
    private readonly InventoryService _inventoryService;

    [ObservableProperty] private string          _userDisplayName  = string.Empty;
    [ObservableProperty] private string          _userRole         = string.Empty;
    [ObservableProperty] private string          _greetingInitials = "?";
    [ObservableProperty] private int             _todayVisitCount;
    [ObservableProperty] private bool            _isSuperviseur;
    [ObservableProperty] private bool            _isDelegue;
    [ObservableProperty] private double          _tauxConversion;
    [ObservableProperty] private StockSummaryDto? _stockSummary;

    // ── Section-level loading states for activity indicators ──────────────────
    [ObservableProperty] private bool            _isLoadingKpis;
    [ObservableProperty] private bool            _isLoadingObjectifs;
    [ObservableProperty] private bool            _isLoadingStock;
    [ObservableProperty] private bool            _isLoadingRegions;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStockFaible), nameof(StockFaibleLabel))]
    private int _stocksFaibles = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasRuptureStock), nameof(RuptureStockLabel))]
    private int _stocksVides = 0;

    public bool   HasStockFaible  => StocksFaibles > 0;
    public bool   HasRuptureStock => StocksVides   > 0;
    public string StockFaibleLabel  => $"⚠️ {StocksFaibles} produit(s) en stock faible";
    public string RuptureStockLabel => $"⚠️ {StocksVides} produit(s) en rupture de stock";

    public ObservableCollection<Objectif>         ObjectifItems { get; } = new();
    public ObservableCollection<PerformanceDto>   PerformanceItems { get; } = new();
    public ObservableCollection<RegionModel>      Regions       { get; } = new();

    public DashboardViewModel(
        VisiteService    visiteService,
        KpiService       kpiService,
        InventoryService inventoryService)
    {
        _visiteService     = visiteService;
        _kpiService        = kpiService;
        _inventoryService  = inventoryService;
        Title = "Tableau de bord";
    }

    private async Task InitializeAsync()
    {
        var name = await SecureStorage.GetAsync(StorageKeys.UserName) ?? string.Empty;
        var role = await SecureStorage.GetAsync(StorageKeys.UserRole);

        // First word of name as the greeting ("Bonjour, Ahmed !")
        UserDisplayName = name.Split(' ').FirstOrDefault() ?? (name.Length > 0 ? name : "Utilisateur");

        // Two-letter initials (or one if single-word name, or "?" if empty)
        var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        GreetingInitials = parts.Length >= 2
            ? $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant()
            : parts.Length == 1
                ? parts[0][0].ToString().ToUpperInvariant()
                : "?";

        UserRole      = role ?? string.Empty;
        IsSuperviseur = UserRole == "SUPERVISEUR";
        IsDelegue     = UserRole == "DELEGUE";
    }

    [RelayCommand]
    private async Task LoadDashboardAsync()
    {
        ClearError();
        SetBusy(true);
        await InitializeAsync();
        if (!await CheckConnectivityAsync())
        {
            await LoadFromCacheAsync();
            SetBusy(false);
            return;
        }

        try
        {
            var today      = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            // Start performance + objectifs in parallel with individual section loaders
            IsLoadingKpis = true;
            IsLoadingObjectifs = true;

            var perfTask = _kpiService.GetPerformanceAsync(monthStart, today);
            var objTask  = _kpiService.GetObjectifsAsync();

            if (IsSuperviseur)
            {
                // Load regions in parallel with perf + objectifs
                IsLoadingRegions = true;
                var regionTask = _kpiService.GetRegionsAsync();
                await Task.WhenAll(perfTask, objTask, regionTask);

                Regions.Clear();
                if (regionTask.Result != null)
                    foreach (var r in regionTask.Result) Regions.Add(r);

                IsLoadingRegions = false;
            }
            else
            {
                await Task.WhenAll(perfTask, objTask);

                var visites = await _visiteService.GetVisitesAsync(today, today, null);
                TodayVisitCount = visites?.Count ?? 0;

                if (IsDelegue)
                {
                    IsLoadingStock = true;
                    var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
                    if (int.TryParse(userIdStr, out var userId))
                    {
                        var debut = new DateTime(today.Year, today.Month, 1);
                        var taux  = await _kpiService.GetTauxConversionAsync(userId, debut, today);
                        TauxConversion = taux ?? 0;
                        StockSummary = await _inventoryService.GetStockSummaryAsync(userId);
                        if (StockSummary != null)
                        {
                            StocksFaibles = StockSummary.StocksFaibles;
                            StocksVides   = StockSummary.StocksVides;
                        }
                    }
                    IsLoadingStock = false;
                }
            }

            // Populate collections from completed tasks
            var kpis = perfTask.Result;
            PerformanceItems.Clear();
            if (kpis != null)
            {
                foreach (var k in kpis) PerformanceItems.Add(k);
                await SaveCacheAsync("dashboard_kpis", kpis);
            }
            IsLoadingKpis = false;

            var objectifs = objTask.Result;
            ObjectifItems.Clear();
            if (objectifs != null)
                foreach (var o in objectifs) ObjectifItems.Add(o);
            IsLoadingObjectifs = false;

            IsOffline = false;
        }
        catch (Exception)
        {
            await LoadFromCacheAsync();
        }
        finally 
        { 
            SetBusy(false);
            IsLoadingKpis = false;
            IsLoadingObjectifs = false;
            IsLoadingStock = false;
            IsLoadingRegions = false;
        }
    }

    private async Task LoadFromCacheAsync()
    {
        var kpis = await LoadCacheAsync<List<PerformanceDto>>("dashboard_kpis");
        PerformanceItems.Clear();
        if (kpis != null) foreach (var k in kpis) PerformanceItems.Add(k);

        if (PerformanceItems.Count > 0)
        {
            IsOffline    = true;
            ErrorMessage = "Mode hors ligne — données du dernier chargement.";
        }
        else
        {
            ErrorMessage = "Pas de connexion et aucune donnée en cache.";
        }
    }

    [RelayCommand]
    private async Task GoToVisitsAsync() => await Shell.Current.GoToAsync("//visits");

    [RelayCommand]
    private async Task GoToPlanningAsync() => await Shell.Current.GoToAsync("//planning");

    [RelayCommand]
    private async Task GoToObjectifsAsync() => await Shell.Current.GoToAsync("//objectifs");
}
