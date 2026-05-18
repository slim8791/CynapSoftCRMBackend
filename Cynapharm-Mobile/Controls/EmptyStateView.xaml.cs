namespace Cynapharm_Mobile.Controls;

public partial class EmptyStateView : ContentView
{
    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(string), typeof(EmptyStateView), "📭");

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(EmptyStateView), "Aucun résultat");

    public static readonly BindableProperty SubtitleProperty =
        BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(EmptyStateView),
            string.Empty, propertyChanged: (b, _, _) => ((EmptyStateView)b).OnPropertyChanged(nameof(HasSubtitle)));

    public static readonly BindableProperty ActionLabelProperty =
        BindableProperty.Create(nameof(ActionLabel), typeof(string), typeof(EmptyStateView),
            "Réessayer", propertyChanged: (b, _, _) => ((EmptyStateView)b).OnPropertyChanged(nameof(HasAction)));

    public static readonly BindableProperty ActionCommandProperty =
        BindableProperty.Create(nameof(ActionCommand), typeof(System.Windows.Input.ICommand),
            typeof(EmptyStateView), null,
            propertyChanged: (b, _, _) => ((EmptyStateView)b).OnPropertyChanged(nameof(HasAction)));

    public static readonly BindableProperty IsEmptyProperty =
        BindableProperty.Create(nameof(IsEmpty), typeof(bool), typeof(EmptyStateView),
            false, propertyChanged: OnIsEmptyChanged);

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public string ActionLabel
    {
        get => (string)GetValue(ActionLabelProperty);
        set => SetValue(ActionLabelProperty, value);
    }

    public System.Windows.Input.ICommand? ActionCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    public bool IsEmpty
    {
        get => (bool)GetValue(IsEmptyProperty);
        set => SetValue(IsEmptyProperty, value);
    }

    public bool HasSubtitle => !string.IsNullOrEmpty(Subtitle);
    public bool HasAction   => ActionCommand != null;

    public EmptyStateView() => InitializeComponent();

    private static void OnIsEmptyChanged(BindableObject bindable, object oldValue, object newValue)
        => ((EmptyStateView)bindable).IsVisible = (bool)newValue;
}
