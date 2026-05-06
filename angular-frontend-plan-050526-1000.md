# Cynapharm CRM — Angular Frontend Generation Plan

**Date:** 2026-05-05
**Branch:** `Front-001`
**Backend solution root:** `C:\Cynapharm\CynapSoftCRMBackend\`
**Angular app root:** `C:\Cynapharm\CynapSoftCRMBackend\Cynapharm\src\app\`
**Gateway base URL (dev):** `http://localhost:5555`
**Architecture reference:** `.github/plans/angular-architecture-plan.md`

---

## 1. Executive Summary

This plan covers everything still missing on the Angular side relative to the .NET microservices backend (Auth / Product / Order / Inventory / Field / Doc) routed through the Ocelot gateway.

What is still missing or incomplete on the front-end:

- 7 brand-new feature modules (Inventory, Stock-Movements, Distributions, Stocks-Promotionnels, Field-Visites, Field-Plannings, Field-Rapports, Field-Objectifs, Field-Regions, Field-KPI, Documents, BonsCommandes, BonsLivraisons, Factures, Reclamations).
- 3 component-level extensions inside existing modules (Reclamations under Orders, dedicated Promotions module split out from Marketing, Lignes management view under Orders).
- Several shared pipes/components/guards.
- Sidebar/navigation overhaul.

The backend is **fully** mapped section-by-section in the API tables below — each row is the **gateway upstream path** the Angular service must call (never the downstream service path).

---

## 2. Complete Backend Web API Inventory (extracted from solution)

The Ocelot gateway listens on `http://localhost:5555` and rewrites these upstream paths to the matching downstream microservice. **All Angular services MUST call the upstream path.**

### 2.1 AuthAPI — port 7000 (gateway prefix: `/auth`)

| Method | Gateway Path                      | Roles                          | Purpose                          |
|--------|-----------------------------------|--------------------------------|----------------------------------|
| POST   | `/auth/login`                     | (anonymous)                    | User login                       |
| POST   | `/auth/forgot-password`           | (anonymous)                    | Request password reset email     |
| POST   | `/auth/register`                  | ADMIN, SUPERVISEUR, DELEGUE    | Create user                      |
| GET    | `/auth/users`                     | ADMIN                          | List all users                   |
| GET    | `/auth/users/{id}`                | ADMIN, SUPERVISEUR             | Get user by id                   |
| GET    | `/auth/disabled-users`            | ADMIN                          | List disabled users              |
| POST   | `/auth/AssignRole`                | ADMIN, SUPERVISEUR             | Assign role to user              |
| PUT    | `/auth/add-role`                  | ADMIN, SUPERVISEUR             | Add role to user                 |
| PUT    | `/auth/change-role`               | ADMIN, SUPERVISEUR             | Change a user's role             |
| PUT    | `/auth/change-password`           | (authenticated)                | Change own password              |
| PUT    | `/auth/reset-password`            | (anonymous, token-based)       | Reset password using token       |
| PUT    | `/auth/enable-user/{email}`       | ADMIN                          | Re-enable a disabled user        |
| PUT    | `/auth/delete-user/{email}`       | ADMIN                          | Soft-delete (disable) user       |

### 2.2 ProductAPI — port 7005

#### 2.2.1 Products (`/products`)

| Method | Gateway Path                                    | Roles                          |
|--------|-------------------------------------------------|--------------------------------|
| GET    | `/products`                                     | (auth)                         |
| GET    | `/products/{id}`                                | (auth)                         |
| GET    | `/products/visible`                             | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/products/available`                           | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/products/unavailable`                         | ADMIN, SUPERVISEUR             |
| GET    | `/products/{productId}/available`               | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/products/{productId}/valid`                   | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/products/{productId}/can-archive`             | ADMIN                          |
| GET    | `/products/{productId}/stock`                   | ADMIN, SUPERVISEUR             |
| GET    | `/products/stock-status`                        | ADMIN, SUPERVISEUR             |
| GET    | `/products/low-stock?seuil=N`                   | ADMIN, SUPERVISEUR             |
| GET    | `/products/search?keyword=&isActive=&allowArchived=&limit=` | (auth)              |
| GET    | `/products/filter?keyword=&category=&allowArchived=&isActive=&page=&pageSize=` | ADMIN, SUPERVISEUR, DELEGUE |
| GET    | `/products/categories`                          | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/products/category/{category}`                 | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/products/exists?productName=`                 | ADMIN, SUPERVISEUR             |
| GET    | `/products/top?topN=N`                          | ADMIN, SUPERVISEUR             |
| GET    | `/products/dashboard`                           | ADMIN, SUPERVISEUR             |
| POST   | `/products`                                     | ADMIN, SUPERVISEUR             |
| PUT    | `/products/{productId}/archive`                 | ADMIN                          |
| PUT    | `/products/{productId}/activate`                | ADMIN, SUPERVISEUR             |
| PUT    | `/products/{id}/deactivate`                     | ADMIN                          |

#### 2.2.2 Lots (`/products/lots` upstream → `/api/lots` downstream)

| Method | Gateway Path                                          | Roles                          |
|--------|-------------------------------------------------------|--------------------------------|
| GET    | `/products/lots`                                      | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/products/lots/{id}/lots`                            | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/products/lots/lot/{numeroLot}`                      | ADMIN, SUPERVISEUR             |
| GET    | `/products/lots/lot/{numeroLot}/expired`              | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/products/lots/lot/{numeroLot}/out-of-stock`         | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/products/lots/expired`                              | ADMIN, SUPERVISEUR             |
| GET    | `/products/lots/near-expiration?daysThreshold=N`      | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/products/lots/product/{productId}/available`        | ADMIN, SUPERVISEUR, DELEGUE    |
| POST   | `/products/lots/lot`                                  | ADMIN, SUPERVISEUR             |
| PUT    | `/products/lots/product/{productId}/adjust-stock?quantityChange=N` | ADMIN, SUPERVISEUR |
| PUT    | `/products/lots/lot/{numeroLot}/update-quantity?quantityChange=N`  | ADMIN, SUPERVISEUR |
| DELETE | `/products/lots/lot/{numeroLot}`                      | ADMIN                          |

#### 2.2.3 Promotions (`/products/promos`)

| Method | Gateway Path                                          | Roles                          |
|--------|-------------------------------------------------------|--------------------------------|
| GET    | `/products/promos`                                    | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/products/promos/{promotionId}`                      | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/products/promos/active-count`                       | ADMIN, SUPERVISEUR             |
| GET    | `/products/promos/coverage-rate`                      | ADMIN, SUPERVISEUR             |
| GET    | `/products/promos/{promotionId}/valid`                | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/products/promos/{promotionId}/applicable?referenceDate=` | ADMIN, SUPERVISEUR, DELEGUE |
| GET    | `/products/promos/product/{productId}`                | (auth)                         |
| GET    | `/products/promos/product/{productId}/in-promotion`   | (auth)                         |
| GET    | `/products/promos/product/{productId}/apply?initialPrice=` | (auth)                    |
| GET    | `/products/promos/lot/{numeroLot}`                    | ADMIN, SUPERVISEUR, DELEGUE    |
| POST   | `/products/promos`                                    | ADMIN, SUPERVISEUR             |
| DELETE | `/products/promos/{promotionId}`                      | ADMIN                          |

