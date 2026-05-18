using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Field;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;
using PlanningModel = Cynapharm_Mobile.Models.Field.Planning;

namespace Cynapharm_Mobile.ViewModels.Planning;

public partial class PlanningViewModel : BaseViewModel
{
    private readonly PlanningService _planningService;
    private CancellationTokenSource? _loadCts;

    [ObservableProperty] private DateTime _weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);

    public string WeekLabel => $"{WeekStart:dd MMM} – {WeekStart.AddDays(6):dd MMM yyyy}";
    public ObservableCollection<PlanningDayGroup> WeekDays { get; } = new();

    public PlanningViewModel(PlanningService planningService)
    {
        _planningService = planningService;
        Title = "Planning";
    }

    partial void OnWeekStartChanged(DateTime value)
    {
        OnPropertyChanged(nameof(WeekLabel));
        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();
        _ = LoadWeekAsync(_loadCts.Token);
    }

    [RelayCommand]
    private void PreviousWeek()
    {
        var candidate = WeekStart.AddDays(-7);
        if (candidate >= DateTime.Today.AddYears(-1))
            WeekStart = candidate;
    }

    [RelayCommand]
    private void NextWeek() => WeekStart = WeekStart.AddDays(7);

    [RelayCommand]
    private async Task LoadWeekAsync(CancellationToken ct = default)
    {
        SetBusy(true);
        WeekDays.Clear();
        if (!await CheckConnectivityAsync()) { SetBusy(false); return; }
        try
        {
            ct.ThrowIfCancellationRequested();
            var entries = await _planningService.GetPlanningAsync(WeekStart) ?? new List<PlanningModel>();
            ct.ThrowIfCancellationRequested();
            for (int i = 0; i < 7; i++)
            {
                var day = WeekStart.AddDays(i);
                var dayEntries = entries.Where(e => e.DatePlanifiee.Date == day.Date).ToList();
                WeekDays.Add(new PlanningDayGroup(day, dayEntries));
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception) { ErrorMessage = "Erreur lors du chargement du planning."; }
        finally { SetBusy(false); }
    }

    [RelayCommand]
    private async Task AddVisitAsync(DateTime date)
        => await Shell.Current.GoToAsync("///visits/detail");
}

public class PlanningDayGroup
{
    public DateTime Date { get; }
    public string DayLabel { get; }
    public List<PlanningModel> Entries { get; }
    public bool HasEntries => Entries.Count > 0;
    public bool IsToday => Date.Date == DateTime.Today;

    public PlanningDayGroup(DateTime date, List<PlanningModel> entries)
    {
        Date = date;
        DayLabel = date.ToString("ddd dd/MM");
        Entries = entries;
    }
}
