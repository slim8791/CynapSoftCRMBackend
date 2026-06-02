# Cynapharm Mobile — Plan d'Amélioration Production-Grade

> **App :** Cynapharm-Mobile (.NET 10 MAUI)  
> **État actuel :** Architecture MVVM partielle, 18 écrans, offline SQLite, RBAC, 10 services API  
> **Objectif :** App prête pour la production (fiabilité, UX soignée, maintenabilité)

---

## Vue d'ensemble des travaux

| # | Axe | Priorité | Effort |
|---|-----|----------|--------|
| 1 | Architecture MVVM | 🔴 Critique | Moyen |
| 2 | Gestion API | 🔴 Critique | Moyen |
| 3 | Gestion des erreurs | 🔴 Critique | Faible |
| 4 | Performance | 🟠 Important | Moyen |
| 5 | Mobile UX | 🟡 Qualité | Élevé |

---

## 1. Architecture MVVM

### État actuel
- `BaseViewModel` existe mais n'est pas cohérent sur tous les VMs
- Navigation faite directement via `Shell.Current.GoToAsync` sans contrat
- Paramètres de navigation en query-strings non typées
- 18 services enregistrés en **Singleton** — cycles de vie non contrôlés
- Pas de pattern Repository entre les Services et l'API

---

### 1.1 — Navigation Service typée

**Problème :** `Shell.Current.GoToAsync("orders/detail?id=123")` — pas de compilation-time safety.

**Solution :** Créer une interface `INavigationService` qui encapsule tous les appels Shell.

```
Cynapharm-Mobile/
└── Services/
    └── Navigation/
        ├── INavigationService.cs
        └── ShellNavigationService.cs
```

**`INavigationService.cs`**
```csharp
public interface INavigationService
{
    Task GoToAsync(string route);
    Task GoToAsync<TParam>(string route, TParam param) where TParam : class;
    Task GoBackAsync();
    Task GoToRootAsync();
}
```

**`ShellNavigationService.cs`**
```csharp
public class ShellNavigationService : INavigationService
{
    public Task GoToAsync(string route) => Shell.Current.GoToAsync(route);

    public Task GoToAsync<TParam>(string route, TParam param) where TParam : class
    {
        var navParam = new Dictionary<string, object> { ["param"] = param };
        return Shell.Current.GoToAsync(route, navParam);
    }

    public Task GoBackAsync() => Shell.Current.GoToAsync("..");
    public Task GoToRootAsync() => Shell.Current.GoToAsync("//login");
}
```

Enregistrer dans `MauiProgram.cs` :
```csharp
builder.Services.AddSingleton<INavigationService, ShellNavigationService>();
```

Injecter dans tous les ViewModels qui naviguent :
```csharp
// Avant
await Shell.Current.GoToAsync($"orders/detail?orderId={order.Id}");

// Après
await _navigationService.GoToAsync<Order>("orders/detail", order);
```

---

### 1.2 — QueryProperty → IQueryAttributable unifié

**Problème :** Certains VMs utilisent `[QueryProperty]` (string), d'autres rien.

**Solution :** Implémenter `IQueryAttributable` sur tous les VMs qui reçoivent des paramètres.

```csharp
// OrderDetailViewModel.cs
public partial class OrderDetailViewModel : BaseViewModel, IQueryAttributable
{
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("param", out var obj) && obj is Order order)
            LoadOrderAsync(order.Id).SafeFireAndForget();
    }
}
```

---

### 1.3 — Cycle de vie des services (Singleton → Transient)

**Problème :** Tous les services sont Singleton — les ViewModels Transient peuvent accéder à des états périmés.

**Règle à appliquer :**

| Type | Enregistrement |
|------|----------------|
| `ApiService`, `LocalDatabaseService`, `SyncService`, `AuthService` | `Singleton` ✅ (état partagé justifié) |
| `ProductService`, `OrderService`, `InventoryService`, `DocumentService`, `VisiteService`, `PlanningService`, `KpiService` | `Transient` — pas d'état interne |
| ViewModels | `Transient` ✅ |
| Views (Pages) | `Transient` ✅ |

