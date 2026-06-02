using System.Globalization;
using Cynapharm_Mobile.Services;

namespace Cynapharm_Mobile;

public partial class App : Application
{
    private readonly AuthService _authService;
    private readonly SyncService _syncService;

    public App(AuthService authService, SyncService syncService)
    {
        // ── Global number format ────────────────────────────────────────────
        // Enforce period as decimal separator and non-breaking space as
        // thousands separator regardless of the device locale. This makes
        // every XAML StringFormat (N3, #,##0.000, …) and C# ToString("N3")
        // produce the correct Tunisian dinar display: "50.000", "1 050.000".
        var nfi = new NumberFormatInfo
        {
            NumberDecimalSeparator  = ".",
            NumberGroupSeparator    = " ", // non-breaking space
            NumberDecimalDigits     = 3,
            CurrencyDecimalSeparator = ".",
            CurrencyGroupSeparator  = " ",
            CurrencyDecimalDigits   = 3
        };
        var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        culture.NumberFormat   = nfi;
        CultureInfo.DefaultThreadCurrentCulture   = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        // ───────────────────────────────────────────────────────────────────

        InitializeComponent();
        _authService = authService;
        _syncService  = syncService;

        ApiService.SessionExpired += OnSessionExpired;
        Connectivity.ConnectivityChanged += OnConnectivityChanged;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        try
        {
            var shell = MauiProgram.Services.GetRequiredService<AppShell>();
            var window = new Window(shell);
            shell.Loaded += OnShellLoaded;
            return window;
        }
        catch (Exception ex)
        {
            CrashLogger.Log("App.CreateWindow", ex);
            // Return a minimal window so Android doesn't get a null reference
            return new Window(new ContentPage { Content = new Label { Text = "Startup error — see crash log" } });
        }
    }

    private async void OnShellLoaded(object? sender, EventArgs e)
    {
        if (sender is AppShell shell)
            shell.Loaded -= OnShellLoaded;

#if DEBUG
        // Show previous crash log (if any) before anything else runs
        var previousCrash = CrashLogger.ReadAndClear();
        if (!string.IsNullOrEmpty(previousCrash))
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Shell.Current.DisplayAlert(
                    "Previous Crash Log",
                    previousCrash.Length > 2000 ? previousCrash[..2000] + "\n…" : previousCrash,
                    "OK");
            });
        }
#endif

        bool isAuthenticated = false;
        try
        {
            // Heavy init in parallel — neither depends on the other
            var dbTask   = InitializeLocalDatabaseAsync();
            var authTask = _authService.IsAuthenticatedAsync();
            await Task.WhenAll(dbTask, authTask);
            isAuthenticated = authTask.Result;
        }
        catch (Exception ex)
        {
            CrashLogger.Log("App.OnShellLoaded.Init", ex);
            // isAuthenticated stays false → navigate to login
        }

        // Flush offline rapports without blocking navigation
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            _ = _syncService.FlushPendingRapportsAsync();

        // Navigation MUST happen on the main thread
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            try
            {
                if (isAuthenticated)
                {
                    var role = await _authService.GetUserRoleAsync();
                    (Shell.Current as AppShell)?.ApplyRoleVisibility(role ?? string.Empty);
                    // SUPERVISEUR role is deprecated in UI. Treated as DELEGUE for navigation.
                    var target = role switch
                    {
                        "DELEGUE" or "ADMIN" or "SUPERVISEUR" => "//dashboard",
                        "PHARMACIEN" or "GROSSISTE" or "CLIENT" => "//orders",
                        "MEDECIN" => "//products",
                        _ => "//orders"
                    };

                    // Get current route to avoid redundant navigation
                    var currentRoute = Shell.Current?.CurrentState?.Location.OriginalString ?? string.Empty;
                    if (!currentRoute.Equals(target, StringComparison.OrdinalIgnoreCase))
                    {
                        await (Shell.Current?.GoToAsync(target) ?? Task.CompletedTask);
                    }
                }
                else
                {
                    // Not authenticated — ensure we're on login
                    var currentRoute = Shell.Current?.CurrentState?.Location.OriginalString ?? string.Empty;
                    if (!currentRoute.Equals("//login", StringComparison.OrdinalIgnoreCase))
                    {
                        await (Shell.Current?.GoToAsync("//login") ?? Task.CompletedTask);
                    }
                }
            }
            catch (Exception ex)
            {
                CrashLogger.Log("App.OnShellLoaded", ex);
                await (Shell.Current?.GoToAsync("//login") ?? Task.CompletedTask);
            }
        });
    }

    private static async Task InitializeLocalDatabaseAsync()
    {
        try
        {
            var db = MauiProgram.Services.GetRequiredService<LocalDatabaseService>();
            await db.InitializeAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[App] SQLite init failed: {ex.Message}");
        }
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess == NetworkAccess.Internet)
            _ = _syncService.FlushPendingRapportsAsync();
    }

    private void OnSessionExpired(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                (Shell.Current as AppShell)?.ApplyRoleVisibility(string.Empty);
                await (Shell.Current?.GoToAsync("//login") ?? Task.CompletedTask);
            }
            catch (Exception ex)
            {
                CrashLogger.Log("App.OnSessionExpired", ex);
            }
        });
    }
}
