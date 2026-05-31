using Cynapharm_Mobile.ViewModels.Clients;

namespace Cynapharm_Mobile.Views.Clients;

public partial class MesClientsPage : ContentPage
{
    public MesClientsPage(MesClientsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MesClientsViewModel vm)
            _ = vm.LoadCommand.ExecuteAsync(null);
    }
}
