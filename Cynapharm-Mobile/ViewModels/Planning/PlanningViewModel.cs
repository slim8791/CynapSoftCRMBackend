using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Field;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;
using PlanningModel = Cynapharm_Mobile.Models.Field.Planning;
using CreateVisiteDto = Cynapharm_Mobile.Models.Field.CreateVisiteDto;

namespace Cynapharm_Mobile.ViewModels.Planning;

public partial class PlanningViewModel : BaseViewModel
{
    private readonly PlanningService _planningService;
    private readonly VisiteService   _visiteService;
    private CancellationTokenSource? _loadCts;

    [ObservableProperty] private DateTime _weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);

    // ISSUE #1 fix: label matches the 6-day (Mon→Sat) display — AddDays(5) = Saturday
    public string WeekLabel => $"{WeekStart:dd MMM} – {WeekStart.AddDays(5):dd MMM yyyy}";

    public ObservableCollection<PlanningDayGroup> WeekDays { get; } = new();

    public PlanningViewModel(PlanningService planningService, VisiteService visiteService)
    {
        _planningService = planningService;
        _visiteService   = visiteService;
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
            var weekEnd = WeekStart.AddDays(5);

            // Load planning entries and visites in parallel
            var planningTask = _planningService.GetPlanningAsync(WeekStart);
            var visiteTask   = _visiteService.GetVisitesAsync(WeekStart, weekEnd, null);
            await Task.WhenAll(planningTask, visiteTask);

            ct.ThrowIfCancellationRequested();
            var entries = planningTask.Result ?? new List<PlanningModel>();
            var visites = visiteTask.Result   ?? new List<Visite>();

            // 6 days: Monday → Saturday (Tunisian working week)
            WeekDays.Clear();
            for (int i = 0; i < 6; i++)
            {
                var day        = WeekStart.AddDays(i);
                var dayVisites = visites.Where(v => v.DateVisite.Date == day.Date).ToList();
                // Filter out plannings that already have a Visite to avoid duplicates and hide the 'Démarrer' button
                var dayEntries = entries
                    .Where(e => e.DatePlanifiee.Date == day.Date && !dayVisites.Any(v => v.IdPlanning == e.Id))
                    .ToList();
                WeekDays.Add(new PlanningDayGroup(day, dayEntries, dayVisites));
            }
        });
    }

    [RelayCommand]
    private async Task OpenVisiteAsync(int visiteId)
    {
        if (visiteId > 0)
            await Shell.Current.GoToAsync($"///visits/detail?visiteId={visiteId}");
    }

    /// <summary>
    /// "+" button → ouvre le formulaire de création de planning (date pré-remplie).
    /// </summary>
    [RelayCommand]
    private async Task AddVisitAsync(DateTime date)
    {
        var effectiveDate = date != default ? date : DateTime.Today;
        var dateStr = effectiveDate.ToString("yyyy-MM-dd");
        await Shell.Current.GoToAsync($"///planning/form?prefillDate={dateStr}");
    }

    /// <summary>
    /// Bouton "Démarrer" sur une carte planning → crée automatiquement la visite
    /// à partir des données du planning (médecin/pharmacien + date).
    /// </summary>
    [RelayCommand]
    private async Task DemarrerVisiteAsync(PlanningModel planning)
    {
        if (planning == null) return;

        var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
        if (!int.TryParse(userIdStr, out var delegueId)) return;

        var dto = new CreateVisiteDto
        {
            DateVisite   = planning.DatePlanifiee,
            Type         = planning.TypeVisite,
            IdMedecin    = planning.IdMedecin,
            IdPharmacien = planning.IdPharmacien,
            IdPlanning   = planning.Id,
            IdDelegue    = delegueId
        };

        var visite = await _visiteService.CreateVisiteAsync(dto);
        if (visite != null)
            await Shell.Current.GoToAsync($"///visits/detail?visiteId={visite.Id}");
    }
}

public class PlanningDayGroup
{
    public DateTime Date { get; }
    public string DayLabel { get; }
    public List<PlanningModel> Entries { get; }
    public List<Visite>        Visites { get; }
    public bool HasEntries => Entries.Count > 0 || Visites.Count > 0;
    public bool HasVisites  => Visites.Count > 0;
    public bool IsToday    => Date.Date == DateTime.Today;

    public PlanningDayGroup(DateTime date, List<PlanningModel> entries, List<Visite> visites)
    {
        Date     = date;
        DayLabel = date.ToString("ddd dd/MM");
        Entries  = entries;
        Visites  = visites;
    }
}
