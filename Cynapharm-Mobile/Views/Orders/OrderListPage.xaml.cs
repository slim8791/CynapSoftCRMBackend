using Cynapharm_Mobile.ViewModels.Orders;

namespace Cynapharm_Mobile.Views.Orders;

public partial class OrderListPage : ContentPage
{
    public OrderListPage() : this(MauiProgram.Services.GetRequiredService<OrderListViewModel>()) { }

    public OrderListPage(OrderListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is OrderListViewModel vm) _ = vm.LoadCommand.ExecuteAsync(null);
    }

    private void OnHamburgerTapped(object sender, EventArgs e)
        => Shell.Current.FlyoutIsPresented = true;
}