#### 2.2.4 Marketting (`/products/marketting` OR `/marketting`)

Note: gateway exposes the same downstream both as `/marketting/{everything}` and `/products/marketting/{everything}`. Prefer `/marketting/...` (matches existing `MarketingService`).

| Method | Gateway Path                                            | Roles                          |
|--------|---------------------------------------------------------|--------------------------------|
| GET    | `/marketting/product/{productId}/supports`              | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/marketting/support/{supportId}`                       | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/marketting/support/{supportId}/active`                | (auth)                         |
| GET    | `/marketting/support/{supportId}/files`                 | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/marketting/product/{productId}/visible-supports`      | (auth)                         |
| GET    | `/marketting/campaign/{campaignName}`                   | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/marketting/campaigns`                                 | ADMIN, SUPERVISEUR, DELEGUE    |
| POST   | `/marketting/support`                                   | ADMIN, SUPERVISEUR             |
| POST   | `/marketting/support/file`                              | ADMIN, SUPERVISEUR             |
| PUT    | `/marketting/support/{supportId}/activate`              | ADMIN                          |
| PUT    | `/marketting/support/{supportId}/disable`               | ADMIN                          |
| DELETE | `/marketting/file/{fichierId}`                          | ADMIN                          |

### 2.3 OrderAPI — port 7004

#### 2.3.1 Orders (`/orders`)

| Method | Gateway Path                                | Roles                          |
|--------|---------------------------------------------|--------------------------------|
| GET    | `/orders?page=&pageSize=`                   | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/orders/{orderId}`                         | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/orders/by-client/{clientId}`              | ADMIN, SUPERVISEUR, DELEGUE    |
| POST   | `/orders`                                   | CLIENT                         |
| PUT    | `/orders/status`                            | (auth)                         |
| DELETE | `/orders/{idCommande}`                      | ADMIN                          |

#### 2.3.2 Lignes commande (`/orders/lignes`)

| Method | Gateway Path                                | Roles                          |
|--------|---------------------------------------------|--------------------------------|
| POST   | `/orders/lignes`                            | CLIENT                         |
| DELETE | `/orders/lignes/{ligneId}`                  | ADMIN, SUPERVISEUR, DELEGUE    |

#### 2.3.3 Reclamations (`/orders/reclamations`)

| Method | Gateway Path                                          | Roles                          |
|--------|-------------------------------------------------------|--------------------------------|
| GET    | `/orders/reclamations`                                | ADMIN, SUPERVISEUR             |
| GET    | `/orders/reclamations/{idReclamation}`                | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/orders/reclamations/by-commande/{orderId}`          | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/orders/reclamations/by-client/{idClient}`           | ADMIN, SUPERVISEUR, DELEGUE    |
| POST   | `/orders/reclamations`                                | CLIENT                         |
| PUT    | `/orders/reclamations/{reclamationId}/status`         | ADMIN, SUPERVISEUR             |
| DELETE | `/orders/reclamations/{reclamationId}`                | ADMIN                          |

### 2.4 InventoryAPI — port 7003

#### 2.4.1 Distributions (`/inventory/distributions`)

| Method | Gateway Path                                                | Roles                          |
|--------|-------------------------------------------------------------|--------------------------------|
| POST   | `/inventory/distributions/distribution`                     | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/inventory/distributions/{idDistribution}`                 | (auth)                         |
| GET    | `/inventory/distributions/by-medecin/{idMedecin}`           | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/inventory/distributions/by-delegue/{idDelegue}`           | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/inventory/distributions/by-pharmacien/{idPharmacien}`     | ADMIN, SUPERVISEUR, DELEGUE    |
| DELETE | `/inventory/distributions/{idDistribution}`                 | ADMIN, SUPERVISEUR             |

#### 2.4.2 Inventory Business (`/inventory/inventory-business`)

| Method | Gateway Path                                                                          | Roles                          |
|--------|---------------------------------------------------------------------------------------|--------------------------------|
| GET    | `/inventory/inventory-business/check-availability?idStock=&qte=`                      | ADMIN, SUPERVISEUR, DELEGUE    |
| POST   | `/inventory/inventory-business/distribute-echantillon?idDelegue=&idPharmacien=&idMedecin=&idStock=&qte=` | ADMIN, SUPERVISEUR, DELEGUE |
| POST   | `/inventory/inventory-business/apply-gratuite?idStock=&quantiteAchetee=&seuilPromo=` | ADMIN, SUPERVISEUR            |
| POST   | `/inventory/inventory-business/reserve-stock?idStock=&quantite=`                     | ADMIN, SUPERVISEUR             |

#### 2.4.3 Stock Movements (`/inventory/stock-movements`)

| Method | Gateway Path                                                                          | Roles                          |
|--------|---------------------------------------------------------------------------------------|--------------------------------|
| POST   | `/inventory/stock-movements/decrement?idStock=&qte=`                                  | ADMIN, SUPERVISEUR             |
| POST   | `/inventory/stock-movements/increment?idStock=&qte=`                                  | ADMIN, SUPERVISEUR             |
| POST   | `/inventory/stock-movements/transfer?idStockSource=&idStockDestination=&qte=`         | ADMIN, SUPERVISEUR             |
| GET    | `/inventory/stock-movements/{idStock}`                                                | ADMIN, SUPERVISEUR             |

#### 2.4.4 Stocks Promotionnels (`/inventory/stocks-promotionnels`)

| Method | Gateway Path                                                  | Roles                          |
|--------|---------------------------------------------------------------|--------------------------------|
| POST   | `/inventory/stocks-promotionnels/Gratuite`                    | (auth)                         |
| GET    | `/inventory/stocks-promotionnels/gratuite/{idStock}`          | ADMIN, SUPERVISEUR, DELEGUE    |
| POST   | `/inventory/stocks-promotionnels/echantillon`                 | ADMIN, SUPERVISEUR             |
| GET    | `/inventory/stocks-promotionnels/echantillon/{idStock}`       | ADMIN, SUPERVISEUR, DELEGUE    |

#### 2.4.5 Stocks Délégués (`/inventory/stocks-delegue`)

| Method | Gateway Path                                              | Roles                          |
|--------|-----------------------------------------------------------|--------------------------------|
| GET    | `/inventory/stocks-delegue?pageNumber=&pageSize=`         | ADMIN, SUPERVISEUR             |
| GET    | `/inventory/stocks-delegue/{idStock}`                     | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/inventory/stocks-delegue/by-delegue/{idDelegue}`        | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/inventory/stocks-delegue/by-produit/{idProduit}`        | ADMIN, SUPERVISEUR             |
| GET    | `/inventory/stocks-delegue/by-lot/{numeroLot}`            | ADMIN, SUPERVISEUR             |
| POST   | `/inventory/stocks-delegue/stock`                         | ADMIN, SUPERVISEUR             |
| DELETE | `/inventory/stocks-delegue/{idStock}?type=`               | ADMIN                          |

### 2.5 FieldAPI — port 7002

#### 2.5.1 KPI (`/fields/kpi`)

| Method | Gateway Path                                                       | Roles                          |
|--------|--------------------------------------------------------------------|--------------------------------|
| GET    | `/fields/kpi/visites-count?idDelegue=&debut=&fin=`                 | ADMIN, SUPERVISEUR             |
| GET    | `/fields/kpi/has-visite?idDelegue=&date=`                          | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/fields/kpi/historique/{idDelegue}`                               | ADMIN, SUPERVISEUR             |
| GET    | `/fields/kpi/client-fidelite/{idClient}`                           | ADMIN, SUPERVISEUR             |
| GET    | `/fields/kpi/performance/{idDelegue}`                              | ADMIN, SUPERVISEUR             |
| GET    | `/fields/kpi/performance-rate/{idDelegue}`                         | ADMIN, SUPERVISEUR             |

