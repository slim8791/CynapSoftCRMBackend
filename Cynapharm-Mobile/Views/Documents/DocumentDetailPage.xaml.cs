using Cynapharm_Mobile.ViewModels.Documents;

namespace Cynapharm_Mobile.Views.Documents;

public partial class DocumentDetailPage : ContentPage
{
    public DocumentDetailPage(DocumentDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
