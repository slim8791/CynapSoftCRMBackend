using Cynapharm_Mobile.ViewModels.Auth;

namespace Cynapharm_Mobile.Views.Auth;

public partial class LoginPage : ContentPage
{
    private static readonly Color ActiveBorder   = Color.FromArgb("#1D9E75");
    private static readonly Color InactiveBorder = Color.FromArgb("#D3D3D3");

    public LoginPage() : this(MauiProgram.Services.GetRequiredService<LoginViewModel>()) { }

    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
    }

    private void EmailEntry_Focused(object sender, FocusEventArgs e)
        => EmailUnderline.BackgroundColor = ActiveBorder;

    private void EmailEntry_Unfocused(object sender, FocusEventArgs e)
        => EmailUnderline.BackgroundColor = InactiveBorder;

    private void PasswordEntry_Focused(object sender, FocusEventArgs e)
        => PasswordUnderline.BackgroundColor = ActiveBorder;

    private void PasswordEntry_Unfocused(object sender, FocusEventArgs e)
        => PasswordUnderline.BackgroundColor = InactiveBorder;
}
