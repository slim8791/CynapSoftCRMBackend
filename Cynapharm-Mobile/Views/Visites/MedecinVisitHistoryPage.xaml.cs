using Cynapharm_Mobile.ViewModels.Visites;

namespace Cynapharm_Mobile.Views.Visites;

public partial class MedecinVisitHistoryPage : ContentPage
{
    public MedecinVisitHistoryPage()
        : this(MauiProgram.Services.GetRequiredService<MedecinVisitHistoryViewModel>()) { }

    public MedecinVisitHistoryPage(MedecinVisitHistoryViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MedecinVisitHistoryViewModel vm)
            _ = vm.LoadVisitesCommand.ExecuteAsync(null);
    }
}