Modifier `MauiProgram.cs` :
```csharp
builder.Services.AddTransient<ProductService>();
builder.Services.AddTransient<OrderService>();
builder.Services.AddTransient<InventoryService>();
builder.Services.AddTransient<DocumentService>();
builder.Services.AddTransient<VisiteService>();
builder.Services.AddTransient<PlanningService>();
builder.Services.AddTransient<KpiService>();
```

---

### 1.4 — BaseViewModel : contrat unifié

Compléter `BaseViewModel` avec un wrapper systématique des appels API :

```csharp
// BaseViewModel.cs — ajouts
protected async Task ExecuteAsync(Func<Task> operation)
{
    if (IsBusy) return;
    IsBusy = true;
    ErrorMessage = string.Empty;
    try
    {
        await operation();
    }
    catch (ApiException ex)
    {
        ErrorMessage = ex.Message;
    }
    catch (TaskCanceledException)
    {
        ErrorMessage = "La requête a expiré. Vérifiez votre connexion.";
    }
    catch (Exception ex)
    {
        ErrorMessage = "Une erreur inattendue s'est produite.";
        _logger.LogError("Unhandled VM error", ex);
    }
    finally
    {
        IsBusy = false;
    }
}

protected async Task<T?> ExecuteAsync<T>(Func<Task<T>> operation)
{
    if (IsBusy) return default;
    IsBusy = true;
    ErrorMessage = string.Empty;
    try { return await operation(); }
    catch (ApiException ex) { ErrorMessage = ex.Message; return default; }
    catch (TaskCanceledException) { ErrorMessage = "La requête a expiré."; return default; }
    finally { IsBusy = false; }
}
```

Remplacer dans chaque ViewModel les try-catch répétitifs par `ExecuteAsync` :
```csharp
// Avant
[RelayCommand]
async Task LoadOrdersAsync()
{
    IsBusy = true;
    try { var orders = await _orderService.GetOrdersAsync(); ... }
    catch (Exception ex) { ErrorMessage = ex.Message; }
    finally { IsBusy = false; }
}

// Après
[RelayCommand]
Task LoadOrdersAsync() => ExecuteAsync(async () =>
{
    var orders = await _orderService.GetOrdersAsync();
    Orders = new ObservableCollection<Order>(orders);
});
```

---

### 1.5 — `SafeFireAndForget` extension

Éviter les `async void` non contrôlés.

```
Services/Extensions/TaskExtensions.cs
```
```csharp
public static class TaskExtensions
{
    public static async void SafeFireAndForget(
        this Task task, Action<Exception>? onError = null)
    {
        try { await task; }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
            Debug.WriteLine($"[TASK ERROR] {ex}");
        }
    }
}
```

---

## 2. Gestion API

### État actuel
- `ApiService` centralise les appels HTTP ✅
- Pas de **retry automatique** sur échec réseau transitoire
- Pas de **refresh JWT** — le token expire → déconnexion brutale
- **Timeout global** 30s appliqué à toutes les requêtes sans distinction
- Pas de **cache HTTP** — chaque navigation recharge tout depuis l'API
- Pas de **logging structuré** des requêtes/réponses en production

---

### 2.1 — Refresh du JWT (Token Renewal)

**Problème :** Le token expire → `SessionExpired` → déconnexion immédiate, même sur une action critique.

**Solution :** Renouveler proactivement le token avant expiration via un `DelegatingHandler`.

