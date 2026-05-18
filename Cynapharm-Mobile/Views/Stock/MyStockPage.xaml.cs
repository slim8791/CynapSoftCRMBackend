using Cynapharm_Mobile.ViewModels.Stock;

namespace Cynapharm_Mobile.Views.Stock;

public partial class MyStockPage : ContentPage
{
    public MyStockPage() : this(MauiProgram.Services.GetRequiredService<MyStockViewModel>()) { }

    public MyStockPage(MyStockViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MyStockViewModel vm) _ = vm.LoadCommand.ExecuteAsync(null);
    }
}
