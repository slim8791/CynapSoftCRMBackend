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

    [ObservableProperty] private string          _userDisplayName = string.Empty;
    [ObservableProperty] private string          _userRole        = string.Empty;
    [ObservableProperty] private int             _todayVisitCount;
    [ObservableProperty] private bool            _isSuperviseur;
    [ObservableProperty] private bool            _isDelegue;
    [ObservableProperty] private double          _tauxConversion;
    [ObservableProperty] private StockSummaryDto? _stockSummary;

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
        var name = await SecureStorage.GetAsync(StorageKeys.UserName);
        var role = await SecureStorage.GetAsync(StorageKeys.UserRole);
        UserDisplayName = name ?? "Utilisateur";
        UserRole        = role ?? string.Empty;
        IsSuperviseur   = UserRole == "SUPERVISEUR";
        IsDelegue       = UserRole == "DELEGUE";
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

            // Start performance + objectifs in parallel regardless of role
            // (BUG-03 removed the DELEGUE-only guard from KpiService)
            var perfTask = _kpiService.GetPerformanceAsync(monthStart, today);
            var objTask  = _kpiService.GetObjectifsAsync();

            if (IsSuperviseur)
            {
                // Load regions in parallel with perf + objectifs
                var regionTask = _kpiService.GetRegionsAsync();
                await Task.WhenAll(perfTask, objTask, regionTask);

                Regions.Clear();
                if (regionTask.Result != null)
                    foreach (var r in regionTask.Result) Regions.Add(r);
            }
            else
            {
                await Task.WhenAll(perfTask, objTask);

                var visites = await _visiteService.GetVisitesAsync(today, today, null);
                TodayVisitCount = visites?.Count ?? 0;

                if (IsDelegue)
                {
                    var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
                    if (int.TryParse(userIdStr, out var userId))
                    {
                        var debut = new DateTime(today.Year, today.Month, 1);
                        var taux  = await _kpiService.GetTauxConversionAsync(userId, debut, today);
                        TauxConversion = taux ?? 0;
                        StockSummary = await _inventoryService.GetStockSummaryAsync(userId);
                    }
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

            var objectifs = objTask.Result;
            ObjectifItems.Clear();
            if (objectifs != null)
                foreach (var o in objectifs) ObjectifItems.Add(o);

            IsOffline = false;
        }
        catch (Exception)
        {
            await LoadFromCacheAsync();
        }
        finally { SetBusy(false); }
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