```
Services/Api/TokenRefreshHandler.cs
```
```csharp
public class TokenRefreshHandler : DelegatingHandler
{
    private readonly IServiceProvider _services;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public TokenRefreshHandler(IServiceProvider services) => _services = services;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var authService = _services.GetRequiredService<AuthService>();

        // Renouveler si < 5 minutes avant expiration
        if (await authService.IsTokenExpiringSoonAsync(TimeSpan.FromMinutes(5)))
        {
            await _lock.WaitAsync(ct);
            try
            {
                if (await authService.IsTokenExpiringSoonAsync(TimeSpan.FromMinutes(5)))
                    await authService.RefreshTokenAsync();
            }
            finally { _lock.Release(); }
        }

        var token = await SecureStorage.GetAsync(StorageKeys.Token);
        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            ApiService.RaiseSessionExpired();

        return response;
    }
}
```

Enregistrer dans `MauiProgram.cs` :
```csharp
builder.Services.AddTransient<TokenRefreshHandler>();
builder.Services.AddHttpClient<ApiService>(client =>
{
    client.BaseAddress = new Uri(AppSettings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<TokenRefreshHandler>();
```

---

### 2.2 — Retry automatique avec Polly

```
NuGet : Microsoft.Extensions.Http.Resilience
```

```csharp
// MauiProgram.cs
builder.Services.AddHttpClient<ApiService>()
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.BackoffType = DelayBackoffType.Exponential;
        options.Retry.Delay = TimeSpan.FromSeconds(1);
        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
    });
```

---

### 2.3 — Cache en mémoire par service

```
Services/Cache/ICacheService.cs
Services/Cache/MemoryCacheService.cs
```

```csharp
public interface ICacheService
{
    Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan ttl);
    void Invalidate(string key);
    void InvalidateAll();
}

public class MemoryCacheService : ICacheService
{
    private readonly Dictionary<string, (object Value, DateTime ExpiresAt)> _cache = new();

    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan ttl)
    {
        if (_cache.TryGetValue(key, out var entry) && entry.ExpiresAt > DateTime.UtcNow)
            return (T)entry.Value;

        var result = await factory();
        if (result is not null)
            _cache[key] = (result, DateTime.UtcNow.Add(ttl));
        return result;
    }

    public void Invalidate(string key) => _cache.Remove(key);
    public void InvalidateAll() => _cache.Clear();
}
```

**TTL recommandés :**

| Ressource | TTL |
|-----------|-----|
| Catalogue produits | 10 min |
| Promotions actives | 5 min |
| Stock délégué | 3 min |
| KPIs dashboard | 2 min |
| Profil utilisateur | 30 min |
| Régions / Objectifs | 15 min |

Usage dans les services :
```csharp
public Task<List<Product>> GetProductsAsync()
    => _cache.GetOrCreateAsync("products:all",
        () => _api.GetAsync<List<Product>>(ApiRoutes.Products.Base),
        TimeSpan.FromMinutes(10));
```

---

### 2.4 — Logging HTTP structuré

```
Services/Api/HttpLoggingHandler.cs
```
```csharp
public class HttpLoggingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        Debug.WriteLine($"[HTTP →] {request.Method} {request.RequestUri}");

        HttpResponseMessage response;
        try { response = await base.SendAsync(request, ct); }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HTTP ✗] {request.RequestUri} — {ex.Message} ({sw.ElapsedMilliseconds}ms)");
            throw;
        }

        sw.Stop();
        var icon = response.IsSuccessStatusCode ? "✓" : "✗";
        Debug.WriteLine($"[HTTP {icon}] {(int)response.StatusCode} {request.RequestUri} ({sw.ElapsedMilliseconds}ms)");
        return response;
    }
}
```

---

### 2.5 — Constantes d'endpoints centralisées

Remplacer les strings hardcodées dans chaque service :

