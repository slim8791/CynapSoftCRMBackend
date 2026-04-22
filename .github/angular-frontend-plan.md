# Angular Frontend Plan for Cynapharm

## Overview
This document defines the Angular frontend plan for the existing `Cynapharm` app in the `CynapSoftCRMBackend` workspace.

The frontend will be built as a standalone Angular 21 application that consumes backend services through the gateway at `https://localhost:7777`.

## Goals
- Add routing and layout to the existing `Cynapharm` shell
- Implement authentication flow and secure pages
- Build feature pages for products, documents, field operations, inventory, and orders
- Centralize API communication with services and an HTTP interceptor

## Feature Areas
### Authentication
- `LoginPageComponent`
- `ForgotPasswordPageComponent`
- `AuthService`
- `AuthGuard`
- `ApiInterceptor`

### Core UI
- `DashboardPageComponent`
- `ShellLayoutComponent`
- `NavMenuComponent`

### Products / Commercial
- `ProductsPageComponent`
- `ProductDetailComponent`
- `LotsPageComponent`
- `PromosPageComponent`
- `MarketingPageComponent`

### Documents
- `DocumentsPageComponent`
- `InvoicesPageComponent`
- `DeliveryNotesPageComponent`
- `PurchaseOrdersPageComponent`

### Field / Sales
- `FieldDashboardPageComponent`
- `KpiPageComponent`
- `ObjectivesPageComponent`
- `PlanningPageComponent`
- `ReportsPageComponent`
- `RegionsPageComponent`
- `VisitsPageComponent`

### Inventory
- `InventoryPageComponent`
- `DistributionsPageComponent`
- `StockPageComponent`
- `InventoryBusinessPageComponent`
- `WarehousesPageComponent`
- `StockMovementsPageComponent`
- `PromotionalStockPageComponent`
- `DelegatedStockPageComponent`

### Orders
- `OrdersPageComponent`
- `OrderLinesPageComponent`
- `ReclamationsPageComponent`

## API Service Mapping
- Auth: `/auth/login`, `/auth/forgot-password`, `/auth/...`
- Products: `/products/...`
- Lots: `/products/lots/...`
- Promos: `/products/promos/...`
- Marketing: `/products/marketting/...`
- Documents: `/documents/...`
- Invoices: `/documents/factures/...`
- Delivery notes: `/documents/bons-livraison/...`
- Purchase orders: `/documents/bons-commandes/...`
- Field sales: `/fields/kpi`, `/fields/objectifs`, `/fields/plannings`, `/fields/rapports`, `/fields/regions`, `/fields/visites`
- Inventory: `/inventory/distributions`, `/inventory/stock`, `/inventory/inventory-business`, `/inventory/warehouses`, `/inventory/stock-movements`, `/inventory/stocks-promotionnels`, `/inventory/stocks-delegue`
- Orders: `/orders`, `/orders/lignes`, `/orders/reclamations`

## Route Structure
- `/login`
- `/dashboard`
- `/products`
- `/products/lots`
- `/products/promos`
- `/products/marketing`
- `/documents`
- `/documents/invoices`
- `/documents/delivery-notes`
- `/documents/purchase-orders`
- `/fields/kpi`
- `/fields/objectives`
- `/fields/planning`
- `/fields/reports`
- `/fields/regions`
- `/fields/visits`
- `/inventory/distributions`
- `/inventory/stock`
- `/inventory/business`
- `/inventory/warehouses`
- `/inventory/movements`
- `/inventory/promotional-stock`
- `/inventory/delegated-stock`
- `/orders`
- `/orders/lines`
- `/orders/reclamations`

## Implementation Phases
1. Scaffold shell, routing, auth, and dashboard
2. Implement products feature and API services
3. Add document, field, inventory, and order pages
4. Add reusable UI components, forms, filters, and error handling

## Notes
- Existing Angular shell currently has no configured routes.
- The gateway is configured in `CynapCRM.Gateway/ocelot.json` with global base URL `https://localhost:7777`.
- Auth is expected to use JWT bearer tokens.
