using Cynapharm_Mobile.ViewModels.Clients;

namespace Cynapharm_Mobile.Views.Clients;

public partial class ClientFormPage : ContentPage
{
    public ClientFormPage(ClientFormViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
