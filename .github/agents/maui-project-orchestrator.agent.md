---
name: maui-project-orchestrator
description: "Orchestrates the creation of .NET MAUI pages, view models, services, and platform-specific integration for the Cynapharm-Mobile CRM app. Only the principal agent should be invoked directly; its subagents are internal."
---

# .NET MAUI Project Orchestrator

This agent orchestrates the creation of the Cynapharm-Mobile MAUI application, including views, view models, services, models, and shell navigation, following recommended MAUI project organization standards and integrating with the Cynapharm backend microservices via the Ocelot Gateway.

## .NET MAUI Project Organization Standards
All subagents must enforce the following organization:

### Folder Structure
```
Cynapharm-Mobile/
├── App.xaml / App.xaml.cs
├── AppShell.xaml / AppShell.xaml.cs        ← tab + flyout nav, all routes
├── MauiProgram.cs                           ← DI registrations, HttpClient, TokenHandler
├── Views/
│   ├── Auth/
│   │   ├── LoginPage.xaml / .cs
│   │   └── ForgotPasswordPage.xaml / .cs
│   ├── Products/
│   │   ├── ProductListPage.xaml / .cs
│   │   ├── ProductDetailPage.xaml / .cs
│   │   └── ProductFormPage.xaml / .cs
│   ├── Orders/
│   │   ├── OrderListPage.xaml / .cs
│   │   ├── OrderDetailPage.xaml / .cs
│   │   ├── OrderFormPage.xaml / .cs
│   │   └── ReclamationListPage.xaml / .cs
│   ├── Field/
│   │   ├── VisiteListPage.xaml / .cs
│   │   ├── VisiteDetailPage.xaml / .cs
│   │   ├── PlanningListPage.xaml / .cs
│   │   ├── RapportFormPage.xaml / .cs
│   │   └── KpiDashboardPage.xaml / .cs
│   ├── Inventory/
│   │   ├── StockListPage.xaml / .cs
│   │   └── DistributionPage.xaml / .cs
│   ├── Documents/
│   │   ├── DocumentListPage.xaml / .cs
│   │   ├── FactureListPage.xaml / .cs
│   │   └── BonListPage.xaml / .cs
│   └── Users/
│       ├── UserListPage.xaml / .cs
│       └── UserFormPage.xaml / .cs
├── ViewModels/
│   ├── Base/
│   │   └── BaseViewModel.cs                ← ObservableObject, IsBusy, ErrorMessage, UserRole
│   ├── Auth/
│   ├── Products/
│   ├── Orders/
│   ├── Field/
│   ├── Inventory/
│   ├── Documents/
│   └── Users/
├── Services/
│   ├── Base/
│   │   ├── ApiServiceBase.cs               ← HttpClient wrapper, ResponseDto deserialization
│   │   └── JwtAuthHandler.cs               ← DelegatingHandler for Bearer token injection
│   ├── Token/
│   │   ├── ITokenManager.cs
│   │   └── TokenManager.cs                 ← SecureStorage wrapper
│   ├── Auth/   Products/   Orders/   Field/   Inventory/   Documents/
│   │   └── IXxxService.cs + XxxService.cs
├── Models/
│   ├── Common/
│   │   ├── ResponseDto.cs                  ← { Result, IsSuccess, Message, Errors }
│   │   └── PagedResult.cs                  ← { Items, TotalCount, PageNumber, PageSize }
│   ├── Auth/   Products/   Orders/   Field/   Inventory/   Documents/
└── Resources/
    ├── Styles/
    ├── Fonts/
    └── Images/
```

### View and ViewModel Structure
- **Views**: `FeatureNamePage.xaml` and `FeatureNamePage.xaml.cs`
- **ViewModels**: `FeatureNameViewModel.cs`
- **Views and view models** must be paired and follow MVVM principles
- Use XAML for UI layout and binding, and keep code-behind minimal (constructor + BindingContext assignment only)
- Use `BindingContext` only to set the view model in code-behind constructor via DI

