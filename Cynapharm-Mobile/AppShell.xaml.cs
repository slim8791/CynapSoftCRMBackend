using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Views.Auth;
using Cynapharm_Mobile.Views.Clients;
using Cynapharm_Mobile.Views.Documents;
using Cynapharm_Mobile.Views.Orders;
using Cynapharm_Mobile.Views.Planning;
using Cynapharm_Mobile.Views.Products;
using Cynapharm_Mobile.Views.Profile;
using Cynapharm_Mobile.Views.Rapports;
using Cynapharm_Mobile.Views.Visites;

namespace Cynapharm_Mobile;

public partial class AppShell : Shell
{
    // ── Role string (raw, from SecureStorage) ─────────────────────────────
    public string Role { get; private set; } = string.Empty;

    // ── Role computed properties ──────────────────────────────────────────
    // SUPERVISEUR role is deprecated in UI. Treated as DELEGUE for navigation.
    public bool IsDelegue => Role is "DELEGUE" or "ADMIN" or "SUPERVISEUR";
    public bool IsClient  => Role is "PHARMACIEN" or "GROSSISTE" or "CLIENT";
    public bool IsMedecin => Role is "MEDECIN";

    // ── Flyout role visibility (custom panel) ────────────────────────────
    public bool ShowDashboard    { get; private set; }
    public bool ShowVisites      { get; private set; }
    public bool ShowMedecinVisites { get; private set; }
    public bool ShowPlanning     { get; private set; }
    public bool ShowCatalogue    { get; private set; } = true;
    public bool ShowOrders       { get; private set; }
    public bool ShowDocuments    { get; private set; }
    public bool ShowReclamations { get; private set; }
    public bool ShowStock        { get; private set; }
    public bool ShowObjectifs    { get; private set; }
    public bool ShowClients      { get; private set; }

    // ── User display info for flyout header ──────────────────────────────
    public string UserName     { get; private set; } = string.Empty;
    public string UserInitials { get; private set; } = "?";
    public string UserRole     { get; private set; } = string.Empty;

    // ── Navigation commands ───────────────────────────────────────────────
    public IAsyncRelayCommand GoToDashboardCommand    { get; }
    public IAsyncRelayCommand GoToVisitesCommand      { get; }
    public IAsyncRelayCommand GoToMedecinVisitesCommand { get; }
    public IAsyncRelayCommand GoToPlanningCommand     { get; }
    public IAsyncRelayCommand GoToCatalogueCommand    { get; }
    public IAsyncRelayCommand GoToOrdersCommand       { get; }
    public IAsyncRelayCommand GoToDocumentsCommand    { get; }
    public IAsyncRelayCommand GoToReclamationsCommand { get; }
    public IAsyncRelayCommand GoToStockCommand        { get; }
    public IAsyncRelayCommand GoToObjectifsCommand    { get; }
    public IAsyncRelayCommand GoToProfileCommand      { get; }
    public IAsyncRelayCommand GoToClientsCommand      { get; }

    public AppShell()
    {
        GoToDashboardCommand    = new AsyncRelayCommand(() => Navigate("//dashboard"));
        GoToVisitesCommand      = new AsyncRelayCommand(() => Navigate("//visits"));
        GoToMedecinVisitesCommand = new AsyncRelayCommand(() => Navigate("//medecinvisits"));
        GoToPlanningCommand     = new AsyncRelayCommand(() => Navigate("//planning"));
        GoToCatalogueCommand    = new AsyncRelayCommand(() => Navigate("//products"));
        GoToOrdersCommand       = new AsyncRelayCommand(() => Navigate("//orders"));
        GoToDocumentsCommand    = new AsyncRelayCommand(() => Navigate("//documents"));
        GoToReclamationsCommand = new AsyncRelayCommand(() => Navigate("//reclamations"));
        GoToStockCommand        = new AsyncRelayCommand(() => Navigate("//stock"));
        GoToObjectifsCommand    = new AsyncRelayCommand(() => Navigate("//objectifs"));
        GoToProfileCommand      = new AsyncRelayCommand(() => Navigate("//profile"));
        GoToClientsCommand      = new AsyncRelayCommand(() => Navigate("//clients"));

        BindingContext = this;
        InitializeComponent();

        Routing.RegisterRoute("forgotpassword",    typeof(ForgotPasswordPage));
        Routing.RegisterRoute("visits/detail",     typeof(VisitDetailPage));
        Routing.RegisterRoute("visits/rapport",    typeof(RapportPage));
        Routing.RegisterRoute("products/detail",        typeof(ProductDetailPage));
        Routing.RegisterRoute("products/detail/viewer", typeof(DocumentViewerPage));
        Routing.RegisterRoute("orders/detail",          typeof(OrderDetailPage));
        Routing.RegisterRoute("orders/create",     typeof(CreateOrderPage));
        Routing.RegisterRoute("documents/detail",      typeof(DocumentDetailPage));
        Routing.RegisterRoute("profile/edit",           typeof(EditProfilePage));
        Routing.RegisterRoute("profile/changepassword", typeof(ChangePasswordPage));
        Routing.RegisterRoute("clients/detail",         typeof(ClientDetailPage));
        Routing.RegisterRoute("clients/form",           typeof(ClientFormPage));
        Routing.RegisterRoute("planning/form",          typeof(PlanningFormPage));
    }

