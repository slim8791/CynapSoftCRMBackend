using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Field;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;
using Cynapharm_Mobile.ViewModels.Visites;
using PlanningModel = Cynapharm_Mobile.Models.Field.Planning;

namespace Cynapharm_Mobile.ViewModels.Planning;

[QueryProperty(nameof(PrefillDate), "prefillDate")]
public partial class PlanningFormViewModel : BaseViewModel
{
    private readonly PlanningService _planningSvc;
    private readonly UserService     _userSvc;

    [ObservableProperty] private string   _prefillDate = string.Empty;
    [ObservableProperty] private DateTime _date        = DateTime.Today;
    [ObservableProperty] private TimeSpan _heureDebut  = new(9, 0, 0);
    [ObservableProperty] private TimeSpan _heureFin    = new(10, 0, 0);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMedecinType), nameof(IsPharmacienType))]
    private int _selectedTypeIndex = 0; // 0=Médecin  1=Pharmacien

    public bool IsMedecinType    => SelectedTypeIndex == 0;
    public bool IsPharmacienType => SelectedTypeIndex == 1;

    [ObservableProperty] private ObservableCollection<UserPickerItem> _medecins    = new();
    [ObservableProperty] private ObservableCollection<UserPickerItem> _pharmaciens = new();
    [ObservableProperty] private UserPickerItem? _selectedMedecin;
    [ObservableProperty] private UserPickerItem? _selectedPharmacien;

    public List<string> TypeOptions { get; } = new() { "Médecin", "Pharmacien" };

    public PlanningFormViewModel(PlanningService planningSvc, UserService userSvc)
    {
        _planningSvc = planningSvc;
        _userSvc     = userSvc;
        Title = "Nouveau planning";
    }

    partial void OnPrefillDateChanged(string value)
    {
        if (DateTime.TryParse(value, out var dt)) Date = dt;
    }

    partial void OnSelectedTypeIndexChanged(int value)
    {
        if (value == 0) SelectedPharmacien = null;
        if (value == 1) SelectedMedecin    = null;
        OnPropertyChanged(nameof(IsMedecinType));
        OnPropertyChanged(nameof(IsPharmacienType));
    }

    [RelayCommand]
    void SelectMedecin()    => SelectedTypeIndex = 0;
    [RelayCommand]
    void SelectPharmacien() => SelectedTypeIndex = 1;

    [RelayCommand]
    public Task InitAsync() => ExecuteAsync(async () =>
    {
        var medecinTask    = _userSvc.GetUsersByRoleAsync("MEDECIN");
        var pharmacienTask = _userSvc.GetUsersByRoleAsync("CLIENT");
        await Task.WhenAll(medecinTask, pharmacienTask);

        Medecins.Clear();
        foreach (var u in medecinTask.Result ?? new())
            Medecins.Add(new UserPickerItem { Id = u.Id, Nom = u.Name });

        Pharmaciens.Clear();
        foreach (var u in (pharmacienTask.Result ?? new())
                     .Where(u => string.Equals(u.TypeClient, "PHARMACIEN",
                                               StringComparison.OrdinalIgnoreCase)))
            Pharmaciens.Add(new UserPickerItem { Id = u.Id, Nom = u.Name });
    });

    [RelayCommand]
    private Task SaveAsync() => ExecuteAsync(async () =>
    {
        if (HeureDebut >= HeureFin)
        {
            await Shell.Current.DisplayAlert("Validation",
                "L'heure de fin doit être après l'heure de début.", "OK");
            return;
        }

        if (IsMedecinType && SelectedMedecin == null)
        {
            await Shell.Current.DisplayAlert("Validation",
                "Veuillez sélectionner un médecin.", "OK");
            return;
        }
        if (IsPharmacienType && SelectedPharmacien == null)
        {
            await Shell.Current.DisplayAlert("Validation",
                "Veuillez sélectionner un pharmacien.", "OK");
            return;
        }

        var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
        if (!int.TryParse(userIdStr, out var delegueId)) return;

        var entry = new PlanningModel
        {
            DatePlanifiee = Date,
            HeureDebut    = HeureDebut,
            HeureFin      = HeureFin,
            DelegueId     = delegueId,
            TypeVisite    = SelectedTypeIndex == 0 ? 1 : 2,
            IdMedecin     = SelectedMedecin?.Id,
            IdPharmacien  = SelectedPharmacien?.Id,
            Etat          = 0
        };

        await _planningSvc.CreatePlanningEntryAsync(entry);
        await Shell.Current.GoToAsync("..");
    });

    [RelayCommand]
    private Task CancelAsync() => Shell.Current.GoToAsync("..");
}