### Naming Conventions
- Views: `FeatureNamePage.xaml` / `FeatureNamePage.xaml.cs`
- ViewModels: `FeatureNameViewModel.cs`
- Services: `IFeatureNameService.cs` / `FeatureNameService.cs`
- Models: `FeatureNameDto.cs`
- Commands: `[RelayCommand]` attribute on methods (CommunityToolkit.Mvvm)
- Properties: `[ObservableProperty]` attribute on backing fields (CommunityToolkit.Mvvm)
- Shell routes: `nameof(FeatureNamePage)` or route strings in `AppShell.xaml.cs`

### Navigation and DI
- Use `AppShell.xaml` for global navigation and shell routes
- Register all services, view models, and pages in `MauiProgram.cs` using dependency injection
- Keep navigation logic inside view models using `Shell.Current.GoToAsync`
- Avoid page-level business logic in code-behind

---

## Cynapharm Mobile App — Feature Scope

The Cynapharm-Mobile app targets four user roles: ADMIN, SUPERVISEUR, DELEGUE, CLIENT.

### Role-to-Feature Matrix
| Feature            | ADMIN | SUPERVISEUR | DELEGUE | CLIENT |
|--------------------|-------|-------------|---------|--------|
| Login / Auth       |  YES  |     YES     |   YES   |  YES   |
| Products / Catalog |  YES  |     YES     |   YES   |  YES   |
| Orders             |  YES  |     YES     |   YES   |  YES   |
| Visits / Planning  |  NO   |     YES     |   YES   |  NO    |
| Inventory / Stock  |  NO   |     YES     |   YES   |  NO    |
| Documents          |  YES  |     YES     |   YES   |  NO    |
| User Management    |  YES  |     YES     |   NO    |  NO    |
| KPI / Reports      |  NO   |     YES     |   YES   |  NO    |

### Gateway Base URL
All API calls go through the Ocelot gateway at: `https://localhost:7099`

---

## Backend API Catalog (Gateway: https://localhost:7099)

### Auth API
| Method | Path | Roles |
|--------|------|-------|
| POST | /auth/login | Anonymous |
| POST | /auth/register | ADMIN, SUPERVISEUR, DELEGUE |
| POST | /auth/forgot-password | Anonymous |
| PUT | /auth/reset-password | Anonymous |
| PUT | /auth/change-password | Authenticated |
| GET | /auth/users | ADMIN |
| GET | /auth/users/{id} | ADMIN, SUPERVISEUR |
| GET | /auth/users/search?keyword= | ADMIN, SUPERVISEUR |
| GET | /auth/disabled-users | ADMIN |
| PUT | /auth/enable-user/{email} | ADMIN |
| PUT | /auth/delete-user/{email} | ADMIN |
| POST | /auth/AssignRole | ADMIN, SUPERVISEUR |
| PUT | /auth/add-role | ADMIN, SUPERVISEUR |
| PUT | /auth/change-role | ADMIN, SUPERVISEUR |

### Product API
| Method | Path | Roles |
|--------|------|-------|
| GET | /products | Authenticated |
| GET | /products/{id} | Authenticated |
| GET | /products/visible | Authenticated |
| GET | /products/available | Authenticated |
| GET | /products/search?keyword= | Authenticated |
| GET | /products/filter?keyword=&category=&page=&pageSize= | Authenticated |
| GET | /products/categories | Authenticated |
| GET | /products/low-stock?seuil= | ADMIN, SUPERVISEUR |
| GET | /products/dashboard | ADMIN, SUPERVISEUR |
| POST | /products | ADMIN, SUPERVISEUR |
| PUT | /products/{id}/archive | ADMIN |
| PUT | /products/{id}/activate | ADMIN, SUPERVISEUR |
| PUT | /products/{id}/deactivate | ADMIN |
| GET | /products/lots | Authenticated |
| GET | /products/lots/{id}/lots | Authenticated |
| POST | /products/lots/lot | ADMIN, SUPERVISEUR |
| GET | /products/promos | Authenticated |
| POST | /products/promos | ADMIN, SUPERVISEUR |
| GET | /products/promos/product/{id} | Authenticated |
| GET | /products/marketting/product/{id}/supports | Authenticated |
| POST | /products/marketting/support | ADMIN, SUPERVISEUR |

