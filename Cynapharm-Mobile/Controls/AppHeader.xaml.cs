using System.Windows.Input;

namespace Cynapharm_Mobile.Controls;

public partial class AppHeader : ContentView
{
    // ── Title ─────────────────────────────────────────────────────────────────
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(AppHeader), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // ── Subtitle ──────────────────────────────────────────────────────────────
    public static readonly BindableProperty SubtitleProperty =
        BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(AppHeader), string.Empty,
            propertyChanged: (b, _, n) => ((AppHeader)b).HasSubtitle = !string.IsNullOrEmpty(n as string));

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    // ── HasSubtitle (computed, read-only from outside) ─────────────────────
    public static readonly BindableProperty HasSubtitleProperty =
        BindableProperty.Create(nameof(HasSubtitle), typeof(bool), typeof(AppHeader), false);

    public bool HasSubtitle
    {
        get => (bool)GetValue(HasSubtitleProperty);
        private set => SetValue(HasSubtitleProperty, value);
    }

    // ── ShowBackButton ─────────────────────────────────────────────────────
    public static readonly BindableProperty ShowBackButtonProperty =
        BindableProperty.Create(nameof(ShowBackButton), typeof(bool), typeof(AppHeader), false);

    public bool ShowBackButton
    {
        get => (bool)GetValue(ShowBackButtonProperty);
        set => SetValue(ShowBackButtonProperty, value);
    }

    // ── BackCommand (optional — falls back to Shell.GoToAsync("..")) ───────
    public static readonly BindableProperty BackCommandProperty =
        BindableProperty.Create(nameof(BackCommand), typeof(ICommand), typeof(AppHeader));

    public ICommand? BackCommand
    {
        get => (ICommand?)GetValue(BackCommandProperty);
        set => SetValue(BackCommandProperty, value);
    }

    // ── ShowHamburger ──────────────────────────────────────────────────────
    public static readonly BindableProperty ShowHamburgerProperty =
        BindableProperty.Create(nameof(ShowHamburger), typeof(bool), typeof(AppHeader), false,
            propertyChanged: (b, _, _) => ((AppHeader)b).UpdateRightButton());

    public bool ShowHamburger
    {
        get => (bool)GetValue(ShowHamburgerProperty);
        set => SetValue(ShowHamburgerProperty, value);
    }

    // ── RightIconGlyph (Unicode char or empty) ─────────────────────────────
    public static readonly BindableProperty RightIconGlyphProperty =
        BindableProperty.Create(nameof(RightIconGlyph), typeof(string), typeof(AppHeader), string.Empty,
            propertyChanged: (b, _, _) => ((AppHeader)b).UpdateRightButton());

    public string RightIconGlyph
    {
        get => (string)GetValue(RightIconGlyphProperty);
        set => SetValue(RightIconGlyphProperty, value);
    }

    // ── RightIconFontFamily (e.g. "Tabler" for icon fonts) ─────────────────
    public static readonly BindableProperty RightIconFontFamilyProperty =
        BindableProperty.Create(nameof(RightIconFontFamily), typeof(string), typeof(AppHeader), null,
            propertyChanged: (b, _, _) => ((AppHeader)b).UpdateRightButton());

    public string? RightIconFontFamily
    {
        get => (string?)GetValue(RightIconFontFamilyProperty);
        set => SetValue(RightIconFontFamilyProperty, value);
    }

    // ── RightCommand ───────────────────────────────────────────────────────
    public static readonly BindableProperty RightCommandProperty =
        BindableProperty.Create(nameof(RightCommand), typeof(ICommand), typeof(AppHeader));

    public ICommand? RightCommand
    {
        get => (ICommand?)GetValue(RightCommandProperty);
        set => SetValue(RightCommandProperty, value);
    }

    // ── HeaderBackground (defaults to Primary at runtime) ─────────────────
    public static readonly BindableProperty HeaderBackgroundProperty =
        BindableProperty.Create(nameof(HeaderBackground), typeof(Color), typeof(AppHeader),
            defaultValueCreator: _ =>
                Application.Current?.Resources.TryGetValue("Primary", out var c) == true
                    ? c as Color
                    : Colors.CornflowerBlue);

    public Color? HeaderBackground
    {
        get => (Color?)GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    // ── ShowRightButton (computed) ─────────────────────────────────────────
    public static readonly BindableProperty ShowRightButtonProperty =
        BindableProperty.Create(nameof(ShowRightButton), typeof(bool), typeof(AppHeader), false);

    public bool ShowRightButton
    {
        get => (bool)GetValue(ShowRightButtonProperty);
        private set => SetValue(ShowRightButtonProperty, value);
    }

    // ─────────────────────────────────────────────────────────────────────────

    public AppHeader()
    {
        InitializeComponent();
    }

    private void UpdateRightButton()
    {
        ShowRightButton = !string.IsNullOrEmpty(RightIconGlyph);
    }

    private void OnBackTapped(object sender, EventArgs e)
    {
        if (BackCommand?.CanExecute(null) == true)
            BackCommand.Execute(null);
        else
            _ = Shell.Current.GoToAsync("..");
    }

    private void OnHamburgerTapped(object sender, EventArgs e)
    {
        Shell.Current.FlyoutIsPresented = true;
    }

    private void OnRightTapped(object sender, EventArgs e)
    {
        RightCommand?.Execute(null);
    }
}
