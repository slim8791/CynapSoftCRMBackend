using Cynapharm_Mobile.ViewModels.Dashboard;

namespace Cynapharm_Mobile.Views.Dashboard;

public partial class DashboardPage : ContentPage
{
    public DashboardPage() : this(MauiProgram.Services.GetRequiredService<DashboardViewModel>()) { }

    public DashboardPage(DashboardViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DashboardViewModel vm) _ = vm.LoadDashboardCommand.ExecuteAsync(null);
    }

    private void OnHamburgerTapped(object sender, EventArgs e)
        => Shell.Current.FlyoutIsPresented = true;
}
