using Cynapharm_Mobile.ViewModels.Visites;

namespace Cynapharm_Mobile.Views.Visites;

public partial class VisitListPage : ContentPage
{
    public VisitListPage() : this(MauiProgram.Services.GetRequiredService<VisitListViewModel>()) { }

    public VisitListPage(VisitListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is VisitListViewModel vm) _ = vm.LoadVisitesCommand.ExecuteAsync(null);
    }

    private void OnHamburgerTapped(object sender, EventArgs e)
        => Shell.Current.FlyoutIsPresented = true;
}
