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
                fonts.AddFont("tabler-icons.ttf", "Tabler"); // place tabler-icons.ttf in Resources/Fonts/
            });

        // ── Base URL: dev in Debug, prod in Release ───────────────────────────
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
            // HttpClient.Timeout must exceed TotalRequestTimeout — set to infinite and let
            // the resilience pipeline manage all timeouts through its own TotalRequestTimeout.
            client.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
        })
        .AddHttpMessageHandler<TokenValidationHandler>()
        .AddHttpMessageHandler<HttpLoggingHandler>()
        .AddStandardResilienceHandler(options =>
        {
            // Global ceiling: must be > AttemptTimeout × attempts + cumulative backoff
            // 4 attempts × 10s + (1+2+4)s backoff ≈ 47s → use 60s
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);

            // 3 retries with exponential backoff (1s, 2s, 4s) for transient failures
            options.Retry.MaxRetryAttempts = 3;
            options.Retry.BackoffType      = Polly.DelayBackoffType.Exponential;
            options.Retry.Delay            = TimeSpan.FromSeconds(1);

            // Circuit breaker: open after 50% failures over 10s window
            options.CircuitBreaker.FailureRatio     = 0.5;
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);

            // Per-attempt timeout — must be < TotalRequestTimeout
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
        });

        // ── Local SQLite DB ───────────────────────────────────────────────────
        builder.Services.AddSingleton<LocalDatabaseService>();

        // ── Cross-cutting services ────────────────────────────────────────────
        builder.Services.AddSingleton<IAppLogger, AppLogger>();
        builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
        builder.Services.AddSingleton<INavigationService, ShellNavigationService>();
        builder.Services.AddSingleton<SyncService>();

        // ── Auth service (Singleton: caches JWT token across the app) ─────────
        builder.Services.AddSingleton<AuthService>();

        // ── Domain services (Transient: stateless, no shared mutable state) ───
        builder.Services.AddTransient<ProductService>();
        builder.Services.AddTransient<OrderService>();
        builder.Services.AddTransient<InventoryService>();
        builder.Services.AddTransient<VisiteService>();
        builder.Services.AddTransient<PlanningService>();
        builder.Services.AddTransient<KpiService>();
        builder.Services.AddTransient<DocumentService>();
        builder.Services.AddTransient<UserService>();

        // ── ViewModels ────────────────────────────────────────────────────────
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

        // ── Views ─────────────────────────────────────────────────────────────
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

        // Remove Android's default Material underline — fields use custom bottom-border BoxView styling
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
