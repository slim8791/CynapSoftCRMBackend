# Angular Test Report — 15/05/2026

## Environment
- Framework: Angular 21 (standalone components)
- Test runner: Vitest 4.1.5 (via `@angular/build:unit-test`)
- Test environment: jsdom (Node.js, no browser)
- Project root: `Cynapharm/`

## Summary
| Status  | Count |
|---------|-------|
| Passed  | 239   |
| Failed  | 0     |
| Missing | 0     |

All 35 spec files generated. All 239 `it()` blocks pass.

---

## Validators
### dateRangeValidator
- [PASS] should return null when startDate is before endDate
- [PASS] should return dateRange error when startDate is after endDate
- [PASS] should return null when startDate equals endDate
- [PASS] should return null when startDate is empty
- [PASS] should return null when endDate is empty
- [PASS] should return null when control keys do not exist in group

### noWhitespaceValidator
- [PASS] should return null for a non-empty, non-whitespace value
- [PASS] should return whitespace error for a value containing only spaces
- [PASS] should return whitespace error for an empty string
- [PASS] should return null for a value with leading/trailing spaces but non-empty content
- [PASS] should return whitespace error for a null value

### passwordMatchValidator
- [PASS] should return null when passwords match
- [PASS] should return passwordMismatch error when passwords do not match
- [PASS] should set confirmPassword control error when passwords do not match
- [PASS] should clear confirmPassword control error when passwords match
- [PASS] should return null when control keys do not exist in group

---

## Guards
### authGuard
- [PASS] should return true when user is authenticated
- [PASS] should return false and navigate to login when user is not authenticated

### roleGuard
- [PASS] should return false and navigate to login when not authenticated
- [PASS] should return true when user has an allowed role
- [PASS] should return false and navigate to forbidden when role is not allowed
- [PASS] should return false and navigate to forbidden when getUserRole returns null

---

## Interceptors
### tokenInterceptor
- [PASS] should add Authorization header when token exists
- [PASS] should not add Authorization header when token is null

### errorInterceptor
- [PASS] should call logout and navigate to login on 401
- [PASS] should show error toast on 403
- [PASS] should show server error toast on 500
- [PASS] should propagate the error as an observable error

---

## Services
### ApiService
- [PASS] get should send GET request to correct URL
- [PASS] get should attach HttpParams when provided
- [PASS] post should send POST request with body
- [PASS] put should send PUT request with body
- [PASS] delete should send DELETE request to correct URL
- [PASS] patch should send PATCH request with body
- [PASS] unwrapResponse should return Result when IsSuccess is not false and Result exists
- [PASS] unwrapResponse should return raw response when Result is undefined
- [PASS] unwrapResponse should return raw response when IsSuccess is false

### AuthService
- [PASS] login should POST credentials and store token and user on success
- [PASS] register should POST user data without storing tokens
- [PASS] logout should clear localStorage and nullify currentUser
- [PASS] getToken should return token from localStorage when browser
- [PASS] getToken should return null when no token in localStorage
- [PASS] isAuthenticated should return true when token exists
- [PASS] isAuthenticated should return false when no token
- [PASS] getUserRole should return null when no user
- [PASS] hasRole should return false when no current user
- [PASS] forgotPassword should POST email to forgot-password endpoint
- [PASS] resetPassword should PUT reset data to reset-password endpoint
- [PASS] changePassword should PUT change-password data

### DashboardService
- [PASS] getDashboardData should call apiService.get with /dashboard
- [PASS] getMetrics should call apiService.get with /dashboard/metrics
- [PASS] getRecentActivity should call apiService.get with /dashboard/recent-activity

### OrderApiService
- [PASS] getAllOrders should call GET /orders and return array
- [PASS] getAllOrders should return empty array when response is not an array
- [PASS] getOrdersByClient should call GET /orders/by-client/:id
- [PASS] computeStats should return correct totals for a list of orders
- [PASS] computeStats should compute last7Days buckets
- [PASS] computeStats should return 0 livrees when no orders

### FieldApiService
- [PASS] getVisitesCount should call correct endpoint without date
- [PASS] getVisitesCount should include date param when provided
- [PASS] getVisitesCount should return empty array when response is not array
- [PASS] getPerformance should call /fields/kpi/performance
- [PASS] getPerformanceRate should call /fields/kpi/performance-rate and return number
- [PASS] getRegions should call /fields/regions and return array
- [PASS] getHistoriqueVisites should call /fields/kpi/historique

### BonCommandeService
- [PASS] getAll should call GET /documents/bons-commandes with pagination params
- [PASS] getAll should unwrap Result when present
- [PASS] getById should call GET /documents/bons-commandes/:id
- [PASS] getByClient should call GET /documents/bons-commandes/client/:id
- [PASS] createOrUpdate should POST to /documents/bons-commandes/createUpdate

