using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Field;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;
using RegionModel = Cynapharm_Mobile.Models.Field.Region;

namespace Cynapharm_Mobile.ViewModels.Dashboard;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly VisiteService _visiteService;
    private readonly KpiService _kpiService;

    [ObservableProperty] private string _userDisplayName = string.Empty;
    [ObservableProperty] private string _userRole = string.Empty;
    [ObservableProperty] private int _todayVisitCount;
    [ObservableProperty] private bool _isSuperviseur;

    public ObservableCollection<Kpi> KpiItems { get; } = new();
    public ObservableCollection<Objectif> ObjectifItems { get; } = new();
    public ObservableCollection<Cynapharm_Mobile.Models.Field.Region> Regions { get; } = new();

    public DashboardViewModel(VisiteService visiteService, KpiService kpiService)
    {
        _visiteService = visiteService;
        _kpiService = kpiService;
        Title = "Tableau de bord";
    }

    private async Task InitializeAsync()
    {
        var name = await SecureStorage.GetAsync(StorageKeys.UserName);
        var role = await SecureStorage.GetAsync(StorageKeys.UserRole);
        UserDisplayName = name ?? "Utilisateur";
        UserRole = role ?? string.Empty;
        IsSuperviseur = UserRole == "SUPERVISEUR";
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
            var today = DateTime.Today;

            var kpis = await _kpiService.GetKpisAsync();
            KpiItems.Clear();
            if (kpis != null)
            {
                foreach (var k in kpis) KpiItems.Add(k);
                await SaveCacheAsync("dashboard_kpis", kpis);
            }

            var objectifs = await _kpiService.GetObjectifsAsync();
            ObjectifItems.Clear();
            if (objectifs != null)
            {
                foreach (var o in objectifs) ObjectifItems.Add(o);
                await SaveCacheAsync("dashboard_objectifs", objectifs);
            }

            if (IsSuperviseur)
            {
                var regions = await _kpiService.GetRegionsAsync();
                Regions.Clear();
                if (regions != null) foreach (var r in regions) Regions.Add(r);
            }
            else
            {
                var visites = await _visiteService.GetVisitesAsync(today, today, null);
                TodayVisitCount = visites?.Count ?? 0;
            }

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
        var kpis = await LoadCacheAsync<List<Kpi>>("dashboard_kpis");
        KpiItems.Clear();
        if (kpis != null) foreach (var k in kpis) KpiItems.Add(k);

        var objectifs = await LoadCacheAsync<List<Objectif>>("dashboard_objectifs");
        ObjectifItems.Clear();
        if (objectifs != null) foreach (var o in objectifs) ObjectifItems.Add(o);

        if (KpiItems.Count > 0 || ObjectifItems.Count > 0)
        {
            IsOffline = true;
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
