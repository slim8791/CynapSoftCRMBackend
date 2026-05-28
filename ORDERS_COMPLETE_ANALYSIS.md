# ORDERS_COMPLETE_ANALYSIS.md
> Complete analysis of OrderAPI, DocAPI, Angular, and MAUI layers.
> Date: 2026-05-26

---

## PART 1 — DATA MODEL TABLES

### 1.1 OrderAPI Database Schema

#### Table: `T_Commandes` (entity: `Commande`)

| Column | Type | Constraints | Default |
|---|---|---|---|
| `Id_Commande` | int | PK, IDENTITY | — |
<!-- | `DateCommande` | datetime2 | NOT NULL | `DateTime.UtcNow` | -->
| `MontantTotalHT` | decimal(18,2) | NOT NULL | 0 |
| `MontantTTC` | decimal(18,2) | NOT NULL | 0 |
| `Statut` | int | NOT NULL | 0 (Brouillon) |
| `Id_Client` | int | NOT NULL, INDEX | — |
| `IsDeleted` | bit | NOT NULL | false |
| `MotifAnnulation` | nvarchar(max) | NULL | null |

- Relations: `Lignes` (ICollection<LigneCommande>), `Reclamations` (ICollection<Reclamation>?)
- Soft delete: `IsDeleted` flag (physical row is kept)

#### Table: `T_LignesCommande` (entity: `LigneCommande`)

| Column | Type | Constraints |
|---|---|---|
| `Id_Ligne` | int | PK, IDENTITY |
| `Quantite` | int | NOT NULL |
| `Remise` | decimal | NOT NULL |
| `Id_Commande` | int | FK → Commande (CASCADE DELETE) |
| `Id_Produit` | int | NOT NULL, INDEX |
| `NumeroLot` | nvarchar(max) | NOT NULL, DEFAULT `""`, INDEX |
| `PrixUnitaire` | decimal | NOT NULL |

- Relations: `Commande` (nav), `Reclamations` (ICollection<Reclamation>?)

#### Table: `T_Reclamations` (entity: `Reclamation`)

| Column | Type | Constraints |
|---|---|---|
| `Id_Rec` | int | PK, IDENTITY |
| `Message` | nvarchar(max) | NOT NULL |
| `DateReclamation` | datetime2 | NOT NULL, DEFAULT `DateTime.Now` (**local time!**) |
| `Statut` | int | NOT NULL, DEFAULT 0 (Ouverte) |
| `Id_Commande` | int | FK → Commande (CASCADE DELETE) |
| `Id_Ligne` | int | FK → LigneCommande (**NO ACTION** — no cascade) |
| `Id_Client` | int | NOT NULL |

### 1.2 DocAPI Database Schema

#### Table: `T_Documents_Commerciaux` (TPH inheritance)

| Column | Type | Constraints | Note |
|---|---|---|---|
| `Numero_Doc` | int | PK, IDENTITY | — |
| `Nom_Doc` | nvarchar(max) | NOT NULL | e.g. "BC-42" |
| `DateCreation` | datetime2 | NOT NULL | `DateTime.UtcNow` |
| `Id_Commande` | int | NOT NULL, INDEX | — |
| `Id_Client` | int | NULL, INDEX | — |
| `TypeDocument` | nvarchar(max) | NOT NULL (discriminator) | "GENERIC"/"FACTURE"/"BL"/"BC" |
| `IsDeleted` | bit | NOT NULL | false |
| `MontantHT` | decimal | NULL | Facture only |
| `MontantTTC` | decimal | NULL | Facture only |
| `DateFacture` | datetime2 | NULL | Facture only |

- TPH: single table, `TypeDocument` discriminator maps to derived classes
- Discriminator values: `"GENERIC"` (Document), `"FACTURE"` (Facture), `"BL"` (BonLivraison), `"BC"` (BonCommande)

---

## PART 2 — ENUMS & STATE MACHINE

### 2.1 EtatCommande Enum (OrderAPI)

```csharp
public enum EtatCommande {
    Brouillon     = 0,   // Draft — not submitted yet
    EnAttente     = 1,   // Submitted — awaiting confirmation
    Confirmee     = 2,   // Confirmed by staff
    EnPreparation = 3,   // Being prepared in warehouse
    Expediee      = 4,   // Shipped
    Livree        = 5,   // Delivered (terminal)
    Annulee       = 6,   // Cancelled (terminal)
}
```

Angular mapping: `ETAT_LABELS`, `ETAT_CSS` in `order.service.ts`  
MAUI mapping: `Order.StatutFrançais` computed property in `Order.cs`

### 2.2 Order State Machine

Backend `_transitionsAutorisees` dictionary (OrderService):

```
Brouillon(0)     → EnAttente(1), Annulee(6)
EnAttente(1)     → Confirmee(2), Annulee(6)
Confirmee(2)     → EnPreparation(3), Annulee(6)
EnPreparation(3) → Expediee(4), Annulee(6)
Expediee(4)      → Livree(5)           [NO cancel allowed]
Livree(5)        → terminal
Annulee(6)       → terminal
```

Angular replicates this in `OrderService.getNextStatuses()`:
```typescript
const transitions: Record<number, EtatCommande[]> = {
  0: [EtatCommande.EnAttente,     EtatCommande.Annulee],
  1: [EtatCommande.Confirmee,     EtatCommande.Annulee],
  2: [EtatCommande.EnPreparation, EtatCommande.Annulee],
  3: [EtatCommande.Expediee,      EtatCommande.Annulee],
  4: [EtatCommande.Livree],
};
```

**BUG — Cancel button bypass (OrderAPI `CancelOrderAsync`):**  
The `CancelOrderAsync` method in OrderService blocks cancellation only if `statut == Livree || statut == Annulee`. This means a direct cancel call to `PUT /orders/{id}/cancel` succeeds for Expediee(4), even though `_transitionsAutorisees` does NOT include Annulee as a valid next state from 4. The cancel endpoint is a separate path that skips the general transition guard.

MAUI `CanCancel` property:
```csharp
public bool CanCancel => Order?.Statut is 0 or 1 or 2;
```
MAUI only shows Cancel button for Brouillon/EnAttente/Confirmee — correct business logic.

Angular `canCancel()`:
```typescript
canCancel(): boolean {
  const n = this.statutNumber;
  return n >= 0 && n <= 3;  // allows cancel through EnPreparation(3)
}
```
Angular allows cancel up to EnPreparation(3) — misaligned with MAUI (0-2) but both differ from backend (0-4 via the bug).

### 2.3 Automatic Document Creation on Status Transitions

| Status Transition | Auto-created Document | Angular (order-list) | Angular (order-detail) |
|---|---|---|---|
| → Confirmee(2) | Bon de Commande (BC) | `autoCreateBC()` | `autoCreateBC()` (skip if already exists) |
| → Expediee(4) | Bon de Livraison (BL) | `autoCreateBL()` | `autoCreateBL()` (skip if already exists) |
| → Livree(5) | Facture | `autoCreateFacture()` | `autoCreateFacture()` (skip if already exists) |

Note: MAUI does NOT auto-create documents — documents are consumed read-only.