### BonLivraisonService
- [PASS] getAll should call GET /documents/bons-livraison with pagination
- [PASS] getAll should unwrap Result from response
- [PASS] getById should call /documents/bons-livraison/:id
- [PASS] getByClient should call /documents/bons-livraison/ByClient/:id
- [PASS] createOrUpdate should POST to /documents/bons-livraison/createUpdate

### DocumentService
- [PASS] getAll should call GET /documents with pagination
- [PASS] getAll should unwrap Result
- [PASS] getById should call /documents/:numero
- [PASS] getByClient should call /documents/client/:id
- [PASS] getByCommande should call /documents/commande/:id
- [PASS] createOrUpdate should POST to /documents/document
- [PASS] delete should call DELETE /documents/:numero

### FactureService
- [PASS] getAll should call GET /documents/factures with pagination
- [PASS] getAll should unwrap Result
- [PASS] getById should call /documents/factures/:id
- [PASS] getByClient should call /documents/factures/client/:id
- [PASS] createOrUpdate should POST to /documents/factures/createUpdate

### KpiService
- [PASS] getNombreVisites should call /fields/kpi/visites-count with idDelegue
- [PASS] getNombreVisites should include debut and fin params when provided
- [PASS] hasVisiteAtDate should call /fields/kpi/has-visite and unwrap boolean
- [PASS] getHistorique should call /fields/kpi/historique/:id
- [PASS] getPerformance should call /fields/kpi/performance/:id
- [PASS] getPerformanceRate should call /fields/kpi/performance-rate/:id and return number
- [PASS] getClientFidelite should call /fields/kpi/client-fidelite/:id

### ObjectifService
- [PASS] getAll should call GET /fields/objectifs
- [PASS] getById should call /fields/objectifs/:id
- [PASS] getByDelegue should call /fields/objectifs/by-delegue/:id
- [PASS] createOrUpdate should POST to /fields/objectifs
- [PASS] updateValue should PUT to /fields/objectifs/:id/value with valeur param
- [PASS] delete should call DELETE /fields/objectifs/:id

### PlanningService
- [PASS] getById should call GET /fields/plannings/:id
- [PASS] getByDelegue should call GET /fields/plannings/by-delegue/:id
- [PASS] getByRange should call GET /fields/plannings/by-range with params
- [PASS] createOrUpdate should POST to /fields/plannings
- [PASS] validate should PUT to /fields/plannings/:id/validate
- [PASS] delete should call DELETE /fields/plannings/:id

### RapportService
- [PASS] getAll should call GET /fields/rapports/all
- [PASS] getById should call GET /fields/rapports/:id
- [PASS] getByVisite should call /fields/rapports/by-visite/:id
- [PASS] canCreate should call /fields/rapports/can-create/:idVisite and return boolean
- [PASS] createOrUpdate should POST to /fields/rapports/createUpdate
- [PASS] validate should PUT to /fields/rapports/:id/validate with superviseur query
- [PASS] delete should call DELETE /fields/rapports/:id

### RegionService
- [PASS] getAll should call GET /fields/regions/all
- [PASS] getById should call GET /fields/regions/:id
- [PASS] getByDelegue should call /fields/regions/by-delegue/:id
- [PASS] getCount should call /fields/regions/count/:id and return number
- [PASS] createOrUpdate should POST to /fields/regions
- [PASS] delete should call DELETE /fields/regions/:id

### VisiteService
- [PASS] getById should call GET /fields/visites/:id
- [PASS] getByDelegue should call /fields/visites/by-delegue/:id
- [PASS] getByPlanning should call /fields/visites/by-planning/:id
- [PASS] createOrUpdate should POST to /fields/visites
- [PASS] affectToPlanning should PUT to /fields/visites/:id/planning/:planId
- [PASS] complete should PUT to /fields/visites/:id/complete
- [PASS] delete should call DELETE /fields/visites/:id

### OrderService
- [PASS] getOrders should call GET /orders with page params
- [PASS] getOrders should normalize order fields from camelCase
- [PASS] getOrderById should call GET /orders/:id and normalize
- [PASS] getOrdersByClient should call GET /orders/by-client/:id
- [PASS] createOrder should call POST /orders
- [PASS] updateOrderStatus should call PUT /orders/status
- [PASS] deleteOrder should call DELETE /orders/:id
- [PASS] statutToNumber should map string statut to enum number
- [PASS] getEtatLabel should return label for numeric statut
- [PASS] getEtatLabel should return label for string statut
- [PASS] getEtatClass should return correct CSS class for numeric statut
- [PASS] getEtatClass should return correct CSS class for string statut
- [PASS] getNextStatuses should return correct transitions for Brouillon
- [PASS] getNextStatuses should return empty array for terminal state Livree

