using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Profile;

public partial class EditProfileViewModel : BaseViewModel
{
    private readonly AuthService _authService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AvatarInitials))]
    private string _name = string.Empty;

    [ObservableProperty] private string _telephone = string.Empty;
    [ObservableProperty] private string _adresse   = string.Empty;
    [ObservableProperty] private string _email     = string.Empty;
    [ObservableProperty] private string _role      = string.Empty;

    public string AvatarInitials =>
        string.IsNullOrWhiteSpace(Name) ? "?"
        : string.Join("", Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)
                              .Select(s => s[0])).ToUpperInvariant();

    public EditProfileViewModel(AuthService authService)
    {
        _authService = authService;
        Title = "Modifier le profil";
    }

    [RelayCommand]
    private Task LoadAsync() => ExecuteAsync(async () =>
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user != null)
        {
            Name      = user.Name;
            Email     = user.Email;
            Role      = user.Role;
            Telephone = user.Telephone ?? string.Empty;
            Adresse   = user.Adresse   ?? string.Empty;
        }
    });

    [RelayCommand]
    private Task SaveAsync() => ExecuteAsync(async () =>
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Le nom est requis.";
            return;
        }
        var userId = await SecureStorage.GetAsync(StorageKeys.UserId) ?? "0";
        await SecureStorage.SetAsync(StorageKeys.UserName,              Name.Trim());
        await SecureStorage.SetAsync(StorageKeys.UserTelephone(userId), Telephone.Trim());
        await SecureStorage.SetAsync(StorageKeys.UserAdresse(userId),   Adresse.Trim());
        await Shell.Current.GoToAsync("..");
    });

    [RelayCommand]
    private async Task CancelAsync() => await Shell.Current.GoToAsync("..");
}