### 2.4 StatutReclamation Enum

```csharp
public enum StatutReclamation { Ouverte = 0, EnCours = 1, Resolue = 2 }
```

State machine: `Ouverte→EnCours` (admin/superviseur), `EnCours→Resolue` (admin/superviseur).  
Both transitions are enforced in `ReclamationService.UpdateStatutAsync()`.  
Angular template enforces this correctly via `onUpdateStatus()` calls with explicit target values.

---

## PART 3 — ALL ENDPOINTS

### 3.1 OrderAPI Endpoints (`api/orders`)

| Method | Route | Roles | Parameters | Notes |
|---|---|---|---|---|
| GET | `/` | ADMIN, SUPERVISEUR, DELEGUE | page, pageSize, statut?, startDate?, endDate? | Filters IsDeleted |
| GET | `/{orderId}` | ALL (incl. CLIENT) | — | Includes Lignes + Reclamations |
| GET | `/by-client/{clientId}` | ALL | page, pageSize, statut? | CLIENT can only see own (JWT check in service) |
| GET | `/by-status` | ALL | statut, page, pageSize | **BUG: doesn't filter IsDeleted** |
| GET | `/by-date` | ADMIN, SUPERVISEUR | startDate, endDate, page, pageSize | — |
| GET | `/dashboard` | ADMIN, SUPERVISEUR | — | Aggregated KPIs |
| POST | `/` | ALL | CreateOrderDto body | Id_Client extracted from JWT |
| PUT | `/status` | ADMIN, SUPERVISEUR | UpdateOrderStatusDto body | Validates transitions |
| PUT | `/{id}/cancel` | SUPERVISEUR, CLIENT, PHARMACIEN, GROSSISTE | motif query param | **BUG: allows cancel from Expediee** |
| DELETE | `/{id}` | ADMIN | — | Soft delete |

### 3.2 OrderAPI Lignes Endpoints (`api/lignes`)

| Method | Route | Roles | Notes |
|---|---|---|---|
| POST | `/` | ALL | CreateOrUpdate — Id_Ligne=0 creates, >0 updates |
| DELETE | `/{ligneId}` | ALL | Hard delete |

### 3.3 OrderAPI Reclamations Endpoints (`api/reclamations`)

| Method | Route | Roles | Notes |
|---|---|---|---|
| GET | `/` | ADMIN, SUPERVISEUR | All reclamations |
| GET | `/by-commande/{orderId}` | ALL | Returns 404 if none — Angular catches it |
| GET | `/by-client/{idClient}` | ALL | MAUI uses this |
| GET | `/{idReclamation}` | ALL | Single reclamation |
| POST | `/` | ALL | Id_Client injected from JWT |
| PUT | `/{id}/status` | ADMIN, SUPERVISEUR | New status in request body |
| DELETE | `/{id}` | ADMIN, PHARMACIEN, GROSSISTE, CLIENT | — |

### 3.4 DocAPI Endpoints (`api/documents`, `api/bons-commandes`, `api/bons-livraison`, `api/factures`)

**Bons de Commande (`/documents/bons-commandes`):**

| Method | Route | Notes |
|---|---|---|
| GET | `/` | page, pageSize |
| GET | `/{id}` | — |
| GET | `/client/{id}` | — |
| GET | `/by-date` | startDate, endDate |
| GET | `/commande/{id}` | docs for order |
| POST | `/createUpdate` | numero_Doc=0 → create, >0 → update |
| DELETE | `/{id}` | Soft delete |

**Bons de Livraison (`/documents/bons-livraison`):** Same structure as BC.

**Factures (`/documents/factures`):** Same + financial fields (montantHT, montantTTC, dateFacture).

**General Documents (`/documents`):**

| Method | Route |
|---|---|
| GET | `/` | page, pageSize |
| GET | `/{numero}` | — |
| GET | `/client/{id}` | — |
| GET | `/commande/{id}` | — |
| GET | `/type/{type}` | "BC"/"BL"/"FACTURE" |
| GET | `/client/{idClient}/type/{type}` | Used by MAUI |
| POST | `/createUpdate` | — |
| DELETE | `/{numero}` | Soft delete |

---

## PART 4 — SERVICE LOGIC, BUGS & MISSING FEATURES

### 4.1 Backend OrderAPI Bugs

#### BUG-1: `GetAllOrdersAsync` without filters — missing IsDeleted filter
**File:** `CynapCRM.Services.OrderAPI/Service/OrderService.cs`  
**Issue:** The no-filter overload of `GetAllOrdersAsync` does NOT filter `!c.IsDeleted`, causing soft-deleted orders to appear in results.  
**Impact:** Deleted orders leak into paginated results returned to ADMIN/SUPERVISEUR.

#### BUG-2: `GetOrdersByStatusAsync` — missing IsDeleted filter
**File:** `CynapCRM.Services.OrderAPI/Service/OrderService.cs`  
**Issue:** `GetOrdersByStatusAsync` queries by `Statut` but never checks `!c.IsDeleted`.  
**Impact:** Soft-deleted orders with any status appear in status-filtered results.

#### BUG-3: `CancelOrderAsync` — allows cancel from Expediee(4)
**File:** `CynapCRM.Services.OrderAPI/Service/OrderService.cs`  
**Issue:** `CancelOrderAsync` blocks only `Livree(5)` and `Annulee(6)`. According to `_transitionsAutorisees`, Expediee(4) can only transition to Livree(5) — but a direct cancel call bypasses this.  
**Impact:** An order that has already been shipped can still be cancelled through this endpoint.

#### BUG-4: `Reclamation.DateReclamation` — local time instead of UTC
**File:** `CynapCRM.Services.OrderAPI/Models/Reclamation.cs`  
**Code:** `public DateTime DateReclamation { get; set; } = DateTime.Now;`  
**Issue:** Uses local server time. All other dates use `DateTime.UtcNow`. Inconsistent across timezone-aware queries.

### 4.2 Backend DocAPI Bugs

#### BUG-5: `BCService.GetBonsCommandeByClientAsync` — missing `.OfType<BonCommande>()`
**File:** `CynapCRM.Services.DocAPI/Service/BCService.cs`  
**Issue:** `GetBonsCommandeByClientAsync` queries the base `Documents` DbSet filtered only by `Id_Client`, without `.OfType<BonCommande>()`. Under TPH, this returns ALL document types (BL, Facture, GENERIC) for that client, not just BCs.  
**Note:** `GetBonCommandeByIdAsync` correctly uses `.OfType<BonCommande>()`.

#### BUG-6: `BCService.CreateOrUpdateBonCommandeAsync` — manual DTO construction instead of AutoMapper
**File:** `CynapCRM.Services.DocAPI/Service/BCService.cs`  
**Issue:** After SaveChanges, the service manually constructs the return DTO field-by-field instead of using `_mapper.Map<BonCommandeDto>(bc)`. If new fields are added to the entity, the return DTO will be stale.

#### BUG-7: Same issues apply to `BLService` (same pattern as BCService)

### 4.3 Angular Bugs

