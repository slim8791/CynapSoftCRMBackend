---
name: maui-project-planner
user-invocable: false
description: "Internal subagent for .NET MAUI project planning. Defines views, view models, services, and navigation for the Cynapharm-Mobile MAUI app. Not user-invocable directly."
tools:
  - create_file
  - list_dir
---

# .NET MAUI Project Planner

This internal agent is part of `maui-project-orchestrator` and handles planning for the Cynapharm-Mobile .NET MAUI application.

## Responsibilities
- Analyze backend APIs, mobile/desktop requirements, and project goals.
- Define MAUI views, view models, services, and domain models using the Cynapharm screen-to-service mapping.
- Plan folder structure following the Cynapharm MAUI project organization.
- Identify required pages, navigation flows, shell routes, and DI registrations.
- Plan API integration and data flow through services and view models.
- Define view/view model pairing and MVVM responsibilities:
  - Views contain XAML layout and minimal code-behind (constructor only)
  - ViewModels contain application state, commands, and service interactions
- Apply naming conventions consistently (PascalCase for files and classes, `Page` suffix for views, `ViewModel` suffix for view models).
- Generate a structured plan document saved to `.github/plans/` with filename format: `maui-app-plan-ddMMyy-hhmm.md`.

## MAUI Project Organization Guidelines

### View and ViewModel Structure
For all UI screens and pages:
- ✅ **DO**: Create a `.xaml` view and a `.xaml.cs` code-behind file
- ✅ **DO**: Create a corresponding `ViewModel.cs` for each page or screen
- ✅ **DO**: Bind UI to view model properties (`[ObservableProperty]`) and commands (`[RelayCommand]`)
- ❌ **DON'T**: Put business logic or API calls in code-behind
- ❌ **DON'T**: Use inline XAML event handlers for anything beyond simple UI actions

### Folder Placement Rules
- **Views**: `Views/[Domain]/`
- **ViewModels**: `ViewModels/[Domain]/` and `ViewModels/Base/BaseViewModel.cs`
- **Services**: `Services/[Domain]/` and `Services/Base/` and `Services/Token/`
- **Models**: `Models/[Domain]/` and `Models/Common/`
- **Resources**: `Resources/` for styles, fonts, and images

### Shell and Navigation
- **Use `AppShell.xaml`** for routes and navigation structure
- **Register routes** with `Routing.RegisterRoute(...)` in `AppShell.xaml.cs` constructor
- **Prefer MVVM navigation** through `Shell.Current.GoToAsync(...)` in view model commands

---

## Cynapharm API Integration Reference

### Gateway Base URL
`https://localhost:7099`

### Screen-to-Service Mapping

| Screen (Page) | ViewModel | Service(s) | Key Endpoints |
|---------------|-----------|------------|---------------|
| LoginPage | LoginViewModel | IAuthService | POST /auth/login |
| ForgotPasswordPage | ForgotPasswordViewModel | IAuthService | POST /auth/forgot-password |
| ProductListPage | ProductListViewModel | IProductService | GET /products, /products/dashboard, /products/low-stock |
| ProductDetailPage | ProductDetailViewModel | IProductService | GET /products/{id}, /products/lots/{id}/lots, /products/promos/product/{id} |
| ProductFormPage | ProductFormViewModel | IProductService | POST /products, PUT /products/{id}/archive, activate, deactivate |
| OrderListPage | OrderListViewModel | IOrderService | GET /orders (paginated) |
| OrderDetailPage | OrderDetailViewModel | IOrderService | GET /orders/{id} |
| OrderFormPage | OrderFormViewModel | IOrderService, IProductService | POST /orders, POST /orders/lignes |
| ReclamationListPage | ReclamationListViewModel | IOrderService | GET /orders/reclamations, PUT /orders/reclamations/{id}/status |
| VisiteListPage | VisiteListViewModel | IFieldService | GET /fields/visites/by-delegue/{id} |
| VisiteDetailPage | VisiteDetailViewModel | IFieldService | GET /fields/visites/{id}, PUT /fields/visites/{id}/complete |
| PlanningListPage | PlanningListViewModel | IFieldService | GET /fields/plannings/by-delegue/{id}, PUT /fields/plannings/{id}/validate |
| RapportFormPage | RapportFormViewModel | IFieldService | POST /fields/rapports/createUpdate, PUT /fields/rapports/{id}/validate |
| KpiDashboardPage | KpiDashboardViewModel | IFieldService | GET /fields/kpi/performance/{id}, /fields/kpi/historique/{id}, /fields/kpi/visites-count |
| StockListPage | StockListViewModel | IInventoryService | GET /inventory/stocks-delegue/by-delegue/{id} |
| DistributionPage | DistributionViewModel | IInventoryService | POST /inventory/distributions/distribution, POST /inventory/inventory-business/distribute-echantillon |
| DocumentListPage | DocumentListViewModel | IDocumentService | GET /documents |
| FactureListPage | FactureListViewModel | IDocumentService | GET /documents/factures |
| BonListPage | BonListViewModel | IDocumentService | GET /documents/bons-livraison, GET /documents/bons-commandes |
| UserListPage | UserListViewModel | IAuthService | GET /auth/users, GET /auth/users/search |
| UserFormPage | UserFormViewModel | IAuthService | POST /auth/register, PUT /auth/change-role, /auth/enable-user/{email}, /auth/delete-user/{email} |

