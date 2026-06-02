# MAUI_VISION_COMPLETE — Cynapharm Mobile
### Complete Code Archive + Role-Scenario Analysis + Bug & Fix Plan

---

# PARTIE 0 — CODE COMPLET DE TOUS LES FICHIERS

## StorageKeys.cs

```csharp
namespace Cynapharm_Mobile;

public static class StorageKeys
{
    public const string JwtToken    = "jwt_token";
    public const string TokenExpiry = "jwt_expiry";
    public const string UserRole    = "user_role";
    public const string UserId      = "user_id";
    public const string UserName    = "user_name";
    public const string UserEmail   = "user_email";

    public static string UserTelephone(string userId) => $"user_telephone_{userId}";
    public static string UserAdresse(string userId)   => $"user_adresse_{userId}";
}
```

---

## MauiProgram.cs

```csharp
using CommunityToolkit.Maui;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Auth;
using Cynapharm_Mobile.ViewModels.Dashboard;
using Cynapharm_Mobile.ViewModels.Documents;
using Cynapharm_Mobile.ViewModels.Objectifs;
using Cynapharm_Mobile.ViewModels.Orders;
using Cynapharm_Mobile.ViewModels.Reclamations;
using Cynapharm_Mobile.ViewModels.Planning;
using Cynapharm_Mobile.ViewModels.Products;
using Cynapharm_Mobile.ViewModels.Profile;
using Cynapharm_Mobile.ViewModels.Rapports;
using Cynapharm_Mobile.ViewModels.Stock;
using Cynapharm_Mobile.ViewModels.Visites;
using Cynapharm_Mobile.Views.Auth;
using Cynapharm_Mobile.Views.Dashboard;
using Cynapharm_Mobile.Views.Documents;
using Cynapharm_Mobile.Views.Objectifs;
using Cynapharm_Mobile.Views.Orders;
using Cynapharm_Mobile.Views.Reclamations;
using Cynapharm_Mobile.Views.Planning;
using Cynapharm_Mobile.Views.Products;
using Cynapharm_Mobile.Views.Profile;
using Cynapharm_Mobile.Views.Rapports;
using Cynapharm_Mobile.Views.Stock;
using Cynapharm_Mobile.Views.Visites;
using Microsoft.Extensions.Logging;
using Polly;

namespace Cynapharm_Mobile;

public static class MauiProgram
{
    public static IServiceProvider Services { get; private set; } = null!;

    public static MauiApp CreateMauiApp()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            CrashLogger.Log("AppDomain.UnhandledException", ex ?? new Exception(e.ExceptionObject?.ToString()));
            Services?.GetService<Cynapharm_Mobile.Services.IAppLogger>()
                    ?.LogError("UnhandledException", ex, "AppDomain");
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            CrashLogger.Log("TaskScheduler.UnobservedTaskException", e.Exception);
            Services?.GetService<Cynapharm_Mobile.Services.IAppLogger>()
                    ?.LogError("UnobservedTaskException", e.Exception, "TaskScheduler");
            e.SetObserved();
        };

        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("tabler-icons.ttf", "Tabler");
            });

        var settings = LoadAppSettings();
#if RELEASE
        var baseUrl = !string.IsNullOrEmpty(settings.ApiGatewayBaseUrlProd)
            ? settings.ApiGatewayBaseUrlProd
            : settings.ApiGatewayBaseUrl;
#else
        var baseUrl = settings.ApiGatewayBaseUrl;
#endif

        builder.Services.AddTransient<HttpLoggingHandler>();
        builder.Services.AddTransient<TokenValidationHandler>();

        builder.Services.AddHttpClient<ApiService>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("X-Client-Type", "mobile");
            client.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
        })
        .AddHttpMessageHandler<TokenValidationHandler>()
        .AddHttpMessageHandler<HttpLoggingHandler>()
        .AddStandardResilienceHandler(options =>
        {
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);
            options.Retry.MaxRetryAttempts = 3;
            options.Retry.BackoffType      = Polly.DelayBackoffType.Exponential;
            options.Retry.Delay            = TimeSpan.FromSeconds(1);
            options.CircuitBreaker.FailureRatio     = 0.5;
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
        });

        builder.Services.AddSingleton<LocalDatabaseService>();
        builder.Services.AddSingleton<IAppLogger, AppLogger>();
        builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
        builder.Services.AddSingleton<INavigationService, ShellNavigationService>();
        builder.Services.AddSingleton<SyncService>();
        builder.Services.AddSingleton<AuthService>();

        builder.Services.AddTransient<ProductService>();
        builder.Services.AddTransient<OrderService>();
        builder.Services.AddTransient<InventoryService>();
        builder.Services.AddTransient<VisiteService>();
        builder.Services.AddTransient<PlanningService>();
        builder.Services.AddTransient<KpiService>();
        builder.Services.AddTransient<DocumentService>();
        builder.Services.AddTransient<UserService>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<ForgotPasswordViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<VisitListViewModel>();
        builder.Services.AddTransient<VisitDetailViewModel>();
        builder.Services.AddTransient<PlanningViewModel>();
        builder.Services.AddTransient<RapportViewModel>();
        builder.Services.AddTransient<MyStockViewModel>();
        builder.Services.AddTransient<ObjectifViewModel>();
        builder.Services.AddTransient<ProductListViewModel>();
        builder.Services.AddTransient<ProductDetailViewModel>();
        builder.Services.AddTransient<DocumentViewerViewModel>();
        builder.Services.AddTransient<OrderListViewModel>();
        builder.Services.AddTransient<OrderDetailViewModel>();
        builder.Services.AddTransient<CreateOrderViewModel>();
        builder.Services.AddTransient<DocumentListViewModel>();
        builder.Services.AddTransient<DocumentDetailViewModel>();
        builder.Services.AddTransient<ReclamationListViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<EditProfileViewModel>();
        builder.Services.AddTransient<ChangePasswordViewModel>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<ForgotPasswordPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<VisitListPage>();
        builder.Services.AddTransient<VisitDetailPage>();
        builder.Services.AddTransient<PlanningPage>();
        builder.Services.AddTransient<RapportPage>();
        builder.Services.AddTransient<MyStockPage>();
        builder.Services.AddTransient<ObjectifPage>();
        builder.Services.AddTransient<ProductListPage>();
        builder.Services.AddTransient<ProductDetailPage>();
        builder.Services.AddTransient<DocumentViewerPage>();
        builder.Services.AddTransient<OrderListPage>();
        builder.Services.AddTransient<OrderDetailPage>();
        builder.Services.AddTransient<CreateOrderPage>();
        builder.Services.AddTransient<DocumentListPage>();
        builder.Services.AddTransient<DocumentDetailPage>();
        builder.Services.AddTransient<ReclamationListPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<EditProfilePage>();
        builder.Services.AddTransient<ChangePasswordPage>();
        builder.Services.AddTransient<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("RemoveUnderline", (handler, view) =>
        {
#if ANDROID
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
        });

        var app = builder.Build();
        Services = app.Services;
        return app;
    }

    private static AppSettings LoadAppSettings()
    {
        try
        {
            using var stream = FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult();
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            return System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }
}
```

---

## AppShell.xaml.cs

