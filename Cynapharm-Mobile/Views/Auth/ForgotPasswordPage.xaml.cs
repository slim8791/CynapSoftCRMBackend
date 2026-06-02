using Cynapharm_Mobile.ViewModels.Auth;

namespace Cynapharm_Mobile.Views.Auth;

public partial class ForgotPasswordPage : ContentPage
{
    private static readonly Color ActiveBorder   = Color.FromArgb("#1D9E75");
    private static readonly Color InactiveBorder = Color.FromArgb("#D3D3D3");

    public ForgotPasswordPage(ForgotPasswordViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    private void EmailEntry_Focused(object sender, FocusEventArgs e)
        => EmailUnderline.Color = ActiveBorder;

    private void EmailEntry_Unfocused(object sender, FocusEventArgs e)
        => EmailUnderline.Color = InactiveBorder;
}
