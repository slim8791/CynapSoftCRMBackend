using Cynapharm_Mobile.ViewModels.Stock;

namespace Cynapharm_Mobile.Views.Stock;

public partial class MedecinEchantillonsPage : ContentPage
{
    public MedecinEchantillonsPage(MedecinEchantillonsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MedecinEchantillonsViewModel vm)
            vm.LoadDataCommand.Execute(null);
    }
}
