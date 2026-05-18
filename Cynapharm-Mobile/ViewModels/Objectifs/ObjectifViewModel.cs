using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Field;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Objectifs;

public partial class ObjectifViewModel : BaseViewModel
{
    private readonly KpiService _kpiService;

    public ObservableCollection<Objectif> Objectifs { get; } = new();

    [ObservableProperty] private string _periode = DateTime.Today.ToString("yyyy-MM");
    [ObservableProperty] private double _globalAchievement;

    public ObjectifViewModel(KpiService kpiService)
    {
        _kpiService = kpiService;
        Title = "Objectifs";
    }

    [RelayCommand]
    private Task LoadAsync() => ExecuteAsync(async () =>
    {
        if (!await CheckConnectivityAsync()) return;
        Objectifs.Clear();
        var result = await _kpiService.GetObjectifsAsync();
        if (result != null)
        {
            foreach (var o in result) Objectifs.Add(o);
            if (result.Count > 0)
            {
                var achieved = result
                    .Where(o => o.ValeurCible > 0)
                    .Select(o => (double)(o.ValeurActuelle ?? 0) / (double)o.ValeurCible * 100);
                GlobalAchievement = achieved.Any() ? achieved.Average() : 0;
            }
        }
    });

    protected override Task RetryAsync() => LoadAsync();
}