```
Services/Api/ApiRoutes.cs
```
```csharp
public static class ApiRoutes
{
    public static class Auth
    {
        public const string Login           = "api/auth/login";
        public const string ForgotPassword  = "api/auth/forgot-password";
        public const string ChangePassword  = "api/auth/change-password";
    }
    public static class Products
    {
        public const string Base       = "api/products";
        public const string Search     = "api/products/search";
        public const string Categories = "api/products/categories";
        public const string Lots       = "api/lots";
        public const string Promos     = "api/promos";
    }
    public static class Orders
    {
        public const string Base       = "api/orders";
        public const string Lines      = "api/lignes";
        public const string Complaints = "api/reclamations";
    }
    public static class Field
    {
        public const string Visites   = "api/visites";
        public const string Rapports  = "api/rapports";
        public const string Plannings = "api/plannings";
        public const string Objectifs = "api/objectifs";
        public const string Kpi       = "api/kpi";
        public const string Regions   = "api/regions";
    }
    public static class Inventory
    {
        public const string Stocks       = "api/stocks-delegue";
        public const string Movements    = "api/stock-movements";
        public const string Distributions = "api/distributions";
        public const string StocksPromo  = "api/stocks-promotionnels";
    }
    public static class Documents
    {
        public const string Factures      = "api/factures";
        public const string BonsCommande  = "api/bons-commandes";
        public const string BonsLivraison = "api/bons-livraison";
    }
}
```

---

### 2.6 — `ApiException` typée

```csharp
public class ApiException : Exception
{
    public HttpStatusCode? StatusCode { get; }
    public ApiException(string message, Exception inner, HttpStatusCode? code = null)
        : base(message, inner) => StatusCode = code;
}
```

Dans `ApiService`, distinguer les erreurs pour donner des messages contextuels :
```csharp
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
    => throw new ApiException("Serveur temporairement indisponible.", ex, HttpStatusCode.ServiceUnavailable);

catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
    => throw new ApiException("Vous n'avez pas les droits pour cette action.", ex, HttpStatusCode.Forbidden);

catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
    => throw new ApiException("Requête expirée. Vérifiez votre connexion.", ex);

catch (JsonException ex)
    => throw new ApiException("Erreur de format de réponse serveur.", ex);
```

---

## 3. Gestion des Erreurs

### État actuel
- Try-catch dupliqués dans chaque ViewModel (×18)
- Messages génériques, aucune action proposée à l'utilisateur
- Pas de crash reporting persisté
- `Debug.WriteLine` invisible en production
- Aucune distinction entre erreurs récupérables et fatales

---

### 3.1 — `IAppLogger` : logging persisté en SQLite

```
Services/Logging/IAppLogger.cs
Services/Logging/AppLogger.cs
```

```csharp
public interface IAppLogger
{
    void LogInfo(string message, string? context = null);
    void LogWarning(string message, string? context = null);
    void LogError(string message, Exception? ex = null, string? context = null);
    Task<IEnumerable<LogEntry>> GetRecentLogsAsync(int count = 100);
}
```

Ajouter la table `Log_Entries` à `LocalDatabaseService` et utiliser `IAppLogger` dans `BaseViewModel` et `ApiService`. Les logs sont stockés localement et consultables depuis l'écran Profil (section debug, visible admin uniquement).

---

### 3.2 — Composant `ErrorBanner` réutilisable

Extraire le bandeau d'erreur ad-hoc de chaque page en `ContentView` partagé.

```
Controls/ErrorBanner.xaml
```
```xml
<ContentView x:Class="Cynapharm.Controls.ErrorBanner"
             IsVisible="{Binding HasError}">
    <Border BackgroundColor="{StaticResource ErrorLight}"
            StrokeShape="RoundRectangle 8" Padding="12,8">
        <Grid ColumnDefinitions="Auto,*,Auto">
            <Label Grid.Column="0" Text="⚠️" VerticalOptions="Center"/>
            <Label Grid.Column="1" Text="{Binding ErrorMessage}"
                   TextColor="{StaticResource ErrorColor}" FontSize="13" Margin="8,0"/>
            <Button Grid.Column="2" Text="↺"
                    Command="{Binding RetryCommand}"
                    IsVisible="{Binding CanRetry}"
                    BackgroundColor="Transparent"
                    TextColor="{StaticResource ErrorColor}"/>
        </Grid>
    </Border>
</ContentView>
```

