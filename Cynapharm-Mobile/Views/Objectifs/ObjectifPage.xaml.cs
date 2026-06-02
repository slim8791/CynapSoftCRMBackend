using Cynapharm_Mobile.ViewModels.Objectifs;

namespace Cynapharm_Mobile.Views.Objectifs;

public partial class ObjectifPage : ContentPage
{
    public ObjectifPage() : this(MauiProgram.Services.GetRequiredService<ObjectifViewModel>()) { }

    public ObjectifPage(ObjectifViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ObjectifViewModel vm) _ = vm.LoadCommand.ExecuteAsync(null);
    }

    private void OnHamburgerTapped(object sender, EventArgs e)
        => Shell.Current.FlyoutIsPresented = true;
}