#### 2.5.2 Objectifs (`/fields/objectifs`)

| Method | Gateway Path                                          | Roles                          |
|--------|-------------------------------------------------------|--------------------------------|
| GET    | `/fields/objectifs`                                   | ADMIN, SUPERVISEUR             |
| GET    | `/fields/objectifs/{idObjectif}`                      | ADMIN, SUPERVISEUR             |
| GET    | `/fields/objectifs/by-delegue/{idDelegue}`            | ADMIN, SUPERVISEUR             |
| POST   | `/fields/objectifs`                                   | ADMIN, SUPERVISEUR             |
| PUT    | `/fields/objectifs/{idObjectif}/value?nouvelleValeur=`| ADMIN, SUPERVISEUR             |
| DELETE | `/fields/objectifs/{idObjectif}`                      | ADMIN                          |

#### 2.5.3 Plannings (`/fields/plannings`)

| Method | Gateway Path                                                              | Roles                          |
|--------|---------------------------------------------------------------------------|--------------------------------|
| GET    | `/fields/plannings/{idPlanning}`                                          | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/fields/plannings/by-delegue/{idDelegue}`                                | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/fields/plannings/by-range?idDelegue=&startDate=&endDate=`               | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/fields/plannings/by-date?idDelegue=&date=`                              | ADMIN, SUPERVISEUR, DELEGUE    |
| POST   | `/fields/plannings`                                                       | ADMIN, SUPERVISEUR, DELEGUE    |
| PUT    | `/fields/plannings/{idPlanning}/validate`                                 | ADMIN, SUPERVISEUR             |
| DELETE | `/fields/plannings/{idPlanning}`                                          | ADMIN, SUPERVISEUR             |

#### 2.5.4 Rapports (`/fields/rapports`)

| Method | Gateway Path                                              | Roles                          |
|--------|-----------------------------------------------------------|--------------------------------|
| GET    | `/fields/rapports/all`                                    | (auth)                         |
| GET    | `/fields/rapports/{id}`                                   | (auth)                         |
| GET    | `/fields/rapports/by-visite/{idVisite}`                   | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/fields/rapports/can-create/{idVisite}`                  | DELEGUE                        |
| GET    | `/fields/rapports/has-rapport/{idVisite}`                 | ADMIN, SUPERVISEUR, DELEGUE    |
| POST   | `/fields/rapports/createUpdate`                           | (auth)                         |
| PUT    | `/fields/rapports/{idRapport}/validate?idSuperviseur=`    | SUPERVISEUR                    |
| DELETE | `/fields/rapports/{idRapport}`                            | DELEGUE, ADMIN                 |

#### 2.5.5 Régions (`/fields/regions`)

| Method | Gateway Path                                          | Roles                          |
|--------|-------------------------------------------------------|--------------------------------|
| GET    | `/fields/regions/all`                                 | (auth)                         |
| GET    | `/fields/regions/{idRegion}`                          | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/fields/regions/by-delegue/{idDelegue}`              | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/fields/regions/count/{idDelegue}`                   | ADMIN, SUPERVISEUR             |
| POST   | `/fields/regions`                                     | ADMIN, SUPERVISEUR             |
| DELETE | `/fields/regions/{idRegion}`                          | ADMIN                          |

#### 2.5.6 Visites (`/fields/visites`)

| Method | Gateway Path                                          | Roles                          |
|--------|-------------------------------------------------------|--------------------------------|
| GET    | `/fields/visites/{idVisite}`                          | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/fields/visites/by-delegue/{idDelegue}`              | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/fields/visites/by-planning/{idPlanning}`            | ADMIN, SUPERVISEUR, DELEGUE    |
| POST   | `/fields/visites`                                     | DELEGUE, ADMIN, SUPERVISEUR    |
| PUT    | `/fields/visites/{idVisite}/planning/{idPlanning}`    | DELEGUE                        |
| PUT    | `/fields/visites/{idVisite}/complete`                 | DELEGUE                        |
| DELETE | `/fields/visites/{idVisite}`                          | ADMIN, SUPERVISEUR             |

### 2.6 DocAPI — port 7001

#### 2.6.1 Documents (`/documents`)