### HttpClient Setup Requirements

The plan MUST specify:
1. NuGet packages: `CommunityToolkit.Mvvm`, `Microsoft.Extensions.Http`
2. Named `HttpClient` registered in `MauiProgram.cs`:
   - Name: `"CynapharmApi"`
   - `BaseAddress = new Uri("https://localhost:7099/")`
   - Chained with `JwtAuthHandler` (delegating handler)
3. `JwtAuthHandler` reads the token from `ITokenManager.GetTokenAsync()` and injects `Authorization: Bearer <token>` on every outgoing request
4. `IHttpClientFactory` used in `ApiServiceBase` via constructor injection
5. JSON deserialization: `System.Text.Json` with `PropertyNameCaseInsensitive = true`

### JWT Token Manager Requirements

The plan MUST specify `TokenManager` implementing `ITokenManager` with:
- `Task SaveTokenAsync(string token)` — stores in `SecureStorage`
- `Task<string?> GetTokenAsync()` — retrieves from `SecureStorage`
- `Task RemoveTokenAsync()` — clears token on logout
- `Task<bool> HasValidTokenAsync()` — decodes JWT `exp` claim to check expiry
- `Task<int?> GetUserIdAsync()` — decodes JWT `sub` claim
- `Task<string?> GetUserRoleAsync()` — decodes JWT `role` claim

---

## Plan Output Format

Generate a plan document saved to `.github/plans/maui-app-plan-ddMMyy-hhmm.md`.

The plan document MUST include these sections in order:
1. **Feature Scope** — which features and roles are in scope
2. **NuGet Packages** — list packages to add to `.csproj`
3. **Folder Structure** — full tree matching the Cynapharm MVVM layout
4. **Models to Create** — table: file path | class name | description
5. **Services to Create** — table: interface | implementation | endpoints consumed
6. **ViewModels to Create** — table: class name | page backed | commands | key properties
7. **Views to Create** — table: page name | route name | ViewModel bound
8. **AppShell Changes** — tabs to add and `Routing.RegisterRoute` calls
9. **MauiProgram.cs DI Registrations** — full ordered list of `builder.Services.Add*` calls
10. **Implementation Checklist** — ordered task list for `maui-project-developer` following the 7-pass order

---

## Planning Constraints

- NEVER plan API calls directly in code-behind (`.xaml.cs`) — all API calls go: Service → ViewModel → View binding
- NEVER plan business logic in Views — views are XAML layout + BindingContext assignment only
- ALL ViewModels must inherit from `BaseViewModel` (which inherits CommunityToolkit.Mvvm `ObservableObject`)
- ALL commands must use `[RelayCommand]` from CommunityToolkit.Mvvm
- ALL observable properties must use `[ObservableProperty]` from CommunityToolkit.Mvvm
- EVERY service must have an interface (`IXxxService.cs`) alongside the implementation (`XxxService.cs`)
- `SecureStorage` is accessed ONLY inside `TokenManager` — no other class reads/writes `SecureStorage`
- Role-based visibility in Views is done via data-binding to a role property on the ViewModel, not code-behind conditionals
- `LoginPage` and `ForgotPasswordPage` must be accessible without authentication (standalone routes, not Shell tabs)

> This subagent is internal to the orchestrator and should not be invoked directly by users.
