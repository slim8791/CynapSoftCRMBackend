using Cynapharm_Mobile.ViewModels.Planning;

namespace Cynapharm_Mobile.Views.Planning;

public partial class PlanningPage : ContentPage
{
    public PlanningPage() : this(MauiProgram.Services.GetRequiredService<PlanningViewModel>()) { }

    public PlanningPage(PlanningViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PlanningViewModel vm) _ = vm.LoadWeekCommand.ExecuteAsync(null);
    }

    private void OnHamburgerTapped(object sender, EventArgs e)
        => Shell.Current.FlyoutIsPresented = true;
}