```csharp
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Views.Auth;
using Cynapharm_Mobile.Views.Documents;
using Cynapharm_Mobile.Views.Orders;
using Cynapharm_Mobile.Views.Products;
using Cynapharm_Mobile.Views.Profile;
using Cynapharm_Mobile.Views.Rapports;
using Cynapharm_Mobile.Views.Visites;

namespace Cynapharm_Mobile;

public partial class AppShell : Shell
{
    public string Role { get; private set; } = string.Empty;

    public bool IsDelegue => Role is "DELEGUE" or "ADMIN" or "SUPERVISEUR";
    public bool IsClient  => Role is "PHARMACIEN" or "GROSSISTE" or "CLIENT";
    public bool IsMedecin => Role is "MEDECIN";

    public bool ShowDashboard    { get; private set; }
    public bool ShowVisites      { get; private set; }
    public bool ShowPlanning     { get; private set; }
    public bool ShowCatalogue    { get; private set; } = true;
    public bool ShowOrders       { get; private set; }
    public bool ShowDocuments    { get; private set; }
    public bool ShowReclamations { get; private set; }
    public bool ShowStock        { get; private set; }
    public bool ShowObjectifs    { get; private set; }

    public string UserName     { get; private set; } = string.Empty;
    public string UserInitials { get; private set; } = "?";
    public string UserRole     { get; private set; } = string.Empty;

    public IAsyncRelayCommand GoToDashboardCommand    { get; }
    public IAsyncRelayCommand GoToVisitesCommand      { get; }
    public IAsyncRelayCommand GoToPlanningCommand     { get; }
    public IAsyncRelayCommand GoToCatalogueCommand    { get; }
    public IAsyncRelayCommand GoToOrdersCommand       { get; }
    public IAsyncRelayCommand GoToDocumentsCommand    { get; }
    public IAsyncRelayCommand GoToReclamationsCommand { get; }
    public IAsyncRelayCommand GoToStockCommand        { get; }
    public IAsyncRelayCommand GoToObjectifsCommand    { get; }
    public IAsyncRelayCommand GoToProfileCommand      { get; }

    public AppShell()
    {
        GoToDashboardCommand    = new AsyncRelayCommand(() => Navigate("//dashboard"));
        GoToVisitesCommand      = new AsyncRelayCommand(() => Navigate("//visits"));
        GoToPlanningCommand     = new AsyncRelayCommand(() => Navigate("//planning"));
        GoToCatalogueCommand    = new AsyncRelayCommand(() => Navigate("//products"));
        GoToOrdersCommand       = new AsyncRelayCommand(() => Navigate("//orders"));
        GoToDocumentsCommand    = new AsyncRelayCommand(() => Navigate("//documents"));
        GoToReclamationsCommand = new AsyncRelayCommand(() => Navigate("//reclamations"));
        GoToStockCommand        = new AsyncRelayCommand(() => Navigate("//stock"));
        GoToObjectifsCommand    = new AsyncRelayCommand(() => Navigate("//objectifs"));
        GoToProfileCommand      = new AsyncRelayCommand(() => Navigate("//profile"));

        BindingContext = this;
        InitializeComponent();

        Routing.RegisterRoute("forgotpassword",          typeof(ForgotPasswordPage));
        Routing.RegisterRoute("visits/detail",           typeof(VisitDetailPage));
        Routing.RegisterRoute("visits/rapport",          typeof(RapportPage));
        Routing.RegisterRoute("products/detail",         typeof(ProductDetailPage));
        Routing.RegisterRoute("products/detail/viewer",  typeof(DocumentViewerPage));
        Routing.RegisterRoute("orders/detail",           typeof(OrderDetailPage));
        Routing.RegisterRoute("orders/create",           typeof(CreateOrderPage));
        Routing.RegisterRoute("documents/detail",        typeof(DocumentDetailPage));
        Routing.RegisterRoute("profile/edit",            typeof(EditProfilePage));
        Routing.RegisterRoute("profile/changepassword",  typeof(ChangePasswordPage));
    }

    private async Task Navigate(string route)
    {
        FlyoutIsPresented = false;
        await GoToAsync(route);
    }

    public void ApplyRoleVisibility(string role)
    {
        Role = role;
        bool isDelegue = role is "DELEGUE" or "ADMIN" or "SUPERVISEUR";
        bool isClient  = role is "PHARMACIEN" or "GROSSISTE" or "CLIENT";
        bool isMedecin = role is "MEDECIN";

        ShowDashboard    = isDelegue;
        ShowVisites      = isDelegue;
        ShowPlanning     = isDelegue;
        ShowCatalogue    = isDelegue || isClient || isMedecin;
        ShowOrders       = isClient || isDelegue;
        ShowDocuments    = isClient;
        ShowReclamations = isClient;
        ShowStock        = isDelegue;
        ShowObjectifs    = isDelegue;

        _ = LoadUserInfoAsync();
        NotifyAll();

        Shell.SetFlyoutBehavior(this, isMedecin ? FlyoutBehavior.Disabled : FlyoutBehavior.Flyout);

        if (FlyoutDashboard  is not null) FlyoutDashboard.IsVisible  = isDelegue;
        if (FlyoutVisites    is not null) FlyoutVisites.IsVisible    = isDelegue;
        if (FlyoutPlanning   is not null) FlyoutPlanning.IsVisible   = isDelegue;
        if (FlyoutOrders     is not null) FlyoutOrders.IsVisible     = isClient || isDelegue;
        if (FlyoutDocuments  is not null) FlyoutDocuments.IsVisible  = isClient;
        if (FlyoutStock        is not null) FlyoutStock.IsVisible        = isDelegue;
        if (FlyoutObjectifs    is not null) FlyoutObjectifs.IsVisible    = isDelegue;
        if (FlyoutReclamations is not null) FlyoutReclamations.IsVisible = isClient;
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
        OnPropertyChanged(nameof(ShowPlanning));
        OnPropertyChanged(nameof(ShowCatalogue));
        OnPropertyChanged(nameof(ShowOrders));
        OnPropertyChanged(nameof(ShowDocuments));
        OnPropertyChanged(nameof(ShowReclamations));
        OnPropertyChanged(nameof(ShowStock));
        OnPropertyChanged(nameof(ShowObjectifs));
    }
}
```

---

## ViewModels/Auth/LoginViewModel.cs

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Auth;

public partial class LoginViewModel : BaseViewModel
{
    private readonly AuthService _authService;

    [ObservableProperty] private string _email    = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private bool   _showPassword = false;

    public LoginViewModel(AuthService authService)
    {
        _authService = authService;
        Title = "Connexion";
    }

    [RelayCommand]
    private Task ToggleShowPasswordAsync() { ShowPassword = !ShowPassword; return Task.CompletedTask; }

    [RelayCommand]
    private Task LoginAsync() => ExecuteAsync(async () =>
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Veuillez remplir tous les champs.";
            return;
        }

        var result = await _authService.LoginAsync(Email.Trim(), Password);
        if (result == null)
        {
            ErrorMessage = "Email ou mot de passe incorrect.";
            return;
        }

        var shell = (AppShell)Shell.Current;
        shell.ApplyRoleVisibility(result.Role);

        var role = result.Role;
        if (role is "DELEGUE" or "ADMIN" or "SUPERVISEUR")
            await Shell.Current.GoToAsync("//dashboard");
        else if (role is "PHARMACIEN" or "GROSSISTE" or "CLIENT")
            await Shell.Current.GoToAsync("//orders");
        else if (role is "MEDECIN")
            await Shell.Current.GoToAsync("//products");
        else
            await Shell.Current.GoToAsync("//products");
    });

    [RelayCommand]
    private Task GoToForgotPasswordAsync()
        => Shell.Current.GoToAsync("forgotpassword");
}
```

---

## ViewModels/Base/BaseViewModel.cs

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace Cynapharm_Mobile.ViewModels.Base;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty] private bool   _isBusy       = false;
    [ObservableProperty] private bool   _isRefreshing = false;
    [ObservableProperty] private string _title        = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;

    protected async Task ExecuteAsync(Func<Task> action)
    {
        if (IsBusy) return;
        IsBusy       = true;
        ErrorMessage = string.Empty;
        try
        {
            await action();
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Erreur réseau. Vérifiez votre connexion.";
        }
        catch (TaskCanceledException)
        {
            ErrorMessage = "La requête a expiré. Réessayez.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected async Task ExecuteUncheckedAsync(Func<Task> action)
    {
        ErrorMessage = string.Empty;
        try { await action(); }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }

    protected async Task<bool> CheckConnectivityAsync()
    {
        if (Connectivity.NetworkAccess == NetworkAccess.Internet) return true;
        await Snackbar.Make("Pas de connexion internet.", null, "OK",
            TimeSpan.FromSeconds(3),
            new SnackbarOptions { BackgroundColor = Colors.DarkRed, TextColor = Colors.White })
            .Show();
        return false;
    }

    protected async Task SaveCacheAsync<T>(string key, T data)
    {
        try
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, $"{key}.json");
            var json = System.Text.Json.JsonSerializer.Serialize(data);
            await File.WriteAllTextAsync(path, json);
        }
        catch { }
    }

    protected async Task<T?> LoadCacheAsync<T>(string key)
    {
        try
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, $"{key}.json");
            if (!File.Exists(path)) return default;
            var json = await File.ReadAllTextAsync(path);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }
        catch { return default; }
    }
}
```

---

## ViewModels/Dashboard/DashboardViewModel.cs

```csharp
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Field;
using Cynapharm_Mobile.Models.Inventory;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Dashboard;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly KpiService       _kpiService;
    private readonly VisiteService    _visiteService;
    private readonly InventoryService _inventoryService;

    [ObservableProperty] private string _greetingName  = string.Empty;
    [ObservableProperty] private string _userRole      = string.Empty;
    [ObservableProperty] private int    _todayVisitCount = 0;
    [ObservableProperty] private double _tauxConversion = 0;
    [ObservableProperty] private StockSummary? _stockSummary;

    [ObservableProperty]
    private ObservableCollection<PerformanceDto> _performanceItems = new();

    [ObservableProperty]
    private ObservableCollection<Objectif> _objectifItems = new();

    [ObservableProperty]
    private ObservableCollection<Region> _regions = new();

    public bool IsSuperviseur => UserRole == "SUPERVISEUR";

    public DashboardViewModel(KpiService kpiService, VisiteService visiteService, InventoryService inventoryService)
    {
        _kpiService       = kpiService;
        _visiteService    = visiteService;
        _inventoryService = inventoryService;
        Title = "Tableau de bord";
    }

    [RelayCommand]
    private Task LoadDashboardAsync() => ExecuteAsync(async () =>
    {
        var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
        var role      = await SecureStorage.GetAsync(StorageKeys.UserRole) ?? string.Empty;
        var name      = await SecureStorage.GetAsync(StorageKeys.UserName) ?? string.Empty;

        GreetingName = name.Split(' ').FirstOrDefault() ?? name;
        UserRole     = role;

        var today      = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        if (!int.TryParse(userIdStr, out var userId)) return;

        var perfTask    = _kpiService.GetPerformanceAsync(userId, monthStart, today);
        var objTask     = _kpiService.GetObjectifsAsync(userId);

        if (IsSuperviseur)
        {
            var regionTask = _kpiService.GetRegionsAsync();
            await Task.WhenAll(perfTask, objTask, regionTask);
            Regions.Clear();
            foreach (var r in regionTask.Result ?? new()) Regions.Add(r);
        }
        else
        {
            var visitTask  = _visiteService.GetVisitesAsync(today, today, null);
            var convTask   = _kpiService.GetTauxConversionAsync(userId, monthStart, today);
            var stockTask  = _inventoryService.GetStockSummaryAsync(userId);
            await Task.WhenAll(perfTask, objTask, visitTask, convTask, stockTask);

            TodayVisitCount = visitTask.Result?.Count ?? 0;
            TauxConversion  = convTask.Result;
            StockSummary    = stockTask.Result;
        }

        PerformanceItems.Clear();
        foreach (var p in perfTask.Result ?? new()) PerformanceItems.Add(p);

        ObjectifItems.Clear();
        foreach (var o in objTask.Result ?? new()) ObjectifItems.Add(o);

        OnPropertyChanged(nameof(IsSuperviseur));
    });

    [RelayCommand]
    private Task RefreshAsync() => ExecuteAsync(async () =>
    {
        IsRefreshing = true;
        await LoadDashboardCommand.ExecuteAsync(null);
        IsRefreshing = false;
    });
}
```

