using Cynapharm_Mobile.ViewModels.Products;
namespace Cynapharm_Mobile.Views.Products;
public partial class ProductDetailPage : ContentPage
{
    public ProductDetailPage(ProductDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