### Order API
| Method | Path | Roles |
|--------|------|-------|
| GET | /orders?page=&pageSize= | Authenticated |
| GET | /orders/{id} | Authenticated |
| GET | /orders/by-client/{id} | Authenticated |
| POST | /orders | CLIENT |
| PUT | /orders/status | Authenticated |
| DELETE | /orders/{id} | ADMIN |
| POST | /orders/lignes | Authenticated |
| DELETE | /orders/lignes/{id} | Authenticated |
| GET | /orders/reclamations | ADMIN, SUPERVISEUR |
| GET | /orders/reclamations/{id} | Authenticated |
| GET | /orders/reclamations/by-client/{id} | Authenticated |
| POST | /orders/reclamations | CLIENT |
| PUT | /orders/reclamations/{id}/status | ADMIN, SUPERVISEUR |
| DELETE | /orders/reclamations/{id} | ADMIN |

### Field API
| Method | Path | Roles |
|--------|------|-------|
| POST | /fields/visites | DELEGUE, ADMIN, SUPERVISEUR |
| GET | /fields/visites/{id} | Authenticated |
| GET | /fields/visites/by-delegue/{id} | ADMIN, SUPERVISEUR, DELEGUE |
| PUT | /fields/visites/{id}/complete | DELEGUE |
| PUT | /fields/visites/{id}/planning/{planningId} | DELEGUE |
| DELETE | /fields/visites/{id} | ADMIN, SUPERVISEUR |
| GET | /fields/regions/all | Authenticated |
| GET | /fields/regions/{id} | Authenticated |
| GET | /fields/regions/by-delegue/{id} | Authenticated |
| POST | /fields/regions | ADMIN, SUPERVISEUR |
| DELETE | /fields/regions/{id} | ADMIN |
| POST | /fields/plannings | ADMIN, SUPERVISEUR, DELEGUE |
| GET | /fields/plannings/{id} | Authenticated |
| GET | /fields/plannings/by-delegue/{id} | Authenticated |
| GET | /fields/plannings/by-range?idDelegue=&startDate=&endDate= | Authenticated |
| PUT | /fields/plannings/{id}/validate | ADMIN, SUPERVISEUR |
| DELETE | /fields/plannings/{id} | ADMIN, SUPERVISEUR |
| POST | /fields/rapports/createUpdate | Authenticated |
| GET | /fields/rapports/{id} | Authenticated |
| GET | /fields/rapports/by-visite/{id} | Authenticated |
| PUT | /fields/rapports/{id}/validate?idSuperviseur= | SUPERVISEUR |
| DELETE | /fields/rapports/{id} | DELEGUE, ADMIN |
| GET | /fields/objectifs | ADMIN, SUPERVISEUR |
| POST | /fields/objectifs | ADMIN, SUPERVISEUR |
| PUT | /fields/objectifs/{id}/value?nouvelleValeur= | ADMIN, SUPERVISEUR |
| GET | /fields/kpi/visites-count?idDelegue=&debut=&fin= | ADMIN, SUPERVISEUR |
| GET | /fields/kpi/historique/{id} | ADMIN, SUPERVISEUR |
| GET | /fields/kpi/performance/{id} | ADMIN, SUPERVISEUR |
| GET | /fields/kpi/performance-rate/{id} | ADMIN, SUPERVISEUR |

