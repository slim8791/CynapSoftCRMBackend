namespace Cynapharm_Mobile.Controls;

public partial class AppLoader : ContentView
{
    public static readonly BindableProperty IsBusyProperty =
        BindableProperty.Create(nameof(IsBusy), typeof(bool), typeof(AppLoader), false);

    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    public AppLoader()
    {
        InitializeComponent();
    }
}
