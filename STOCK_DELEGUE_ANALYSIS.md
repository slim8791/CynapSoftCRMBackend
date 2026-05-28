# Stock Délégué — Complete Analysis

> Generated: 2026-05-25  
> Branch: dev/Mobile-0001  
> Scope: InventoryAPI backend + Angular inventory feature

---

## 1. Backend Endpoints Inventory

### StocksDelegueController — `[Route("api/stocks-delegue")]`

| Method | Route | Roles | Description | Status |
|--------|-------|-------|-------------|--------|
| GET | `/api/stocks-delegue` | ADMIN, SUPERVISEUR | Paginated list (`pageNumber`, `pageSize`) | ✅ OK |
| GET | `/api/stocks-delegue/{id}` | ADMIN, SUPERVISEUR, DELEGUE | Get by ID | ✅ OK |
| GET | `/api/stocks-delegue/by-delegue/{id}` | ADMIN, SUPERVISEUR, DELEGUE | All stocks for one delegue | ✅ OK |
| GET | `/api/stocks-delegue/by-produit/{id}` | ADMIN, SUPERVISEUR | All stocks for a product | ✅ OK |
| GET | `/api/stocks-delegue/by-lot/{numero}` | ADMIN, SUPERVISEUR | Returns **first match** only | ⚠️ See issue #5 |
| POST | `/api/stocks-delegue` | ADMIN, SUPERVISEUR | Create (id=0) or update (id>0) | ❌ DateExpiration not saved on create |
| DELETE | `/api/stocks-delegue/{id}?type=` | ADMIN | Soft delete; blocked if QteDisponible > 0 | ✅ OK |

### DistributionController — `[Route("api/distributions")]`

| Method | Route | Roles | Description | Status |
|--------|-------|-------|-------------|--------|
| POST | `/api/distributions` | ADMIN, SUPERVISEUR, DELEGUE | Create (Id_Distribution=0) or update | ⚠️ Update path doesn't adjust stock qty |
| GET | `/api/distributions` | ADMIN, SUPERVISEUR | Paginated list | ✅ OK |
| GET | `/api/distributions/{id}` | ADMIN, SUPERVISEUR, DELEGUE | Get by ID | ✅ OK |
| GET | `/api/distributions/by-medecin/{id}` | ADMIN, SUPERVISEUR, DELEGUE | By medecin | ✅ OK |
| GET | `/api/distributions/by-delegue/{id}` | ADMIN, SUPERVISEUR, DELEGUE | By delegue | ✅ OK |
| GET | `/api/distributions/by-pharmacien/{id}` | ADMIN, SUPERVISEUR, DELEGUE | By pharmacien | ✅ OK |
| DELETE | `/api/distributions/{id}` | ADMIN, SUPERVISEUR | Soft delete + re-increments stock | ✅ OK |

### StockMovementController — `[Route("api/stock-movements")]`

| Method | Route | Roles | Description | Status |
|--------|-------|-------|-------------|--------|
| POST | `/api/stock-movements/decrement` | ADMIN, SUPERVISEUR | Decrement qty + record movement (query params) | ✅ OK |
| POST | `/api/stock-movements/increment` | ADMIN, SUPERVISEUR | Increment qty + record movement (query params) | ✅ OK |
| POST | `/api/stock-movements/transfer` | ADMIN, SUPERVISEUR | Transfer between two stocks (DB transaction) | ✅ OK |
| GET | `/api/stock-movements/{id}` | ADMIN, SUPERVISEUR | Movement history by stock ID | ✅ OK |
| GET | `/api/stock-movements/by-delegue/{id}` | ADMIN, SUPERVISEUR | All movements for all stocks of a delegue | ✅ OK |

### StockPromotionnelController — `[Route("api/stocks-promotionnels")]`

| Method | Route | Roles | Description | Status |
|--------|-------|-------|-------------|--------|
| POST | `/api/stocks-promotionnels/gratuite` | ADMIN, SUPERVISEUR | Create/update Stock_Gratuite (TPH) | ✅ OK |
| GET | `/api/stocks-promotionnels/gratuite/{id}` | ADMIN, SUPERVISEUR, DELEGUE | Get gratuite stock by ID | ✅ OK |
| POST | `/api/stocks-promotionnels/echantillon` | ADMIN, SUPERVISEUR | Create/update Stock_Echantillon (TPH) | ✅ OK |
| GET | `/api/stocks-promotionnels/echantillon/{id}` | ADMIN, SUPERVISEUR, DELEGUE | Get echantillon stock by ID | ✅ OK |

---

## 2. Ocelot Routes