    private async Task Navigate(string route)
    {
        FlyoutIsPresented = false;
        await GoToAsync(route);
    }

    public void ApplyRoleVisibility(string role)
    {
        Role = role;

        // SUPERVISEUR role is deprecated in UI. Treated as DELEGUE for navigation.
        bool isDelegue = role is "DELEGUE" or "ADMIN" or "SUPERVISEUR";
        bool isClient  = role is "PHARMACIEN" or "GROSSISTE" or "CLIENT";
        bool isMedecin = role is "MEDECIN";

        ShowDashboard    = isDelegue;
        ShowVisites      = isDelegue;
        ShowMedecinVisites = isMedecin;
        ShowPlanning     = isDelegue;
        ShowCatalogue    = isDelegue || isClient || isMedecin;
        ShowOrders       = isClient || isDelegue;
        ShowDocuments    = isClient;
        ShowReclamations = isClient;
        ShowStock        = isDelegue;
        ShowObjectifs    = isDelegue;
        ShowClients      = isDelegue;

        // User display for flyout header (loaded from SecureStorage)
        _ = LoadUserInfoAsync();

        NotifyAll();

        // Flyout enabled for every role. MEDECIN gets a reduced menu
        // (Mes visites + Catalogue + Profil) via the Show* flags below.
        Shell.SetFlyoutBehavior(this, FlyoutBehavior.Flyout);

        // Tab bar visibility — role-specific tabs; Catalogue and Profil tabs are always visible
        if (FlyoutDashboard  is not null) FlyoutDashboard.IsVisible  = isDelegue;
        if (FlyoutVisites    is not null) FlyoutVisites.IsVisible    = isDelegue;
        if (FlyoutMedecinVisites is not null) FlyoutMedecinVisites.IsVisible = isMedecin;
        if (FlyoutPlanning   is not null) FlyoutPlanning.IsVisible   = isDelegue;
        if (FlyoutOrders     is not null) FlyoutOrders.IsVisible     = isClient || isDelegue;
        if (FlyoutDocuments  is not null) FlyoutDocuments.IsVisible  = isClient;
        // Secondary page route accessibility (FlyoutItem, not in tab bar)
        if (FlyoutStock        is not null) FlyoutStock.IsVisible        = isDelegue;
        if (FlyoutObjectifs    is not null) FlyoutObjectifs.IsVisible    = isDelegue;
        if (FlyoutReclamations is not null) FlyoutReclamations.IsVisible = isClient;
        if (FlyoutClients      is not null) FlyoutClients.IsVisible      = isDelegue;
    }

    private async Task LoadUserInfoAsync()
    {
        try
        {
            var name = await SecureStorage.GetAsync(StorageKeys.UserName) ?? string.Empty;
            var role = await SecureStorage.GetAsync(StorageKeys.UserRole) ?? string.Empty;

            UserName = name;
            UserRole = role;
            UserInitials = BuildInitials(name);

            OnPropertyChanged(nameof(UserName));
            OnPropertyChanged(nameof(UserRole));
            OnPropertyChanged(nameof(UserInitials));
        }
        catch { }
    }

    private static string BuildInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2
            ? $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant()
            : name[0].ToString().ToUpperInvariant();
    }

    private void NotifyAll()
    {
        OnPropertyChanged(nameof(Role));
        OnPropertyChanged(nameof(IsDelegue));
        OnPropertyChanged(nameof(IsClient));
        OnPropertyChanged(nameof(IsMedecin));
        OnPropertyChanged(nameof(ShowDashboard));
        OnPropertyChanged(nameof(ShowVisites));
        OnPropertyChanged(nameof(ShowMedecinVisites));
        OnPropertyChanged(nameof(ShowPlanning));
        OnPropertyChanged(nameof(ShowCatalogue));
        OnPropertyChanged(nameof(ShowOrders));
        OnPropertyChanged(nameof(ShowDocuments));
        OnPropertyChanged(nameof(ShowReclamations));
        OnPropertyChanged(nameof(ShowStock));
        OnPropertyChanged(nameof(ShowObjectifs));
        OnPropertyChanged(nameof(ShowClients));
    }
}