| Method | Gateway Path                                          | Roles                          |
|--------|-------------------------------------------------------|--------------------------------|
| GET    | `/documents?pageNumber=&pageSize=`                    | ADMIN, SUPERVISEUR             |
| GET    | `/documents/{numeroDoc}`                              | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/documents/client/{idClient}`                        | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/documents/commande/{idCommande}`                    | ADMIN, SUPERVISEUR, DELEGUE    |
| POST   | `/documents/document`                                 | ADMIN, SUPERVISEUR             |
| DELETE | `/documents/{numeroDoc}`                              | ADMIN                          |

#### 2.6.2 Bons de commande (`/documents/bons-commandes`)

| Method | Gateway Path                                                  | Roles                          |
|--------|---------------------------------------------------------------|--------------------------------|
| GET    | `/documents/bons-commandes?pageNumber=&pageSize=`             | ADMIN, SUPERVISEUR             |
| GET    | `/documents/bons-commandes/{id}`                              | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/documents/bons-commandes/client/{idClient}`                 | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/documents/bons-commandes/by-date?startDate=&endDate=`       | ADMIN, SUPERVISEUR             |
| POST   | `/documents/bons-commandes/createUpdate`                      | ADMIN, SUPERVISEUR             |

#### 2.6.3 Bons de livraison (`/documents/bons-livraison`)

| Method | Gateway Path                                                  | Roles                          |
|--------|---------------------------------------------------------------|--------------------------------|
| GET    | `/documents/bons-livraison?pageNumber=&pageSize=`             | ADMIN, SUPERVISEUR             |
| GET    | `/documents/bons-livraison/{id}`                              | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/documents/bons-livraison/ByClient/{idClient}`               | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/documents/bons-livraison/by-date?startDate=&endDate=`       | ADMIN, SUPERVISEUR             |
| POST   | `/documents/bons-livraison/createUpdate`                      | ADMIN, SUPERVISEUR             |

#### 2.6.4 Factures (`/documents/factures`)

