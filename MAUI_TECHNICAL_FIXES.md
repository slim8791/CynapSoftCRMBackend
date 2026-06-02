# Solutions Techniques Immédiates - Cynapharm Mobile

## 🔴 PHASE 1: FIXES CRITIQUES (48h)

### Issue #1: Crash Android Runtime sur Navigation

**Symptôme:** `Android.Runtime.JavaProxyThrowable` lors de `GoToAsync("//products")`

**Root Cause:** Navigation de Shell interruptée ou exception non gérée au niveau Android/Java

**Fix #1: Ajouter Exception Handler Global**

Créer `Platforms/Android/MainActivity.cs`:
```csharp
using Android.App;
using Android.OS;
using Microsoft.Maui;

namespace Cynapharm_Mobile;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, 
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // Capture unhandled exceptions
        AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
        {
            System.Diagnostics.Debug.WriteLine($"[UNHANDLED] {args.Exception}");
            args.Handled = true; // Prevent crash, let app continue
        };
    }
}
```

**Fix #2: Améliorer Navigation avec Vérification**

Modifier `ViewModels/Auth/LoginViewModel.cs`:
```csharp
[RelayCommand]
private async Task LoginAsync()
{
    ClearError();
    if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
    {
        ErrorMessage = "Veuillez renseigner votre email et mot de passe.";
        return;
    }
    if (!await CheckConnectivityAsync()) return;
    SetBusy(true);
    try
    {
        var result = await _authService.LoginAsync(new LoginRequest(Email, Password));

        if (result == null || result.User == null)
        {
            ErrorMessage = "Email ou mot de passe incorrect.";
            return;
        }

        // Safe navigation with proper threading
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            try
            {
                // Add small delay to ensure Shell is fully initialized
                await Task.Delay(100);
                
                // Navigate with validated route
                var currentShell = Shell.Current;
                if (currentShell != null)
                {
                    await currentShell.GoToAsync("//products", smooth: true);
                }
                else
                {
                    ErrorMessage = "Erreur d'initialisation de l'app.";
                }
            }
            catch (Exception navEx)
            {
                System.Diagnostics.Debug.WriteLine($"Nav error: {navEx}");
                ErrorMessage = $"Erreur de navigation: {navEx.Message}";
            }
        });
    }
    catch (HttpRequestException ex)
    {
        ErrorMessage = ex.Message;
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Login error: {ex}");
        ErrorMessage = $"Erreur: {ex.Message}";
    }
    finally { SetBusy(false); }
}
```

**Fix #3: Ajouter Null Checks dans AppShell**

Modifier `AppShell.xaml.cs`:
```csharp
public partial class AppShell : Shell
{
    public AppShell()
    {
        try
        {
            InitializeComponent();

            Routing.RegisterRoute("forgotpassword", typeof(ForgotPasswordPage));
            Routing.RegisterRoute("visits/detail", typeof(VisitDetailPage));
            Routing.RegisterRoute("visits/rapport", typeof(RapportPage));
            Routing.RegisterRoute("products/detail", typeof(ProductDetailPage));
            Routing.RegisterRoute("orders/detail", typeof(OrderDetailPage));
            Routing.RegisterRoute("orders/create", typeof(CreateOrderPage));
            Routing.RegisterRoute("documents/detail", typeof(DocumentDetailPage));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AppShell init error: {ex}");
        }
    }

    public void ApplyRoleVisibility(string role)
    {
        try
        {
            bool isDelegue     = role == "DELEGUE";
            bool isSuperviseur = role == "SUPERVISEUR";
            bool isAdmin       = role == "ADMIN"; // ADD THIS
            bool isClient      = role is "PHARMACIEN" or "GROSSISTE" or "CLIENT";

            // Safe property access
            if (FlyoutDashboard != null)
                FlyoutDashboard.IsVisible = isAdmin || isDelegue || isSuperviseur; // FIX: Add admin
            
            if (FlyoutVisites != null)
                FlyoutVisites.IsVisible = isDelegue;
            
            if (FlyoutPlanning != null)
                FlyoutPlanning.IsVisible = isDelegue;
            
            if (FlyoutStock != null)
                FlyoutStock.IsVisible = isDelegue;
            
            if (FlyoutObjectifs != null)
                FlyoutObjectifs.IsVisible = isAdmin || isDelegue || isSuperviseur; // FIX: Add admin
            
            if (FlyoutOrders != null)
                FlyoutOrders.IsVisible = isClient || isDelegue;
            
            if (FlyoutDocuments != null)
                FlyoutDocuments.IsVisible = isClient;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ApplyRoleVisibility error: {ex}");
        }
    }
}
```