All 7 `/inventory/` routes in `ocelot.json`. All downstream host: `cynapharminventories.runasp.net:80`. All require Bearer auth.

| Upstream | Downstream | Methods | Auth | Status |
|----------|------------|---------|------|--------|
| `/inventory/stocks-delegue/{everything}` | `/api/stocks-delegue/{everything}` | ALL | Bearer | ✅ Correct |
| `/inventory/distributions/{everything}` | `/api/distributions/{everything}` | ALL | Bearer | ✅ Correct |
| `/inventory/stock-movements/{everything}` | `/api/stock-movements/{everything}` | ALL | Bearer | ✅ Correct |
| `/inventory/stocks-promotionnels/{everything}` | `/api/stocks-promotionnels/{everything}` | ALL | Bearer | ✅ Correct |
| `/inventory/stock/{everything}` | `/api/stock/{everything}` | ALL | Bearer | ❌ Dead route — no controller at `/api/stock` |
| `/inventory/inventory-business/{everything}` | `/api/inventory-business/{everything}` | ALL | Bearer | ⚠️ No Angular service uses this prefix |
| `/inventory/warehouses/{everything}` | `/api/warehouses/{everything}` | ALL | Bearer | ⚠️ No Angular service uses this prefix |

---

## 3. Angular Components Status

### `stock-list` — `/inventory/stocks`

- **Data displayed**: Table of all stocks with delegue name, product name, lot number, expiration date, qty available, qty reserved. Paginated.
- **API called**: `GET /inventory/stocks-delegue?pageNumber=&pageSize=` → `StockService.getAll()`
- **Related data**: Delegue name via `UserService.getUserById()`, product name via `ProductService.getProductById()`, expiration date via `LotService.getLotByNumero()`
- ✅ Table renders, delete modal works, action buttons (view/edit) work
- ❌ **`this.total = data.length`** — total is set to page size, not total record count. Paginator gets wrong total.
- ⚠️ If backend returns no `total` in response, proper pagination is impossible without a count endpoint.

### `stock-detail` — `/inventory/stocks/:id`

- **Data displayed**: All fields of one stock + resolved delegue/product/lot names
- **API called**: `GET /inventory/stocks-delegue/{id}`
- ✅ Display works correctly
- ❌ **Edit button broken**: template uses `[routerLink]="['/inventory/stocks/edit', stock.id_stock]"` which generates `/inventory/stocks/edit/5`. Router expects `/inventory/stocks/5/edit`.

### `stock-form` — `/inventory/stocks/new` and `/inventory/stocks/:id/edit`