| Method | Gateway Path                                                  | Roles                          |
|--------|---------------------------------------------------------------|--------------------------------|
| GET    | `/documents/factures?pageNumber=&pageSize=`                   | ADMIN, SUPERVISEUR             |
| GET    | `/documents/factures/{id}`                                    | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/documents/factures/client/{idClient}`                       | ADMIN, SUPERVISEUR, DELEGUE    |
| GET    | `/documents/factures/by-date?startDate=&endDate=`             | ADMIN, SUPERVISEUR             |
| POST   | `/documents/factures/createUpdate`                            | ADMIN, SUPERVISEUR             |

---

## 3. Gap Analysis (Front vs Back)

| Domain                  | Backend Controller             | Angular Status      | Action |
|-------------------------|--------------------------------|---------------------|--------|
| Auth                    | AuthController                 | EXISTS (auth feature) | None — already covered |
| Dashboard               | (from products/orders/inv.)    | EXISTS              | None |
| Products                | ProductController              | EXISTS              | None |
| Lots                    | LotController                  | EXISTS              | None |
| Orders                  | OrderController                | EXISTS              | None |
| Order Lines             | LigneController                | PARTIAL (handled inside OrderForm) | **EXTEND** orders module — add a `LigneService` and inline ligne management |
| Reclamations            | ReclamationController          | MISSING             | **NEW** sub-module under orders |
| Marketing supports      | MarkettingController           | EXISTS (marketing feature) | None — verify campaigns sub-views |
| Promotions              | PromoController                | PARTIAL (PromotionAdvancedService inside products) | **NEW** dedicated `promotions` feature module (CRUD + analytics views: coverage rate, active count, applicable check) |
| Users                   | AuthController                 | EXISTS              | None |
| Inventory: Distributions| DistributionController         | MISSING             | **NEW** module |
| Inventory: Stock Mvts   | StockMovementController        | MISSING             | **NEW** module |
| Inventory: Promo stocks | StockPromotionnelController    | MISSING             | **NEW** module |
| Inventory: Stocks delegue | StocksDelegueController      | MISSING             | **NEW** module (this is the "rep stock" view) |
| Inventory: Business ops | InventoryBusinessController    | MISSING             | **NEW** — group of action buttons inside the stocks-delegue detail page (no list view; pure ops) |
| Field: KPI              | KPIController                  | MISSING             | **NEW** module — analytics dashboards |
| Field: Objectifs        | ObjectifController             | MISSING             | **NEW** module |
| Field: Plannings        | PlanningVisiteController       | MISSING             | **NEW** module |
| Field: Rapports         | RapportsController             | MISSING             | **NEW** module |
| Field: Régions          | RegionController               | MISSING             | **NEW** module |
| Field: Visites          | VisitesController              | MISSING             | **NEW** module |
| Documents (general)     | DocumentsController            | MISSING             | **NEW** module |
| Bons de commande        | BonsCommandesController        | MISSING             | **NEW** sub-module |
| Bons de livraison       | BonsLivraisonsController       | MISSING             | **NEW** sub-module |
| Factures                | FacturesController             | MISSING             | **NEW** sub-module |

---

## 4. New Feature Modules (Detailed Spec)

For every new module, the convention is:
- Folder: `src/app/features/<name>/`
- Lazy-loaded route module
- Service in `<name>/services/<entity>.service.ts` calling the gateway via the existing `ApiService` (or `HttpClient` directly with `environment.apiBaseUrl`)
- All HTTP returns are wrapped in `ResponseDto<T> = { isSuccess, message, result }` — reuse the existing typed wrapper if present in `core/models`.

### 4.1 Promotions (split out from products)

- Folder: `src/app/features/promotions/`
- Components: `PromotionListComponent`, `PromotionDetailComponent`, `PromotionFormComponent`, `PromotionAnalyticsComponent` (coverage-rate + active-count cards)
- Service: `PromotionService`
- Service responsibilities:
  - `getAll()` → GET `/products/promos`
  - `getById(id)` → GET `/products/promos/{id}`
  - `createOrUpdate(dto)` → POST `/products/promos`
  - `delete(id)` → DELETE `/products/promos/{id}`
  - `getActiveCount()` → GET `/products/promos/active-count`
  - `getCoverageRate()` → GET `/products/promos/coverage-rate`
  - `isValid(id)` → GET `/products/promos/{id}/valid`
  - `isApplicable(id, refDate)` → GET `/products/promos/{id}/applicable?referenceDate=`
  - `getByProduct(productId)` → GET `/products/promos/product/{productId}`
  - `isProductInPromotion(productId)` → GET `/products/promos/product/{productId}/in-promotion`
  - `applyBest(productId, initialPrice)` → GET `/products/promos/product/{productId}/apply?initialPrice=`
  - `getByLot(numeroLot)` → GET `/products/promos/lot/{numeroLot}`
- Routes: `/promotions`, `/promotions/new`, `/promotions/:id`, `/promotions/:id/edit`, `/promotions/analytics`

### 4.2 Reclamations (under orders feature)

- Folder: `src/app/features/orders/reclamations/`
- Components: `ReclamationListComponent`, `ReclamationDetailComponent`, `ReclamationFormComponent`, `ReclamationStatusBadgeComponent` (shared visual)
- Service: `ReclamationService`
- Endpoints called:
  - `getAll()` → GET `/orders/reclamations`
  - `getById(id)` → GET `/orders/reclamations/{id}`
  - `getByOrder(orderId)` → GET `/orders/reclamations/by-commande/{orderId}`
  - `getByClient(clientId)` → GET `/orders/reclamations/by-client/{clientId}`
  - `createOrUpdate(dto)` → POST `/orders/reclamations`
  - `updateStatus(id, status)` → PUT `/orders/reclamations/{id}/status`
  - `delete(id)` → DELETE `/orders/reclamations/{id}`
- Routes (registered inside orders routing): `/orders/reclamations`, `/orders/reclamations/new`, `/orders/reclamations/:id`, `/orders/reclamations/:id/edit`

### 4.3 Inventory module (umbrella)

Create a top-level inventory feature module that lazy-loads the four sub-features below.

- Folder: `src/app/features/inventory/`
- Sub-routes: `stocks`, `movements`, `distributions`, `promo-stocks`

#### 4.3.1 Stocks (Stocks Délégués)

- Folder: `src/app/features/inventory/stocks/`
- Components: `StockListComponent`, `StockDetailComponent`, `StockFormComponent`, `StockOpsPanelComponent` (reservation / availability check / gratuite / distribute echantillon, calls InventoryBusiness)
- Service: `StockService`
  - `getAll(page, pageSize)` → GET `/inventory/stocks-delegue?pageNumber=&pageSize=`
  - `getById(idStock)` → GET `/inventory/stocks-delegue/{idStock}`
  - `getByDelegue(idDelegue)` → GET `/inventory/stocks-delegue/by-delegue/{idDelegue}`
  - `getByProduit(idProduit)` → GET `/inventory/stocks-delegue/by-produit/{idProduit}`
  - `getByLot(numeroLot)` → GET `/inventory/stocks-delegue/by-lot/{numeroLot}`
  - `createOrUpdate(dto)` → POST `/inventory/stocks-delegue/stock`
  - `delete(idStock, type)` → DELETE `/inventory/stocks-delegue/{idStock}?type=`
- Service: `InventoryBusinessService` (no list view; ops only)
  - `checkAvailability(idStock, qte)` → GET `/inventory/inventory-business/check-availability`
  - `distributeEchantillon(...)` → POST `/inventory/inventory-business/distribute-echantillon`
  - `applyGratuite(...)` → POST `/inventory/inventory-business/apply-gratuite`
  - `reserveStock(idStock, qte)` → POST `/inventory/inventory-business/reserve-stock`

#### 4.3.2 Stock Movements

- Folder: `src/app/features/inventory/movements/`
- Components: `MovementListComponent` (per-stock history), `MovementOpsComponent` (decrement / increment / transfer triggers)
- Service: `StockMovementService`
  - `getMovements(idStock)` → GET `/inventory/stock-movements/{idStock}`
  - `decrement(idStock, qte)` → POST `/inventory/stock-movements/decrement?idStock=&qte=`
  - `increment(idStock, qte)` → POST `/inventory/stock-movements/increment?idStock=&qte=`
  - `transfer(idStockSource, idStockDestination, qte)` → POST `/inventory/stock-movements/transfer?...`

#### 4.3.3 Distributions

- Folder: `src/app/features/inventory/distributions/`
- Components: `DistributionListComponent` (filters: by médecin / by délégué / by pharmacien), `DistributionDetailComponent`, `DistributionFormComponent`
- Service: `DistributionService`
  - `getById(id)` → GET `/inventory/distributions/{id}`
  - `getByMedecin(id)` → GET `/inventory/distributions/by-medecin/{id}`
  - `getByDelegue(id)` → GET `/inventory/distributions/by-delegue/{id}`
  - `getByPharmacien(id)` → GET `/inventory/distributions/by-pharmacien/{id}`
  - `createOrUpdate(echantillon)` → POST `/inventory/distributions/distribution`
  - `delete(id)` → DELETE `/inventory/distributions/{id}`

#### 4.3.4 Promo Stocks (Gratuités & Echantillons)

- Folder: `src/app/features/inventory/promo-stocks/`
- Components: `GratuiteFormComponent`, `EchantillonFormComponent`, `PromoStockDetailComponent` (lookup by idStock)
- Service: `PromoStockService`
  - `createOrUpdateGratuite(dto)` → POST `/inventory/stocks-promotionnels/Gratuite`
  - `getGratuite(idStock)` → GET `/inventory/stocks-promotionnels/gratuite/{idStock}`
  - `createOrUpdateEchantillon(dto)` → POST `/inventory/stocks-promotionnels/echantillon`
  - `getEchantillon(idStock)` → GET `/inventory/stocks-promotionnels/echantillon/{idStock}`

### 4.4 Field module (umbrella)

- Folder: `src/app/features/field/`
- Sub-features: `visites`, `plannings`, `rapports`, `objectifs`, `regions`, `kpi`

#### 4.4.1 Visites

- Components: `VisiteListComponent`, `VisiteDetailComponent`, `VisiteFormComponent`, `VisiteCalendarComponent` (optional)
- Service: `VisiteService`
  - `getById(id)` → GET `/fields/visites/{id}`
  - `getByDelegue(idDelegue)` → GET `/fields/visites/by-delegue/{idDelegue}`
  - `getByPlanning(idPlanning)` → GET `/fields/visites/by-planning/{idPlanning}`
  - `createOrUpdate(dto)` → POST `/fields/visites`
  - `affectToPlanning(idVisite, idPlanning)` → PUT `/fields/visites/{idVisite}/planning/{idPlanning}`
  - `complete(id)` → PUT `/fields/visites/{id}/complete`
  - `delete(id)` → DELETE `/fields/visites/{id}`

#### 4.4.2 Plannings

- Components: `PlanningListComponent`, `PlanningDetailComponent`, `PlanningFormComponent`, `PlanningCalendarViewComponent`
- Service: `PlanningService`
  - `getById(id)` → GET `/fields/plannings/{id}`
  - `getByDelegue(idDelegue)` → GET `/fields/plannings/by-delegue/{idDelegue}`
  - `getByRange(idDelegue, start, end)` → GET `/fields/plannings/by-range`
  - `getByDelegueAndDate(idDelegue, date)` → GET `/fields/plannings/by-date`
  - `createOrUpdate(dto)` → POST `/fields/plannings`
  - `validate(id)` → PUT `/fields/plannings/{id}/validate`
  - `delete(id)` → DELETE `/fields/plannings/{id}`

#### 4.4.3 Rapports

- Components: `RapportListComponent`, `RapportDetailComponent`, `RapportFormComponent`, `RapportValidationButtonComponent`
- Service: `RapportService`
  - `getAll()` → GET `/fields/rapports/all`
  - `getById(id)` → GET `/fields/rapports/{id}`
  - `getByVisite(idVisite)` → GET `/fields/rapports/by-visite/{idVisite}`
  - `canCreate(idVisite)` → GET `/fields/rapports/can-create/{idVisite}`
  - `hasRapport(idVisite)` → GET `/fields/rapports/has-rapport/{idVisite}`
  - `createOrUpdate(dto)` → POST `/fields/rapports/createUpdate`
  - `validate(id, idSuperviseur)` → PUT `/fields/rapports/{id}/validate?idSuperviseur=`
  - `delete(id)` → DELETE `/fields/rapports/{id}`

#### 4.4.4 Objectifs

- Components: `ObjectifListComponent`, `ObjectifDetailComponent`, `ObjectifFormComponent`, `ObjectifProgressBarComponent` (shared)
- Service: `ObjectifService`
  - `getAll()` → GET `/fields/objectifs`
  - `getById(id)` → GET `/fields/objectifs/{id}`
  - `getByDelegue(idDelegue)` → GET `/fields/objectifs/by-delegue/{idDelegue}`
  - `createOrUpdate(dto)` → POST `/fields/objectifs`
  - `updateValue(id, nouvelleValeur)` → PUT `/fields/objectifs/{id}/value?nouvelleValeur=`
  - `delete(id)` → DELETE `/fields/objectifs/{id}`

#### 4.4.5 Régions

- Components: `RegionListComponent`, `RegionDetailComponent`, `RegionFormComponent`
- Service: `RegionService`
  - `getAll()` → GET `/fields/regions/all`
  - `getById(id)` → GET `/fields/regions/{id}`
  - `getByDelegue(idDelegue)` → GET `/fields/regions/by-delegue/{idDelegue}`
  - `getNombreRegionsCouvre(idDelegue)` → GET `/fields/regions/count/{idDelegue}`
  - `createOrUpdate(dto)` → POST `/fields/regions`
  - `delete(id)` → DELETE `/fields/regions/{id}`

#### 4.4.6 KPI

- Components: `KpiDashboardComponent`, `KpiDelegueCardComponent`, `KpiClientFideliteComponent`
- Service: `KpiService`
  - `getNombreVisites(idDelegue, debut, fin)` → GET `/fields/kpi/visites-count`
  - `hasVisiteAtDate(idDelegue, date)` → GET `/fields/kpi/has-visite`
  - `getHistorique(idDelegue)` → GET `/fields/kpi/historique/{idDelegue}`
  - `getClientFidelite(idClient)` → GET `/fields/kpi/client-fidelite/{idClient}`
  - `getPerformance(idDelegue)` → GET `/fields/kpi/performance/{idDelegue}`
  - `getPerformanceRate(idDelegue)` → GET `/fields/kpi/performance-rate/{idDelegue}`

### 4.5 Documents module (umbrella)

- Folder: `src/app/features/documents/`
- Sub-features: `documents` (generic), `bons-commandes`, `bons-livraison`, `factures`

#### 4.5.1 Documents (generic)

- Components: `DocumentListComponent`, `DocumentDetailComponent`, `DocumentFormComponent`
- Service: `DocumentService`
  - `getAll(page, pageSize)` → GET `/documents`
  - `getById(numeroDoc)` → GET `/documents/{numeroDoc}`
  - `getByClient(idClient)` → GET `/documents/client/{idClient}`
  - `getByCommande(idCommande)` → GET `/documents/commande/{idCommande}`
  - `createOrUpdate(dto)` → POST `/documents/document`
  - `delete(id)` → DELETE `/documents/{id}`

#### 4.5.2 Bons de commande

- Components: `BonCommandeListComponent`, `BonCommandeDetailComponent`, `BonCommandeFormComponent`
- Service: `BonCommandeService`
  - `getAll(page, pageSize)` → GET `/documents/bons-commandes`
  - `getById(id)` → GET `/documents/bons-commandes/{id}`
  - `getByClient(idClient)` → GET `/documents/bons-commandes/client/{idClient}`
  - `getByDate(start, end)` → GET `/documents/bons-commandes/by-date`
  - `createOrUpdate(dto)` → POST `/documents/bons-commandes/createUpdate`

#### 4.5.3 Bons de livraison

- Components: `BonLivraisonListComponent`, `BonLivraisonDetailComponent`, `BonLivraisonFormComponent`
- Service: `BonLivraisonService`
  - `getAll(page, pageSize)` → GET `/documents/bons-livraison`
  - `getById(id)` → GET `/documents/bons-livraison/{id}`
  - `getByClient(idClient)` → GET `/documents/bons-livraison/ByClient/{idClient}`
  - `getByDate(start, end)` → GET `/documents/bons-livraison/by-date`
  - `createOrUpdate(dto)` → POST `/documents/bons-livraison/createUpdate`

#### 4.5.4 Factures

- Components: `FactureListComponent`, `FactureDetailComponent`, `FactureFormComponent`, `FacturePrintViewComponent`
- Service: `FactureService`
  - `getAll(page, pageSize)` → GET `/documents/factures`
  - `getById(id)` → GET `/documents/factures/{id}`
  - `getByClient(idClient)` → GET `/documents/factures/client/{idClient}`
  - `getByDate(start, end)` → GET `/documents/factures/by-date`
  - `createOrUpdate(dto)` → POST `/documents/factures/createUpdate`

---

## 5. Existing Module Extensions

| Existing module | Add | Reason |
|-----------------|-----|--------|
| `features/orders/` | `LigneService` (POST `/orders/lignes`, DELETE `/orders/lignes/{ligneId}`) and a `LigneEditorComponent` reused in OrderForm | Order line CRUD is currently inlined and lacks a clean abstraction for delete/update flows |
| `features/orders/` | Reclamation sub-feature (see §4.2) | Backend exists; frontend missing |
| `features/products/` | Verify `PromotionAdvancedService` is removed/refactored once the dedicated `promotions` feature module ships | Avoid duplicated promotion logic |
| `features/marketing/` | Add `CampaignsListComponent` if not already present (uses `/marketting/campaigns`, `/marketting/campaign/{name}`) | Backend exposes campaign endpoints not yet visible in UI |
| `features/dashboard/` | Add tiles backed by `KpiService.getPerformance` and `ProductService.getProductDashboard` | Lift metrics to a single landing page |

---

## 6. Shared / Core Additions

### Shared (`src/app/shared/`)

- `pipes/`
  - `RoleBadgePipe` — maps `ADMIN | SUPERVISEUR | DELEGUE | CLIENT | MEDECIN` to colored chip
  - `OrderStatusPipe`, `ReclamationStatusPipe`, `VisiteStatusPipe`, `PlanningStatusPipe`, `DocumentTypePipe`
  - `BooleanYesNoPipe`
- `components/`
  - `ConfirmDialogComponent` (reusable yes/no modal — used by every Delete action)
  - `PaginatorComponent` (works with backend `?pageNumber=&pageSize=`)
  - `DateRangePickerComponent` (used by KPI / Factures / BC / BL filters)
  - `EmptyStateComponent`
  - `StatusChipComponent`
  - `KpiCardComponent` (small numeric card)
  - `ChartCardComponent` (wraps a chart lib like ng2-charts)
- `directives/`
  - `RoleVisibleDirective` — `*appRoleVisible="['ADMIN','SUPERVISEUR']"`

### Core (`src/app/core/`)

- `models/response-dto.model.ts` — generic `ResponseDto<T>` matching backend (already partially in code; verify)
- `models/page-result.model.ts` — `{ items, pageNumber, pageSize, total }`
- `models/enums/` — `UserRole`, `OrderStatus`, `StatutReclamation`, `StockType`, `VisiteStatus`, `PlanningStatus`, `DocumentType`, `ObjectifStatus`
- `services/api-error.service.ts` — central toast on 4xx/5xx (already partially in `ErrorInterceptor`; ensure consistency)
- `guards/role.guard.ts` — already exists; verify it supports the new roles per route

---

## 7. Navigation / Sidebar Updates

Update `src/app/layouts/main-layout/sidebar/` (or wherever the menu lives) to add the following entries grouped by domain.

```text
Dashboard
Products
  ├─ Catalogue
  ├─ Categories
  └─ Lots
