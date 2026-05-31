using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Auth;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Auth;

public partial class LoginViewModel : BaseViewModel
{
    private readonly AuthService _authService;

    [ObservableProperty] private string _email          = string.Empty;
    [ObservableProperty] private string _password       = string.Empty;
    [ObservableProperty] private bool   _isPasswordHidden = true;

    public LoginViewModel(AuthService authService)
    {
        _authService = authService;
        Title = "Connexion";
    }

    [RelayCommand]
    private Task LoginAsync() => ExecuteAsync(async () =>
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Veuillez renseigner votre email et mot de passe.";
            return;
        }
        if (!await CheckConnectivityAsync()) return;

        var result = await _authService.LoginAsync(new LoginRequest(Email, Password));
        if (result == null || result.User == null)
        {
            ErrorMessage = "Email ou mot de passe incorrect.";
            return;
        }

        var role  = result.User.Role;
        var shell = Shell.Current as AppShell;
        if (shell == null)
        {
            ErrorMessage = "Erreur d'initialisation. Veuillez redémarrer l'application.";
            return;
        }

        shell.ApplyRoleVisibility(role);
        // SUPERVISEUR role is deprecated in UI. Treated as DELEGUE for navigation.
        var target = role switch
        {
            "DELEGUE" or "ADMIN" or "SUPERVISEUR" => "//dashboard",
            "PHARMACIEN" or "GROSSISTE" or "CLIENT" => "//orders",
            "MEDECIN" => "//products",
            _ => "//orders"
        };
        await Shell.Current.GoToAsync(target);

    });

    [RelayCommand]
    private void TogglePasswordVisibility() => IsPasswordHidden = !IsPasswordHidden;

    [RelayCommand]
    private async Task GoToForgotPasswordAsync()
        => await Shell.Current.GoToAsync("forgotpassword");
}
