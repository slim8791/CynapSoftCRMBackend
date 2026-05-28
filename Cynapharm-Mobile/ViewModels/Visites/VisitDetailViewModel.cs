using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Cynapharm_Mobile.Messages;
using Cynapharm_Mobile.Models.Field;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Visites;

public class UserPickerItem
{
    public int    Id  { get; set; }
    public string Nom { get; set; } = string.Empty;
}

[QueryProperty(nameof(VisiteId),      "visiteId")]
[QueryProperty(nameof(PrefillDate),   "prefillDate")]
[QueryProperty(nameof(IdPlanningRaw), "idPlanning")]
public partial class VisitDetailViewModel : BaseViewModel
{
    private readonly VisiteService   _visiteService;
    private readonly UserService     _userSvc;
    private readonly PlanningService _planningSvc;

    [ObservableProperty] private int      _visiteId;
    [ObservableProperty] private string   _prefillDate  = string.Empty;
    [ObservableProperty] private string   _clientName   = string.Empty;
    [ObservableProperty] private DateTime _visiteDate   = DateTime.Now;
    [ObservableProperty] private string   _notes        = string.Empty;
    [ObservableProperty] private int      _selectedType = 1;
    [ObservableProperty] private int?     _selectedPlanningId;
    [ObservableProperty] private string   _idPlanningRaw = string.Empty;

    [ObservableProperty]
    private ObservableCollection<UserPickerItem> _medecins = new();

    [ObservableProperty]
    private ObservableCollection<UserPickerItem> _pharmaciens = new();

    [ObservableProperty]
    private UserPickerItem? _selectedMedecin;