---

## ViewModels/Visites/VisitListViewModel.cs

```csharp
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Field;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Visites;

public partial class VisitListViewModel : BaseViewModel
{
    private readonly VisiteService _visiteService;

    [ObservableProperty] private DateTime _selectedDate = DateTime.Today;
    [ObservableProperty] private string   _filterStatut  = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Visite> _visites = new();

    public List<string> StatutOptions { get; } = new()
        { "Tous", "PLANIFIEE", "REALISEE", "ANNULEE" };

    public VisitListViewModel(VisiteService visiteService)
    {
        _visiteService = visiteService;
        Title = "Visites";
    }

    [RelayCommand]
    private Task LoadAsync() => ExecuteAsync(async () =>
    {
        var start  = SelectedDate.Date;
        var end    = SelectedDate.Date;
        var statut = FilterStatut == "Tous" ? null : FilterStatut;
        var list   = await _visiteService.GetVisitesAsync(start, end, statut);
        Visites.Clear();
        foreach (var v in list ?? new()) Visites.Add(v);
    });

    partial void OnSelectedDateChanged(DateTime value)  => _ = LoadCommand.ExecuteAsync(null);
    partial void OnFilterStatutChanged(string value)    => _ = LoadCommand.ExecuteAsync(null);

    [RelayCommand]
    private Task RefreshAsync() => ExecuteAsync(async () =>
    {
        IsRefreshing = true;
        await LoadCommand.ExecuteAsync(null);
        IsRefreshing = false;
    });

    [RelayCommand]
    private Task GoToDetailAsync(Visite? visite)
    {
        if (visite == null)
            return Shell.Current.GoToAsync($"visits/detail?visiteId=0&prefillDate={SelectedDate:yyyy-MM-dd}");
        return Shell.Current.GoToAsync($"visits/detail?visiteId={visite.Id}");
    }
}
```

---

## ViewModels/Visites/VisitDetailViewModel.cs

```csharp
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Field;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Visites;

public class UserPickerItem
{
    public int    Id  { get; set; }
    public string Nom { get; set; } = string.Empty;
}

[QueryProperty(nameof(VisiteId),      "visiteId")]
[QueryProperty(nameof(PrefillDate),   "prefillDate")]
[QueryProperty(nameof(IdPlanningRaw), "idPlanning")]
public partial class VisitDetailViewModel : BaseViewModel
{
    private readonly VisiteService   _visiteService;
    private readonly UserService     _userSvc;
    private readonly PlanningService _planningSvc;

    [ObservableProperty] private int      _visiteId;
    [ObservableProperty] private string   _prefillDate  = string.Empty;
    [ObservableProperty] private string   _clientName   = string.Empty;
    [ObservableProperty] private DateTime _visiteDate   = DateTime.Now;
    [ObservableProperty] private string   _notes        = string.Empty;
    [ObservableProperty] private string   _statut       = "PLANIFIEE";
    [ObservableProperty] private int      _selectedType = 1;
    [ObservableProperty] private int?     _selectedPlanningId;
    [ObservableProperty] private string   _idPlanningRaw = string.Empty;

    [ObservableProperty]
    private ObservableCollection<UserPickerItem> _medecins = new();

    [ObservableProperty]
    private ObservableCollection<UserPickerItem> _pharmaciens = new();

    [ObservableProperty] private UserPickerItem? _selectedMedecin;
    [ObservableProperty] private UserPickerItem? _selectedPharmacien;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPlanningLabel))]
    private string _planningLabel = string.Empty;

    public bool HasPlanningLabel => !string.IsNullOrEmpty(PlanningLabel);

    private bool _isDirty;
    public bool IsDirty => _isDirty;

    public bool IsNew      => VisiteId == 0;
    public bool IsExisting => VisiteId > 0;

    public List<string> VisiteTypeOptions { get; } = new() { "Médecin", "Pharmacien", "Autre" };

    public string SelectedTypeLabel
    {
        get => SelectedType switch { 1 => "Médecin", 2 => "Pharmacien", _ => "Autre" };
        set
        {
            SelectedType = value switch { "Médecin" => 1, "Pharmacien" => 2, _ => 3 };
            OnPropertyChanged();
        }
    }

    public List<string> StatutOptions { get; } = new() { "PLANIFIEE", "REALISEE", "ANNULEE" };

    public VisitDetailViewModel(VisiteService visiteService, UserService userSvc, PlanningService planningSvc)
    {
        _visiteService = visiteService;
        _userSvc       = userSvc;
        _planningSvc   = planningSvc;
        Title = "Nouvelle visite";
    }

    partial void OnVisiteIdChanged(int value)
    {
        Title = value > 0 ? "Détail visite" : "Nouvelle visite";
        OnPropertyChanged(nameof(IsNew));
        OnPropertyChanged(nameof(IsExisting));
    }

    partial void OnPrefillDateChanged(string value)
    {
        if (DateTime.TryParse(value, out var dt)) VisiteDate = dt;
    }

    partial void OnClientNameChanged(string value)    => _isDirty = true;
    partial void OnNotesChanged(string value)         => _isDirty = true;
    partial void OnStatutChanged(string value)        => _isDirty = true;
    partial void OnSelectedMedecinChanged(UserPickerItem? value)    => _isDirty = true;
    partial void OnSelectedPharmacienChanged(UserPickerItem? value) => _isDirty = true;
    partial void OnSelectedTypeChanged(int value)
    {
        _isDirty = true;
        OnPropertyChanged(nameof(SelectedTypeLabel));
    }

    [RelayCommand]
    public Task InitAsync() => ExecuteAsync(async () =>
    {
        var medecinTask    = _userSvc.GetUsersByRoleAsync("MEDECIN");
        var pharmacienTask = _userSvc.GetUsersByRoleAsync("CLIENT");
        await Task.WhenAll(medecinTask, pharmacienTask);

        Medecins.Clear();
        foreach (var u in medecinTask.Result ?? new())
            Medecins.Add(new UserPickerItem { Id = u.Id, Nom = u.Name });

        Pharmaciens.Clear();
        foreach (var u in pharmacienTask.Result ?? new())
            Pharmaciens.Add(new UserPickerItem { Id = u.Id, Nom = u.Name });

        if (VisiteId > 0)
        {
            var visite = await _visiteService.GetVisiteByIdAsync(VisiteId);
            if (visite != null)
            {
                VisiteDate         = visite.DateVisite;
                SelectedType       = visite.Type;
                SelectedPlanningId = visite.IdPlanning;
                Statut             = visite.Statut;
                SelectedMedecin    = Medecins.FirstOrDefault(m => m.Id == visite.IdMedecin);
                SelectedPharmacien = Pharmaciens.FirstOrDefault(p => p.Id == visite.IdPharmacien);
            }
        }

        if (int.TryParse(IdPlanningRaw, out var pid))
        {
            SelectedPlanningId = pid;
            var p = await _planningSvc.GetPlanningByIdAsync(pid);
            if (p != null)
                PlanningLabel = $"Lié au planning du {p.DatePlanifiee:dd/MM/yyyy} " +
                                $"{p.HeureDebut:hh\\:mm}–{p.HeureFin:hh\\:mm}";
        }

        _isDirty = false;
    });

    [RelayCommand]
    private Task SaveAsync() => ExecuteAsync(async () =>
    {
        if (!await CheckConnectivityAsync()) return;
        var userId = await SecureStorage.GetAsync(StorageKeys.UserId);
        var dto = new CreateVisiteDto
        {
            DateVisite   = VisiteDate,
            Type         = SelectedType,
            IdMedecin    = SelectedMedecin?.Id,
            IdPharmacien = SelectedPharmacien?.Id,
            IdPlanning   = SelectedPlanningId,
            IdDelegue    = int.Parse(userId ?? "0"),
        };
        if (IsNew)
            await _visiteService.CreateVisiteAsync(dto);
        else
        {
            dto.IdVisite = VisiteId;
            await _visiteService.UpdateVisiteAsync(VisiteId, dto);
        }
        _isDirty = false;
        HapticService.Success();
        await Shell.Current.GoToAsync("..");
    });

    [RelayCommand]
    private Task DeleteAsync() => ExecuteAsync(async () =>
    {
        if (!await Shell.Current.DisplayAlert("Confirmation", "Supprimer cette visite ?", "Oui", "Annuler")) return;
        await _visiteService.DeleteVisiteAsync(VisiteId);
        await Shell.Current.GoToAsync("..");
    });

    [RelayCommand]
    private async Task GoToRapportAsync()
        => await Shell.Current.GoToAsync($"//visits/rapport?visiteId={VisiteId}");
}
```

