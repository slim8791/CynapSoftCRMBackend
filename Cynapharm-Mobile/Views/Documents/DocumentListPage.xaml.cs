using Cynapharm_Mobile.ViewModels.Documents;

namespace Cynapharm_Mobile.Views.Documents;

public partial class DocumentListPage : ContentPage
{
    public DocumentListPage() : this(MauiProgram.Services.GetRequiredService<DocumentListViewModel>()) { }

    public DocumentListPage(DocumentListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DocumentListViewModel vm) _ = vm.LoadCommand.ExecuteAsync(null);
    }
}