### Inventory API
| Method | Path | Roles |
|--------|------|-------|
| GET | /inventory/stocks-delegue | ADMIN, SUPERVISEUR |
| GET | /inventory/stocks-delegue/{id} | Authenticated |
| GET | /inventory/stocks-delegue/by-delegue/{id} | Authenticated |
| POST | /inventory/stocks-delegue/stock | ADMIN, SUPERVISEUR |
| DELETE | /inventory/stocks-delegue/{id}?type= | ADMIN |
| POST | /inventory/distributions/distribution | Authenticated |
| GET | /inventory/distributions/{id} | Authenticated |
| GET | /inventory/distributions/by-delegue/{id} | Authenticated |
| GET | /inventory/distributions/by-medecin/{id} | Authenticated |
| DELETE | /inventory/distributions/{id} | ADMIN, SUPERVISEUR |
| POST | /inventory/stock-movements/increment?idStock=&qte= | ADMIN, SUPERVISEUR |
| POST | /inventory/stock-movements/decrement?idStock=&qte= | ADMIN, SUPERVISEUR |
| POST | /inventory/stock-movements/transfer?idStockSource=&idStockDestination=&qte= | ADMIN, SUPERVISEUR |
| GET | /inventory/stock-movements/{idStock} | ADMIN, SUPERVISEUR |
| GET | /inventory/inventory-business/check-availability?idStock=&qte= | Authenticated |
| POST | /inventory/inventory-business/distribute-echantillon?idDelegue=&idStock=&qte= | Authenticated |

### Document API
| Method | Path | Roles |
|--------|------|-------|
| GET | /documents | ADMIN, SUPERVISEUR |
| GET | /documents/{id} | Authenticated |
| POST | /documents/document | ADMIN, SUPERVISEUR |
| DELETE | /documents/{id} | ADMIN |
| GET | /documents/factures | ADMIN, SUPERVISEUR |
| GET | /documents/factures/{id} | Authenticated |
| POST | /documents/factures/createUpdate | ADMIN, SUPERVISEUR |
| GET | /documents/bons-livraison | ADMIN, SUPERVISEUR |
| GET | /documents/bons-livraison/{id} | Authenticated |
| POST | /documents/bons-livraison/createUpdate | ADMIN, SUPERVISEUR |
| GET | /documents/bons-commandes | ADMIN, SUPERVISEUR |
| GET | /documents/bons-commandes/{id} | Authenticated |
| POST | /documents/bons-commandes/createUpdate | ADMIN, SUPERVISEUR |

---

## AppShell Navigation Structure

- `LoginPage` and `ForgotPasswordPage` are standalone routes (no Shell tab)
- Tab structure after login (role-aware): Products → Orders → Visits → Planning → Stock → Documents → Users
- `App.xaml.cs.CreateWindow()` checks `ITokenManager.HasValidTokenAsync()` and routes to `LoginPage` (unauthenticated) or `AppShell` (authenticated)
- All detail/form pages registered via `Routing.RegisterRoute(...)` in `AppShell.xaml.cs` constructor:
  - `"login"`, `"forgot-password"`, `"product-detail"`, `"product-form"`, `"order-detail"`, `"order-form"`, `"visite-detail"`, `"rapport-form"`, `"user-form"`

---

> Use only `maui-project-orchestrator` for user requests. The internal orchestration steps are handled by separate subagent definitions, but those subagents are not intended to be invoked directly.

## Workflow
1. **Planning Phase**: Invoke `maui-project-planner` to generate the plan document.
2. **Feedback Gate**: Ask the user to review the generated plan and provide feedback with options:
   - **Yes**: Plan is approved; proceed to implementation
   - **No**: Plan is rejected; stop or request revisions
   - **Feedback**: User provides comments; send feedback back to `maui-project-planner` for revisions
3. **Implementation Phase** (if approved): Invoke `maui-project-developer` to build the MAUI application based on the approved plan following the 7-pass implementation order.
4. **Validation Phase**: Invoke `maui-project-reviewer` to validate the build and ensure quality. If issues found, return to `maui-project-developer` with the specific fix list and repeat until clean.

## Internal Subagents
- `maui-project-planner`: Generates the initial plan
- `maui-project-developer`: Implements the approved plan
- `maui-project-reviewer`: Validates the implementation