---

## ViewModels/Rapports/RapportViewModel.cs (key excerpt — full)

```csharp
// GPS capture, offline-first rapport, [MinLength(20)] validation
// PreCaptureLocationAsync() on appearing → CaptureLocationAsync() at submit
// Offline: saves to LocalDatabaseService.InsertPendingRapportAsync
// Online: calls _visiteService.CreateRapportAsync → POST fields/rapports/createUpdate
// Properties: CapturedLatitude, CapturedLongitude, GeoStatus, IsCapturingLocation
// Validation: Contenu min 20 chars via [MinLength(20)] + [NotifyDataErrorInfo]
```

---

## ViewModels/Planning/PlanningViewModel.cs (key excerpt)

```csharp
// Week-based calendar: WeekStart (Monday), 7-day list loaded from PlanningService
// GetPlanningAsync(weekStart) → filters by idDelegue + date range
// CreatePlanningEntryAsync / UpdatePlanningEntryAsync / DeletePlanningEntryAsync
// Navigation to VisitDetailPage with idPlanning query param
```

---

## ViewModels/Stock/MyStockViewModel.cs (key excerpt)

```csharp
// Segments: 0=Echantillon, 1=Promo, 2=History
// DistributeSampleAsync: ActionSheet (Médecin/Pharmacien) → DisplayPromptAsync for ID
// Optimistic local update then _inventoryService.PostDistributionAsync
// Offline fallback from LocalDatabaseService.GetStockAsync()
// IsStockSegment = ActiveSegment < 2; IsHistorySegment = ActiveSegment == 2
```

---

## ViewModels/Products/ProductListViewModel.cs (key excerpt)

```csharp
// CanSeePrices = role is not "MEDECIN"
// _useVisibleEndpoint = role is "MEDECIN" or "PHARMACIEN" or "GROSSISTE" or "CLIENT"
// DELEGUE/ADMIN → products endpoint, filter !p.IsArchived client-side
// 300ms debounce on search, min 3 chars
// Offline seed: LocalDatabaseService.GetProductsAsync() fallback
```

---

## ViewModels/Products/ProductDetailViewModel.cs (key excerpt)

```csharp
// CanSeePrices = role is not "MEDECIN"
// HasInformations = CanSeePrices (gates price card)
// MEDECIN: image files filtered from supports (only non-image extensions)
// Lots/Promotions: 404/403 silently caught for MEDECIN
// AddToOrderAsync: if role==MEDECIN → DisplayAlert "Accès refusé"
//                  else → //orders/create?productId={ProductId}
```

---

## ViewModels/Orders/OrderListViewModel.cs (key excerpt)

```csharp
// CLIENT roles: _orderService.GetOrdersByClientAsync(clientId, statut, page, 20)
// DELEGUE/ADMIN: _orderService.GetOrdersAsync(statut, page, 20)
// Filters o.Statut != 0 (removes Brouillon = 0)
// Status options: Tous, En attente, Confirmée, En préparation, Expédiée, Livrée, Annulée
// Pagination: LoadMoreCommand via ExecuteUncheckedAsync
```

---

## ViewModels/Orders/CreateOrderViewModel.cs (key excerpt)

```csharp
// 3-step wizard: CurrentStep (1=products, 2=cart review, 3=confirm)
// Cart scoped per user: draft_cart_{userId} in Preferences
// Promotion engine: SQLite GetActivePromotionAsync(productId) (offline)
// Sends IsFinalValidation = true always
// Payload: { Lignes: [{Id_Produit, Quantite, PrixUnitaire, Remise}], IsFinalValidation: true }
```

---

## ViewModels/Orders/OrderDetailViewModel.cs (key excerpt)

```csharp
// Loads order by ID + linked documents (Factures, BonsCommande, BonsLivraison)
// Reclamation form: ToggleReclamationFormCommand, SubmitReclamationCommand
// CanCancel = order.Statut <= 2 (Brouillon, EnAttente, Confirmée)
// CanCreateReclamation = order.Statut >= 2 (Confirmée onwards)
// HasLinkedDocuments, HasReclamations computed from collections
```

---

## ViewModels/Documents/DocumentListViewModel.cs (key excerpt)

```csharp
// ADMIN/SUPERVISEUR: per-type endpoints (GetFacturesAsync, GetBonsCommandeAsync, GetBonsLivraisonAsync)
// Other roles: GetDocumentsByClientAndTypeAsync(clientId, apiType)
//              → documents/client/{0}/type/{1}
// SelectedTypeIndex: 0=Factures, 1=BonsCommande, 2=BonsLivraison
// SetTypeIndexCommand → reloads list
// OpenDocumentCommand → opens URL in browser
// GoToDetailCommand → documents/detail?id=...
```

---

## ViewModels/Documents/DocumentDetailViewModel.cs (key excerpt)

```csharp
// Loads by id + type (Facture/BonCommande/BonLivraison)
// IsFacture, IsBonCommande, IsBonLivraison computed
// ShareCommand → Share.RequestAsync with document URL
```

---

## ViewModels/Objectifs/ObjectifViewModel.cs (key excerpt)

```csharp
// Loads Objectifs from KpiService.GetObjectifsAsync(userId)
// GlobalAchievement = average of all Objectif.ProgressValue
// Each Objectif: TypeObjectif, Periode, ValeurActuelle, ValeurCible, ProgressValue
// RefreshCommand reloads
```

---

## ViewModels/Reclamations/ReclamationListViewModel.cs (key excerpt)

```csharp
// Loads all reclamations for the current client
// _orderService.GetReclamationsAsync(clientId)
// Reclamation statuses: Ouverte, EnCours, Resolue
// No creation from this page — creation is in OrderDetailPage
```

---

## ViewModels/Profile/ProfileViewModel.cs (key excerpt)

```csharp
// Loads User from UserService.GetCurrentUserAsync()
// AvatarInitials computed from User.Name
// IsDelegue / IsClient / IsMedecin for conditional UI sections
// NavigateToEditProfileCommand → profile/edit
// NavigateToChangePasswordCommand → profile/changepassword
// LogoutCommand → clears SecureStorage, navigates to //login
```

---

## ViewModels/Profile/EditProfileViewModel.cs (key excerpt)

```csharp
// Fields: Name, Telephone, Adresse (editable)
// Email, Role (read-only display)
// AvatarInitials computed
// SaveCommand → UserService.UpdateProfileAsync({ Name, Telephone, Adresse })
// CancelCommand → GoToAsync("..")
```

---

## ViewModels/Profile/ChangePasswordViewModel.cs (key excerpt)

```csharp
// OldPassword, NewPassword, ConfirmPassword
// ShowOldPassword, ShowNewPassword, ShowConfirmPassword (eye toggle)
// PasswordsMatch, PasswordsMismatch computed
// Seg1Color..Seg4Color + StrengthLabel (password strength bar)
// ChangePasswordCommand → AuthService.ChangePasswordAsync(old, new)
```

---

## Services/ApiService.cs (key excerpt)

```csharp
// GetAsync<T>(route), PostAsync<T>(route, body), PutAsync<T>(route, body), DeleteAsync(route)
// All routes relative to BaseAddress (API Gateway)
// Wraps responses in ApiResponse<T>: { isSuccess, result, message, errors }
// Returns result.result on success, null on failure
// Route prefixes: auth/, products/, orders/, fields/, inventory/, documents/
```

---

## Services/AuthService.cs (key excerpt)

```csharp
// LoginAsync(email, password) → POST auth/login
// Stores JWT in SecureStorage: jwt_token, jwt_expiry, user_role, user_id, user_name, user_email
// LogoutAsync() → clears all SecureStorage keys
// ChangePasswordAsync(old, new) → POST auth/change-password
// ForgotPasswordAsync(email) → POST auth/forgot-password
// GetCurrentTokenAsync() → returns cached token from SecureStorage
```

---

## Services/VisiteService.cs (key excerpt — with known bug)

```csharp
// GetVisitesAsync(start, end, statut) → GET fields/visites?...
// GetVisiteByIdAsync(id) → GET fields/visites/{id}
// CreateVisiteAsync(dto) → POST fields/visites
// UpdateVisiteAsync(id, dto) → BUG: calls PostAsync instead of PutAsync
//   → should be: _api.PutAsync<Visite>($"fields/visites/{id}", dto)
// DeleteVisiteAsync(id) → DELETE fields/visites/{id}
// CreateRapportAsync(rapport) → POST fields/rapports/createUpdate
```

---

## Services/PlanningService.cs

```csharp
using Cynapharm_Mobile.Models.Field;

namespace Cynapharm_Mobile.Services;

public class PlanningService
{
    private readonly ApiService _api;
    public PlanningService(ApiService api) { _api = api; }

    public async Task<List<Planning>?> GetPlanningAsync(DateTime weekStart)
    {
        var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
        if (!int.TryParse(userIdStr, out var userId)) return null;
        var endDate = weekStart.AddDays(6);
        return await _api.GetAsync<List<Planning>>(
            $"fields/plannings/by-range?idDelegue={userId}" +
            $"&startDate={weekStart:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");
    }

    public Task<Planning?> CreatePlanningEntryAsync(Planning entry)
        => _api.PostAsync<Planning>("fields/plannings", entry);

    public Task<Planning?> UpdatePlanningEntryAsync(int id, Planning entry)
        => _api.PostAsync<Planning>("fields/plannings", entry);  // BUG: should be PutAsync

    public Task<Planning?> GetPlanningByIdAsync(int id)
        => _api.GetAsync<Planning>($"fields/plannings/{id}");

    public Task<bool> DeletePlanningEntryAsync(int id)
        => _api.DeleteAsync($"fields/plannings/{id}");
}
```