---

### Issue #2: Erreurs HTTP Vagues

**Problème:** Utilisateur reçoit message d'erreur technique au lieu de message lisible

**Solution: Créer HttpErrorHandler**

Créer `Services/HttpErrorHandler.cs`:
```csharp
using System.Net;

namespace Cynapharm_Mobile.Services;

public static class HttpErrorHandler
{
    /// <summary>
    /// Convert HTTP status code to user-friendly French message
    /// </summary>
    public static string GetUserMessage(HttpStatusCode code, string? serverMessage = null)
    {
        var message = code switch
        {
            HttpStatusCode.BadRequest => "Requête invalide. Vérifiez vos données.",
            HttpStatusCode.Unauthorized => "Session expirée. Veuillez vous reconnecter.",
            HttpStatusCode.Forbidden => "Vous n'avez pas les permissions pour cette action.",
            HttpStatusCode.NotFound => "La ressource demandée n'existe pas.",
            HttpStatusCode.Conflict => "Cette action crée un conflit. Vérifiez que les données existent.",
            HttpStatusCode.InternalServerError => "Erreur serveur. Veuillez contacter le support.",
            HttpStatusCode.ServiceUnavailable => "Service temporairement indisponible. Réessayez dans quelques secondes.",
            HttpStatusCode.BadGateway => "Problème de communication avec le serveur. Vérifiez votre connexion.",
            HttpStatusCode.GatewayTimeout => "Le serveur met trop de temps à répondre. Réessayez.",
            _ when (int)code >= 500 => "Erreur serveur. Nous travaillons pour résoudre le problème.",
            _ when (int)code >= 400 => "Erreur de requête. Vérifiez vos données.",
            _ when (int)code >= 300 => "Redirection anormale.",
            _ => serverMessage ?? "Une erreur s'est produite. Veuillez réessayer."
        };

        return message;
    }

    /// <summary>
    /// Determine if error is retryable
    /// </summary>
    public static bool IsRetryable(HttpStatusCode code)
    {
        return code switch
        {
            HttpStatusCode.RequestTimeout => true,
            HttpStatusCode.BadGateway => true,
            HttpStatusCode.ServiceUnavailable => true,
            HttpStatusCode.GatewayTimeout => true,
            HttpStatusCode.TooManyRequests => true, // 429
            _ when (int)code >= 500 => true,
            _ => false
        };
    }
}
```

**Utiliser dans ApiService:**

Modifier `Services/ApiService.cs` `HandleResponseAsync()`:
```csharp
private async Task<T?> HandleResponseAsync<T>(HttpResponseMessage response)
{
    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
    {
        HandleUnauthorized();
        return default;
    }
    
    var json = await response.Content.ReadAsStringAsync();
    
    if (!response.IsSuccessStatusCode)
    {
        // Try to extract error message from response body
        string errorMessage = null;
        try 
        {
            var errorWrapper = JsonSerializer.Deserialize<ApiResponse<object>>(json, _jsonOptions);
            errorMessage = errorWrapper?.Message;
        }
        catch { }

        // Use HttpErrorHandler for user-friendly message
        var userMessage = HttpErrorHandler.GetUserMessage(
            response.StatusCode, 
            errorMessage);
        
        throw new HttpRequestException(userMessage);
    }

    // ... rest of handling
}
```

---

### Issue #3: Déboguer avec Logcat au lieu du Debugger

**Solution: Créer classe de Logging**

Créer `Services/LoggingService.cs`:
```csharp
using System.Diagnostics;

namespace Cynapharm_Mobile.Services;

public static class LoggingService
{
    private const string LogTag = "CynapharmApp";

    public static void LogInfo(string message, string? context = null)
    {
        var fullMessage = FormatMessage("INFO", message, context);
        Debug.WriteLine(fullMessage);
        Android.Util.Log.Info(LogTag, fullMessage);
    }

    public static void LogError(Exception ex, string? context = null)
    {
        var fullMessage = FormatMessage("ERROR", ex.Message, context, ex.StackTrace);
        Debug.WriteLine(fullMessage);
        Android.Util.Log.Error(LogTag, fullMessage);
    }

    public static void LogWarning(string message, string? context = null)
    {
        var fullMessage = FormatMessage("WARN", message, context);
        Debug.WriteLine(fullMessage);
        Android.Util.Log.Warn(LogTag, fullMessage);
    }

    private static string FormatMessage(string level, string message, string? context, string? stackTrace = null)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var contextStr = !string.IsNullOrEmpty(context) ? $" [{context}]" : "";
        var result = $"{timestamp} | {level}{contextStr}: {message}";
        
        if (!string.IsNullOrEmpty(stackTrace))
            result += $"\n{stackTrace}";
        
        return result;
    }
}
```

