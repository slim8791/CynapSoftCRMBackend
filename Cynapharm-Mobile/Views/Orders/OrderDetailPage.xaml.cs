using Cynapharm_Mobile.ViewModels.Orders;
namespace Cynapharm_Mobile.Views.Orders;
public partial class OrderDetailPage : ContentPage
{
    public OrderDetailPage(OrderDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is Cynapharm_Mobile.ViewModels.Orders.OrderDetailViewModel vm && vm.OrderId > 0)
            _ = vm.LoadCommand.ExecuteAsync(null);
    }
}
