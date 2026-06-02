using Cynapharm_Mobile.ViewModels.Profile;

namespace Cynapharm_Mobile.Views.Profile;

public partial class EditProfilePage : ContentPage
{
    private readonly EditProfileViewModel _vm;

    public EditProfilePage(EditProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _vm = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.LoadCommand.Execute(null);
    }
}