**Utiliser dans les services:**

```csharp
// Dans LoginViewModel.cs
catch (Exception ex)
{
    LoggingService.LogError(ex, "LoginViewModel.LoginAsync");
    ErrorMessage = $"Erreur: {ex.Message}";
}

// Dans ApiService.cs
catch (Exception ex)
{
    LoggingService.LogError(ex, "ApiService.GetAsync");
    throw;
}
```

**Pour voir les logs:**
```bash
# Dans PowerShell/Terminal
adb logcat | findstr CynapharmApp

# Ou voir tous les logs
adb logcat
```

---

## 🟠 PHASE 2: IMPROVEMENTS IMPORTANTS (1 semaine)

### Feature #1: Token Refresh Automatique

Créer `Services/TokenRefreshHandler.cs`:
```csharp
using System.IdentityModel.Tokens.Jwt;

namespace Cynapharm_Mobile.Services;

public class TokenRefreshHandler
{
    private readonly AuthService _authService;

    public TokenRefreshHandler(AuthService authService)
    {
        _authService = authService;
    }

    public async Task<bool> EnsureValidTokenAsync()
    {
        var token = await SecureStorage.GetAsync(StorageKeys.JwtToken);
        if (string.IsNullOrEmpty(token))
            return false;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            var expiryUtc = jwtToken.ValidTo;
            var now = DateTime.UtcNow;
            var minutesUntilExpiry = (expiryUtc - now).TotalMinutes;

            if (minutesUntilExpiry < 0) // Already expired
            {
                LoggingService.LogWarning($"Token expired {minutesUntilExpiry * -1} minutes ago");
                return false; // Trigger re-login
            }

            if (minutesUntilExpiry < 2) // Less than 2 minutes left
            {
                LoggingService.LogInfo($"Token expiring in {minutesUntilExpiry} minutes, attempting refresh");
                // TODO: Implement refresh endpoint with backend
                // var newToken = await _authService.RefreshTokenAsync();
                // return newToken != null;
            }

            return true; // Token still valid
        }
        catch (Exception ex)
        {
            LoggingService.LogError(ex, "TokenRefreshHandler.EnsureValidTokenAsync");
            return false;
        }
    }
}
```

**Utiliser dans ApiService:**

```csharp
public async Task<T?> GetAsync<T>(string endpoint, CancellationToken ct = default)
{
    try
    {
        // CHECK TOKEN BEFORE REQUEST
        var tokenValid = await _tokenRefreshHandler.EnsureValidTokenAsync();
        if (!tokenValid)
        {
            throw new UnauthorizedAccessException("Token invalide ou expiré");
        }

        await PrepareAuthHeaderAsync();
        var uri = BuildUri(endpoint);
        var response = await _httpClient.GetAsync(uri, ct);
        return await HandleResponseAsync<T>(response);
    }
    catch (Exception ex)
    {
        LoggingService.LogError(ex, "ApiService.GetAsync");
        throw;
    }
}
```

---

## 📋 Checklist Implémentation

### Today (Samedi):
- [ ] Implémenter fix Android crash (MainActivity.cs)
- [ ] Ajouter ADMIN au visibility logic dans AppShell
- [ ] Créer HttpErrorHandler
- [ ] Tester login → navigation
- [ ] Documenter avec screenshots

### Demain (Dimanche):
- [ ] Créer LoggingService
- [ ] Ajouter logging à tous les services critiques
- [ ] Déboguer avec logcat au lieu du debugger
- [ ] Documenter commandes adb utiles

### Prochaine semaine:
- [ ] Implémenter TokenRefreshHandler
- [ ] Planifier refresh endpoint avec backend
- [ ] Ajouter Certificate Pinning
- [ ] Refactor Repository pattern

---

## 🔗 Commandes Utiles

```bash
# Connecter Android device via USB
adb devices

# Installer l'app
adb install -r bin/Release/net8.0-android/Cynapharm-Mobile.apk

# Voir les logs
adb logcat | findstr CynapharmApp

# Clear logs
adb logcat -c

# Voir seulement les erreurs
adb logcat *:E | findstr CynapharmApp

# Capture logcat dans un fichier
adb logcat > logs.txt

# Forward port pour localhost testing
adb forward tcp:5555 tcp:5555
```

---

**Status:** Draft - À approuver avant implémentation  
**Prochaine Review:** Lundi matin  