Ajouter dans `BaseViewModel` :
```csharp
public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
public bool CanRetry { get; protected set; } = true;

[RelayCommand]
protected virtual Task RetryAsync() => Task.CompletedTask;
```

---

### 3.3 — `RetryCommand` sur toutes les pages à liste

```csharp
// OrderListViewModel.cs
protected override Task RetryAsync() => LoadOrdersAsync();
```

Dans le XAML état vide :
```xml
<VerticalStackLayout IsVisible="{Binding HasError}" HorizontalOptions="Center" Spacing="12">
    <Label Text="😕" FontSize="48" HorizontalOptions="Center"/>
    <Label Text="{Binding ErrorMessage}" HorizontalTextAlignment="Center"/>
    <Button Text="Réessayer" Command="{Binding RetryCommand}" Style="{StaticResource PrimaryButton}"/>
</VerticalStackLayout>
```

---

### 3.4 — Global exception handler enrichi

```csharp
// MauiProgram.cs
AppDomain.CurrentDomain.UnhandledException += (s, e) =>
{
    var logger = services.GetService<IAppLogger>();
    logger?.LogError("UnhandledException", e.ExceptionObject as Exception, "AppDomain");
};

TaskScheduler.UnobservedTaskException += (s, e) =>
{
    var logger = services.GetService<IAppLogger>();
    logger?.LogError("UnobservedTaskException", e.Exception, "TaskScheduler");
    e.SetObserved();
};
```

---

### 3.5 — Connectivité : guard avant toute mutation

Dans `BaseViewModel`, avant tout appel de mutation (POST/PUT/DELETE) :
```csharp
protected bool RequireConnectivity()
{
    if (Connectivity.NetworkAccess != NetworkAccess.Internet)
    {
        ErrorMessage = "Cette action nécessite une connexion internet.";
        return false;
    }
    return true;
}
```

---

## 4. Performance

### État actuel
- Chargement synchrone → `ActivityIndicator` spinner bloquant
- Pas de `CancellationToken` par requête de recherche → résultats dans le désordre
- `ObservableCollection` reconstruite entièrement à chaque filtre
- Images produit sans cache ni placeholder
- Startup séquentiel (SQLite init → auth check → sync en série)

---

### 4.1 — Skeleton Loading

Remplacer `ActivityIndicator` par des placeholders animés.

```xml
<!-- SkeletonCard.xaml — simuler le layout d'une carte produit -->
<Border IsVisible="{Binding IsBusy}" HeightRequest="80" Margin="0,4">
    <toolkit:Skeleton IsActive="True"
                      BackgroundColor="{StaticResource Gray200}"
                      HeightRequest="80" CornerRadius="8"/>
</Border>
```

Afficher 3–5 skeleton cards pendant `IsBusy`, cacher quand la liste est peuplée.

---

### 4.2 — Search avec `CancellationToken` par requête

**Problème :** Saisie rapide → plusieurs requêtes parallèles → résultats dans le désordre.

```csharp
private CancellationTokenSource? _searchCts;

partial void OnSearchTextChanged(string value)
{
    _searchCts?.Cancel();
    _searchCts = new CancellationTokenSource();
    _ = SearchWithDebounceAsync(value, _searchCts.Token);
}

private async Task SearchWithDebounceAsync(string query, CancellationToken ct)
{
    await Task.Delay(300, ct);
    if (ct.IsCancellationRequested) return;

    await ExecuteAsync(async () =>
    {
        var results = await _productService.SearchAsync(query, ct);
        if (!ct.IsCancellationRequested)
            Products = new ObservableCollection<Product>(results);
    });
}
```

---

### 4.3 — `ObservableCollection` : mise à jour incrémentale

**Problème :** `Products = new ObservableCollection<Product>(list)` redessine toute la liste → jank visuel.

