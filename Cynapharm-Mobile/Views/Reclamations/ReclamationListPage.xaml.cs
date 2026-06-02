using Cynapharm_Mobile.ViewModels.Reclamations;

namespace Cynapharm_Mobile.Views.Reclamations;

public partial class ReclamationListPage : ContentPage
{
    public ReclamationListPage(ReclamationListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ReclamationListViewModel vm)
            _ = vm.LoadCommand.ExecuteAsync(null);
    }
}
