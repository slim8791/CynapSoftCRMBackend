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
    private readonly UserService   _userService;

    // Debounce: cancels the pending API call if the filter changes again within 400 ms
    private CancellationTokenSource? _filterCts;
    private List<Visite> _allVisites = new();

    public ObservableCollection<Visite> Visites { get; } = new();

    [ObservableProperty] private DateTime _filterStartDate = DateTime.Today.AddDays(-30);
    [ObservableProperty] private DateTime _filterEndDate   = DateTime.Today.AddDays(15);
    [ObservableProperty] private string   _filterStatus    = "Toutes";
    [ObservableProperty] private string   _searchQuery     = string.Empty;

    public List<string> StatusOptions { get; } = new() { "Toutes", "En attente", "À corriger", "Validées" };

    partial void OnFilterStartDateChanged(DateTime value) => ScheduleDebouncedLoad();
    partial void OnFilterEndDateChanged(DateTime value)   => ScheduleDebouncedLoad();
    partial void OnFilterStatusChanged(string value)      => ScheduleDebouncedLoad();
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

    private IEnumerable<Visite> ApplyFilter(IEnumerable<Visite> visites) =>
        FilterStatus switch
        {
            "En attente" => visites.Where(v => v.Statut == "En attente"),
            "À corriger" => visites.Where(v => v.Statut == "Rejetée"),
            "Validées"   => visites.Where(v => v.Statut == "Validée"),
            _            => visites // "Toutes" ou cas par défaut
        };

    private void ApplySearch()
    {
        Visites.Clear();
        var filtered = ApplyFilter(_allVisites);
        if (!string.IsNullOrWhiteSpace(SearchQuery))
            // ISSUE #13 fix: search on type, contact name AND date
            filtered = filtered.Where(v =>
                v.TypeLabel.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)
                || (v.ContactName).Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)
                || v.DateVisite.ToString("dd/MM/yyyy").Contains(SearchQuery));
        foreach (var v in filtered) Visites.Add(v);
    }

    public VisitListViewModel(VisiteService visiteService, UserService userService)
    {
        _visiteService = visiteService;
        _userService   = userService;
        Title = "Visites";
    }

    [RelayCommand]
    private Task LoadVisitesAsync() => ExecuteAsync(async () =>
    {
        if (!await CheckConnectivityAsync()) return;
        // Always fetch all statuses from the API; IsCompleted filtering is applied
        // locally in ApplyFilter so the full list is available for instant switching.
        var result = await _visiteService.GetVisitesAsync(FilterStartDate, FilterEndDate, null);
        _allVisites = result ?? new List<Visite>();

        // Resolve display names for medecins and pharmaciens
        if (_allVisites.Count > 0)
        {
            try
            {
                var medecinTask    = _userService.GetUsersByRoleAsync("MEDECIN");
                var pharmacienTask = _userService.GetUsersByRoleAsync("CLIENT"); // CLIENT role covers both PHARMACIEN and GROSSISTE subtypes
                await Task.WhenAll(medecinTask, pharmacienTask);

                var medecins    = medecinTask.Result    ?? new();
                var pharmaciens = pharmacienTask.Result ?? new();

                foreach (var v in _allVisites)
                {
                    if (v.IdMedecin.HasValue)
                        v.MedecinNom = medecins
                            .FirstOrDefault(m => m.Id == v.IdMedecin.Value)?.Name ?? string.Empty;
                    if (v.IdPharmacien.HasValue)
                        v.PharmacienNom = pharmaciens
                            .FirstOrDefault(p => p.Id == v.IdPharmacien.Value)?.Name ?? string.Empty;
                }
            }
            catch { /* names stay empty — non-critical */ }
        }

        ApplySearch();
    });

    [RelayCommand]
    private void Search() => ApplySearch();

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
        await Shell.Current.GoToAsync($"///visits/detail?visiteId={visite.Id}");
    }

    [RelayCommand]
    private async Task CreateVisitAsync()
        => await Shell.Current.GoToAsync("///visits/detail");

    [RelayCommand]
    private void SetStatusFilter(string status) => FilterStatus = status;
}