```csharp
// Services/Extensions/ObservableCollectionExtensions.cs
public static void UpdateFrom<T>(
    this ObservableCollection<T> collection, IEnumerable<T> newItems)
    where T : IEquatable<T>
{
    var list = newItems.ToList();
    for (int i = collection.Count - 1; i >= 0; i--)
        if (!list.Contains(collection[i])) collection.RemoveAt(i);
    for (int i = 0; i < list.Count; i++)
        if (i >= collection.Count) collection.Add(list[i]);
        else if (!collection[i].Equals(list[i])) collection[i] = list[i];
}
```

---

### 4.4 — Startup time : initialisation parallèle

```csharp
// App.xaml.cs
protected override async void OnStart()
{
    // DB init et auth check en parallèle
    var dbTask   = _localDb.InitializeAsync();
    var authTask = _authService.IsAuthenticatedAsync();
    await Task.WhenAll(dbTask, authTask);

    // Sync en arrière-plan, ne pas bloquer la navigation
    if (Connectivity.NetworkAccess == NetworkAccess.Internet)
        _ = _syncService.FlushPendingRapportsAsync();

    await NavigateBasedOnAuthAsync(authTask.Result);
}
```

---

### 4.5 — `CollectionView` : pagination uniforme

S'assurer que toutes les pages listes utilisent `RemainingItemsThreshold` :

```xml
<CollectionView ItemsSource="{Binding Items}"
                RemainingItemsThreshold="3"
                RemainingItemsThresholdReachedCommand="{Binding LoadMoreCommand}">
    <CollectionView.ItemsLayout>
        <LinearItemsLayout Orientation="Vertical" ItemSpacing="8"/>
    </CollectionView.ItemsLayout>
</CollectionView>
```

Pages concernées : `OrderListPage`, `DocumentListPage`, `VisitListPage`, `ProductListPage`.

---

### 4.6 — Vérifier XAML Compiled Bindings

S'assurer que dans le `.csproj` :
```xml
<MauiEnableXamlCBindingWithSourceCompilation>true</MauiEnableXamlCBindingWithSourceCompilation>
```

Et sur toutes les pages :
```xml
x:DataType="viewmodels:OrderListViewModel"
```

Évite la reflection à l'exécution sur tous les bindings.

---

## 5. Mobile UX

### État actuel
- Thème clair uniquement (dark mode non supporté)
- Aucun feedback haptique sur les actions
- Formulaires sans validation visuelle en temps réel
- Pas de confirmation avant de quitter un formulaire modifié
- États vides (`EmptyState`) ad-hoc dans chaque page — 17 implémentations différentes
- Pas d'animation d'entrée sur les listes

---

### 5.1 — Validation de formulaires en temps réel

`CommunityToolkit.Mvvm` inclut `ObservableValidator`. L'utiliser sur les formulaires de saisie.

```csharp
// CreateOrderViewModel.cs
[ObservableProperty]
[NotifyDataErrorInfo]
[Required(ErrorMessage = "Le client est obligatoire")]
private string _clientId = string.Empty;
```

Dans le XAML, afficher les erreurs sous le champ :
```xml
<Entry Text="{Binding Email, Mode=TwoWay}" Placeholder="Email"/>
<Label Text="{Binding GetErrors('Email')[0].ErrorMessage}"
       TextColor="{StaticResource ErrorColor}" FontSize="11"
       IsVisible="{Binding HasErrors}"/>
```

Désactiver le bouton Valider tant que `HasErrors` est vrai :
```xml
<Button Text="Confirmer" Command="{Binding SubmitCommand}"
        IsEnabled="{Binding HasErrors, Converter={StaticResource InvertedBool}}"/>
```

---

### 5.2 — Feedback haptique

Ajouter sur les actions importantes (distribution, validation, commande) :

```csharp
// BaseViewModel.cs
protected static Task HapticSuccessAsync()
    => HapticFeedback.PerformAsync(HapticFeedbackType.Click);

protected static Task HapticErrorAsync()
    => HapticFeedback.PerformAsync(HapticFeedbackType.LongPress);
```