#### BUG-8: `reclamation-list.component.html` — camelCase vs PascalCase mismatch
**File:** `Cynapharm/src/app/features/orders/reclamations/reclamation-list/reclamation-list.component.ts`  
**Issue:** `load()` stores raw API response (not normalized) directly to `this.reclamations`. The template uses `rec.id_Rec`, `rec.message`, `rec.id_Commande`, `rec.id_Ligne`, `rec.id_Client`, `rec.dateReclamation`, `rec.statut` (all camelCase). Since OrderAPI returns PascalCase (`Id_Rec`, `Message`, `Id_Commande`, etc.), all fields will be undefined/empty in the template.  
**Impact:** Reclamation list shows empty rows (N°, Message, Commande, Ligne, Client, Date all blank).  
**Fix:** Normalize the raw response through `ReclamationService.normalizeRec()` or map fields in the template with `rec.Id_Rec ?? rec.id_Rec` fallback.

#### BUG-9: `order-detail.component.ts` — `canAssignLot` too restrictive
**File:** `Cynapharm/src/app/features/orders/order-detail/order-detail.component.ts` line 425  
**Code:** `return this.isAdmin && !ligne.NumeroLot && this.statutNumber === 3;`  
**Issue:** Only allows lot assignment when status is exactly EnPreparation(3). The underlying lignes endpoint allows lot assignment up to Expediee(4).

