using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Field;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Visites;

public partial class VisitListViewModel : BaseViewModel
{
    private readonly VisiteService _visiteService;

    // Debounce: cancels the pending API call if the filter changes again within 400 ms
    private CancellationTokenSource? _filterCts;

    public ObservableCollection<Visite> Visites { get; } = new();

    [ObservableProperty] private DateTime _filterStartDate = DateTime.Today.AddDays(-30);
    [ObservableProperty] private DateTime _filterEndDate   = DateTime.Today;
    [ObservableProperty] private string   _filterStatus    = "Tous";

    public List<string> StatusOptions { get; } = new() { "Tous", "PLANIFIEE", "REALISEE", "ANNULEE" };

    partial void OnFilterStartDateChanged(DateTime value) => ScheduleDebouncedLoad();
    partial void OnFilterEndDateChanged(DateTime value)   => ScheduleDebouncedLoad();
    partial void OnFilterStatusChanged(string value)      => ScheduleDebouncedLoad();

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

    public VisitListViewModel(VisiteService visiteService)
    {
        _visiteService = visiteService;
        Title = "Visites";
    }

    [RelayCommand]
    private Task LoadVisitesAsync() => ExecuteAsync(async () =>
    {
        if (!await CheckConnectivityAsync()) return;
        Visites.Clear();
        var status = FilterStatus is "" or "Tous" ? null : FilterStatus;
        var result = await _visiteService.GetVisitesAsync(FilterStartDate, FilterEndDate, status);
        if (result != null) foreach (var v in result) Visites.Add(v);
    });

    [RelayCommand]
    private Task RefreshAsync()
    {
        _filterCts?.Cancel();
        return LoadVisitesAsync();
    }

    protected override Task RetryAsync() => LoadVisitesAsync();

    [RelayCommand]
    private async Task GoToDetailAsync(Visite? visite)
    {
        if (visite == null) return;
        await Shell.Current.GoToAsync($"//visits/detail?visiteId={visite.Id}");
    }

    [RelayCommand]
    private async Task CreateVisitAsync()
        => await Shell.Current.GoToAsync("//visits/detail");

    [RelayCommand]
    private void SetStatusFilter(string status) => FilterStatus = status;
}