---

### 5.3 — Confirmation avant quitter un formulaire modifié

```csharp
// BaseViewModel.cs
protected bool _hasUnsavedChanges;

public async Task<bool> CanNavigateAwayAsync()
{
    if (!_hasUnsavedChanges) return true;
    return await Shell.Current.DisplayAlert(
        "Modifications non sauvegardées",
        "Quitter sans sauvegarder ?",
        "Quitter", "Rester");
}
```

Dans le code-behind des pages formulaire :
```csharp
protected override bool OnBackButtonPressed()
{
    _ = ViewModel.CanNavigateAwayAsync().ContinueWith(t =>
    {
        if (t.Result)
            MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(".."));
    });
    return true;
}
```

---

### 5.4 — `AppTheme` : Dark Mode

Ajouter les variantes dark dans `Colors.xaml` :
```xml
<Color x:Key="PageBackgroundColor">
    <AppThemeBinding Light="#F7F7F7" Dark="#121212"/>
</Color>
<Color x:Key="CardBackgroundColor">
    <AppThemeBinding Light="#FFFFFF" Dark="#1E1E1E"/>
</Color>
<Color x:Key="TextPrimaryColor">
    <AppThemeBinding Light="#1A1A2E" Dark="#E0E0E0"/>
</Color>
<Color x:Key="BorderColor">
    <AppThemeBinding Light="#E0E0E0" Dark="#333333"/>
</Color>
```

Appliquer `AppThemeBinding` dans `Styles.xaml` pour tous les styles globaux (Background, TextColor, BorderColor).

---

### 5.5 — `EmptyStateView` component réutilisable

Remplacer les 17 états vides ad-hoc par un composant unique :

```
Controls/EmptyStateView.xaml
```
```xml
<ContentView x:Class="Cynapharm.Controls.EmptyStateView"
             IsVisible="{Binding IsEmpty}">
    <VerticalStackLayout HorizontalOptions="Center" VerticalOptions="Center" Spacing="12">
        <Label Text="{Binding Icon}" FontSize="56" HorizontalOptions="Center"/>
        <Label Text="{Binding Title}" FontSize="18" FontAttributes="Bold"
               HorizontalTextAlignment="Center"/>
        <Label Text="{Binding Subtitle}" FontSize="14"
               TextColor="{StaticResource Gray500}" HorizontalTextAlignment="Center"/>
        <Button Text="{Binding ActionLabel}"
                Command="{Binding ActionCommand}"
                IsVisible="{Binding HasAction}"
                Style="{StaticResource OutlinedButton}"/>
    </VerticalStackLayout>
</ContentView>
```

---

### 5.6 — Pull-to-refresh uniforme

S'assurer que toutes les pages avec listes ont `RefreshView` correctement configuré :

```xml
<RefreshView Command="{Binding RefreshCommand}"
             IsRefreshing="{Binding IsRefreshing}"
             RefreshColor="{StaticResource Primary}">
    <CollectionView .../>
</RefreshView>
```

Ajouter dans `BaseViewModel` :
```csharp
[ObservableProperty] bool _isRefreshing;

[RelayCommand]
async Task RefreshAsync()
{
    IsRefreshing = true;
    _cacheService?.Invalidate(CacheKey);
    await LoadAsync();
    IsRefreshing = false;
}
```

---

### 5.7 — Animations d'entrée sur les listes

```xml
<CollectionView.ItemTemplate>
    <DataTemplate>
        <Border Opacity="0">
            <Border.Triggers>
                <EventTrigger Event="Loaded">
                    <toolkit:AnimateAction PropertyName="Opacity"
                                         From="0" To="1" Duration="200"/>
                </EventTrigger>
            </Border.Triggers>
        </Border>
    </DataTemplate>
</CollectionView.ItemTemplate>
```

---

## Résumé des fichiers à créer / modifier

