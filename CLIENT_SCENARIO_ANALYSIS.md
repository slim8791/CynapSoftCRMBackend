# CLIENT Scenario — Complete Analysis

> Generated: 2026-05-20  
> Roles covered: `PHARMACIEN`, `GROSSISTE`, `CLIENT`  
> Files read: 22 source files + 5 backend DTOs for field-mapping verification

---

## 1. API Endpoints Used

### OrderService

| Service method | HTTP | URL pattern | Description |
|---|---|---|---|
| `GetOrdersAsync` | GET | `orders?page={p}&pageSize={ps}[&statut={s}]` | All orders — DELEGUE/ADMIN only |
| `GetOrdersByStatusAsync` | GET | `orders/by-status?page={p}&pageSize={ps}[&statut={s}]` | Filter by status |
| `GetOrdersByClientAsync` | GET | `orders/by-client/{clientId}?page={p}&pageSize={ps}` | **CLIENT: own orders** |
| `GetOrderByIdAsync` | GET | `orders/{id}` | Single order detail |
| `GetLignesAsync` | GET | `orders/lignes?orderId={id}` | Order line items (declared but never called in VMs) |
| `CreateOrderAsync` | POST | `orders` | **CLIENT: create order** |
| `UpdateOrderStatusAsync` | PUT | `orders/{id}/status` | Admin status update |
| `CancelOrderAsync` | PUT | `orders/{id}/cancel?motif={motif}` | **CLIENT: cancel order** |
| `CreateReclamationAsync` | POST | `orders/reclamations` | **CLIENT: submit claim** |
| `GetReclamationsAsync` | GET | `orders/reclamations?orderId={id}` | Declared but never called in VMs |

### DocumentService

| Service method | HTTP | URL pattern | Description |
|---|---|---|---|
| `GetDocumentsByClientAndTypeAsync` | GET | `documents/client/{clientId}/type/{type}` | **Unified list for CLIENT** — type = `FACTURE`, `BC`, `BL` |
| `GetFacturesByCommandeAsync` | GET | `documents/factures/commande/{id}` | Factures linked to an order |
| `GetBCByCommandeAsync` | GET | `documents/bons-commandes/commande/{id}` | BCs linked to an order |
| `GetBLByCommandeAsync` | GET | `documents/bons-livraison/commande/{id}` | BLs linked to an order |
| `GetFacturesAsync` | GET | `documents/factures?page={p}&size={s}` | Declared but not used in VMs |
| `GetBonsCommandeAsync` | GET | `documents/bons-commandes?page={p}&size={s}` | Declared but not used in VMs |
| `GetBonsLivraisonAsync` | GET | `documents/bons-livraison?page={p}&size={s}` | Declared but not used in VMs |
| `GetFactureByIdAsync` | GET | `documents/factures/{id}` | Declared but not used in VMs |
| `GetBonCommandeByIdAsync` | GET | `documents/bons-commandes/{id}` | Declared but not used in VMs |
| `GetBonLivraisonByIdAsync` | GET | `documents/bons-livraison/{id}` | Declared but not used in VMs |

### ProductService (used by CreateOrderViewModel)

| Service method | HTTP | URL pattern | Description |
|---|---|---|---|
| `GetVisibleProductsAsync` | GET | `products/visible` | **CLIENT catalogue list** — active + non-archived |
| `GetProductsAsync` | GET | `products/search?keyword={kw}&isActive=true&limit={n}` | Search in order creation |
| `GetProductByIdAsync` | GET | `products/{id}` | Preload product from deeplink |
| `GetPromotionsAsync` | GET | `products/promos` | Seed local promo cache at startup |

---

## 2. Models — Field Mapping Issues

> Backend serializes with ASP.NET Core default camelCase.  
> `PropertyNameCaseInsensitive = true` handles simple PascalCase ↔ camelCase.  
> It does **NOT** resolve underscore mismatches (e.g., `id_Commande` ≠ `commandeId`).

### Order.cs

| Field | C# name | C# type | JSON key (backend) | Status |
|---|---|---|---|---|
| Id | `Id` | `int` | `id_Commande` | ✅ `[JsonPropertyName("id_Commande")]` |
| Date | `DateCommande` | `DateTime` | `dateCommande` | ✅ case-insensitive match |
| Statut | `Statut` | `int` | `statut` (EtatCommande int) | ✅ correct |
| Amount | `MontantTotal` | `decimal` | `montantTotalHT` | ✅ `[JsonPropertyName("montantTotalHT")]` |
| ClientId | `ClientId` | `int` | `id_Client` | ✅ `[JsonPropertyName("id_Client")]` |
| Notes | `Notes` | `string?` | `notes` | ✅ case-insensitive |
| Cancellation | `MotifAnnulation` | `string?` | `motifAnnulation` | ✅ case-insensitive |
| Lines | `Lignes` | `List<LigneCommande>` | `lignes` | ✅ case-insensitive |
| **Missing** | — | — | `montantTTC` | ❌ `MontantTTC` not mapped in MAUI |

### LigneCommande.cs

| Field | C# name | C# type | JSON key (backend) | Status |
|---|---|---|---|---|
| Id | `Id` | `int` | `id_Ligne` | ✅ `[JsonPropertyName("id_Ligne")]` |
| OrderId | `CommandeId` | `int` | `id_Commande` | ✅ `[JsonPropertyName("id_Commande")]` |
| ProductId | `ProductId` | `int` | `id_Produit` | ✅ `[JsonPropertyName("id_Produit")]` |
| Qty | `Quantite` | `int` | `quantite` | ✅ case-insensitive |
| Price | `PrixUnitaire` | `decimal` | `prixUnitaire` | ✅ case-insensitive |
| Discount | `Remise` | `decimal` | `remise` | ✅ case-insensitive |
| Lot | `NumeroLot` | `string` | `numeroLot` | ✅ case-insensitive |
| **Missing** | `ProductNom` | `string` | **not in backend DTO** | ❌ always empty; `DisplayName` always falls back to `"Produit #{ProductId}"` |
| **Wrong formula** | `SousTotal` (computed) | `decimal` | — | ❌ `Quantite * PrixUnitaire` — does not apply Remise. Backend formula is `PrixUnitaire × Quantite × (1 − Remise/100)` |

### Facture.cs

| Field | C# name | C# type | JSON key (backend) | Status |
|---|---|---|---|---|
| Id | `Id` | `int` | `numero_Doc` | ❌ MAUI `Id` ≠ `numero_Doc` — always 0 |
| Number | `NumeroFacture` | `string` | `nom_Doc` | ❌ MAUI `NumeroFacture` ≠ `nom_Doc` — always empty |
| Date | `DateFacture` | `DateTime` | `dateFacture` | ✅ case-insensitive match (FactureDto) |
| OrderId | `CommandeId` | `int` | `id_Commande` | ❌ MAUI `CommandeId` ≠ `id_Commande` — always 0 |
| HT | `MontantHT` | `decimal` | `montantHT` | ✅ case-insensitive |
| TVA | `TVA` | `decimal` | **not in backend DTO** | ❌ always 0 |
| TTC | `MontantTTC` | `decimal` | `montantTTC` | ✅ case-insensitive |
| Status | `Statut` | `string` | **not in backend DTO** | ❌ always empty |