- **Data displayed**: Reactive form with cascading delegue → product → lot dropdowns; lot selection auto-fills expiration date
- **API called**: `POST /inventory/stocks-delegue` for create/update
- ✅ Cascading selects work, lot auto-fills expiration
- ❌ **Backend bug surfaces here**: DateExpiration sent in DTO but service ignores it on create (see issue #1)
- ⚠️ Lots filtered to non-expired only (`l.isExpired` = false), but in edit mode the existing lot is preserved even if expired

### `distribution-list` — `/inventory/distributions`

- **Data displayed**: Tabbed view — filter by delegue / medecin / pharmacien / all. Shows lot, qty, date, delegue, recipient.
- **API called**: `GET /inventory/distributions/by-delegue/{id}`, `by-medecin`, `by-pharmacien`, or paginated `GET /` for "all" tab
- ✅ Tabs switch correctly, user dropdown filters work, "all" tab loads with pagination
- ❌ **Mixed template syntax**: `*ngIf`/`*ngFor` (legacy) mixed with `@if`/`@for` (new) in the same file — inconsistent, potential issues
- ❌ **No delete button** — users cannot delete a distribution from the list or detail
- ⚠️ User names resolved from pre-loaded role lists but medecins/pharmaciens are loaded by role "MEDECIN"/"PHARMACIEN" — if those roles don't exist in AuthAPI the lists are empty

### `distribution-form` — `/inventory/distributions/new`

- **Data displayed**: Create form: delegue → stock (cascading), medecin/pharmacien (at least one required), qty, lot (auto-filled from stock)
- **API called**: `POST /inventory/distributions`
- ✅ Cascading delegue → stock works, lot auto-fills, at-least-one-recipient validator works
- ✅ Matches backend validation (FIX 4 in service)
- ⚠️ No update/edit form — distributions can only be created, not edited via Angular

### `distribution-detail` — `/inventory/distributions/:id`

- **Data displayed**: Single distribution detail
- **API called**: `GET /inventory/distributions/{id}`
- ✅ Basic detail display works
- ❌ No delete button on detail page either

### `movement-list` — `/inventory/movements`

- **Data displayed**: Stock movements filtered by stock ID, date range, and movement type
- **API called**: `GET /inventory/stock-movements/{idStock}`
- ✅ Client-side filters (date range, type) work
- ❌ **No initial load without stock ID** — page is blank until user enters a stock ID. No browse-all capability.
- ⚠️ `getMovementsByDelegue()` is defined in the service but never used in the component

### `promo-stock-detail` — `/inventory/promo-stocks`

- **Data displayed**: Lookup by stock ID → shows both gratuite and echantillon data with editable forms
- **API called**: `GET /inventory/stocks-promotionnels/gratuite/{id}` and `echantillon/{id}`
- ✅ Dual-lookup works, forms update on save
- ⚠️ History section reads `response.historique` but neither `Stock_Gratuite` nor `Stock_Echantillon` model includes a history array — will always show empty history

---

## 4. Stock Délégué — Business Logic

### How stock is created
1. Admin/Superviseur fills `stock-form`: selects delegue → product → lot → enters qty
2. Angular sends `POST /inventory/stocks-delegue` with `StockDelegueDto` (`id_stock = 0`)
3. `StockDelegueService.CreateUpdateStockAsync()` detects `dto.Id_stock == 0` → creates new `Stock_Delegue`
4. Sets: `Id_User_Delegue`, `Id_Produit`, `NumeroLot`, `QteDisponible`, `QteReservee = 0`, `DateCreation = UtcNow`
5. **BUG: `DateExpiration` is NOT set** — defaults to `DateTime.MinValue` (0001-01-01)
6. Soft-delete flag `IsDeleted = false`

### How stock is updated
1. Admin/Superviseur visits `/inventory/stocks/:id/edit` (route ok; link in detail page broken — see issue #2)
2. Angular sends same `POST` endpoint with `id_stock > 0`
3. Service finds existing stock, updates only `QteDisponible` and `NumeroLot`
4. `DateExpiration`, `Id_User_Delegue`, `Id_Produit` **cannot be changed** after creation

### How stock is decremented after distribution
When a distribution is **created** (new):
- `DistributionService.CreateOrUpdateEchantillonAsync()` loads the stock
- Validates lot expiration, stock existence, qty availability
- `stock.QteDisponible -= echantillon.Qte` — direct decrement, no movement recorded
- **Note**: No `StockMovement` record is created by distribution — movements are only created by explicit `/stock-movements/decrement` calls

When a distribution is **deleted**:
- `DeleteEchantillonAsync()` re-increments: `stock.QteDisponible += distribution.Qte` — correctly reverses the decrement

When a distribution is **updated** (existing):
- `_mapper.Map(echantillon, distribution)` — just overwrites fields
- **BUG**: If qty changes, the stock delta is **not calculated or applied**

### How movements are tracked
- Movements are a separate, explicit API: `POST /stock-movements/increment|decrement|transfer`
- Each call modifies `QteDisponible` AND inserts a `StockMovement` row with type, qty, timestamp
- Transfer uses a DB transaction: two movements created (Transfer-Out + Transfer-In), both stocks updated atomically
- **Distribution operations do NOT create StockMovement records** — they modify stock directly without a movement trace

### How promo stock works
- TPH (Table Per Hierarchy) with discriminator column `TypeStock` ("Standard" / "Echantillon" / "Gratuite")
- All subtypes share the `Stocks` table
- `Stock_Gratuite` adds: `QteGratuite`, `TypePromotion`
- `Stock_Echantillon` adds: `QteEchantillon`
- Managed via separate `StockPromotionnelController` endpoints
- `QteReservee` on promo stock is tracked but no mechanism automatically updates it

---

## 5. Distribution — Business Logic

### How a distribution is created
1. User selects delegue in `distribution-form`
2. Form loads all stocks for that delegue (`GET /inventory/stocks-delegue/by-delegue/{id}`)
3. User selects a stock → lot number auto-fills
4. User selects medecin OR pharmacien (at least one required — validated in both form and backend)
5. User enters qty
6. `POST /inventory/distributions` → `DistributionService.CreateOrUpdateEchantillonAsync()`

### What happens to stock after distribution
- **YES: stock IS decremented** — `stock.QteDisponible -= echantillon.Qte`
- Validations before decrement:
  - Stock must exist and not be soft-deleted
  - Lot must not be expired (`DateExpiration.Date >= UtcNow.Date`)
  - `QteDisponible >= Qte`
  - At least one of Id_Medecin or Id_Pharmacien must be set

### Is a movement created? NO
Distribution does **not** create a `StockMovement` record. The stock qty changes are invisible in the movement history. This is a gap in traceability.

### What validations exist

| Validation | Where | Notes |
|-----------|-------|-------|
| `Id_Medecin` or `Id_Pharmacien` required | Service (FIX 4) + Angular form validator | Both layers |
| `QteDisponible >= Qte` | Service (FIX 2) | Backend only |
| Lot not expired | Service (FIX 3) | Backend only |
| Stock exists and not deleted | Service (FIX 1 guard) | Backend only |
| `Qte > 0` | Angular form (`Validators.min(1)`) | Frontend only; backend doesn't validate |
| ModelState valid | Controller | Checks DTO annotation validity |

---

## 6. Business Logic Issues Found

| # | Component | Issue | Impact | Priority |
|---|-----------|-------|--------|----------|
| 1 | `StockDelegueService.cs` L38-47 | `DateExpiration` never assigned on create — stays `0001-01-01` | Every new stock has wrong expiration date | **CRITICAL** |
| 2 | `stock-detail.component.html` L53 | Edit link: `/inventory/stocks/edit/:id` instead of `/inventory/stocks/:id/edit` | Edit button on detail page goes to 404 | **HIGH** |
| 3 | `stock-list.component.ts` L60 | `this.total = data.length` — sets total to page size, not record count | Paginator always shows ≤20 total, "next" never appears properly | **HIGH** |
| 4 | `DistributionService.cs` L55 | Update path (`mapper.Map`) doesn't adjust `QteDisponible` when `Qte` changes | Increasing qty gives items without decrementing stock; decreasing qty loses stock | **HIGH** |
| 5 | `StockDelegueService.cs` L98 | `GetStockByLotAsync` returns `FirstOrDefault` — lot numbers not globally unique | Multiple delegues sharing a lot return only one result | **MEDIUM** |
| 6 | `DistributionService.cs` (all) | Distribution create/delete never creates a `StockMovement` record | Movement history is incomplete — distributions are invisible in audit trail | **MEDIUM** |
| 7 | `distribution-list.component.html` | Mixes `*ngIf`/`*ngFor` (legacy) with `@if`/`@for` (new control flow) in same template | Style inconsistency; possible Angular 17 compilation warnings | **LOW** |
| 8 | All controllers | `StatusCode(515, _response)` — 515 is a non-standard HTTP code | API clients treating 5xx as server error will receive unknown status | **LOW** |
| 9 | `promo-stock-detail.component.ts` L112 | History lookup reads `response.historique` but model has no such field | History section always shows empty | **LOW** |
| 10 | `ocelot.json` | Route `/inventory/stock/{everything}` → `/api/stock` — no controller exists | Any call to this route 404s silently through gateway | **LOW** |

---

## 7. Missing Features

| Feature | Where | Impact |
|---------|-------|--------|
| Delete button on distribution list/detail | `distribution-list.html`, `distribution-detail.html` | Users cannot delete distributions from UI |
| Stock total count for proper pagination | `StocksDelegueController` + `stock-list.component.ts` | Paginator never shows true total; "next page" unreliable |
| StockMovement record on distribution create/delete | `DistributionService.cs` | No audit trail for stock changes caused by distributions |
| Browse all movements without entering a stock ID | `movement-list.component.ts` | Page is blank on load — unusable as an overview screen |
| Edit distribution form | Routing + new component | Distributions can only be created, never edited |
| `QteReservee` management | Backend services | Field exists in model but nothing increments/decrements it |

---

## 8. Fix Plan

### Fix #1 — DateExpiration never saved on stock create
**File**: `CynapCRM.Services.InventoryAPI/Service/StockDelegueService.cs`  
**Lines**: 38–47

```csharp
// BEFORE — DateExpiration missing
stock = new Stock_Delegue {
    Id_User_Delegue = dto.Id_User_Delegue,
    Id_Produit      = dto.Id_Produit,
    NumeroLot       = dto.NumeroLot,
    QteDisponible   = dto.QteDisponible,
    QteReservee     = 0,
    DateCreation    = DateTime.UtcNow,
    IsDeleted       = false
};

// AFTER — add DateExpiration
stock = new Stock_Delegue {
    Id_User_Delegue = dto.Id_User_Delegue,
    Id_Produit      = dto.Id_Produit,
    NumeroLot       = dto.NumeroLot,
    DateExpiration  = dto.DateExpiration,   // ← ADD THIS LINE
    QteDisponible   = dto.QteDisponible,
    QteReservee     = 0,
    DateCreation    = DateTime.UtcNow,
    IsDeleted       = false
};
```

---

### Fix #2 — Edit link broken in stock-detail
**File**: `Cynapharm/src/app/features/inventory/stocks/stock-detail/stock-detail.component.html`  
**Line**: 53

```html
<!-- BEFORE -->
<a [routerLink]="['/inventory/stocks/edit', stock.id_stock]" class="btn btn-primary">Modifier</a>

<!-- AFTER -->
<a [routerLink]="['/inventory/stocks', stock.id_stock, 'edit']" class="btn btn-primary">Modifier</a>
```

---

### Fix #3 — Pagination total count
**File**: `Cynapharm/src/app/features/inventory/stocks/stock-list/stock-list.component.ts`  
**Line**: 60

The backend returns a flat array with no total count. Two options:

**Option A (backend change — not allowed):** Add a `totalCount` to the response wrapper.

**Option B (frontend workaround — no backend change):** Mark total as unknown and hide paginator when on last page:

```typescript
// BEFORE
this.stocks = data;
this.total  = data.length;

// AFTER — infer total: if we got a full page, there may be more
this.stocks = data;
this.total  = data.length < this.pageSize
  ? (this.page - 1) * this.pageSize + data.length  // last page
  : (this.page) * this.pageSize + 1;               // at least one more page exists
```

---

### Fix #4 — Distribution update doesn't adjust stock qty
**File**: `CynapCRM.Services.InventoryAPI/Service/DistributionService.cs`  
**Lines**: 54–56 (the else branch)

```csharp
// BEFORE — no stock adjustment
else
{
    _mapper.Map(echantillon, distribution);
}

// AFTER — calculate delta and adjust stock
else
{
    var stock = await _db.StocksDelegues
        .FirstOrDefaultAsync(s => s.Id_stock == distribution.Id_Stock && !s.IsDeleted);

    if (stock != null)
    {
        int delta = echantillon.Qte - distribution.Qte;  // positive = need more stock
        if (delta > 0 && stock.QteDisponible < delta)
            return false;  // not enough stock for the increase
        stock.QteDisponible -= delta;
    }

    _mapper.Map(echantillon, distribution);
}
```

---

### Fix #5 — Add StockMovement record on distribution create/delete
**File**: `CynapCRM.Services.InventoryAPI/Service/DistributionService.cs`

```csharp
// In CreateOrUpdateEchantillonAsync, after "FIX 1: decrement available stock":
stock.QteDisponible -= echantillon.Qte;

// ADD: record movement
_db.StockMovements.Add(new StockMovement {
    Id_Stock      = stock.Id_stock,
    Quantite      = -echantillon.Qte,
    TypeMovement  = "Distribution",
    DateMovement  = DateTime.UtcNow,
    Description   = $"Distribution échantillon #{echantillon.Id_Distribution}"
});

// In DeleteEchantillonAsync, after "FIX 5: reincrement stock":
stock.QteDisponible += distribution.Qte;

// ADD: record movement
_db.StockMovements.Add(new StockMovement {
    Id_Stock      = stock.Id_stock,
    Quantite      = distribution.Qte,
    TypeMovement  = "Distribution-Annulée",
    DateMovement  = DateTime.UtcNow,
    Description   = $"Annulation distribution #{idDistribution}"
});
```

---

### Fix #6 — Add delete to distribution list/detail
**File**: `Cynapharm/src/app/features/inventory/distributions/distribution-list/distribution-list.component.ts`

Add to component:
```typescript
deleteId: number | null = null;

onDelete(id: number): void {
  if (!confirm('Supprimer cette distribution ?')) return;
  this.svc.delete(id).pipe(takeUntil(this.destroy$)).subscribe({
    next: () => { this.toast.showSuccess('Distribution supprimée.'); this.load(); /* or loadAll(true) */ },
    error: () => this.toast.showError('Erreur lors de la suppression.')
  });
}
```

Add delete column to both table sections in the HTML.

---

### Fix #7 — Mixed template syntax in distribution-list
**File**: `Cynapharm/src/app/features/inventory/distributions/distribution-list/distribution-list.component.html`

Replace all `*ngIf="..."` and `*ngFor="let x of y"` with Angular 17+ `@if` / `@for` blocks to match the rest of the app.

---

### Fix #8 — StatusCode 515 → 500
**Files**: All 4 controllers  
**Change**: Replace `StatusCode(515, _response)` with `StatusCode(500, _response)` throughout.

---

### Fix #9 — Movement list blank on load
**File**: `Cynapharm/src/app/features/inventory/movements/movement-list/movement-list.component.ts`

Option: If no `idStock` query param, call `getMovementsByDelegue` for the current user's delegue ID (read from JWT), or add a "search all" button that calls a new paginated movements endpoint.

---

## 9. Complete Code of Every File Read

### `Controllers/StocksDelegueController.cs`

```csharp
using CynapCRM.Services.InventoryAPI.Models;
using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.InventoryAPI.Controllers
{
    [Route("api/stocks-delegue")]
    [ApiController]
    [Authorize]
    public class StocksDelegueController : ControllerBase
    {
        private readonly IStockDelegueService _stockDelegueService;
        protected ResponseDto _response;

        public StocksDelegueController(IStockDelegueService stockDelegueService)
        {
            _stockDelegueService = stockDelegueService;
            _response = new ResponseDto();
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetAllStocks(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (pageNumber <= 0 || pageSize <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Paramètres de pagination invalides.";
                    return BadRequest(_response);
                }
                _response.Result = await _stockDelegueService
                    .GetAllStocksAsync(pageNumber, pageSize);
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpGet("{idStock:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetStockById(int idStock)
        {
            try
            {
                if (idStock <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id stock invalide.";
                    return BadRequest(_response);
                }
                var stock = await _stockDelegueService.GetStockByIdAsync(idStock);
                if (stock == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Stock introuvable.";
                    return NotFound(_response);
                }
                _response.Result = stock;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpGet("by-delegue/{idDelegue:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetStocksByDelegue(int idDelegue) { ... }

        [HttpGet("by-produit/{idProduit:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetStocksByProduit(int idProduit) { ... }

        [HttpGet("by-lot/{numeroLot}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetStockByLot(string numeroLot) { ... }

        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> CreateOrUpdateStock([FromBody] StockDelegueDto stockDto) { ... }

        [HttpDelete("{idStock:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteStock(int idStock, [FromQuery] StockType type) { ... }
    }
}
```

---

### `Controllers/DistributionController.cs`

```csharp
[Route("api/distributions")]
[ApiController]
[Authorize]
public class DistributionController : ControllerBase
{
    // POST /  — create or update, maps DTO to entity manually, validates ModelState
    // GET /   — paginated list (ADMIN, SUPERVISEUR)
    // GET /{id} — get by id (ADMIN, SUPERVISEUR, DELEGUE)
    // GET /by-medecin/{id}
    // GET /by-delegue/{id}    ← previously called wrong method (FIX comment)
    // GET /by-pharmacien/{id}
    // DELETE /{id}            — soft delete (ADMIN, SUPERVISEUR)
}
```

---

### `Service/StockDelegueService.cs`

```csharp
public class StockDelegueService : IStockDelegueService
{
    public async Task<IEnumerable<StockDelegueDto>> GetAllStocksAsync(int pageNumber, int pageSize)
    {
        var stocks = await _db.StocksDelegues
            .AsNoTracking()
            .Where(s => !s.IsDeleted)
            .OrderByDescending(s => s.DateCreation)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return _mapper.Map<IEnumerable<StockDelegueDto>>(stocks);
    }

    public async Task<StockDelegueDto?> CreateUpdateStockAsync(StockDelegueDto dto)
    {
        Stock_Delegue stock;
        if (dto.Id_stock == 0)
        {
            stock = new Stock_Delegue {
                Id_User_Delegue = dto.Id_User_Delegue,
                Id_Produit      = dto.Id_Produit,
                NumeroLot       = dto.NumeroLot,
                // BUG: DateExpiration = dto.DateExpiration  ← MISSING
                QteDisponible   = dto.QteDisponible,
                QteReservee     = 0,
                DateCreation    = DateTime.UtcNow,
                IsDeleted       = false
            };
            _db.StocksDelegues.Add(stock);
        }
        else
        {
            stock = await _db.StocksDelegues
                .FirstOrDefaultAsync(s => s.Id_stock == dto.Id_stock && !s.IsDeleted);
            if (stock == null) return null;
            stock.QteDisponible = dto.QteDisponible;
            stock.NumeroLot     = dto.NumeroLot;
        }
        await _db.SaveChangesAsync();
        return _mapper.Map<StockDelegueDto>(stock);
    }

    public async Task<bool> DeleteStockAsync(int idStock, StockType type)
    {
        if (type != StockType.Delegue) return false;
        var stock = await _db.StocksDelegues.FirstOrDefaultAsync(s => s.Id_stock == idStock);
        if (stock == null) return false;
        if (stock.QteDisponible > 0) return false;  // Business rule: cannot delete if qty remains
        stock.IsDeleted = true;
        await _db.SaveChangesAsync();
        return true;
    }
}
```

---

### `Service/DistributionService.cs`

```csharp
public class DistributionService : IDistributionService
{
    public async Task<bool> CreateOrUpdateEchantillonAsync(Echantillon echantillon)
    {
        var distribution = await _db.Echantillons
            .FirstOrDefaultAsync(e => e.Id_Distribution == echantillon.Id_Distribution);

        if (distribution == null)
        {
            // FIX 4: at least one recipient required
            if (echantillon.Id_Medecin == null && echantillon.Id_Pharmacien == null) return false;

            var stock = await _db.StocksDelegues
                .FirstOrDefaultAsync(s => s.Id_stock == echantillon.Id_Stock && !s.IsDeleted);
            if (stock == null) return false;

            // FIX 3: lot expiration check
            if (stock.DateExpiration != default(DateTime) &&
                stock.DateExpiration.Date < DateTime.UtcNow.Date) return false;

            // FIX 2: quantity check
            if (stock.QteDisponible < echantillon.Qte) return false;

            echantillon.DateDistribution = DateTime.UtcNow;
            echantillon.IsDeleted = false;
            _db.Echantillons.Add(echantillon);

            // FIX 1: decrement available stock
            stock.QteDisponible -= echantillon.Qte;
        }
        else
        {
            // BUG: no qty delta applied to stock
            _mapper.Map(echantillon, distribution);
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteEchantillonAsync(int idDistribution)
    {
        var distribution = await _db.Echantillons
            .FirstOrDefaultAsync(e => e.Id_Distribution == idDistribution);
        if (distribution == null || distribution.IsDeleted) return false;

        // FIX 5: reincrement stock when distribution is deleted
        var stock = await _db.StocksDelegues
            .FirstOrDefaultAsync(s => s.Id_stock == distribution.Id_Stock && !s.IsDeleted);
        if (stock != null)
            stock.QteDisponible += distribution.Qte;

        distribution.IsDeleted = true;
        await _db.SaveChangesAsync();
        return true;
    }
}
```

---

### `Service/StockMovementService.cs`

```csharp
public class StockMovementService : IStockMovementService
{
    // DecrementStockAsync: validates qte>0, stock exists, sufficient qty → subtracts, records "Decrement" movement
    // IncrementStockAsync: validates qte>0, stock exists → adds, records "Increment" movement
    // TransferStockAsync: DB transaction → source-=qte, dest+=qte, records "Transfer-Out" and "Transfer-In"
    // GetStockMovementsAsync: returns all movements for a stock ID ordered by date DESC
    // GetMovementHistoryByDelegueAsync: joins stocks by delegue → returns all movements for all those stocks
}
```

---

### `Data/AppDbContext.cs`

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Stock_Delegue>  StocksDelegues { get; set; }
    public DbSet<Echantillon>    Echantillons   { get; set; }
    public DbSet<StockMovement>  StockMovements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TPH: Stock_Delegue / Stock_Echantillon / Stock_Gratuite share "Stocks" table
        // Discriminator column: "TypeStock" with values "Standard", "Echantillon", "Gratuite"

        // Keys: Id_stock, Id_Distribution, Id_Movement
        // Indexes: NumeroLot, Id_User_Delegue, Id_Medecin, Id_Pharmacien, Id_Stock
        // Cascade delete: StockMovement → Stock_Delegue
        // Default values: QteReservee=0, DateDistribution=GETUTCDATE(), DateMovement=GETUTCDATE()
        // Tables: Stocks, Distributions_Echantillons, Stock_Movements
    }
}
```

---

### `Models/Stock_Delegue.cs`

```csharp
public class Stock_Delegue
{
    public int      Id_stock        { get; set; }
    public int      Id_User_Delegue { get; set; }
    public int      Id_Produit      { get; set; }
    public string   NumeroLot       { get; set; } = string.Empty;
    public DateTime DateCreation    { get; set; }
    public DateTime DateExpiration  { get; set; }
    public int      QteDisponible   { get; set; }
    public int      QteReservee     { get; set; } = 0;
    public bool     IsDeleted       { get; internal set; } = false;
}
```

---

### `Models/Dto/StockDelegueDto.cs`

```csharp
public class StockDelegueDto
{
    public int      Id_stock        { get; set; }
    public int      Id_User_Delegue { get; set; }
    public int      Id_Produit      { get; set; }
    public string   NumeroLot       { get; set; }
    public DateTime DateExpiration  { get; set; }
    public int      QteDisponible   { get; set; }
    public int      QteReservee     { get; set; }
}
```

---

### `Models/Echantillon.cs`

```csharp
public class Echantillon
{
    public int      Id_Distribution  { get; set; }
    public int      Id_Delegue       { get; set; }
    public int?     Id_Medecin       { get; set; }
    public int?     Id_Pharmacien    { get; set; }
    public int      Id_Stock         { get; set; }
    public int      Qte              { get; set; }
    public string   NumeroLot        { get; set; } = string.Empty;
    public DateTime DateDistribution { get; set; } = DateTime.UtcNow;
    public bool     IsDeleted        { get; internal set; } = false;
}
```

---

### `Models/Dto/EchantillonDto.cs`

```csharp
public class EchantillonDto
{
    public int      Id_Distribution  { get; set; }
    public int      Id_Delegue       { get; set; }
    public int?     Id_Medecin       { get; set; }
    public int?     Id_Pharmacien    { get; set; }
    public int      Id_Stock         { get; set; }
    public int      Qte              { get; set; }
    public string   NumeroLot        { get; set; }
    public DateTime DateDistribution { get; set; }
}
```

---

### Angular `stock.service.ts`

```typescript
export interface StockDelegueDto {
  id_stock?:       number;
  id_User_Delegue: number;
  id_Produit:      number;
  numeroLot:       string;
  dateExpiration:  string;
  qteDisponible:   number;
  qteReservee:     number;
}

