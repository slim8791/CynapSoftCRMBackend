using Cynapharm_Mobile.ViewModels.Planning;

namespace Cynapharm_Mobile.Views.Planning;

public partial class PlanningFormPage : ContentPage
{
    public PlanningFormPage(PlanningFormViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PlanningFormViewModel vm)
            _ = vm.InitCommand.ExecuteAsync(null);
    }
}