### BonCommande.cs

| Field | C# name | C# type | JSON key (backend) | Status |
|---|---|---|---|---|
| Id | `Id` | `int` | `numero_Doc` | ❌ always 0 |
| Number | `NumeroBon` | `string` | `nom_Doc` | ❌ always empty |
| Date | `DateEmission` | `DateTime` | `dateCreation` | ❌ different name — always `DateTime.MinValue` |
| OrderId | `CommandeId` | `int` | `id_Commande` | ❌ always 0 |
| Amount | `MontantTotal` | `decimal` | **not in backend DTO** | ❌ always 0 |
| Status | `Statut` | `string` | **not in backend DTO** | ❌ always empty |

### BonLivraison.cs

| Field | C# name | C# type | JSON key (backend) | Status |
|---|---|---|---|---|
| Id | `Id` | `int` | `numero_Doc` | ❌ always 0 |
| Number | `NumeroBon` | `string` | `nom_Doc` | ❌ always empty |
| Date | `DateLivraison` | `DateTime` | `dateCreation` | ❌ different name — always `DateTime.MinValue` |
| OrderId | `CommandeId` | `int` | `id_Commande` | ❌ always 0 |
| Status | `Statut` | `string` | **not in backend DTO** | ❌ always empty |

### DocumentSummary.cs (used by DocumentListViewModel)

Backend unified endpoint `GET documents/client/{id}/type/{type}` returns the base `DocumentDto` shape:  
`numero_Doc`, `nom_Doc`, `dateCreation`, `id_Commande`, `id_Client`, `typeDocument`

| Field | C# name | JSON key (backend) | Status |
|---|---|---|---|
| `Id` | `Id` | `numero_Doc` | ❌ won't map — always 0 |
| `Numero` | `Numero` | `nom_Doc` | ❌ won't map — always empty |
| `Date` | `Date` | `dateCreation` | ❌ different name — always `DateTime.MinValue` |
| `Type` | `Type` | `typeDocument` | ❌ won't map — always empty |
| `Statut` | `Statut` | **not in backend** | ❌ always empty |
| `Montant` | `Montant` | **not in backend** | ❌ always null |

> **Critical:** `DocumentListPage` will always show an empty list with all fields blank because none of the `DocumentSummary` fields correctly map to the `DocumentDto` JSON keys.

### Reclamation.cs

| Field | C# name | JSON key (backend) | Status |
|---|---|---|---|
| `Id` | `Id` | `id_Rec` | ✅ `[JsonPropertyName("id_Rec")]` |
| `CommandeId` | `CommandeId` | `id_Commande` | ✅ `[JsonPropertyName("id_Commande")]` |
| `LigneId` | `LigneId` | `id_Ligne` | ✅ `[JsonPropertyName("id_Ligne")]` |
| `Motif` | `Motif` | `message` | ✅ `[JsonPropertyName("message")]` |
| `DateCreation` | `DateCreation` | `dateReclamation` | ✅ `[JsonPropertyName("dateReclamation")]` |

### ApiResponse\<T\>

All fields have `[JsonPropertyName]` after recent fix. ✅

---

## 3. Order Flow — Current Implementation

### Step 1 — Products

1. `CreateOrderViewModel` constructor fires `InitializeAsync()` as fire-and-forget:
   - Loads persisted cart draft from `Preferences` (key `"draft_cart"`)
   - Fetches all promotions via `GET products/promos` and seeds local SQLite cache
2. User types in the search `Entry` (`Text="{Binding SearchQuery}"`):
   - **⚠️ No ReturnCommand or search trigger bound in XAML** — `SearchProductCommand` is never automatically invoked. The user must manually trigger search (e.g., by pressing Enter on device keyboard, which defaults to `ReturnType.Default` — no command attached).
3. When `SearchProductAsync` does fire: calls `GET /products/search?keyword=...&isActive=true&limit=20` (online) or SQLite cache (offline). Results filtered client-side by `p.Actif && !p.IsArchived`.
4. User taps a search result → `SelectProduct` sets `SelectedProduct`, clears `SearchResults`.
5. User enters `Quantity` (1–9999 validated via `[Range]` attribute).
6. Taps "Ajouter au panier" → `AddLineAsync`:
   - Queries local SQLite for active promotion on product
   - If promo found: `prixEffectif = prixOriginal × (1 - remise/100)`
   - If product already in cart: `existing.Quantite += Quantity` — **⚠️ no `OnPropertyChanged` fired on `CartLine` — UI does not update the displayed quantity**
   - If new: appends `CartLine` with promo-adjusted price
   - Saves cart to `Preferences`
7. Cart preview shown in the same Step 1 screen below the "Ajouter" button.

### Step 2 — Panier

1. Shows `CartLines` CollectionView with `Qté × Prix = Sous-total` formula per row.
2. Delete button (✕) per row calls `RemoveLineCommand`.
3. Promo badge shown when `HasPromo = true` with crossed-out original price.
4. Total card shows subtotal, savings, and grand total.
5. User taps "Suivant →" — validates cart is not empty, advances to Step 3.

### Step 3 — Confirm

1. Shows ✓ icon, "Commande prête à confirmer", and cart total amount.
2. User taps "Confirmer la commande" → `SubmitOrderAsync`:
   - Client-side validation: quantity 1–9999, remise 0–100, price > 0 per line
   - Builds payload:
     ```json
     {
       "Lignes": [
         { "Id_Produit": 12, "Quantite": 3, "PrixUnitaire": 15.500, "Remise": 10.0 }
       ]
     }
     ```
   - `id_Client` is **not** sent — backend extracts it from the JWT `NameIdentifier` claim
   - Calls `POST orders`
   - On success: clears cart cache, shows alert "Votre commande a été soumise", navigates to `//orders`

---

## 4. Business Logic Issues Found

