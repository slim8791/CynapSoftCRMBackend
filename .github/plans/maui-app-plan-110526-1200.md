# Cynapharm-Mobile — .NET MAUI App Implementation Plan

**Document version:** 1.0  
**Date:** 2026-05-11  
**Author:** maui-project-planner agent  
**Status:** Ready for developer review

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Folder Structure](#2-folder-structure)
3. [Pages / Views](#3-pages--views)
4. [ViewModels](#4-viewmodels)
5. [Services](#5-services)
6. [Models / DTOs](#6-models--dtos)
7. [Shell Navigation & Routes](#7-shell-navigation--routes)
8. [Dependency Injection (MauiProgram.cs)](#8-dependency-injection-mauiprogramcs)
9. [Cross-Cutting Concerns](#9-cross-cutting-concerns)
10. [Implementation Phases](#10-implementation-phases)

---

## 1. Project Overview

| Property | Value |
|---|---|
| **App name** | Cynapharm-Mobile |
| **Solution project path** | `Cynapharm-Mobile/Cynapharm-Mobile.csproj` |
| **Root namespace** | `Cynapharm_Mobile` |
| **Target frameworks** | `net10.0-android` (primary mobile), `net10.0-windows10.0.19041.0` (primary desktop) |
| **Min Android API** | 21 (Android 5.0) |
| **Min Windows version** | 10.0.17763.0 (Windows 10 1809) |
| **Architecture** | MVVM + Shell navigation + Microsoft DI |
| **API Gateway base URL** | `{ApiGatewayBaseUrl}` (configured via `appsettings.json` in `Resources/Raw/`) |
| **Auth mechanism** | JWT Bearer tokens stored in `SecureStorage` |
| **UI language** | French (fr-FR) |

### Primary User Roles

| Role constant | Description | Primary screens |
|---|---|---|
| `DELEGUE` | Field sales representative | Dashboard, Visits, Planning, Rapports, My Stock, Objectives |
| `SUPERVISEUR` | Regional supervisor | Dashboard (read), Team KPIs, Reports overview |
| `PHARMACIEN` | Pharmacy client | Orders, Products, Documents, Profile |
| `GROSSISTE` | Wholesaler client | Orders, Products, Documents, Profile |
| `CLIENT` | Generic client | Orders, Products, Profile |

### NuGet Packages to Add

```xml
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.*" />
<PackageReference Include="CommunityToolkit.Maui" Version="10.*" />
<PackageReference Include="Microsoft.Extensions.Http" Version="10.*" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.*" />
```

---

## 2. Folder Structure

The full target folder tree inside `Cynapharm-Mobile/`:

```
Cynapharm-Mobile/
├── App.xaml                              # Application resources entry point
├── App.xaml.cs                           # App startup, MainPage assignment
├── AppShell.xaml                         # Shell definition: flyout + tabs + routes
├── AppShell.xaml.cs                      # Role-aware shell code-behind
├── MauiProgram.cs                        # DI registrations, fonts, plugins
├── Cynapharm-Mobile.csproj
│
├── Views/
│   ├── Auth/
│   │   ├── LoginPage.xaml
│   │   ├── LoginPage.xaml.cs
│   │   ├── ForgotPasswordPage.xaml
│   │   └── ForgotPasswordPage.xaml.cs
│   ├── Dashboard/
│   │   ├── DashboardPage.xaml
│   │   └── DashboardPage.xaml.cs
│   ├── Visites/
│   │   ├── VisitListPage.xaml
│   │   ├── VisitListPage.xaml.cs
│   │   ├── VisitDetailPage.xaml
│   │   └── VisitDetailPage.xaml.cs
│   ├── Planning/
│   │   ├── PlanningPage.xaml
│   │   └── PlanningPage.xaml.cs
│   ├── Rapports/
│   │   ├── RapportPage.xaml
│   │   └── RapportPage.xaml.cs
│   ├── Stock/
│   │   ├── MyStockPage.xaml
│   │   └── MyStockPage.xaml.cs
│   ├── Objectifs/
│   │   ├── ObjectifPage.xaml
│   │   └── ObjectifPage.xaml.cs
│   ├── Products/
│   │   ├── ProductListPage.xaml
│   │   ├── ProductListPage.xaml.cs
│   │   ├── ProductDetailPage.xaml
│   │   └── ProductDetailPage.xaml.cs
│   ├── Orders/
│   │   ├── OrderListPage.xaml
│   │   ├── OrderListPage.xaml.cs
│   │   ├── OrderDetailPage.xaml
│   │   ├── OrderDetailPage.xaml.cs
│   │   ├── CreateOrderPage.xaml
│   │   └── CreateOrderPage.xaml.cs
│   ├── Documents/
│   │   ├── DocumentListPage.xaml
│   │   ├── DocumentListPage.xaml.cs
│   │   ├── DocumentDetailPage.xaml
│   │   └── DocumentDetailPage.xaml.cs
│   └── Profile/
│       ├── ProfilePage.xaml
│       └── ProfilePage.xaml.cs
│
├── ViewModels/
│   ├── Base/
│   │   └── BaseViewModel.cs              # ObservableObject base, IsBusy, Title
│   ├── Auth/
│   │   ├── LoginViewModel.cs
│   │   └── ForgotPasswordViewModel.cs
│   ├── Dashboard/
│   │   └── DashboardViewModel.cs
│   ├── Visites/
│   │   ├── VisitListViewModel.cs
│   │   └── VisitDetailViewModel.cs
│   ├── Planning/
│   │   └── PlanningViewModel.cs
│   ├── Rapports/
│   │   └── RapportViewModel.cs
│   ├── Stock/
│   │   └── MyStockViewModel.cs
│   ├── Objectifs/
│   │   └── ObjectifViewModel.cs
│   ├── Products/
│   │   ├── ProductListViewModel.cs
│   │   └── ProductDetailViewModel.cs
│   ├── Orders/
│   │   ├── OrderListViewModel.cs
│   │   ├── OrderDetailViewModel.cs
│   │   └── CreateOrderViewModel.cs
│   ├── Documents/
│   │   ├── DocumentListViewModel.cs
│   │   └── DocumentDetailViewModel.cs
│   └── Profile/
│       └── ProfileViewModel.cs
│
├── Services/
│   ├── ApiService.cs                     # Base HTTP client with JWT injection
│   ├── AuthService.cs
│   ├── ProductService.cs
│   ├── OrderService.cs
│   ├── InventoryService.cs
│   ├── VisiteService.cs
│   ├── PlanningService.cs
│   ├── KpiService.cs
│   └── DocumentService.cs
│
├── Models/
│   ├── Auth/
│   │   ├── LoginRequest.cs
│   │   ├── LoginResponse.cs
│   │   ├── ForgotPasswordRequest.cs
│   │   ├── ChangePasswordRequest.cs
│   │   └── UserInfo.cs
│   ├── Products/
│   │   ├── Product.cs
│   │   ├── Lot.cs
│   │   └── Promotion.cs
│   ├── Orders/
│   │   ├── Order.cs
│   │   ├── LigneCommande.cs
│   │   └── Reclamation.cs
│   ├── Inventory/
│   │   ├── StockMouvement.cs
│   │   ├── StockDelegue.cs
│   │   └── StockPromo.cs
│   ├── Field/
│   │   ├── Visite.cs
│   │   ├── Planning.cs
│   │   ├── Rapport.cs
│   │   ├── Region.cs
│   │   ├── Objectif.cs
│   │   └── Kpi.cs
│   ├── Documents/
│   │   ├── Facture.cs
│   │   ├── BonCommande.cs
│   │   └── BonLivraison.cs
│   └── Common/
│       ├── ApiResponse.cs                # Generic wrapper: T Data, bool Success, string Message
│       └── PagedResult.cs               # Items, TotalCount, Page, PageSize
│
├── Resources/
│   ├── AppIcon/
│   │   ├── appicon.svg
│   │   └── appiconfg.svg
│   ├── Fonts/
│   │   ├── OpenSans-Regular.ttf
│   │   └── OpenSans-Semibold.ttf
│   ├── Images/
│   │   └── logo_cynapharm.png            # App brand logo
│   ├── Raw/
│   │   └── appsettings.json              # ApiGatewayBaseUrl, Feature flags
│   ├── Splash/
│   │   └── splash.svg
│   └── Styles/
│       ├── Colors.xaml                   # Brand palette
│       └── Styles.xaml                   # Global control styles
│
└── Platforms/
    ├── Android/
    │   ├── AndroidManifest.xml
    │   ├── MainActivity.cs
    │   ├── MainApplication.cs
    │   └── Resources/values/colors.xml
    ├── Windows/
    │   ├── App.xaml
    │   ├── App.xaml.cs
    │   ├── app.manifest
    │   └── Package.appxmanifest
    ├── iOS/                              # Keep scaffold; not primary target
    └── MacCatalyst/                      # Keep scaffold; not primary target
```

---

## 3. Pages / Views

### 3.1 Authentication Flow

#### `LoginPage` — `Views/Auth/LoginPage.xaml`
**Purpose:** Entry point for all users. Collects email/password, calls AuthAPI, persists JWT to SecureStorage, then navigates to the role-appropriate Shell.  
**Key UI elements:** Logo, email Entry, password Entry (IsPassword), Connexion Button, "Mot de passe oublié ?" link, loading indicator.  
**Navigation outcome:** On success → `//main/dashboard` (DELEGUE/SUPERVISEUR) or `//main/orders` (clients). On failure → inline error label.

#### `ForgotPasswordPage` — `Views/Auth/ForgotPasswordPage.xaml`
**Purpose:** Allows a user to request a password reset link via email.  
**Key UI elements:** Email Entry, Envoyer Button, success/error feedback label, back navigation.  
**Navigation outcome:** Returns to `LoginPage` after submission.

---

### 3.2 Shell / Main Navigation

#### `AppShell` — `AppShell.xaml`
**Purpose:** Root navigation container. Renders a flyout menu (desktop/tablet) or bottom tab bar (mobile portrait). Items shown are filtered by the authenticated user's role.  
**Flyout items by role:**

| Role | Visible flyout items |
|---|---|
| DELEGUE | Tableau de bord, Visites, Planning, Rapports, Mon Stock, Objectifs, Catalogue, Profil |
| SUPERVISEUR | Tableau de bord, Équipe KPIs, Catalogue, Profil |
| PHARMACIEN / GROSSISTE / CLIENT | Catalogue, Commandes, Documents, Profil |

---

### 3.3 Delegate (DELEGUE) Screens

#### `DashboardPage` — `Views/Dashboard/DashboardPage.xaml`
**Purpose:** Main landing page for DELEGUE and SUPERVISEUR. Shows today's visit count, monthly KPI progress bars, pending objectives summary, and quick-action buttons.  
**Key UI elements:** Welcome banner with user name, KPI summary cards (CollectionView), objective progress bars, shortcut buttons to VisitListPage and PlanningPage.

#### `VisitListPage` — `Views/Visites/VisitListPage.xaml`
**Purpose:** Displays the delegate's list of field visits. Supports filter by date range and status.  
**Key UI elements:** Date range picker, status filter chips, CollectionView of visit cards (client name, date, status badge), FAB "Nouvelle visite", pull-to-refresh.

#### `VisitDetailPage` — `Views/Visites/VisitDetailPage.xaml`
**Purpose:** View or create/edit a single visit record. Used both for detail display and inline editing.  
**Key UI elements:** Client name (read or picker), visit date/time picker, notes Editor, status picker, save/submit Button. When creating: empty form. When editing: pre-populated form with delete option.  
**Route parameter:** `visiteId` (int, optional — omit for new visit).

#### `PlanningPage` — `Views/Planning/PlanningPage.xaml`
**Purpose:** Weekly calendar grid of planned visits. The delegate can view the current week, navigate forward/backward, and tap a day to add a planned visit.  
**Key UI elements:** Week navigator header (prev/next arrows + date range label), 7-column day grid with visit chips, "Ajouter" bottom sheet triggered on day tap.

#### `RapportPage` — `Views/Rapports/RapportPage.xaml`
**Purpose:** Submit or review a visit report associated with a completed visit.  
**Key UI elements:** Visit picker (or pre-linked visit), free-text rapport Editor, product discussed multi-select, results/outcome Picker, submit Button.  
**Route parameter:** `visiteId` (int, optional — pre-links the rapport to a visit).

#### `MyStockPage` — `Views/Stock/MyStockPage.xaml`
**Purpose:** Shows the delegate's personal stock: samples allocated and promotional items.  
**Key UI elements:** Segment control (Echantillons / Stock Promo), CollectionView of stock lines (product name, quantity, expiry), pull-to-refresh.

#### `ObjectifPage` — `Views/Objectifs/ObjectifPage.xaml`
**Purpose:** Displays the delegate's assigned objectives for the current period alongside actual KPI values.  
**Key UI elements:** Period selector, list of objectives with target vs. actual (progress bar), overall achievement percentage badge.

---

### 3.4 Product Catalog (shared across all roles)

#### `ProductListPage` — `Views/Products/ProductListPage.xaml`
**Purpose:** Browseable, searchable product catalog for all users.  
**Key UI elements:** SearchBar, category filter chips, CollectionView of product cards (name, reference, thumbnail, price), pull-to-refresh, infinite scroll (load-more on scroll end).

#### `ProductDetailPage` — `Views/Products/ProductDetailPage.xaml`
**Purpose:** Full product detail including available lots and active promotions.  
**Key UI elements:** Product image, name, reference, description, price, stock indicator; expandable Lots section (CollectionView); expandable Promotions section; "Ajouter à la commande" Button (visible for client roles).  
**Route parameter:** `productId` (int).

---

### 3.5 Orders (Client / PHARMACIEN / GROSSISTE)

#### `OrderListPage` — `Views/Orders/OrderListPage.xaml`
**Purpose:** Lists all orders for the authenticated client (or all orders for DELEGUE acting on behalf of clients).  
**Key UI elements:** Status filter tabs (En attente, Confirmée, Livrée, Annulée), CollectionView of order rows (order number, date, total, status badge), FAB "Nouvelle commande", pull-to-refresh.

#### `OrderDetailPage` — `Views/Orders/OrderDetailPage.xaml`
**Purpose:** Shows the full detail of an order: header info, line items, and status history.  
**Key UI elements:** Order header card (number, date, status), CollectionView of LigneCommande (product, quantity, unit price, subtotal), totals section, "Réclamation" Button (if order is delivered), status timeline.  
**Route parameter:** `orderId` (int).

#### `CreateOrderPage` — `Views/Orders/CreateOrderPage.xaml`
**Purpose:** Multi-step order creation form.  
**Step 1:** Product search + add to cart (SearchBar, product picker, quantity entry, add-line Button).  
**Step 2:** Cart review (editable line items CollectionView, remove-line swipe action, total).  
**Step 3:** Confirmation (delivery address, notes, submit Button).  
**Key UI elements:** Step indicator, per-step content, "Suivant" / "Précédent" navigation buttons.

---

### 3.6 Documents

#### `DocumentListPage` — `Views/Documents/DocumentListPage.xaml`
**Purpose:** Lists all financial/logistics documents accessible to the user: factures, bons de commande, bons de livraison.  
**Key UI elements:** Document type filter (tabs: Factures / Bons de commande / Bons de livraison), CollectionView of document rows (number, date, amount/reference, status), pull-to-refresh.

#### `DocumentDetailPage` — `Views/Documents/DocumentDetailPage.xaml`
**Purpose:** Full detail of a single document with option to share/download.  
**Key UI elements:** Document header (number, dates, parties), line items table, totals, share Button (native share sheet).  
**Route parameters:** `documentType` (string: `facture` | `bon-commande` | `bon-livraison`), `documentId` (int).

---

### 3.7 Profile

#### `ProfilePage` — `Views/Profile/ProfilePage.xaml`
**Purpose:** User account information: view and edit name/contact info, change password, logout.  
**Key UI elements:** Avatar/initials circle, name + email + role labels, Edit toggle (inline edit), "Changer le mot de passe" section (expandable), "Se déconnecter" Button (danger style).

---

## 4. ViewModels

All ViewModels inherit from `BaseViewModel` which extends `CommunityToolkit.Mvvm.ComponentModel.ObservableObject` and provides:
- `[ObservableProperty] bool IsBusy`
- `[ObservableProperty] string Title`
- `[ObservableProperty] string ErrorMessage`
- Helpers: `SetBusy()`, `ClearError()`

### 4.1 Auth ViewModels

#### `LoginViewModel`
| Member | Type | Purpose |
|---|---|---|
| `Email` | `[ObservableProperty] string` | Bound to email Entry |
| `Password` | `[ObservableProperty] string` | Bound to password Entry |
| `LoginCommand` | `[RelayCommand] async Task` | Calls `AuthService.LoginAsync`, stores token, navigates |
| `GoToForgotPasswordCommand` | `[RelayCommand]` | Navigates to ForgotPasswordPage |

#### `ForgotPasswordViewModel`
| Member | Type | Purpose |
|---|---|---|
| `Email` | `[ObservableProperty] string` | Bound to email Entry |
| `SuccessMessage` | `[ObservableProperty] string` | Shown after successful request |
| `SendResetCommand` | `[RelayCommand] async Task` | Calls `AuthService.ForgotPasswordAsync` |
| `GoBackCommand` | `[RelayCommand]` | Shell.GoToAsync("..") |

---

### 4.2 Dashboard ViewModel

#### `DashboardViewModel`
| Member | Type | Purpose |
|---|---|---|
| `UserDisplayName` | `[ObservableProperty] string` | Greeting label |
| `KpiItems` | `ObservableCollection<KpiSummaryItem>` | KPI cards CollectionView source |
| `ObjectifItems` | `ObservableCollection<ObjectifSummary>` | Objective progress list |
| `TodayVisitCount` | `[ObservableProperty] int` | Today's visit badge |
| `LoadDashboardCommand` | `[RelayCommand] async Task` | Parallel load of KPI + Objectifs |
| `GoToVisitsCommand` | `[RelayCommand]` | Navigate to VisitListPage |
| `GoToPlanningCommand` | `[RelayCommand]` | Navigate to PlanningPage |

---

### 4.3 Visite ViewModels

#### `VisitListViewModel`
| Member | Type | Purpose |
|---|---|---|
| `Visites` | `ObservableCollection<Visite>` | CollectionView source |
| `FilterStartDate` | `[ObservableProperty] DateTime` | Date range filter start |
| `FilterEndDate` | `[ObservableProperty] DateTime` | Date range filter end |
| `FilterStatus` | `[ObservableProperty] string` | Status filter chip selection |
| `LoadVisitesCommand` | `[RelayCommand] async Task` | Loads filtered visits via VisiteService |
| `RefreshCommand` | `[RelayCommand] async Task` | Pull-to-refresh handler |
| `GoToDetailCommand` | `[RelayCommand(CanExecute = ...)] async Task<Visite>` | Navigate to VisitDetailPage |
| `CreateVisitCommand` | `[RelayCommand] async Task` | Navigate to VisitDetailPage (new) |

#### `VisitDetailViewModel`
| Member | Type | Purpose |
|---|---|---|
| `VisiteId` | `int` | Route parameter (0 = new) |
| `ClientName` | `[ObservableProperty] string` | Client name (read or picker) |
| `VisiteDate` | `[ObservableProperty] DateTime` | Visit date/time |
| `Notes` | `[ObservableProperty] string` | Visit notes |
| `Statut` | `[ObservableProperty] string` | Status picker value |
| `IsNew` | `bool` | Derived: VisiteId == 0 |
| `SaveCommand` | `[RelayCommand] async Task` | Create or update via VisiteService |
| `DeleteCommand` | `[RelayCommand] async Task` | Delete via VisiteService (existing only) |
| `LoadCommand` | `[RelayCommand] async Task` | Load existing visite by ID |

---

### 4.4 Planning ViewModel

#### `PlanningViewModel`
| Member | Type | Purpose |
|---|---|---|
| `WeekStart` | `[ObservableProperty] DateTime` | Monday of displayed week |
| `WeekLabel` | `string` (computed) | e.g., "5 mai – 11 mai 2026" |
| `WeekDays` | `ObservableCollection<PlanningDay>` | 7-day grid items |
| `PreviousWeekCommand` | `[RelayCommand]` | WeekStart -= 7 days |
| `NextWeekCommand` | `[RelayCommand]` | WeekStart += 7 days |
| `LoadWeekCommand` | `[RelayCommand] async Task` | Load planning entries for week |
| `AddVisitCommand` | `[RelayCommand] async Task<DateTime>` | Open create bottom sheet for a day |

---

### 4.5 Rapport ViewModel

#### `RapportViewModel`
| Member | Type | Purpose |
|---|---|---|
| `LinkedVisiteId` | `int` | Pre-linked visit (route param) |
| `Contenu` | `[ObservableProperty] string` | Rapport body text |
| `ProduitsDiscutes` | `ObservableCollection<ProductCheckItem>` | Multi-select products list |
| `Resultat` | `[ObservableProperty] string` | Outcome picker |
| `SubmitCommand` | `[RelayCommand] async Task` | POST rapport via VisiteService |
| `LoadProduitsCommand` | `[RelayCommand] async Task` | Load product list from ProductService |

---

### 4.6 Stock ViewModel

#### `MyStockViewModel`
| Member | Type | Purpose |
|---|---|---|
| `ActiveSegment` | `[ObservableProperty] int` | 0 = Echantillons, 1 = Stock Promo |
| `StockLines` | `ObservableCollection<StockDelegue>` | Current segment list source |
| `EchantillonStock` | `List<StockDelegue>` | Cached samples data |
| `PromoStock` | `List<StockPromo>` | Cached promo stock data |
| `LoadCommand` | `[RelayCommand] async Task` | Load both stocks via InventoryService |
| `RefreshCommand` | `[RelayCommand] async Task` | Pull-to-refresh |
| `SegmentChangedCommand` | `[RelayCommand(CanExecute = ...)] Task<int>` | Switch active segment |

---

### 4.7 Objectif ViewModel

#### `ObjectifViewModel`
| Member | Type | Purpose |
|---|---|---|
| `Periode` | `[ObservableProperty] string` | Selected period (month/quarter) |
| `Objectifs` | `ObservableCollection<ObjectifWithKpi>` | Combined objective + KPI list |
| `GlobalAchievement` | `[ObservableProperty] double` | Overall % progress |
| `LoadCommand` | `[RelayCommand] async Task` | Load objectifs + KPIs via KpiService |

---

### 4.8 Product ViewModels

#### `ProductListViewModel`
| Member | Type | Purpose |
|---|---|---|
| `SearchQuery` | `[ObservableProperty] string` | SearchBar binding |
| `SelectedCategory` | `[ObservableProperty] string` | Category filter |
| `Products` | `ObservableCollection<Product>` | CollectionView source |
| `CurrentPage` | `int` | Pagination state |
| `HasMore` | `[ObservableProperty] bool` | Show load-more indicator |
| `SearchCommand` | `[RelayCommand] async Task` | Reset + search via ProductService |
| `LoadMoreCommand` | `[RelayCommand] async Task` | Append next page |
| `GoToDetailCommand` | `[RelayCommand] async Task<Product>` | Navigate to ProductDetailPage |

#### `ProductDetailViewModel`
| Member | Type | Purpose |
|---|---|---|
| `ProductId` | `int` | Route parameter |
| `Product` | `[ObservableProperty] Product` | Loaded product |
| `Lots` | `ObservableCollection<Lot>` | Lots for this product |
| `Promotions` | `ObservableCollection<Promotion>` | Active promotions |
| `LoadCommand` | `[RelayCommand] async Task` | Load product + lots + promos |
| `AddToOrderCommand` | `[RelayCommand] async Task` | Navigate to CreateOrderPage pre-seeded |

---

### 4.9 Order ViewModels

#### `OrderListViewModel`
| Member | Type | Purpose |
|---|---|---|
| `Orders` | `ObservableCollection<Order>` | CollectionView source |
| `StatusFilter` | `[ObservableProperty] string` | Active status tab |
| `LoadCommand` | `[RelayCommand] async Task` | Load orders via OrderService |
| `RefreshCommand` | `[RelayCommand] async Task` | Pull-to-refresh |
| `GoToDetailCommand` | `[RelayCommand] async Task<Order>` | Navigate to OrderDetailPage |
| `CreateOrderCommand` | `[RelayCommand] async Task` | Navigate to CreateOrderPage |

#### `OrderDetailViewModel`
| Member | Type | Purpose |
|---|---|---|
| `OrderId` | `int` | Route parameter |
| `Order` | `[ObservableProperty] Order` | Loaded order header |
| `Lignes` | `ObservableCollection<LigneCommande>` | Order lines |
| `LoadCommand` | `[RelayCommand] async Task` | Load order + lines |
| `SubmitReclamationCommand` | `[RelayCommand] async Task` | POST reclamation via OrderService |

#### `CreateOrderViewModel`
| Member | Type | Purpose |
|---|---|---|
| `CurrentStep` | `[ObservableProperty] int` | 1, 2, or 3 |
| `CartLines` | `ObservableCollection<CartLine>` | In-progress order lines |
| `SearchQuery` | `[ObservableProperty] string` | Product search in step 1 |
| `SearchResults` | `ObservableCollection<Product>` | Step 1 search results |
| `SelectedProduct` | `[ObservableProperty] Product` | Picked product |
| `Quantity` | `[ObservableProperty] int` | Quantity entry |
| `DeliveryNotes` | `[ObservableProperty] string` | Step 3 notes |
| `CartTotal` | `decimal` (computed) | Sum of line totals |
| `AddLineCommand` | `[RelayCommand] async Task` | Add product to CartLines |
| `RemoveLineCommand` | `[RelayCommand] async Task<CartLine>` | Remove line from cart |
| `NextStepCommand` | `[RelayCommand]` | Advance step with validation |
| `PreviousStepCommand` | `[RelayCommand]` | Go back a step |
| `SubmitOrderCommand` | `[RelayCommand] async Task` | POST order via OrderService |
| `SearchProductCommand` | `[RelayCommand] async Task` | Query ProductService |

---

### 4.10 Document ViewModels

#### `DocumentListViewModel`
| Member | Type | Purpose |
|---|---|---|
| `DocumentType` | `[ObservableProperty] string` | Active tab: facture / bon-commande / bon-livraison |
| `Documents` | `ObservableCollection<DocumentSummary>` | CollectionView source (polymorphic DTO) |
| `LoadCommand` | `[RelayCommand] async Task` | Load correct document list |
| `GoToDetailCommand` | `[RelayCommand] async Task<DocumentSummary>` | Navigate with type + id params |

#### `DocumentDetailViewModel`
| Member | Type | Purpose |
|---|---|---|
| `DocumentType` | `string` | Route parameter |
| `DocumentId` | `int` | Route parameter |
| `Document` | `[ObservableProperty] object` | Loaded document (cast per type) |
| `LoadCommand` | `[RelayCommand] async Task` | Load via DocumentService based on type |
| `ShareCommand` | `[RelayCommand] async Task` | Native share sheet |

---

### 4.11 Profile ViewModel

#### `ProfileViewModel`
| Member | Type | Purpose |
|---|---|---|
| `User` | `[ObservableProperty] UserInfo` | Current user data |
| `IsEditing` | `[ObservableProperty] bool` | Toggle edit mode |
| `CurrentPassword` | `[ObservableProperty] string` | Change-password field |
| `NewPassword` | `[ObservableProperty] string` | Change-password field |
| `ConfirmPassword` | `[ObservableProperty] string` | Change-password field |
| `LoadCommand` | `[RelayCommand] async Task` | Load user info from SecureStorage / AuthService |
| `EditCommand` | `[RelayCommand]` | Toggle IsEditing |
| `SaveCommand` | `[RelayCommand] async Task` | PUT updated user info |
| `ChangePasswordCommand` | `[RelayCommand] async Task` | PUT change password |
| `LogoutCommand` | `[RelayCommand] async Task` | Clear SecureStorage, navigate to LoginPage |

---

## 5. Services

### 5.1 `ApiService` — `Services/ApiService.cs`

Base HTTP wrapper. All other services inherit from or compose `ApiService`.

**Responsibilities:**
- Maintain a single named `HttpClient` instance with `BaseAddress = {ApiGatewayBaseUrl}`
- Retrieve JWT from `SecureStorage` and attach `Authorization: Bearer <token>` header on each request
- Deserialize JSON responses into typed objects
- Map HTTP error codes to user-facing French messages
- Expose `GetAsync<T>`, `PostAsync<T>`, `PutAsync<T>`, `DeleteAsync`

**Key method signatures:**
```csharp
Task<T?> GetAsync<T>(string endpoint, CancellationToken ct = default);
Task<T?> PostAsync<T>(string endpoint, object payload, CancellationToken ct = default);
Task<T?> PutAsync<T>(string endpoint, object payload, CancellationToken ct = default);
Task<bool> DeleteAsync(string endpoint, CancellationToken ct = default);
```

---

### 5.2 `AuthService` — `Services/AuthService.cs`

| Method | Endpoint | Notes |
|---|---|---|
| `LoginAsync(LoginRequest)` | `POST /api/auth/login` | Returns `LoginResponse` (token + user info); stores token in SecureStorage |
| `ForgotPasswordAsync(string email)` | `POST /api/auth/forgot-password` | Triggers backend email |
| `ChangePasswordAsync(ChangePasswordRequest)` | `PUT /api/auth/change-password` | Requires valid JWT |
| `GetCurrentUserAsync()` | `GET /api/auth/me` | Returns `UserInfo` |
| `LogoutAsync()` | local only | Clears SecureStorage |
| `IsAuthenticatedAsync()` | local only | Checks token presence + expiry via `JwtSecurityTokenHandler` |

---

### 5.3 `ProductService` — `Services/ProductService.cs`

| Method | Endpoint | Notes |
|---|---|---|
| `GetProductsAsync(string? search, string? category, int page, int size)` | `GET /api/products?search=&category=&page=&size=` | Paginated |
| `GetProductByIdAsync(int id)` | `GET /api/products/{id}` | Full detail |
| `GetLotsByProductAsync(int productId)` | `GET /api/lots?productId={productId}` | Available lots |
| `GetPromotionsAsync(int? productId)` | `GET /api/promos?productId=` | Active promotions |
| `GetMarketingAsync(int? productId)` | `GET /api/marketing?productId=` | Marketing materials |

---

### 5.4 `OrderService` — `Services/OrderService.cs`

| Method | Endpoint | Notes |
|---|---|---|
| `GetOrdersAsync(string? status, int page, int size)` | `GET /api/orders?status=&page=&size=` | Filtered by role server-side |
| `GetOrderByIdAsync(int id)` | `GET /api/orders/{id}` | With lignes |
| `GetLignesAsync(int orderId)` | `GET /api/lignes?orderId={orderId}` | Order line items |
| `CreateOrderAsync(CreateOrderRequest)` | `POST /api/orders` | Full order with lines |
| `UpdateOrderStatusAsync(int id, string status)` | `PUT /api/orders/{id}/status` | DELEGUE/SUPERVISEUR only |
| `CreateReclamationAsync(Reclamation)` | `POST /api/reclamations` | Client-submitted claim |
| `GetReclamationsAsync(int? orderId)` | `GET /api/reclamations?orderId=` | Optional order filter |

---

### 5.5 `InventoryService` — `Services/InventoryService.cs`

| Method | Endpoint | Notes |
|---|---|---|
| `GetStockMouvementsAsync(int? productId, DateTime? from)` | `GET /api/stock-movements` | Movement history |
| `GetStockDelegueAsync()` | `GET /api/stocks-delegue` | Current delegate sample stock |
| `GetDistributionAsync()` | `GET /api/distribution` | Distribution records |
| `GetStockPromoAsync()` | `GET /api/stock-promo` | Promotional stock for delegate |

---

### 5.6 `VisiteService` — `Services/VisiteService.cs`

| Method | Endpoint | Notes |
|---|---|---|
| `GetVisitesAsync(DateTime? from, DateTime? to, string? status)` | `GET /api/visites` | Delegate's visit list |
| `GetVisiteByIdAsync(int id)` | `GET /api/visites/{id}` | Single visit |
| `CreateVisiteAsync(Visite)` | `POST /api/visites` | New visit record |
| `UpdateVisiteAsync(int id, Visite)` | `PUT /api/visites/{id}` | Edit existing visit |
| `DeleteVisiteAsync(int id)` | `DELETE /api/visites/{id}` | Remove visit |
| `CreateRapportAsync(Rapport)` | `POST /api/rapports` | Submit visit report |
| `GetRapportsAsync(int? visiteId)` | `GET /api/rapports?visiteId=` | Reports list |

---

### 5.7 `PlanningService` — `Services/PlanningService.cs`

| Method | Endpoint | Notes |
|---|---|---|
| `GetPlanningAsync(DateTime weekStart)` | `GET /api/planning?weekStart={date}` | Week's planned items |
| `CreatePlanningEntryAsync(Planning)` | `POST /api/planning` | Add a planned visit slot |
| `UpdatePlanningEntryAsync(int id, Planning)` | `PUT /api/planning/{id}` | Edit slot |
| `DeletePlanningEntryAsync(int id)` | `DELETE /api/planning/{id}` | Remove slot |

---

### 5.8 `KpiService` — `Services/KpiService.cs`

| Method | Endpoint | Notes |
|---|---|---|
| `GetObjectifsAsync(string? periode)` | `GET /api/objectifs?periode=` | Delegate objectives |
| `GetKpisAsync(string? periode)` | `GET /api/kpi?periode=` | KPI actual values |
| `GetRegionsAsync()` | `GET /api/regions` | Reference data for SUPERVISEUR |

---

### 5.9 `DocumentService` — `Services/DocumentService.cs`

| Method | Endpoint | Notes |
|---|---|---|
| `GetFacturesAsync(int page, int size)` | `GET /api/factures` | Invoice list |
| `GetFactureByIdAsync(int id)` | `GET /api/factures/{id}` | Invoice detail |
| `GetBonsCommandeAsync(int page, int size)` | `GET /api/bons-commandes` | Purchase order list |
| `GetBonCommandeByIdAsync(int id)` | `GET /api/bons-commandes/{id}` | PO detail |
| `GetBonsLivraisonAsync(int page, int size)` | `GET /api/bons-livraisons` | Delivery note list |
| `GetBonLivraisonByIdAsync(int id)` | `GET /api/bons-livraisons/{id}` | Delivery note detail |

---

## 6. Models / DTOs

### 6.1 Auth Models (`Models/Auth/`)

```csharp
// LoginRequest.cs
public record LoginRequest(string Email, string Password);

// LoginResponse.cs
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiry { get; set; }
    public UserInfo User { get; set; } = new();
}

// UserInfo.cs
public class UserInfo
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;   // "DELEGUE" | "SUPERVISEUR" | "PHARMACIEN" | ...
    public string? RegionId { get; set; }
    public string? Telephone { get; set; }
}

// ForgotPasswordRequest.cs
public record ForgotPasswordRequest(string Email);

// ChangePasswordRequest.cs
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
```

---

### 6.2 Product Models (`Models/Products/`)

```csharp
// Product.cs
public class Product
{
    public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Categorie { get; set; }
    public decimal PrixUnitaire { get; set; }
    public string? ImageUrl { get; set; }
    public bool Actif { get; set; }
}

// Lot.cs
public class Lot
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string NumeroLot { get; set; } = string.Empty;
    public DateTime DateFabrication { get; set; }
    public DateTime DateExpiration { get; set; }
    public int QuantiteDisponible { get; set; }
}

// Promotion.cs
public class Promotion
{
    public int Id { get; set; }
    public int? ProductId { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? RemisePourcentage { get; set; }
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
}
```

---

### 6.3 Order Models (`Models/Orders/`)

```csharp
// Order.cs
public class Order
{
    public int Id { get; set; }
    public string NumeroCommande { get; set; } = string.Empty;
    public DateTime DateCommande { get; set; }
    public string Statut { get; set; } = string.Empty;  // EN_ATTENTE | CONFIRMEE | LIVREE | ANNULEE
    public decimal MontantTotal { get; set; }
    public int ClientId { get; set; }
    public string? Notes { get; set; }
    public List<LigneCommande> Lignes { get; set; } = new();
}

// LigneCommande.cs
public class LigneCommande
{
    public int Id { get; set; }
    public int CommandeId { get; set; }
    public int ProductId { get; set; }
    public string ProductNom { get; set; } = string.Empty;
    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public decimal SousTotal => Quantite * PrixUnitaire;
}

// Reclamation.cs
public class Reclamation
{
    public int Id { get; set; }
    public int CommandeId { get; set; }
    public string Motif { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DateCreation { get; set; }
    public string Statut { get; set; } = string.Empty;
}
```

---

### 6.4 Inventory Models (`Models/Inventory/`)

```csharp
// StockMouvement.cs
public class StockMouvement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductNom { get; set; } = string.Empty;
    public int Quantite { get; set; }          // positive = in, negative = out
    public string TypeMouvement { get; set; } = string.Empty;
    public DateTime DateMouvement { get; set; }
}

// StockDelegue.cs
public class StockDelegue
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductNom { get; set; } = string.Empty;
    public int QuantiteAllouee { get; set; }
    public int QuantiteRestante { get; set; }
    public DateTime? DateExpiration { get; set; }
}

// StockPromo.cs
public class StockPromo
{
    public int Id { get; set; }
    public int PromotionId { get; set; }
    public string PromotionTitre { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductNom { get; set; } = string.Empty;
    public int Quantite { get; set; }
}
```

---

### 6.5 Field Models (`Models/Field/`)

```csharp
// Visite.cs
public class Visite
{
    public int Id { get; set; }
    public int DelegueId { get; set; }
    public string ClientNom { get; set; } = string.Empty;
    public string ClientType { get; set; } = string.Empty;  // PHARMACIEN | GROSSISTE
    public DateTime DateVisite { get; set; }
    public string Statut { get; set; } = string.Empty;      // PLANIFIEE | REALISEE | ANNULEE
    public string? Notes { get; set; }
}

// Planning.cs
public class Planning
{
    public int Id { get; set; }
    public int DelegueId { get; set; }
    public DateTime DatePlanifiee { get; set; }
    public string ClientNom { get; set; } = string.Empty;
    public string? Objectif { get; set; }
    public int? VisiteId { get; set; }   // Linked visit once realised
}

// Rapport.cs
public class Rapport
{
    public int Id { get; set; }
    public int VisiteId { get; set; }
    public string Contenu { get; set; } = string.Empty;
    public string? ProduitsDiscutes { get; set; }  // JSON array of product IDs
    public string Resultat { get; set; } = string.Empty;
    public DateTime DateSoumission { get; set; }
}

// Objectif.cs
public class Objectif
{
    public int Id { get; set; }
    public int DelegueId { get; set; }
    public string Periode { get; set; } = string.Empty;
    public string TypeObjectif { get; set; } = string.Empty;  // VISITES | CHIFFRE_AFFAIRES | ...
    public decimal ValeurCible { get; set; }
    public decimal? ValeurActuelle { get; set; }
}

// Kpi.cs
public class Kpi
{
    public int Id { get; set; }
    public int DelegueId { get; set; }
    public string Periode { get; set; } = string.Empty;
    public string Indicateur { get; set; } = string.Empty;
    public decimal Valeur { get; set; }
    public DateTime DateCalcul { get; set; }
}
```

---

### 6.6 Document Models (`Models/Documents/`)

```csharp
// Facture.cs
public class Facture
{
    public int Id { get; set; }
    public string NumeroFacture { get; set; } = string.Empty;
    public DateTime DateFacture { get; set; }
    public int CommandeId { get; set; }
    public decimal MontantHT { get; set; }
    public decimal TVA { get; set; }
    public decimal MontantTTC { get; set; }
    public string Statut { get; set; } = string.Empty;   // EMISE | PAYEE | ANNULEE
}

// BonCommande.cs
public class BonCommande
{
    public int Id { get; set; }
    public string NumeroBon { get; set; } = string.Empty;
    public DateTime DateEmission { get; set; }
    public int CommandeId { get; set; }
    public decimal MontantTotal { get; set; }
    public string Statut { get; set; } = string.Empty;
}

// BonLivraison.cs
public class BonLivraison
{
    public int Id { get; set; }
    public string NumeroBon { get; set; } = string.Empty;
    public DateTime DateLivraison { get; set; }
    public int CommandeId { get; set; }
    public string Statut { get; set; } = string.Empty;   // EN_TRANSIT | LIVRE | RETOURNE
}
```

---

### 6.7 Common Models (`Models/Common/`)

```csharp
// ApiResponse.cs
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
}

// PagedResult.cs
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasMore => (Page * PageSize) < TotalCount;
}
```

---

## 7. Shell Navigation & Routes

### 7.1 AppShell Structure

The shell is constructed conditionally in `App.xaml.cs` after JWT decode determines the user's role.

```xml
<!-- AppShell.xaml — simplified structure -->
<Shell x:Class="Cynapharm_Mobile.AppShell"
       xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
       xmlns:views="clr-namespace:Cynapharm_Mobile.Views">

    <!-- Auth route (not visible in flyout) -->
    <ShellContent Route="login"
                  ContentTemplate="{DataTemplate views:Auth.LoginPage}"
                  Shell.NavBarIsVisible="False" />

    <!-- Main authenticated section -->
    <FlyoutItem Title="Tableau de bord" Icon="dashboard.png" Route="dashboard"
                x:Name="FlyoutDashboard">
        <ShellContent ContentTemplate="{DataTemplate views:Dashboard.DashboardPage}" />
    </FlyoutItem>

    <FlyoutItem Title="Visites" Icon="visits.png" Route="visits"
                x:Name="FlyoutVisites">
        <ShellContent ContentTemplate="{DataTemplate views:Visites.VisitListPage}" />
    </FlyoutItem>

    <FlyoutItem Title="Planning" Icon="calendar.png" Route="planning"
                x:Name="FlyoutPlanning">
        <ShellContent ContentTemplate="{DataTemplate views:Planning.PlanningPage}" />
    </FlyoutItem>

    <FlyoutItem Title="Catalogue" Icon="products.png" Route="products">
        <ShellContent ContentTemplate="{DataTemplate views:Products.ProductListPage}" />
    </FlyoutItem>

    <FlyoutItem Title="Commandes" Icon="orders.png" Route="orders"
                x:Name="FlyoutOrders">
        <ShellContent ContentTemplate="{DataTemplate views:Orders.OrderListPage}" />
    </FlyoutItem>

    <FlyoutItem Title="Documents" Icon="documents.png" Route="documents"
                x:Name="FlyoutDocuments">
        <ShellContent ContentTemplate="{DataTemplate views:Documents.DocumentListPage}" />
    </FlyoutItem>

    <FlyoutItem Title="Mon Stock" Icon="stock.png" Route="stock"
                x:Name="FlyoutStock">
        <ShellContent ContentTemplate="{DataTemplate views:Stock.MyStockPage}" />
    </FlyoutItem>

    <FlyoutItem Title="Objectifs" Icon="objectives.png" Route="objectifs"
                x:Name="FlyoutObjectifs">
        <ShellContent ContentTemplate="{DataTemplate views:Objectifs.ObjectifPage}" />
    </FlyoutItem>

    <FlyoutItem Title="Profil" Icon="profile.png" Route="profile">
        <ShellContent ContentTemplate="{DataTemplate views:Profile.ProfilePage}" />
    </FlyoutItem>

</Shell>
```

### 7.2 Route Registrations (`AppShell.xaml.cs`)

All detail/modal pages that are not flyout items must be registered via `Routing.RegisterRoute`:

```csharp
public AppShell()
{
    InitializeComponent();

    // Auth
    Routing.RegisterRoute("forgotpassword", typeof(ForgotPasswordPage));

    // Visites
    Routing.RegisterRoute("visits/detail", typeof(VisitDetailPage));
    Routing.RegisterRoute("visits/rapport", typeof(RapportPage));

    // Products
    Routing.RegisterRoute("products/detail", typeof(ProductDetailPage));

    // Orders
    Routing.RegisterRoute("orders/detail", typeof(OrderDetailPage));
    Routing.RegisterRoute("orders/create", typeof(CreateOrderPage));

    // Documents
    Routing.RegisterRoute("documents/detail", typeof(DocumentDetailPage));
}
```

### 7.3 Navigation Examples

```csharp
// Navigate to visit detail (existing)
await Shell.Current.GoToAsync($"visits/detail?visiteId={visite.Id}");

// Navigate to create new visit
await Shell.Current.GoToAsync("visits/detail");

// Navigate to product detail
await Shell.Current.GoToAsync($"products/detail?productId={product.Id}");

// Navigate to order detail
await Shell.Current.GoToAsync($"orders/detail?orderId={order.Id}");

// Navigate to create order (optionally pre-seed a product)
await Shell.Current.GoToAsync($"orders/create?productId={product.Id}");

// Navigate to document detail
await Shell.Current.GoToAsync($"documents/detail?documentType=facture&documentId={facture.Id}");

// Post-login navigation (clear back stack)
await Shell.Current.GoToAsync("//dashboard");

// Logout — return to login clearing stack
await Shell.Current.GoToAsync("//login");
```

### 7.4 Role-Based Flyout Visibility (`AppShell.xaml.cs`)

```csharp
public void ApplyRoleVisibility(string role)
{
    bool isDelegue      = role == "DELEGUE";
    bool isSuperviseur  = role == "SUPERVISEUR";
    bool isClient       = role is "PHARMACIEN" or "GROSSISTE" or "CLIENT";

    FlyoutDashboard.IsVisible  = isDelegue || isSuperviseur;
    FlyoutVisites.IsVisible    = isDelegue;
    FlyoutPlanning.IsVisible   = isDelegue;
    FlyoutStock.IsVisible      = isDelegue;
    FlyoutObjectifs.IsVisible  = isDelegue || isSuperviseur;
    FlyoutOrders.IsVisible     = isClient || isDelegue;
    FlyoutDocuments.IsVisible  = isClient;
}
```

---

## 8. Dependency Injection (MauiProgram.cs)

Full `MauiProgram.cs` DI registrations:

```csharp
using CommunityToolkit.Maui;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Auth;
using Cynapharm_Mobile.ViewModels.Dashboard;
using Cynapharm_Mobile.ViewModels.Documents;
using Cynapharm_Mobile.ViewModels.Objectifs;
using Cynapharm_Mobile.ViewModels.Orders;
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
using Cynapharm_Mobile.Views.Planning;
using Cynapharm_Mobile.Views.Products;
using Cynapharm_Mobile.Views.Profile;
using Cynapharm_Mobile.Views.Rapports;
using Cynapharm_Mobile.Views.Stock;
using Cynapharm_Mobile.Views.Visites;

namespace Cynapharm_Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // --- HttpClient ---
        builder.Services.AddHttpClient<ApiService>(client =>
        {
            // Base URL loaded from appsettings.json at runtime
            var settings = LoadAppSettings();
            client.BaseAddress = new Uri(settings.ApiGatewayBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // --- Services (Singleton: shared state / HTTP client) ---
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<ProductService>();
        builder.Services.AddSingleton<OrderService>();
        builder.Services.AddSingleton<InventoryService>();
        builder.Services.AddSingleton<VisiteService>();
        builder.Services.AddSingleton<PlanningService>();
        builder.Services.AddSingleton<KpiService>();
        builder.Services.AddSingleton<DocumentService>();

        // --- ViewModels (Transient: fresh state per navigation) ---
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
        builder.Services.AddTransient<OrderListViewModel>();
        builder.Services.AddTransient<OrderDetailViewModel>();
        builder.Services.AddTransient<CreateOrderViewModel>();
        builder.Services.AddTransient<DocumentListViewModel>();
        builder.Services.AddTransient<DocumentDetailViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();

        // --- Views (Transient: paired with their ViewModel) ---
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
        builder.Services.AddTransient<OrderListPage>();
        builder.Services.AddTransient<OrderDetailPage>();
        builder.Services.AddTransient<CreateOrderPage>();
        builder.Services.AddTransient<DocumentListPage>();
        builder.Services.AddTransient<DocumentDetailPage>();
        builder.Services.AddTransient<ProfilePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static AppSettings LoadAppSettings()
    {
        // Load from Resources/Raw/appsettings.json
        using var stream = FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult();
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        return System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
    }
}
```

**`Resources/Raw/appsettings.json`:**
```json
{
  "ApiGatewayBaseUrl": "http://10.0.2.2:5000/",
  "ApiGatewayBaseUrlProd": "https://api.cynapharm.com/"
}
```

> Note: `10.0.2.2` is the Android emulator loopback address for the host machine's localhost. Replace with actual server address for device testing.

---

## 9. Cross-Cutting Concerns

### 9.1 JWT Token Storage

- Tokens are persisted using `Microsoft.Maui.Storage.SecureStorage`, which maps to:
  - **Android:** Android Keystore + EncryptedSharedPreferences
  - **Windows:** Windows Data Protection API (DPAPI)
- Key constants (defined in a static `StorageKeys` class):
  ```csharp
  public static class StorageKeys
  {
      public const string JwtToken   = "jwt_token";
      public const string TokenExpiry = "jwt_expiry";
      public const string UserRole   = "user_role";
      public const string UserId     = "user_id";
      public const string UserName   = "user_name";
  }
  ```
- On app start, `App.xaml.cs` reads the stored token, checks expiry, and navigates either to `//login` or `//dashboard`.

### 9.2 HTTP Bearer Token Injection

`ApiService` retrieves the token before every request:

```csharp
private async Task PrepareAuthHeaderAsync()
{
    var token = await SecureStorage.GetAsync(StorageKeys.JwtToken);
    if (!string.IsNullOrEmpty(token))
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }
}
```

On HTTP 401 responses, `ApiService` clears the stored token and raises a global `SessionExpired` event that `App.xaml.cs` handles by navigating to `//login`.

### 9.3 Offline / Connectivity Handling

- Use `Microsoft.Maui.Networking.Connectivity.Current.NetworkAccess` to check connectivity before API calls.
- If `NetworkAccess != NetworkAccess.Internet`, show a snackbar ("Pas de connexion internet") using `CommunityToolkit.Maui.Alerts.Snackbar`.
- Phase 5 enhancement: cache the last successful response of read-heavy screens (product list, dashboard KPIs) in a local JSON file under `FileSystem.AppDataDirectory` for offline viewing.
- No offline write/sync is required in the initial version — all mutations require connectivity.

### 9.4 French Localisation

- All user-facing strings must be in French.
- Use `.resx` resource files placed under `Resources/Raw/`:
  - `Strings.fr.resx` — French (default, no fallback needed as app is French-only)
- String key conventions: `FeatureName_ElementPurpose` (e.g., `Login_EmailPlaceholder`, `Order_StatusConfirmee`).
- Date formats: `dd/MM/yyyy` throughout; use `CultureInfo("fr-FR")` for all converters.
- Currency: `{value:C2}` with `fr-FR` culture renders as `1 234,56 DZD` (or `TND` depending on market — configure via `appsettings.json`).

### 9.5 Error Handling Pattern

- All ViewModel commands wrap service calls in `try/catch`:
  ```csharp
  [RelayCommand]
  private async Task LoadAsync()
  {
      IsBusy = true;
      ErrorMessage = string.Empty;
      try
      {
          // ... service call
      }
      catch (HttpRequestException ex)
      {
          ErrorMessage = "Erreur réseau. Veuillez réessayer.";
      }
      catch (Exception ex)
      {
          ErrorMessage = "Une erreur inattendue s'est produite.";
      }
      finally
      {
          IsBusy = false;
      }
  }
  ```
- `ErrorMessage` is bound to a visible `Label` with `IsVisible="{Binding ErrorMessage, Converter={StaticResource IsNotNullOrEmptyConverter}}"`.

### 9.6 XAML Source Generation

The project already has `<MauiXamlInflator>SourceGen</MauiXamlInflator>` enabled. All XAML code-behind constructors must call `InitializeComponent()` without any hand-written XAML parsing calls.

---

## 10. Implementation Phases

### Phase 1 — Authentication & Shell Skeleton
**Goal:** A working app that can log in, store a token, and navigate to a role-appropriate shell with stub pages.

**Tasks:**
- [ ] Add NuGet packages: `CommunityToolkit.Mvvm`, `CommunityToolkit.Maui`, `Microsoft.Extensions.Http`, `System.IdentityModel.Tokens.Jwt`
- [ ] Create full folder structure (Views, ViewModels, Services, Models, subfolders)
- [ ] Implement `ApiService` (base HTTP + bearer injection + 401 handling)
- [ ] Implement `AuthService` (login, logout, token storage, role decode)
- [ ] Create `LoginPage` + `LoginViewModel`
- [ ] Create `ForgotPasswordPage` + `ForgotPasswordViewModel`
- [ ] Create `AppShell` with role-aware flyout items and `ApplyRoleVisibility`
- [ ] Update `App.xaml.cs` with startup token check and role-based navigation
- [ ] Update `MauiProgram.cs` with full DI registrations
- [ ] Create stub pages (empty XAML + code-behind + ViewModel binding) for all remaining pages
- [ ] Set brand colors in `Resources/Styles/Colors.xaml` (Cynapharm palette)
- [ ] Define global styles in `Resources/Styles/Styles.xaml`
- [ ] Add `appsettings.json` to `Resources/Raw/`

**Deliverable:** Tap Connexion → land on the correct shell flyout for the role. Logout returns to login.

---

### Phase 2 — Product Catalog & Orders (Client Flow)
**Goal:** PHARMACIEN/GROSSISTE users can browse products and place orders.

**Tasks:**
- [ ] Implement `ProductService` (all methods)
- [ ] Build `ProductListPage` + `ProductListViewModel` (search, category filter, infinite scroll)
- [ ] Build `ProductDetailPage` + `ProductDetailViewModel` (product + lots + promotions)
- [ ] Implement `OrderService` (all methods)
- [ ] Build `OrderListPage` + `OrderListViewModel` (filter tabs)
- [ ] Build `OrderDetailPage` + `OrderDetailViewModel` (header + lines + reclamation)
- [ ] Build `CreateOrderPage` + `CreateOrderViewModel` (3-step form)
- [ ] Wire "Ajouter à la commande" from `ProductDetailPage` to pre-seed `CreateOrderPage`

**Deliverable:** Client can browse catalog, view product details, create an order, and view order history.

---

### Phase 3 — Delegate Field Operations
**Goal:** DELEGUE users can manage visits, planning, rapports, and view their stock and objectives.

**Tasks:**
- [ ] Implement `VisiteService` (CRUD + rapports)
- [ ] Build `VisitListPage` + `VisitListViewModel`
- [ ] Build `VisitDetailPage` + `VisitDetailViewModel` (create + edit + delete)
- [ ] Build `RapportPage` + `RapportViewModel`
- [ ] Implement `PlanningService`
- [ ] Build `PlanningPage` + `PlanningViewModel` (weekly grid)
- [ ] Implement `InventoryService`
- [ ] Build `MyStockPage` + `MyStockViewModel` (samples + promo tabs)
- [ ] Implement `KpiService`
- [ ] Build `ObjectifPage` + `ObjectifViewModel`
- [ ] Build `DashboardPage` + `DashboardViewModel` (KPI cards + visit count)

**Deliverable:** DELEGUE can complete a full day's workflow: view dashboard, execute planned visit, submit rapport, check stock.

---

### Phase 4 — Documents, KPIs, Profile
**Goal:** Complete the document center and profile management.

**Tasks:**
- [ ] Implement `DocumentService`
- [ ] Build `DocumentListPage` + `DocumentListViewModel` (tab per type)
- [ ] Build `DocumentDetailPage` + `DocumentDetailViewModel` (with share button)
- [ ] Build `ProfilePage` + `ProfileViewModel` (edit info + change password + logout)
- [ ] Integrate SUPERVISEUR dashboard view (team KPIs using `KpiService.GetRegionsAsync`)
- [ ] Add `Reclamation` submission flow from `OrderDetailPage`

**Deliverable:** All pages functional end-to-end. App is feature-complete.

---

### Phase 5 — Polish, Platform Testing & Offline Handling
**Goal:** Production-ready quality on Android and Windows.

**Tasks:**
- [ ] Android: test on API 21 device/emulator, fix layout issues
- [ ] Windows: test on Windows 10 1809+, fix flyout/tab behavior for desktop
- [ ] Implement offline read cache (product list, dashboard KPIs) using local JSON files
- [ ] Add `Snackbar` connectivity warnings on all network-dependent actions
- [ ] Validate all French strings in `.resx` for correctness
- [ ] Implement loading skeletons or `ActivityIndicator` on all list pages
- [ ] Add empty-state views (illustrations + message) for empty collections
- [ ] Performance: ensure CollectionView uses `LinearItemsLayout` with `ItemSizingStrategy.MeasureAllItems` only where needed; prefer `MeasureFirstItem` for uniform-height lists
- [ ] Security: verify SecureStorage fallback behaviour on devices without hardware keystore
- [ ] Final review of all route parameters and query string bindings via `[QueryProperty]`
- [ ] Update `ApplicationId` to `com.cynapharm.mobile` and set final app icon + splash assets

**Deliverable:** App approved for internal distribution on Android (APK/AAB) and Windows (MSIX or unpackaged exe).

---

*End of plan — Cynapharm-Mobile v1.0*
