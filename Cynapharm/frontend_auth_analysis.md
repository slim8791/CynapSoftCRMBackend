# Angular Frontend Authorization Rules

This document outlines how access control is managed in the Angular frontend application.

## Core Authorization Concepts

### 1. Guards
- **`authGuard`**: Protects routes by ensuring the user is authenticated. It checks `authService.isAuthenticated()`. If not authenticated, it redirects to `/login` with the `returnUrl`.
- **`roleGuard`**: Protects routes based on user roles. It expects a `data: { roles: [...] }` array in the route definition. It verifies if `authService.getUserRole()` is included in the allowed roles. If not, it redirects to `/forbidden`.

### 2. User Roles
Defined in `UserRole` enum (`auth.service.ts`):
- `ADMIN`
- `SUPERVISEUR`
- `DELEGUE`
- `MEDECIN`
- `CLIENT`

### 3. UI-Level Checks
UI components use `authService.getUserRole()` or signals to show/hide elements. For instance, `isAdmin` or `isSuperviseur` boolean properties are populated and then used with `@if (isAdmin)` or `*ngIf="isAdmin"` directives to selectively render buttons or table actions (e.g., in Orders, Reclamations, Navigation bar).

---

## Route Configuration

| Route Path | CanActivate Guards | Allowed Roles | Description |
|---|---|---|---|
| `login` | `None (Public)` | `All` | Defined in `app.routes.ts` |
| `register` | `None (Public)` | `All` | Defined in `app.routes.ts` |
| `forgot-password` | `None (Public)` | `All` | Defined in `app.routes.ts` |
| `reset-password` | `None (Public)` | `All` | Defined in `app.routes.ts` |
| `forbidden` | `None (Public)` | `All` | Defined in `app.routes.ts` |
| `dashboard` | `authGuard` | `Any Authenticated` | Defined in `app.routes.ts` |
| `products` | `authGuard` | `Any Authenticated` | Defined in `app.routes.ts` |
| `lots` | `authGuard, roleGuard` | `ADMIN, SUPERVISEUR, DELEGUE` | Defined in `app.routes.ts` |
| `promotions` | `authGuard, roleGuard` | `ADMIN, SUPERVISEUR` | Defined in `app.routes.ts` |
| `marketing` | `authGuard, roleGuard` | `ADMIN, SUPERVISEUR, DELEGUE` | Defined in `app.routes.ts` |
| `orders` | `authGuard` | `Any Authenticated` | Defined in `app.routes.ts` |
| `inventory` | `authGuard, roleGuard` | `ADMIN, SUPERVISEUR, DELEGUE` | Defined in `app.routes.ts` |
| `field` | `authGuard, roleGuard` | `ADMIN, SUPERVISEUR, DELEGUE` | Defined in `app.routes.ts` |
| `documents` | `authGuard, roleGuard` | `ADMIN, SUPERVISEUR` | Defined in `app.routes.ts` |
| `users` | `authGuard, roleGuard` | `ADMIN, SUPERVISEUR` | Defined in `app.routes.ts` |
| `settings` | `authGuard` | `Any Authenticated` | Defined in `app.routes.ts` |
| `general` | `None (Public)` | `All` | Defined in `documents-routing.module.ts` |
| `bons-commandes` | `None (Public)` | `All` | Defined in `documents-routing.module.ts` |
| `bons-commandes/:id` | `None (Public)` | `All` | Defined in `documents-routing.module.ts` |
| `bons-livraison` | `None (Public)` | `All` | Defined in `documents-routing.module.ts` |
| `bons-livraison/:id` | `None (Public)` | `All` | Defined in `documents-routing.module.ts` |
| `factures` | `None (Public)` | `All` | Defined in `documents-routing.module.ts` |
| `factures/:id` | `None (Public)` | `All` | Defined in `documents-routing.module.ts` |
| `visites` | `None (Public)` | `All` | Defined in `field-routing.module.ts` |
| `visites/all` | `None (Public)` | `All` | Defined in `field-routing.module.ts` |
| `visites/new` | `None (Public)` | `All` | Defined in `field-routing.module.ts` |
| `visites/:id` | `None (Public)` | `All` | Defined in `field-routing.module.ts` |
| `visites/:id/edit` | `None (Public)` | `All` | Defined in `field-routing.module.ts` |
| `plannings` | `None (Public)` | `All` | Defined in `field-routing.module.ts` |
| `plannings/new` | `None (Public)` | `All` | Defined in `field-routing.module.ts` |
| `plannings/:id/edit` | `None (Public)` | `All` | Defined in `field-routing.module.ts` |
| `rapports` | `None (Public)` | `All` | Defined in `field-routing.module.ts` |
| `rapports/new` | `None (Public)` | `All` | Defined in `field-routing.module.ts` |
| `rapports/:id/edit` | `None (Public)` | `All` | Defined in `field-routing.module.ts` |
| `objectifs` | `None (Public)` | `All` | Defined in `field-routing.module.ts` |
| `objectifs/new` | `None (Public)` | `All` | Defined in `field-routing.module.ts` |
| `objectifs/:id/edit` | `None (Public)` | `All` | Defined in `field-routing.module.ts` |
| `regions` | `None (Public)` | `All` | Defined in `field-routing.module.ts` |
| `regions/new` | `None (Public)` | `All` | Defined in `field-routing.module.ts` |
| `regions/:id/edit` | `None (Public)` | `All` | Defined in `field-routing.module.ts` |
| `kpi` | `None (Public)` | `All` | Defined in `field-routing.module.ts` |
| `stocks` | `None (Public)` | `All` | Defined in `inventory-routing.module.ts` |
| `stocks/new` | `None (Public)` | `All` | Defined in `inventory-routing.module.ts` |
| `stocks/:id` | `None (Public)` | `All` | Defined in `inventory-routing.module.ts` |
| `stocks/:id/edit` | `None (Public)` | `All` | Defined in `inventory-routing.module.ts` |
| `movements/new` | `None (Public)` | `All` | Defined in `inventory-routing.module.ts` |
| `movements` | `None (Public)` | `All` | Defined in `inventory-routing.module.ts` |
| `distributions` | `None (Public)` | `All` | Defined in `inventory-routing.module.ts` |
| `distributions/new` | `None (Public)` | `All` | Defined in `inventory-routing.module.ts` |
| `distributions/:id` | `None (Public)` | `All` | Defined in `inventory-routing.module.ts` |
| `promo-stocks/new` | `None (Public)` | `All` | Defined in `inventory-routing.module.ts` |
| `promo-stocks` | `None (Public)` | `All` | Defined in `inventory-routing.module.ts` |
| `new` | `authGuard, roleGuard` | `ADMIN, SUPERVISEUR` | Defined in `lots-routing.module.ts` |
| `:numero` | `authGuard, roleGuard` | `ADMIN, SUPERVISEUR, DELEGUE` | Defined in `lots-routing.module.ts` |
| `:numero/edit` | `authGuard, roleGuard` | `ADMIN, SUPERVISEUR` | Defined in `lots-routing.module.ts` |
| `supports` | `authGuard, roleGuard` | `ADMIN, SUPERVISEUR, DELEGUE` | Defined in `marketing-routing.module.ts` |
| `supports/new` | `authGuard, roleGuard` | `ADMIN, SUPERVISEUR` | Defined in `marketing-routing.module.ts` |
| `supports/:id` | `authGuard, roleGuard` | `ADMIN, SUPERVISEUR, DELEGUE` | Defined in `marketing-routing.module.ts` |
| `supports/:id/edit` | `authGuard, roleGuard` | `ADMIN, SUPERVISEUR` | Defined in `marketing-routing.module.ts` |
| `new` | `None (Public)` | `All` | Defined in `orders-routing.module.ts` |
| `reclamations` | `None (Public)` | `All` | Defined in `orders-routing.module.ts` |
| `reclamations/new` | `None (Public)` | `All` | Defined in `orders-routing.module.ts` |
| `reclamations/:id` | `None (Public)` | `All` | Defined in `orders-routing.module.ts` |
| `reclamations/:id/edit` | `None (Public)` | `All` | Defined in `orders-routing.module.ts` |
| `:id` | `None (Public)` | `All` | Defined in `orders-routing.module.ts` |
| `:id/edit` | `None (Public)` | `All` | Defined in `orders-routing.module.ts` |
| `new` | `None (Public)` | `All` | Defined in `products-routing.module.ts` |
| `:id/edit` | `None (Public)` | `All` | Defined in `products-routing.module.ts` |
| `:id` | `None (Public)` | `All` | Defined in `products-routing.module.ts` |
| `new` | `None (Public)` | `All` | Defined in `promotions-routing.module.ts` |
| `analytics` | `None (Public)` | `All` | Defined in `promotions-routing.module.ts` |
| `:id` | `None (Public)` | `All` | Defined in `promotions-routing.module.ts` |
| `:id/edit` | `None (Public)` | `All` | Defined in `promotions-routing.module.ts` |
| `new` | `authGuard, roleGuard` | `ADMIN, SUPERVISEUR` | Defined in `users-routing.module.ts` |
| `:id` | `authGuard, roleGuard` | `ADMIN, SUPERVISEUR` | Defined in `users-routing.module.ts` |
| `:id/edit` | `authGuard, roleGuard` | `ADMIN, SUPERVISEUR` | Defined in `users-routing.module.ts` |

## Feature Modules Detail

In addition to main routing, certain UI components have explicit access constraints:
- **Reclamations**: Only `ADMIN` or `SUPERVISEUR` can change status. Only `ADMIN` has full delete privileges.
- **Orders (Commandes)**: Only `ADMIN` can delete orders or see certain modification actions.
- **Navigation Sidebar (`app.html`)**: Menu items like Users, Documents, Promo-stocks are strictly restricted to `ADMIN` or `SUPERVISEUR` via `*ngIf`.
