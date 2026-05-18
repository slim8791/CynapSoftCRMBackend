using Cynapharm_Mobile.ViewModels.Profile;

namespace Cynapharm_Mobile.Views.Profile;

public partial class ProfilePage : ContentPage
{
    public ProfilePage() : this(MauiProgram.Services.GetRequiredService<ProfileViewModel>()) { }

    public ProfilePage(ProfileViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ProfileViewModel vm)
            await vm.LoadCommand.ExecuteAsync(null);
    }
}