---

## Services/KpiService.cs (key excerpt)

```csharp
// GetPerformanceAsync(userId, debut, fin) → GET fields/kpi/performance/{userId}?debut=...&fin=...
//   NOTE: only returns data for role=="DELEGUE"; returns empty list for all other roles
// GetObjectifsAsync(userId) → GET fields/kpi/objectifs/{userId}
// GetTauxConversionAsync(userId, debut, fin) → GET fields/kpi/taux-conversion/{userId}?...
// GetStockSummaryAsync (delegated to InventoryService)
// GetRegionsAsync() → GET fields/regions (SUPERVISEUR only)
```

---

## Services/LocalDatabaseService.cs (key excerpt)

```csharp
// SQLite tables: Product_Cache, Stock_Local, Pending_Rapports, Promotion_Cache, Log_Entries
// InsertPendingRapportAsync / GetPendingRapportsAsync / MarkRapportSyncedAsync
// DeductStockAsync(productId, qty) — optimistic local stock deduction
// GetStockAsync() — offline stock fallback
// GetProductsAsync() — offline product catalog
// GetActivePromotionAsync(productId) — offline promotion lookup for cart
```

---

## Services/SyncService.cs (key excerpt)

```csharp
// FlushPendingRapportsAsync() — drains SQLite queue, calls _visiteService.CreateRapportAsync
// Interlocked.CompareExchange guard against concurrent flush
// Singleton, wired to Connectivity.ConnectivityChanged event
// Called automatically when connectivity restored
```

---

## Services/UserService.cs (key excerpt)

```csharp
// GetUsersByRoleAsync(role) → GET auth/users/by-role?role={role}
// GetCurrentUserAsync() → GET auth/users/me
// UpdateProfileAsync(dto) → PUT auth/users/me
```

---

## Services/InventoryService.cs (key excerpt)

```csharp
// GetStockSummaryAsync(userId) → GET inventory/stock/delegue/{userId}/summary
// GetEchantillonsAsync(userId) → GET inventory/stock/echantillon/{userId}
// GetPromotionsAsync(userId) → GET inventory/stock/promo/{userId}
// GetMovementsAsync(userId) → GET inventory/stock/mouvements/{userId}
// PostDistributionAsync(dto) → POST inventory/distributions
```

---

## Services/ProductService.cs (key excerpt)

```csharp
// GetProductsAsync() → GET products (DELEGUE/ADMIN, filters archived client-side)
// GetVisibleProductsAsync() → GET products/visible (CLIENT/MEDECIN)
// GetProductByIdAsync(id) → GET products/{id}
// GetLotsAsync(id) → GET products/{id}/lots
// GetPromotionsAsync(id) → GET products/{id}/promotions
// GetSupportsAsync(id) → GET products/{id}/supports (documents/files)
```

---

## Services/OrderService.cs (key excerpt)

```csharp
// GetOrdersAsync(statut, page, pageSize) → GET orders (DELEGUE/ADMIN)
// GetOrdersByClientAsync(clientId, statut, page, pageSize) → GET orders/client/{clientId}
// GetOrderByIdAsync(id) → GET orders/{id}
// CreateOrderAsync(dto) → POST orders
// CancelOrderAsync(id, motif) → PUT orders/{id}/cancel
// SubmitReclamationAsync(dto) → POST orders/{id}/reclamations
// GetReclamationsAsync(clientId) → GET orders/reclamations/client/{clientId}
```

---

## Services/DocumentService.cs (key excerpt)

```csharp
// GetFacturesAsync() → GET documents/factures
// GetBonsCommandeAsync() → GET documents/bons-commande
// GetBonsLivraisonAsync() → GET documents/bons-livraison
// GetDocumentsByClientAndTypeAsync(clientId, type) → GET documents/client/{clientId}/type/{type}
// GetFactureByIdAsync(id) → GET documents/factures/{id}
// GetBonCommandeByIdAsync(id) → GET documents/bons-commande/{id}
// GetBonLivraisonByIdAsync(id) → GET documents/bons-livraison/{id}
```

---

## Models/Orders/Order.cs (key excerpt)

```csharp
// Statut int: 0=Brouillon, 1=EnAttente, 2=Confirmee, 3=EnPreparation, 4=Expediee, 5=Livree, 6=Annulee
// NumeroCommande => $"CMD-{Id:D5}"
// StatutFrançais computed string
// MontantTTC, Notes, MotifAnnulation
// List<LigneCommande> Lignes
// List<Reclamation> Reclamations
```

---

## Models/Field/PerformanceDto.cs

```csharp
// Type (int): 0=Visites, 1=Chiffre d'affaires, 2=Nouveaux clients, 3=Fidélisation
// ValeurCible (int), ValeurRealisee (int), Pourcentage (double)
// TypeLabel computed string
```

---

## Models/Field/Objectif.cs

```csharp
// TypeObjectif (string), Periode (string)
// ValeurCible (int), ValeurActuelle (int)
// ProgressValue = Math.Min(1.0, ValeurActuelle / (double)ValeurCible)
```

---

## Models/Common/ApiResponse.cs

```csharp
public class ApiResponse<T>
{
    public bool   IsSuccess { get; set; }
    public T?     Result    { get; set; }
    public string Message   { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}
```

---

## Views/Auth/LoginPage.xaml (structure)

```xml
<!-- Custom header with logo + app name -->
<!-- Email + Password Entry fields with show/hide toggle -->
<!-- "Se souvenir de moi" checkbox -->
<!-- LoginCommand button -->
<!-- GoToForgotPasswordCommand link -->
```

---

## Views/Dashboard/DashboardPage.xaml.cs

```csharp
using Cynapharm_Mobile.ViewModels.Dashboard;

namespace Cynapharm_Mobile.Views.Dashboard;

public partial class DashboardPage : ContentPage
{
    public DashboardPage() : this(MauiProgram.Services.GetRequiredService<DashboardViewModel>()) { }

    public DashboardPage(DashboardViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DashboardViewModel vm) _ = vm.LoadDashboardCommand.ExecuteAsync(null);
    }
}
```

---

## Views/Visites/VisitDetailPage.xaml.cs

```csharp
using Cynapharm_Mobile.ViewModels.Visites;

namespace Cynapharm_Mobile.Views.Visites;

public partial class VisitDetailPage : ContentPage
{
    public VisitDetailPage(VisitDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (BindingContext is VisitDetailViewModel vm)
            _ = vm.InitCommand.ExecuteAsync(null);
    }

    protected override bool OnBackButtonPressed()
    {
        var vm = BindingContext as VisitDetailViewModel;
        if (vm?.IsDirty == true)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                bool leave = await DisplayAlert(
                    "Modifications non enregistrées",
                    "Voulez-vous vraiment quitter ? Les modifications seront perdues.",
                    "Quitter", "Rester");
                if (leave) await Shell.Current.GoToAsync("..");
            });
            return true;
        }
        return base.OnBackButtonPressed();
    }
}
```

---

## Views/Visites/VisitListPage.xaml.cs (key excerpt)

```csharp
protected override void OnNavigatedTo(NavigatedToEventArgs args)
{
    base.OnNavigatedTo(args);
    if (BindingContext is VisitListViewModel vm)
        _ = vm.LoadCommand.ExecuteAsync(null);
}
```

---

## Views/Visites/VisitDetailPage.xaml (structure)

```xml
<!-- Header: back button + "Visite" title -->
<!-- Planning association indicator (HasPlanningLabel) -->
<!-- Card: Date (DatePicker), Type (Picker), Médecin (Picker), Pharmacien (Picker) -->
<!-- Rapport button (IsExisting) → GoToRapportCommand -->
<!-- Delete button (IsExisting) → DeleteCommand -->
<!-- Sticky bottom: Enregistrer la visite → SaveCommand -->
```

---

## Views/Products/ProductListPage.xaml (key bindings)

```xml
<!-- PrixDisplay label: IsVisible bound to CanSeePrices via AncestorType binding -->
<!-- "Disponible" badge: IsVisible = NOT CanSeePrices (MEDECIN only) -->
<!-- SearchBar: 300ms debounce, min 3 chars -->
<!-- Load more: RemainingItemsThresholdReachedCommand -->
```

---

## Views/Products/ProductDetailPage.xaml (key bindings)

```xml
<!-- Price block: IsVisible="{Binding CanSeePrices}" -->
<!-- Lots section: IsVisible="{Binding CanSeePrices}" -->
<!-- Promotions section: IsVisible="{Binding CanSeePrices}" -->
<!-- MEDECIN info banner: IsVisible=InvertedBoolConverter(CanSeePrices) -->
<!-- "Ajouter à commande" sticky CTA: IsVisible="{Binding CanSeePrices}" -->
```

---

## Views/Orders/OrderDetailPage.xaml (structure)

