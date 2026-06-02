using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Auth;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Clients;

public partial class ClientFormViewModel : BaseViewModel, IQueryAttributable
{
    private readonly UserService  _userSvc;
    private readonly FieldService _fieldSvc;

    [ObservableProperty] private string _nomComplet    = string.Empty;
    [ObservableProperty] private string _email         = string.Empty;
    [ObservableProperty] private string _telephone     = string.Empty;
    [ObservableProperty] private string _adresse       = string.Empty;
    [ObservableProperty] private string _password      = string.Empty;
    [ObservableProperty] private string _specialite    = string.Empty;
    [ObservableProperty] private string _typeEtablissement = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMedecin), nameof(IsClient),
                              nameof(IsPharmacien), nameof(IsGrossiste),
                              nameof(RoleBadgeLabel), nameof(RoleBadgeColor))]
    private int _selectedTypeIndex; // 0=Pharmacien  1=Grossiste  2=Médecin

    public bool IsNew        => ClientId <= 0;
    public bool IsMedecin    => SelectedTypeIndex == 2;
    public bool IsClient     => SelectedTypeIndex != 2;
    public bool IsPharmacien => SelectedTypeIndex == 0;
    public bool IsGrossiste  => SelectedTypeIndex == 1;
    public int  ClientId     { get; private set; }

    // Textes calculés — évite le recours à InvertedBoolConverter avec paramètre pipe
    public string FormSubtitle => IsNew ? "Remplissez les informations du compte" : "Modifiez les informations";
    public string SaveLabel    => IsNew ? "Créer le compte" : "Mettre à jour";

    // Badge de rôle affiché sous le sélecteur
    public string RoleBadgeLabel => SelectedTypeIndex switch
    {
        0 => "PHARMACIEN",
        1 => "GROSSISTE",
        _ => "MÉDECIN"
    };
    public string RoleBadgeColor => IsMedecin ? "#1565C0" : "#2E7D32";

    public ClientFormViewModel(UserService userSvc, FieldService fieldSvc)
    {
        _userSvc  = userSvc;
        _fieldSvc = fieldSvc;
        Title = "Nouveau compte";
    }

    [RelayCommand] void SelectPharmacien() => SelectedTypeIndex = 0;
    [RelayCommand] void SelectGrossiste()  => SelectedTypeIndex = 1;
    [RelayCommand] void SelectMedecin()    => SelectedTypeIndex = 2;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("clientId", out var idObj) && int.TryParse(idObj?.ToString(), out var id) && id > 0)
        {
            ClientId = id;
            Title = "Modifier le client";
            OnPropertyChanged(nameof(IsNew));
            _ = LoadClientAsync();
        }
    }

    private async Task LoadClientAsync()
    {
        await ExecuteAsync(async () =>
        {
            var client = await _userSvc.GetUserByIdAsync(ClientId);
            if (client == null) return;

            NomComplet = client.Name ?? string.Empty;
            Email = client.Email ?? string.Empty;
            Telephone = client.Telephone ?? string.Empty;
            Adresse = client.Adresse ?? string.Empty;
        });
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(NomComplet) || string.IsNullOrWhiteSpace(Email))
        {
            await Shell.Current.DisplayAlert("Validation", "Nom complet et Email sont obligatoires.", "OK");
            return;
        }

        if (IsMedecin && string.IsNullOrWhiteSpace(Specialite))
        {
            await Shell.Current.DisplayAlert("Validation", "La spécialité est obligatoire pour un médecin.", "OK");
            return;
        }

        await ExecuteAsync(async () =>
        {
            if (IsNew)
            {
                if (string.IsNullOrWhiteSpace(Password))
                {
                    await Shell.Current.DisplayAlert("Validation", "Le mot de passe est obligatoire.", "OK");
                    return;
                }

                var idRegion = await ResolveCurrentUserRegionAsync();

                CreateUserDto dto;
                if (IsMedecin)
                {
                    dto = new CreateUserDto
                    {
                        Name      = NomComplet.Trim(),
                        Email     = Email,
                        Password  = Password,
                        PhoneNumber = Telephone,
                        Adresse   = Adresse,
                        Role      = "MEDECIN",
                        UserType  = "PHARMACIEN", // ignored by backend for MEDECIN
                        IdRegion  = idRegion,
                        Specialite         = Specialite.Trim(),
                        TypeEtablissement  = TypeEtablissement.Trim()
                    };
                }
                else
                {
                    dto = new CreateUserDto
                    {
                        Name      = NomComplet.Trim(),
                        Email     = Email,
                        Password  = Password,
                        PhoneNumber = Telephone,
                        Adresse   = Adresse,
                        Role      = "CLIENT",
                        UserType  = SelectedTypeIndex == 0 ? "PHARMACIEN" : "GROSSISTE",
                        IdRegion  = idRegion
                    };
                }

                await _userSvc.CreateUserAsync(dto);
                var label = IsMedecin ? "médecin" : SelectedTypeIndex == 0 ? "pharmacien" : "grossiste";
                await Shell.Current.DisplayAlert("Succès", $"Compte {label} créé avec succès.", "OK");
            }
            else
            {
                var dto = new UpdateUserDto
                {
                    Email = Email,
                    Name = NomComplet.Trim(),
                    PhoneNumber = Telephone,
                    Adresse = Adresse
                    // Role NOT included — DÉLÉGUÉ cannot change role
                };
                await _userSvc.UpdateUserAsync(dto);
                await Shell.Current.DisplayAlert("Succès", "Profil client mis à jour.", "OK");
            }
            await Shell.Current.GoToAsync("..");
        });
    }

    /// <summary>
    /// Resolves the logged-in user's region ID.
    /// Fast path: reads the cached value from SecureStorage.
    /// Fallback (SUPERVISEUR whose idRegion is stored on the Region side):
    ///   calls GET /fields/regions/by-superviseur/{id}, caches the result,
    ///   so future calls hit the fast path.
    /// </summary>
    private async Task<int?> ResolveCurrentUserRegionAsync()
    {
        var cached = await SecureStorage.GetAsync(StorageKeys.UserIdRegion);
        if (int.TryParse(cached, out var rid))
            return rid;

        // Cache miss — the SUPERVISEUR's region is stored on the Region entity,
        // not on their user profile.  Fetch it explicitly.
        var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
        if (!int.TryParse(userIdStr, out var supId))
            return null;

        try
        {
            var regions = await _fieldSvc.GetRegionsBySuperviseurAsync(supId);
            var regionId = regions?.FirstOrDefault()?.Id;
            if (regionId.HasValue)
                await SecureStorage.SetAsync(StorageKeys.UserIdRegion, regionId.Value.ToString());
            return regionId;
        }
        catch
        {
            return null;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