Promotions                         (NEW)
  ├─ Liste
  └─ Analytics
Marketing
  ├─ Supports
  └─ Campagnes
Orders
  ├─ Commandes
  └─ Réclamations                  (NEW)
Inventory                          (NEW group)
  ├─ Stocks délégués
  ├─ Mouvements de stock
  ├─ Distributions
  └─ Stocks promotionnels (Gratuités / Échantillons)
Field                              (NEW group)
  ├─ Visites
  ├─ Plannings
  ├─ Rapports
  ├─ Objectifs
  ├─ Régions
  └─ KPI
Documents                          (NEW group)
  ├─ Documents généraux
  ├─ Bons de commande
  ├─ Bons de livraison
  └─ Factures
Users
  ├─ Liste
  └─ Désactivés
Settings (profile, change-password)
```

Each entry uses `*appRoleVisible` to gate visibility per role (refer to §2 role columns).

---

## 8. Implementation Order (recommended)

| Phase | Module(s) | Why |
|-------|-----------|-----|
| 1 | **Promotions** (split out of products) | Quickest win, isolates already-half-done logic, unblocks product detail cleanup |
| 2 | **Reclamations** (under orders) | Closes a visible gap in the Orders flow; small surface; depends only on existing OrderService |
| 3 | **Inventory umbrella** (Stocks → Movements → Distributions → Promo-stocks) | Foundational data needed by Field and Documents modules; build in this internal order so Stocks exists before Movements references it |
| 4 | **Field umbrella** (Régions → Objectifs → Plannings → Visites → Rapports → KPI) | Régions and Objectifs are referenced by Plannings/Visites; KPI consumes the others |
| 5 | **Documents umbrella** (Documents → BonsCommandes → BonsLivraisons → Factures) | Depends on Orders + Clients (exists) and on Stocks (Phase 3) for some references |
| 6 | **Shared/Core polish** | Refactor toward `ConfirmDialog`, `Paginator`, `DateRangePicker` once usage patterns are clear from earlier phases |
| 7 | **Sidebar + Dashboard tiles** | Final integration once all routes resolve |

Rationale: build foundations before consumers; ship a small, demoable phase early (Phase 1) to validate the gateway base-URL config and the auth/role flow on a brand-new feature.

---

## 9. Angular CLI Generation Commands

Run from `C:\Cynapharm\CynapSoftCRMBackend\Cynapharm\` (project root with `angular.json`).

> All modules use `--routing=true --route=...` style only when registered in `app-routing.module.ts`. The commands below assume each new module file owns its own children routes via a `Routes` const inside the module.

### 9.1 Phase 1 — Promotions

```bash
ng generate module features/promotions/promotions --module app
ng generate component features/promotions/promotion-list
ng generate component features/promotions/promotion-detail
ng generate component features/promotions/promotion-form
ng generate component features/promotions/promotion-analytics
ng generate service  features/promotions/services/promotion
```

### 9.2 Phase 2 — Reclamations

```bash
ng generate module features/orders/reclamations/reclamations --module ../orders.module
ng generate component features/orders/reclamations/reclamation-list
ng generate component features/orders/reclamations/reclamation-detail
ng generate component features/orders/reclamations/reclamation-form
ng generate service  features/orders/reclamations/services/reclamation
ng generate service  features/orders/services/ligne
ng generate component features/orders/components/ligne-editor
```

### 9.3 Phase 3 — Inventory

```bash
ng generate module features/inventory/inventory --module app
ng generate module features/inventory/stocks/stocks --module ../inventory.module
ng generate component features/inventory/stocks/stock-list
ng generate component features/inventory/stocks/stock-detail
ng generate component features/inventory/stocks/stock-form
ng generate component features/inventory/stocks/stock-ops-panel
ng generate service  features/inventory/stocks/services/stock
ng generate service  features/inventory/stocks/services/inventory-business

