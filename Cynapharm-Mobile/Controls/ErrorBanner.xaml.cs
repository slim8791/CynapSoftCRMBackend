namespace Cynapharm_Mobile.Controls;

public partial class ErrorBanner : ContentView
{
    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(ErrorBanner),
            string.Empty, propertyChanged: OnMessageChanged);

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly BindableProperty DismissCommandProperty =
        BindableProperty.Create(nameof(DismissCommand), typeof(System.Windows.Input.ICommand),
            typeof(ErrorBanner));

    public System.Windows.Input.ICommand? DismissCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(DismissCommandProperty);
        set => SetValue(DismissCommandProperty, value);
    }

    public ErrorBanner()
    {
        InitializeComponent();
        DismissCommand = new Command(() =>
        {
            Message = string.Empty;
            IsVisible = false;
        });
    }

    private static void OnMessageChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var banner = (ErrorBanner)bindable;
        var msg = newValue as string;
        banner.IsVisible = !string.IsNullOrEmpty(msg);
    }
}