| # | File | Issue | Impact |
|---|---|---|---|
| 1 | `OrderDetailPage.xaml:111` | `{Binding Order.Statut}` binds `int` to `Text` — shows `"2"` instead of `"Confirmée"` | Status label shows raw integer to user |
| 2 | `LigneCommande.cs:21` | `SousTotal = Quantite * PrixUnitaire` — ignores `Remise` | Inflated subtotals when discount > 0; order totals shown wrong |
| 3 | `LigneCommande.cs:14` | `ProductNom` has no matching backend field — always empty | All order line items show `"Produit #X"` instead of product name |
| 4 | `Facture.cs` | `Id`, `NumeroFacture`, `CommandeId`, `TVA`, `Statut` all fail to deserialize | Facture detail page shows all zeroes/blanks |
| 5 | `BonCommande.cs` | `Id`, `NumeroBon`, `DateEmission`, `CommandeId`, `MontantTotal`, `Statut` all fail | BC detail page shows all zeroes/blanks |
| 6 | `BonLivraison.cs` | Same structural issues as BonCommande + `DateLivraison` vs `dateCreation` | BL detail page shows all zeroes/blanks |
| 7 | `DocumentSummary.cs` | All 6 fields fail to map to backend `DocumentDto` JSON keys | Document list always empty — entire Documents tab non-functional |
| 8 | `OrderListViewModel.cs:101-102` | `GetOrdersByClientAsync` ignores `statut` parameter — status filter chips do nothing for CLIENT | Filter UI is cosmetic-only for CLIENT/PHARMACIEN/GROSSISTE |
| 9 | `CreateOrderViewModel.cs:169-172` | `existing.Quantite += Quantity` mutates `CartLine` without `OnPropertyChanged` | Quantity shown in cart does not update in UI when same product added twice |
| 10 | `CreateOrderViewModel.cs:151` | `LigneId = Lignes.FirstOrDefault()?.Id ?? 0` for reclamation | Réclamation always references first line item; user cannot choose |
| 11 | `OrderDetailViewModel.cs:36-38` | `IsDelivered`, `IsAnnulee`, `CanCancel` correct only after `Order` loads; XAML binds these before load | Buttons briefly incorrect state on page open |
| 12 | `CreateOrderPage.xaml:209-217` | Search `Entry` has no `ReturnCommand` or trigger bound | Product search never fires unless keyboard return is pressed — undiscoverable UX |
| 13 | `CreateOrderViewModel.cs:99` | `GetProductsAsync(SearchQuery, 20)` uses `/products/search` (all products including archived) for order creation, while list page uses `/products/visible` | Archived or inactive products can appear in order creation search |
| 14 | `DocumentListViewModel.cs:49-50` | `if (!int.TryParse(userIdStr, out var clientId)) return;` silently returns empty — no error message | Documents tab appears broken without feedback if userId not found |
| 15 | `DocumentListViewModel.cs:35-43` | `LoadAsync` only triggered by `OnSelectedTypeIndexChanged`; default index is 0 so change never fires on first navigation | Documents list empty on first page open unless user switches tabs |
| 16 | `OrderDetailViewModel.cs:64` | `LoadLinkedDocumentsAsync(order.Id)` fires 3 sequential API calls — if `order.Id = 0` (failed deserialization), all 3 calls use `commandeId=0` | Linked documents always empty if Order.Id didn't deserialize |
| 17 | `OrderService.cs:39-40` | `UpdateOrderStatusAsync` sends `{ Status = status }` but `OrderController` method is restricted to `ADMIN,SUPERVISEUR` — CLIENT cannot call this | No issue for CLIENT, but dead code in OrderService for this role |
| 18 | `CreateOrderViewModel.cs:219` | `SubmitOrderAsync` does not re-validate that `CartLines.Count > 0` — only `NextStep()` checks this | Possible empty-order submit if cart cleared between Step 2 and Step 3 |

---

## 5. Missing Features

### ReclamationListViewModel / ReclamationDetailViewModel / ReclamationListPage
- **Entirely absent.** `ViewModels/Reclamations/` directory does not exist. `Views/Reclamations/` directory does not exist.
- Only reclamation functionality: embedded form in `OrderDetailPage`, only accessible when `Order.Statut == 5` (LIVREE).
- A CLIENT cannot view their past réclamations.
- A CLIENT cannot file a réclamation for an order in state EXPEDIEE (4) even if delivery issue.

### Document Download / Share Flow
- `DocumentListPage` taps navigate to `DocumentDetailPage`.
- `DocumentDetailPage` has a "Partager ce document" button bound to `ShareCommand`.
- **No download action** (save PDF locally) exists.
- `DocumentViewerPage` exists for PDF viewing but is only accessible from `ProductDetailPage` (product marketing documents), not from the document list.

### Status Filter for CLIENT Orders
- `OrderListPage` shows filter chips "En attente", "Confirmée", etc.
- For CLIENT role, `FetchPageAsync` calls `GetOrdersByClientAsync` which has no `statut` parameter.
- **Status filter silently ignored.** All CLIENT orders are returned regardless of selected chip.
- Fix: add `int? statut` param to `GetOrdersByClientAsync` and pass it through.

### Order Tracking Timeline
- No visual progression indicator (EN_ATTENTE → CONFIRMEE → EN_PREPARATION → EXPEDIEE → LIVREE).
- User only sees the current status badge.

### Cart Isolation per User
- Cart draft is stored in `Preferences` with key `"draft_cart"` (device-wide, not user-scoped).
- If two users log in on the same device, they share the same draft cart.

### Order Cancellation Success Feedback
- `CancelOrderAsync` in `OrderDetailViewModel` does not show a success alert.
- Page silently reloads after cancellation.

### Pagination in DocumentList
- `DocumentListViewModel` loads all documents at once via `GetDocumentsByClientAndTypeAsync`.
- No page/size parameters sent; no `HasMore` logic; no infinite scroll.

### Reclamation for Non-LIVREE Orders
- `IsDelivered` check restricts reclamation button to `Statut == 5` (LIVREE).
- CLIENT cannot file a complaint for a EXPEDIEE (4) order with wrong items, or ANNULEE (6) dispute.

---

## 6. ViewModel State Issues

### OrderListViewModel

| Property / Command | Issue |
|---|---|
| `_isClient`, `_clientId` | Set inside `LoadAsync` — if `StatusFilter` changes while busy, a second load re-reads SecureStorage. No race-condition guard. |
| `HasMore = result.Count == 20` | If last page contains exactly 20 items, one extra empty API call is made. |
| `StatusFilter` change | Triggers `LoadAsync` which re-reads role from `SecureStorage` every time — correct but slow. |
| `FetchPageAsync` for CLIENT | Calls `GetOrdersByClientAsync` with no `statut` — filter chips are ignored. |

### OrderDetailViewModel