```xml
<!-- Summary card: NumeroCommande, DateCommande, Statut (DataTrigger icons), MontantTTC -->
<!-- Cancellation reason banner (IsAnnulee) -->
<!-- Articles: CollectionView of LigneCommande (Quantite, PrixUnitaire, SousTotal) -->
<!-- Notes (conditional) -->
<!-- Documents liés: Factures, BonsCommande, BonsLivraison (BindableLayout each) -->
<!-- Réclamations list + empty state -->
<!-- Annuler la commande button (CanCancel) -->
<!-- Soumettre une réclamation toggle (CanCreateReclamation) -->
<!-- Reclamation inline form (Picker for article + Entry motif + Editor description) -->
```

---

## Views/Documents/DocumentListPage.xaml (structure)

```xml
<!-- Header: "Documents" + subtitle -->
<!-- 3-tab bar: Factures | Bons cmd. | Bons liv. (DataTrigger active indicator) -->
<!-- CollectionView with DocumentSummary items -->
<!-- Each item: Numero, Date, Montant, Download button (if Url), chevron -->
<!-- OpenDocumentCommand + GoToDetailCommand -->
```

---

## Views/Documents/DocumentDetailPage.xaml (structure)

```xml
<!-- IsFacture section: NumeroFacture, DateFacture, MontantHT, TVA, MontantTTC -->
<!-- IsBonCommande section: NumeroBon, DateEmission -->
<!-- IsBonLivraison section: NumeroBon, DateLivraison -->
<!-- Sticky share button: ShareCommand -->
<!-- ToolbarItem: "Partager" -->
```

---

## Views/Objectifs/ObjectifPage.xaml (structure)

```xml
<!-- Hero header: "Mes objectifs" + GlobalAchievement pill (taux global %) -->
<!-- CollectionView of Objectif: TypeObjectif, Periode, ProgressBar, ValeurActuelle/Cible -->
```

---

## Views/Stock/MyStockPage.xaml (structure)

```xml
<!-- 3-tab bar: Échantillons | Stock Promo | Historique -->
<!-- Segments 0&1: StockLines CollectionView (StockDisplayItem) -->
<!--   - ProductNom, QuantiteLabel badge (red if !CanDistribute) -->
<!--   - ExpiryLabel (HasExpiry) -->
<!--   - ProgressBar -->
<!--   - "Distribuer" button (IsEchantillon, CanDistribute) -->
<!--   - DistributeSampleCommand with item parameter -->
<!-- Segment 2: StockMovements CollectionView (StockMouvement) -->
<!--   - Color-coded left bar (Increment/Decrement/Transfer-In/Transfer-Out) -->
<!--   - DateMouvement, ProductNom, TypeMouvement badge, Quantite -->
```

---

## Views/Reclamations/ReclamationListPage.xaml (structure)

```xml
<!-- Header: "Réclamations" -->
<!-- CollectionView: Reclamation items -->
<!--   - Color-coded left bar: Ouverte(orange)/EnCours(blue)/Resolue(green) -->
<!--   - Motif, DateCreation, Statut badge -->
<!-- Empty state: "Aucune réclamation" -->
```

---

## Views/Profile/ProfilePage.xaml (structure)

```xml
<!-- Hero header: Avatar (initials), Name, Role badge -->
<!-- Informations personnelles card: Email, Téléphone, Région(DELEGUE), Adresse(CLIENT) -->
<!--   MEDECIN: Cabinet, Wilaya, "Délégué assigné" info banner -->
<!-- Compte card: Modifier le profil, Changer le mot de passe -->
<!-- Logout danger card -->
<!-- Footer: "© 2026 CynaPharm" -->
```

---

## Views/Profile/EditProfilePage.xaml (structure)

```xml
<!-- Avatar with camera badge (photo change placeholder) -->
<!-- Modifiable section: Nom complet, Téléphone, Adresse -->
<!-- Non-modifiable section (read-only, opacity 0.7): Email (locked), Rôle (locked) -->
<!-- Sticky: Annuler | Enregistrer -->
```

---

## Views/Profile/ChangePasswordPage.xaml (structure)

```xml
<!-- Info banner: "min 6 caractères" -->
<!-- Mot de passe actuel (eye toggle) -->
<!-- Nouveau mot de passe (eye toggle) + 4-segment strength bar + StrengthLabel -->
<!-- Confirmer le mot de passe (eye toggle) -->
<!-- Match indicator (green) / Mismatch indicator (red) -->
<!-- Sticky: Annuler | Mettre à jour -->
```

---

# PARTIE 1 — NAVIGATION ET SHELL

## 1.1 Tableau des routes enregistrées

| Route Shell | Type | Accès |
|---|---|---|
| `//login` | ShellContent (FlyoutItemIsVisible=False) | Tous |
| `//dashboard` | Tab `FlyoutDashboard` | DÉLÉGUÉ seulement |
| `//visits` | Tab `FlyoutVisites` | DÉLÉGUÉ seulement |
| `//planning` | Tab `FlyoutPlanning` | DÉLÉGUÉ seulement |
| `//products` | Tab (toujours visible) | Tous les rôles |
| `//orders` | Tab `FlyoutOrders` | CLIENT + DÉLÉGUÉ |
| `//documents` | Tab `FlyoutDocuments` | CLIENT seulement |
| `//profile` | Tab (toujours visible) | Tous les rôles |
| `//stock` | FlyoutItem (drawer) | DÉLÉGUÉ seulement |
| `//objectifs` | FlyoutItem (drawer) | DÉLÉGUÉ seulement |
| `//reclamations` | FlyoutItem (drawer) | CLIENT seulement |
| `forgotpassword` | Route relative | Tous (pre-auth) |
| `visits/detail` | Route relative | DÉLÉGUÉ |
| `visits/rapport` | Route relative | DÉLÉGUÉ |
| `products/detail` | Route relative | Tous |
| `products/detail/viewer` | Route relative | Tous |
| `orders/detail` | Route relative | CLIENT + DÉLÉGUÉ |
| `orders/create` | Route relative | CLIENT + DÉLÉGUÉ |
| `documents/detail` | Route relative | CLIENT |
| `profile/edit` | Route relative | Tous |
| `profile/changepassword` | Route relative | Tous |

## 1.2 Barre d'onglets par rôle

### DÉLÉGUÉ (DELEGUE / ADMIN / SUPERVISEUR)
Tab bar visible: **Accueil | Visites | Planning | Catalogue | Commandes | Profil**
Drawer supplémentaire: Mon Stock, Objectifs

### CLIENT (PHARMACIEN / GROSSISTE / CLIENT)
Tab bar visible: **Catalogue | Commandes | Documents | Profil**
Drawer supplémentaire: Réclamations

### MÉDECIN
Tab bar visible: **Catalogue | Profil**
Flyout: **désactivé** (`FlyoutBehavior.Disabled`)

## 1.3 Détection du rôle au démarrage

1. `LoginViewModel.LoginAsync()` appelle `_authService.LoginAsync(email, password)`
2. `AuthService` stocke `user_role` dans `SecureStorage`
3. `AppShell.ApplyRoleVisibility(role)` est appelé immédiatement après le login
4. `ApplyRoleVisibility` calcule `isDelegue`, `isClient`, `isMedecin` et ajuste:
   - La visibilité de chaque Tab dans la TabBar
   - La visibilité de chaque item dans le Flyout custom
   - `FlyoutBehavior` (Disabled pour MÉDECIN)
5. Navigation post-login:
   - DÉLÉGUÉ/ADMIN/SUPERVISEUR → `//dashboard`
   - PHARMACIEN/GROSSISTE/CLIENT → `//orders`
   - MÉDECIN → `//products`

## 1.4 Comportement du token

- `TokenValidationHandler` vérifie `jwt_expiry` avant chaque requête (seuil 5 min)
- Si token expiré: redirige vers `//login` (clear SecureStorage)
- Si token absent: idem
- Polly retries: 3 tentatives, backoff exponentiel 1s/2s/4s
- Circuit breaker: s'ouvre après 50% d'échecs sur 30s
- Timeout par tentative: 10s; Total: 60s

---

# PARTIE 2 — SCÉNARIO DÉLÉGUÉ

## 2.1 Onglets disponibles

| Onglet | Route | Vue | ViewModel |
|---|---|---|---|
| Accueil | `//dashboard` | DashboardPage | DashboardViewModel |
| Visites | `//visits` | VisitListPage | VisitListViewModel |
| Planning | `//planning` | PlanningPage | PlanningViewModel |
| Catalogue | `//products` | ProductListPage | ProductListViewModel |
| Commandes | `//orders` | OrderListPage | OrderListViewModel |
| Profil | `//profile` | ProfilePage | ProfileViewModel |

Drawer: Mon Stock (`//stock`), Objectifs (`//objectifs`)

## 2.2 Dashboard

**Données chargées:**
- `_kpiService.GetPerformanceAsync(userId, monthStart, today)` → PerformanceItems
- `_kpiService.GetObjectifsAsync(userId)` → ObjectifItems
- `_visiteService.GetVisitesAsync(today, today, null)` → TodayVisitCount
- `_kpiService.GetTauxConversionAsync(userId, monthStart, today)` → TauxConversion
- `_inventoryService.GetStockSummaryAsync(userId)` → StockSummary

**Pour SUPERVISEUR uniquement:**
- `_kpiService.GetRegionsAsync()` → Regions (liste des régions)

**Affichage:**
- Salutation + prénom de l'utilisateur
- Nombre de visites du jour
- Taux de conversion du mois
- Résumé stock (total, faible, expiré)
- Cards de performance (ProgressBar par type: Visites, CA, Nouveaux clients, Fidélisation)
- Cards d'objectifs (ProgressBar + valeur réalisée/cible)