---

## Components
### LoginComponent
- [PASS] should create with loginForm initialized
- [PASS] onLogin should do nothing when form is invalid
- [PASS] onLogin should call authService.login with credentials when form is valid
- [PASS] onLogin should navigate to /users for ADMIN on success
- [PASS] onLogin should set error message on failure
- [PASS] onLogin should navigate to /dashboard for SUPERVISEUR
- [PASS] onLogin should navigate to /home for CLIENT

### RegisterComponent
- [PASS] should create with registerForm initialized
- [PASS] onRegister should do nothing when form is invalid
- [PASS] onRegister should call authService.register with payload when form is valid
- [PASS] onRegister should set success=true on successful registration
- [PASS] onRegister should set error on failure

### ForgotPasswordComponent
- [PASS] should create with forgotForm initialized
- [PASS] onSubmit should do nothing when form is invalid
- [PASS] onSubmit should call forgotPassword with email when form is valid
- [PASS] onSubmit should set success and message when IsSuccess is true
- [PASS] onSubmit should set error when IsSuccess is false
- [PASS] onSubmit should set error on HTTP failure
- [PASS] goToLogin should navigate to /login

### ResetPasswordComponent
- [PASS] should read email and token from query params
- [PASS] ngOnInit should set error when email or token missing
- [PASS] onSubmit should do nothing when form is invalid
- [PASS] onSubmit should call resetPassword when form is valid
- [PASS] onSubmit should set success on successful reset
- [PASS] onSubmit should set error on failure
- [PASS] goToLogin should navigate to /login

### DashboardComponent
- [PASS] should create
- [PASS] ngOnInit should call getAllOrders and computeStats
- [PASS] should handle getAllOrders error gracefully via catchError
- [PASS] reload should trigger loadAll again
- [PASS] tauxLivraison should be 0 when no orders
- [PASS] ngOnDestroy should not throw

### BonCommandeListComponent
- [PASS] should create and load on init
- [PASS] load should set bons and total on success
- [PASS] load should set error message on failure
- [PASS] onPage should update page and reload

### BonLivraisonListComponent
- [PASS] should create and load on init
- [PASS] load should populate bons on success
- [PASS] load should set error on failure
- [PASS] onPage should update page and reload

### DocumentListComponent
- [PASS] should create and load on init
- [PASS] load should populate docs on success
- [PASS] load should set error on failure
- [PASS] onPage should update page and call load

### FactureListComponent
- [PASS] should create and load on init
- [PASS] load should populate factures on success
- [PASS] load should set error on failure
- [PASS] onPage should update page and reload

### KpiDashboardComponent
- [PASS] should create
- [PASS] load should do nothing when idDelegue is null
- [PASS] load should call all kpi methods when idDelegue is set
- [PASS] load should handle errors gracefully

### ObjectifFormComponent
- [PASS] should create in create mode when no id
- [PASS] should enter edit mode and load objectif when id is in route
- [PASS] submit should not call service when form is invalid
- [PASS] submit should call createOrUpdate on success
- [PASS] submit should set submitError on failure

### ObjectifListComponent
- [PASS] should create and load on init
- [PASS] load should populate objectifs on success
- [PASS] load should set error on failure
- [PASS] progressPct should return 0 when cible is 0
- [PASS] progressPct should return correct percentage
- [PASS] progressPct should cap at 100

### PlanningFormComponent
- [PASS] should create in create mode
- [PASS] should enter edit mode and load data when id provided
- [PASS] submit should not call service when form is invalid
- [PASS] submit should call createOrUpdate and set successMsg on success
- [PASS] submit should set submitError on failure

### PlanningListComponent
- [PASS] should create
- [PASS] load should do nothing when delegueId is null
- [PASS] load should call getByDelegue and populate plannings on success
- [PASS] load should set error on failure
- [PASS] statusLabel should return readable label for EtatPlanning

### RapportFormComponent
- [PASS] should create in create mode
- [PASS] should enter edit mode and load rapport when id provided
- [PASS] submit should not call service when form is invalid
- [PASS] submit should createOrUpdate and set successMsg on success
- [PASS] submit should set submitError on failure

### RapportListComponent
- [PASS] should create and load on init
- [PASS] load should populate rapports on success
- [PASS] load should set error on failure

### RegionFormComponent
- [PASS] should create in create mode
- [PASS] should enter edit mode and load region when id provided
- [PASS] submit should not call service when form is invalid
- [PASS] submit should createOrUpdate and set successMsg on success
- [PASS] submit should set submitError on failure

