using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Auth;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Clients;

public partial class ClientFormViewModel : BaseViewModel, IQueryAttributable
{
    private readonly UserService _userSvc;

    [ObservableProperty] private string _nom = string.Empty;
    [ObservableProperty] private string _prenom = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _telephone = string.Empty;
    [ObservableProperty] private string _adresse = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private int _selectedTypeIndex; // 0 = PHARMACIEN, 1 = GROSSISTE

    public bool IsNew => ClientId <= 0;
    public int ClientId { get; private set; }

    public List<string> ClientTypes { get; } = new() { "PHARMACIEN", "GROSSISTE" };

    public ClientFormViewModel(UserService userSvc)
    {
        _userSvc = userSvc;
        Title = "Nouveau client";
    }

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

            Nom = client.Name ?? string.Empty;
            Email = client.Email ?? string.Empty;
            Telephone = client.Telephone ?? string.Empty;
            Adresse = client.Adresse ?? string.Empty;
        });
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(Nom) || string.IsNullOrWhiteSpace(Email))
        {
            await Shell.Current.DisplayAlert("Validation", "Nom et Email sont obligatoires.", "OK");
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

                var idRegionStr = await SecureStorage.GetAsync(StorageKeys.UserIdRegion);
                var dto = new CreateUserDto
                {
                    Name = $"{Prenom} {Nom}".Trim(),
                    Email = Email,
                    Password = Password,
                    PhoneNumber = Telephone,
                    Adresse = Adresse,
                    Role = "CLIENT",
                    UserType = ClientTypes[SelectedTypeIndex],
                    IdRegion = int.TryParse(idRegionStr, out var idRegion) ? idRegion : (int?)null
                };
                await _userSvc.CreateUserAsync(dto);
                await Shell.Current.DisplayAlert("Succès", "Compte client créé avec succès.", "OK");
            }
            else
            {
                var dto = new UpdateUserDto
                {
                    Email = Email,
                    Name = $"{Prenom} {Nom}".Trim(),
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

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
