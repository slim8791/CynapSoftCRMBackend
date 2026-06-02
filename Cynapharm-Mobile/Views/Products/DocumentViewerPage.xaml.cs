using Cynapharm_Mobile.ViewModels.Products;

namespace Cynapharm_Mobile.Views.Products;

public partial class DocumentViewerPage : ContentPage
{
    public DocumentViewerPage(DocumentViewerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