### RegionListComponent
- [PASS] should create and load on init
- [PASS] load should populate regions on success
- [PASS] load should set error on failure

### VisiteFormComponent
- [PASS] should create in create mode
- [PASS] should enter edit mode and load visite when id provided
- [PASS] submit should not call service when form is invalid
- [PASS] submit should createOrUpdate and set successMsg on success
- [PASS] submit should set submitError on failure

### VisiteListComponent
- [PASS] should create
- [PASS] load should do nothing when delegueId is null
- [PASS] load should call getByDelegue and populate visites
- [PASS] load should set error on failure
- [PASS] visiteTypeLabel should return correct label for each VisiteType

---

## Spec Files Written (35 total)

| Layer | File |
|-------|------|
| Validator | `core/validators/date-range.validator.spec.ts` |
| Validator | `core/validators/no-whitespace.validator.spec.ts` |
| Validator | `core/validators/password-match.validator.spec.ts` |
| Guard | `core/guards/auth.guard.spec.ts` |
| Guard | `core/guards/role.guard.spec.ts` |
| Interceptor | `core/interceptors/token.interceptor.spec.ts` |
| Interceptor | `core/interceptors/error.interceptor.spec.ts` |
| Service | `core/services/api.service.spec.ts` |
| Service | `core/services/auth.service.spec.ts` |
| Service | `features/dashboard/dashboard.service.spec.ts` |
| Service | `features/dashboard/services/order-api.service.spec.ts` |
| Service | `features/dashboard/services/field-api.service.spec.ts` |
| Service | `features/documents/bons-commandes/services/bon-commande.service.spec.ts` |
| Service | `features/documents/bons-livraison/services/bon-livraison.service.spec.ts` |
| Service | `features/documents/documents-general/services/document.service.spec.ts` |
| Service | `features/documents/factures/services/facture.service.spec.ts` |
| Service | `features/field/kpi/services/kpi.service.spec.ts` |
| Service | `features/field/objectifs/services/objectif.service.spec.ts` |
| Service | `features/field/plannings/services/planning.service.spec.ts` |
| Service | `features/field/rapports/services/rapport.service.spec.ts` |
| Service | `features/field/regions/services/region.service.spec.ts` |
| Service | `features/field/visites/services/visite.service.spec.ts` |
| Service | `features/orders/order.service.spec.ts` |
| Component | `features/auth/login/login.component.spec.ts` |
| Component | `features/auth/register/register.component.spec.ts` |
| Component | `features/auth/forgot-password/forgot-password.component.spec.ts` |
| Component | `features/auth/reset-password/reset-password.component.spec.ts` |
| Component | `features/dashboard/dashboard.component.spec.ts` |
| Component | `features/documents/bons-commandes/bon-commande-list/bon-commande-list.component.spec.ts` |
| Component | `features/documents/bons-livraison/bon-livraison-list/bon-livraison-list.component.spec.ts` |
| Component | `features/documents/documents-general/document-list/document-list.component.spec.ts` |
| Component | `features/documents/factures/facture-list/facture-list.component.spec.ts` |
| Component | `features/field/kpi/kpi-dashboard/kpi-dashboard.component.spec.ts` |
| Component | `features/field/objectifs/objectif-form/objectif-form.component.spec.ts` |
| Component | `features/field/objectifs/objectif-list/objectif-list.component.spec.ts` |
| Component | `features/field/plannings/planning-form/planning-form.component.spec.ts` |
| Component | `features/field/plannings/planning-list/planning-list.component.spec.ts` |
| Component | `features/field/rapports/rapport-form/rapport-form.component.spec.ts` |
| Component | `features/field/rapports/rapport-list/rapport-list.component.spec.ts` |
| Component | `features/field/regions/region-form/region-form.component.spec.ts` |
| Component | `features/field/regions/region-list/region-list.component.spec.ts` |
| Component | `features/field/visites/visite-form/visite-form.component.spec.ts` |
| Component | `features/field/visites/visite-list/visite-list.component.spec.ts` |

---

## Notes
- Test runner: Vitest 4.1.5 (`@angular/build:unit-test`, Angular 21). Jasmine/Karma not used.
- `fakeAsync`/`tick` not supported in this Vitest+jsdom environment; all tests use synchronous RxJS `of()` streams.
- `ResizeObserver` mocked globally in `dashboard.component.spec.ts` to handle `ng-apexcharts` in jsdom.
- Out of scope (not listed in spec): additional feature components in `inventory/`, `marketing/`, `lots/`, `products/`, `promotions/`, `settings/`, `users/`, `orders` (components), `reclamations/`.

## Verdict
**PASS**