ng generate module features/inventory/movements/movements --module ../inventory.module
ng generate component features/inventory/movements/movement-list
ng generate component features/inventory/movements/movement-ops
ng generate service  features/inventory/movements/services/stock-movement

ng generate module features/inventory/distributions/distributions --module ../inventory.module
ng generate component features/inventory/distributions/distribution-list
ng generate component features/inventory/distributions/distribution-detail
ng generate component features/inventory/distributions/distribution-form
ng generate service  features/inventory/distributions/services/distribution

ng generate module features/inventory/promo-stocks/promo-stocks --module ../inventory.module
ng generate component features/inventory/promo-stocks/gratuite-form
ng generate component features/inventory/promo-stocks/echantillon-form
ng generate component features/inventory/promo-stocks/promo-stock-detail
ng generate service  features/inventory/promo-stocks/services/promo-stock
```

### 9.4 Phase 4 — Field

```bash
ng generate module features/field/field --module app

ng generate module features/field/regions/regions --module ../field.module
ng generate component features/field/regions/region-list
ng generate component features/field/regions/region-detail
ng generate component features/field/regions/region-form
ng generate service  features/field/regions/services/region

ng generate module features/field/objectifs/objectifs --module ../field.module
ng generate component features/field/objectifs/objectif-list
ng generate component features/field/objectifs/objectif-detail
ng generate component features/field/objectifs/objectif-form
ng generate component features/field/objectifs/objectif-progress-bar
ng generate service  features/field/objectifs/services/objectif

