using Cynapharm_Mobile.ViewModels.Clients;

namespace Cynapharm_Mobile.Views.Clients;

public partial class ClientDetailPage : ContentPage
{
    public ClientDetailPage(ClientDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