| Property / Command | Issue |
|---|---|
| `Order` | Null until `LoadAsync` completes. XAML binds `Order.Statut`, `Order.MontantTotal`, etc. — silently shows defaults (0, blank) before load. |
| `Order.Statut` in XAML | Bound as `int` to `Text` — shows "2" not "Confirmée". Needs `StatutFrançais`. |
| `Lignes` | Populated from `order.Lignes` (inline in response). `LigneCommande.ProductNom` will always be empty (backend doesn't include it). |
| `LoadLinkedDocumentsAsync` | 3 sequential awaits — if first fails, others still run. No partial-failure handling. |
| `CancelOrderAsync` | No success alert. No error on `CancelOrderAsync` returning false. |
| `SubmitReclamationAsync` | `LigneId = Lignes.FirstOrDefault()?.Id ?? 0` — always first line or 0. No line selection UI. |

### CreateOrderViewModel

| Property / Command | Issue |
|---|---|
| `InitializeAsync()` | Fire-and-forget in constructor. Exceptions silently swallowed. |
| `CartLines` (ObservableCollection) | `CartLine` does not implement `INotifyPropertyChanged`. `existing.Quantite += Quantity` does not update the UI. |
| `SearchProductAsync` | Not auto-triggered by `SearchQuery` changes. No `ReturnCommand` in XAML. User must press keyboard Enter — undiscoverable. |
| `SelectedProduct` | Null before user selects. `AddLineAsync` guards against null. |
| `CartTotal`, `CartSavings` | Computed from `CartLines.Sum(...)` — manually `OnPropertyChanged` called after each mutation. If mutation happens without notify, totals stale. |
| `SubmitOrderAsync` | Does not check `CartLines.Count > 0` again — only `NextStep()` does. |
| `PrixUnitaire <= 0` check | Will reject lines if product price was 0 at time of adding (was failing before `prixVente` fix). |

### DocumentListViewModel

| Property / Command | Issue |
|---|---|
| `LoadAsync` trigger | Only fires from `OnSelectedTypeIndexChanged`. Index defaults to 0; page loads without documents unless user switches tabs or page re-appears. |
| `clientId` from SecureStorage | Silent `return` (no error message) if parse fails. |
| `DocumentSummary` fields | All fields fail to map from backend `DocumentDto` JSON keys — list always empty even when API returns data. |
| `RefreshCommand` | Bound to `RetryAsync → LoadAsync` — works correctly once `SelectedTypeIndex` is set. |
| `GoToDetailAsync` | Navigates with `doc.Type` which will always be `""` due to mapping failure — `DocumentDetailPage` may fail to determine document type. |

---

## 7. Complete Code of Each File

---

### Models/Orders/Order.cs

```csharp
using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Orders;
public class Order
{
    [JsonPropertyName("id_Commande")]
    public int Id { get; set; }

    public string NumeroCommande => $"CMD-{Id:D5}";

    public DateTime DateCommande { get; set; }

    // EtatCommande: 0=Brouillon 1=EnAttente 2=Confirmee 3=EnPreparation 4=Expediee 5=Livree 6=Annulee
    public int Statut { get; set; }

    public string StatutFrançais => Statut switch
    {
        0 => "Brouillon",
        1 => "En attente",
        2 => "Confirmée",
        3 => "En préparation",
        4 => "Expédiée",
        5 => "Livrée",
        6 => "Annulée",
        _ => $"Statut {Statut}"
    };

    [JsonPropertyName("montantTotalHT")]
    public decimal MontantTotal { get; set; }

    [JsonPropertyName("id_Client")]
    public int ClientId { get; set; }

    public string? Notes { get; set; }
    public string? MotifAnnulation { get; set; }
    public bool IsDeleted { get; set; }
    public List<LigneCommande>  Lignes       { get; set; } = new();
    public List<Reclamation>?   Reclamations { get; set; }
}
```

---

### Models/Orders/LigneCommande.cs

```csharp
using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Orders;
public class LigneCommande
{
    [JsonPropertyName("id_Ligne")]
    public int Id { get; set; }

    [JsonPropertyName("id_Commande")]
    public int CommandeId { get; set; }

    [JsonPropertyName("id_Produit")]
    public int ProductId { get; set; }

    public string ProductNom { get; set; } = string.Empty;
    public string DisplayName => string.IsNullOrEmpty(ProductNom) ? $"Produit #{ProductId}" : ProductNom;
    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public string NumeroLot { get; set; } = string.Empty;
    public decimal Remise { get; set; }
    public decimal SousTotal => Quantite * PrixUnitaire;
}
```

---

### Models/Orders/Reclamation.cs

```csharp
using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Orders;
public class Reclamation
{
    [JsonPropertyName("id_Rec")]
    public int Id { get; set; }

    [JsonPropertyName("id_Commande")]
    public int CommandeId { get; set; }

    [JsonPropertyName("id_Ligne")]
    public int LigneId { get; set; }

    [JsonPropertyName("message")]
    public string Motif { get; set; } = string.Empty;

    [JsonPropertyName("dateReclamation")]
    public DateTime DateCreation { get; set; }

    public string? Statut { get; set; }
}
```

---

### Models/Documents/Facture.cs

```csharp
namespace Cynapharm_Mobile.Models.Documents;
public class Facture
{
    public int Id { get; set; }
    public string NumeroFacture { get; set; } = string.Empty;
    public DateTime DateFacture { get; set; }
    public int CommandeId { get; set; }
    public decimal MontantHT { get; set; }
    public decimal TVA { get; set; }
    public decimal MontantTTC { get; set; }
    public string Statut { get; set; } = string.Empty;
}
```

---

### Models/Documents/BonLivraison.cs

```csharp
namespace Cynapharm_Mobile.Models.Documents;
public class BonLivraison
{
    public int Id { get; set; }
    public string NumeroBon { get; set; } = string.Empty;
    public DateTime DateLivraison { get; set; }
    public int CommandeId { get; set; }
    public string Statut { get; set; } = string.Empty;
}
```

---

### Models/Documents/BonCommande.cs

```csharp
namespace Cynapharm_Mobile.Models.Documents;
public class BonCommande
{
    public int Id { get; set; }
    public string NumeroBon { get; set; } = string.Empty;
    public DateTime DateEmission { get; set; }
    public int CommandeId { get; set; }
    public decimal MontantTotal { get; set; }
    public string Statut { get; set; } = string.Empty;
}
```

---

### Models/Common/ApiResponse.cs

```csharp
using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Common;

public class ApiResponse<T>
{
    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; set; } = true;

    [JsonPropertyName("result")]
    public T? Result { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("errors")]
    public List<string>? Errors { get; set; }
}
```

---

### Services/OrderService.cs

```csharp
using Cynapharm_Mobile.Models.Orders;

namespace Cynapharm_Mobile.Services;

public class OrderService
{
    private readonly ApiService _api;
    public OrderService(ApiService api) { _api = api; }

    /// <summary>GET orders?page={page}&pageSize={pageSize}&statut={statut} — DELEGUE/ADMIN/SUPERVISEUR</summary>
    public Task<List<Order>?> GetOrdersAsync(int? statut = null, int page = 1, int pageSize = 20)
    {
        var url = $"orders?page={page}&pageSize={pageSize}";
        if (statut.HasValue) url += $"&statut={statut.Value}";
        return _api.GetAsync<List<Order>>(url);
    }

    /// <summary>GET orders/by-status?statut={statut}&page={page}&pageSize={size}</summary>
    public Task<List<Order>?> GetOrdersByStatusAsync(int? statut, int page = 1, int size = 20)
    {
        var url = $"{ApiRoutes.Orders.ByStatus}?page={page}&pageSize={size}";
        if (statut.HasValue) url += $"&statut={statut.Value}";
        return _api.GetAsync<List<Order>>(url);
    }

    /// <summary>GET orders/by-client/{id}?page={page}&pageSize={size}</summary>
    public Task<List<Order>?> GetOrdersByClientAsync(int clientId, int page = 1, int size = 20)
        => _api.GetAsync<List<Order>>($"{ApiRoutes.Orders.ByClient}/{clientId}?page={page}&pageSize={size}");

    public Task<Order?> GetOrderByIdAsync(int id)
        => _api.GetAsync<Order>($"orders/{id}");

    public Task<List<LigneCommande>?> GetLignesAsync(int orderId)
        => _api.GetAsync<List<LigneCommande>>($"orders/lignes?orderId={orderId}");

    public Task<Order?> CreateOrderAsync(object request)
        => _api.PostAsync<Order>("orders", request);

    public Task<Order?> UpdateOrderStatusAsync(int id, string status)
        => _api.PutAsync<Order>($"orders/{id}/status", new { Status = status });

    /// <summary>PUT orders/{id}/cancel?motif={motif}</summary>
    public Task<object?> CancelOrderAsync(int id, string motif)
        => _api.PutAsync<object>(
            $"{string.Format(ApiRoutes.Orders.Cancel, id)}?motif={Uri.EscapeDataString(motif)}",
            new { });

    public Task<Reclamation?> CreateReclamationAsync(Reclamation reclamation)
        => _api.PostAsync<Reclamation>("orders/reclamations", reclamation);

    public Task<List<Reclamation>?> GetReclamationsAsync(int? orderId)
        => _api.GetAsync<List<Reclamation>>($"orders/reclamations?orderId={orderId}");
}
```

---

### Services/DocumentService.cs

```csharp
using Cynapharm_Mobile.Models.Documents;

namespace Cynapharm_Mobile.Services;

public class DocumentService
{
    private readonly ApiService _api;
    public DocumentService(ApiService api) { _api = api; }

    public Task<List<Facture>?> GetFacturesAsync(int page = 1, int size = 20)
        => _api.GetAsync<List<Facture>>($"documents/factures?page={page}&size={size}");

    public Task<Facture?> GetFactureByIdAsync(int id)
        => _api.GetAsync<Facture>($"documents/factures/{id}");

    public Task<List<BonCommande>?> GetBonsCommandeAsync(int page = 1, int size = 20)
        => _api.GetAsync<List<BonCommande>>($"documents/bons-commandes?page={page}&size={size}");

    public Task<BonCommande?> GetBonCommandeByIdAsync(int id)
        => _api.GetAsync<BonCommande>($"documents/bons-commandes/{id}");

    public Task<List<BonLivraison>?> GetBonsLivraisonAsync(int page = 1, int size = 20)
        => _api.GetAsync<List<BonLivraison>>($"documents/bons-livraison?page={page}&size={size}");

    public Task<BonLivraison?> GetBonLivraisonByIdAsync(int id)
        => _api.GetAsync<BonLivraison>($"documents/bons-livraison/{id}");

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

---

### Services/ProductService.cs

```csharp
using Cynapharm_Mobile.Models.Products;

namespace Cynapharm_Mobile.Services;

public class ProductService
{
    private readonly ApiService _api;
    public ProductService(ApiService api) { _api = api; }

    private static readonly HashSet<string> _imageExts =
        new(StringComparer.OrdinalIgnoreCase) { "jpg", "png", "webp" };

    private static string? ExtractImageUrl(Product p) =>
        p.Supports?
            .FirstOrDefault(s => s.IsActive &&
                string.Equals(s.Type, "Image", StringComparison.OrdinalIgnoreCase))
            ?.Fichiers?
            .FirstOrDefault(f => _imageExts.Contains(f.Extension))
            ?.Url;

    public async Task<List<Product>?> GetProductsAsync(string? search = null, int limit = 100)
    {
        List<Product>? result;
        if (!string.IsNullOrWhiteSpace(search))
            result = await _api.GetAsync<List<Product>>(
                $"products/search?keyword={Uri.EscapeDataString(search)}&isActive=true&limit={limit}");
        else
            result = await _api.GetAsync<List<Product>>("products");

        if (result != null)
            foreach (var p in result)
                p.ImageUrl ??= ExtractImageUrl(p);

        return result;
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        var product = await _api.GetAsync<Product>($"products/{id}");
        if (product != null)
            product.ImageUrl ??= ExtractImageUrl(product);
        return product;
    }

    public Task<List<Lot>?> GetLotsByProductAsync(int productId)
        => _api.GetAsync<List<Lot>>($"products/lots/{productId}");

    public Task<List<Promotion>?> GetPromotionsAsync(int? productId)
    {
        var url = "products/promos";
        if (productId.HasValue) url += $"?productId={productId.Value}";
        return _api.GetAsync<List<Promotion>>(url);
    }

    /// <summary>GET products/visible — active, non-archived products only (CLIENT + MEDECIN).</summary>
    public async Task<List<Product>?> GetVisibleProductsAsync()
    {
        var result = await _api.GetAsync<List<Product>>("products/visible");
        if (result != null)
            foreach (var p in result)
                p.ImageUrl ??= ExtractImageUrl(p);
        return result;
    }

    public Task<List<string>?> GetCategoriesAsync()
        => _api.GetAsync<List<string>>("products/categories");

    public Task<object?> GetMarketingAsync(int? productId)
    {
        var url = "products/marketing";
        if (productId.HasValue) url += $"?productId={productId.Value}";
        return _api.GetAsync<object>(url);
    }

    public Task<byte[]?> DownloadFileAsync(string url)
        => _api.DownloadFileAsync(url);
}
```

---

### ViewModels/Orders/OrderListViewModel.cs

```csharp
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Orders;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Orders;

public partial class OrderListViewModel : BaseViewModel
{
    private readonly OrderService _orderService;

    public ObservableCollection<Order> Orders { get; } = new();

    public List<string> StatusOptions { get; } = new()
    {
        "Tous", "En attente", "Confirmée", "En préparation", "Expédiée", "Livrée", "Annulée"
    };

    private static readonly Dictionary<string, int?> _statusCodeMap = new()
    {
        { "En attente",     1 },
        { "Confirmée",      2 },
        { "En préparation", 3 },
        { "Expédiée",       4 },
        { "Livrée",         5 },
        { "Annulée",        6 },
    };

    [ObservableProperty] private string _statusFilter = "Tous";
    [ObservableProperty] private bool   _isGrossiste;

    private int  _currentPage = 1;
    private bool _isClient;
    private int  _clientId;

    [ObservableProperty] private bool _hasMore;

    public OrderListViewModel(OrderService orderService)
    {
        _orderService = orderService;
        Title = "Commandes";
    }

    partial void OnStatusFilterChanged(string value) => _ = LoadAsync();

    [RelayCommand]
    private Task LoadAsync() => ExecuteAsync(async () =>
    {
        var role = await SecureStorage.GetAsync(StorageKeys.UserRole) ?? string.Empty;
        IsGrossiste = role is "GROSSISTE";
        _isClient   = role is "PHARMACIEN" or "GROSSISTE" or "CLIENT";

        var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
        if (!int.TryParse(userIdStr, out _clientId) || _clientId <= 0)
        {
            if (_isClient)
            {
                ErrorMessage = "Impossible d'identifier votre compte client. Veuillez vous reconnecter.";
                return;
            }
        }

        if (!await CheckConnectivityAsync()) return;
        _currentPage = 1;
        Orders.Clear();

        var result = await FetchPageAsync(_currentPage);
        if (result != null)
        {
            foreach (var o in result) Orders.Add(o);
            HasMore = result.Count == 20;
        }
    });

    [RelayCommand]
    private Task LoadMoreAsync()
    {
        if (!HasMore || IsBusy) return Task.CompletedTask;
        _currentPage++;
        return ExecuteUncheckedAsync(async () =>
        {
            var result = await FetchPageAsync(_currentPage);
            if (result != null)
            {
                foreach (var o in result) Orders.Add(o);
                HasMore = result.Count == 20;
            }
        });
    }

    private Task<List<Order>?> FetchPageAsync(int page)
    {
        _statusCodeMap.TryGetValue(StatusFilter, out var statut);

        if (_isClient && _clientId > 0)
            return _orderService.GetOrdersByClientAsync(_clientId, page, 20);

        return _orderService.GetOrdersAsync(statut, page, 20);
    }

    [RelayCommand] private Task RefreshAsync() => LoadAsync();
    protected override Task RetryAsync() => LoadAsync();

    [RelayCommand]
    private async Task GoToDetailAsync(Order? order)
    {
        if (order == null) return;
        await Shell.Current.GoToAsync($"//orders/detail?orderId={order.Id}");
    }

    [RelayCommand]
    private async Task CreateOrderAsync()
        => await Shell.Current.GoToAsync("//orders/create");

    [RelayCommand]
    private void SetStatusFilter(string status) => StatusFilter = status;
}
```

---

### ViewModels/Orders/OrderDetailViewModel.cs

```csharp
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Documents;
using Cynapharm_Mobile.Models.Orders;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Orders;

[QueryProperty(nameof(OrderId), "orderId")]
public partial class OrderDetailViewModel : BaseViewModel
{
    private readonly OrderService    _orderService;
    private readonly DocumentService _documentService;

    [ObservableProperty] private int    _orderId;
    [ObservableProperty] private Order? _order;
    [ObservableProperty] private string _reclamationMotif       = string.Empty;
    [ObservableProperty] private string _reclamationDescription = string.Empty;
    [ObservableProperty] private bool   _showReclamationForm;
    [ObservableProperty] private bool   _hasLinkedDocuments;

    public ObservableCollection<LigneCommande>   Lignes         { get; } = new();
    public ObservableCollection<DocumentSummary> LinkedFactures { get; } = new();
    public ObservableCollection<DocumentSummary> LinkedBC       { get; } = new();
    public ObservableCollection<DocumentSummary> LinkedBL       { get; } = new();

    public OrderDetailViewModel(OrderService orderService, DocumentService documentService)
    {
        _orderService    = orderService;
        _documentService = documentService;
        Title = "Commande";
    }

    public bool IsDelivered => Order?.Statut == 5; // Livree
    public bool IsAnnulee   => Order?.Statut == 6; // Annulee
    public bool CanCancel   => Order?.Statut is 0 or 1 or 2; // Brouillon, EnAttente, Confirmee

    partial void OnOrderIdChanged(int value) { if (value > 0) _ = LoadAsync(); }

    partial void OnOrderChanged(Order? value)
    {
        OnPropertyChanged(nameof(IsDelivered));
        OnPropertyChanged(nameof(IsAnnulee));
        OnPropertyChanged(nameof(CanCancel));
    }

    [RelayCommand]
    private Task LoadAsync() => ExecuteAsync(async () =>
    {
        if (!await CheckConnectivityAsync()) return;
        var order = await _orderService.GetOrderByIdAsync(OrderId);
        if (order != null)
        {
            Order = order;
            Title = $"Commande #{order.NumeroCommande}";
            Lignes.Clear();
            foreach (var l in order.Lignes) Lignes.Add(l);
            await LoadLinkedDocumentsAsync(order.Id);
        }
    });

    private async Task LoadLinkedDocumentsAsync(int commandeId)
    {
        LinkedFactures.Clear();
        LinkedBC.Clear();
        LinkedBL.Clear();

        var factures = await _documentService.GetFacturesByCommandeAsync(commandeId);
        if (factures != null)
            foreach (var f in factures)
                LinkedFactures.Add(new DocumentSummary
                {
                    Id = f.Id, Numero = f.NumeroFacture, Date = f.DateFacture,
                    Type = "FACTURE", Statut = f.Statut, Montant = f.MontantTTC
                });

        var bcs = await _documentService.GetBCByCommandeAsync(commandeId);
        if (bcs != null)
            foreach (var b in bcs)
                LinkedBC.Add(new DocumentSummary
                {
                    Id = b.Id, Numero = b.NumeroBon, Date = b.DateEmission,
                    Type = "BC", Statut = b.Statut, Montant = b.MontantTotal
                });

        var bls = await _documentService.GetBLByCommandeAsync(commandeId);
        if (bls != null)
            foreach (var bl in bls)
                LinkedBL.Add(new DocumentSummary
                {
                    Id = bl.Id, Numero = bl.NumeroBon, Date = bl.DateLivraison,
                    Type = "BL", Statut = bl.Statut
                });

        HasLinkedDocuments = LinkedFactures.Count > 0 || LinkedBC.Count > 0 || LinkedBL.Count > 0;
    }

    protected override Task RetryAsync() => LoadAsync();

    [RelayCommand]
    private Task CancelOrderAsync() => ExecuteAsync(async () =>
    {
        var motif = await Shell.Current.DisplayPromptAsync(
            "Annuler la commande", "Veuillez saisir le motif d'annulation :",
            accept: "Confirmer", cancel: "Retour", placeholder: "Motif...");
        if (motif == null) return;
        await _orderService.CancelOrderAsync(OrderId, motif.Trim());
        await LoadAsync();
    });

    [RelayCommand]
    private void ToggleReclamationForm() => ShowReclamationForm = !ShowReclamationForm;

    [RelayCommand]
    private Task SubmitReclamationAsync() => ExecuteAsync(async () =>
    {
        if (string.IsNullOrWhiteSpace(ReclamationMotif))
        {
            ErrorMessage = "Veuillez saisir un motif.";
            return;
        }
        var message = string.IsNullOrWhiteSpace(ReclamationDescription)
            ? ReclamationMotif
            : $"{ReclamationMotif}: {ReclamationDescription}";

        await _orderService.CreateReclamationAsync(new Reclamation
        {
            CommandeId   = OrderId,
            LigneId      = Lignes.FirstOrDefault()?.Id ?? 0,
            Motif        = message,
            DateCreation = DateTime.UtcNow
        });
        ShowReclamationForm    = false;
        ReclamationMotif       = string.Empty;
        ReclamationDescription = string.Empty;
        await Shell.Current.DisplayAlert("Succès", "Votre réclamation a été soumise.", "OK");
    });
}
```

---

### ViewModels/Orders/CreateOrderViewModel.cs

```csharp
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Orders;
using Cynapharm_Mobile.Models.Products;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Orders;

[QueryProperty(nameof(PreselectedProductId), "productId")]
public partial class CreateOrderViewModel : BaseViewModel
{
    private readonly OrderService _orderService;
    private readonly ProductService _productService;
    private readonly LocalDatabaseService _localDb;
    private const string CartCacheKey = "draft_cart";

    [ObservableProperty] private int _preselectedProductId;
    [ObservableProperty] private int _currentStep = 1;
    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private Product? _selectedProduct;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(1, 9999, ErrorMessage = "La quantité doit être comprise entre 1 et 9 999.")]
    [NotifyPropertyChangedFor(nameof(QuantityError))]
    private int _quantity = 1;

    public string QuantityError =>
        GetErrors(nameof(Quantity)).Cast<ValidationResult>().FirstOrDefault()?.ErrorMessage ?? string.Empty;

    public ObservableCollection<CartLine> CartLines { get; } = new();
    public ObservableCollection<Product> SearchResults { get; } = new();

    public decimal CartTotal      => CartLines.Sum(l => l.SousTotal);
    public decimal CartSavings    => CartLines.Sum(l => l.EconomieTotale);
    public bool    HasCartSavings => CartSavings > 0;

    public bool IsStep1    => CurrentStep == 1;
    public bool IsStep2    => CurrentStep == 2;
    public bool IsStep3    => CurrentStep == 3;
    public bool IsNotStep1 => CurrentStep > 1;
    public bool IsNotStep3 => CurrentStep < 3;

    public CreateOrderViewModel(OrderService orderService, ProductService productService, LocalDatabaseService localDb)
    {
        _orderService   = orderService;
        _productService = productService;
        _localDb        = localDb;
        Title = "Nouvelle commande";
        _ = InitializeAsync();
    }

    partial void OnPreselectedProductIdChanged(int value)
    {
        if (value > 0) _ = PreloadProductAsync(value);
    }

    partial void OnCurrentStepChanged(int value)
    {
        OnPropertyChanged(nameof(IsStep1));
        OnPropertyChanged(nameof(IsStep2));
        OnPropertyChanged(nameof(IsStep3));
        OnPropertyChanged(nameof(IsNotStep1));
        OnPropertyChanged(nameof(IsNotStep3));
    }

    private async Task PreloadProductAsync(int productId)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product != null) SelectedProduct = product;
    }

    [RelayCommand]
    private async Task SearchProductAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) { SearchResults.Clear(); return; }
        SetBusy(true);
        try
        {
            SearchResults.Clear();
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                var result = await _productService.GetProductsAsync(SearchQuery, 20);
                if (result != null)
                    foreach (var p in result.Where(p => p.Actif && !p.IsArchived))
                        SearchResults.Add(p);
            }
            else
            {
                var cached = await _localDb.SearchProductsAsync(SearchQuery);
                foreach (var e in cached)
                    SearchResults.Add(new Product
                    {
                        Id = e.Id, Reference = e.Reference, Nom = e.Nom,
                        Categorie = e.Categorie, PrixUnitaire = e.PrixUnitaire,
                        ImageUrl = e.ImageUrl, Actif = e.Actif
                    });
            }
            if (SearchResults.Count == 0) ErrorMessage = "Aucun produit trouvé pour cette recherche.";
        }
        catch (Exception) { ErrorMessage = "Erreur de recherche."; }
        finally { SetBusy(false); }
    }

    [RelayCommand]
    private void SelectProduct(Product product)
    {
        SelectedProduct = product;
        SearchResults.Clear();
        SearchQuery = product.Nom;
    }

    [RelayCommand]
    private async Task AddLineAsync()
    {
        ClearError();
        ValidateProperty(Quantity, nameof(Quantity));
        if (SelectedProduct == null || HasErrors)
        {
            ErrorMessage = SelectedProduct == null ? "Sélectionnez un produit avant d'ajouter." : QuantityError;
            return;
        }

        var prixOriginal = SelectedProduct.PrixUnitaire;
        var prixEffectif = prixOriginal;
        decimal remise   = 0;
        string? promoTitre = null;

        try
        {
            var promo = await _localDb.GetActivePromotionAsync(SelectedProduct.Id);
            if (promo != null && promo.RemisePourcentage > 0)
            {
                remise       = (decimal)promo.RemisePourcentage;
                prixEffectif = prixOriginal * (1m - remise / 100m);
                promoTitre   = promo.Titre;
            }
        }
        catch { }

        var existing = CartLines.FirstOrDefault(l => l.ProductId == SelectedProduct.Id);
        if (existing != null) { existing.Quantite += Quantity; }
        else
        {
            CartLines.Add(new CartLine
            {
                ProductId = SelectedProduct.Id, ProductNom = SelectedProduct.Nom,
                Quantite = Quantity, PrixOriginal = prixOriginal,
                PrixUnitaire = prixEffectif, RemisePourcentage = remise, PromoTitre = promoTitre
            });
        }

        OnPropertyChanged(nameof(CartTotal));
        OnPropertyChanged(nameof(CartSavings));
        OnPropertyChanged(nameof(HasCartSavings));

        SelectedProduct = null;
        SearchQuery     = string.Empty;
        Quantity        = 1;
        _ = SaveCartAsync();
    }

    [RelayCommand]
    private void RemoveLine(CartLine? line)
    {
        if (line == null) return;
        CartLines.Remove(line);
        OnPropertyChanged(nameof(CartTotal));
        OnPropertyChanged(nameof(CartSavings));
        OnPropertyChanged(nameof(HasCartSavings));
    }

    [RelayCommand]
    private void NextStep()
    {
        if (CurrentStep == 1 && CartLines.Count == 0) { ErrorMessage = "Ajoutez au moins un produit."; return; }
        if (CurrentStep < 3) { ClearError(); CurrentStep++; }
    }

    [RelayCommand]
    private void PreviousStep() { if (CurrentStep > 1) CurrentStep--; }

    [RelayCommand]
    private async Task SubmitOrderAsync()
    {
        ClearError();
        foreach (var line in CartLines)
        {
            if (line.Quantite < 1 || line.Quantite > 9999)
            { ErrorMessage = $"La quantité de « {line.ProductNom} » doit être entre 1 et 9 999."; return; }
            if (line.RemisePourcentage < 0 || line.RemisePourcentage > 100)
            { ErrorMessage = $"La remise de « {line.ProductNom} » doit être entre 0 et 100 %."; return; }
            if (line.PrixUnitaire <= 0)
            { ErrorMessage = $"Le prix unitaire de « {line.ProductNom} » doit être supérieur à 0."; return; }
        }

        if (!await CheckConnectivityAsync()) return;
        SetBusy(true);
        try
        {
            var payload = new
            {
                Lignes = CartLines.Select(l => new
                {
                    Id_Produit   = l.ProductId,
                    Quantite     = l.Quantite,
                    PrixUnitaire = l.PrixUnitaire,
                    Remise       = l.RemisePourcentage
                }).ToList()
            };
            await _orderService.CreateOrderAsync(payload);
            ClearCartCache();
            await Shell.Current.DisplayAlert("Succès", "Votre commande a été soumise.", "OK");
            await Shell.Current.GoToAsync("//orders");
        }
        catch (Exception) { ErrorMessage = "Erreur lors de la soumission de la commande."; }
        finally { SetBusy(false); }
    }

    private async Task InitializeAsync()
    {
        await LoadCartAsync();
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            try
            {
                var promos = await _productService.GetPromotionsAsync(null);
                if (promos != null && promos.Count > 0)
                    await _localDb.SeedPromotionsAsync(promos);
            }
            catch { }
        }
    }

    private Task SaveCartAsync()
    {
        try { Preferences.Set(CartCacheKey, System.Text.Json.JsonSerializer.Serialize(CartLines.ToList())); }
        catch { }
        return Task.CompletedTask;
    }

    private Task LoadCartAsync()
    {
        try
        {
            var json = Preferences.Get(CartCacheKey, string.Empty);
            if (!string.IsNullOrEmpty(json))
            {
                var items = System.Text.Json.JsonSerializer.Deserialize<List<CartLine>>(json);
                if (items != null)
                {
                    CartLines.Clear();
                    foreach (var item in items) CartLines.Add(item);
                    OnPropertyChanged(nameof(CartTotal));
                    OnPropertyChanged(nameof(CartSavings));
                    OnPropertyChanged(nameof(HasCartSavings));
                }
            }
        }
        catch { }
        return Task.CompletedTask;
    }

    private void ClearCartCache() => Preferences.Remove(CartCacheKey);
}
```

---

### ViewModels/Documents/DocumentListViewModel.cs

```csharp
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Documents;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Documents;

