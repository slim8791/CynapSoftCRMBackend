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

    // ISSUE #1 fix: label matches the 6-day (Mon→Sat) display — AddDays(5) = Saturday
    public string WeekLabel => $"{WeekStart:dd MMM} – {WeekStart.AddDays(5):dd MMM yyyy}";

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
        WeekDays.Clear();
        if (!await CheckConnectivityAsync()) return;

        await ExecuteAsync(async () =>
        {
            ct.ThrowIfCancellationRequested();
            var entries = await _planningService.GetPlanningAsync(WeekStart) ?? new List<PlanningModel>();
            ct.ThrowIfCancellationRequested();

            // 6 days: Monday → Saturday (Tunisian working week)
            WeekDays.Clear();
            for (int i = 0; i < 6; i++)
            {
                var day = WeekStart.AddDays(i);
                var dayEntries = entries.Where(e => e.DatePlanifiee.Date == day.Date).ToList();
                WeekDays.Add(new PlanningDayGroup(day, dayEntries));
            }
        });
    }

    /// <summary>
    /// Called by the "+" button on each day row (CommandParameter = day date)
    /// AND by the sticky "+ Ajouter" button (CommandParameter = default → today).
    /// Passes idPlanning when a planning entry exists for that day (ISSUE #3/#4 fix).
    /// </summary>
    [RelayCommand]
    private async Task AddVisitAsync(DateTime date)
    {
        var effectiveDate = date != default ? date : DateTime.Today;
        var dateStr = effectiveDate.ToString("yyyy-MM-dd");

        // Find the first planning entry for this day in the current week view
        var planning = WeekDays
            .FirstOrDefault(g => g.Date.Date == effectiveDate.Date)
            ?.Entries.FirstOrDefault();

        var route = planning != null
            ? $"//visits/detail?prefillDate={dateStr}&idPlanning={planning.Id}"
            : $"//visits/detail?prefillDate={dateStr}";

        await Shell.Current.GoToAsync(route);
    }
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
