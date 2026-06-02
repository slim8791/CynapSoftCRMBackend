using Cynapharm_Mobile.ViewModels.Orders;

namespace Cynapharm_Mobile.Views.Orders;

public partial class CreateOrderPage : ContentPage
{
    public CreateOrderPage(CreateOrderViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override bool OnBackButtonPressed()
    {
        var vm = BindingContext as CreateOrderViewModel;
        // Warn if the user has progressed past step 1 (product selected)
        if (vm?.SelectedProduct != null)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                bool leave = await DisplayAlert(
                    "Commande en cours",
                    "Voulez-vous vraiment annuler la création de la commande ?",
                    "Annuler la commande", "Continuer");
                if (leave) await Shell.Current.GoToAsync("..");
            });
            return true;
        }
        return base.OnBackButtonPressed();
    }
}