public partial class DocumentListViewModel : BaseViewModel
{
    private readonly DocumentService _documentService;

    public ObservableCollection<DocumentSummary> Documents { get; } = new();
    public List<string> TypeOptions { get; } = new() { "facture", "bon-commande", "bon-livraison" };
    public List<string> TypeLabels  { get; } = new() { "Factures", "Bons de commande", "Bons de livraison" };

    [ObservableProperty] private string _documentType    = "facture";
    [ObservableProperty] private int    _selectedTypeIndex;

    private static readonly Dictionary<string, string> _apiTypeMap = new()
    {
        { "facture",        "FACTURE" },
        { "bon-commande",   "BC"      },
        { "bon-livraison",  "BL"      },
    };

    public DocumentListViewModel(DocumentService documentService)
    {
        _documentService = documentService;
        Title = "Documents";
    }

    partial void OnSelectedTypeIndexChanged(int value)
    {
        if (value >= 0 && value < TypeOptions.Count)
        {
            DocumentType = TypeOptions[value];
            _ = LoadAsync();
        }
    }

    [RelayCommand]
    private Task LoadAsync() => ExecuteAsync(async () =>
    {
        if (!await CheckConnectivityAsync()) return;

        var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
        if (!int.TryParse(userIdStr, out var clientId)) return;

        if (!_apiTypeMap.TryGetValue(DocumentType, out var apiType)) return;

        var docs = await _documentService.GetDocumentsByClientAndTypeAsync(clientId, apiType);
        Documents.Clear();
        if (docs != null)
            foreach (var d in docs)
                Documents.Add(d);
    });

