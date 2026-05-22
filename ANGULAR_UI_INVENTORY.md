# Angular UI Inventory — Cynapharm CRM Frontend

Comprehensive visual + functional inventory of every page and component in
the Angular application located at `Cynapharm/src/app`.

For every page: route URL, role guard, top-level layout, all visible
fields (with their backing property), every action/button, the API
service call it triggers, and visible business-logic conditions
(`*ngIf` / `[disabled]` / `@if`).

App-level routes are declared in [`app.routes.ts`](Cynapharm/src/app/app.routes.ts).
Most modules use Angular 17+ control-flow blocks (`@if` / `@for`).

---

## Table of Contents
1. [AUTH](#1-auth)
2. [DASHBOARD](#2-dashboard)
3. [PRODUCTS](#3-products)
4. [LOTS](#4-lots)
5. [PROMOTIONS](#5-promotions)
6. [MARKETING](#6-marketing)
7. [USERS](#7-users)
8. [ORDERS](#8-orders)
9. [DOCUMENTS](#9-documents)
10. [INVENTORY](#10-inventory)
11. [FIELD](#11-field)
12. [Global navigation guards & role matrix](#12-global-navigation-guards--role-matrix)

---

## 1. AUTH

### 1.1 Login page
- **Component:** `LoginComponent`
  ([login.component.ts](Cynapharm/src/app/features/auth/login/login.component.ts),
  [login.component.html](Cynapharm/src/app/features/auth/login/login.component.html))
- **Route:** `/login` — public (no guard)
- **Layout:** two-panel card. Left panel = branding (CynaPharm SVG logo,
  brand title *"L'excellence au service de la santé."*, three feature
  highlights: *Gestion des Délégués*, *Suivi des Performances*,
  *Catalogue & Stocks*). Right panel = login form.

**Form fields** (reactive `loginForm`)

| # | Label | Form control | Validators | Notes |
|---|---|---|---|---|
| 1 | Adresse email  | `email`    | `required`, `email`     | autocomplete=email, placeholder *votre.email@cynapharm.com* |
| 2 | Mot de passe   | `password` | `required`, `minLength(6)` | toggleable visibility (`showPassword`) |

Below the form: Cloudflare Turnstile widget (`<app-turnstile>`), error
alerts (`turnstileError`, `error`).

**Buttons / links**

| Trigger | Action | API call |
|---|---|---|
| **Se connecter** (submit) | `onLogin()` — POST credentials + turnstileToken | `AuthService.login()` → `POST /auth/login` |
| **Mot de passe oublié ?** (`routerLink`) | navigate to `/forgot-password` | — |
| **Toggle password** (`button`) | flips `showPassword` | — |

**Business logic / disabled rules**
- Submit disabled when `loginForm.invalid || !isTurnstileValid || loading`.
- On success: redirect via `getRedirectByRole()` —
  `ADMIN→/users`, `SUPERVISEUR|DELEGUE|MEDECIN→/dashboard`,
  `CLIENT→/home`, fallback `/dashboard`.
- On `ngOnInit`, if already authenticated, redirect by role.

---

### 1.2 Forgot password page
- **Component:** `ForgotPasswordComponent`
  ([forgot-password.component.ts](Cynapharm/src/app/features/auth/forgot-password/forgot-password.component.ts),
  [forgot-password.component.html](Cynapharm/src/app/features/auth/forgot-password/forgot-password.component.html))
- **Route:** `/forgot-password` — public
- **Layout:** single auth card with brand logo, heading
  *"Mot de passe oublié"* and explanation.

**Form fields**

| Label | Form control | Validators |
|---|---|---|
| Adresse email | `email` | `required`, `email` |

**Buttons**

| Trigger | Action | API call |
|---|---|---|
| **Envoyer le lien** (submit) | `onSubmit()` | `AuthService.forgotPassword(email)` → `POST /auth/forgot-password` |
| **Retour à la connexion** (`routerLink`) | go `/login` | — |
| **Retour à la connexion** (success box) | `goToLogin()` | — |

**Conditional UI**
- `*ngIf="success"` → renders a success card with `message` and
  *"Email envoyé !"* and hides the form.
- Submit disabled when `forgotForm.invalid || loading`.
- Inline error alert when `error` is set.

> Note: companion pages `RegisterComponent` (`/register`) and
> `ResetPasswordComponent` (`/reset-password`) also exist but were not
> requested in this inventory.

---

## 2. DASHBOARD

### 2.1 Main dashboard
- **Component:** `DashboardComponent`
  ([dashboard.component.ts](Cynapharm/src/app/features/dashboard/dashboard.component.ts),
  [dashboard.component.html](Cynapharm/src/app/features/dashboard/dashboard.component.html))
- **Route:** `/dashboard` — `authGuard`
- **Layout:** header with title + subtitle + refresh button, four KPI
  cards row, optional `orderDash` stats grid, two-chart row, full-width
  volume chart.

**KPI cards (top row)**

| Card | Backing property | Source |
|---|---|---|
| Commandes aujourd'hui | `commandesAujourdhui` | `OrderApiService.computeStats().countToday` |
| Commandes en attente  | `commandesEnAttente`  | `stats.countEnAttente` |
| Chiffre d'affaires total (TND) | `caTotal` (via `currencyTND` pipe) | `stats.totalCA` |
| Taux de livraison %   | `tauxLivraison` (+ progress bar `kpi-bar`) | `(countLivrees / (totalOrders − countAnnulees)) × 100` |

**Order dashboard stats grid** (only `@if (orderDash)`):
Total, En attente, Confirmées, En préparation, Expédiées, Livrées,
Annulées (red), Total HT (TND), Total TTC (TND), Réclam. ouvertes,
Réclam. en cours, Aujourd'hui, Ce mois — each maps directly to
`orderDash.<field>`.

**Charts** (ApexCharts via `ng-apexcharts`)
1. Horizontal bar — *Commandes par statut* (`statutBarSeries`).
2. Donut — *Commandes par statut* (`statutChartSeries` /
   `statutChartLabels`).
3. Area — *Volume de commandes — 7 derniers jours* (`volumeChartSeries`
   over `last7Days`).

**Buttons / API calls**

| Trigger | Action | API call |
|---|---|---|
| **Actualiser** (refresh) | `reload()` → `loadAll()` | `OrderApiService.getAllOrders()` and `getOrdersDashboard()` |

**Conditional UI**
- Refresh button has `[class.spinning]="loading"` and is `[disabled]="loading"`.
- `*ngIf="loading"` → skeleton grid (`skeleton-card × 4`).
- `*ngIf="error && !loading"` → red error banner.
- Each chart has an empty-state placeholder when its series is empty.

---

## 3. PRODUCTS

Routes ([products-routing.module.ts](Cynapharm/src/app/features/products/products-routing.module.ts)):
- `/products` → `ProductListComponent`
- `/products/new` → `ProductFormComponent`
- `/products/:id` → `ProductDetailComponent`
- `/products/:id/edit` → `ProductFormComponent`

Module-level guard: `authGuard` (declared in app.routes).

### 3.1 Product list
- **Component:** `ProductListComponent`
  ([product-list.component.ts](Cynapharm/src/app/features/products/product-list/product-list.component.ts),
  [product-list.component.html](Cynapharm/src/app/features/products/product-list/product-list.component.html))

**Header**
- Title *"Gestion des produits"* + `{{ totalProducts }} produit(s) au total`.
- Button **+ Nouveau produit** → `routerLink="/products/new"`.

**KPI cards (click-to-filter)**

| Card | Value | Filter applied (`statusFilter`) |
|---|---|---|
| Total    | `totalProducts`    | `all`      |
| Actifs   | `totalActive`      | `active`   |
| Inactifs | `totalInactive`    | `inactive` |
| Archivés | `totalArchived`    | `archived` |

Active card is highlighted via `kpi-active`.

**Filters bar**
- Search input bound to `searchTerm` (min 3 chars, hint shown < 3) +
  clear button.
- Select **Filtre statut** (`statusFilter`): `all|active|inactive|archived`.
- Select **Filtre catégorie** (`categoryFilter`): dynamic from
  `availableCategories`.
- Select **page size** (`pageSize`): 5 / 10 / 20 / 50.
- Live counter — `{{ filteredProducts.length }} résultat(s)`.

**Table columns**

| Column | Source |
|---|---|
| Nom         | `product.Nom` |
| Catégorie   | `product.Categorie` |
| Description | `product.Description` |
| Prix vente  | `product.Prix_Vente` (via `currencyTND`) |
| TVA         | `product.TVA` (% suffix) |
| Statut      | `badge` class via `IsActive`/`IsArchived` |
| Actions     | see below |

**Per-row actions** (`actions-cell`)

| Button | Visible when | Click handler | Notes |
|---|---|---|---|
| 👁 Voir | always | `onView(id)` → `/products/:id` | |
| ✏ Modifier | `!product.IsArchived` | `onEdit(id)` → `/products/:id/edit` | |
| 📦 Archiver | `!product.IsArchived` | `onArchive(id)` → opens confirm modal `archive` | |
| ⛔ Désactiver | `IsActive && !IsArchived` | `onDelete(id)` → confirm `deactivate` | calls `productService.deleteProduct(id)` (soft delete) |
| ✅ Activer | `!IsActive && !IsArchived` | `onActivate(id)` → confirm `activate` | |
| ✅ Désarchiver | `IsArchived` | `onUnarchive(id)` → confirm `unarchive` | |
| 🗑 Supprimer définitivement | `IsArchived` | `onHardDelete(id)` → confirm `harddelete` | irreversible; requires archived + zero stock |

**Row detail (expand)**: clicking a row toggles `expandedRows`. Loads
lots via `lotService.getLotsByProductId(id)`. Inner mini-table columns:
`Lot #`, `Qty`, `Expiry`, `Status`, view link → `/lots/:numero`.

**Confirmation modal** (`showConfirmModal`)
- Buttons: **Annuler** (`cancelAction`) / **Confirmer**
  (`confirmAction_execute`) — dispatches by `confirmAction` to one of
  `deleteProduct`, `archiveProduct`, `unarchiveProduct`,
  `activateProduct`, `hardDeleteProduct`.

**Pagination**: `Précédent` / numbered pages / `Suivant`.

---

### 3.2 Product detail
- **Component:** `ProductDetailComponent`
  ([product-detail.component.html](Cynapharm/src/app/features/products/product-detail/product-detail.component.html))
- **Route:** `/products/:id`

**Header**
- ← *Produits* back button.
- `<h1>{{ product?.Nom || 'Détail produit' }}</h1>`
- Status badge (`getStatusClass`).
- **Modifier** — disabled when `!canEditProduct()`; tooltip *"Produit
  archivé — non modifiable"* when archived.
- Contextual actions (only when `!isProductArchived()`):
  - **Désactiver** (`onDeactivate`) when `isProductActive()`.
  - **Activer** (`onActivate`) otherwise.
  - **Archiver** (`onArchive`).

**Tabs nav** (`tabs[]` — each shows `tab.label` + optional `tab.count()`):
`info`, `stock`, `lots`, `supports`, `promotions`, `dashboard`.

#### Tab: Informations (`activeTab === 'info'`)

Info grid fields:

| Label | Source |
|---|---|
| Nom               | `product.Nom` |
| ID                | `product.Id_Produit` |
| Description (full row) | `product.Description` |
| Catégorie         | `product.Categorie` |
| Prix de vente     | `product.Prix_Vente` (currencyTND) |
| Prix de création  | `product.Prix_Creation` (currencyTND) |
| TVA               | `product.TVA` (%) |
| Statut            | status badge |

Quick-action cards: **Gérer les lots** (`goToLots()` → `/lots?productId=`),
**Supports** (switches to `supports` tab).

#### Tab: Stock (InventoryAPI temps réel)
KPI row: *Disponible (InventoryAPI)* (`stockDisponible`), *Total lots
(ProductAPI)* (`stock`), *Lignes stock délégué*
(`inventoryStocks.length`).

Conditional banners:
- `loadingInventory` → spinner row.
- `stockDisponible === 0 && inventoryStocks.length > 0` → warning
  *"Toutes les lignes de stock sont épuisées."*
- `inventoryStocks.length === 0` → empty state.

Inventory table columns: `N° Lot`, `Délégué (ID)`, `Qté disponible`
(red if 0), `Qté réservée`, `Expiration`, `Statut` (`rupture` if 0,
`faible` if ≤5, else `ok`).

Action link **+ Nouveau lot** → `/lots/new?productId=…`.

#### Tab: Lots
Toolbar: `{{ lots.length }} lot(s) associé(s)` + **+ Nouveau lot** link.
Empty state when `lots.length === 0`.
Each lot row: number, quantité, exp date, expiration warning (if any),
status badge via `(lot | lotStatus)` pipe.

#### Tab: Supports
- **Product image card** (Cloudinary upload, only `canManageMarketing`):
  Add / Change / Delete image via hidden file input `imgInput`.
  Errors via `imageUploadError`, progress via `imageUploading`.
- **Supports list card**:
  - Toolbar: `{{ supports.length }} support(s) marketing` +
    **+ Nouveau support** (open modal `openCreateSupport()`).
  - Each `support-card`:
    - Type badge, campaign name, active/inactive badge,
      expand-files button (count badge), edit, toggle active/inactive.
    - Expanded section lists files: name (link, broken-state warning),
      extension, size; **+ Ajouter un fichier** (file input)
      and delete file button — both gated by `canManageMarketing`.
- **Support modal** (`showSupportModal`) — form fields:
  - **Type de support** `select` (required, from `supportTypes`).
  - **Nom de la campagne** `text` (required).
  - **Support actif** `checkbox`.
  - Footer: Annuler / Créer (or Mettre à jour).

#### Tab: Promotions
- Toolbar: count + (if `canManageMarketing`) **+ Nouvelle promotion**
  → `/promotions/new`.
- Promo cards: code badge, status chip (Active / Inactive / Expirée
  via `isPromoExpired()`), reduction (`-{{ pourcentage }}%`), N° lot,
  period. Actions: **Voir** / **Modifier** → `/promotions/:id[/edit]`.

#### Tab: Dashboard
Metric cards: Stock total, Statut, Lots actifs, Promotions.

---

### 3.3 Product form
- **Component:** `ProductFormComponent`
  ([product-form.component.ts](Cynapharm/src/app/features/products/product-form/product-form.component.ts),
  [product-form.component.html](Cynapharm/src/app/features/products/product-form/product-form.component.html))
- **Route:** `/products/new` and `/products/:id/edit`.

**Header**
- Back button to `/products`, breadcrumb separator `/`, title
  *"{{ isEditMode ? 'Modifier le produit' : 'Ajouter un produit' }}"*.

**Card header**
- Card title *"Informations du produit"* + mode badge
  *Modification* / *Nouveau*.

**Form fields**

| Field | Control name | Type / source | Validators |
|---|---|---|---|
| Nom du produit *           | `Nom`           | text, max 200 | required, maxLength 200 |
| Description                | `Description`   | textarea (4 rows) | maxLength 1000 |
| Catégorie *                | `Categorie`     | `<select>` from `categories[]` + special `__new__` option that reveals a text input | required |
| Prix de vente *            | `Prix_Vente`    | text decimal (TND prefix) — sanitized via `formatDecimal()` | required, min 0 |
| Prix de création *         | `Prix_Creation` | text decimal (TND prefix) | required, min 0 |
| TVA (%)                    | `TVA`           | number 0–100 (suffix `%`) | required, min 0, max 100 (default 19) |
| Produit actif (toggle)     | `isActive`      | checkbox (toggle switch) | — default true |

**Alerts**
- `@if (error)` red danger alert.
- `@if (success)` green success alert with verb depending on
  `isEditMode`.

**Buttons**

| Trigger | Action |
|---|---|
| **Annuler** | `routerLink="/products"` |
| **Créer le produit / Mettre à jour** (submit) | `onSubmit()` → `productService.createProduct(dto)` or `updateProduct(id, dto)`; toast + redirect to `/products` |

Submit disabled when `!productForm.valid || loading`.

In edit mode: pre-loads via `productService.getProductById(id)`;
keeps `loadedIsArchived` and resyncs the category select between
`__new__` and existing entries.

---

## 4. LOTS

Routes ([lots-routing.module.ts](Cynapharm/src/app/features/lots/lots-routing.module.ts)) —
all need `authGuard + roleGuard`:
- `/lots` → `LotListComponent` (ADMIN, SUPERVISEUR, DELEGUE)
- `/lots/new` → `LotFormComponent` (ADMIN, SUPERVISEUR)
- `/lots/:numero` → `LotDetailComponent`
- `/lots/:numero/edit` → `LotFormComponent` (ADMIN, SUPERVISEUR)

### 4.1 Lot list
- **Component:** `LotListComponent`
  ([lot-list.component.html](Cynapharm/src/app/features/lots/lot-list/lot-list.component.html))

Header title is conditional: *Lots de {{ productName }}* if
`productId` query param is present, otherwise *Gestion des lots*.
Sub: `{{ totalLots }} lot(s) au total`. Button **+ Ajouter un lot**
→ `/lots/new?productId=`.

**KPI cards (clickable filters)**

| Card | Backing | `statusFilter` |
|---|---|---|
| Total        | `totalLots`     | `all` |
| Actifs       | `totalActive`   | `active` |
| Stock faible | `totalLowStock` | `low-stock` |
| Expirés      | `totalExpired`  | `expired` |

If `productId` is set → **← Retour au produit** link to `/products/:id`.

**Filters bar**
- Search input (min 2 chars) for lot number.
- `statusFilter` select: `all|active|low-stock|expired|out-of-stock`.
- Result count.

**Table columns** (driven by `columns[]` + `numero` + `produit`):
`N° Lot`, `Produit` (`getProductName(idProduit)`), plus other columns
(quantité, date d'expiration, statut), `Actions`.

Status column uses `<span class="badge {{ getStatusClass(lot) }}">` and
`getStatusText(lot)` from `lot.model.ts` enum
(`active|low-stock|out-of-stock|expired`).

**Per-row actions**: 👁 Voir (`onView`), ✏ Modifier (`onEdit`),
🗑 Supprimer (`onDelete` → opens `showDeleteModal`). Delete modal has
**Annuler** / **Supprimer** (`confirmDelete()`).

States: loading spinner, empty state ("Aucun lot trouvé" + CTA), red
error alert.

---

### 4.2 Lot detail
- **Component:** `LotDetailComponent`
  ([lot-detail.component.html](Cynapharm/src/app/features/lots/lot-detail/lot-detail.component.html),
  [lot-detail.component.ts](Cynapharm/src/app/features/lots/lot-detail/lot-detail.component.ts))
- **Route:** `/lots/:numero`

**Header** (`page-header`): `<h1>Lot <span class="lot-num">{{ lot.numero }}</span></h1>`
+ status badge. Right side `header-actions` (only when lot loaded):
- **Modifier** — `onEdit()` (disabled when `isEditDisabled()` i.e.
  `lot.isExpired`, tooltip *"Impossible de modifier un lot expiré."*).
- **Supprimer** — `onDelete()` (disabled when `isDeleteDisabled()` i.e.
  `inventoryStock.qteDisponible > 0`, tooltip *"Impossible de supprimer
  un lot avec du stock disponible."*).

**Loading / error**: spinner row `state-loading`, danger alert when
`error`.

**Info card — Informations du lot**

| Detail label | Source |
|---|---|
| N° de lot          | `lot.numero` |
| Quantité           | `lot.quantite` |
| Date d'expiration  | `lot.dateExpiration` (dd/MM/yyyy) |
| Produit            | `productName` + `#{{ lot.idProduit }}` muted |
| Expiré             | flag (`flag-true`/`flag-false`) |
| Rupture de stock   | flag |
| Promotion          | `getFormattedPromotion()` if any, otherwise *"Aucune promotion"* + helper line *"Cliquez sur « Ajouter une promotion »…"* |

**Inv section — Stock InventoryAPI (temps réel)**
KPI cards (`inv-cards`): `Qté disponible` (red if 0), `Qté réservée`,
`Délégué` (`delegueName` + `#id`), `Statut stock` (badge `ok` /
`faible` / `rupture`). States: `inv-loading`, `inv-empty`.

**Action buttons row**
- **← Retour à la liste** (`onBackToList`).
- `<app-button label="Modifier">` (disabled if expired, tooltip wrap).
- `<app-button label="Supprimer">` (disabled if qteDisponible > 0).
- **+ Ajouter une promotion** (`canManagePromo`, opens
  `showPromoModal`).

**Modals**
- *Confirmer la suppression* → **Annuler** / **Supprimer**
  (`confirmDelete()` → `lotService.deleteLot()`).
- *Ajouter une promotion* — `promoForm` fields:
  - `codePromo` text *
  - `dateDebut` date *
  - `dateExpiration` date *
  - `pourcentage` number 1–100 *
  - `estActive` checkbox
  - Submit calls `promotionAdvancedService.createOrUpdatePromotion()`.

---

### 4.3 Lot form
- **Component:** `LotFormComponent`
  ([lot-form.component.html](Cynapharm/src/app/features/lots/lot-form/lot-form.component.html))
- **Route:** `/lots/new` & `/lots/:numero/edit`

Header: `<h1>{{ isEditMode ? 'Modifier le lot' : 'Ajouter un lot' }}</h1>`,
optional `<span class="lot-badge">{{ numeroLot }}</span>`.

**Form fields**

| Field | Control | Mode | Validation |
|---|---|---|---|
| Numéro du lot *  | `numero` | text — read-only in edit (clé métier) | required, 3–50, pattern alphanumeric / `-` / `_` |
| Produit *        | `idProduit` (select) | options from `products[]` loaded async; pre-selectable via `productId` query param | required |
| Date d'expiration * | `dateExpiration` (date) | shows original date hint in edit mode | required, `pastDate` (must be ≥ today) |
| Quantité *       | `quantite` (number) | min 1, max 999 999 | required, min 1, max 999999 |

Info note (non-editable): "Le statut (expiré, rupture de stock) est
calculé automatiquement par le backend…"

**Actions**
- **Annuler** (`onCancel`) — back to list.
- `<app-button label="Mettre à jour / Créer le lot">` — disabled when
  `lotForm.invalid || loading`.

Feedback: red `feedback-error` / green `feedback-success`.

---

## 5. PROMOTIONS

Routes ([promotions-routing.module.ts](Cynapharm/src/app/features/promotions/promotions-routing.module.ts)) — app guard `[authGuard, roleGuard]` with
`roles: [ADMIN, SUPERVISEUR]`.
- `/promotions` → `PromotionListComponent`
- `/promotions/new` → `PromotionFormComponent`
- `/promotions/analytics` → `PromotionAnalyticsComponent`
- `/promotions/:id` → `PromotionDetailComponent`
- `/promotions/:id/edit` → `PromotionFormComponent`

### 5.1 Promotion list
- **Component:** `PromotionListComponent`
  ([promotion-list.component.html](Cynapharm/src/app/features/promotions/promotion-list/promotion-list.component.html))

Header: title *"Promotions"* + `{{ promotions.length }} promotion(s)`.
Header actions: **📊 Analytics** (`/promotions/analytics`),
**+ Nouvelle promotion** (`/promotions/new`).

States: `error` red banner, `loading` block, empty state via
`<app-empty-state>`.

**Table columns**: Code, Type, Benefit, Product, Scope, Start, Expiry,
Status, Actions.

| Column | Backing |
|---|---|
| Code     | `p.codePromo` (mono) |
| Type     | `-{{ p.pourcentage }}%` (text-danger) |
| Benefit  | `p.numeroLot` |
| Product  | (col header says Product but cell shows `dateDebut`) |
| Start    | `p.dateDebut` |
| Expiry   | `p.dateExpiration` |
| Status   | chip `Active` / `Expirée` / `Inactive` from `p.isValid` + `isExpired()` |
| Actions  | 👁 (`/promotions/:id`), ✏ (`/promotions/:id/edit`), 🗑 (`openDelete(p)`) |

`<app-confirm-dialog>` is shown when `showConfirm`; **Confirmer**
triggers `confirmDelete()` → `svc.delete(id)`.

> The list filters promotions where `pourcentage > 0` only.

---

### 5.2 Promotion form
- **Component:** `PromotionFormComponent`
  ([promotion-form.component.ts](Cynapharm/src/app/features/promotions/promotion-form/promotion-form.component.ts),
  [promotion-form.component.html](Cynapharm/src/app/features/promotions/promotion-form/promotion-form.component.html))

Header: ← *Promotions* + title *"{{ isEdit ? 'Modifier' : 'Nouvelle' }} promotion"*.

**Form fields**

| Field | Control | Validators |
|---|---|---|
| Code promo *           | `codePromo`      | required, maxLength 50, `noWhitespaceValidator` |
| Réduction (%) *        | `pourcentage`    | required, min 1, max 100 |
| N° de lot *            | `numeroLot` (select) — options from `availableLots` = `getAllLots()` filtered to non-expired & in-stock | required |
| Date de début *        | `dateDebut`      | required |
| Date d'expiration *    | `dateExpiration` | required + cross-field `dateRangeValidator` |
| Promotion active       | `estActive` (checkbox) | — |

Errors render conditional `.err` spans. Form-level `dateRange` error
shown for `dateExpiration`.

Buttons: **Annuler** (`/promotions`) / **Créer** or **Mettre à jour**
(submit) — calls `svc.createOrUpdate(dto)`, then toast + redirect.

---

### 5.3 Promotion detail
- **Component:** `PromotionDetailComponent`
  ([promotion-detail.component.html](Cynapharm/src/app/features/promotions/promotion-detail/promotion-detail.component.html))

← *Promotions* back + `<h1>Promotion — {{ promo?.codePromo }}</h1>`.

Detail rows: Code (mono), Type (badge — 🎁 gift or % percentage),
Discount (`-{{ pourcentage }}%`) OR Rule
(*Buy X get Y free*), Scope (all lots / single `numeroLot`), Start,
Expiry, Status chip (`Active` / `Inactive`).

Actions footer: **← Back** (`/promotions`) and **Edit**
(`/promotions/:id/edit`).

---

### 5.4 Promotion analytics
- **Component:** `PromotionAnalyticsComponent`
  ([promotion-analytics.component.html](Cynapharm/src/app/features/promotions/promotion-analytics/promotion-analytics.component.html))

← *Promotions* back + `<h1>Analytics promotions</h1>`.

KPI row:
- *Promotions actives* (`activeCount`)
- *Taux de couverture* — `coverageRate | number:'1.0-1'` `%`.

No other interactive controls.

---

## 6. MARKETING

Routes ([marketing-routing.module.ts](Cynapharm/src/app/features/marketing/marketing-routing.module.ts)) — `[authGuard, roleGuard]`:
- `/marketing/supports` → `SupportListComponent` (ADMIN, SUPERVISEUR, DELEGUE)
- `/marketing/supports/new` → `SupportFormComponent` (ADMIN, SUPERVISEUR)
- `/marketing/supports/:id` → `SupportDetailComponent`
- `/marketing/supports/:id/edit` → `SupportFormComponent` (ADMIN, SUPERVISEUR)

> Note: the in-product Supports tab on `/products/:id` is the primary
> entry point for marketing CRUD. These dedicated routes still exist for
> direct linking.

### 6.1 Support marketing list
- **Component:** `SupportListComponent`
  ([support-list.component.html](Cynapharm/src/app/features/marketing/support-list/support-list.component.html))

`<h1>Marketing Supports for {{ productName }}</h1>`. Debug line shows
`loading`, `error`, `supports.length`.

Action bar: **Add Support** (`/marketing/supports/new?productId=`),
**Back to Product** (`onBackToProduct`).

**Table columns** (driven by `columns[]`): each column shows
`{{ getValue(support, col.key) }}` + Actions
(View / Edit / Delete buttons).

Per-row actions: 👁 View (`onView`), ✏ Edit (`onEdit`), ✕ Delete
(`onDelete`).

Empty state: *"No marketing supports found for this product."*

---

### 6.2 Support marketing form
- **Component:** `SupportFormComponent`
  ([support-form.component.html](Cynapharm/src/app/features/marketing/support-form/support-form.component.html))

Title *"{{ isEditMode ? 'Edit Support' : 'Add Marketing Support' }}"*.

**Form fields**

| Field | Control | Notes |
|---|---|---|
| Support Type * | `type` (select)        | options from `supportTypes` |
| Campaign Name (Optional) | `campaignName` (text) | — |
| Active         | `isActive` (checkbox)  | — |

Buttons: **Cancel** (`onCancel`), `<app-button>` **Create / Update Support**
— disabled when `supportForm.invalid || loading`.

Feedback divs: `error`, `success`.

---

### 6.3 Support marketing detail
- **Component:** `SupportDetailComponent`
  ([support-detail.component.html](Cynapharm/src/app/features/marketing/support-detail/support-detail.component.html))

Title *"Marketing Support Details"*.

Detail rows: ID, Type, Campaign Name, Status (Active/Inactive class).

Files section (only if `Fichiers.length > 0`): list of file rows —
name, extension, size, `Download` link.

Action buttons: **Back to Supports** (`onBack`), **Edit** (`onEdit`),
**Delete** (`onDelete`).

---

## 7. USERS

Routes ([users-routing.module.ts](Cynapharm/src/app/features/users/users-routing.module.ts)) — all `[authGuard, roleGuard]` with
`roles: [ADMIN, SUPERVISEUR]`.
- `/users`, `/users/new`, `/users/:id`, `/users/:id/edit`.

### 7.1 User list
- **Component:** `UserListComponent`
  ([user-list.component.html](Cynapharm/src/app/features/users/user-list/user-list.component.html))

Header *"Gestion des utilisateurs"* + sub `{{ allUsers.length }}
utilisateur(s) au total`. CTA **+ Nouvel utilisateur** → `/users/new`.

**KPI cards (clickable)**

| Card | `statusFilter` |
|---|---|
| Total (`allUsers.length`)        | `all`      |
| Actifs (`totalActive`)           | `active`   |
| Désactivés (`totalDisabled`)     | `disabled` |

**Toolbar**
- Search (`searchTerm`, min 3 chars).
- Filter `statusFilter`: all / active / disabled.
- Filter `roleFilter`: All + each role with `countByRole(r)`.
- Counter `{{ filteredUsers.length }} résultat(s)`.

**Table columns**

| Col | Source |
|---|---|
| Utilisateur | avatar (`getInitials`) + name + `#{{ id }}` |
| Email       | `mailto:` link |
| Téléphone   | `user.phoneNumber` |
| Rôle        | `role-badge` with role-specific class |
| Statut      | active/disabled dot + label (computed from `user.isDeleted`) |
| Actions     | 👁 Voir, ✏ Modifier, ⊘ Désactiver / ✔ Réactiver |

Row class `row-disabled` when `user.isDeleted`.

**Per-row actions**

| Button | Click | Visible |
|---|---|---|
| Voir le profil   | `onView(id)` → `/users/:id` | always |
| Modifier         | `onEdit(id)` → `/users/:id/edit` | always |
| Désactiver       | `onDisable(user)` — opens modal `disable` | `!user.isDeleted` |
| Réactiver        | `onEnable(user)` — opens modal `enable` | `user.isDeleted` |

**Confirmation modal** (`showConfirmModal`):
- Icon depends on `confirmAction` (disable/enable).
- Title: *Désactiver l'utilisateur* / *Réactiver l'utilisateur*.
- Body: explains side-effect + shows `confirmUser.name (email)`.
- Buttons: **Annuler** / **Désactiver | Réactiver** (`onConfirmAction`).

Pagination: ‹ / numbered / › when `filteredUsers.length > pageSize`.

---

### 7.2 User detail
- **Component:** `UserDetailComponent`
  ([user-detail.component.html](Cynapharm/src/app/features/users/user-detail/user-detail.component.html))

Header: ← *Utilisateurs* + *Profil utilisateur*.

**Identity card**: large avatar + initials, `user.name`, `user.email`,
role badge, status badge.

**Actions row**
- **← Retour à la liste** (`/users`).
- **Modifier le rôle** (`onEdit()`).
- **Désactiver** (`openDisable()`) — only when `!user.isDeleted`.
- **Réactiver** (`openEnable()`) — only when `user.isDeleted`.

**Stock summary section** — only `@if ((user.role | uppercase) === 'DELEGUE')`.
Four KPI tiles: Produits, Qté disponible, Distributions, Qté distribuée
(from `stockSummary`). Status chips: stocks faibles, vides, last
movement timestamp.

**Tabs (DELEGUE only)**: Informations | Rapports (`rapports.length`) |
Mouvements (`movements.length`).

Rapports tab table: `#`, Date, Résultat (chip from
`getResultatClass`), Commentaire, Validé.

Mouvements tab table: Date, Type (chip), Quantité, Description.

**Details card** (Informations détaillées) — always shown for non
delegues:

| Detail | Value |
|---|---|
| ID            | `#{{ user.id }}` |
| Nom complet   | `user.name` |
| Email         | `mailto:` link |
| Téléphone     | `user.phoneNumber` |
| Adresse       | `user.adresse` |
| Rôle          | role badge |
| Statut        | active/disabled badge |

**Action modal**: same disable/enable confirm flow as list.

---

### 7.3 User form
- **Component:** `UserFormComponent`
  ([user-form.component.html](Cynapharm/src/app/features/users/user-form/user-form.component.html))

Header ← *Utilisateurs* + title
*"{{ isEditMode ? 'Modifier le rôle' : 'Nouvel utilisateur' }}"*.

Mode badge *"Création d'un nouveau compte"* / *"Modification du rôle
uniquement"*.

**Section 1 — Identité**

| Field | Control | Edit mode behavior |
|---|---|---|
| Nom complet *  | `name`  | `readonly` in edit |
| Adresse email *| `email` | `readonly` in edit |

**Section 2 — Coordonnées** (`*ngIf="!isEditMode"`):

| Field | Control |
|---|---|
| Téléphone           | `phoneNumber` |
| Adresse *           | `adresse` |

**Section 3 — Rôle & Accès**

| Field | Control | Notes |
|---|---|---|
| Rôle *               | `role` select | options from `roles[]` |
| Type d'utilisateur   | `userType` select | only `!isEditMode && role === 'CLIENT'` |

**Section 4 — Sécurité** (`*ngIf="!isEditMode"`):
- Mot de passe * — `password`, min 6 chars.

Edit-mode info note: *"En mode édition, seul le rôle peut être
modifié."*

Feedback: red `uf-error`, green `uf-success`.

Actions: **Annuler** (`/users`), **Créer l'utilisateur / Mettre à jour
le rôle** — disabled when `userForm.invalid || loading`.

---

## 8. ORDERS

Routes ([orders-routing.module.ts](Cynapharm/src/app/features/orders/orders-routing.module.ts)) — `authGuard` from app root:
- `/orders` → `OrderListComponent`
- `/orders/new` → `OrderFormComponent`
- `/orders/reclamations` → `ReclamationListComponent`
- `/orders/reclamations/new` → `ReclamationFormComponent`
- `/orders/reclamations/:id` → `ReclamationDetailComponent`
- `/orders/reclamations/:id/edit` → `ReclamationFormComponent`
- `/orders/:id` → `OrderDetailComponent`
- `/orders/:id/edit` → `OrderFormComponent`

### 8.1 Order list
- **Component:** `OrderListComponent`
  ([order-list.component.html](Cynapharm/src/app/features/orders/order-list/order-list.component.html))

Header: *"Commandes"* + sub *"Gestion des commandes clients"*.
Right side: **Réclamations** link (`/orders/reclamations`) with red
`recl-badge` showing `reclamationsTotal` when > 0.

**Filters**
- `statusFilter` select (options from `statuses[]`).
- Date range: `startDate`, `endDate` inputs separated by `→`.
- **✕ Effacer** clears filters when any is set.

**Error banner**: with `<button class="retry-btn" (click)="load()">Réessayer</button>`.

**Loading row**: spinner + *"Chargement des commandes…"*.

**Empty state**: *"Aucune commande trouvée."*

**Table columns**

| Col | Source |
|---|---|
| N° Commande   | `#{{ order.Id_Commande }}` |
| Date          | `DateCommande` (dd/MM/yyyy) |
| Client        | `getClientName(Id_Client)` — falls back to *"Client inconnu"* |
| Lignes        | count badge `getLignesCount(order)` |
| HT (TND)      | `MontantTotalHT` |
| TTC (TND)     | `MontantTTC` (bold) |
| Statut        | chip via `getEtatClass(Statut)` + `getEtatLabel` |
| Actions       | see below |

**Per-row actions**

| Button | Visible | Click |
|---|---|---|
| 👁 Voir le détail | always | `onView(Id_Commande)` |
| 🔄 Changer le statut (toggle menu) | `getNextStatuses(order).length > 0` | `toggleStatusMenu(id)` — opens dropdown with each `next.label` calling `changeStatus(order, next.value)` |
| 🗑 Supprimer | `isAdmin && canDelete(order)` (Brouillon/Annulée only) | `openDeleteModal(order)` |

Status menu items use status-specific class.

Backdrop closes the open menu (`closeStatusMenu`).

**Delete modal** (`showDeleteModal && deletingOrder`): Annuler /
Supprimer (`confirmDelete`).

Pagination: ‹ Précédent / Page {{ currentPage }} / Suivant ›
(disabled when `!hasMore`).

---

### 8.2 Order detail
- **Component:** `OrderDetailComponent`
  ([order-detail.component.html](Cynapharm/src/app/features/orders/order-detail/order-detail.component.html))

Header: ← *Commandes* + title `Commande #{{ order.Id_Commande }}` with
sub date + client name. Status chip (large), action buttons.

**Header actions**

| Button | Visible | Click |
|---|---|---|
| Direct-transition (label varies) | `directTransition` is set | `applyStatus(directTransition.value)` |
| **Changer le statut** | `canChangeStatus()` and no direct transition | `openStatusModal()` |
| **Supprimer** (danger) | `isAdmin && canDelete()` | `openDeleteModal()` |

**KPI row**
- Total HT (TND) — `MontantTotalHT`
- Total TTC (TND) — `MontantTTC`
- Ligne(s) — `getTotalLignes()`
- Réclamation(s) — `reclamations.length`

**Tabs**: `info` | `lignes` ({{ count }}) | `reclamations` (count) |
`documents` (`totalDocs`).

#### Tab: Informations

| Label | Source |
|---|---|
| N° Commande  | `#{{ order.Id_Commande }}` |
| Date         | `DateCommande` dd/MM/yyyy HH:mm |
| Client       | `clientName` |
| Statut       | chip |
| Montant HT   | `MontantTotalHT` TND |
| Montant TTC  | `MontantTTC` TND (bold) |

`@if (order.Statut === 'Annulee' || 'Annulée')` — red box with motif.

#### Tab: Lignes
Table: `N°`, `Produit` (`getProductName(Id_Produit)`), `N° Lot`,
`Qté`, `Prix unitaire`, `Remise`, `Sous-total`
(`PrixUnitaire * Quantite * (1 - Remise/100)`).

#### Tab: Réclamations
Toolbar: count + **Voir toutes** link to
`/orders/reclamations?orderId=`.

Table: `#Rec`, `Message`, `Date`, `Ligne`, `Statut` chip,
(if admin) delete button → `onDeleteRec(Id_Rec)`.

#### Tab: Documents
Three sub-tables (Bons de commande, Bons de livraison, Factures), each
with its own create button:
- **Créer un bon de commande** when `statutNumber >= 2 && != 6 && bonsCommandes.length === 0 && isAdmin` → `createBC()`.
- **Créer un bon de livraison** when `statutNumber >= 4 && != 6 && bonsLivraison.length === 0 && isAdmin` → `createBL()`.
- **Créer une facture** when `statutNumber === 5 && factures.length === 0 && isAdmin` → `createFacture()`.

Each table has columns (N° Doc, Nom, Client, Date, …) and an admin-only
🗑 delete column (`onDeleteBC/BL/Facture(numero_Doc)`).

**Modals**
- Delete order modal — Annuler / Supprimer (`confirmDeleteOrder()`).
- Cancel with motif modal — textarea `cancelMotif`, **Confirmer
  l'annulation** (`confirmCancel()`).
- Status modal — list of `statusOptions` buttons, each triggers
  `applyStatus(opt.value)`.

---

### 8.3 Order form (Create)
- **Component:** `OrderFormComponent`
  ([order-form.component.html](Cynapharm/src/app/features/orders/order-form/order-form.component.html))

← *Commandes* back. Title *"Nouvelle commande"* + notice *"Le client
est identifié automatiquement via votre token JWT."*

**Lignes array** (`FormArray Lignes`)
Each row (`#i + 1`):
- `Id_Produit` * (number)
- `Quantite` * (number, min 1)
- `PrixUnitaire` * (number, step 0.01, TND placeholder)
- `Remise` % (number, 0–100, step 0.1)
- Remove button (visible only if `lignes.length > 1`).

**+ Ajouter une ligne** appends a new row via `addLigne()`.

**Other fields**
- `IsFinalValidation` checkbox — *"passe en statut En attente"*; hint
  *"Laissez décoché pour enregistrer en brouillon."*.

Error: red `alert-danger` when `error`.

**Buttons**
- **Annuler** → `/orders`.
- **Créer la commande** (submit) — disabled when `loading`; text
  changes to *"Envoi…"*.

---

## 9. DOCUMENTS

Routes ([documents-routing.module.ts](Cynapharm/src/app/features/documents/documents-routing.module.ts)) —
`[authGuard, roleGuard]` (ADMIN, SUPERVISEUR):
- `/documents/general` → `DocumentListComponent` (default redirect)
- `/documents/bons-commandes` → `BonCommandeListComponent`
- `/documents/bons-livraison` → `BonLivraisonListComponent`
- `/documents/factures` → `FactureListComponent`

### 9.1 Facture list
- **Component:** `FactureListComponent`
  ([facture-list.component.html](Cynapharm/src/app/features/documents/factures/facture-list/facture-list.component.html))

Header: *"Factures"* + `{{ factures.length }} facture(s)`. Loading
block / `<app-empty-state>` when empty.

**Table columns**: N° Doc, Nom, Client, Commande, Date facture,
Montant HT (`currencyTND`), Montant TTC (`currencyTND`, bold),
Actions.

Per-row: 🗑 delete (`delete(numero_Doc)`).

`<app-paginator [page] [pageSize] [total] (pageChange)="onPage($event)">`.

---

### 9.2 Bon de commande list
- **Component:** `BonCommandeListComponent`
  ([bon-commande-list.component.html](Cynapharm/src/app/features/documents/bons-commandes/bon-commande-list/bon-commande-list.component.html))

Header *"Bons de commande"* + count.
**Table**: N° Doc, Nom, Client, Commande, Date création, Actions (🗑 delete).
Empty state + paginator.

---

### 9.3 Bon de livraison list
- **Component:** `BonLivraisonListComponent`
  ([bon-livraison-list.component.html](Cynapharm/src/app/features/documents/bons-livraison/bon-livraison-list/bon-livraison-list.component.html))

Same shape as BC list — title "Bons de livraison". Same columns and
🗑 delete row action.

---

### 9.4 Documents general list (cross-type)
- **Component:** `DocumentListComponent`
  ([document-list.component.html](Cynapharm/src/app/features/documents/documents-general/document-list/document-list.component.html))

Header *"Documents"* + count. CTA **+ Nouveau document** (`/documents/new`).

Type-filter tabs: **Tous** / **Factures** / **Bons de commande** /
**Bons de livraison** (`setTypeFilter('' | 'FACTURE' | 'BC' | 'BL')`).

**Table columns**: N° Doc (mono), Type, Client (`#{{ id_Client }}`),
Commande (`#id` or `—`), Date.

Pagination via `<app-paginator>`.

---

## 10. INVENTORY

Routes ([inventory-routing.module.ts](Cynapharm/src/app/features/inventory/inventory-routing.module.ts)) — `[authGuard, roleGuard]`
(ADMIN, SUPERVISEUR, DELEGUE):
- `/inventory/stocks` → `StockListComponent` (default redirect)
- `/inventory/stocks/new`, `:id`, `:id/edit` → list / detail / form.
- `/inventory/movements` → `MovementListComponent`.
- `/inventory/distributions[/new|/:id]` → distribution screens.
- `/inventory/promo-stocks` → `PromoStockDetailComponent`.

### 10.1 Stock delegué list
- **Component:** `StockListComponent`
  ([stock-list.component.html](Cynapharm/src/app/features/inventory/stocks/stock-list/stock-list.component.html))

Header *"Stocks Délégués"* + `{{ stocks.length }} ligne(s)`.
CTA **+ Nouveau stock** (`/inventory/stocks/new`).

**Table columns**: Délégué (`getDelegrueName`), Produit
(`getProductName`), N° Lot (mono), Expiration
(`getLotDate(numeroLot, dateExpiration)`), Disponible
(red if 0), Réservé, Actions.

Per-row: 👁 view (`/inventory/stocks/:id`), ✏ edit
(`/inventory/stocks/:id/edit`), 🗑 delete (`openDelete(s)`).

Delete modal: details about stock + Annuler / Supprimer
(`confirmDelete()`).

`<app-paginator>` at bottom.

---

### 10.2 Stock detail
- **Component:** `StockDetailComponent`
  ([stock-detail.component.html](Cynapharm/src/app/features/inventory/stocks/stock-detail/stock-detail.component.html))

Header ← *Retour aux stocks* + *"Détail du stock"*.
Empty state via `<app-empty-state>` if not found.

**Detail grid**: ID Stock, Délégué (`delegeName`), Produit
(`productName`), Numéro de lot (mono), Date d'expiration, Qté
disponible, Qté réservée.

Actions: **Modifier** → `/inventory/stocks/edit/:id`, **Retour** →
`/inventory/stocks`.

---

### 10.3 Stock form
- **Component:** `StockFormComponent`
  ([stock-form.component.html](Cynapharm/src/app/features/inventory/stocks/stock-form/stock-form.component.html))

← *Retour* back link + title *"{{ isEdit ? 'Modifier le stock' :
'Nouveau stock' }}"*.

**Form fields**

| Field | Control | Notes |
|---|---|---|
| Délégué *           | `id_User_Delegue` select | from `delegues[]` |
| Produit *           | `id_Produit` select       | from `products[]` |
| Numéro de lot *     | `numeroLot` select        | cascading from product; loads `lots[]` async (qté + exp date in option text) |
| Date d'expiration   | display-only (`lotDateDisplay`) + hidden `dateExpiration` | auto-filled from selected lot |
| Qté disponible *    | `qteDisponible` (number, min 1) | — |

**Buttons**: **Enregistrer / Mettre à jour** (submit, disabled while
`saving`), **Annuler** → `/inventory/stocks`.

Banners: `submitError`, `successMsg`.

---

### 10.4 Distribution list (échantillons)
- **Component:** `DistributionListComponent`
  ([distribution-list.component.html](Cynapharm/src/app/features/inventory/distributions/distribution-list/distribution-list.component.html))

Header *"Distributions"* + CTA **+ Nouvelle distribution**.

**Tabs**: dynamic from `tabs[]` (e.g. *Délégué*, *Médecin*,
*Pharmacien*, *Toutes*). Each filter tab shows a dropdown
(`selectedUserId` from `filterUsers[]`) and triggers `load()`.

**Per-entity table** (`activeTab !== 'all'`):
columns Délégué, Lot (mono), Qté, Date, Médecin (or `—`),
Pharmacien (or `—`), Voir link → `/inventory/distributions/:id`.

**All tab** table: Délégué, Destinataire (medecin or pharmacien name),
Lot, Qté, Date, Voir.
With **Charger plus** button when `hasMore`.

Empty state via `<app-empty-state>`.

---

### 10.5 Distribution detail
- **Component:** `DistributionDetailComponent`
  ([distribution-detail.component.html](Cynapharm/src/app/features/inventory/distributions/distribution-detail/distribution-detail.component.html))

← *Retour* + *"Détail de la distribution"*.

**Detail grid**: ID Distribution (mono), Délégué (ID), Stock (ID),
Quantité, Numéro de lot (mono), Date de distribution (HH:mm),
Médecin (ID) or `—`, Pharmacien (ID) or `—`.

Action: **Retour à la liste** (`/inventory/distributions`).

---

### 10.6 Distribution form
- **Component:** `DistributionFormComponent`
  ([distribution-form.component.html](Cynapharm/src/app/features/inventory/distributions/distribution-form/distribution-form.component.html))

← *Retour* + *"Nouvelle distribution"*.

**Form fields**

| Field | Control | Notes |
|---|---|---|
| Délégué *      | `id_Delegue` select | from `delegues[]` |
| Stock *        | `id_Stock` select   | cascading from délégué; disabled until délégué selected; shows *"Lot X — Dispo: Y"* |
| Numéro de lot *| `numeroLot` text    | auto-filled, `readonly` |
| Quantité *     | `qte` (number, min 1) | — |
| Médecin (opt)  | `id_Medecin` select | from `medecins[]` |
| Pharmacien (opt)| `id_Pharmacien` select | from `pharmaciens[]` |

`recipientError` banner: *"Au moins un destinataire est requis…"*.

Buttons: **Créer** (submit, disabled while `saving`) / **Annuler**.

---

### 10.7 Movements list
- **Component:** `MovementListComponent`
  ([movement-list.component.html](Cynapharm/src/app/features/inventory/movements/movement-list/movement-list.component.html))

Header *"Mouvements de stock"* + subtitle.

Filter bar: ID Stock input (number) + **Filtrer** (`applyFilter()`,
disabled while `!filterStockId`) + **Effacer** (when `activeStockId`).

Hint panel before any filter is applied.

**Table columns**: ID (mono), ID Stock, Quantité (color-badge for
`increment`/`decrement`), Type (`type-chip` chip-in/chip-out/chip-other),
Date (HH:mm), Description.

Empty state for *no movements found for stock*.

---

### 10.8 Promo stock detail
- **Component:** `PromoStockDetailComponent`
  ([promo-stock-detail.component.html](Cynapharm/src/app/features/inventory/promo-stocks/promo-stock-detail/promo-stock-detail.component.html))

← *Retour* + *"Stocks promotionnels"*.

**Lookup bar**: ID du stock input + **Rechercher** (`lookup()`,
disabled while `loadingLookup`).

Then two side-by-side cards:

#### Card "Stock gratuite" (`badge-purple`)
Info list: Délégué (ID), Produit (ID), N° lot (mono), Qté disponible,
Qté réservée, Qté gratuite (highlight), Type promo.

Sub-form `gratuiteForm`: id_User_Delegue, id_Produit, numeroLot,
qteDisponible, qteReservee, qteGratuite, typePromotion. Submit calls
`saveGratuite()`.

#### Card "Stock échantillon" (`badge-teal`)
Info list mirrors above + Qté échantillon.

Sub-form `echantillonForm` with the same fields minus typePromotion.
Submit calls `saveEchantillon()`.

Per-card success/error banners (`gratuiteSuccess`, `gratuiteError`,
`echantillonSuccess`, `echantillonError`).

Hint panel before first search.

---

## 11. FIELD

Routes ([field-routing.module.ts](Cynapharm/src/app/features/field/field-routing.module.ts)) — `[authGuard, roleGuard]`
(ADMIN, SUPERVISEUR, DELEGUE).
Default redirect → `/field/visites`.

### 11.1 Visite list (par délégué)
- **Component:** `VisiteListComponent`
  ([visite-list.component.html](Cynapharm/src/app/features/field/visites/visite-list/visite-list.component.html))

Header *"Visites"* + subtitle. CTA **+ Nouvelle visite** →
`/field/visites/new`.

Filter bar: ID Délégué input + **Rechercher** (`load()`, disabled when
`!delegueId || loading`).

**Table columns**: ID (mono), Délégué, Date, Type (chip
`chip-med`/`chip-ph`/`chip-other` via `VisiteType`), Médecin (ID),
Pharmacien (ID), Actions (✏ Modifier
→ `/field/visites/edit/:id`).

Empty state, loading spinner, error banner.

---

### 11.2 Visite detail / form
- **Component:** `VisiteFormComponent`
  ([visite-form.component.html](Cynapharm/src/app/features/field/visites/visite-form/visite-form.component.html))

> The app has no read-only "visite detail" page — the form acts as
> both edit and view. The "Toutes les visites" list (`/field/visites/all`)
> displays a status chip ("Terminée" / "En cours") and click row
> triggers `onRow(visite)` which navigates to the form.

← *Retour* + title *"{{ isEdit ? 'Modifier la visite' : 'Nouvelle
visite' }}"*.

**Form fields**

| Field | Control | Type |
|---|---|---|
| Délégué (ID) *      | `id_User_Delegue` | number |
| Date *              | `date`            | date |
| Type de visite *    | `type` select     | from `typeOptions` |
| Médecin (ID) (opt)  | `id_Medecin`      | number |
| Pharmacien (ID) (opt)| `id_Pharmacien`  | number |

Buttons: **Créer / Mettre à jour** (`submit`, disabled while
`saving`), **Annuler** → `/field/visites`.

#### Bonus: Visite all
- **Component:** `VisiteAllComponent`
  ([visite-all.component.html](Cynapharm/src/app/features/field/visites/visite-all/visite-all.component.html))
- **Route:** `/field/visites/all`

Filter bar: date début, date fin, ID Délégué (`onDelegueFilter`).
**Actualiser** button.

Table: #ID, Délégué (`#id`), Date, Type, Statut (chip
"Terminée"/"En cours" depending on `isCompleted`). Click a row →
`onRow(v)`.

---

### 11.3 Planning list
- **Component:** `PlanningListComponent`
  ([planning-list.component.html](Cynapharm/src/app/features/field/plannings/planning-list/planning-list.component.html))

Header *"Plannings"* + CTA **+ Nouveau planning**.

Filter bar: ID Délégué + **Rechercher** (`load()`).

**Table columns**: ID (mono), Délégué, Date, Heure début, Heure fin,
État (badge `badge-pending|badge-confirmed|badge-cancelled` via
`EtatPlanning` enum), Actions (✏ Modifier
→ `/field/plannings/edit/:id`).

---

### 11.4 Planning form
- **Component:** `PlanningFormComponent`
  ([planning-form.component.html](Cynapharm/src/app/features/field/plannings/planning-form/planning-form.component.html))

← *Retour* + *"{{ isEdit ? 'Modifier le planning' : 'Nouveau
planning' }}"*.

**Form fields**

| Field | Control | Type |
|---|---|---|
| Délégué (ID) *  | `id_User_Delegue` | number |
| Date *          | `date`            | date |
| Heure de début  | `heureDebut`      | time |
| Heure de fin    | `heureFin`        | time |
| État du planning| `etatPlanning` select | from `etatOptions` |

Buttons: **Créer / Mettre à jour** (`submit`), **Annuler**.

---

### 11.5 Rapport list
- **Component:** `RapportListComponent`
  ([rapport-list.component.html](Cynapharm/src/app/features/field/rapports/rapport-list/rapport-list.component.html))

Header *"Rapports de visite"* + sub *"Tous les rapports soumis"*.
CTA **+ Nouveau rapport**.

Empty state with CTA.

Error banner with **Réessayer** (`load()`).

**Table columns**: Délégué (`getDelegrueName`), Visite (`#id_Visite`),
Date rapport, Commentaire (truncated, title=full), Résultat,
Validé — `valid-badge`: *Validé* / *Refusé* / *En attente* (from
`estValide`).
Actions: ✏ Modifier → `/field/rapports/edit/:id`.

---

### 11.6 Rapport form
- **Component:** `RapportFormComponent`
  ([rapport-form.component.html](Cynapharm/src/app/features/field/rapports/rapport-form/rapport-form.component.html))

← *Retour* + *"{{ isEdit ? 'Modifier le rapport' : 'Nouveau
rapport' }}"*.

**Form fields**

| Field | Control | Notes |
|---|---|---|
| Délégué *    | `id_User_Delegue` select | from `delegues[]` |
| Visite *     | `id_Visite` select       | cascading from délégué; disabled until délégué selected |
| Commentaire *| `commentaire` textarea (4 rows) | required |
| Résultat *   | `resultat` select        | from `resultats[]` |

Buttons: **Créer / Mettre à jour** (submit), **Annuler**.

---

### 11.7 Objectif list
- **Component:** `ObjectifListComponent`
  ([objectif-list.component.html](Cynapharm/src/app/features/field/objectifs/objectif-list/objectif-list.component.html))

Header *"Objectifs"* + sub *"Suivi des objectifs commerciaux"* +
CTA **+ Nouvel objectif**.

Cards grid (`cards-grid`) per objectif:
- Top row: type, periode badge, ID (mono).
- Date range *"dd/MM/yyyy — dd/MM/yyyy"*.
- Progress section: header *"Progression"* +
  `valeurRealisee / valeurCible`; progress bar fill width
  `progressPct()`; `.complete` when realised ≥ target.
- Footer: delegate tag + ✏ Modifier
  (`/field/objectifs/edit/:id`).

Error banner with **Réessayer**. Empty state + CTA.

---

### 11.8 Objectif form
- **Component:** `ObjectifFormComponent`
  ([objectif-form.component.html](Cynapharm/src/app/features/field/objectifs/objectif-form/objectif-form.component.html))

← *Retour* + *"{{ isEdit ? 'Modifier l'objectif' : 'Nouvel
objectif' }}"*.

**Form fields**

| Field | Control | Notes |
|---|---|---|
| Délégué *         | `id_User_Delegue` select | from `delegues[]` |
| Type d'objectif * | `type` select           | from `typeOptions` |
| Période *         | `periode` select        | from `periodeOptions` — auto-fills dates |
| Valeur cible *    | `valeurCible` (number, min 1) | — |
| Date de début *   | `dateDebut` date        | auto-filled by `periode` |
| Date de fin *     | `dateFin` date          | auto-filled by `periode` |

Buttons: **Créer / Mettre à jour** (submit), **Annuler**.

---

### 11.9 Region list
- **Component:** `RegionListComponent`
  ([region-list.component.html](Cynapharm/src/app/features/field/regions/region-list/region-list.component.html))

Header *"Régions"* + sub + CTA **+ Nouvelle région**.

**Table columns**: Nom de la région (bold), Code postal (mono),
Délégué (`getDelegrueName`), Actions (✏ Modifier
→ `/field/regions/edit/:id`).

Empty state with CTA, error banner with Retry.

---

### 11.10 Region form
- **Component:** `RegionFormComponent`
  ([region-form.component.html](Cynapharm/src/app/features/field/regions/region-form/region-form.component.html))

← *Retour* + *"{{ isEdit ? 'Modifier la région' : 'Nouvelle
région' }}"*.

**Form fields**

| Field | Control | Validators |
|---|---|---|
| Nom de la région * | `nomRegion` (text)        | required |
| Code postal *      | `codePostal` (text, maxLength 10) | required + pattern digits ≥ 4 |
| Délégué *          | `id_User_Delegue` select  | required |

Buttons: **Créer / Mettre à jour** (submit), **Annuler**.

---

### 11.11 KPI dashboard
- **Component:** `KpiDashboardComponent`
  ([kpi-dashboard.component.html](Cynapharm/src/app/features/field/kpi/kpi-dashboard/kpi-dashboard.component.html))

← *Field* back + *"KPI Délégués"* title.

Filters row: `idDelegue` (number), `dateDebut` (date), `dateFin`
(date). **Charger** button — disabled while `loading || !idDelegue`.

Error box for `error`.

KPI cards (after `loaded`):
- Visites — `visitesCount`.
- Performance — `performanceRate | number:'1.0-1'` `%`.
- Taux de conversion — `tauxConversion | number:'1.0-1'` `%`. Falls
  back to *"—"* + *"Période requise"* when null; shows ellipsis while
  `loadingTaux`.

Historique panel: list of raw entries `{{ h | json }}` (if any).

---

## 12. Global navigation guards & role matrix

Defined in [`app.routes.ts`](Cynapharm/src/app/app.routes.ts) via
`authGuard` and `roleGuard` (`UserRole` enum:
`ADMIN | SUPERVISEUR | DELEGUE | MEDECIN | CLIENT`).

| Section | Auth | Role-restricted |
|---|---|---|
| `/login`, `/register`, `/forgot-password`, `/reset-password`, `/forbidden` | public | — |
| `/dashboard` | auth | — |
| `/products` | auth | — |
| `/lots` | auth | ADMIN, SUPERVISEUR, DELEGUE |
| `/promotions` | auth | ADMIN, SUPERVISEUR |
| `/marketing` | auth | ADMIN, SUPERVISEUR, DELEGUE |
| `/orders` | auth | — |
| `/inventory` | auth | ADMIN, SUPERVISEUR, DELEGUE |
| `/field` | auth | ADMIN, SUPERVISEUR, DELEGUE |
| `/documents` | auth | ADMIN, SUPERVISEUR |
| `/users` | auth | ADMIN, SUPERVISEUR |
| `/settings` | auth | — |
| `**` fallback | redirect to `/dashboard` | — |

Lots, promotions, users have additional per-route role checks inside
their own routing modules — `/lots/new` and `/lots/:numero/edit` require
ADMIN or SUPERVISEUR; `/users/*` require ADMIN or SUPERVISEUR; etc.

---

*End of inventory.*