### Nouveaux fichiers à créer

```
Cynapharm-Mobile/
├── Controls/
│   ├── ErrorBanner.xaml + .cs
│   ├── EmptyStateView.xaml + .cs
│   └── SkeletonCard.xaml + .cs
└── Services/
    ├── Navigation/
    │   ├── INavigationService.cs
    │   └── ShellNavigationService.cs
    ├── Cache/
    │   ├── ICacheService.cs
    │   └── MemoryCacheService.cs
    ├── Logging/
    │   ├── IAppLogger.cs
    │   └── AppLogger.cs
    ├── Api/
    │   ├── ApiRoutes.cs
    │   ├── ApiException.cs
    │   ├── TokenRefreshHandler.cs
    │   └── HttpLoggingHandler.cs
    └── Extensions/
        ├── TaskExtensions.cs
        └── ObservableCollectionExtensions.cs
```

### Fichiers existants à modifier

| Fichier | Modifications |
|---------|--------------|
| `MauiProgram.cs` | DI cycles de vie, HttpClient handlers, INavigationService, ICacheService, IAppLogger |
| `App.xaml.cs` | Startup parallèle, global exception handler enrichi |
| `ViewModels/BaseViewModel.cs` | `ExecuteAsync`, `RetryCommand`, `IsRefreshing`, `HasError`, haptique, `RequireConnectivity` |
| `Services/ApiService.cs` | `ApiException`, supprimer auth header manuel (délégué au handler) |
| `Resources/Styles/Colors.xaml` | Dark mode `AppThemeBinding` |
| `Resources/Styles/Styles.xaml` | `OutlinedButton`, dark mode styles |
| Tous les ViewModels (×18) | Remplacer try-catch par `ExecuteAsync`, injecter `INavigationService`, `ICacheService` |
| Toutes les pages (×17) | `ErrorBanner`, `EmptyStateView`, `RefreshView`, skeleton UI, `x:DataType` |

---

## Ordre d'implémentation recommandé

```
Phase 1 — Fondations (2–3 jours)
  ✦ ApiException + gestion d'erreurs par type dans ApiService
  ✦ IAppLogger + AppLogger (table SQLite)
  ✦ BaseViewModel.ExecuteAsync (supprimer les try-catch dupliqués)
  ✦ ApiRoutes.cs (centralisation endpoints)

Phase 2 — Architecture (2–3 jours)
  ✦ INavigationService + ShellNavigationService
  ✦ TokenRefreshHandler + HttpLoggingHandler
  ✦ ICacheService + MemoryCacheService
  ✦ Microsoft.Extensions.Http.Resilience (retry + circuit breaker)
  ✦ Cycles de vie DI (Singleton → Transient pour services sans état)

Phase 3 — Composants UX (2–3 jours)
  ✦ ErrorBanner (control réutilisable)
  ✦ EmptyStateView (control réutilisable)
  ✦ RetryCommand sur tous les VMs de liste
  ✦ Pull-to-refresh uniforme (RefreshView + BaseViewModel.RefreshAsync)

Phase 4 — Performance (2 jours)
  ✦ CancellationToken sur les recherches (ProductList, VisitList)
  ✦ Skeleton loading (ProductList, OrderList, Dashboard)
  ✦ Startup parallèle (App.OnStart)
  ✦ x:DataType sur toutes les pages (compiled bindings)

Phase 5 — UX Finale (2–3 jours)
  ✦ Dark mode (Colors.xaml + Styles.xaml)
  ✦ Haptique sur actions critiques (distribution, validation, commande)
  ✦ Validation inline formulaires (ObservableValidator)
  ✦ Confirmation quitter formulaire modifié
  ✦ Animations d'entrée CollectionView
```

---

> **Note :** Ce plan n'inclut pas de projet de tests unitaires. Un projet xUnit + Moq ciblant les ViewModels et Services peut être ajouté en Phase 6 si souhaité.
