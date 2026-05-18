using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Field;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Visites;

[QueryProperty(nameof(VisiteId), "visiteId")]
public partial class VisitDetailViewModel : BaseViewModel
{
    private readonly VisiteService _visiteService;

    [ObservableProperty] private int      _visiteId;
    [ObservableProperty] private string   _clientName = string.Empty;
    [ObservableProperty] private DateTime _visiteDate = DateTime.Now;
    [ObservableProperty] private string   _notes  = string.Empty;
    [ObservableProperty] private string   _statut = "PLANIFIEE";

    private bool _isDirty;
    public bool IsDirty => _isDirty;

    public bool IsNew      => VisiteId == 0;
    public bool IsExisting => VisiteId > 0;

    public List<string> StatutOptions { get; } = new() { "PLANIFIEE", "REALISEE", "ANNULEE" };

    public VisitDetailViewModel(VisiteService visiteService)
    {
        _visiteService = visiteService;
        Title = "Nouvelle visite";
    }

    partial void OnVisiteIdChanged(int value)
    {
        if (value > 0) _ = LoadAsync();
        Title = value > 0 ? "Détail visite" : "Nouvelle visite";
        OnPropertyChanged(nameof(IsNew));
        OnPropertyChanged(nameof(IsExisting));
    }

    partial void OnClientNameChanged(string value) => _isDirty = true;
    partial void OnNotesChanged(string value)      => _isDirty = true;
    partial void OnStatutChanged(string value)     => _isDirty = true;

    [RelayCommand]
    private Task LoadAsync() => ExecuteAsync(async () =>
    {
        var visite = await _visiteService.GetVisiteByIdAsync(VisiteId);
        if (visite != null)
        {
            ClientName = visite.ClientNom;
            VisiteDate = visite.DateVisite;
            Notes      = visite.Notes ?? string.Empty;
            Statut     = visite.Statut;
        }
    });

    [RelayCommand]
    private Task SaveAsync() => ExecuteAsync(async () =>
    {
        if (string.IsNullOrWhiteSpace(ClientName))
        {
            ErrorMessage = "Le nom du client est requis.";
            return;
        }
        if (!await CheckConnectivityAsync()) return;
        var visite = new Visite { ClientNom = ClientName, DateVisite = VisiteDate, Notes = Notes, Statut = Statut };
        if (IsNew)
            await _visiteService.CreateVisiteAsync(visite);
        else
            await _visiteService.UpdateVisiteAsync(VisiteId, visite);
        _isDirty = false;
        HapticService.Success();
        await Shell.Current.GoToAsync("..");
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
        => await Shell.Current.GoToAsync($"//visits/rapport?visiteId={VisiteId}");
}