ng generate module features/field/plannings/plannings --module ../field.module
ng generate component features/field/plannings/planning-list
ng generate component features/field/plannings/planning-detail
ng generate component features/field/plannings/planning-form
ng generate component features/field/plannings/planning-calendar-view
ng generate service  features/field/plannings/services/planning

ng generate module features/field/visites/visites --module ../field.module
ng generate component features/field/visites/visite-list
ng generate component features/field/visites/visite-detail
ng generate component features/field/visites/visite-form
ng generate service  features/field/visites/services/visite

ng generate module features/field/rapports/rapports --module ../field.module
ng generate component features/field/rapports/rapport-list
ng generate component features/field/rapports/rapport-detail
ng generate component features/field/rapports/rapport-form
ng generate component features/field/rapports/rapport-validation-button
ng generate service  features/field/rapports/services/rapport

ng generate module features/field/kpi/kpi --module ../field.module
ng generate component features/field/kpi/kpi-dashboard
ng generate component features/field/kpi/kpi-delegue-card
ng generate component features/field/kpi/kpi-client-fidelite
ng generate service  features/field/kpi/services/kpi
```

### 9.5 Phase 5 — Documents

```bash
ng generate module features/documents/documents --module app

ng generate component features/documents/documents-general/document-list
ng generate component features/documents/documents-general/document-detail
ng generate component features/documents/documents-general/document-form
ng generate service  features/documents/documents-general/services/document

ng generate module features/documents/bons-commandes/bons-commandes --module ../documents.module
ng generate component features/documents/bons-commandes/bon-commande-list
ng generate component features/documents/bons-commandes/bon-commande-detail
ng generate component features/documents/bons-commandes/bon-commande-form
ng generate service  features/documents/bons-commandes/services/bon-commande

ng generate module features/documents/bons-livraison/bons-livraison --module ../documents.module
ng generate component features/documents/bons-livraison/bon-livraison-list
ng generate component features/documents/bons-livraison/bon-livraison-detail
ng generate component features/documents/bons-livraison/bon-livraison-form
ng generate service  features/documents/bons-livraison/services/bon-livraison

ng generate module features/documents/factures/factures --module ../documents.module
ng generate component features/documents/factures/facture-list
ng generate component features/documents/factures/facture-detail
ng generate component features/documents/factures/facture-form
ng generate component features/documents/factures/facture-print-view
ng generate service  features/documents/factures/services/facture
```

### 9.6 Phase 6 — Shared / Core polish

```bash
ng generate component shared/components/confirm-dialog
ng generate component shared/components/paginator
ng generate component shared/components/date-range-picker
ng generate component shared/components/empty-state
ng generate component shared/components/status-chip
ng generate component shared/components/kpi-card
ng generate component shared/components/chart-card

ng generate pipe  shared/pipes/role-badge
ng generate pipe  shared/pipes/order-status
ng generate pipe  shared/pipes/reclamation-status
ng generate pipe  shared/pipes/visite-status
ng generate pipe  shared/pipes/planning-status
ng generate pipe  shared/pipes/document-type
ng generate pipe  shared/pipes/boolean-yes-no

ng generate directive shared/directives/role-visible
```

---

## 10. Cross-cutting requirements for every new service

1. Inject `HttpClient` (or the existing `ApiService` wrapper).
2. Use `environment.apiBaseUrl` (must equal `http://localhost:5555` in dev — the gateway, not a downstream service).
3. Always type return as `Observable<ResponseDto<T>>`, then `.pipe(map(r => r.result))` at the call site if the component only needs the payload.
4. Auth token is attached automatically by `TokenInterceptor`.
5. Errors surface via `ErrorInterceptor` → `ToastService`.
6. Apply `RoleGuard` on every route per the role columns in §2.
7. For paginated endpoints, accept `pageNumber` and `pageSize` and surface them through the shared `PaginatorComponent`.
8. For DELETE buttons, always wrap in `ConfirmDialogComponent`.

---

## 11. Acceptance Checklist

- [ ] Every gateway path in §2 has a corresponding service method.
- [ ] Every service method is type-safe and returns `Observable<ResponseDto<T>>`.
- [ ] Every list view supports server-side pagination where the backend supports it.
- [ ] Every form view validates required fields before submitting.
- [ ] Every action requiring a role is gated by `RoleGuard` on the route AND `*appRoleVisible` on the trigger button.
- [ ] Sidebar reflects the new groups (Inventory, Field, Documents) and hides items the user cannot access.
- [ ] All endpoints reachable through `http://localhost:5555` (no direct calls to `:7000–:7005`).
- [ ] No duplicated promotion logic between `features/products/` and `features/promotions/`.
- [ ] Reclamation flow is reachable from an order detail page.
- [ ] KPI dashboard is the default `/dashboard` for `ADMIN` / `SUPERVISEUR`.

---

*End of plan.*