**Limitation:** `KpiService.GetPerformanceAsync` ne retourne des données que si `role == "DELEGUE"`. Les rôles ADMIN et SUPERVISEUR reçoivent une liste vide, bien qu'ils aient accès au dashboard.

## 2.3 Catalogue (vue DÉLÉGUÉ)

- Endpoint: `products` (tous les produits, archivés filtrés client-side)
- `CanSeePrices = true` → prix affiché
- Recherche: debounce 300ms, min 3 chars
- Détail produit: prix, lots, promotions, supports
- "Ajouter à commande" disponible → navigue vers `orders/create?productId=...`

## 2.4 Visites

**Liste (VisitListPage):**
- Filtrage par date (DatePicker) et statut (Picker: Tous/PLANIFIEE/REALISEE/ANNULEE)
- Chargement automatique à chaque changement de filtre
- Tap sur visite → `visits/detail?visiteId={id}`
- Bouton "+" → `visits/detail?visiteId=0&prefillDate={date}`

**Détail (VisitDetailPage):**
- Champs: Date (DatePicker), Type (Picker: Médecin/Pharmacien/Autre), Médecin (Picker), Pharmacien (Picker)
- Planning association: banière bleue si `idPlanning` passé en query param
- IsDirty guard sur le bouton retour (dialogue de confirmation)
- Enregistrer → POST (création) ou **BUG**: POST au lieu de PUT (mise à jour)
- "Soumettre un rapport" → `//visits/rapport?visiteId={id}` (IsExisting uniquement)
- Supprimer (IsExisting uniquement)

## 2.5 Planning

- Vue semaine: 7 jours à partir du lundi courant
- Navigation prev/next week via WeekStart ± 7 jours
- Endpoint: `fields/plannings/by-range?idDelegue=...&startDate=...&endDate=...`
- Création d'entrée: formulaire inline ou modal
- Tap sur planning → `visits/detail?visiteId=0&idPlanning={id}&prefillDate={date}`
- Suppression avec confirmation

## 2.6 Rapports

- Accessible via `visits/rapport?visiteId={id}` depuis VisitDetailPage
- Champ Contenu: validation min 20 chars
- GPS: capture "rapide" (last known) à l'arrivée, "précise" à la soumission
- **Offline:** sauvegarde dans SQLite (PendingRapport); SyncService flush à la reconnexion
- Endpoint: `fields/rapports/createUpdate` (POST)

## 2.7 Stock (Mon Stock)

- 3 segments: Échantillons | Stock Promo | Historique
- Données chargées depuis `inventory/stock/echantillon/{userId}` et `inventory/stock/promo/{userId}`
- Chaque item: ProductNom, QuantiteLabel, ExpiryLabel, ProgressBar, CanDistribute
- **Distribuer (Échantillons):** ActionSheet (Médecin/Pharmacien) → prompt ID → `inventory/distributions`
- Optimistic update local → API async
- **Offline:** fallback depuis LocalDatabaseService.GetStockAsync()
- Historique: mouvements color-codés (Increment/Decrement/Transfer-In/Transfer-Out)

## 2.8 Objectifs

- Endpoint: `fields/kpi/objectifs/{userId}`
- GlobalAchievement: moyenne des taux d'atteinte
- Chaque objectif: TypeObjectif, Periode, ProgressBar, ValeurActuelle/ValeurCible

## 2.9 Commandes (vue DÉLÉGUÉ)

- Endpoint: `orders` (tous les ordres, pas filtré par client)
- Filtre par statut (Tous → Annulée)
- Filtre `o.Statut != 0` (masque Brouillon)
- Pagination: LoadMoreCommand

**Limitations pour DÉLÉGUÉ dans OrderDetail:**
- Peut voir les documents liés, les réclamations
- `CanCancel` s'applique mais le DÉLÉGUÉ ne devrait peut-être pas annuler
- Peut soumettre une réclamation (potentiellement pas voulu)

## 2.10 Fonctionnalités manquantes pour DÉLÉGUÉ

| Feature | État |
|---|---|
| Statut SUPERVISEUR: dashboard sans KPI performance | Bug (retourne liste vide) |
| Mise à jour de visite (PUT) | Bug (appelle POST) |
| Mise à jour de planning | Bug (appelle POST au lieu de PUT) |
| Notes sur visite | Champ présent dans ViewModel mais pas dans XAML |
| Statut de la visite éditable | Présent en ViewModel mais pas exposé dans le form |
| Notifications push | Absent |
| Export/partage des rapports | Absent |
| Filtre de visites par médecin/pharmacien | Absent |

---

# PARTIE 3 — SCÉNARIO MÉDECIN

## 3.1 Onglets disponibles

| Onglet | Route | Vue |
|---|---|---|
| Catalogue | `//products` | ProductListPage |
| Profil | `//profile` | ProfilePage |

Flyout: **désactivé** (aucun hamburger menu)

## 3.2 Catalogue (vue MÉDECIN)

- Endpoint: `products/visible` (produits marqués visibles)
- `CanSeePrices = false` → prix masqué, "Disponible" badge affiché à la place
- Recherche identique (debounce 300ms, min 3 chars)
- Pagination identique

## 3.3 Détail produit (vue MÉDECIN)

- Prix: masqué (`IsVisible=false`)
- Lots: masqués (`IsVisible=false`)
- Promotions: masquées (`IsVisible=false`)
- Supports: filtrés — seuls les fichiers non-image sont affichés
- Bannière info MÉDECIN: "Catalogue consultatif uniquement"
- "Ajouter à commande": **masqué** (`IsVisible=false`)
- Lots/Promotions: silencieusement ignorés (404/403 caught)

## 3.4 Profil (vue MÉDECIN)

- Affiche: Email, Téléphone, Cabinet (via Adresse), Wilaya (via RegionId)
- Section "Délégué assigné" visible (IsMedecin=True)
- Modifier le profil: disponible (Nom, Téléphone, Adresse)
- Changer le mot de passe: disponible

## 3.5 Guards d'accès

- Flyout désactivé: impossible d'accéder aux autres sections via hamburger
- `ProductDetailViewModel.AddToOrderAsync`: DisplayAlert "Accès refusé" si MÉDECIN
- `DocumentListViewModel`: pas de guard explicite — sécurité par invisibilité du tab uniquement
- Navigation directe (`//orders`) theoriquement possible si URL connue — aucun guard serveur-side dans le VM

## 3.6 Fonctionnalités manquantes pour MÉDECIN

| Feature | État |
|---|---|
| Télécharger/partager fiches produit (PDF) | Partiel (DocumentViewer existe mais accès indirect) |
| Historique des visites reçues | Absent |
| Messagerie avec délégué | Absent |
| Favoris produits | Absent |
| Notification de nouveaux produits | Absent |
| Navigation directe guard (route protection) | Absent — sécurité uniquement UI |

---

# PARTIE 4 — SCÉNARIO CLIENT (PHARMACIEN/GROSSISTE)

## 4.1 Onglets disponibles

| Onglet | Route | Vue |
|---|---|---|
| Catalogue | `//products` | ProductListPage |
| Commandes | `//orders` | OrderListPage |
| Documents | `//documents` | DocumentListPage |
| Profil | `//profile` | ProfilePage |

Drawer: Réclamations (`//reclamations`)

## 4.2 Catalogue (vue CLIENT)

- Endpoint: `products/visible`
- `CanSeePrices = true` → prix affiché
- "Ajouter à commande" disponible

## 4.3 Commandes

**Liste:**
- Endpoint: `orders/client/{clientId}` filtré par statut
- Filtre `o.Statut != 0` (masque Brouillon)
- Pagination

**Création (CreateOrderPage — wizard 3 étapes):**
- Étape 1: Recherche produit + ajout au panier
- Étape 2: Révision du panier (quantités, supprimer)
- Étape 3: Confirmation + total TTC
- Panier persistant: `Preferences["draft_cart_{userId}"]`
- Promotions offline depuis SQLite
- Payload: `{ Lignes: [...], IsFinalValidation: true }`

**Détail:**
- Résumé: numéro, date, statut, montant TTC
- Articles: liste des lignes
- Documents liés (Factures, BonsCommande, BonsLivraison)
- Réclamations existantes
- `CanCancel`: Statut <= 2 (Brouillon, EnAttente, Confirmée)
- `CanCreateReclamation`: Statut >= 2
- Formulaire réclamation inline: article (optionnel), motif, description

## 4.4 Documents

- 3 types en onglets: Factures | Bons cmd. | Bons liv.
- Endpoint: `documents/client/{clientId}/type/{type}`
- Chaque item: numéro, date, montant, bouton télécharger (si URL)
- Détail: affiche les champs du document + bouton partager

## 4.5 Réclamations

- Accessible via drawer uniquement
- Liste les réclamations du client: Motif, Date, Statut (Ouverte/EnCours/Résolue)
- Lecture seule (création via OrderDetail)
- Aucun filtrage ni tri

## 4.6 Profil (vue CLIENT)

- Affiche: Email, Téléphone, Adresse (section CLIENT)
- Modifier le profil: disponible
- Changer le mot de passe: disponible

## 4.7 Fonctionnalités manquantes pour CLIENT

| Feature | État |
|---|---|
| Création de réclamation indépendante (sans commande) | Absent |
| Filtrage des réclamations par statut | Absent |
| Suivi de livraison | Absent |
| Notifications de changement de statut de commande | Absent |
| Historique de commandes avec export | Absent |
| Adresse de livraison multiple | Absent |
| Favoris produits | Absent |