    protected override Task RetryAsync() => LoadAsync();

    [RelayCommand]
    private async Task GoToDetailAsync(DocumentSummary? doc)
    {
        if (doc == null) return;
        await Shell.Current.GoToAsync($"//documents/detail?documentType={doc.Type}&documentId={doc.Id}");
    }

    [RelayCommand]
    private void SetTypeIndex(string index)
    {
        if (int.TryParse(index, out var i)) SelectedTypeIndex = i;
    }
}
```

---

### ViewModels/Reclamations — ABSENT

> `ViewModels/Reclamations/` directory does not exist.  
> `Views/Reclamations/` directory does not exist.  
> There is no `ReclamationListViewModel`, `ReclamationDetailViewModel`, `ReclamationListPage`, or `ReclamationDetailPage`.

---

### Views/Orders/OrderListPage.xaml — see section 7 full XAML above (see file read output)

*(Full XAML at 240 lines read above — omitted here for brevity; save the full read output separately if needed.)*

---

### Views/Orders/OrderDetailPage.xaml — notable binding issue

Line 111:
```xml
<Label Text="{Binding Order.Statut}"     <!-- ❌ shows integer e.g. "2", not "Confirmée" -->
       FontSize="14" FontAttributes="Bold"
       TextColor="{Binding Order.Statut, Converter={StaticResource StatusColorConverter}}" />
