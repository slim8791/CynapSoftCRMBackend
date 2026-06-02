using Cynapharm_Mobile.ViewModels.Products;

namespace Cynapharm_Mobile.Views.Products;

public partial class ProductListPage : ContentPage
{
    private bool _loaded;

    public ProductListPage() : this(MauiProgram.Services.GetRequiredService<ProductListViewModel>()) { }

    public ProductListPage(ProductListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is not ProductListViewModel vm) return;

        // Load on first appear, or retry if the previous load returned nothing.
        // This lets the user come back to the page and have it auto-retry after
        // a connectivity or API issue without having to pull-to-refresh manually.
        if (!_loaded || vm.Products.Count == 0)
        {
            _loaded = true;
            _ = vm.LoadCommand.ExecuteAsync(null);
        }
    }

    private void OnHamburgerTapped(object sender, EventArgs e)
        => Shell.Current.FlyoutIsPresented = true;
}
