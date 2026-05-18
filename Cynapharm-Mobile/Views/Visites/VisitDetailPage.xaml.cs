using Cynapharm_Mobile.ViewModels.Visites;

namespace Cynapharm_Mobile.Views.Visites;

public partial class VisitDetailPage : ContentPage
{
    public VisitDetailPage(VisitDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override bool OnBackButtonPressed()
    {
        var vm = BindingContext as VisitDetailViewModel;
        if (vm?.IsDirty == true)
        {
            // Run async dialog on main thread without blocking the back-button handler
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                bool leave = await DisplayAlert(
                    "Modifications non enregistrées",
                    "Voulez-vous vraiment quitter ? Les modifications seront perdues.",
                    "Quitter", "Rester");
                if (leave) await Shell.Current.GoToAsync("..");
            });
            return true; // Intercept — we handle navigation ourselves
        }
        return base.OnBackButtonPressed();
    }
}