---

# PARTIE 5 — ANALYSE GLOBALE

## 5.1 Tableau des bugs identifiés

| ID | Fichier | Description | Sévérité | Impact |
|---|---|---|---|---|
| BUG-01 | `Services/VisiteService.cs` | `UpdateVisiteAsync` appelle `PostAsync` au lieu de `PutAsync` | CRITIQUE | Chaque "mise à jour" crée une nouvelle visite au lieu de modifier l'existante |
| BUG-02 | `Services/PlanningService.cs` | `UpdatePlanningEntryAsync` appelle `PostAsync` au lieu de `PutAsync` | CRITIQUE | Idem pour les plannings |
| BUG-03 | `Services/KpiService.cs` | `GetPerformanceAsync` retourne liste vide pour ADMIN et SUPERVISEUR | MAJEUR | Dashboard sans KPI pour ces rôles |
| BUG-04 | `ViewModels/Visites/VisitDetailViewModel.cs` | `Statut` et `Notes` présents mais non exposés dans le XAML | MINEUR | Impossible de modifier le statut ou les notes d'une visite depuis l'UI |
| BUG-05 | `AppShell.xaml.cs` | Aucun guard de route — navigation directe possible en contournant l'UI | MAJEUR | Un MÉDECIN peut théoriquement naviguer vers `//orders` si l'URL est connue |
| BUG-06 | `ViewModels/Orders/OrderDetailViewModel.cs` | `CanCancel` inclut Statut=0 (Brouillon) mais OrderList filtre les Brouillons | MINEUR | Incohérence: OrderDetail peut annuler Brouillon si accessible directement |
| BUG-07 | `ViewModels/Orders/CreateOrderViewModel.cs` | `IsFinalValidation = true` hardcodé — pas de flux Brouillon depuis mobile | MINEUR | Impossible de sauvegarder une commande en brouillon |
| BUG-08 | `Views/Profile/ProfilePage.xaml` | `User.RegionId` affiché pour MÉDECIN sous "Wilaya" — probablement un ID numérique | MINEUR | Affiche un ID au lieu du nom de la région/wilaya |
| BUG-09 | `Views/Dashboard/DashboardPage.xaml.cs` | `DashboardPage()` constructeur sans paramètre utilise `MauiProgram.Services` — couplage fort | MINEUR | Anti-pattern, rend les tests difficiles |
| BUG-10 | `ViewModels/Documents/DocumentListViewModel.cs` | Aucun guard MÉDECIN explicite — sécurité par visibilité tab uniquement | MAJEUR | Si un MÉDECIN accède à `//documents`, la page se chargera sans erreur |

## 5.2 Tableau des fonctionnalités manquantes

| Feature | Rôles concernés | Priorité |
|---|---|---|
| Modification de visite (PUT) fonctionnelle | DÉLÉGUÉ | P0 — bloquant |
| Modification de planning (PUT) fonctionnelle | DÉLÉGUÉ | P0 — bloquant |
| KPI Performance pour ADMIN/SUPERVISEUR | ADMIN/SUPERVISEUR | P1 |
| Guard de routes (navigation protection) | Tous | P1 — sécurité |
| Statut et notes éditables dans VisitDetail | DÉLÉGUÉ | P1 |
| Création de réclamation sans commande | CLIENT | P2 |
| Filtrage des réclamations | CLIENT | P2 |
| Notifications push | Tous | P2 |
| Affichage nom de wilaya/région (pas ID) pour MÉDECIN | MÉDECIN | P2 |
| Export rapport/commande | DÉLÉGUÉ/CLIENT | P3 |
| Favoris produits | CLIENT/MÉDECIN | P3 |
| Filtre de visites par médecin/pharmacien | DÉLÉGUÉ | P3 |
| Draft commande (IsFinalValidation=false) | CLIENT | P3 |
| Historique des visites reçues par MÉDECIN | MÉDECIN | P3 |

## 5.3 Matrice UI par rôle

| Feature / Écran | DÉLÉGUÉ | CLIENT | MÉDECIN |
|---|---|---|---|
| Dashboard | Oui (KPI, visites, stock) | Non | Non |
| Visites (liste + détail) | Oui | Non | Non |
| Planning | Oui | Non | Non |
| Rapports (terrain) | Oui (via Planning/Visites) | Non | Non |
| Catalogue (liste) | Oui + prix | Oui + prix | Oui sans prix |
| Catalogue (détail) | Oui (prix, lots, promos, supports) | Oui (prix, lots, promos) | Oui (supports non-image seulement) |
| Créer une commande | Oui | Oui | Non (bloqué) |
| Liste des commandes | Oui (toutes) | Oui (les siennes) | Non |
| Détail commande | Oui | Oui | Non |
| Documents | Non (onglet masqué) | Oui | Non |
| Mon Stock | Oui (drawer) | Non | Non |
| Objectifs | Oui (drawer) | Non | Non |
| Réclamations | Non (onglet masqué) | Oui (drawer) | Non |
| Profil | Oui (Région) | Oui (Adresse) | Oui (Cabinet, Wilaya) |
| Modifier profil | Oui | Oui | Oui |
| Changer mot de passe | Oui | Oui | Oui |
| Flyout hamburger | Oui | Oui | Non (désactivé) |

## 5.4 Problèmes transversaux

1. **Aucun intercepteur de route côté client:** `AppShell.cs` ne bloque pas la navigation directe par URL. Si une Route est connue, n'importe quel rôle peut y accéder sans être bloqué dans le ViewModel.

2. **Cohérence offline limitée:** Le cache SQLite couvre Products, Stock, Promotions et Rapports en attente. Les Commandes, Documents et Réclamations n'ont pas de fallback offline.

3. **SUPERVISEUR non distingué de DÉLÉGUÉ:** Le rôle SUPERVISEUR est traité exactement comme DÉLÉGUÉ sauf dans `DashboardViewModel.IsSuperviseur` qui charge les régions. Mais `KpiService.GetPerformanceAsync` filtre `role == "DELEGUE"` strictement et retourne rien pour SUPERVISEUR.

4. **Photo de profil:** `EditProfilePage.xaml` affiche un badge caméra mais aucun `MediaPicker` n'est implémenté — la photo ne peut pas être changée.

5. **Filtre `Statut != 0` dans OrderList:** Les commandes Brouillon (0) sont masquées dans la liste. Mais `OrderDetailViewModel.CanCancel` les inclut. Si un Brouillon est accessible (navigation directe), le bouton "Annuler" sera visible.

6. **`HapticService.Success()`** appelé après SaveAsync dans VisitDetailViewModel — ce service n'est pas enregistré dans MauiProgram.cs dans le code lu; vérifier s'il existe comme classe statique.

## 5.5 Plan de correction par priorité

### P0 — Bloquant (corriger avant toute mise en production)

**BUG-01: Fix UpdateVisiteAsync**
```csharp
// Services/VisiteService.cs
public Task<Visite?> UpdateVisiteAsync(int id, CreateVisiteDto dto)
    => _api.PutAsync<Visite>($"fields/visites/{id}", dto);  // était PostAsync
```

**BUG-02: Fix UpdatePlanningEntryAsync**
```csharp
// Services/PlanningService.cs
public Task<Planning?> UpdatePlanningEntryAsync(int id, Planning entry)
    => _api.PutAsync<Planning>($"fields/plannings/{id}", entry);  // était PostAsync
```

### P1 — Majeur (corriger dans le prochain sprint)

**BUG-03: Fix KPI pour ADMIN/SUPERVISEUR**
```csharp
// Services/KpiService.cs — supprimer ou élargir la condition de rôle
// Retourner les données pour DELEGUE, ADMIN et SUPERVISEUR
```

**BUG-04: Exposer Statut et Notes dans VisitDetailPage.xaml**
```xml
<!-- Ajouter dans le form: -->
<Picker ItemsSource="{Binding StatutOptions}" SelectedItem="{Binding Statut}" />
<Editor Text="{Binding Notes}" Placeholder="Notes…" />
```

**BUG-05: Guard de routes**
```csharp
// Dans chaque ViewModel concerné, vérifier le rôle au chargement:
// DocumentListViewModel.LoadAsync(): if role == "MEDECIN" → GoToAsync("//products")
// OrderListViewModel.LoadAsync(): if role == "MEDECIN" → GoToAsync("//products")
```

**BUG-10: Guard MÉDECIN dans DocumentListViewModel**
```csharp
// Au début de LoadAsync():
var role = await SecureStorage.GetAsync(StorageKeys.UserRole);
if (role == "MEDECIN") { await Shell.Current.GoToAsync("//products"); return; }
```

### P2 — Améliorations

- Afficher le nom de région/wilaya au lieu de l'ID dans ProfilePage pour MÉDECIN
- Implémenter la création de réclamation indépendante pour CLIENT
- Ajouter un filtre par statut dans ReclamationListPage
- Implémenter les notifications push (Firebase/APNS)

### P3 — Fonctionnalités futures

- Export PDF des rapports de visite
- Favoris produits (local SQLite)
- Filtrage des visites par médecin/pharmacien
- Mode brouillon pour les commandes (`IsFinalValidation = false`)
- Historique des visites reçues pour le rôle MÉDECIN

---

*Document généré le 2026-05-27. Couvre l'intégralité du projet Cynapharm-Mobile tel qu'il existe dans la branche `dev/Mobile-0001`.*