@Injectable({ providedIn: 'root' })
export class StockService {
  private readonly base = '/inventory/stocks-delegue';
  // getAll(page, size) → GET /inventory/stocks-delegue?pageNumber=&pageSize=
  // getById(id)        → GET /inventory/stocks-delegue/{id}
  // getByDelegue(id)   → GET /inventory/stocks-delegue/by-delegue/{id}
  // getByProduit(id)   → GET /inventory/stocks-delegue/by-produit/{id}
  // getByLot(num)      → GET /inventory/stocks-delegue/by-lot/{num}
  // createOrUpdate(dto)→ POST /inventory/stocks-delegue
  // delete(id, type)   → DELETE /inventory/stocks-delegue/{id}?type={type}
}
```

---

### Angular `distribution.service.ts`

```typescript
export interface EchantillonDto {
  id_Distribution?: number;
  id_Delegue:       number;
  id_Medecin?:      number | null;
  id_Pharmacien?:   number | null;
  id_Stock:         number;
  qte:              number;
  numeroLot:        string;
  dateDistribution?: string;
}

@Injectable({ providedIn: 'root' })
export class DistributionService {
  private readonly base = '/inventory/distributions';
  // getAll(page, size)     → GET /inventory/distributions?pageNumber=&pageSize=
  // getById(id)            → GET /inventory/distributions/{id}
  // getByMedecin(id)       → GET /inventory/distributions/by-medecin/{id}
  // getByDelegue(id)       → GET /inventory/distributions/by-delegue/{id}
  // getByPharmacien(id)    → GET /inventory/distributions/by-pharmacien/{id}
  // createOrUpdate(dto)    → POST /inventory/distributions
  // delete(id)             → DELETE /inventory/distributions/{id}
}
```

---

### Angular `stock-movement.service.ts`

```typescript
export interface StockMovementDto {
  id_Movement?: number;
  id_Stock:     number;
  quantite:     number;
  typeMovement: string;
  dateMovement?: string;
  description?: string;
}