```
Should be `{Binding Order.StatutFrançais}` for the `Text` binding.

---

## Summary — Priority Fix List

| Priority | Issue | File(s) |
|---|---|---|
| 🔴 **Critical** | `DocumentSummary` fields all fail to map from backend `DocumentDto` keys | `Models/Documents/DocumentSummary.cs` + `Facture.cs` + `BonCommande.cs` + `BonLivraison.cs` |
| 🔴 **Critical** | `OrderDetailPage` shows integer status (`"2"`) not French label | `Views/Orders/OrderDetailPage.xaml` line 111 |
| 🔴 **Critical** | `LigneCommande.ProductNom` never populated — all order lines show `"Produit #X"` | `Models/Orders/LigneCommande.cs` + backend must include product name |
| 🟠 **High** | Status filter ignored for CLIENT orders | `ViewModels/Orders/OrderListViewModel.cs` + `Services/OrderService.cs` |
| 🟠 **High** | `DocumentListViewModel` does not load on page open (index already 0) | `ViewModels/Documents/DocumentListViewModel.cs` |
| 🟠 **High** | Search in CreateOrder never auto-fires | `Views/Orders/CreateOrderPage.xaml` — missing `ReturnCommand` |
| 🟡 **Medium** | `LigneCommande.SousTotal` ignores Remise discount | `Models/Orders/LigneCommande.cs` |
| 🟡 **Medium** | Cart same-product update doesn't notify UI | `ViewModels/Orders/CreateOrderViewModel.cs` |
| 🟡 **Medium** | Reclamation hardcoded to first ligne | `ViewModels/Orders/OrderDetailViewModel.cs` |
| 🟢 **Low** | No ReclamationListPage | New feature required |
| 🟢 **Low** | Cart not scoped per user | `ViewModels/Orders/CreateOrderViewModel.cs` |