#### BUG-10: `order-detail.component.html` — Annulee check uses string comparison
**File:** `order-detail.component.html` line 105  
**Code:** `@if (order.Statut === 'Annulee' || order.Statut === 'Annulée')`  
**Issue:** Uses string comparison to detect cancelled orders, while the rest of the code uses `statutNumber === 6`. Duplicates the cancellation display (there's already a `@if (statutNumber === 6)` block above).

#### BUG-11: `document-list.component.html` — broken "New document" link
**File:** `Cynapharm/src/app/features/documents/documents-general/document-list/document-list.component.html` line 4  
**Code:** `<a routerLink="/documents/new" class="btn-p">+ Nouveau document</a>`  
**Issue:** Route `/documents/new` is not defined in `documents-routing.module.ts`. Will result in a 404 page.

#### BUG-12: `DocumentService.DocumentDto` — `type` field casing
**File:** `Cynapharm/src/app/features/documents/documents-general/services/document.service.ts`  
**Code:** `export interface DocumentDto { type: string; ... }`  
**Issue:** Backend returns `typeDocument` (camelCase key with "typeDocument" not "type"). The DTO field name `type` may fail to map. Also `numeroDoc` vs `numero_Doc` discrepancy.

### 4.4 MAUI Bugs

#### BUG-13: `Order.cs` — `DateCommande` missing `[JsonPropertyName]`
**File:** `Cynapharm-Mobile/Models/Orders/Order.cs`  
**Issue:** `DateCommande` has no `[JsonPropertyName]` annotation. If the backend returns `"dateCommande"` (camelCase, ASP.NET Core default), the C# property `DateCommande` (PascalCase) won't deserialize. Result: `DateCommande = default(DateTime)` = `0001-01-01`.

#### BUG-14: `Order.cs` — `[JsonPropertyName]` casing mismatch with backend PascalCase
**File:** `Cynapharm-Mobile/Models/Orders/Order.cs`  
**Issue:** Annotations use `"id_Commande"` (lowercase i), `"montantTotalHT"` (camelCase), `"montantTTC"` (camelCase), `"id_Client"` (lowercase i). If OrderAPI returns PascalCase (`"Id_Commande"`, `"MontantTotalHT"`, etc.), these annotations will fail to match.  
**Note:** This is consistent only if ASP.NET Core's default camelCase naming policy is active in OrderAPI.

#### BUG-15: `MAUI OrderService.UpdateOrderStatusAsync` — wrong field name
**File:** `Cynapharm-Mobile/Services/OrderService.cs` line 44  
**Code:** `=> _api.PutAsync<Order>($"orders/{id}/status", new { Status = status });`  
**Issue:** Backend `UpdateOrderStatusDto` has `NouveauStatut` field, not `Status`. This call will fail (NouveauStatut = 0 = Brouillon).

#### BUG-16: `MAUI OrderService.GetReclamationsAsync` — wrong URL
**File:** `Cynapharm-Mobile/Services/OrderService.cs` line 56  
**Code:** `_api.GetAsync<List<Reclamation>>($"orders/reclamations?orderId={orderId}")`  
**Issue:** The backend endpoint is `GET /orders/reclamations/by-commande/{orderId}` (path param), NOT a query param. This will hit `GET /orders/reclamations` (admin-only endpoint) and return all reclamations, not order-specific ones.  
**Fix:** Change to `$"orders/reclamations/by-commande/{orderId}"`

#### BUG-17: `MAUI OrderService.GetOrdersByStatusAsync` — wrong statut type
**File:** `Cynapharm-Mobile/Services/OrderService.cs` line 19-23  
**Code:** Passes `statut` as int directly but backend expects enum string name (`"EnAttente"`, `"Confirmee"`, etc.) or numeric value. If backend accepts numeric, this is fine; if it expects string name, this breaks.

#### BUG-18: `MAUI DocumentDetailViewModel` — `Statut` / `TVA` / `MontantTotal` fields not in backend DTO
**File:** `Cynapharm-Mobile/Models/Documents/Facture.cs`, `BonCommande.cs`, `BonLivraison.cs`  
**Issue:** Fields `Statut`, `TVA`, `MontantTotal` are declared with comments "not in backend DTO — kept for XAML compiled-binding compatibility". The XAML binds to `Facture.Statut` and `BonCommande.MontantTotal` which will always be empty/zero, showing misleading values.

### 4.5 Missing Features

#### MISSING-1: OrderAPI — No GET /orders/lignes endpoint
`ApiRoutes.Orders.Lignes = "orders/lignes"` in MAUI, and MAUI `GetLignesAsync` calls `orders/lignes?orderId={orderId}`, but no such GET endpoint exists. LigneController only has POST and DELETE. Lignes are returned embedded in `GET /orders/{id}`.

#### MISSING-2: OrderAPI — No lot validation on assignment
`PUT /orders/lignes` (createOrUpdate) accepts any NumeroLot string without validating that the lot exists in InventoryAPI, is not expired, or has sufficient stock. Lot validation is done client-side only (Angular `openLotModal` fetches available lots).

#### MISSING-3: DocAPI — No Cloudinary URL in BCService/BLService response
Backend DocAPI BonCommandeDto and BonLivraisonDto don't include a Cloudinary/storage URL for the document PDF. The Angular download button only works if the backend returns a `cloudinaryUrl` field — which it doesn't currently. Download feature always shows "Aucun fichier Cloudinary disponible."

#### MISSING-4: MAUI — No create/submit reclamation from ReclamationListViewModel
`ReclamationListViewModel` only loads and displays reclamations. There is no create button or form. Creation is handled via `OrderDetailViewModel.SubmitReclamationAsync`.

#### MISSING-5: Angular — No reclamation create button on reclamation-list
`reclamation-list.component.html` shows a table with status update and delete actions but no "New reclamation" button. Users must navigate to an order to create one.

---

## PART 5 — FIX PLAN

### Priority 1 — Data Correctness (Breaking)

| # | File | Fix |
|---|---|---|
| F-1 | `OrderService.cs` (GetAllOrdersAsync no-filter) | Add `.Where(c => !c.IsDeleted)` |
| F-2 | `OrderService.cs` (GetOrdersByStatusAsync) | Add `.Where(c => !c.IsDeleted)` |
| F-3 | `MAUI OrderService.cs` line 44 | Change `new { Status = status }` → `new { NouveauStatut = status }` |
| F-4 | `MAUI OrderService.cs` line 56 | Change URL to `orders/reclamations/by-commande/{orderId}` |
| F-5 | `reclamation-list.component.ts` | Normalize raw response via `svc.normalizeRec()` or use fallback fields in template |

### Priority 2 — Business Logic Correctness

| # | File | Fix |
|---|---|---|
| F-6 | `OrderService.cs` CancelOrderAsync | Block cancel when Statut >= Expediee(4) |
| F-7 | `BCService.cs` GetBonsCommandeByClientAsync | Add `.OfType<BonCommande>()` before `Where(b => b.Id_Client == idClient)` |
| F-8 | `Reclamation.cs` | Change `DateTime.Now` → `DateTime.UtcNow` |
| F-9 | `order-detail.component.ts` canAssignLot | Allow lot assignment for statutNumber <= 4 (not just === 3) |

### Priority 3 — UI / UX

| # | File | Fix |
|---|---|---|
| F-10 | `document-list.component.html` | Remove "New document" button or add the route |
| F-11 | `order-detail.component.html` | Remove duplicate Annulee string check (use `statutNumber === 6` consistently) |
| F-12 | `MAUI DocumentDetailPage.xaml` | Replace `Facture.Statut` / `BonCommande.MontantTotal` with real data or hide these fields |

### Priority 4 — Architecture / Consistency

| # | File | Fix |
|---|---|---|
| F-13 | `BCService.cs`, `BLService.cs` CreateOrUpdate | Replace manual DTO construction with `_mapper.Map<>()` |
| F-14 | `MAUI Order.cs` | Add `[JsonPropertyName("dateCommande")]` to DateCommande; verify all field-name annotations match actual API JSON casing |
| F-15 | `DocumentService.DocumentDto` | Rename `type` → `typeDocument`, `numeroDoc` → `numero_Doc` to match backend JSON |

---

## PART 6 — COMPLETE CODE OF ALL FILES

### 6.1 Angular Order Services

#### `order.service.ts`

```typescript
import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../core/services/api.service';

export enum EtatCommande {
  Brouillon     = 0,
  EnAttente     = 1,
  Confirmee     = 2,
  EnPreparation = 3,
  Expediee      = 4,
  Livree        = 5,
  Annulee       = 6,
}

export const ETAT_LABELS: Record<number, string> = {
  0: 'Brouillon', 1: 'En attente', 2: 'Confirmée',
  3: 'En préparation', 4: 'Expédiée', 5: 'Livrée', 6: 'Annulée',
};

export const ETAT_CSS: Record<number, string> = {
  0: 'chip-default', 1: 'chip-warning', 2: 'chip-info',
  3: 'chip-primary', 4: 'chip-purple', 5: 'chip-success', 6: 'chip-danger',
};

export interface OrderDashboardDto {
  TotalCommandes: number; EnAttente: number; Confirmees: number;
  EnPreparation: number; Expediees: number; Livrees: number; Annulees: number;
  MontantTotalHT: number; MontantTotalTTC: number;
  ReclamationsOuvertes: number; ReclamationsEnCours: number; ReclamationsResolues: number;
  CommandesAujourdHui: number; CommandesCeMois: number;
}

export interface LigneCommandeDto {
  Id_Ligne: number; Id_Produit: number; Id_Commande: number;
  Quantite: number; Remise: number; NumeroLot: string | null; PrixUnitaire: number; SousTotal?: number;
}

export interface CommandeDto {
  Id_Commande: number; DateCommande: string; MontantTotalHT: number;
  MontantTTC: number; Statut: string; Id_Client: number;
  Lignes: LigneCommandeDto[]; MotifAnnulation?: string | null;
  IsDeleted?: boolean; Reclamations?: any[];
}

export interface CreateLigneDto {
  Id_Commande: number; Id_Produit: number; Id_Ligne: number;
  Quantite: number; Remise: number; PrixUnitaire: number;
}

export interface CreateOrderDto {
  Id_Client: number; Lignes: CreateLigneDto[]; IsFinalValidation: boolean;
}

export interface UpdateOrderStatusDto {
  Id_Commande: number; NouveauStatut: EtatCommande;
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly base = '/orders';
  constructor(private api: ApiService) {}

  private unwrap<T>(r: any): T {
    if (r?.Result !== undefined) return r.Result;
    if (r?.result !== undefined) return r.result;
    return r;
  }

  private normalizeOrder(o: any): CommandeDto {
    return {
      Id_Commande:    o.Id_Commande    ?? o.id_Commande    ?? o.idCommande    ?? 0,
      DateCommande:   o.DateCommande   ?? o.dateCommande   ?? '',
      MontantTotalHT: o.MontantTotalHT ?? o.montantTotalHT ?? o.montantTotalHt ?? 0,
      MontantTTC:     o.MontantTTC     ?? o.montantTTC     ?? o.montantTtc    ?? 0,
      Statut:         this.toStatutString(o.Statut ?? o.statut ?? ''),
      Id_Client:      o.Id_Client      ?? o.id_Client      ?? o.idClient      ?? 0,
      Lignes:         (o.Lignes ?? o.lignes ?? []).map((l: any) => this.normalizeLigne(l)),
      MotifAnnulation: o.MotifAnnulation ?? o.motifAnnulation ?? null,
      IsDeleted:      o.IsDeleted ?? o.isDeleted ?? false,
      Reclamations:   o.Reclamations ?? o.reclamations ?? [],
    };
  }

  private toStatutString(s: any): string {
    if (typeof s === 'number') {
      const names: Record<number, string> = {
        0: 'Brouillon', 1: 'EnAttente', 2: 'Confirmee',
        3: 'EnPreparation', 4: 'Expediee', 5: 'Livree', 6: 'Annulee',
      };
      return names[s] ?? '';
    }
    return String(s ?? '');
  }

  private normalizeLigne(l: any): LigneCommandeDto {
    return {
      Id_Ligne:     l.Id_Ligne     ?? l.id_Ligne     ?? l.idLigne     ?? 0,
      Id_Produit:   l.Id_Produit   ?? l.id_Produit   ?? l.idProduit   ?? 0,
      Id_Commande:  l.Id_Commande  ?? l.id_Commande  ?? l.idCommande  ?? 0,
      Quantite:     l.Quantite     ?? l.quantite     ?? 0,
      Remise:       l.Remise       ?? l.remise       ?? 0,
      NumeroLot:    l.NumeroLot    || l.numeroLot    || null,
      PrixUnitaire: l.PrixUnitaire ?? l.prixUnitaire ?? 0,
      SousTotal:    l.SousTotal    ?? l.sousTotal    ?? undefined,
    };
  }

  statutToNumber(statut: string): number {
    const map: Record<string, number> = {
      Brouillon: 0, EnAttente: 1, Confirmee: 2, Validee: 2,
      EnPreparation: 3, Expediee: 4, Livree: 5, Annulee: 6,
    };
    return map[statut] ?? -1;
  }

  getEtatLabel(statut: string | number): string {
    if (typeof statut === 'number') return ETAT_LABELS[statut] ?? '—';
    return ETAT_LABELS[this.statutToNumber(statut)] ?? statut;
  }

  getEtatClass(statut: string | number): string {
    const n = typeof statut === 'number' ? statut : this.statutToNumber(statut);
    return ETAT_CSS[n] ?? 'chip-default';
  }

  getNextStatuses(current: string): { label: string; value: EtatCommande }[] {
    const n = this.statutToNumber(current);
    const transitions: Record<number, EtatCommande[]> = {
      0: [EtatCommande.EnAttente,     EtatCommande.Annulee],
      1: [EtatCommande.Confirmee,     EtatCommande.Annulee],
      2: [EtatCommande.EnPreparation, EtatCommande.Annulee],
      3: [EtatCommande.Expediee,      EtatCommande.Annulee],
      4: [EtatCommande.Livree],
    };
    return (transitions[n] ?? []).map(v => ({ value: v, label: ETAT_LABELS[v] }));
  }

  getOrders(page = 1, pageSize = 20, statut?: string, startDate?: string, endDate?: string): Observable<CommandeDto[]> {
    let p = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (statut)    p = p.set('statut', statut);
    if (startDate) p = p.set('startDate', startDate);
    if (endDate)   p = p.set('endDate', endDate);
    return this.api.get<any>(this.base, p).pipe(
      map(r => { const raw = this.unwrap<any[]>(r) ?? []; return Array.isArray(raw) ? raw.map(o => this.normalizeOrder(o)) : []; })
    );
  }

  getOrdersByStatus(statut: string, page = 1, pageSize = 20): Observable<CommandeDto[]> {
    const p = new HttpParams().set('statut', statut).set('page', page).set('pageSize', pageSize);
    return this.api.get<any>(`${this.base}/by-status`, p).pipe(
      map(r => (this.unwrap<any[]>(r) ?? []).map(o => this.normalizeOrder(o)))
    );
  }

  getOrdersByDateRange(startDate: string, endDate: string, page = 1, pageSize = 20): Observable<CommandeDto[]> {
    const p = new HttpParams().set('startDate', startDate).set('endDate', endDate).set('page', page).set('pageSize', pageSize);
    return this.api.get<any>(`${this.base}/by-date`, p).pipe(
      map(r => (this.unwrap<any[]>(r) ?? []).map(o => this.normalizeOrder(o)))
    );
  }

  getOrdersDashboard(): Observable<OrderDashboardDto> {
    return this.api.get<any>(`${this.base}/dashboard`).pipe(map(r => this.unwrap<OrderDashboardDto>(r)));
  }

  cancelOrder(id: number, motif: string): Observable<any> {
    return this.api.put<any>(`${this.base}/${id}/cancel`, { Motif: motif });
  }

  getOrderById(id: number): Observable<CommandeDto | null> {
    return this.api.get<any>(`${this.base}/${id}`).pipe(
      map(r => { const raw = this.unwrap<any>(r); return raw ? this.normalizeOrder(raw) : null; })
    );
  }

  getOrdersByClient(clientId: number): Observable<CommandeDto[]> {
    return this.api.get<any>(`${this.base}/by-client/${clientId}`).pipe(
      map(r => { const raw = this.unwrap<any[]>(r) ?? []; return Array.isArray(raw) ? raw.map(o => this.normalizeOrder(o)) : []; })
    );
  }

  createOrder(dto: CreateOrderDto): Observable<any> { return this.api.post<any>(this.base, dto); }
  updateOrderStatus(dto: UpdateOrderStatusDto): Observable<any> { return this.api.put<any>(`${this.base}/status`, dto); }
  deleteOrder(id: number): Observable<any> { return this.api.delete<any>(`${this.base}/${id}`); }

  assignLot(ligne: LigneCommandeDto, numeroLot: string): Observable<any> {
    return this.api.post<any>(`${this.base}/lignes`, {
      Id_Ligne: ligne.Id_Ligne, Id_Commande: ligne.Id_Commande,
      Id_Produit: ligne.Id_Produit, Quantite: ligne.Quantite,
      Remise: ligne.Remise, PrixUnitaire: ligne.PrixUnitaire, NumeroLot: numeroLot,
    });
  }
}
```

#### `services/ligne.service.ts`

```typescript
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';

export interface CreateOrUpdateLigneDto {
  Id_Commande: number; Id_Produit: number; Id_Ligne: number;
  Quantite: number; Remise: number; PrixUnitaire: number;
}

@Injectable({ providedIn: 'root' })
export class LigneService {
  private readonly base = '/orders/lignes';
  constructor(private api: ApiService) {}
  createOrUpdate(dto: CreateOrUpdateLigneDto): Observable<any> { return this.api.post<any>(this.base, dto); }
  delete(ligneId: number): Observable<any> { return this.api.delete<any>(`${this.base}/${ligneId}`); }
}
```

#### `services/reclamation.service.ts`

```typescript
import { Injectable } from '@angular/core';
import { Observable, of, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { ApiService } from '../../../core/services/api.service';

export enum StatutReclamation { Ouverte = 0, EnCours = 1, Resolue = 2 }

export const STATUT_REC_LABELS: Record<number, string> = { 0: 'Ouverte', 1: 'En cours', 2: 'Résolue' };
export const STATUT_REC_CSS: Record<number, string>    = { 0: 'chip-warning', 1: 'chip-info', 2: 'chip-success' };

export interface ReclamationDto {
  Id_Rec: number; Message: string; DateReclamation: string;
  Statut?: string | number; Id_Commande: number; Id_Ligne: number; Id_Client: number;
}

@Injectable({ providedIn: 'root' })
export class ReclamationService {
  private readonly base = '/orders/reclamations';
  constructor(private api: ApiService) {}

  private unwrap<T>(r: any): T {
    if (r?.Result !== undefined) return r.Result;
    if (r?.result !== undefined) return r.result;
    return r;
  }

  private normalizeRec(r: any): ReclamationDto {
    return {
      Id_Rec:          r.Id_Rec          ?? r.id_Rec          ?? r.idRec          ?? 0,
      Message:         r.Message         ?? r.message         ?? '',
      DateReclamation: r.DateReclamation ?? r.dateReclamation ?? '',
      Statut:          r.Statut          ?? r.statut          ?? 'Ouverte',
      Id_Commande:     r.Id_Commande     ?? r.id_Commande     ?? r.idCommande     ?? 0,
      Id_Ligne:        r.Id_Ligne        ?? r.id_Ligne        ?? r.idLigne        ?? 0,
      Id_Client:       r.Id_Client       ?? r.id_Client       ?? r.idClient       ?? 0,
    };
  }

  statutToNumber(statut?: string | number): number {
    if (typeof statut === 'number') return statut;
    const map: Record<string, number> = { Ouverte: 0, EnCours: 1, Resolue: 2, '0': 0, '1': 1, '2': 2 };
    return statut != null ? (map[statut] ?? 0) : 0;
  }

  getStatutLabel(statut?: string | number): string { return STATUT_REC_LABELS[this.statutToNumber(statut)] ?? statut ?? '—'; }
  getStatutClass(statut?: string | number): string { return STATUT_REC_CSS[this.statutToNumber(statut)] ?? 'chip-default'; }

  getAll(): Observable<any> { return this.api.get<any>(this.base); }

  getById(id: number): Observable<ReclamationDto | null> {
    return this.api.get<any>(`${this.base}/${id}`).pipe(
      map(r => { const raw = this.unwrap<any>(r); return raw ? this.normalizeRec(raw) : null; })
    );
  }

  getByOrder(orderId: number): Observable<any> {
    return this.api.get<any>(`${this.base}/by-commande/${orderId}`).pipe(
      catchError((err: HttpErrorResponse) => err.status === 404 ? of(null) : throwError(() => err))
    );
  }

  getByClient(clientId: number): Observable<any> {
    return this.api.get<any>(`${this.base}/by-client/${clientId}`).pipe(
      catchError((err: HttpErrorResponse) => err.status === 404 ? of(null) : throwError(() => err))
    );
  }

  createOrUpdate(dto: ReclamationDto): Observable<any> { return this.api.post<any>(this.base, dto); }

  updateStatus(id: number, status: StatutReclamation): Observable<any> {
    return this.api.put<any>(`${this.base}/${id}/status`, status);
  }

  delete(id: number): Observable<any> { return this.api.delete<any>(`${this.base}/${id}`); }
}
```

### 6.2 Angular Document Services

#### `bons-commandes/services/bon-commande.service.ts`

```typescript
export interface BonCommandeDto {
  numero_Doc: number; nom_Doc?: string; id_Client?: number;
  id_Commande?: number; dateCreation?: string; typeDocument?: string;
  cloudinaryUrl?: string; url?: string;
}

@Injectable({ providedIn: 'root' })
export class BonCommandeService {
  private readonly base = '/documents/bons-commandes';
  constructor(private api: ApiService) {}
  private u<T>(r: any): T { return r?.Result ?? r?.result ?? r; }

  getAll(pageNumber = 1, pageSize = 20): Observable<BonCommandeDto[]> {
    const p = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.api.get<any>(this.base, p).pipe(map(r => this.u<BonCommandeDto[]>(r) ?? []));
  }
  getById(id: number)        { return this.api.get<any>(`${this.base}/${id}`).pipe(map(r => this.u<BonCommandeDto>(r))); }
  getByClient(id: number)    { return this.api.get<any>(`${this.base}/client/${id}`).pipe(map(r => this.u<BonCommandeDto[]>(r) ?? [])); }
  getByCommande(id: number): Observable<BonCommandeDto[]> {
    return this.api.get<any>(`${this.base}/commande/${id}`).pipe(map(r => this.u<BonCommandeDto[]>(r) ?? []));
  }
  createOrUpdate(dto: BonCommandeDto): Observable<BonCommandeDto> {
    return this.api.post<any>(`${this.base}/createUpdate`, dto).pipe(map(r => this.u<BonCommandeDto>(r)));
  }
  delete(id: number): Observable<void> { return this.api.delete<void>(`${this.base}/${id}`); }
}
```

#### `factures/services/facture.service.ts`

```typescript
export interface FactureDto {
  numero_Doc: number; nom_Doc?: string; id_Client?: number;
  id_Commande?: number; montantHT?: number; montantTTC?: number;
  dateFacture?: string; typeDocument?: string; cloudinaryUrl?: string; url?: string;
}

@Injectable({ providedIn: 'root' })
export class FactureService {
  private readonly base = '/documents/factures';
  // ... same pattern as BonCommandeService
  getByCommande(id: number): Observable<FactureDto[]> {
    return this.api.get<any>(`${this.base}/commande/${id}`).pipe(map(r => this.u<FactureDto[]>(r) ?? []));
  }
  createOrUpdate(dto: FactureDto): Observable<FactureDto> {
    return this.api.post<any>(`${this.base}/createUpdate`, dto).pipe(map(r => this.u<FactureDto>(r)));
  }
}
```

### 6.3 Angular Routing

#### `orders-routing.module.ts`

```typescript
const routes: Routes = [
  { path: '',       component: OrderListComponent  },
  { path: 'new',    component: OrderFormComponent  },
  { path: 'reclamations',
    loadComponent: () => import('./reclamations/reclamation-list/reclamation-list.component').then(m => m.ReclamationListComponent) },
  { path: 'reclamations/new',
    loadComponent: () => import('./reclamations/reclamation-form/reclamation-form.component').then(m => m.ReclamationFormComponent) },
  { path: 'reclamations/:id',
    loadComponent: () => import('./reclamations/reclamation-detail/reclamation-detail.component').then(m => m.ReclamationDetailComponent) },
  { path: 'reclamations/:id/edit',
    loadComponent: () => import('./reclamations/reclamation-form/reclamation-form.component').then(m => m.ReclamationFormComponent) },
  { path: ':id',    component: OrderDetailComponent },
  { path: ':id/edit', component: OrderFormComponent }
];
```

#### `documents-routing.module.ts`

```typescript
const routes: Routes = [
  { path: '', redirectTo: 'general', pathMatch: 'full' },
  { path: 'general', loadComponent: () => import('./documents-general/document-list/document-list.component')... },
  { path: 'bons-commandes', loadComponent: () => import('./bons-commandes/bon-commande-list/...')... },
  { path: 'bons-commandes/:id', loadComponent: () => import('./document-detail/...')..., data: { documentKind: 'bon-commande' } },
  { path: 'bons-livraison', loadComponent: () => import('./bons-livraison/bon-livraison-list/...')... },
  { path: 'bons-livraison/:id', loadComponent: () => import('./document-detail/...')..., data: { documentKind: 'bon-livraison' } },
  { path: 'factures', loadComponent: () => import('./factures/facture-list/...')... },
  { path: 'factures/:id', loadComponent: () => import('./document-detail/...')..., data: { documentKind: 'facture' } }
];
```

### 6.4 Angular order-list Component

**Key behaviors:**
- Filters out Brouillon(0) orders from display (drafts not shown to ADMIN)
- Supports status + date range filters, pagination (15/page)
- Status dropdown inline per row with `@HostListener('document:click')` to close
- Delete restricted to Brouillon(0) or Annulee(6) (front-end guard)
- Auto-creates BC/BL/Facture on status transitions: Confirmee→BC, Expediee→BL, Livree→Facture
- Loads open reclamation count badge for the Reclamations nav link
- Client names resolved from UserAPI

### 6.5 Angular order-detail Component

**Key behaviors:**
- 4-tab layout: Informations / Lignes / Réclamations / Documents
- `directTransition` property: shows labeled forward-transition button (skip modal for known single next state)
- Cancel order: requires motif ≥ 10 chars via dedicated modal
- Lot assignment: modal fetches available lots from ProductAPI, filtered by non-expired, non-out-of-stock
- Lot assignment guarded: admin only, no lot yet assigned, status = EnPreparation(3)
- Auto-create documents buttons visible based on status (BC when ≥ Confirmee, BL when ≥ Expediee, Facture when Livree)
- Loaded documents linked with `getByCommande` calls to all three doc services

### 6.6 Angular Reclamation Components

- `reclamation-list`: shows all reclamations with inline status transitions (Prendre en charge / Marquer comme résolue), supports orderId/clientId query params for filtered view
- `reclamation-form`: creates or updates reclamation, prefills orderId from query params, Id_Client server-side
- `reclamation-detail`: read-only view of a single reclamation with link back to order

### 6.7 Angular Document Components

- `bon-commande-list`, `bon-livraison-list`, `facture-list`: identical structure — paginated table, client name resolution, Cloudinary download (via URL field)
- `document-list`: unified view with type filter tabs (Tous/Factures/BC/BL), delegates to `getByType` or `getAll`
- `document-detail`: uses route `data.documentKind` to select correct service; shared view for Facture/BC/BL

### 6.8 MAUI Models

#### `Models/Orders/Order.cs`

```csharp
public class Order
{
    [JsonPropertyName("id_Commande")]  public int Id { get; set; }
    public string NumeroCommande => $"CMD-{Id:D5}";
    public DateTime DateCommande { get; set; }   // NOTE: no [JsonPropertyName] — relies on exact name match
    public int Statut { get; set; }
    public string StatutFrançais => Statut switch {
        0 => "Brouillon", 1 => "En attente", 2 => "Confirmée",
        3 => "En préparation", 4 => "Expédiée", 5 => "Livrée", 6 => "Annulée",
        _ => $"Statut {Statut}"
    };
    [JsonPropertyName("montantTotalHT")] public decimal MontantTotal { get; set; }
    [JsonPropertyName("montantTTC")]     public decimal MontantTTC { get; set; }
    [JsonPropertyName("id_Client")]      public int ClientId { get; set; }
    public string? Notes { get; set; }
    public string? MotifAnnulation { get; set; }
    public bool IsDeleted { get; set; }
    public List<LigneCommande> Lignes { get; set; } = new();
    public List<Reclamation>?  Reclamations { get; set; }
}
```

#### `Models/Orders/LigneCommande.cs`

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

#### `Models/Orders/Reclamation.cs`

```csharp
public class Reclamation
{
    [JsonPropertyName("id_Rec")]            public int Id { get; set; }
    [JsonPropertyName("id_Commande")]       public int CommandeId { get; set; }
    [JsonPropertyName("id_Ligne")]          public int LigneId { get; set; }
    [JsonPropertyName("message")]           public string Motif { get; set; } = string.Empty;
    [JsonPropertyName("dateReclamation")]   public DateTime DateCreation { get; set; }
    public string? Statut { get; set; }
}
```

#### `Models/Orders/CartLine.cs`

```csharp
public class CartLine : ObservableObject
{
    public int ProductId { get; set; }
    public string ProductNom { get; set; } = string.Empty;
    private int _quantite;
    public int Quantite {
        get => _quantite;
        set { if (SetProperty(ref _quantite, value)) {
            OnPropertyChanged(nameof(SousTotal));
            OnPropertyChanged(nameof(EconomieTotale));
        }}
    }
    public decimal PrixOriginal { get; set; }
    public decimal PrixUnitaire { get; set; }
    public decimal RemisePourcentage { get; set; }
    public string? PromoTitre { get; set; }
    public bool    HasPromo       => RemisePourcentage > 0;
    public decimal SousTotal      => Quantite * PrixUnitaire;
    public decimal EconomieTotale => Quantite * (PrixOriginal - PrixUnitaire);
}
```

#### `Models/Documents/Facture.cs`, `BonCommande.cs`, `BonLivraison.cs`, `DocumentSummary.cs`

All use `[JsonPropertyName]` with `numero_Doc`, `nom_Doc`, `dateCreation`/`dateFacture`, `id_Commande`.  
Fields `Statut`, `TVA`, `MontantTotal` are stub properties (not in backend DTO) kept for XAML binding.

### 6.9 MAUI Services

#### `Services/ApiRoutes.cs` (key routes)

```csharp
public static class Orders {
    public const string Base         = "orders";
    public const string ByStatus     = "orders/by-status";
    public const string Dashboard    = "orders/dashboard";
    public const string Cancel       = "orders/{0}/cancel";    // string.Format
    public const string UpdateStatus = "orders/status";
    public const string ByClient     = "orders/by-client";
    public const string Lignes       = "orders/lignes";
    public const string Reclamations = "orders/reclamations";
}
public static class Documents {
    public const string DocumentsByClientAndType = "documents/client/{0}/type/{1}";
    public const string FacturesByCommande       = "documents/factures/commande";
    public const string BCByCommande             = "documents/bons-commandes/commande";
    public const string BLByCommande             = "documents/bons-livraison/commande";
}
```

#### `Services/OrderService.cs`

```csharp
public class OrderService
{
    private readonly ApiService _api;
    public OrderService(ApiService api) { _api = api; }

    public Task<List<Order>?> GetOrdersAsync(int? statut = null, int page = 1, int pageSize = 20) {
        var url = $"orders?page={page}&pageSize={pageSize}";
        if (statut.HasValue) url += $"&statut={statut.Value}";
        return _api.GetAsync<List<Order>>(url);
    }

    public Task<List<Order>?> GetOrdersByClientAsync(int clientId, int? statut = null, int page = 1, int size = 20) {
        var url = $"{ApiRoutes.Orders.ByClient}/{clientId}?page={page}&pageSize={size}";
        if (statut.HasValue) url += $"&statut={statut.Value}";
        return _api.GetAsync<List<Order>>(url);
    }

    public Task<Order?> GetOrderByIdAsync(int id) => _api.GetAsync<Order>($"orders/{id}");

    // BUG: route is wrong — should be by-commande/{orderId} not query param
    public Task<List<Reclamation>?> GetReclamationsAsync(int? orderId)
        => _api.GetAsync<List<Reclamation>>($"orders/reclamations?orderId={orderId}");

    public Task<List<Reclamation>?> GetReclamationsByClientAsync(int clientId)
        => _api.GetAsync<List<Reclamation>>($"orders/reclamations/by-client/{clientId}");

    public Task<Order?> CreateOrderAsync(object request) => _api.PostAsync<Order>("orders", request);

    // BUG: sends { Status } not { NouveauStatut }
    public Task<Order?> UpdateOrderStatusAsync(int id, string status)
        => _api.PutAsync<Order>($"orders/{id}/status", new { Status = status });

    public Task<object?> CancelOrderAsync(int id, string motif)
        => _api.PutAsync<object>(
            $"{string.Format(ApiRoutes.Orders.Cancel, id)}?motif={Uri.EscapeDataString(motif)}",
            new { });

    public Task<Reclamation?> CreateReclamationAsync(Reclamation reclamation)
        => _api.PostAsync<Reclamation>("orders/reclamations", reclamation);
}
```

#### `Services/DocumentService.cs`

```csharp
public class DocumentService
{
    private readonly ApiService _api;
    public DocumentService(ApiService api) { _api = api; }

    public Task<List<Facture>?> GetFacturesAsync(int page = 1, int size = 20)
        => _api.GetAsync<List<Facture>>($"documents/factures?page={page}&size={size}");

    public Task<Facture?> GetFactureByIdAsync(int id)
        => _api.GetAsync<Facture>($"documents/factures/{id}");

    // ... similar for BonCommande, BonLivraison

    public Task<List<DocumentSummary>?> GetDocumentsByClientAndTypeAsync(int idClient, string type)
        => _api.GetAsync<List<DocumentSummary>>(
            string.Format(ApiRoutes.Documents.DocumentsByClientAndType, idClient, type));

    public Task<List<Facture>?> GetFacturesByCommandeAsync(int idCommande)
        => _api.GetAsync<List<Facture>>($"{ApiRoutes.Documents.FacturesByCommande}/{idCommande}");
    
    public Task<List<BonCommande>?> GetBCByCommandeAsync(int idCommande)
        => _api.GetAsync<List<BonCommande>>($"{ApiRoutes.Documents.BCByCommande}/{idCommande}");
    
    public Task<List<BonLivraison>?> GetBLByCommandeAsync(int idCommande)
        => _api.GetAsync<List<BonLivraison>>($"{ApiRoutes.Documents.BLByCommande}/{idCommande}");
}
```

### 6.10 MAUI ViewModels

#### `OrderListViewModel.cs` — Key Logic

- Reads `StorageKeys.UserRole` and `StorageKeys.UserId` from SecureStorage
- `IsGrossiste` shows KPI strip in header
- `_isClient` (PHARMACIEN/GROSSISTE/CLIENT) → calls `GetOrdersByClientAsync` scoped to their ID
- Admin/Delegue/Superviseur → calls `GetOrdersAsync` (all orders)
- Filters `o.Statut != 0` (hides Brouillon from list, consistent with Angular)
- Infinite scroll via `RemainingItemsThresholdReachedCommand`
- `StatusFilter` → `_statusCodeMap` maps French labels → EtatCommande int

#### `OrderDetailViewModel.cs` — Key Logic

```csharp
public bool CanCancel            => Order?.Statut is 0 or 1 or 2;  // Brouillon, EnAttente, Confirmee only
public bool CanCreateReclamation => Order?.Statut is 4 or 5;        // Expediee or Livree
```

- Loads product names for each ligne (sequential calls to ProductService)
- Loads linked documents (Factures, BC, BL) via DocumentService
- Cancel: `Shell.Current.DisplayPromptAsync` for motif input
- Reclamation form: inline form in OrderDetailPage.xaml, collapses/expands

#### `CreateOrderViewModel.cs` — Key Logic

- 3-step wizard: (1) Search products + add to cart, (2) Review cart, (3) Confirm
- Cart persisted to `Preferences` keyed by user (`draft_cart_{userId}`)
- Promotions fetched from API on init, seeded to SQLite via `LocalDatabaseService`
- `AddLineAsync`: queries SQLite promotion cache, applies discount to `PrixUnitaire`
- Cart lines use `ObservableObject` for reactive quantity changes
- `SubmitOrderAsync`: client-side validation before POST to OrderAPI
- `IsFinalValidation: true` always (mobile creates directly as EnAttente)

#### `DocumentListViewModel.cs` — Key Logic

- Type tabs: "Factures"/"Bons cmd."/"Bons liv." → index 0/1/2
- Calls `GetDocumentsByClientAndTypeAsync(clientId, apiType)` — uses unified endpoint
- ClientId read from SecureStorage
- Tapping opens `DocumentDetailViewModel` via Shell navigation with `documentType` and `documentId` params

#### `ReclamationListViewModel.cs` — Key Logic

- Loads reclamations via `GetReclamationsByClientAsync(clientId)` — shows only current user's reclamations
- No create functionality (creation is via OrderDetailViewModel)
- Supports pull-to-refresh

---

## PART 7 — CROSS-LAYER CONSISTENCY MATRIX

| Feature | Backend OrderAPI | DocAPI | Angular | MAUI |
|---|---|---|---|---|
| Brouillon hidden | Returns it (no filter) | N/A | Filtered out (line 150) | Filtered (Statut != 0) |
| Cancel from Expediee | Allowed (BUG) | N/A | Not shown (transitions[4] = [Livree]) | Not shown (CanCancel: 0-2 only) |
| Auto BC on Confirmee | No (Angular does it) | N/A | ✓ Angular auto-creates | Not supported |
| Lot assignment | Accepts any lot string | N/A | Modal, filtered lots | Not implemented |
| Reclamation create | JWT-based Id_Client | N/A | ReclamationForm | OrderDetailPage inline form |
| Doc download | No URL in DTO | No cloudinaryUrl field | Shows error "aucun fichier" | Hidden when no URL |
| Status transitions | `_transitionsAutorisees` | N/A | `getNextStatuses()` | `CanCancel`, `CanCreateReclamation` |

---

## SUMMARY OF ALL BUGS

| ID | Severity | Layer | Description |
|---|---|---|---|
| BUG-1 | High | Backend OrderAPI | GetAllOrdersAsync no-filter — IsDeleted not filtered |
| BUG-2 | High | Backend OrderAPI | GetOrdersByStatusAsync — IsDeleted not filtered |
| BUG-3 | Medium | Backend OrderAPI | CancelOrderAsync allows cancel from Expediee(4) |
| BUG-4 | Low | Backend OrderAPI | Reclamation.DateReclamation uses DateTime.Now (local) |
| BUG-5 | High | Backend DocAPI | BCService.GetBonsCommandeByClientAsync missing .OfType<BonCommande>() |
| BUG-6 | Low | Backend DocAPI | BCService manual DTO construction instead of AutoMapper |
| BUG-7 | Low | Backend DocAPI | BLService same as BUG-6 |
| BUG-8 | Critical | Angular | reclamation-list uses raw API data with camelCase access — all fields undefined |
| BUG-9 | Low | Angular | canAssignLot too restrictive (===3 only, should be <=4) |
| BUG-10 | Low | Angular | order-detail Annulee check uses string not number |
| BUG-11 | Low | Angular | document-list broken "New document" link (route not defined) |
| BUG-12 | Low | Angular | DocumentService.DocumentDto field names don't match backend JSON |
| BUG-13 | Medium | MAUI | Order.DateCommande missing JsonPropertyName annotation |
| BUG-14 | High | MAUI | Order.cs JsonPropertyName annotations use wrong casing if API is PascalCase |
| BUG-15 | High | MAUI | OrderService.UpdateOrderStatusAsync sends wrong field name {Status} not {NouveauStatut} |
| BUG-16 | High | MAUI | OrderService.GetReclamationsAsync wrong URL (query param vs path param) |
| BUG-17 | Medium | MAUI | GetOrdersByStatusAsync passes int statut but endpoint may expect string |
| BUG-18 | Low | MAUI | Stub fields (Statut/TVA/MontantTotal) in Document models always empty |
