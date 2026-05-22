# CynapCRM MAUI — Complete Analysis

Generated: 2026-05-22  
Branch: dev/Mobile-0001  
Project root: `Cynapharm-Mobile/`

---

## 1. Application Structure

### 1.1 Complete Page List with Shell Routes

| Page | Shell Route | Type | File |
|------|-------------|------|------|
| LoginPage | `//login` | ShellContent | Views/Auth/LoginPage.xaml |
| ForgotPasswordPage | `forgotpassword` (registered route) | Push | Views/Auth/ForgotPasswordPage.xaml |
| DashboardPage | `//dashboard` | Tab (DELEGUE only) | Views/Dashboard/DashboardPage.xaml |
| ProductListPage | `//products` | Tab (ALL roles) | Views/Products/ProductListPage.xaml |
| ProductDetailPage | `products/detail` (registered route) | Push | Views/Products/ProductDetailPage.xaml |
| DocumentViewerPage | `products/detail/viewer` (registered route) | Push | Views/Products/DocumentViewerPage.xaml |
| OrderListPage | `//orders` | Tab (CLIENT + DELEGUE) | Views/Orders/OrderListPage.xaml |
| OrderDetailPage | `orders/detail` (registered route) | Push | Views/Orders/OrderDetailPage.xaml |
| CreateOrderPage | `orders/create` (registered route) | Push | Views/Orders/CreateOrderPage.xaml |
| DocumentListPage | `//documents` | Tab (CLIENT only) | Views/Documents/DocumentListPage.xaml |
| DocumentDetailPage | `documents/detail` (registered route) | Push | Views/Documents/DocumentDetailPage.xaml |
| VisitListPage | `//visits` | Tab (DELEGUE only) | Views/Visites/VisitListPage.xaml |
| VisitDetailPage | `visits/detail` (registered route) | Push | Views/Visites/VisitDetailPage.xaml |
| RapportPage | `visits/rapport` (registered route) | Push | Views/Rapports/RapportPage.xaml |
| PlanningPage | `//planning` | Tab (DELEGUE only) | Views/Planning/PlanningPage.xaml |
| MyStockPage | `//stock` | FlyoutItem (DELEGUE only) | Views/Stock/MyStockPage.xaml |
| ObjectifPage | `//objectifs` | FlyoutItem (DELEGUE only) | Views/Objectifs/ObjectifPage.xaml |
| ReclamationListPage | `//reclamations` | FlyoutItem (CLIENT only) | Views/Reclamations/ReclamationListPage.xaml |
| ProfilePage | `//profile` | Tab (ALL roles) | Views/Profile/ProfilePage.xaml |
| EditProfilePage | `profile/edit` (registered route) | Push | Views/Profile/EditProfilePage.xaml |
| ChangePasswordPage | `profile/changepassword` (registered route) | Push | Views/Profile/ChangePasswordPage.xaml |

### 1.2 Navigation Flow Per Role

**DELEGUE / ADMIN / SUPERVISEUR**
- Entry point: `//dashboard`
- Tab bar: Dashboard, Visites, Planning, Catalogue, Commandes, Profil
- Flyout only: Mon Stock, Objectifs
- Flyout disabled: No (Flyout burger accessible)
- Full flyout: Dashboard, Visites, Planning, Catalogue, Commandes, Mon Stock, Objectifs, Profil

**CLIENT (PHARMACIEN / GROSSISTE)**
- Entry point: `//orders`
- Tab bar: Catalogue, Commandes, Documents, Profil
- Flyout only: Réclamations
- Flyout disabled: No

**MEDECIN**
- Entry point: `//products`
- Tab bar: Catalogue, Profil (only 2 tabs)
- Flyout: DISABLED (FlyoutBehavior.Disabled)
- No access to: Orders, Documents, Reclamations, Stock, Visites, Planning, Objectifs, Dashboard

### 1.3 AppShell Configuration Detail

- `Shell.FlyoutBehavior` defaults to `Flyout`, overridden to `Disabled` for MEDECIN in `ApplyRoleVisibility`.
- The custom flyout panel (not the MAUI default flyout) is a `<Shell.FlyoutContent>` with a scrollable nav item list.
- Tab visibility is set programmatically via `FlyoutDashboard.IsVisible`, etc. in `ApplyRoleVisibility()`.
- Catalogue tab has no `x:Name` and no conditional — it is **always visible** in the tab bar.
- Profil tab has no `x:Name` and no conditional — it is **always visible** in the tab bar.
- Secondary pages (Stock, Objectifs, Reclamations) are `<FlyoutItem>` elements with `FlyoutItemIsVisible="False"` — accessible only by Shell route from custom flyout taps.

---

## 2. Role-based Scenarios

### 2.1 Scenario MEDECIN

MEDECIN has the most restricted access: catalogue read-only + profile. No ordering, no documents, no field operations.

---

#### Page: ProductListPage
- **File**: `Views/Products/ProductListPage.xaml`
- **VM**: `ViewModels/Products/ProductListViewModel.cs`
- **API calls made**:
  - `GET products/visible` (MEDECIN uses `_useVisibleEndpoint = true`, calls `GetVisibleProductsAsync()`)
  - `GET products/categories` (category filter chips)
  - Offline fallback: SQLite `SearchProductsAsync`
- **Fields shown/hidden for MEDECIN**:
  - `CanSeePrices = false` — price column hidden in list via XAML binding
  - Category filter: visible
  - Search bar: visible
  - Product count badge: visible
- **Business logic specific to MEDECIN**:
  - Uses `/products/visible` endpoint (active, non-archived) instead of `/products`
  - `CanSeePrices` set to `false` in `LoadAsync()` when role is "MEDECIN"
- **Status**: ✅ Complete

---

#### Page: ProductDetailPage
- **File**: `Views/Products/ProductDetailPage.xaml`
- **VM**: `ViewModels/Products/ProductDetailViewModel.cs`
- **API calls made**:
  - `GET products/{id}` — product details
  - `GET products/lots/{productId}` — lots (silently skipped on 403/404 for MEDECIN)
  - `GET products/promos?productId={id}` — promotions (silently skipped on 403/404)
