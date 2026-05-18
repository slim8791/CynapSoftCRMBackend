using Cynapharm_Mobile.ViewModels.Rapports;

namespace Cynapharm_Mobile.Views.Rapports;

public partial class RapportPage : ContentPage
{
    public RapportPage(RapportViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is not RapportViewModel vm) return;

        // Load products (falls back to SQLite when offline)
        _ = vm.LoadProduitsCommand.ExecuteAsync(null);

        // Show the last known GPS position immediately — fast, no permission dialog.
        // The precise fix runs at submit time via CaptureLocationAsync.
        _ = vm.PreCaptureLocationCommand.ExecuteAsync(null);
    }
}
