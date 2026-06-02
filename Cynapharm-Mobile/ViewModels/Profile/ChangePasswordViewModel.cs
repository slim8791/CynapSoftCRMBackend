using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Auth;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;
using Microsoft.Maui.Graphics;

namespace Cynapharm_Mobile.ViewModels.Profile;

public partial class ChangePasswordViewModel : BaseViewModel
{
    private readonly AuthService _authService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PasswordStrength), nameof(StrengthLabel),
        nameof(Seg1Color), nameof(Seg2Color), nameof(Seg3Color), nameof(Seg4Color),
        nameof(PasswordsMatch), nameof(PasswordsMismatch))]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PasswordsMatch), nameof(PasswordsMismatch))]
    private string _confirmPassword = string.Empty;

    [ObservableProperty] private string _oldPassword        = string.Empty;
    [ObservableProperty] private bool   _showOldPassword;
    [ObservableProperty] private bool   _showNewPassword;
    [ObservableProperty] private bool   _showConfirmPassword;

    // ── Password strength ─────────────────────────────────────────────────

    public int PasswordStrength => Strength(NewPassword);

    public string StrengthLabel => PasswordStrength switch
    {
        1 => "Faible",
        2 => "Moyen",
        3 => "Fort",
        4 => "Très fort",
        _ => string.Empty
    };

    public Color Seg1Color => PasswordStrength >= 1 ? StrengthColor(1) : Color.FromArgb("#E0E0E0");
    public Color Seg2Color => PasswordStrength >= 2 ? StrengthColor(2) : Color.FromArgb("#E0E0E0");
    public Color Seg3Color => PasswordStrength >= 3 ? StrengthColor(3) : Color.FromArgb("#E0E0E0");
    public Color Seg4Color => PasswordStrength >= 4 ? StrengthColor(4) : Color.FromArgb("#E0E0E0");

    // ── Match indicators ──────────────────────────────────────────────────

    public bool PasswordsMatch =>
        !string.IsNullOrEmpty(NewPassword) &&
        !string.IsNullOrEmpty(ConfirmPassword) &&
        NewPassword == ConfirmPassword;

    public bool PasswordsMismatch =>
        !string.IsNullOrEmpty(NewPassword) &&
        !string.IsNullOrEmpty(ConfirmPassword) &&
        NewPassword != ConfirmPassword;

    public ChangePasswordViewModel(AuthService authService)
    {
        _authService = authService;
        Title = "Changer le mot de passe";
    }

    [RelayCommand]
    private Task ChangePasswordAsync() => ExecuteAsync(async () =>
    {
        if (string.IsNullOrWhiteSpace(OldPassword))
        {
            ErrorMessage = "Veuillez saisir votre mot de passe actuel.";
            return;
        }
        if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword.Length < 6)
        {
            ErrorMessage = "Le nouveau mot de passe doit contenir au moins 6 caractères.";
            return;
        }
        if (NewPassword != ConfirmPassword)
        {
            ErrorMessage = "Les mots de passe ne correspondent pas.";
            return;
        }

        var user = await _authService.GetCurrentUserAsync();
        var email = user?.Email ?? string.Empty;
        if (string.IsNullOrWhiteSpace(email))
        {
            ErrorMessage = "Impossible de récupérer l'adresse e-mail. Reconnectez-vous.";
            return;
        }
        await _authService.ChangePasswordAsync(new ChangePasswordRequest(email, OldPassword, NewPassword));

        OldPassword = NewPassword = ConfirmPassword = string.Empty;

        try
        {
            await Snackbar.Make("Mot de passe modifié avec succès ✓",
                duration: TimeSpan.FromSeconds(3)).Show();
        }
        catch { }

        await Shell.Current.GoToAsync("..");
    });

    [RelayCommand]
    private void ToggleShowOldPassword()     => ShowOldPassword     = !ShowOldPassword;
    [RelayCommand]
    private void ToggleShowNewPassword()     => ShowNewPassword     = !ShowNewPassword;
    [RelayCommand]
    private void ToggleShowConfirmPassword() => ShowConfirmPassword = !ShowConfirmPassword;

    [RelayCommand]
    private async Task CancelAsync() => await Shell.Current.GoToAsync("..");

    // ── Helpers ───────────────────────────────────────────────────────────

    private static int Strength(string pw)
    {
        if (string.IsNullOrEmpty(pw))    return 0;
        if (pw.Length < 6)               return 1;
        bool hasDigit   = pw.Any(char.IsDigit);
        bool hasSpecial = pw.Any(c => !char.IsLetterOrDigit(c));
        if (pw.Length >= 8 && hasDigit && hasSpecial) return 4;
        if (pw.Length >= 8 && hasDigit)               return 3;
        return 2;
    }

    private static Color StrengthColor(int level) => level switch
    {
        1 => Color.FromArgb("#E24B4A"),
        2 => Color.FromArgb("#F5A623"),
        3 => Color.FromArgb("#00B4D8"),
        _ => Color.FromArgb("#1A6B3C"),
    };
}