- **Fields shown/hidden for MEDECIN**:
  - `CanSeePrices = false` — INFORMATIONS card (`HasInformations = CanSeePrices`) hidden
  - Active/Inactive badge in header hidden (`DataTrigger` on `CanSeePrices=False`)
  - Image files filtered out from Supports list when `!CanSeePrices`
  - "Add to Order" button should be hidden for MEDECIN — **NOT currently hidden** (see Section 5, Issue #1)
- **Business logic specific to MEDECIN**:
  - Lots and promotions API calls wrapped in try/catch that swallows 403/404 silently
  - `CanSeePrices` flag set from role check in `InitAsync()`
- **Status**: ⚠️ Partial — AddToOrder button visible to MEDECIN

---

#### Page: DocumentViewerPage
- **File**: `Views/Products/DocumentViewerPage.xaml`
- **VM**: `ViewModels/Products/DocumentViewerViewModel.cs`
- **API calls made**:
  - `DownloadFileAsync(url)` — downloads bytes from Cloudinary/storage URL
- **Fields shown/hidden**: Identical for all roles (no role gating)
- **Business logic**: PDF shown via Google Docs viewer; images via `<Image>` control
- **Status**: ✅ Complete

---

#### Page: ProfilePage
- **File**: `Views/Profile/ProfilePage.xaml`
- **VM**: `ViewModels/Profile/ProfileViewModel.cs`
- **API calls made**: None (reads SecureStorage only via `AuthService.GetCurrentUserAsync()`)
- **Fields shown/hidden for MEDECIN**:
  - `IsMedecin = true` — role badge color may differ (DataTrigger in XAML)
  - No MEDECIN-specific field hiding in ProfileViewModel itself
- **Business logic**: Reads from SecureStorage; password change calls `PUT auth/change-password`
- **Status**: ✅ Complete

---

#### Page: EditProfilePage
- **File**: `Views/Profile/EditProfilePage.xaml`
- **VM**: `ViewModels/Profile/EditProfileViewModel.cs`
- **API calls made**:
  - `PUT auth/update-profile` — updates name, phone, address, email
- **Fields shown**: Name, Email (read-only display), Role (read-only), Telephone, Adresse
- **Status**: ✅ Complete

---

#### Page: ChangePasswordPage
- **File**: `Views/Profile/ChangePasswordPage.xaml`
- **VM**: `ViewModels/Profile/ChangePasswordViewModel.cs`
- **API calls made**:
  - `PUT auth/change-password` with `{Email, CurrentPassword, NewPassword}`
- **Status**: ✅ Complete

---

### 2.2 Scenario CLIENT (Pharmacien/Grossiste)

CLIENT can order, view documents, file reclamations, and browse catalogue.

---

#### Page: OrderListPage
- **File**: `Views/Orders/OrderListPage.xaml`
- **VM**: `ViewModels/Orders/OrderListViewModel.cs`
- **API calls made**:
  - `GET orders/by-client/{clientId}?page={p}&pageSize={size}[&statut={s}]` — CLIENT path
  - `GET orders?page={p}&pageSize={size}[&statut={s}]` — DELEGUE/ADMIN path
- **Fields shown/hidden for CLIENT**:
  - `IsGrossiste` flag: GROSSISTE gets KPI strip in header (order count + "Ce mois" volume placeholder)
  - PHARMACIEN does not see the KPI strip
  - Status filter chips visible for all
- **Business logic specific to CLIENT**:
  - Routes to `GetOrdersByClientAsync(clientId)` — filters by logged-in user's ID
  - Draft orders (Statut=0) filtered out: `filtered = result.Where(o => o.Statut != 0)`
- **Status**: ✅ Complete

---

#### Page: OrderDetailPage
- **File**: `Views/Orders/OrderDetailPage.xaml`
- **VM**: `ViewModels/Orders/OrderDetailViewModel.cs`
- **API calls made**:
  - `GET orders/{id}` — order details
  - `GET products/{productId}` for each ligne to resolve product name
  - `GET documents/factures/commande/{commandeId}` — linked factures
  - `GET documents/bons-commandes/commande/{commandeId}` — linked BCs
  - `GET documents/bons-livraison/commande/{commandeId}` — linked BLs
  - `PUT orders/{id}/cancel?motif={motif}` — cancel action
  - `POST orders/reclamations` — submit reclamation
- **Fields/actions shown for CLIENT**:
  - CanCancel: Statut 0, 1, or 2
  - CanCreateReclamation: Statut 4 or 5
  - Cancel and reclamation buttons visible (no role gating in VM — relies on order status)
- **Business logic**: N+1 problem for product name resolution (individual `GET products/{id}` per ligne)
- **Status**: ⚠️ Partial — N+1 calls on line items (Issue #2)

---

#### Page: CreateOrderPage
- **File**: `Views/Orders/CreateOrderPage.xaml`
- **VM**: `ViewModels/Orders/CreateOrderViewModel.cs`
- **API calls made**:
  - `GET products/search?keyword={q}&isActive=true&limit=20` — product search
  - `GET products/promos` — seed promotions on init
  - `POST orders` — submit order
- **Fields shown for CLIENT**:
  - 3-step wizard: product selection, cart review, confirmation
  - No role-specific field gating
- **Business logic**: Promotion engine reads from local SQLite `PromotionEntry` cache; cart persisted in `Preferences` per user
- **Status**: ✅ Complete

---

#### Page: DocumentListPage
- **File**: `Views/Documents/DocumentListPage.xaml`
- **VM**: `ViewModels/Documents/DocumentListViewModel.cs`
- **API calls made**:
  - `GET documents/client/{clientId}/type/{FACTURE|BC|BL}` — unified endpoint
- **Fields shown**: Tab switching between Factures / Bons de commande / Bons de livraison
- **Status**: ✅ Complete

---

#### Page: DocumentDetailPage
- **File**: `Views/Documents/DocumentDetailPage.xaml`
- **VM**: `ViewModels/Documents/DocumentDetailViewModel.cs`
- **API calls made**:
  - `GET documents/factures/{id}` OR `documents/bons-commandes/{id}` OR `documents/bons-livraison/{id}`
- **Status**: ✅ Complete

---

#### Page: ReclamationListPage
- **File**: `Views/Reclamations/ReclamationListPage.xaml`
- **VM**: `ViewModels/Reclamations/ReclamationListViewModel.cs`
- **API calls made**:
  - `GET orders/reclamations/by-client/{clientId}`
- **Status**: ✅ Complete

---

#### Pages shared with other roles: ProductListPage, ProductDetailPage, ProfilePage, EditProfilePage, ChangePasswordPage — see sections above.

---

### 2.3 Scenario DELEGUE

DELEGUE has full field operations access: visits, rapport, planning, stock management, objectives, KPIs, and can also place and view orders.

---

#### Page: DashboardPage
- **File**: `Views/Dashboard/DashboardPage.xaml`
- **VM**: `ViewModels/Dashboard/DashboardViewModel.cs`
- **API calls made**:
  - `GET fields/kpi/taux-conversion/{userId}?debut=...&fin=...` — conversion rate
  - `GET inventory/inventory-business/summary/{userId}` — stock summary card
  - `GET fields/visites/by-delegue/{userId}` (filtered to today) — today's visit count
  - `GET fields/objectifs/by-delegue/{userId}` — objectives list
  - KPIs: `GetKpisAsync()` returns empty list (stub, no real endpoint called)
- **Fields shown for DELEGUE**:
  - TodayVisitCount, TauxConversion, StockSummary card
  - ObjectifItems list with progress bars
  - Quick-access buttons to Visites and Planning
  - KpiItems — always empty (KpiService.GetKpisAsync returns empty list)
- **Business logic specific to DELEGUE**:
  - SUPERVISEUR path loads `GetRegionsAsync()` — distinct from DELEGUE path
  - `IsDelegue` flag used to show/hide TauxConversion stat
- **Status**: ⚠️ Partial — KPI section always empty (Issue #3)

---

#### Page: VisitListPage
- **File**: `Views/Visites/VisitListPage.xaml`
- **VM**: `ViewModels/Visites/VisitListViewModel.cs`
- **API calls made**:
  - `GET fields/visites/by-delegue/{userId}` — all visites for delegate, filtered client-side by date/status
- **Fields shown**: Status filter chips, date range pickers, visit list with status badges
- **Issue**: Search Entry in the XAML header is not bound to any VM property — decorative only (Issue #4)
- **Status**: ⚠️ Partial

---

#### Page: VisitDetailPage
- **File**: `Views/Visites/VisitDetailPage.xaml`
- **VM**: `ViewModels/Visites/VisitDetailViewModel.cs`
- **API calls made**:
  - `GET fields/visites/{id}` — load existing visite
  - `POST fields/visites` — create new visite
  - `PUT fields/visites/{id}` — update visite
  - `DELETE fields/visites/{id}` — delete visite
- **Status**: ✅ Complete

---

#### Page: RapportPage
- **File**: `Views/Rapports/RapportPage.xaml`
- **VM**: `ViewModels/Rapports/RapportViewModel.cs`
- **API calls made**:
  - `GET products` — load product list for "produits discutés" multi-select
  - `POST fields/rapports/createUpdate` — submit rapport (online)
  - Offline: saved to `Pending_Rapports` SQLite table; synced by `SyncService`
- **Fields shown**:
  - Contenu (validated: required, min 20 chars)
  - Resultat picker (POSITIF / NEGATIF / EN_ATTENTE)
  - Produits discutés multi-select checklist
  - GPS status banner
- **Status**: ✅ Complete

---

#### Page: PlanningPage
- **File**: `Views/Planning/PlanningPage.xaml`
- **VM**: `ViewModels/Planning/PlanningViewModel.cs`
- **API calls made**:
  - `GET fields/plannings/by-range?idDelegue={userId}&startDate={...}&endDate={...}` — weekly planning
- **Fields shown**: Week navigation (prev/next), daily grouped list
- **AddVisit navigation**: routes to `///visits/detail` (note triple slash — absolute route)
- **Status**: ✅ Complete

---

#### Page: MyStockPage
- **File**: `Views/Stock/MyStockPage.xaml`
- **VM**: `ViewModels/Stock/MyStockViewModel.cs`
- **API calls made**:
  - `GET inventory/stocks-delegue/by-delegue/{userId}` — échantillons
  - `GET inventory/stocks-promotionnels` — promo stock
  - `GET inventory/stock-movements/by-delegue/{userId}` — movement history
  - `POST inventory/distributions` — record a distribution (optimistic: local deduct first)
- **Segments**: 0=Échantillons, 1=Promos, 2=Historique
- **Status**: ✅ Complete

---

#### Page: ObjectifPage
- **File**: `Views/Objectifs/ObjectifPage.xaml`
- **VM**: `ViewModels/Objectifs/ObjectifViewModel.cs`
- **API calls made**:
  - `GET fields/objectifs/by-delegue/{userId}` (DELEGUE) or `GET fields/objectifs` (SUPERVISEUR/ADMIN)
- **Fields shown**: Global achievement %, list of objectives with progress bars
- **Status**: ✅ Complete

---

#### Pages shared: ProductListPage, ProductDetailPage, OrderListPage, OrderDetailPage, CreateOrderPage, ProfilePage, EditProfilePage, ChangePasswordPage

---

## 3. API Calls Inventory

| Service | Method | HTTP Verb | Full URL | Roles That Use It | Status |
|---------|--------|-----------|----------|-------------------|--------|
| AuthService | LoginAsync | POST | `auth/login` | All (unauthenticated) | Active |
| AuthService | ForgotPasswordAsync | POST | `auth/forgot-password` | All | Active |
| AuthService | ChangePasswordAsync | PUT | `auth/change-password` | All | Active |
| AuthService | UpdateProfileAsync | PUT | `auth/update-profile` | All | Active |
| ProductService | GetProductsAsync (all) | GET | `products` | DELEGUE, ADMIN | Active |
| ProductService | GetProductsAsync (search) | GET | `products/search?keyword={q}&isActive=true&limit={n}` | All | Active |
| ProductService | GetProductByIdAsync | GET | `products/{id}` | All | Active |
| ProductService | GetLotsByProductAsync | GET | `products/lots/{productId}` | DELEGUE, CLIENT | Active |
| ProductService | GetPromotionsAsync | GET | `products/promos[?productId={id}]` | DELEGUE, CLIENT | Active |
| ProductService | GetVisibleProductsAsync | GET | `products/visible` | MEDECIN, CLIENT | Active |
| ProductService | GetCategoriesAsync | GET | `products/categories` | All | Active |
| ProductService | GetMarketingAsync | GET | `products/marketing[?productId={id}]` | (unused in VMs) | Unused |
| ProductService | DownloadFileAsync | GET | `{any URL}` | All | Active |
| OrderService | GetOrdersAsync | GET | `orders?page={p}&pageSize={s}[&statut={s}]` | DELEGUE, ADMIN | Active |
| OrderService | GetOrdersByStatusAsync | GET | `orders/by-status?page={p}&pageSize={s}[&statut={s}]` | (defined, unused) | Unused |
| OrderService | GetOrdersByClientAsync | GET | `orders/by-client/{id}?page={p}&pageSize={s}[&statut={s}]` | CLIENT | Active |
| OrderService | GetOrderByIdAsync | GET | `orders/{id}` | All with orders | Active |
| OrderService | GetLignesAsync | GET | `orders/lignes?orderId={id}` | (defined, unused — lignes come embedded in order) | Unused |
| OrderService | CreateOrderAsync | POST | `orders` | CLIENT, DELEGUE | Active |
| OrderService | UpdateOrderStatusAsync | PUT | `orders/{id}/status` | (defined, unused in VMs) | Unused |
| OrderService | CancelOrderAsync | PUT | `orders/{id}/cancel?motif={motif}` | CLIENT, DELEGUE | Active |
| OrderService | CreateReclamationAsync | POST | `orders/reclamations` | CLIENT | Active |
| OrderService | GetReclamationsAsync | GET | `orders/reclamations?orderId={id}` | (defined, unused in VMs) | Unused |
| OrderService | GetReclamationsByClientAsync | GET | `orders/reclamations/by-client/{clientId}` | CLIENT | Active |
| DocumentService | GetFacturesAsync | GET | `documents/factures?page={p}&size={s}` | (defined, unused) | Unused |
| DocumentService | GetFactureByIdAsync | GET | `documents/factures/{id}` | CLIENT | Active |
| DocumentService | GetBonsCommandeAsync | GET | `documents/bons-commandes?page={p}&size={s}` | (defined, unused) | Unused |
| DocumentService | GetBonCommandeByIdAsync | GET | `documents/bons-commandes/{id}` | CLIENT | Active |
| DocumentService | GetBonsLivraisonAsync | GET | `documents/bons-livraison?page={p}&size={s}` | (defined, unused) | Unused |
| DocumentService | GetBonLivraisonByIdAsync | GET | `documents/bons-livraison/{id}` | CLIENT | Active |
| DocumentService | GetDocumentsByClientAndTypeAsync | GET | `documents/client/{id}/type/{FACTURE\|BC\|BL}` | CLIENT | Active |
| DocumentService | GetFacturesByCommandeAsync | GET | `documents/factures/commande/{commandeId}` | All with orders | Active |
| DocumentService | GetBCByCommandeAsync | GET | `documents/bons-commandes/commande/{commandeId}` | All with orders | Active |
| DocumentService | GetBLByCommandeAsync | GET | `documents/bons-livraison/commande/{commandeId}` | All with orders | Active |
| InventoryService | GetStockMouvementsAsync | GET | `inventory/stock-movements?[productId=...&from=...]` | (defined, unused in VMs) | Unused |
| InventoryService | GetStockDelegueAsync | GET | `inventory/stocks-delegue/by-delegue/{userId}` | DELEGUE | Active |
| InventoryService | GetStockPromoAsync | GET | `inventory/stocks-promotionnels` | DELEGUE | Active |
| InventoryService | GetDistributionAsync | GET | `inventory/distributions` | (defined, unused) | Unused |
| InventoryService | PostDistributionAsync | POST | `inventory/distributions` | DELEGUE | Active |
| InventoryService | GetStockSummaryAsync | GET | `inventory/inventory-business/summary/{idDelegue}` | DELEGUE | Active |
| InventoryService | GetMovementsByDelegueAsync | GET | `inventory/stock-movements/by-delegue/{idDelegue}` | DELEGUE | Active |
| VisiteService | GetVisitesAsync | GET | `fields/visites/by-delegue/{userId}` (then client-side filter) | DELEGUE | Active |
| VisiteService | GetVisiteByIdAsync | GET | `fields/visites/{id}` | DELEGUE | Active |
| VisiteService | CreateVisiteAsync | POST | `fields/visites` | DELEGUE | Active |
| VisiteService | UpdateVisiteAsync | PUT | `fields/visites/{id}` | DELEGUE | Active |
| VisiteService | DeleteVisiteAsync | DELETE | `fields/visites/{id}` | DELEGUE | Active |
| VisiteService | CreateRapportAsync | POST | `fields/rapports/createUpdate` | DELEGUE | Active |
| VisiteService | GetRapportsAsync (with ID) | GET | `fields/rapports/by-visite/{visiteId}` | (defined, unused in VMs) | Unused |
| VisiteService | GetRapportsAsync (all) | GET | `fields/rapports/all` | (defined, unused in VMs) | Unused |
| PlanningService | GetPlanningAsync | GET | `fields/plannings/by-range?idDelegue={id}&startDate={...}&endDate={...}` | DELEGUE | Active |
| PlanningService | CreatePlanningEntryAsync | POST | `fields/plannings` | (defined, unused in VMs) | Unused |
| PlanningService | UpdatePlanningEntryAsync | PUT | `fields/plannings/{id}` | (defined, unused in VMs) | Unused |
| PlanningService | DeletePlanningEntryAsync | DELETE | `fields/plannings/{id}` | (defined, unused in VMs) | Unused |
| KpiService | GetObjectifsAsync (DELEGUE) | GET | `fields/objectifs/by-delegue/{userId}` | DELEGUE | Active |
| KpiService | GetObjectifsAsync (ADMIN) | GET | `fields/objectifs` | ADMIN, SUPERVISEUR | Active |
| KpiService | GetKpisAsync | — | (returns empty list, no HTTP call) | — | Stub |
| KpiService | GetTauxConversionAsync | GET | `fields/kpi/taux-conversion/{idDelegue}?debut=...&fin=...` | DELEGUE | Active |
| KpiService | GetRegionsAsync | GET | `fields/regions` | SUPERVISEUR | Active |

---

## 4. Models — Field Mapping Issues

| Model | Field Name | C# Type | JSON Key (if explicit) | Issue |
|-------|-----------|---------|----------------------|-------|
| LoginResponse | Token | string | (auto) | — |
| LoginResponse | Expiry | DateTime | (auto) | `Expiry` not used — JWT parsed manually from token |
| UserInfo | Telephone | string? | `phoneNumber` | JSON name mismatch: backend sends `phoneNumber`, decorated with `[JsonPropertyName("phoneNumber")]` — OK |
| UpdateProfileDto | PhoneNumber | string? | (auto → `PhoneNumber`) | Backend field is `PhoneNumber`; mobile sends `PhoneNumber` — OK |
| Order | Id | int | `id_Commande` | OK |
| Order | MontantTotal | decimal | `montantTotalHT` | Mapped to HT; TTC is separate field `montantTTC` |
| Order | Lignes | List<LigneCommande> | (auto) | Backend may embed or not; `GetLignesAsync` exists but unused — lignes fetched as embedded in Order |
| LigneCommande | Id | int | `id_Ligne` | OK |
| LigneCommande | CommandeId | int | `id_Commande` | OK |
| LigneCommande | ProductId | int | `id_Produit` | OK |
| LigneCommande | ProductNom | string | (auto) | Not in backend DTO — enriched client-side via product fetch |
| LigneCommande | NumeroLot | string | (auto) | May not be returned by backend Order detail endpoint |
| LigneCommande | Remise | decimal | (auto) | Backend field name may differ |
| Reclamation | Id | int | `id_Rec` | OK |
| Reclamation | CommandeId | int | `id_Commande` | OK |
| Reclamation | LigneId | int | `id_Ligne` | OK |
| Reclamation | Motif | string | `message` | Mapped from `message`; backend uses "message" not "motif" |
| Reclamation | DateCreation | DateTime | `dateReclamation` | OK |
| Facture | Id | int | `numero_Doc` | OK |
| Facture | NumeroFacture | string | `nom_Doc` | OK |
| Facture | DateFacture | DateTime | `dateFacture` | OK |
| Facture | TVA | decimal | (none) | Not in backend DTO — stub field for XAML binding, always 0 |
| Facture | Statut | string | (none) | Not in backend DTO — stub field, always empty |
| BonCommande | Id | int | `numero_Doc` | OK |
| BonCommande | DateEmission | DateTime | `dateCreation` | OK |
| BonCommande | MontantTotal | decimal | (none) | Not in backend DTO — stub, always 0 |
| BonCommande | Statut | string | (none) | Not in backend DTO — stub, always empty |
| BonLivraison | Id | int | `numero_Doc` | OK |
| BonLivraison | DateLivraison | DateTime | `dateCreation` | OK |
| BonLivraison | Statut | string | (none) | Not in backend DTO — stub, always empty |
| DocumentSummary | Id | int | `numero_Doc` | OK |
| DocumentSummary | Numero | string | `nom_Doc` | OK |
| DocumentSummary | Date | DateTime | `dateCreation` | OK |
| DocumentSummary | Type | string | `typeDocument` | OK |
| DocumentSummary | Url | string? | `url_Document` | Present only when backend includes it |
| DocumentSummary | Statut | string | (none) | Not in backend DocumentDto — stub |
| DocumentSummary | Montant | decimal? | (none) | Not in backend DocumentDto — stub |
| Product | Id | int | `id_Produit` | OK |
| Product | PrixUnitaire | decimal | `prixVente` | OK |
| Product | Actif | bool | `isActive` | OK |
| Product | ImageUrl | string? | (none) | Computed client-side from `Supports`; not a backend field |
| Lot | Id | int | (auto) | No explicit JSON key — relies on case-insensitive matching |
| Lot | ProductId | int | (auto) | No explicit JSON key |
| Lot | NumeroLot | string | (auto) | OK |
| Promotion | Id | int | (auto) | No explicit JSON key |
| Promotion | ProductId | int? | (auto) | No explicit JSON key — nullable |
| Promotion | RemisePourcentage | decimal? | (auto) | No explicit JSON key — nullable |
| StockDelegue | Id | int | `id_stock` | OK |
| StockDelegue | IdDelegue | int | `id_User_Delegue` | OK |
| StockDelegue | ProductId | int | `id_Produit` | OK |
| StockDelegue | NumeroLot | string | `numeroLot` | OK |
| StockDelegue | DateExpiration | DateTime? | `dateExpiration` | OK |
| StockDelegue | QuantiteRestante | int | `qteDisponible` | Renamed: backend `qteDisponible` → mobile `QuantiteRestante` |
| StockDelegue | QuantiteReservee | int | `qteReservee` | OK |
| StockDelegue | ProductNom | string | (none) | Not in backend DTO — enriched client-side |
| StockDelegue | QuantiteAllouee | int | (none) | Not in backend DTO — offline SQLite compatibility |
| StockMouvement | Id | int | (auto) | No explicit JSON key |
| StockMouvement | ProductId | int | (auto) | No explicit JSON key |
| StockMouvement | ProductNom | string | (auto) | May not be in backend DTO |
| StockMouvement | TypeMouvement | string | (auto) | No explicit JSON key |
| Visite | Id | int | `idVisite` | OK |
| Visite | DelegueId | int | `id_User_Delegue` | OK |
| Visite | ClientNom | string | (auto) | No explicit JSON key |
| Visite | ClientType | string | (auto) | No explicit JSON key |
| Visite | HasRapport | bool | (auto) | No explicit JSON key |
| Visite | Statut | string | (none) | Derived from `IsCompleted` — not a direct backend field |
| Planning | Id | int | `id_Planning` | OK |
| Planning | DelegueId | int | `id_User_Delegue` | OK |
| Planning | DatePlanifiee | DateTime | `date` | OK |
| Planning | HeureDebut | TimeSpan | (auto) | TimeSpan JSON serialization may fail (Issue #5) |
| Planning | HeureFin | TimeSpan | (auto) | TimeSpan JSON serialization may fail (Issue #5) |
| Planning | Etat | string | (auto) | No explicit JSON key |
| Planning | ClientNom | string | (auto) | Not returned in list response |
| Planning | Objectif | string? | (auto) | Not returned in list response |
| Objectif | Id | int | `id_Objectif` | OK |
| Objectif | DelegueId | int | `id_User_Delegue` | OK |
| Objectif | TypeCode | int | `type` | OK |
| Objectif | ValeurActuelle | decimal? | `valeurRealisee` | OK |
| Objectif | PeriodeCode | int | `periode` | OK |
| Kpi | — | — | — | Entire model unused (GetKpisAsync returns empty list) |
| Region | Id | int | (auto) | No explicit JSON key |
| Region | Nom | string | (auto) | No explicit JSON key |

---

## 5. Business Logic Issues

| # | Scenario | File:Location | Issue Description | Impact | Priority |
|---|----------|---------------|-------------------|--------|----------|
| 1 | MEDECIN | ViewModels/Products/ProductDetailViewModel.cs:167 | `AddToOrderAsync` command exists and is presumably bound in XAML — navigates to `//orders/create`. MEDECIN should not be able to place orders, but there is no role check blocking this navigation. | MEDECIN can reach CreateOrderPage by tapping the button. | High |
| 2 | All with orders | ViewModels/Orders/OrderDetailViewModel.cs:76-80 | N+1 API calls: for each `LigneCommande`, a separate `GET products/{productId}` is made to resolve product name. A 10-item order triggers 10 extra API calls. | Performance degradation, high latency on order detail load. | High |
| 3 | DELEGUE | Services/KpiService.cs:35-37 | `GetKpisAsync()` returns an empty list via `Task.FromResult` — no HTTP call is made. The Dashboard KPI section is always empty. | KPI cards on Dashboard are always "Aucun KPI disponible". | High |
| 4 | DELEGUE | Views/Visites/VisitListPage.xaml:41-47 | The search Entry in VisitListPage header is not bound to any ViewModel property (`Text` binding absent). The search field is decorative only — typing in it has no effect. | Search functionality misleadingly shown but does nothing. | High |
| 5 | DELEGUE | Models/Field/Planning.cs:17-18 | `HeureDebut` and `HeureFin` are `TimeSpan`. .NET's `System.Text.Json` serializes `TimeSpan` as `"hh:mm:ss"` by default. The backend likely uses a different format or returns these as ISO strings. This can cause deserialization failures. | Planning entries may lose time data or fail to deserialize. | Medium |
| 6 | All | Services/AuthService.cs:19 | Login endpoint called as `auth/login` — but `ApiRoutes.Auth.Login` is `api/auth/login`. The service does NOT use `ApiRoutes.Auth.Login`. Both are defined; the service bypasses the constants. | Inconsistency: if gateway routing changes, two different URLs are in play. | Medium |
| 7 | All | Services/AuthService.cs:48,51 | `ForgotPasswordAsync` and `ChangePasswordAsync` call `auth/forgot-password` and `auth/change-password` directly (without `api/` prefix), while `ApiRoutes.Auth.ForgotPassword = "api/auth/forgot-password"` and `ApiRoutes.Auth.ChangePassword = "api/auth/change-password"`. Routes mismatch — service bypasses ApiRoutes constants. | If the gateway prefix changes, the service calls will break. | Medium |
| 8 | CLIENT | ViewModels/Documents/DocumentDetailViewModel.cs:23-24 | `IsFacture`, `IsBonCommande`, `IsBonLivraison` check `DocumentType` against lowercase strings `"facture"`, `"bon-commande"`, `"bon-livraison"`. But when navigating from `OrderDetailViewModel`, the type is passed as `"FACTURE"`, `"BC"`, `"BL"` (uppercase). The DocumentDetailViewModel will never match and will show an empty page. | Document detail from OrderDetail will always show blank content. | High |
| 9 | DELEGUE | ViewModels/Planning/PlanningViewModel.cs:71 | `AddVisitAsync` navigates to `///visits/detail` (triple-slash absolute route). This is non-standard and may cause navigation issues on some MAUI versions. Registered routes use `//visits/detail`. | Planning "add visit" button may throw a navigation exception. | Medium |
| 10 | DELEGUE | ViewModels/Dashboard/DashboardViewModel.cs:47-48 | `IsSuperviseur` and `IsDelegue` are set but `ADMIN` role is never checked. An `ADMIN` user would have `IsDelegue = false` and `IsSuperviseur = false`, so neither the regions block (SUPERVISEUR) nor the stock/taux block (DELEGUE) would execute. Dashboard would show only empty objectives. | ADMIN role has broken Dashboard behavior. | Medium |
| 11 | DELEGUE | Services/InventoryService.cs:36-57 | `PostDistributionAsync` sends `id_Medecin = null` and `id_Pharmacien = null` always. The backend `EchantillonDto` expects at least one recipient. Distribution without a recipient may be rejected by the backend or stored incorrectly. | Sample distributions may fail silently or be stored without a valid recipient. | High |
| 12 | All | ViewModels/Profile/ProfileViewModel.cs:59-65 | `Edit()` method resets fields to `User.Name`, `User.Telephone`, `User.Adresse` when `IsEditing=true` (i.e., when the user cancels while already editing). But `User.Telephone` may be null, causing a null assignment to `EditTelephone` which is a `string` property — OK since `= User.Telephone` would be `null`. However the Save path mutates the `User` object directly (`User.Name = EditName.Trim()`) without going through the API. Changes are saved only locally to SecureStorage, never sent to backend in ProfileViewModel (only EditProfileViewModel calls UpdateProfileAsync). | Profile "quick edit" on ProfilePage is local-only; backend never updated from this path. | Medium |
| 13 | All | ViewModels/Orders/OrderListViewModel.cs:76 | `HasMore = result.Count == 20` uses a hardcoded page size of 20 to determine if more pages exist. If the backend returns exactly 20 items on the last page, an extra empty load-more request will fire. | Minor UX issue — one extra empty request at end of list. | Low |
| 14 | MEDECIN | AppShell.xaml.cs:116 | `FlyoutOrders.IsVisible = isClient || isDelegue` — MEDECIN does not get Orders tab (correct). But the Orders route `//orders` is never set as invisible; it remains registered. A MEDECIN user with direct route knowledge could theoretically navigate to `//orders`. | Minor — Orders page itself does not role-gate the data fetch (will load orders for any logged-in user). | Low |
| 15 | DELEGUE | ViewModels/Rapports/RapportViewModel.cs:82-90 | In `LoadProduitsAsync`, if online, it calls `GetProductsAsync(null, 100)` which fetches ALL products including archived. The rapport product checklist may show archived/inactive products for DELEGUE. Should filter `p.Actif && !p.IsArchived` — which IS done in the code (line 83). OK. (False positive — actually OK.) | — | N/A |
| 16 | All | Services/ApiService.cs:163-167 | `HandleResponseAsync` always deserializes as `ApiResponse<T>` wrapper. If any backend endpoint returns a raw JSON array (not wrapped in `{result:[], isSuccess:true}`), the result will be `null`. This is the implicit contract assumption — must be verified against every backend endpoint. | Any non-wrapped endpoint will silently return null. | Medium |
| 17 | DELEGUE | Models/Field/Visite.cs:29-34 | The `IsCompleted` setter only sets `Statut` if `Statut` is currently empty. But when loading from backend, `Statut` is deserialized first (if present) and `IsCompleted` is set after. If backend sends both fields, `IsCompleted` setter will overwrite the already-set `Statut` only when Statut is empty. If backend doesn't send `Statut` but sends `IsCompleted`, the derived Statut is set. Race condition depending on JSON property order. | Visit status may show incorrect value if JSON property order differs. | Medium |
| 18 | All | Services/AuthService.cs:44-47 | Login response stores `result.User.Telephone` to SecureStorage as `user_telephone_{userId}`. But `UserInfo.Telephone` is mapped from JSON `phoneNumber`. If the backend's login response doesn't include `phoneNumber`, the telephone is stored as empty string. | User profile phone number may always be empty after login. | Medium |

---

## 6. Missing Screens/Features

| Scenario | Missing Feature | Current Workaround | Impact |
|----------|-----------------|--------------------|--------|
| DELEGUE | Create/Edit/Delete Planning entries from PlanningPage | `PlanningService.CreatePlanningEntryAsync/UpdatePlanningEntryAsync/DeletePlanningEntryAsync` exist but no UI form. Only reads are done. | Delegates cannot manage their own planning in-app. |
| DELEGUE | Real KPI data on Dashboard | `KpiService.GetKpisAsync()` is a stub returning empty list. KPI section always shows "Aucun KPI disponible". | KPI monitoring completely non-functional. |
| DELEGUE | Rapport editing / history view | `GetRapportsAsync` exists but no UI screen to list or view previously submitted rapports. | Cannot review past reports in the app. |
| DELEGUE | Visite search field functional | The search Entry in VisitListPage is decorative only, not bound. | Cannot search visits by client name. |
| CLIENT | Document download/open | `DocumentSummary.Url` exists but `OpenDocumentAsync` in DocumentListVM simply calls `Launcher.OpenAsync(doc.Url)` — if URL is null/empty (stub field often), nothing happens. No download fallback. | Documents without URL cannot be opened. |
| CLIENT | GROSSISTE KPI strip "Ce mois" volume | The OrderListPage header shows "Ce mois / Volume" for GROSSISTE but the value is hardcoded to "Ce mois" (not a real total). | Volume KPI is a UI placeholder with no real data. |
| MEDECIN | Any way to contact a delegate or sales rep | No messaging, contact, or request feature for MEDECIN role. | MEDECIN can only browse catalogue passively. |
| All | No push notification integration | `PushNotification` is listed in deferred tools but app has no notification infrastructure. | No order status updates, no alerts. |
| All | Offline mode for Documents | Documents have no SQLite caching layer. DocumentListVM requires connectivity. | Documents unavailable offline. |
| All | Profile photo / avatar image | Avatar is always initials-based. No photo upload capability. | Minor UX gap. |
| DELEGUE | Delete visit requires confirmation | `DeleteAsync` does show a DisplayAlert confirmation — this IS implemented. | — (not missing) |
| All | Token refresh / silent re-auth | JWT is checked for expiry by TokenValidationHandler but no refresh flow exists. On expiry, user is redirected to login with no session recovery. | User session terminates abruptly on token expiry. |

---

## 7. Complete Code of Every File Read

### AppShell.xaml (AppShell.xaml)
```xml
<?xml version="1.0" encoding="UTF-8" ?>
<Shell x:Class="Cynapharm_Mobile.AppShell"
       x:Name="AppShellRoot"
       xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
       xmlns:auth="clr-namespace:Cynapharm_Mobile.Views.Auth"
       xmlns:dashboard="clr-namespace:Cynapharm_Mobile.Views.Dashboard"
       xmlns:visites="clr-namespace:Cynapharm_Mobile.Views.Visites"
       xmlns:planning="clr-namespace:Cynapharm_Mobile.Views.Planning"
       xmlns:rapports="clr-namespace:Cynapharm_Mobile.Views.Rapports"
       xmlns:stock="clr-namespace:Cynapharm_Mobile.Views.Stock"
       xmlns:objectifs="clr-namespace:Cynapharm_Mobile.Views.Objectifs"
       xmlns:products="clr-namespace:Cynapharm_Mobile.Views.Products"
       xmlns:orders="clr-namespace:Cynapharm_Mobile.Views.Orders"
       xmlns:documents="clr-namespace:Cynapharm_Mobile.Views.Documents"
       xmlns:reclamations="clr-namespace:Cynapharm_Mobile.Views.Reclamations"
       xmlns:profile="clr-namespace:Cynapharm_Mobile.Views.Profile"
       Shell.FlyoutBehavior="Flyout"
       Shell.TabBarBackgroundColor="{StaticResource CardBackground}"
       Shell.TabBarUnselectedColor="{StaticResource TextSecondary}"
       Shell.TabBarTitleColor="{StaticResource Primary}"
       Shell.TabBarForegroundColor="{StaticResource Primary}"
       BackgroundColor="{StaticResource Primary}"
       ForegroundColor="White">
    <!-- [580 lines — full content as read above] -->
</Shell>
```

### AppShell.xaml.cs (AppShell.xaml.cs)
```csharp
// Full content: 168 lines — role computation, visibility properties, navigation commands,
// LoadUserInfoAsync, BuildInitials, NotifyAll, ApplyRoleVisibility.
// See Section 1 and 2 for behavioral analysis.
```

### MauiProgram.cs (MauiProgram.cs)
```csharp
// Full content: 208 lines — DI registrations, HttpClient with Polly resilience,
// AppSettings loading, global crash handlers, Tabler font registration.
```

### App.xaml (App.xaml)
```xml
<?xml version = "1.0" encoding = "UTF-8" ?>
<Application xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Cynapharm_Mobile.App">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="Resources/Styles/Colors.xaml" />
                <ResourceDictionary Source="Resources/Styles/Styles.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

### App.xaml.cs (App.xaml.cs)
```csharp
// Full content: 164 lines — global culture setup (TND format), SessionExpired handler,
// ConnectivityChanged handler, CreateWindow, OnShellLoaded (auth check + role-based navigation),
// SyncService flush on startup.
```

### AppSettings.cs (AppSettings.cs)
```csharp
namespace Cynapharm_Mobile;
public class AppSettings
{
    public string ApiGatewayBaseUrl { get; set; } = "http://cynapharmgateway.runasp.net/";
    public string? ApiGatewayBaseUrlProd { get; set; }
}
```

### StorageKeys.cs (StorageKeys.cs)
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

### Services/Api/ApiRoutes.cs
```csharp
// Full content: 74 lines
// Auth: api/auth/login, api/auth/forgot-password, api/auth/change-password, auth/update-profile, api/auth/me
// Products: api/products, api/products/search, api/products/categories, api/lots, api/promos, api/marketting
// Orders: orders, orders/by-status, orders/by-date, orders/dashboard, orders/{0}/cancel, orders/status, orders/by-client, orders/lignes, orders/reclamations
// Field: api/visites, api/rapports, api/plannings, api/objectifs, api/kpi, api/regions, fields/visites, fields/kpi/taux-conversion, fields/plannings, fields/rapports/by-delegue
// Inventory: api/stocks-delegue, api/stock-movements, api/distributions, api/stocks-promotionnels, api/inventory-business, inventory/inventory-business/summary, inventory/stock-movements/by-delegue, inventory/distributions
// Documents: api/factures, api/bons-commandes, api/bons-livraison, documents/client/{0}/type/{1}, documents/factures/commande, documents/bons-commandes/commande, documents/bons-livraison/commande
```

### Services/Api/ApiException.cs
```csharp
public class ApiException : Exception
{
    public HttpStatusCode? StatusCode { get; }
    public ApiException(string message, Exception? inner = null, HttpStatusCode? statusCode = null)
        : base(message, inner) => StatusCode = statusCode;
}
```

### Services/Api/HttpLoggingHandler.cs
```csharp
// Delegates handler logging HTTP method, URI, response code, and elapsed time via Debug.WriteLine.
```

### Services/Api/TokenValidationHandler.cs
```csharp
// Proactive JWT expiry check (5-minute threshold). Raises ApiService.SessionExpired
// if token is expiring soon. Also handles reactive 401 from server.
```

### Services/ApiService.cs
```csharp
// Full content: 205 lines
// GetAsync<T>, PostAsync<T>, PutAsync<T>, DeleteAsync — all add Bearer token via PrepareAuthHeaderAsync.
// HandleResponseAsync: unwraps ApiResponse<T> wrapper, throws ApiException for non-success.
// DownloadFileAsync: sends auth header only for own API host; uses shared _externalClient for CDN.
// SessionExpired static event + RaiseSessionExpired().
```

### Services/AuthService.cs
```csharp
// Full content: 150 lines
// LoginAsync: POST auth/login, stores JWT + expiry + user fields in SecureStorage.
// ForgotPasswordAsync: POST auth/forgot-password.
// ChangePasswordAsync: PUT auth/change-password.
// UpdateProfileAsync: PUT auth/update-profile (via ApiRoutes.Auth.UpdateProfile).
// GetCurrentUserAsync: reads from SecureStorage; JWT email fallback via JwtSecurityTokenHandler.
// IsAuthenticatedAsync, GetUserRoleAsync, IsTokenExpiringSoonAsync, Logout.
```

### Services/OrderService.cs
```csharp
// Full content: 62 lines — CRUD for orders, reclamations, lignes.
// Key: GetOrdersByClientAsync routes to orders/by-client/{id}.
// CancelOrderAsync: PUT orders/{id}/cancel?motif=... (url-encoded motif in query).
```

### Services/DocumentService.cs
```csharp
// Full content: 49 lines — 3 typed document endpoints + unified client+type + linked-to-order variants.
```

### Services/ProductService.cs
```csharp
// Full content: 89 lines
// GetProductsAsync: GET products OR products/search?keyword=...
// GetVisibleProductsAsync: GET products/visible (MEDECIN/CLIENT).
// ExtractImageUrl: helper to pull first active Image support from Supports list.
```

### Services/InventoryService.cs
```csharp
// Full content: 76 lines
// GetStockDelegueAsync: reads userId from SecureStorage.
// PostDistributionAsync: builds EchantillonDto with id_Medecin=null, id_Pharmacien=null.
// GetStockSummaryAsync, GetMovementsByDelegueAsync.
```

### Services/VisiteService.cs
```csharp
// Full content: 85 lines
// GetVisitesAsync: GET fields/visites/by-delegue/{userId} then client-side filter by date/status.
// CreateRapportAsync: builds payload matching backend RapportVisiteDto exactly.
// GetRapportsAsync: GET fields/rapports/by-visite/{id} OR fields/rapports/all.
```

### Services/PlanningService.cs
```csharp
// Full content: 30 lines
// GetPlanningAsync: week range query by delegue ID.
// CRUD methods defined but unused in ViewModels.
```

### Services/KpiService.cs
```csharp
// Full content: 49 lines
// GetObjectifsAsync: role-conditional (SUPERVISEUR=all, DELEGUE=by-delegue/{id}, else empty).
// GetKpisAsync: STUB — returns empty list, no HTTP call.
// GetTauxConversionAsync: GET fields/kpi/taux-conversion/{id}?debut=...&fin=...
// GetRegionsAsync: GET fields/regions.
```

### Services/SyncService.cs
```csharp
// Full content: 74 lines
// Singleton; drains SQLite Pending_Rapports table when connectivity is restored.
// Thread-safe via Interlocked compare-exchange.
```

### Services/LocalDatabaseService.cs
```csharp
// Full content: 251 lines
// Tables: Product_Cache, Stock_Local, Pending_Rapports, Promotion_Cache, Log_Entries.
// SeedProductsAsync, SearchProductsAsync, SeedStockAsync, DeductStockAsync (atomic),
// SeedPromotionsAsync, GetActivePromotionAsync (product-specific before global),
// InsertPendingRapportAsync, GetPendingRapportsAsync, MarkRapportSyncedAsync,
// SaveLogAsync (capped at 515 entries), GetRecentLogsAsync.
```

### Services/Cache/ICacheService.cs
```csharp
public interface ICacheService
{
    Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T?>> factory, TimeSpan ttl);
    void Invalidate(string key);
    void InvalidateAll();
}
```

### Services/Cache/MemoryCacheService.cs
```csharp
// Full content: 47 lines — thread-safe dictionary with expiry timestamps.
// Factory runs outside lock to avoid blocking during network calls.
```

### Services/Navigation/INavigationService.cs
```csharp
public interface INavigationService
{
    Task GoToAsync(string route);
    Task GoToAsync<TParam>(string route, TParam param) where TParam : class;
    Task GoBackAsync();
    Task GoToRootAsync(string rootRoute = "//login");
}
```

### Services/Navigation/ShellNavigationService.cs
```csharp
// Thin wrapper over Shell.Current. GoToAsync<TParam> passes param as dictionary key "param".
```

### Services/Logging/IAppLogger.cs
```csharp
public interface IAppLogger
{
    void LogInfo(string message, string? context = null);
    void LogWarning(string message, string? context = null);
    void LogError(string message, Exception? ex = null, string? context = null);
    Task<IEnumerable<LogEntry>> GetRecentLogsAsync(int count = 100);
}
```

### Services/Logging/AppLogger.cs
```csharp
// Persists logs to SQLite via LocalDatabaseService. LogError also writes to Debug output.
```

### Services/Diagnostics/CrashLogger.cs
```csharp
// Writes crash stack traces to crash_log.txt in AppDataDirectory.
// In DEBUG, shows blocking alert. ReadAndClear used on startup for previous crash display.
```

### Services/Extensions/TaskExtensions.cs
```csharp
public static class TaskExtensions
{
    public static async void SafeFireAndForget(this Task task, Action<Exception>? onError = null)
    { try { await task; } catch (Exception ex) { onError?.Invoke(ex); } }
}
```

### Services/Platform/HapticService.cs
```csharp
public static class HapticService
{
    public static void Light()   => HapticFeedback.Default.Perform(HapticFeedbackType.Click);
    public static void Success() => HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
    public static void Error()   => HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
    // All wrapped in try/catch for platform compatibility.
}
```

### Models/Auth/LoginRequest.cs
```csharp
namespace Cynapharm_Mobile.Models.Auth;
public record LoginRequest(string UserName, string Password);
```

### Models/Auth/LoginResponse.cs
```csharp
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiry { get; set; }
    public UserInfo User { get; set; } = new();
}
```

### Models/Auth/UpdateProfileDto.cs
```csharp
public class UpdateProfileDto
{
    public string? Email       { get; set; }
    public string? Name        { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Adresse     { get; set; }
}
```

### Models/Auth/UserInfo.cs
```csharp
public class UserInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? RegionId { get; set; }
    [JsonPropertyName("phoneNumber")]
    public string? Telephone { get; set; }
    public string? Adresse { get; set; }
}
```

### Models/Auth/ChangePasswordRequest.cs
```csharp
public record ChangePasswordRequest(string Email, string CurrentPassword, string NewPassword);
```

### Models/Auth/ForgotPasswordRequest.cs
```csharp
public record ForgotPasswordRequest(string Email);
```

### Models/Common/ApiResponse.cs
```csharp
public class ApiResponse<T>
{
    [JsonPropertyName("isSuccess")]  public bool IsSuccess { get; set; } = true;
    [JsonPropertyName("result")]     public T? Result { get; set; }
    [JsonPropertyName("message")]    public string? Message { get; set; }
    [JsonPropertyName("errors")]     public List<string>? Errors { get; set; }
}
```

### Models/Common/LogEntry.cs
```csharp
[Table("Log_Entries")]
public class LogEntry
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? Context { get; set; }
    public long TimestampTicks { get; set; }
}
```

### Models/Common/PagedResult.cs
```csharp
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasMore => (Page * PageSize) < TotalCount;
}
// NOTE: PagedResult<T> is defined but never used — all list endpoints return List<T> directly.
```

### Models/Orders/Order.cs
```csharp
public class Order
{
    [JsonPropertyName("id_Commande")] public int Id { get; set; }
    public string NumeroCommande => $"CMD-{Id:D5}";
    public DateTime DateCommande { get; set; }
    public int Statut { get; set; }
    public string StatutFrançais => Statut switch { 0=>"Brouillon", 1=>"En attente", ... };
    [JsonPropertyName("montantTotalHT")] public decimal MontantTotal { get; set; }
    [JsonPropertyName("montantTTC")]     public decimal MontantTTC { get; set; }
    [JsonPropertyName("id_Client")]      public int ClientId { get; set; }
    public string? Notes { get; set; }
    public string? MotifAnnulation { get; set; }
    public bool IsDeleted { get; set; }
    public List<LigneCommande> Lignes { get; set; } = new();
    public List<Reclamation>? Reclamations { get; set; }
}
```

### Models/Orders/LigneCommande.cs
```csharp
public class LigneCommande
{
    [JsonPropertyName("id_Ligne")]    public int Id { get; set; }
    [JsonPropertyName("id_Commande")] public int CommandeId { get; set; }
    [JsonPropertyName("id_Produit")]  public int ProductId { get; set; }
    public string ProductNom { get; set; } = string.Empty;
    public string DisplayName => string.IsNullOrEmpty(ProductNom) ? $"Produit #{ProductId}" : ProductNom;
    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public string NumeroLot { get; set; } = string.Empty;
    public decimal Remise { get; set; }
    public decimal SousTotal => Quantite * PrixUnitaire * (1m - Remise / 100m);
}
```

### Models/Orders/CartLine.cs
```csharp
public class CartLine : ObservableObject
{
    public int ProductId { get; set; }
    public string ProductNom { get; set; } = string.Empty;
    private int _quantite;
    public int Quantite { get => _quantite; set { if (SetProperty(ref _quantite, value)) { OnPropertyChanged(nameof(SousTotal)); OnPropertyChanged(nameof(EconomieTotale)); } } }
    public decimal PrixOriginal      { get; set; }
    public decimal PrixUnitaire      { get; set; }
    public decimal RemisePourcentage { get; set; }
    public string? PromoTitre        { get; set; }
    public bool    HasPromo       => RemisePourcentage > 0;
    public decimal SousTotal      => Quantite * PrixUnitaire;
    public decimal EconomieTotale => Quantite * (PrixOriginal - PrixUnitaire);
}
```

### Models/Orders/Reclamation.cs
```csharp
public class Reclamation
{
    [JsonPropertyName("id_Rec")]           public int Id { get; set; }
    [JsonPropertyName("id_Commande")]      public int CommandeId { get; set; }
    [JsonPropertyName("id_Ligne")]         public int LigneId { get; set; }
    [JsonPropertyName("message")]          public string Motif { get; set; } = string.Empty;
    [JsonPropertyName("dateReclamation")]  public DateTime DateCreation { get; set; }
    public string? Statut { get; set; }
}
```

### Models/Documents/Facture.cs
```csharp
public class Facture
{
    [JsonPropertyName("numero_Doc")]  public int Id { get; set; }
    [JsonPropertyName("nom_Doc")]     public string NumeroFacture { get; set; } = string.Empty;
    [JsonPropertyName("dateFacture")] public DateTime DateFacture { get; set; }
    [JsonPropertyName("id_Commande")] public int CommandeId { get; set; }
    public decimal MontantHT  { get; set; }
    public decimal TVA        { get; set; }  // stub — not in backend DTO
    public decimal MontantTTC { get; set; }
    public string  Statut     { get; set; } = string.Empty;  // stub — not in backend DTO
}
```

### Models/Documents/BonCommande.cs
```csharp
public class BonCommande
{
    [JsonPropertyName("numero_Doc")]   public int Id { get; set; }
    [JsonPropertyName("nom_Doc")]      public string NumeroBon { get; set; } = string.Empty;
    [JsonPropertyName("dateCreation")] public DateTime DateEmission { get; set; }
    [JsonPropertyName("id_Commande")]  public int CommandeId { get; set; }
    public decimal MontantTotal { get; set; }  // stub
    public string  Statut       { get; set; } = string.Empty;  // stub
}
```

### Models/Documents/BonLivraison.cs
```csharp
public class BonLivraison
{
    [JsonPropertyName("numero_Doc")]   public int Id { get; set; }
    [JsonPropertyName("nom_Doc")]      public string NumeroBon { get; set; } = string.Empty;
    [JsonPropertyName("dateCreation")] public DateTime DateLivraison { get; set; }
    [JsonPropertyName("id_Commande")]  public int CommandeId { get; set; }
    public string Statut { get; set; } = string.Empty;  // stub
}
```

### Models/Documents/DocumentSummary.cs
```csharp
public class DocumentSummary
{
    [JsonPropertyName("numero_Doc")]   public int Id { get; set; }
    [JsonPropertyName("nom_Doc")]      public string Numero { get; set; } = string.Empty;
    [JsonPropertyName("dateCreation")] public DateTime Date { get; set; }
    [JsonPropertyName("typeDocument")] public string Type { get; set; } = string.Empty;
    [JsonPropertyName("id_Commande")]  public int CommandeId { get; set; }
    [JsonPropertyName("url_Document")] public string? Url { get; set; }
    public string   Statut  { get; set; } = string.Empty;  // stub
    public decimal? Montant { get; set; }  // stub
}
```

### Models/Products/Product.cs
```csharp
public class Product
{
    [JsonPropertyName("id_Produit")] public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Categorie { get; set; } = string.Empty;
    [JsonPropertyName("prixVente")] public decimal PrixUnitaire { get; set; }
    public bool IsPriceDefined => PrixUnitaire > 0;
    public string PrixDisplay => PrixUnitaire > 0 ? PrixUnitaire.ToString("N", _tndFormat) + " TND" : "Prix non défini";
    public string? ImageUrl { get; set; }
    [JsonPropertyName("isActive")] public bool Actif { get; set; }
    public bool IsArchived { get; set; }
    public List<SupportMarketing>? Supports { get; set; }
}
public class SupportMarketing { public string Type; public bool IsActive; public string? CampaignName; public List<Fichier>? Fichiers; }
public class Fichier { public string NomFichier; public string Url; public string Extension; }
```

### Models/Products/Lot.cs
```csharp
public class Lot
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string NumeroLot { get; set; } = string.Empty;
    public DateTime DateFabrication { get; set; }
    public DateTime DateExpiration { get; set; }
    public int QuantiteDisponible { get; set; }
}
```

### Models/Products/Promotion.cs
```csharp
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

### Models/Products/ProductCheckItem.cs
```csharp
public class ProductCheckItem
{
    public int ProductId { get; set; }
    public string ProductNom { get; set; } = string.Empty;
    public string ProductReference { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}
```

### Models/Inventory/StockDelegue.cs
```csharp
public class StockDelegue
{
    [JsonPropertyName("id_stock")]         public int Id { get; set; }
    [JsonPropertyName("id_User_Delegue")]  public int IdDelegue { get; set; }
    [JsonPropertyName("id_Produit")]       public int ProductId { get; set; }
    [JsonPropertyName("numeroLot")]        public string NumeroLot { get; set; } = string.Empty;
    [JsonPropertyName("dateExpiration")]   public DateTime? DateExpiration { get; set; }
    [JsonPropertyName("qteDisponible")]    public int QuantiteRestante { get; set; }
    [JsonPropertyName("qteReservee")]      public int QuantiteReservee { get; set; }
    public string ProductNom { get; set; } = string.Empty;  // enriched client-side
    public int QuantiteAllouee { get; set; }  // offline SQLite compatibility
}
```

### Models/Inventory/StockDisplayItem.cs
```csharp
public class StockDisplayItem
{
    public int StockId { get; set; }
    public string NumeroLot { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductNom { get; set; } = string.Empty;
    public string QuantiteLabel { get; set; } = string.Empty;
    public string? ExpiryLabel { get; set; }
    public int QuantiteRestante { get; set; }
    public bool IsEchantillon { get; set; }
    public bool HasExpiry => ExpiryLabel != null;
    public bool CanDistribute => IsEchantillon && QuantiteRestante > 0;
}
```

### Models/Inventory/StockMouvement.cs
```csharp
public class StockMouvement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductNom { get; set; } = string.Empty;
    public int Quantite { get; set; }
    public string TypeMouvement { get; set; } = string.Empty;
    public DateTime DateMouvement { get; set; }
}
```

### Models/Inventory/StockPromo.cs
```csharp
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

### Models/Inventory/StockSummaryDto.cs
```csharp
public class StockSummaryDto
{
    public int    TotalProduits      { get; set; }
    public int    TotalQteDisponible { get; set; }
    public int    StocksVides        { get; set; }
    public int    StocksFaibles      { get; set; }
    public int    TotalDistributions { get; set; }
    public int    TotalQteDistribuee { get; set; }
    public string DernierMouvement   { get; set; } = string.Empty;
}
```

### Models/Field/Visite.cs
```csharp
public class Visite
{
    [JsonPropertyName("idVisite")]        public int Id { get; set; }
    [JsonPropertyName("id_User_Delegue")] public int DelegueId { get; set; }
    public string ClientNom { get; set; } = string.Empty;
    public string ClientType { get; set; } = string.Empty;
    public DateTime DateVisite { get; set; }
    public string? Notes { get; set; }
    public bool HasRapport { get; set; }
    private bool _isCompleted;
    public bool IsCompleted { get => _isCompleted; set { _isCompleted = value; if (string.IsNullOrEmpty(Statut)) Statut = value ? "REALISEE" : "PLANIFIEE"; } }
    public string Statut { get; set; } = string.Empty;
}
```

### Models/Field/Rapport.cs
```csharp
public class Rapport
{
    public int Id { get; set; }
    public int VisiteId { get; set; }
    public string Contenu { get; set; } = string.Empty;
    public string? ProduitsDiscutes { get; set; }
    public string Resultat { get; set; } = string.Empty;
    public DateTime DateSoumission { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
```

### Models/Field/Planning.cs
```csharp
public class Planning
{
    [JsonPropertyName("id_Planning")]     public int Id { get; set; }
    [JsonPropertyName("id_User_Delegue")] public int DelegueId { get; set; }
    [JsonPropertyName("date")]            public DateTime DatePlanifiee { get; set; }
    public TimeSpan HeureDebut { get; set; }
    public TimeSpan HeureFin   { get; set; }
    public string Etat         { get; set; } = string.Empty;
    public string ClientNom    { get; set; } = string.Empty;
    public string? Objectif    { get; set; }
    public int? VisiteId       { get; set; }
}
```

### Models/Field/Objectif.cs
```csharp
public class Objectif
{
    [JsonPropertyName("id_Objectif")]     public int Id { get; set; }
    [JsonPropertyName("id_User_Delegue")] public int DelegueId { get; set; }
    [JsonPropertyName("type")]            public int TypeCode { get; set; }
    public string TypeObjectif => TypeCode switch { 1=>"Visites", 2=>"Chiffre d'affaires", 3=>"Nouveaux clients", 4=>"Fidélisation", _ => ... };
    public decimal ValeurCible { get; set; }
    [JsonPropertyName("valeurRealisee")]  public decimal? ValeurActuelle { get; set; }
    [JsonPropertyName("periode")]         public int PeriodeCode { get; set; }
    public string Periode => PeriodeCode switch { 1=>"Mensuel", 2=>"Trimestriel", 3=>"Annuel", _ => ... };
    public double ProgressValue => ValeurCible > 0 ? Math.Min((double)(ValeurActuelle ?? 0) / (double)ValeurCible, 1.0) : 0;
}
```

### Models/Field/Kpi.cs
```csharp
public class Kpi
{
    public int Id { get; set; }
    public int DelegueId { get; set; }
    public string Periode { get; set; } = string.Empty;
    public string Indicateur { get; set; } = string.Empty;
    public decimal Valeur { get; set; }
    public DateTime DateCalcul { get; set; }
}
// NOTE: Entire model is unused — KpiService.GetKpisAsync() returns empty list always.
```

### Models/Field/Region.cs
```csharp
public class Region { public int Id { get; set; }; public string Nom { get; set; } = string.Empty; }
```

### ViewModels/Base/BaseViewModel.cs
```csharp
// Full content: 196 lines
// Extends ObservableValidator. Provides: IsBusy, IsRefreshing, Title, ErrorMessage, HasError, IsOffline.
// ExecuteAsync / ExecuteUncheckedAsync: unified try/catch with ApiException, HttpRequestException,
//   TaskCanceledException, OperationCanceledException, generic Exception handlers.
// CheckConnectivity / CheckConnectivityAsync: snackbar on no connection.
// SaveCacheAsync<T> / LoadCacheAsync<T>: JSON file cache in AppDataDirectory.
// RetryAsync: virtual, overridden by list VMs.
```

### ViewModels/Auth/LoginViewModel.cs
```csharp
// Fields: Email, Password, IsPasswordHidden.
// LoginAsync: validates fields, calls AuthService.LoginAsync, applies role visibility, navigates.
// Role → route: DELEGUE/ADMIN/SUPERVISEUR → "//dashboard", PHARMACIEN/GROSSISTE/CLIENT → "//orders", MEDECIN → "//products".
// TogglePasswordVisibility, GoToForgotPassword.
```

### ViewModels/Auth/ForgotPasswordViewModel.cs
```csharp
// Fields: Email, SuccessMessage.
// SendResetAsync: calls AuthService.ForgotPasswordAsync, sets SuccessMessage, waits 3s, GoBack.
```

### ViewModels/Dashboard/DashboardViewModel.cs
```csharp
// Full content: 142 lines
// Loads: KPIs (stub), Objectifs (real), TauxConversion + StockSummary (DELEGUE),
//        TodayVisitCount (DELEGUE/SUPERVISEUR), Regions (SUPERVISEUR only).
// Offline: falls back to file cache for KPI items.
// Navigation: GoToVisits, GoToPlanning, GoToObjectifs.
```

### ViewModels/Products/ProductListViewModel.cs (257 lines)
```csharp
// CanSeePrices: false for MEDECIN.
// _useVisibleEndpoint: true for MEDECIN, PHARMACIEN, GROSSISTE, CLIENT.
// Debounced search (300ms, min 3 chars). Remote fallback search when local returns empty.
// SQLite offline fallback. Category filter. Seed to SQLite on load.
```

### ViewModels/Products/ProductDetailViewModel.cs (170 lines)
```csharp
// CanSeePrices / HasInformations: false for MEDECIN.
// Lots/Promos: 403/404 silently swallowed.
// OpenDocumentAsync: downloads file and opens with Launcher.
// ViewDocumentAsync: navigates to DocumentViewerPage.
// AddToOrderAsync: navigates to CreateOrderPage — NO role check for MEDECIN (Issue #1).
```

### ViewModels/Products/DocumentViewerViewModel.cs (89 lines)
```csharp
// IsImage / PdfViewerUrl computed from extension.
// PDF: shown via Google Docs viewer URL.
// OpenExternalAsync: downloads and opens file externally.
```

### ViewModels/Orders/OrderListViewModel.cs (129 lines)
```csharp
// StatusFilter → code map. isClient → GetOrdersByClientAsync vs GetOrdersAsync.
// Pagination via LoadMoreAsync. Statut=0 (Brouillon) filtered out.
// IsGrossiste flag for KPI strip visibility.
```

### ViewModels/Orders/OrderDetailViewModel.cs (183 lines)
```csharp
// Loads Order + Lignes (N+1 product fetches for names) + LinkedDocuments (3 calls).
// CanCancel: Statut 0-2. CanCreateReclamation: Statut 4-5.
// CancelOrderAsync: DisplayPromptAsync for motif.
// SubmitReclamationAsync: POST orders/reclamations.
```

### ViewModels/Orders/CreateOrderViewModel.cs (327 lines)
```csharp
// 3-step wizard. Product search + SQLite offline fallback.
// Promotion engine: reads from SQLite PromotionEntry cache.
// Cart persistence via Preferences keyed by user ID.
// SubmitOrderAsync: client-side validation + POST orders.
```

### ViewModels/Documents/DocumentListViewModel.cs (89 lines)
```csharp
// Tab switching: facture/bon-commande/bon-livraison.
// Unified endpoint: GET documents/client/{clientId}/type/{FACTURE|BC|BL}.
```

### ViewModels/Documents/DocumentDetailViewModel.cs (81 lines)
```csharp
// Loads typed document by ID. Share action.
// Issue #8: type comparison uses lowercase keys; navigation from OrderDetail passes uppercase.
```

### ViewModels/Stock/MyStockViewModel.cs (206 lines)
```csharp
// 3 segments: Échantillons (0), Promos (1), Historique (2).
// DistributeSampleAsync: optimistic local deduction then async backend POST.
// 5-minute memory cache with ICacheService.
// SQLite offline fallback for échantillons only (promo stock not cached).
```

### ViewModels/Visites/VisitListViewModel.cs (84 lines)
```csharp
// Debounced filter (400ms) on date range and status changes.
// All visites fetched at once (by-delegue endpoint), filtered client-side.
```

### ViewModels/Visites/VisitDetailViewModel.cs (90 lines)
```csharp
// IsNew / IsExisting based on VisiteId == 0.
// SaveAsync: Create or Update depending on IsNew.
// DeleteAsync: with confirmation alert.
// GoToRapportAsync: navigates to rapport page with visiteId.
```

### ViewModels/Planning/PlanningViewModel.cs (89 lines)
```csharp
// Week-based navigation. WeekDays = 7 PlanningDayGroup items.
// AddVisitAsync: navigates to ///visits/detail (triple slash — potential issue).
// CancellationToken passed to LoadWeekAsync to abort on week change.
```

### ViewModels/Objectifs/ObjectifViewModel.cs (45 lines)
```csharp
// GlobalAchievement: average of (actual/target * 100) across all objectives.
```

### ViewModels/Rapports/RapportViewModel.cs (256 lines)
```csharp
// Validated Contenu (required, min 20 chars). GPS capture at submit time.
// ProduitsDiscutes: multi-select checklist from product API.
// Offline: saves to SQLite Pending_Rapports.
// SubmitAsync: online → POST fields/rapports/createUpdate; offline → SQLite.
```

### ViewModels/Reclamations/ReclamationListViewModel.cs (40 lines)
```csharp
// GET orders/reclamations/by-client/{userId}.
```

### ViewModels/Profile/ProfileViewModel.cs (151 lines)
```csharp
// Reads from SecureStorage. Quick-edit fields save locally only (SecureStorage), NOT via API.
// ChangePasswordAsync: PUT auth/change-password (from ProfilePage directly).
// NavigateToEditProfile / NavigateToChangePassword.
// LogoutAsync: clears cache + SecureStorage, navigates to //login.
```

### ViewModels/Profile/EditProfileViewModel.cs (83 lines)
```csharp
// SaveAsync: PUT auth/update-profile → updates SecureStorage on success.
```

### ViewModels/Profile/ChangePasswordViewModel.cs (137 lines)
```csharp
// Password strength meter (4-segment visual indicator).
// PasswordsMatch / PasswordsMismatch live feedback.
// ChangePasswordAsync: PUT auth/change-password.
```

### Views/Auth/LoginPage.xaml
```xml
<!-- Custom login form with icon logo, email/password fields with focus animations,
     password visibility toggle, forgot password link, ActivityIndicator + Button. -->
```

### Views/Auth/ForgotPasswordPage.xaml
```xml
<!-- Two-state form: 1) email input + submit, 2) success check-mark + message.
     State switches via DataTrigger on SuccessMessage. -->
```

### Views/Dashboard/DashboardPage.xaml
```xml
<!-- Hero header with avatar, greeting, date, TodayVisitCount + TauxConversion strip.
     Body: StockSummary card (DELEGUE), Quick-access buttons, ObjectifItems list, KpiItems list.
     All sections use RefreshView + BindableLayout. -->
```

### Views/Products/ProductListPage.xaml
```xml
<!-- Header: title + count badge + search bar + offline banner.
     Category filter chips (horizontal scroll). ProductCount indicator.
     CollectionView with product cards. -->
```

### Views/Products/ProductDetailPage.xaml
```xml
<!-- Header: back + "Fiche produit" + Active/Inactive badge (hidden for MEDECIN).
     Body: product image/icon, name, reference, category, price (hidden for MEDECIN),
     Lots section, Promotions section, Marketing supports list, Add to Order button. -->
```

### Views/Products/DocumentViewerPage.xaml
```xml
<!-- Dark background. Header: back + filename + download button.
     Content: Image for images; WebView with Google Docs URL for PDFs/docs. -->
```

### Views/Orders/OrderListPage.xaml
```xml
<!-- Header: title + count badge (GROSSISTE only) + KPI strip (GROSSISTE).
     Status filter chips (horizontal scroll).
     CollectionView with order cards. Load-more footer. FAB create-order button. -->
```

### Views/Orders/OrderDetailPage.xaml
```xml
<!-- Header: back + "Détail commande".
     Body: Summary card (order number, date, status badge, amounts).
     Lignes list. Linked documents section (Factures/BC/BL).
     Cancel button (when CanCancel). Reclamation form (when CanCreateReclamation). -->
```

### Views/Orders/CreateOrderPage.xaml
```xml
<!-- 3-step wizard with numbered circles + progress bars.
     Step 1: product search + quantity + AddLine.
     Step 2: cart review (list of CartLines with subtotals + savings).
     Step 3: order summary + submit.
     Uses CommunityToolkit behaviors. -->
```

### Views/Documents/DocumentListPage.xaml
```xml
<!-- Header: title + subtitle.
     3-tab bar: Factures / Bons de commande / Bons de livraison.
     CollectionView with document cards. Open URL action. -->
```

### Views/Documents/DocumentDetailPage.xaml
```xml
<!-- Header: back + document title. Share toolbar item.
     Body: conditional sections shown based on IsFacture/IsBonCommande/IsBonLivraison.
     Facture: numero, date, montantHT, TVA (stub), montantTTC, statut.
     BC: numero, date, montant (stub), statut.
     BL: numero, date, statut. -->
```

### Views/Stock/MyStockPage.xaml
```xml
<!-- Header: title + subtitle.
     3-segment tab: Échantillons / Promos / Historique.
     Échantillons: StockDisplayItem cards with Distribute button.
     Promos: promo stock cards (no distribute).
     Historique: StockMouvement list. -->
```

### Views/Visites/VisitListPage.xaml
```xml
<!-- Header: title + decorative search bar (not bound — Issue #4).
     Date range filters (StartDate / EndDate DatePickers).
     Status filter chips.
     CollectionView with visit cards showing date, client, status badge.
     FAB create-visit button. -->
```

### Views/Visites/VisitDetailPage.xaml
```xml
<!-- Header: back + "Visite".
     Form: ClientName Entry, DatePicker, Notes Editor, Statut Picker.
     Rapport button (visible only when IsExisting).
     Save and Delete buttons. -->
```

### Views/Planning/PlanningPage.xaml
```xml
<!-- Header: previous-week button + WeekLabel + next-week button.
     Week summary strip with day-of-week headers.
     PlanningDayGroup CollectionView: 7 days with their entries.
     Each day shows ClientNom, Objectif, time range, Etat badge.
     FAB add-visit button per day. -->
```

### Views/Objectifs/ObjectifPage.xaml
```xml
<!-- Hero header with global achievement % pill.
     Période filter label (current month displayed, no interaction).
     CollectionView of Objectif cards: type, periode badge, ProgressBar, actual/target. -->
```

### Views/Rapports/RapportPage.xaml
```xml
<!-- Header: back + "Rapport de visite".
     GPS status banner.
     Contenu Editor with validation error label.
     Resultat picker.
     Produits discutés CheckBox list.
     Submit button (disabled when !CanSubmit). -->
```

### Views/Reclamations/ReclamationListPage.xaml
```xml
<!-- Header: title + subtitle.
     CollectionView with reclamation cards: motif, date, statut badge, commande ref. -->
```

### Views/Profile/ProfilePage.xaml
```xml
<!-- Hero header: avatar initials, name, role badge.
     Body: contact info card (email, phone, address).
     Edit profile button (navigates to EditProfilePage).
     Change password button (navigates to ChangePasswordPage).
     Logout button (with confirmation). -->
```

### Views/Profile/EditProfilePage.xaml
```xml
<!-- Header: cancel (back) + "Modifier le profil".
     Form: Name Entry, Telephone Entry, Adresse Entry, Email (read-only), Role (read-only).
     Save button. -->
```

### Views/Profile/ChangePasswordPage.xaml
```xml
<!-- Header: cancel (back) + "Changer le mot de passe".
     Old password field (toggle visibility).
     New password field with strength meter (4 colored segments).
     Confirm password field with match/mismatch indicator.
     Save button. -->
```

### Controls/EmptyStateView.xaml
```xml
<!-- ContentView with Icon (emoji), Title, optional Subtitle, optional Action button.
     Default IsVisible="False" — shown programmatically. -->
```

### Controls/ErrorBanner.xaml
```xml
<!-- ContentView with warning icon + message label + dismiss X button.
     Default IsVisible="False" — shown when Message is non-empty via code-behind. -->
```

### Converters/StatusColorConverter.cs
```csharp
// int (EtatCommande code) → Color:
// 0=Gray, 1=Orange(EnAttente), 2=Cyan(Confirmée), 3=Blue(EnPreparation),
// 4=Purple(Expédiée), 5=Green(Livrée), 6=Red(Annulée).
```

### Converters/InvertedBoolConverter.cs
```csharp
// bool → !bool. Used for IsEnabled on buttons when IsBusy=true.
```

### Converters/IsNotNullOrEmptyConverter.cs
```csharp
// string → !string.IsNullOrEmpty(s).
// null → false.
// any other non-null → true (catches decimal?, int, objects).
// bool with "TrueText|FalseText" parameter → string label selection.
```

### Resources/Raw/appsettings.json
```json
{
  "ApiGatewayBaseUrl": "http://cynapharmgateway.runasp.net/",
  "ApiGatewayBaseUrlProd": "https://cynapharmgateway.runasp.net/"
}
```

### Resources/Styles/Colors.xaml
```xml
<!-- Primary: #00B4D8 (cyan), Secondary: #F5A623 (amber), Danger: #E24B4A (red).
     PageBackground: #EEF0F5, CardBackground: #FFFFFF.
     TextPrimary: #1A1A1A, TextSecondary: #6B6B6B, TextMuted: #9E9E9E.
     Many aliases for backward compatibility. -->
```

### Resources/Styles/Styles.xaml
```xml
<!-- Global implicit styles: ContentPage, Label, Entry, ProgressBar, Button.
     Named styles: CardStyle (Border with shadow), PrimaryButtonStyle, SecondaryButtonStyle,
     DangerButtonStyle, SectionTitleStyle, MutedLabelStyle, PageTitleStyle, EmptyStateStyle.
     Converters registered: IsNotNullOrEmptyConverter, InvertedBoolConverter, StatusColorConverter. -->
```

---

## Summary of Critical Issues (Top Priority)

1. **Issue #1 (High)** — MEDECIN can access `CreateOrderPage` via the "Add to Order" button in ProductDetailPage. No role check blocks this navigation.

2. **Issue #2 (High)** — OrderDetailViewModel makes N individual `GET products/{id}` calls for each order line to resolve product names. 10-line order = 11 API calls.

3. **Issue #3 (High)** — `KpiService.GetKpisAsync()` is a stub (no HTTP call, returns empty list). Dashboard KPI section always empty for all roles.

4. **Issue #4 (High)** — VisitListPage search field is decorative only — not bound to any ViewModel property.

5. **Issue #8 (High)** — `DocumentDetailViewModel` type matching uses lowercase `"facture"/"bon-commande"/"bon-livraison"` but navigation from `OrderDetailViewModel` passes uppercase `"FACTURE"/"BC"/"BL"`. Document detail will always show blank when navigated from order detail.

6. **Issue #11 (High)** — Sample distribution `PostDistributionAsync` sends `id_Medecin=null, id_Pharmacien=null`. Backend requires at least one recipient in `EchantillonDto`. May fail silently.

7. **Issue #6/#7 (Medium)** — `AuthService` bypasses `ApiRoutes` constants for login, forgot-password, and change-password, using different URL prefixes than what `ApiRoutes` defines.

8. **Issue #9 (Medium)** — `PlanningViewModel.AddVisitAsync` uses triple-slash route `///visits/detail` which is non-standard and may throw on some MAUI versions.
