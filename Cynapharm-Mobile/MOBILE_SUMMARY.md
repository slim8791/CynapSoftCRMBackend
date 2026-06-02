# Cynapharm Mobile — Technical Summary

> **Platform:** .NET 10 MAUI (Android / iOS / macCatalyst / Windows)  
> **Architecture:** MVVM · Shell Navigation · Offline-First · Role-Based Access  
> **App ID:** `com.cynapharm.mobile`

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Technology Stack](#2-technology-stack)
3. [Project Structure](#3-project-structure)
4. [Application Entry Points](#4-application-entry-points)
5. [Navigation & Shell](#5-navigation--shell)
6. [Screens & Features](#6-screens--features)
7. [ViewModels](#7-viewmodels)
8. [Services](#8-services)
9. [Data Models](#9-data-models)
10. [Reusable Controls](#10-reusable-controls)
11. [Offline & Sync Architecture](#11-offline--sync-architecture)
12. [Authentication & Security](#12-authentication--security)
13. [HTTP Client & Resilience](#13-http-client--resilience)
14. [Resources & Styling](#14-resources--styling)
15. [Role-Based Access Control](#15-role-based-access-control)
16. [Key Architectural Patterns](#16-key-architectural-patterns)
17. [API Endpoint Map](#17-api-endpoint-map)

---

## 1. Project Overview

**Cynapharm Mobile** is the field-agent companion app for the CynapCRM platform. It serves pharmaceutical sales representatives (Délégués), supervisors, and client-side users (pharmacists, wholesalers).

### Core capabilities

| Feature | Description |
|---|---|
| Authentication | JWT login with proactive token-expiry detection |
| Visit management | Create, update, delete field visits; track status (PLANIFIEE / REALISEE / ANNULEE) |
| Rapport de visite | Structured visit reports with GPS capture, product selection, and offline queuing |
| Weekly planning | 7-day calendar view grouped by day |
| Product catalogue | Searchable, filterable, with lots and promotions |
| Orders | Create multi-product orders with promo pricing, pagination, reclamation submission |
| Documents | View factures, bons de commande, bons de livraison; native share |
| Stock management | Échantillons and promotional stock with local deduction |
| KPIs & Objectifs | Progress bars, achievement percentage, regional team view (supervisors) |
| Profile | View/edit user info, change password, logout |
| Offline-first | SQLite caching, pending rapport queue flushed on reconnect |

---

## 2. Technology Stack

### NuGet packages

| Package | Version | Purpose |
|---|---|---|
| Microsoft.Maui.Controls | $(MauiVersion) | Core MAUI framework |
| Microsoft.Maui.Essentials | 10.0.20 | SecureStorage, Connectivity, Geolocation, Haptic |
| CommunityToolkit.Mvvm | 8.* | `[ObservableProperty]`, `[RelayCommand]`, validators |
| CommunityToolkit.Maui | 10.* | Snackbar, `UseMauiCommunityToolkit()` |
| Microsoft.Extensions.Http | 10.* | `IHttpClientFactory`, `AddHttpClient<T>` |
| Microsoft.Extensions.Http.Resilience | 9.* | Polly retry + circuit-breaker + timeouts |
| System.IdentityModel.Tokens.Jwt | 8.* | JWT parsing, expiry validation |
| sqlite-net-pcl | 1.9.* | Local SQLite database |
| SQLitePCLRaw.bundle_green | 2.* | SQLite native bindings |
| Microsoft.Extensions.Logging.Debug | 10.* | Debug logging |

### Build configuration

```xml
<MauiXamlInflator>SourceGen</MauiXamlInflator>   <!-- XAML compiled to C# at build time -->
<ImplicitUsings>enable</ImplicitUsings>
<Nullable>enable</Nullable>
<ApplicationId>com.cynapharm.mobile</ApplicationId>
```

**`MauiXamlInflator=SourceGen`** — all XAML is compiled to strongly-typed C# at build time. Every DataTemplate requires an explicit `x:DataType`. The `x:Name` attribute must avoid C# reserved keywords.

---

## 3. Project Structure

```
Cynapharm-Mobile/
├── App.xaml / App.xaml.cs              # Application lifecycle, startup navigation
├── AppShell.xaml / AppShell.xaml.cs    # Shell routes, flyout menu, role visibility
├── MauiProgram.cs                      # DI container, HTTP client, resilience
├── AppSettings.cs                      # Typed appsettings model
│
├── Controls/
│   ├── ErrorBanner.xaml[.cs]           # Dismissable error strip
│   └── EmptyStateView.xaml[.cs]        # Empty list placeholder
│
├── Converters/
│   ├── InvertedBoolConverter.cs
│   └── IsNotNullOrEmptyConverter.cs
│
├── Models/
│   ├── Auth/                           # LoginRequest, LoginResponse, UserInfo, ChangePasswordRequest
│   ├── Common/                         # ApiResponse<T>, ApiException, StorageKeys, LogEntry, PagedResult
│   ├── Documents/                      # Facture, BonCommande, BonLivraison, DocumentSummary
│   ├── Field/                          # Visite, Rapport, Planning, Objectif, Kpi, Region
│   ├── Inventory/                      # StockDelegue, StockPromo, StockDisplayItem, StockMouvement
│   ├── Orders/                         # Order, LigneCommande, CartLine, Reclamation
│   └── Products/                       # Product, Lot, Promotion, ProductCheckItem
│
├── Services/
│   ├── Api/                            # ApiService, TokenValidationHandler, HttpLoggingHandler, ApiRoutes
│   ├── Cache/                          # ICacheService, MemoryCacheService
│   ├── Diagnostics/                    # CrashLogger
│   ├── Extensions/                     # TaskExtensions
│   ├── Logging/                        # IAppLogger, AppLogger
│   ├── Navigation/                     # INavigationService, ShellNavigationService
│   ├── Platform/                       # HapticService
│   ├── AuthService.cs
│   ├── DocumentService.cs
│   ├── InventoryService.cs
│   ├── KpiService.cs
│   ├── LocalDatabaseService.cs
│   ├── OrderService.cs
│   ├── PlanningService.cs
│   ├── ProductService.cs
│   ├── SyncService.cs
│   └── VisiteService.cs
│
├── ViewModels/
│   ├── Base/BaseViewModel.cs
│   ├── Auth/                           # LoginViewModel, ForgotPasswordViewModel
│   ├── Dashboard/                      # DashboardViewModel
│   ├── Documents/                      # DocumentListViewModel, DocumentDetailViewModel
│   ├── Objectifs/                      # ObjectifViewModel
│   ├── Orders/                         # OrderListViewModel, OrderDetailViewModel, CreateOrderViewModel
│   ├── Planning/                       # PlanningViewModel
│   ├── Products/                       # ProductListViewModel, ProductDetailViewModel
│   ├── Profile/                        # ProfileViewModel
│   ├── Rapports/                       # RapportViewModel
│   ├── Stock/                          # MyStockViewModel
│   └── Visites/                        # VisitListViewModel, VisitDetailViewModel
│
├── Views/
│   ├── Auth/                           # LoginPage, ForgotPasswordPage
│   ├── Dashboard/                      # DashboardPage
│   ├── Documents/                      # DocumentListPage, DocumentDetailPage
│   ├── Objectifs/                      # ObjectifPage
│   ├── Orders/                         # OrderListPage, OrderDetailPage, CreateOrderPage
│   ├── Planning/                       # PlanningPage
│   ├── Products/                       # ProductListPage, ProductDetailPage
│   ├── Profile/                        # ProfilePage
│   ├── Rapports/                       # RapportPage
│   ├── Stock/                          # MyStockPage
│   └── Visites/                        # VisitListPage, VisitDetailPage
│
├── Platforms/
│   ├── Android/                        # AndroidManifest.xml, MainActivity.cs
│   ├── iOS/
│   ├── MacCatalyst/
│   └── Windows/
│
└── Resources/
    ├── AppIcon/                        # appicon.svg (#1A6B3C)
    ├── Fonts/                          # OpenSans-Regular.ttf, OpenSans-Semibold.ttf
    ├── Images/
    ├── Raw/                            # appsettings.json
    ├── Splash/                         # splash.svg (#1A6B3C, 128×128)
    └── Styles/
        ├── Colors.xaml
        └── Styles.xaml
```

---

## 4. Application Entry Points

### MauiProgram.cs — DI container

```
Global exception handlers
├── AppDomain.UnhandledException      → CrashLogger.Log + IAppLogger.LogError
└── TaskScheduler.UnobservedTaskException → CrashLogger.Log + SetObserved

HTTP Client (AddHttpClient<ApiService>)
├── BaseAddress: ApiGatewayBaseUrl (Debug) | ApiGatewayBaseUrlProd (Release)
├── Timeout: Infinite (managed by resilience pipeline)
├── Handlers: TokenValidationHandler → HttpLoggingHandler
└── Resilience (AddStandardResilienceHandler)
    ├── TotalRequestTimeout:  60s
    ├── AttemptTimeout:       10s
    ├── Retry:                3 attempts, exponential backoff (1s → 2s → 4s)
    └── CircuitBreaker:       50% failure ratio, 30s sampling window

Singletons
├── LocalDatabaseService      — SQLite connection
├── IAppLogger → AppLogger
├── ICacheService → MemoryCacheService
├── INavigationService → ShellNavigationService
├── SyncService
└── AuthService

Transients (domain services)
└── ProductService, OrderService, InventoryService, VisiteService,
    PlanningService, KpiService, DocumentService

Transients (ViewModels + Views)
└── All ViewModels and Pages
```

### App.xaml.cs — lifecycle

**`CreateWindow()`** — resolves `AppShell` from DI, subscribes `shell.Loaded`.

**`OnShellLoaded()`** — startup sequence:
1. Show previous crash log (DEBUG only)
2. Parallel: `InitializeLocalDatabaseAsync()` + `IsAuthenticatedAsync()`
3. If internet: `SyncService.FlushPendingRapportsAsync()` (fire-and-forget)
4. Navigate to `//dashboard` / `//orders` (authenticated) or `//login` (not authenticated)

---

## 5. Navigation & Shell

### Shell routes (AppShell.xaml)

| FlyoutItem | Route | Page | x:Name |
|---|---|---|---|
| Tableau de bord | `//dashboard` | DashboardPage | FlyoutDashboard |
| Visites | `//visits` | VisitListPage | FlyoutVisites |
| Planning | `//planning` | PlanningPage | FlyoutPlanning |
| Catalogue | `//products` | ProductListPage | — |
| Commandes | `//orders` | OrderListPage | FlyoutOrders |
| Documents | `//documents` | DocumentListPage | FlyoutDocuments |
| Mon Stock | `//stock` | MyStockPage | FlyoutStock |
| Objectifs | `//objectifs` | ObjectifPage | FlyoutObjectifs |
| Profil | `//profile` | ProfilePage | — |
| *(hidden)* | `//login` | LoginPage | — |

### Registered sub-routes (AppShell.xaml.cs)

| Route | Page | Query params |
|---|---|---|
| `forgotpassword` | ForgotPasswordPage | — |
| `visits/detail` | VisitDetailPage | `visiteId` |
| `visits/rapport` | RapportPage | `visiteId` |
| `products/detail` | ProductDetailPage | `productId` |
| `orders/detail` | OrderDetailPage | `orderId` |
| `orders/create` | CreateOrderPage | `productId` (optional) |
| `documents/detail` | DocumentDetailPage | `documentType`, `documentId` |

### Navigation conventions

- `//route` — absolute navigation (resets stack)
- `route` — relative push
- `..` — pop back
- All `GoToAsync` calls wrapped in `try/catch` with `CrashLogger.Log` fallback
- All navigation from `async void` handlers runs inside `MainThread.InvokeOnMainThreadAsync`

---

## 6. Screens & Features

### Login (LoginPage)

Full-screen branded page. Three-stop diagonal gradient hero with double-ring pharmacy cross logo and tagline pill. White form card with rounded top corners.

- Email entry (keyboard: Email), password entry with eye-toggle
- Gradient submit button with shadow, loading indicator
- Inline error border (red, conditional)
- Forgot password tap → `forgotpassword` route

### Forgot Password (ForgotPasswordPage)

Similar hero design. On success, form hides and a triple-ring checkmark success state appears with 3-second auto-back.

### Dashboard (DashboardPage)

Role-aware summary page with pull-to-refresh.

- **Welcome banner**: user name, visit count (DELEGUE) or "Vue Superviseur" badge, avatar circle with role initials
- **Quick actions**: adapts to role (DELEGUE: Visites + Planning / SUPERVISEUR: Objectifs + Catalogue)
- **KPIs**: CollectionView with `x:DataType="field:Kpi"` — Indicateur, Période, Valeur
- **Objectifs**: CollectionView with progress bars — Réalisé vs. Cible
- **Régions** (SUPERVISEUR only): CollectionView with location icon, Nom, Id

### Visit List (VisitListPage)

- Date-range pickers (Du / Au), status chips (Tous / PLANIFIEE / REALISEE / ANNULEE)
- Debounced reload (400ms) on filter change
- FAB: "+ Nouvelle visite" → `visits/detail` (no id = create mode)
- EmptyStateView when no results

### Visit Detail (VisitDetailPage)

- QueryProperty: `visiteId` (0 = new visit)
- Fields: ClientName, VisiteDate (DatePicker), Statut (Picker), Notes (Editor)
- `IsDirty` flag: set on any field change; `OnBackButtonPressed` prompts confirmation dialog before discarding
- Actions: Save (create/update), Submit rapport (`//visits/rapport?visiteId=N`), Delete (existing only)

### Planning (PlanningPage)

- Week navigator (Previous / Next buttons, WeekLabel showing date range)
- 7 `PlanningDayGroup` items: DayLabel, `IsToday` highlight, list of visits for that day
- "+" button per day → `visits/detail`

### Rapport de Visite (RapportPage)

- QueryProperty: `visiteId`
- **GPS strip**: shows last-known position immediately (non-blocking), captures precise position on submit
- **Content**: multi-line Editor (min 20 characters, validated), Résultat Picker (POSITIF / NEGATIF / EN_ATTENTE)
- **Products**: CollectionView of `ProductCheckItem` with checkboxes
- **Submit logic**:
  - Online: `VisiteService.CreateRapportAsync()`
  - Offline: `LocalDatabaseService.InsertPendingRapportAsync()` → queued for sync

### Product List (ProductListPage)

- Search field with clear button, debounced 300ms
- Category filter chips (horizontal scroll)
- Result count: "{N} produit(s)"
- Product card: image/emoji placeholder, Nom, Catégorie + Référence, Prix, "Inactif" badge
- `_loaded` guard prevents redundant re-loads on back-navigation

### Product Detail (ProductDetailPage)

- QueryProperty: `productId`
- Hero image (180px), full product info
- **Lots**: CollectionView — NuméroLot, DateExpiration, QuantitéDisponible
- **Promotions**: CollectionView — Titre, RemisePourcentage, DateFin
- "Ajouter à une commande" → `//orders/create?productId=N`

### Order List (OrderListPage)

- Status filter chips (Tous / EN_ATTENTE / CONFIRMEE / LIVREE / ANNULEE)
- Pull-to-refresh + load-more pagination (`RemainingItemsThreshold: 3`)
- FAB: "+ Nouvelle commande" → `//orders/create`

### Order Detail (OrderDetailPage)

- QueryProperty: `orderId`
- Header: NuméroCommande, DateCommande, Statut, MontantTotal TTC
- Articles: CollectionView — ProductNom, Quantité, PrixUnitaire, SousTotal
- Inline réclamation form (toggle): Motif + Description → `OrderService.CreateReclamationAsync()`

### Create Order (CreateOrderPage)

3-step wizard with step indicator:

| Step | Content |
|---|---|
| 1 — Produits | Product search, quantity input, cart preview with promo pricing |
| 2 — Récapitulatif | Cart review with delete per line, totals + savings |
| 3 — Confirmation | Delivery notes, total, Confirm button |

- `OnBackButtonPressed` prompts if cart is non-empty
- Promo prices shown in green, original price struck through
- Cart persisted as draft (`"draft_cart"` cache key)

### My Stock (MyStockPage)

- Segment control: Échantillons (0) / Stock Promo (1)
- Stock card: ProductNom, quantity badge (red if `CanDistribute=false`), expiry label
- "Distribuer" button (Échantillons only) → deducts 1 from SQLite, haptic feedback

### Objectifs (ObjectifPage)

- Global achievement badge (% of average)
- CollectionView with ProgressBar per objectif (ValeurActuelle / ValeurCible)

### Documents (DocumentListPage / DocumentDetailPage)

- Tab buttons: Factures / Bons de commande / Bons de livraison
- Document card: colored left-bar accent, Numéro, Date, Montant, Statut badge
- Detail: type-specific fields, **Share** button (native `Share.RequestAsync`)

### Profile (ProfilePage)

- Header: avatar circle with initials, name, role badge
- **Info card** (view/edit toggle): Name, Email, Téléphone
- **Password card**: CurrentPassword, NewPassword, ConfirmPassword → `AuthService.ChangePasswordAsync()`
- **Logout**: clears SecureStorage, navigates `//login`

---

## 7. ViewModels

### BaseViewModel

All ViewModels inherit `ObservableValidator` (CommunityToolkit.Mvvm) via `BaseViewModel`.

**Observable properties:** `IsBusy`, `IsRefreshing`, `Title`, `ErrorMessage`, `IsOffline`  
**Computed:** `HasError`

**`ExecuteAsync(Func<Task>)`** — the unified execution wrapper:
- Sets `IsBusy = true`, clears `ErrorMessage`
- Catches `ApiException`, `HttpRequestException`, `TaskCanceledException`, `OperationCanceledException`, `Exception`
- Calls `HapticService.Error()` on error
- Sets `IsBusy = false`, `IsRefreshing = false` in `finally`

**`CheckConnectivityAsync()`** — returns false + shows Snackbar when offline  
**`SaveCacheAsync<T>` / `LoadCacheAsync<T>`** — JSON file cache in `AppDataDirectory`

### ViewModel summary

| ViewModel | Key Observables | Commands | API Calls |
|---|---|---|---|
| LoginViewModel | Email, Password, IsPasswordHidden | Login, TogglePassword, GoToForgotPassword | `auth/login` |
| ForgotPasswordViewModel | Email, SuccessMessage | SendReset, GoBack | `auth/forgot-password` |
| DashboardViewModel | UserDisplayName, UserRole, IsSuperviseur, TodayVisitCount | LoadDashboard, GoToVisits, GoToPlanning, GoToObjectifs | `kpi`, `objectifs`, `regions`, `visites` |
| VisitListViewModel | FilterStartDate, FilterEndDate, FilterStatus | LoadVisites, GoToDetail, CreateVisit, SetStatusFilter | `visites` |
| VisitDetailViewModel | ClientName, VisiteDate, Notes, Statut, IsDirty | Load, Save, Delete, GoToRapport | `visites/{id}` |
| PlanningViewModel | WeekStart, WeekLabel, WeekDays | PreviousWeek, NextWeek, LoadWeek, AddVisit | `plannings` |
| RapportViewModel | Contenu, Resultat, ProduitsDiscutes, GeoStatus, CanSubmit | LoadProduits, PreCaptureLocation, Submit | `rapports`, `products` |
| ProductListViewModel | SearchQuery, SelectedCategory, Products, ProductCount | Load, Refresh, SetCategory, ClearSearch, GoToDetail | `products`, `products/categories` |
| ProductDetailViewModel | Product, Lots, Promotions | Load, AddToOrder | `products/{id}`, `lots`, `promos` |
| OrderListViewModel | StatusFilter, Orders, HasMore | Load, LoadMore, Refresh, GoToDetail, CreateOrder, SetStatusFilter | `orders` |
| OrderDetailViewModel | Order, Lignes, ShowReclamationForm | Load, ToggleReclamation, SubmitReclamation | `orders/{id}`, `reclamations` |
| CreateOrderViewModel | CurrentStep, CartLines, CartTotal, CartSavings | SearchProduct, SelectProduct, AddLine, RemoveLine, PreviousStep, NextStep, SubmitOrder | `products/search`, `orders` |
| MyStockViewModel | ActiveSegment, StockLines | Load, Refresh, SetSegment, DistributeSample | `stocks-delegue`, `stocks-promotionnels` |
| ObjectifViewModel | GlobalAchievement, Objectifs | Load | `objectifs` |
| DocumentListViewModel | DocumentType, SelectedTypeIndex, Documents | Load, Refresh, SetTypeIndex, GoToDetail | `factures`, `bons-commandes`, `bons-livraison` |
| DocumentDetailViewModel | DocumentType, DocumentId, Facture, BonCommande, BonLivraison | Load, Share | `factures/{id}`, `bons-commandes/{id}`, `bons-livraison/{id}` |
| ProfileViewModel | User, IsEditing, AvatarInitials | Load, Edit, Save, ChangePassword, Logout | `auth/me`, `auth/change-password` |

---

## 8. Services

### AuthService (Singleton)

Manages JWT token lifecycle. Stores all auth data in `SecureStorage`.

| Method | Description |
|---|---|
| `LoginAsync(LoginRequest)` | POST `auth/login`, stores JWT + expiry + role + userId + name |
| `IsAuthenticatedAsync()` | Reads JWT from SecureStorage, validates `exp` claim > `DateTime.UtcNow` |
| `GetUserRoleAsync()` | Returns role from SecureStorage |
| `IsTokenExpiringSoonAsync(threshold)` | Returns `true` if token exists AND expires within threshold; `false` if no token |
| `GetCurrentUserAsync()` | GET `auth/me` |
| `ChangePasswordAsync(request)` | PUT `auth/change-password` |
| `ForgotPasswordAsync(email)` | POST `auth/forgot-password` |
| `Logout()` | Removes all SecureStorage keys, clears HTTP auth header |

**SecureStorage keys** (`StorageKeys` constants): `JwtToken`, `TokenExpiry`, `UserRole`, `UserId`, `UserName`

### ApiService (Transient, registered via IHttpClientFactory)

Base HTTP service with token injection and unified response handling.

- `PrepareAuthHeaderAsync()` — injects `Authorization: Bearer {token}` before each call
- `HandleResponseAsync<T>()` — detects `{"IsSuccess": bool, "Result": T}` wrapper, falls back to direct deserialization; raises `SessionExpired` on 401
- Static event `SessionExpired` — triggers `App.OnSessionExpired` → navigate to `//login`

### TokenValidationHandler (DelegatingHandler)

Sits first in the HTTP pipeline. Before every outbound request:
1. Calls `IsTokenExpiringSoonAsync(5 minutes)`
2. If `true` (token exists and expires soon): fires `SessionExpired`, returns synthetic `401` without sending the request
3. Also handles `401` responses from the server

> **Important**: returns `false` when no token is stored — ensures the login call is never blocked.

### LocalDatabaseService (Singleton)

SQLite database at `FileSystem.AppDataDirectory/CynapharmLocal.db`.

| Table | Purpose |
|---|---|
| `Product_Cache` | Offline product search |
| `Stock_Local` | Offline stock deduction for sample distribution |
| `Pending_Rapports` | Rapports written while offline, pending sync |
| `Promotion_Cache` | Offline promotion lookup for order cart |
| `Log_Entries` | App-level log storage (capped at 515 entries) |

Key methods: `SearchProductsAsync`, `SeedProductsAsync`, `GetStockAsync`, `DeductStockAsync`, `InsertPendingRapportAsync`, `GetPendingRapportsAsync`, `MarkRapportSyncedAsync`

### SyncService (Singleton)

Called on app start (if online) and on every `ConnectivityChanged` event.

`FlushPendingRapportsAsync()` — reads `IsSynced=false` rows from `Pending_Rapports`, POSTs each to `api/rapports`, marks as synced on success.

### MemoryCacheService (Singleton)

In-memory TTL cache used by `MyStockViewModel` (5-minute TTL) and others.

Methods: `GetOrCreateAsync<T>(key, factory, ttl)`, `Invalidate(key)`, `Clear()`

### CrashLogger (static)

Writes exception chain (type → message → stack → InnerException, depth 10) to `crash_log.txt` in `AppDataDirectory`. In DEBUG, also shows a `DisplayAlert` on-device immediately. `ReadAndClear()` is called on the next launch to surface the previous crash to the developer.

### HapticService (static)

| Method | Haptic type | When used |
|---|---|---|
| `Light()` | Click | Minor interactions |
| `Success()` | LongPress | Successful save/submit |
| `Error()` | LongPress | Error state in ExecuteAsync |

---

## 9. Data Models

### Auth

| Model | Properties |
|---|---|
| `LoginRequest` | `UserName` (string), `Password` (string) |
| `LoginResponse` | `Token` (string), `Expiry` (DateTime), `User` (UserInfo) |
| `UserInfo` | `Id`, `Name`, `Email`, `Role`, `RegionId`, `Telephone`, `Adresse` |
| `ChangePasswordRequest` | `Email`, `CurrentPassword`, `NewPassword` |

### Field

| Model | Key properties |
|---|---|
| `Visite` | `Id`, `ClientNom`, `ClientType`, `DateVisite`, `Statut`, `Notes` |
| `Rapport` | `Id`, `VisiteId`, `Contenu`, `Resultat`, `ProduitsDiscutes` (JSON), `Latitude`, `Longitude` |
| `Planning` | `Id`, `ClientNom`, `DatePlanifiee`, `ClientId` |
| `Objectif` | `TypeObjectif`, `ValeurActuelle`, `ValeurCible`, `Periode`, `ProgressValue` (computed) |
| `Kpi` | `Indicateur`, `Valeur`, `Periode` |
| `Region` | `Id`, `Nom` |

### Products

| Model | Key properties |
|---|---|
| `Product` | `Id`, `Nom`, `Reference`, `Categorie`, `PrixUnitaire`, `Description`, `ImageUrl`, `Actif` |
| `Lot` | `Id`, `NumeroLot`, `ProductId`, `DateExpiration`, `QuantiteDisponible` |
| `Promotion` | `Id`, `Titre`, `RemisePourcentage`, `DateDebut`, `DateFin`, `ProductId` |
| `ProductCheckItem` | `ProductId`, `ProductNom`, `ProductReference`, `IsSelected` |

### Orders

| Model | Key properties |
|---|---|
| `Order` | `Id`, `NumeroCommande`, `DateCommande`, `Statut`, `MontantTotal`, `Notes` |
| `LigneCommande` | `ProductNom`, `Quantite`, `PrixUnitaire`, `SousTotal` |
| `CartLine` | `ProductNom`, `Quantite`, `PrixUnitaire`, `HasPromo`, `PromoTitre`, `PrixOriginal`, `EconomieTotale`, `SousTotal` |
| `Reclamation` | `CommandeId`, `Motif`, `Description`, `Statut` |

### Inventory

| Model | Key properties |
|---|---|
| `StockDelegue` | `ProductId`, `ProductNom`, `QuantiteAllouee`, `QuantiteRestante`, `DateExpiration` |
| `StockPromo` | `ProductId`, `ProductNom`, `QuantiteTotale`, `PromoTitre`, `RemisePourcentage` |
| `StockDisplayItem` | `ProductNom`, `QuantiteLabel`, `ExpiryLabel`, `CanDistribute`, `IsEchantillon` |

### Documents

| Model | Key properties |
|---|---|
| `DocumentSummary` | `Id`, `Numero`, `Date`, `Type`, `Statut`, `Montant` |
| `Facture` | `NumeroFacture`, `DateFacture`, `Statut`, `MontantHT`, `TVA`, `MontantTTC` |
| `BonCommande` | `NumeroBon`, `DateEmission`, `Statut`, `MontantTotal` |
| `BonLivraison` | `NumeroBon`, `DateLivraison`, `Statut` |

### Common

- `ApiResponse<T>` — `IsSuccess` (bool), `Result` (T), `Message` (string), `Errors` (List\<string\>)
- `ApiException` — user-facing French error message, HTTP status code
- `LogEntry` — SQLite-stored app log (context, level, message, timestamp)

---

## 10. Reusable Controls

### ErrorBanner

```xml
<controls:ErrorBanner Message="{Binding ErrorMessage}" />
```

Auto-shows/hides based on `Message`. Red background (`ErrorLight`), warning icon, dismiss button. Implemented as ContentView with BindableProperties `Message` and `DismissCommand`. Registered as `x:Name="rootView"` to avoid C# keyword conflict with source generation.

### EmptyStateView

```xml
<controls:EmptyStateView IsEmpty="{Binding HasNoData}"
                         Icon="📭"
                         Title="Aucun résultat"
                         Subtitle="Modifiez vos filtres"
                         ActionLabel="Réessayer"
                         ActionCommand="{Binding RetryCommand}" />
```

Centered placeholder for empty lists. Optional subtitle and action button (conditional visibility via `HasSubtitle`, `HasAction`).

### Converters

| Converter | Input → Output |
|---|---|
| `InvertedBoolConverter` | `bool` → `!bool` |
| `IsNotNullOrEmptyConverter` | `string?` → `bool` (true if not null/empty) |

---

## 11. Offline & Sync Architecture

```
Online flow
  API call → TokenValidationHandler → HttpLoggingHandler → Polly → Gateway → Service

Offline flow (Rapport de visite)
  RapportViewModel.SubmitCommand
    ↓ no internet detected
  LocalDatabaseService.InsertPendingRapportAsync(entry)
    ↓ stored in SQLite Pending_Rapports

Sync on reconnect
  App.OnConnectivityChanged (NetworkAccess.Internet)
    → SyncService.FlushPendingRapportsAsync()
        ↓ reads IsSynced=false rows
        ↓ POSTs each to api/rapports
        ↓ marks IsSynced=true on success

Offline product search
  ProductListViewModel.LoadCommand → seeds LocalDatabaseService.SeedProductsAsync()
  CreateOrderViewModel.SearchProductCommand (offline) → LocalDatabaseService.SearchProductsAsync()

Offline stock distribution
  MyStockViewModel.DistributeSampleCommand → LocalDatabaseService.DeductStockAsync()
```

---

## 12. Authentication & Security

### JWT flow

```
Login → AuthService.LoginAsync()
  → POST api/auth/login (UserName, Password)
  → JWT stored in SecureStorage["JwtToken"]
  → Role, UserId, Name stored in SecureStorage

Every API call → TokenValidationHandler
  → IsTokenExpiringSoonAsync(5 min)
      - No token   → false (let request through)
      - Expiring   → fire SessionExpired, return 401
      - Valid      → continue
  → HandleResponseAsync: 401 response → HandleUnauthorized() → SessionExpired

SessionExpired event → App.OnSessionExpired
  → ApplyRoleVisibility("") → all flyout items hidden
  → GoToAsync("//login")
```

### SecureStorage keys

| Key | Value |
|---|---|
| `JwtToken` | Raw JWT string |
| `TokenExpiry` | ISO 8601 string (`jwt.ValidTo.ToString("O")`) |
| `UserRole` | e.g., `"DELEGUE"` |
| `UserId` | User ID as string |
| `UserName` | Display name |

---

## 13. HTTP Client & Resilience

Configured in `MauiProgram.cs` via `AddStandardResilienceHandler`:

```
Request pipeline:
  [App code]
    → TokenValidationHandler      (proactive JWT check)
    → HttpLoggingHandler          (debug logging)
    → Polly TotalRequestTimeout   (60s ceiling)
    → Polly Retry                 (3 attempts, exponential: 1s → 2s → 4s)
    → Polly CircuitBreaker        (50% fail ratio, 30s sampling)
    → Polly AttemptTimeout        (10s per attempt)
    → [Network]
```

**Important:** `HttpClient.Timeout` is set to `Timeout.InfiniteTimeSpan` — all timeout enforcement is delegated to the Polly pipeline.

**Validation constraint** (enforced at startup): `SamplingDuration ≥ 2 × AttemptTimeout`. Current values: `30s ≥ 2 × 10s`. ✓

---

## 14. Resources & Styling

### Color palette (Colors.xaml)

| Key | Value | Usage |
|---|---|---|
| `Primary` | `#1A6B3C` | Brand green, buttons, accents |
| `PrimaryDark` | `#124D2B` | Gradient end, pressed states |
| `PrimaryLight` | `#E8F5EE` | Badges, light backgrounds |
| `Secondary` | `#F5A623` | Orange accent, planning buttons |
| `ErrorColor` | `#D32F2F` | Error text, error border |
| `ErrorLight` | `#FFEBEE` | Error banner background |
| `SuccessColor` | `#388E3C` | Success states |
| `WarningColor` | `#F57C00` | Warning text |
| `WarningLight` | `#FFF3E0` | Warning background |
| `PageBackgroundColor` | `#F7F7F7` | Page background (light theme) |
| `CardBackgroundColor` | `#FFFFFF` | Card surfaces |
| `Gray100–900` | Various | Text and border hierarchy |

### Global styles (Styles.xaml)

- **Button**: Primary background, white text, 8px corner radius, bold
- **Entry**: `AppThemeBinding` Light=White / Dark=#2A2A2A
- **Label**: `AppThemeBinding` Light=Gray900 / Dark=#EFEFEF, OpenSansRegular font
- **ContentPage**: `AppThemeBinding` Light=PageBackgroundColor / Dark=#121212
- **CardStyle** (Border): White background with Gray200 stroke, 10px radius

### Fonts

- `OpenSansRegular` — body text
- `OpenSansSemibold` — headings, bold labels

---

## 15. Role-Based Access Control

`AppShell.ApplyRoleVisibility(string role)` is called:
- After login (in `LoginViewModel`)
- On startup (if already authenticated, in `App.OnShellLoaded`)
- On session expiry (visibility cleared)

| Role | Visible flyout items |
|---|---|
| `DELEGUE` | Dashboard, Visites, Planning, Stock, Objectifs, Commandes |
| `SUPERVISEUR` | Dashboard, Objectifs |
| `PHARMACIEN` / `GROSSISTE` / `CLIENT` | Commandes, Documents |
| *(no role / expired)* | None (login only) |

Post-login navigation target:
- `DELEGUE`, `SUPERVISEUR`, `ADMIN` → `//dashboard`
- All other roles → `//orders`

---

## 16. Key Architectural Patterns

### MVVM with source generation

All ViewModels use `[ObservableProperty]` and `[RelayCommand]` attributes from CommunityToolkit.Mvvm. Source generators emit `OnXyzChanged` partial methods, property-changed notifications, and command wrappers at compile time.

### ExecuteAsync pattern

Every command body is wrapped in `BaseViewModel.ExecuteAsync()` which provides unified: IsBusy guard, error mapping (French messages), HapticFeedback.Error on failure, always-reset IsRefreshing/IsBusy.

### Debouncing

`VisitListViewModel` (400ms) and `ProductListViewModel` (300ms) use `CancellationTokenSource` to cancel pending filter/search calls when the user changes input rapidly.

### Pagination

`OrderListViewModel` uses `_currentPage` + `HasMore` + `RemainingItemsThreshold=3` for infinite scroll. `LoadMoreCommand` uses `ExecuteUncheckedAsync` to bypass the IsBusy guard and append results.

### Offline-first

1. SQLite seeded with products and stock on first online load
2. Rapports written to `Pending_Rapports` when offline
3. `SyncService.FlushPendingRapportsAsync()` runs on start and reconnect
4. ViewModel cache via `SaveCacheAsync/LoadCacheAsync` (JSON files) for last-known-good data

### Back-navigation guard

`VisitDetailPage.OnBackButtonPressed` and `CreateOrderPage.OnBackButtonPressed` intercept the back button when `IsDirty` or cart is non-empty, and display a confirmation dialog via `DisplayAlert`.

### Crash recovery

`CrashLogger.Log` is called from:
- `AppDomain.UnhandledException`
- `TaskScheduler.UnobservedTaskException`
- `App.CreateWindow` try/catch
- `App.OnShellLoaded.Init` try/catch
- `App.OnShellLoaded` navigation try/catch
- `App.OnSessionExpired` try/catch

Previous crash is surfaced to the developer on the next DEBUG launch via a `DisplayAlert` in `OnShellLoaded`.

---

## 17. API Endpoint Map

All calls go through the Ocelot gateway at `ApiGatewayBaseUrl`. The gateway strips the leading path segment and forwards to the appropriate microservice.

| Endpoint | Method | Service | Called by |
|---|---|---|---|
| `auth/login` | POST | AuthAPI | AuthService.LoginAsync |
| `auth/forgot-password` | POST | AuthAPI | AuthService.ForgotPasswordAsync |
| `auth/change-password` | PUT | AuthAPI | AuthService.ChangePasswordAsync |
| `auth/me` | GET | AuthAPI | AuthService.GetCurrentUserAsync |
| `products` | GET | ProductAPI | ProductService.GetProductsAsync |
| `products/search` | GET | ProductAPI | ProductService.GetProductsAsync |
| `products/categories` | GET | ProductAPI | ProductService.GetCategoriesAsync |
| `products/{id}` | GET | ProductAPI | ProductService.GetProductByIdAsync |
| `lots?productId={id}` | GET | ProductAPI | ProductService.GetLotsByProductAsync |
| `promos?productId={id}` | GET | ProductAPI | ProductService.GetPromotionsAsync |
| `orders` | GET | OrderAPI | OrderService.GetOrdersAsync |
| `orders` | POST | OrderAPI | OrderService.CreateOrderAsync |
| `orders/{id}` | GET | OrderAPI | OrderService.GetOrderByIdAsync |
| `reclamations` | POST | OrderAPI | OrderService.CreateReclamationAsync |
| `visites` | GET | FieldAPI | VisiteService.GetVisitesAsync |
| `visites` | POST | FieldAPI | VisiteService.CreateVisiteAsync |
| `visites/{id}` | GET | FieldAPI | VisiteService.GetVisiteByIdAsync |
| `visites/{id}` | PUT | FieldAPI | VisiteService.UpdateVisiteAsync |
| `visites/{id}` | DELETE | FieldAPI | VisiteService.DeleteVisiteAsync |
| `rapports` | POST | FieldAPI | VisiteService.CreateRapportAsync |
| `rapports?visiteId={id}` | GET | FieldAPI | VisiteService.GetRapportsByVisiteAsync |
| `plannings?weekStart={date}` | GET | FieldAPI | PlanningService.GetPlanningAsync |
| `objectifs` | GET | FieldAPI | KpiService.GetObjectifsAsync |
| `kpi` | GET | FieldAPI | KpiService.GetKpisAsync |
| `regions` | GET | FieldAPI | KpiService.GetRegionsAsync |
| `stocks-delegue` | GET | InventoryAPI | InventoryService.GetStockDelegueAsync |
| `stocks-promotionnels` | GET | InventoryAPI | InventoryService.GetStockPromoAsync |
| `distributions` | GET | InventoryAPI | InventoryService.GetDistributionsAsync |
| `factures` | GET | DocAPI | DocumentService.GetFacturesAsync |
| `factures/{id}` | GET | DocAPI | DocumentService.GetFactureByIdAsync |
| `bons-commandes` | GET | DocAPI | DocumentService.GetBonsCommandeAsync |
| `bons-commandes/{id}` | GET | DocAPI | DocumentService.GetBonCommandeByIdAsync |
| `bons-livraison` | GET | DocAPI | DocumentService.GetBonsLivraisonAsync |
| `bons-livraison/{id}` | GET | DocAPI | DocumentService.GetBonLivraisonByIdAsync |

---

*Generated from source — Cynapharm-Mobile, .NET 10 MAUI, branch `dev/Mobile-0001`*