@Injectable({ providedIn: 'root' })
export class StockMovementService {
  // getMovements(idStock)               → GET /inventory/stock-movements/{idStock}
  // getMovementsByDelegue(idDelegue)    → GET /inventory/stock-movements/by-delegue/{id}
  // decrement(idStock, qte)             → POST /inventory/stock-movements/decrement?idStock=&qte=
  // increment(idStock, qte)             → POST /inventory/stock-movements/increment?idStock=&qte=
  // transfer(idSource, idDest, qte)     → POST /inventory/stock-movements/transfer?idStockSource=&idStockDestination=&qte=
}
```

---

## Summary

| Priority | Count | Description |
|----------|-------|-------------|
| CRITICAL | 1 | `DateExpiration` never saved on stock create |
| HIGH | 2 | Edit link broken; pagination total wrong |
| HIGH | 1 | Distribution update doesn't adjust stock qty |
| MEDIUM | 2 | by-lot returns single result; no movement trace on distribution |
| LOW | 4 | Mixed template syntax; status 515; empty history; dead Ocelot route |
| Missing | 6 | Delete dist UI; proper pagination; movement on dist; browse-all movements; edit dist form; QteReservee management |

**Immediate actions (no backend change needed):**
1. Fix edit link in `stock-detail.component.html` (1 line)
2. Fix pagination total in `stock-list.component.ts` (1 line)
3. Add delete button to distribution list HTML + method in TS

**Backend changes needed:**
1. Add `DateExpiration = dto.DateExpiration` in `StockDelegueService.CreateUpdateStockAsync`
2. Fix distribution update to apply qty delta to stock
3. Add `StockMovement` record in distribution create/delete
