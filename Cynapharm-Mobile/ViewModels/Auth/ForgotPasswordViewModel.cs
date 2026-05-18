using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Auth;

public partial class ForgotPasswordViewModel : BaseViewModel
{
    private readonly AuthService _authService;

    [ObservableProperty] private string _email          = string.Empty;
    [ObservableProperty] private string _successMessage = string.Empty;

    public ForgotPasswordViewModel(AuthService authService)
    {
        _authService = authService;
        Title = "Mot de passe oublié";
    }

    [RelayCommand]
    private Task SendResetAsync() => ExecuteAsync(async () =>
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Veuillez saisir votre adresse email.";
            return;
        }
        if (!await CheckConnectivityAsync()) return;
        await _authService.ForgotPasswordAsync(Email);
        SuccessMessage = "Un lien de réinitialisation a été envoyé à votre email.";
        await Task.Delay(3000);
        await Shell.Current.GoToAsync("..");
    });

    [RelayCommand]
    private async Task GoBackAsync() => await Shell.Current.GoToAsync("..");
}
