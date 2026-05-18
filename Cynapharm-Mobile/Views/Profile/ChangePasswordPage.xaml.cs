using Cynapharm_Mobile.ViewModels.Profile;

namespace Cynapharm_Mobile.Views.Profile;

public partial class ChangePasswordPage : ContentPage
{
    public ChangePasswordPage(ChangePasswordViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
