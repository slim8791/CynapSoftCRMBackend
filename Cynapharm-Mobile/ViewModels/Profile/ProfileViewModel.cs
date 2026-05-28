using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Auth;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Profile;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly AuthService   _authService;
    private readonly ICacheService _cache;
    private readonly KpiService    _kpiService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AvatarInitials), nameof(IsDelegue), nameof(IsClient), nameof(IsMedecin))]
    private UserInfo? _user;
    [ObservableProperty] private bool      _isEditing;
    [ObservableProperty] private string    _currentPassword       = string.Empty;
    [ObservableProperty] private string    _newPassword           = string.Empty;
    [ObservableProperty] private string    _confirmPassword       = string.Empty;
    [ObservableProperty] private string    _passwordErrorMessage  = string.Empty;
    [ObservableProperty] private string    _editName              = string.Empty;
    [ObservableProperty] private string    _editTelephone         = string.Empty;
    [ObservableProperty] private string    _editAdresse           = string.Empty;
    [ObservableProperty] private string    _wilayaLabel           = string.Empty;

    public bool IsDelegue => User?.Role is "DELEGUE" or "ADMIN" or "SUPERVISEUR";
    public bool IsClient  => User?.Role is "PHARMACIEN" or "GROSSISTE" or "CLIENT";
    public bool IsMedecin => User?.Role is "MEDECIN";

    public string AvatarInitials =>
        User != null && !string.IsNullOrWhiteSpace(User.Name)
            ? string.Join("", User.Name.Split(' ').Select(s => s[0])).ToUpper()
            : "?";

    public ProfileViewModel(AuthService authService, ICacheService cache, KpiService kpiService)
    {
        _authService = authService;
        _cache       = cache;
        _kpiService  = kpiService;
        Title = "Mon Profil";
    }

    [RelayCommand]
    private Task LoadAsync() => ExecuteAsync(async () =>
    {
        User = await _authService.GetCurrentUserAsync();
        if (User != null)
        {
            EditName      = User.Name;
            EditTelephone = User.Telephone ?? string.Empty;
            EditAdresse   = User.Adresse   ?? string.Empty;
            OnPropertyChanged(nameof(AvatarInitials));

            // For MÉDECIN: resolve RegionId (numeric string) to region name
            if (User.Role == "MEDECIN" && !string.IsNullOrWhiteSpace(User.RegionId))
            {
                if (int.TryParse(User.RegionId, out var regionIdInt))
                {
                    var regions = await _kpiService.GetRegionsAsync();
                    var match   = regions?.FirstOrDefault(r => r.Id == regionIdInt);
                    WilayaLabel = match?.Nom ?? User.RegionId;
                }
                else
                {
                    WilayaLabel = User.RegionId;
                }
            }
            else
            {
                WilayaLabel = string.Empty;
            }
        }
    });

    [RelayCommand]
    private void Edit()
    {
        if (IsEditing && User != null)
        {
            EditName      = User.Name;
            EditTelephone = User.Telephone;
            EditAdresse   = User.Adresse;
        }
        IsEditing = !IsEditing;
    }

    [RelayCommand]
    private Task SaveAsync() => ExecuteAsync(async () =>
    {
        if (string.IsNullOrWhiteSpace(EditName))
        {
            ErrorMessage = "Le nom est requis.";
            return;
        }
        if (User != null)
        {
            var result = await _authService.UpdateProfileAsync(new UpdateProfileDto
            {
                Name        = EditName.Trim(),
                PhoneNumber = EditTelephone?.Trim(),
                Adresse     = EditAdresse?.Trim(),
                Email       = User.Email
            });

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Message ?? "Erreur lors de la mise à jour du profil.";
                return;
            }

            User.Name      = EditName.Trim();
            User.Telephone = EditTelephone?.Trim() ?? string.Empty;
            User.Adresse   = EditAdresse?.Trim()   ?? string.Empty;

            var userId = User.Id.ToString();
            await SecureStorage.SetAsync(StorageKeys.UserName,              User.Name);
            await SecureStorage.SetAsync(StorageKeys.UserTelephone(userId), User.Telephone);
            await SecureStorage.SetAsync(StorageKeys.UserAdresse(userId),   User.Adresse);
            OnPropertyChanged(nameof(User));
            OnPropertyChanged(nameof(AvatarInitials));
        }
        IsEditing = false;
        await Shell.Current.DisplayAlert(
            "Profil mis à jour",
            "Vos informations ont été enregistrées.",
            "OK");
    });

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        PasswordErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(CurrentPassword))
        {
            PasswordErrorMessage = "Veuillez saisir votre mot de passe actuel.";
            return;
        }
        if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword.Length < 6)
        {
            PasswordErrorMessage = "Le nouveau mot de passe doit contenir au moins 6 caractères.";
            return;
        }
        if (NewPassword != ConfirmPassword)
        {
            PasswordErrorMessage = "Les mots de passe ne correspondent pas.";
            return;
        }
        if (!await CheckConnectivityAsync()) return;

        await ExecuteAsync(async () =>
        {
            if (User == null) return;
            await _authService.ChangePasswordAsync(
                new ChangePasswordRequest(User.Email, CurrentPassword, NewPassword));
            CurrentPassword = string.Empty;
            NewPassword     = string.Empty;
            ConfirmPassword = string.Empty;
            await Shell.Current.DisplayAlert("Succès", "Votre mot de passe a été modifié.", "OK");
        });

        if (HasError) PasswordErrorMessage = ErrorMessage;
    }

    [RelayCommand]
    private Task NavigateToEditProfileAsync()
        => Shell.Current.GoToAsync("///profile/edit");

    [RelayCommand]
    private Task NavigateToChangePasswordAsync()
        => Shell.Current.GoToAsync("///profile/changepassword");

    [RelayCommand]
    private async Task LogoutAsync()
    {
        bool confirmed = await Shell.Current.DisplayAlert(
            "Déconnexion",
            "Voulez-vous vraiment vous déconnecter ?",
            "Se déconnecter", "Annuler");

        if (confirmed)
        {
            _cache.InvalidateAll(); // clear all in-memory caches on logout
            _authService.Logout();
            await Shell.Current.GoToAsync("//login");
        }
    }
}