    [ObservableProperty]
    private UserPickerItem? _selectedPharmacien;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPlanningLabel))]
    private string _planningLabel = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEdit))]
    [NotifyPropertyChangedFor(nameof(CanDelete))]
    [NotifyPropertyChangedFor(nameof(ShowEditRapport))]
    [NotifyPropertyChangedFor(nameof(ShowViewRapport))]
    private bool _isCompleted;

    /// <summary>True when the visite already has a submitted rapport.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowSubmitRapport))]
    [NotifyPropertyChangedFor(nameof(ShowEditRapport))]
    [NotifyPropertyChangedFor(nameof(ShowViewRapport))]
    private bool _hasRapport;

    public bool HasPlanningLabel => !string.IsNullOrEmpty(PlanningLabel);

    /// <summary>Form is editable only when the visite has not been completed yet.</summary>
    public bool CanEdit   => !IsCompleted;

    /// <summary>Delete is only available for existing, non-completed visites.</summary>
    public bool CanDelete => IsExisting && !IsCompleted;

    // ── Rapport action button — three mutually exclusive display states ─────────

    /// <summary>No rapport yet → "Soumettre un rapport" (create).</summary>
    public bool ShowSubmitRapport => IsExisting && !HasRapport;

    /// <summary>Rapport exists, superviseur hasn't validated yet → "Modifier le rapport" (edit).</summary>
    public bool ShowEditRapport   => IsExisting && HasRapport && !IsCompleted;

    /// <summary>Rapport locked by superviseur validation → "Voir le rapport" (read-only).</summary>
    public bool ShowViewRapport   => IsExisting && HasRapport && IsCompleted;

    private bool _isDirty;
    public bool IsDirty => _isDirty;

    public bool IsNew      => VisiteId == 0;
    public bool IsExisting => VisiteId > 0;

    public List<string> VisiteTypeOptions { get; } = new() { "Médecin", "Pharmacien", "Autre" };

    public string SelectedTypeLabel
    {
        get => SelectedType switch { 1 => "Médecin", 2 => "Pharmacien", _ => "Autre" };
        set
        {
            SelectedType = value switch { "Médecin" => 1, "Pharmacien" => 2, _ => 3 };
            OnPropertyChanged();
        }
    }

    public VisitDetailViewModel(VisiteService visiteService, UserService userSvc, PlanningService planningSvc)
    {
        _visiteService = visiteService;
        _userSvc       = userSvc;
        _planningSvc   = planningSvc;
        Title = "Nouvelle visite";

        // Listen for rapport submission from RapportViewModel.
        // Only HasRapport is set here — IsCompleted stays false until the superviseur validates.
        WeakReferenceMessenger.Default.Register<VisiteCompletedMessage>(this, (_, m) =>
        {
            if (m.VisiteId == VisiteId)
                HasRapport = true;
            // ShowSubmitRapport→false, ShowEditRapport→true via [NotifyPropertyChangedFor]
        });
    }

    partial void OnVisiteIdChanged(int value)
    {
        Title = value > 0 ? "Détail visite" : "Nouvelle visite";
        OnPropertyChanged(nameof(IsNew));
        OnPropertyChanged(nameof(IsExisting));
        OnPropertyChanged(nameof(CanDelete));
        OnPropertyChanged(nameof(ShowSubmitRapport));
        OnPropertyChanged(nameof(ShowEditRapport));
        OnPropertyChanged(nameof(ShowViewRapport));
    }

    partial void OnPrefillDateChanged(string value)
    {
        if (DateTime.TryParse(value, out var dt)) VisiteDate = dt;
    }

    partial void OnClientNameChanged(string value)    => _isDirty = true;
    partial void OnNotesChanged(string value)         => _isDirty = true;
    partial void OnSelectedMedecinChanged(UserPickerItem? value)    => _isDirty = true;
    partial void OnSelectedPharmacienChanged(UserPickerItem? value) => _isDirty = true;
    partial void OnSelectedTypeChanged(int value)
    {
        _isDirty = true;
        OnPropertyChanged(nameof(SelectedTypeLabel));
    }

    [RelayCommand]
    public Task InitAsync() => ExecuteAsync(async () =>
    {
        // In CynapCRM, both PHARMACIEN and GROSSISTE share the "CLIENT" role.
        // TypeClient (PHARMACIEN / GROSSISTE) is the subtype field.
        var medecinTask    = _userSvc.GetUsersByRoleAsync("MEDECIN");
        var pharmacienTask = _userSvc.GetUsersByRoleAsync("CLIENT");
        await Task.WhenAll(medecinTask, pharmacienTask);

        Medecins.Clear();
        foreach (var u in medecinTask.Result ?? new())
            Medecins.Add(new UserPickerItem { Id = u.Id, Nom = u.Name });

        Pharmaciens.Clear();
        foreach (var u in pharmacienTask.Result ?? new())
            Pharmaciens.Add(new UserPickerItem
            {
                Id  = u.Id,
                // Show TypeClient subtype so the delegate can distinguish
                // "Pharmacie Centrale (PHARMACIEN)" from "Grossiste Nord (GROSSISTE)"
                Nom = string.IsNullOrWhiteSpace(u.TypeClient)
                    ? u.Name
                    : $"{u.Name} ({u.TypeClient})"
            });

        // Load existing visite data and restore picker selections
        if (VisiteId > 0)
        {
            var visite = await _visiteService.GetVisiteByIdAsync(VisiteId);
            if (visite != null)
            {
                VisiteDate         = visite.DateVisite;
                SelectedType       = visite.Type;
                SelectedPlanningId = visite.IdPlanning;
                IsCompleted        = visite.IsCompleted;
                HasRapport         = visite.HasRapport;   // ISSUE #11
                SelectedMedecin    = Medecins.FirstOrDefault(m => m.Id == visite.IdMedecin);
                SelectedPharmacien = Pharmaciens.FirstOrDefault(p => p.Id == visite.IdPharmacien);
            }
        }

        // Load planning label if navigated from PlanningPage
        if (int.TryParse(IdPlanningRaw, out var pid))
        {
            SelectedPlanningId = pid;
            var p = await _planningSvc.GetPlanningByIdAsync(pid);
            if (p != null)
                PlanningLabel = $"Lié au planning du {p.DatePlanifiee:dd/MM/yyyy} " +
                                $"{p.HeureDebut:hh\\:mm}–{p.HeureFin:hh\\:mm}";
        }

        _isDirty = false;
    });

    [RelayCommand]
    private Task SaveAsync() => ExecuteAsync(async () =>
    {
        if (!await CheckConnectivityAsync()) return;

        // ISSUE #8 fix: validate userId before use — never silently pass 0
        var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
        if (!int.TryParse(userIdStr, out var delegueId) || delegueId == 0)
        {
            await Shell.Current.DisplayAlert(
                "Session expirée",
                "Veuillez vous reconnecter.",
                "OK");
            await Shell.Current.GoToAsync("//login");
            return;
        }

        var dto = new CreateVisiteDto
        {
            DateVisite   = VisiteDate,
            Type         = SelectedType,
            IdMedecin    = SelectedMedecin?.Id,
            IdPharmacien = SelectedPharmacien?.Id,
            IdPlanning   = SelectedPlanningId,
            IdDelegue    = delegueId,
        };

        // ISSUE #9/#10/#22 fix: capture returned visite to navigate to it after creation
        Visite? result;
        if (IsNew)
            result = await _visiteService.CreateVisiteAsync(dto);
        else
        {
            dto.IdVisite = VisiteId;
            result = await _visiteService.UpdateVisiteAsync(VisiteId, dto);
        }

        _isDirty = false;
        HapticService.Success();

        if (IsNew && result != null)
        {
            // Navigate to the new visite so the user can immediately submit a rapport
            await Shell.Current.GoToAsync($"//visits/detail?visiteId={result.Id}");
        }
        else
        {
            await Shell.Current.GoToAsync("..");
        }
    });

    [RelayCommand]
    private Task DeleteAsync() => ExecuteAsync(async () =>
    {
        if (!await Shell.Current.DisplayAlert("Confirmation", "Supprimer cette visite ?", "Oui", "Annuler")) return;
        await _visiteService.DeleteVisiteAsync(VisiteId);
        await Shell.Current.GoToAsync("..");
    });

    [RelayCommand]
    private async Task GoToRapportAsync()
    {
        // Determine mode from current state — single command handles all three buttons
        var mode = !HasRapport   ? string.Empty           // create
                 : !IsCompleted  ? "&rapportMode=edit"    // edit (superviseur not yet validated)
                 :                 "&rapportMode=view";   // read-only (locked by superviseur)

        await Shell.Current.GoToAsync($"//visits/rapport?visiteId={VisiteId}{mode}");
    }
}
