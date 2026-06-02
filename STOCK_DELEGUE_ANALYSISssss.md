# Stock Délégué — Analyse complète

---

## 1. Modèle `Stock_Delegue`

**Fichier :** `CynapCRM.Services.InventoryAPI/Models/Stock_Delegue.cs`

```csharp
public class Stock_Delegue
{
    public int Id_stock { get; set; }
    public int Id_User_Delegue { get; set; }
    public int Id_Produit { get; set; }
    public string NumeroLot { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; }
    public DateTime DateExpiration { get; set; }
    public int QteDisponible { get; set; }
    public int QteReservee { get; set; } = 0;
    public bool IsDeleted { get; internal set; } = false;
}
```

### Propriétés
| Propriété | Type | Remarque |
|---|---|---|
| `Id_stock` | `int` | Clé primaire (configurée dans `OnModelCreating`) |
| `Id_User_Delegue` | `int` | FK vers AuthAPI user (aucune contrainte FK EF déclarée) |
| `Id_Produit` | `int` | FK vers ProductAPI product (aucune contrainte FK EF déclarée) |
| `NumeroLot` | `string` | Index DB, max 100 caractères, required |
| `DateCreation` | `DateTime` | Initialisé à `DateTime.UtcNow` à la création |
| `DateExpiration` | `DateTime` | Date expiration du lot |
| `QteDisponible` | `int` | Quantité en stock |
| `QteReservee` | `int` | Initialisé à 0 par défaut |
| `IsDeleted` | `bool` | Soft-delete flag (`internal set` — ne peut être modifié que depuis l'assembly) |

### Héritage TPH (Table Per Hierarchy)

**Fichier :** `CynapCRM.Services.InventoryAPI/Data/AppDbContext.cs`

```csharp
modelBuilder.Entity<Stock_Delegue>()
    .HasDiscriminator<string>("TypeStock")
    .HasValue<Stock_Delegue>("Standard")
    .HasValue<Stock_Echantillon>("Echantillon")
    .HasValue<Stock_Gratuite>("Gratuite");

modelBuilder.Entity<Stock_Delegue>().ToTable("Stocks");
```

**Point critique :** `Stock_Delegue` est la classe de base d'un arbre TPH. La table physique `[Stocks]` contient trois types de lignes :
- `TypeStock = "Standard"` → `Stock_Delegue` (délégués)
- `TypeStock = "Echantillon"` → `Stock_Echantillon` (promotionnel)
- `TypeStock = "Gratuite"` → `Stock_Gratuite` (gratuite)

`_db.StocksDelegues` interroge TOUTE la table sans filtre `TypeStock` automatique, car `Stock_Delegue` est la **classe de base** (pas une classe dérivée). Seules les classes dérivées bénéficient d'un filtre discriminant implicite dans EF Core.

---

## 2. Endpoints Backend

**Fichier :** `CynapCRM.Services.InventoryAPI/Controllers/StocksDelegueController.cs`

**Route base :** `api/stocks-delegue`  
**Auth classe :** `[Authorize]` (classe entière)

| Méthode | Route | Rôles autorisés | Description |
|---|---|---|---|
| `GET` | `/` | `ADMIN,SUPERVISEUR` | Liste paginée (pageNumber, pageSize) |
| `GET` | `/{idStock:int}` | `ADMIN,SUPERVISEUR,DELEGUE` | Stock par ID |
| `GET` | `/by-delegue/{idDelegue:int}` | `ADMIN,SUPERVISEUR,DELEGUE` | Stocks d'un délégué |
| `GET` | `/by-produit/{idProduit:int}` | `ADMIN,SUPERVISEUR` | Stocks d'un produit |
| `GET` | `/by-lot/{numeroLot}` | `ADMIN,SUPERVISEUR` | Stock par numéro de lot (premier résultat) |
| `POST` | `/` | `ADMIN,SUPERVISEUR` | Créer ou mettre à jour (Id_stock == 0 = création) |
| `DELETE` | `/{idStock:int}?type={StockType}` | `ADMIN` | Soft-delete avec vérification de type |

### Validations dans le contrôleur
- `pageNumber <= 0 || pageSize <= 0` → 400 BadRequest
- `idStock <= 0` / `idDelegue <= 0` / `idProduit <= 0` → 400 BadRequest
- `string.IsNullOrWhiteSpace(numeroLot)` → 400 BadRequest
- `!ModelState.IsValid` pour POST → 400 BadRequest
- Résultat `null` de service → 400 BadRequest ou 404 NotFound selon le contexte
- Exception non gérée → StatusCode(515, _response)

---

## 3. Routes Ocelot

**Fichier :** `CynapCRM.Gateway/ocelot.json`

```json
{
  "UpstreamPathTemplate": "/inventory/stocks-delegue/{everything}",
  "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
  "DownstreamPathTemplate": "/api/stocks-delegue/{everything}",
  "DownstreamScheme": "https",
  "DownstreamHostAndPorts": [
    { "Host": "cynapharminventories.runasp.net", "Port": 443 }
  ]
}
```

**État :** correct et complet. Tous les verbes nécessaires sont présents. Le wildcard `{everything}` couvre toutes les sous-routes (`/by-delegue/`, `/by-produit/`, `/by-lot/`, `/{id}`).

---

## 4. Service Layer

### Interface

**Fichier :** `CynapCRM.Services.InventoryAPI/Service/IService/IStockDelegueService.cs`

```csharp
public interface IStockDelegueService
{
    Task<IEnumerable<StockDelegueDto>> GetAllStocksAsync(int pageNumber, int pageSize);
    Task<StockDelegueDto?> CreateUpdateStockAsync(StockDelegueDto stockDto);
    Task<StockDelegueDto?> GetStockByIdAsync(int idStock);
    Task<IEnumerable<StockDelegueDto>> GetStocksByDelegueAsync(int idDelegue);
    Task<IEnumerable<StockDelegueDto>> GetStockByProduitAsync(int idProduit);
    Task<StockDelegueDto?> GetStockByLotAsync(string numeroLot);
    Task<bool> DeleteStockAsync(int idStock, StockType type);
}
```

### Implémentation — analyse méthode par méthode

**Fichier :** `CynapCRM.Services.InventoryAPI/Service/StockDelegueService.cs`

#### `GetAllStocksAsync(int pageNumber, int pageSize)`

```csharp
var stocks = await _db.StocksDelegues
    .AsNoTracking()
    .Where(s => !s.IsDeleted)
    .OrderByDescending(s => s.DateCreation)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
return _mapper.Map<IEnumerable<StockDelegueDto>>(stocks);
```

**BUG P1 :** Aucun filtre `TypeStock`. `_db.StocksDelegues` retourne les trois types (`Standard`, `Echantillon`, `Gratuite`). La liste inclut des stocks promotionnels qui n'appartiennent pas aux délégués.

**Fix :** Ajouter `.OfType<Stock_Delegue>()` — mais attention : `Stock_Delegue` EST la classe de base. Il faut filtrer sur le discriminateur directement :
```csharp
.Where(s => EF.Property<string>(s, "TypeStock") == "Standard" && !s.IsDeleted)
```

#### `CreateUpdateStockAsync(StockDelegueDto dto)`

```csharp
if (dto.Id_stock == 0)
{
    stock = new Stock_Delegue
    {
        Id_User_Delegue = dto.Id_User_Delegue,
        Id_Produit      = dto.Id_Produit,
        NumeroLot       = dto.NumeroLot,
        DateExpiration  = dto.DateExpiration,
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

    stock.QteDisponible = dto.QteDisponible;   // ← seulement ces deux champs
    stock.NumeroLot     = dto.NumeroLot;        // ← mis à jour
}
```

**BUG P1 :** Le path UPDATE ignore silencieusement `DateExpiration`, `Id_User_Delegue`, et `Id_Produit`. Un ADMIN qui modifie un stock via le formulaire Angular ne peut pas changer le délégué assigné ni la date d'expiration — les changements sont perdus.

**Fix :** Ajouter les champs manquants dans le bloc `else` :
```csharp
stock.QteDisponible    = dto.QteDisponible;
stock.NumeroLot        = dto.NumeroLot;
stock.DateExpiration   = dto.DateExpiration;
stock.Id_User_Delegue  = dto.Id_User_Delegue;
stock.Id_Produit       = dto.Id_Produit;
```

#### `GetStockByIdAsync(int idStock)`

```csharp
var stock = await _db.StocksDelegues
    .AsNoTracking()
    .FirstOrDefaultAsync(s => s.Id_stock == idStock && !s.IsDeleted);
```

**État :** correct. Filtre `!IsDeleted` présent.

#### `GetStocksByDelegueAsync(int idDelegue)`

```csharp
var stocks = await _db.StocksDelegues
    .AsNoTracking()
    .Where(s => s.Id_User_Delegue == idDelegue && !s.IsDeleted)
    .OrderByDescending(s => s.DateCreation)
    .ToListAsync();
```

**Note :** Pas de pagination. Pour un délégué avec beaucoup de stocks, cela peut retourner des centaines de lignes. Acceptable pour l'usage actuel (chargement du profil délégué), mais à surveiller.

**Note secondaire :** Même risque TPH que `GetAllStocksAsync` — peut retourner des stocks Echantillon/Gratuite associés au même `Id_User_Delegue` si ces types partagent la même colonne.

#### `GetStockByProduitAsync(int idProduit)`

```csharp
var stocks = await _db.StocksDelegues
    .AsNoTracking()
    .Where(s => s.Id_Produit == idProduit && !s.IsDeleted)
    .ToListAsync();
```

**Note :** Pas d'`OrderBy`, pas de pagination.

#### `GetStockByLotAsync(string numeroLot)`

```csharp
var stock = await _db.StocksDelegues
    .AsNoTracking()
    .FirstOrDefaultAsync(s => s.NumeroLot == numeroLot && !s.IsDeleted);
```

**Note :** Retourne seulement le **premier** stock correspondant. Si plusieurs types TPH partagent le même `NumeroLot`, le résultat est non-déterministe.

#### `DeleteStockAsync(int idStock, StockType type)`

```csharp
if (type != StockType.Delegue) return false;

var stock = await _db.StocksDelegues
    .FirstOrDefaultAsync(s => s.Id_stock == idStock);   // ← pas de !IsDeleted

if (stock == null) return false;
if (stock.QteDisponible > 0) return false;

stock.IsDeleted = true;
await _db.SaveChangesAsync();
return true;
```

**BUG P2 :** La requête ne filtre pas `!s.IsDeleted`. Un stock déjà supprimé (soft-deleted) peut être retrouvé et la propriété `IsDeleted` sera remise à `true` inutilement (double soft-delete). Plus grave : la vérification `QteDisponible > 0` s'applique à un stock déjà supprimé, ce qui peut produire un retour `false` trompeur ("suppression impossible") alors que le stock est déjà supprimé.

**Fix :** Ajouter `&& !s.IsDeleted` dans la requête :
```csharp
.FirstOrDefaultAsync(s => s.Id_stock == idStock && !s.IsDeleted);
```

---

## 5. Angular — État actuel

### 5.1 Routes Angular (stocks)

**Fichier :** `Cynapharm/src/app/features/inventory/inventory-routing.module.ts`

```typescript
{ path: 'stocks',          loadComponent: () => import('./stocks/stock-list/...')   },
{ path: 'stocks/new',      loadComponent: () => import('./stocks/stock-form/...')   },
{ path: 'stocks/:id',      loadComponent: () => import('./stocks/stock-detail/...') },
{ path: 'stocks/:id/edit', loadComponent: () => import('./stocks/stock-form/...')   },
```

**État :** correct. L'ordre `stocks/new` avant `stocks/:id` est critique pour éviter que "new" soit interprété comme un ID. L'ordre est respecté.

---

### 5.2 `stock.service.ts`

**Fichier :** `Cynapharm/src/app/features/inventory/stocks/services/stock.service.ts`

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
  private u<T>(r: any): T { return r?.Result ?? r?.result ?? r; }

  getAll(pageNumber = 1, pageSize = 20): Observable<StockDelegueDto[]>
  getById(id: number): Observable<StockDelegueDto>
  getByDelegue(id: number): Observable<StockDelegueDto[]>
  getByProduit(id: number): Observable<StockDelegueDto[]>
  getByLot(numero: string): Observable<StockDelegueDto>
  createOrUpdate(dto: StockDelegueDto): Observable<StockDelegueDto>
  delete(id: number, type: StockType): Observable<void>
}
```

**Observations :**
- Le helper `u<T>()` gère la variabilité de casse (`Result` / `result`) du pattern `ResponseDto`.
- `dateExpiration` est `string` côté Angular — la conversion ISO vers `Date` est faite dans les templates via le pipe `date`.
- L'interface Angular est cohérente avec le DTO backend sauf `dateCreation` qui n'est pas dans le DTO backend (`StockDelegueDto.cs` ne l'expose pas).
- `delete()` passe `type` en query param : `/inventory/stocks-delegue/{id}?type=Delegue` — correctement mappé sur `[FromQuery] StockType type` dans le contrôleur.

---

### 5.3 `stock-list` Component

**Fichiers :** `stock-list.component.ts / .html / .css`

#### Fonctionnalités
- Chargement paginé : page=1, pageSize=20
- Résolution des noms : `UserService.getUserById()` par délégué unique, `ProductService.getProductById()` par produit unique, `LotService.getLotByNumero()` par lot unique
- Modale de suppression avec confirmation
- Colonnes : Délégué, Produit, N°Lot, Expiration, Disponible, Réservé, Actions
- Syntaxe Angular 17+ : `@if`, `@else if`, `@for (s of stocks; track s.id_stock)`

#### Calcul du total (paginator)

```typescript
this.total = data.length < this.pageSize
  ? (this.page - 1) * this.pageSize + data.length
  : this.page * this.pageSize + 1;
```

**BUG P3 :** Approximation. Quand la page est pleine (`data.length === pageSize`), le total est estimé à `page * pageSize + 1` pour indiquer qu'il y a une page suivante. Cela ne reflète pas le vrai total et peut afficher "21 items" quand il y en a 40. Le backend ne retourne pas de `totalCount` — résolution complète nécessite une modification backend.

#### Résolution des noms
```typescript
delegueIds.forEach(id => {
  this.userSvc.getUserById(id).subscribe({
    next: (res: any) => {
      const u = res?.Result ?? res?.result ?? res;
      this.delegueNames[id] = u?.fullName ?? u?.FullName ?? u?.name ?? u?.Name ?? u?.email ?? `#${id}`;
    },
    error: () => { this.delegueNames[id] = `#${id}`; }
  });
});
```

Cache local par ID (`delegueNames`, `productNames`, `lotDates`) — évite les doublons d'appel sur la même page.

#### Template — points notables
- `@if (error)` s'affiche même pendant le chargement si une erreur persistante est présente (non bloquant)
- `getDelegrueName()` — faute de frappe dans le nom de méthode (extra `u`) mais sans impact fonctionnel
- La colonne "Expiration" utilise `getLotDate(s.numeroLot, s.dateExpiration)` : priorité à la date résolue depuis LotService, fallback sur `dateExpiration` du DTO

---

### 5.4 `stock-detail` Component

**Fichiers :** `stock-detail.component.ts / .html / .css`

#### Fonctionnalités
- Chargement par `route.snapshot.paramMap.get('id')`
- Résolution séparée : délégué (UserService), produit (ProductService), lot/expiration (LotService)
- Affichage en grille : ID, Délégué, Produit, N°Lot, Date expiration, Qté disponible, Qté réservée
- Boutons : Modifier (routerLink edit), Retour
- Syntaxe Angular classique `*ngIf` (pas de control flow @if)

#### Résolution du nom délégué — champs testés
```typescript
this.delegeName = u?.fullName ?? u?.FullName ?? u?.name ?? u?.Name ??
                  u?.userName ?? u?.UserName ?? u?.email ?? u?.Email ?? `#${id}`;
```

Plus exhaustif que `stock-list` (inclut `userName`/`UserName`).

#### Template — points notables
- `*ngIf="!loading && !error && !stock"` — état "introuvable" séparé
- Fallback `lotDate || stock.dateExpiration` pour l'affichage de la date

---

### 5.5 `stock-form` Component

**Fichiers :** `stock-form.component.ts / .html / .css`

#### Fonctionnalités
- Modes : création (`stocks/new`) et édition (`stocks/:id/edit`)
- `FormGroup` réactif avec 5 champs : `id_User_Delegue`, `id_Produit`, `numeroLot`, `dateExpiration`, `qteDisponible`
- Cascading dropdowns :
  1. Délégué — chargé via `getUsersByRole('DELEGUE')`
  2. Produit — chargé via `getVisibleProducts()`
  3. N°Lot — activé seulement après sélection d'un produit (disabled sinon), chargé via `getLotsByProductId(productId)`, filtré `!isExpired`
  4. Date expiration — read-only display, auto-rempli depuis le lot sélectionné, stocké dans champ `hidden`
- En mode édition : lots du produit existant chargés avec inclusion du lot courant (même expiré) pour préserver la sélection

#### Logique de cascading
```typescript
// Changement produit → reset lot + date + reload lots
this.form.get('id_Produit')!.valueChanges.subscribe(id => {
  this.lots = []; this.lotDateDisplay = '';
  this.form.get('numeroLot')!.setValue('', { emitEvent: false });
  this.form.get('numeroLot')!.disable();
  if (id) this.loadLots(+id);
});

// Changement lot → auto-fill date
this.form.get('numeroLot')!.valueChanges.subscribe(num => {
  const lot = this.lots.find(l => l.numero === num);
  if (lot?.dateExpiration) { ... this.form.patchValue({ dateExpiration: iso }); }
});
```

#### Submit
```typescript
const v = this.form.getRawValue(); // getRawValue() inclut les contrôles disabled (numeroLot)
const dto: StockDelegueDto = {
  id_User_Delegue: +v.id_User_Delegue,
  id_Produit:      +v.id_Produit,
  numeroLot:       v.numeroLot,
  dateExpiration:  v.dateExpiration,
  qteDisponible:   +v.qteDisponible,
  qteReservee:     0,
  ...(this.isEdit && this.editId ? { id_stock: this.editId } : {})
};
```

**Note :** `getRawValue()` est correct — `form.value` aurait omis `numeroLot` (disabled).

**Note :** Le message d'erreur submit est en anglais : `'Error saving stock.'` — incohérent avec le reste de l'UI en français.

---

## 6. Business Logic Analysis

### Cycle de vie d'un stock délégué

```
Création (Id_stock = 0, POST)
    → QteReservee = 0, DateCreation = UtcNow, IsDeleted = false
    → TypeStock = "Standard" (implicite, défaut EF)
        ↓
Opérations de mouvement (StockMovementController)
    → Decrement : QteDisponible -= qte (vérifié dans StockMovementService)
    → Increment : QteDisponible += qte
    → Transfer  : source QteDisponible -= qte, dest QteDisponible += qte
        ↓
Mise à jour (Id_stock != 0, POST)
    → QteDisponible mis à jour, NumeroLot mis à jour
    → DateExpiration, Id_User_Delegue, Id_Produit NON mis à jour (BUG)
        ↓
Suppression (DELETE /{id}?type=Delegue)
    → Règle métier : QteDisponible > 0 → refus
    → IsDeleted = true
```

### Relation avec StockMovement

```csharp
modelBuilder.Entity<StockMovement>()
    .HasOne<Stock_Delegue>()
    .WithMany()
    .HasForeignKey(m => m.Id_Stock)
    .OnDelete(DeleteBehavior.Cascade);
```

**Attention :** La cascade est sur `DeleteBehavior.Cascade` — si un `Stock_Delegue` est hard-deleted (ce qui ne peut pas arriver avec le soft-delete actuel), tous ses mouvements seraient supprimés. Avec le soft-delete actuel, la cascade ne se déclenche jamais.

### Règle de suppression
La règle `QteDisponible > 0 → cannot delete` est cohérente métier (on ne supprime pas un stock encore utilisable). Mais avec le BUG P2 (`!IsDeleted` manquant), un stock déjà supprimé peut bloquer faussement cette règle.

### Relation TPH et TypeStock

Le champ discriminant `TypeStock` n'est jamais exposé dans `StockDelegueDto`. Le frontend ne peut pas distinguer un stock "Standard" d'un stock "Echantillon" ou "Gratuite" à partir des données retournées. Si le BUG P1 n'est pas corrigé, des stocks d'autres types peuvent apparaître dans la liste délégués sans que l'UI le signale.

---

## 7. Bugs Trouvés

### BUG P1 — `GetAllStocksAsync` : absence de filtre TypeStock

**Fichier :** `StockDelegueService.cs` ligne 23  
**Sévérité :** Critique (données incorrectes)

**Description :**  
`_db.StocksDelegues` en EF Core TPH, quand la classe appelante est la **classe de base** (`Stock_Delegue`), retourne toutes les lignes de la table `[Stocks]` sans filtre discriminant. La requête inclut `Stock_Echantillon` (TypeStock="Echantillon") et `Stock_Gratuite` (TypeStock="Gratuite") dans les résultats de l'API stocks-délégués.

**Code actuel :**
```csharp
var stocks = await _db.StocksDelegues
    .AsNoTracking()
    .Where(s => !s.IsDeleted)
    .OrderByDescending(s => s.DateCreation)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

**Fix :**
```csharp
var stocks = await _db.StocksDelegues
    .AsNoTracking()
    .Where(s => !s.IsDeleted && EF.Property<string>(s, "TypeStock") == "Standard")
    .OrderByDescending(s => s.DateCreation)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

**Impact :** La même correction est nécessaire dans `GetStocksByDelegueAsync` et `GetStockByProduitAsync` qui peuvent aussi retourner des stocks d'autres types.

---

### BUG P1 — `CreateUpdateStockAsync` : UPDATE ignore DateExpiration, Id_User_Delegue, Id_Produit

**Fichier :** `StockDelegueService.cs` lignes 59-60  
**Sévérité :** Critique (perte de données)

**Description :**  
Le chemin UPDATE (quand `dto.Id_stock != 0`) ne met à jour que `QteDisponible` et `NumeroLot`. Les champs `DateExpiration`, `Id_User_Delegue`, et `Id_Produit` sont silencieusement ignorés. Un ADMIN qui réassigne un stock à un autre délégué ou corrige une date d'expiration depuis le formulaire Angular ne verra aucune erreur — mais les modifications ne seront jamais persistées.

**Code actuel :**
```csharp
stock.QteDisponible = dto.QteDisponible;
stock.NumeroLot     = dto.NumeroLot;
```

**Fix :**
```csharp
stock.QteDisponible   = dto.QteDisponible;
stock.NumeroLot       = dto.NumeroLot;
stock.DateExpiration  = dto.DateExpiration;
stock.Id_User_Delegue = dto.Id_User_Delegue;
stock.Id_Produit      = dto.Id_Produit;
```

---

### BUG P2 — `DeleteStockAsync` : requête sans filtre `!IsDeleted`

**Fichier :** `StockDelegueService.cs` lignes 116-117  
**Sévérité :** Modérée (comportement incohérent)

**Description :**  
La requête de suppression ne filtre pas les stocks déjà soft-deleted. Un stock avec `IsDeleted = true` peut être retrouvé, sa `QteDisponible` vérifiée (produisant un `false` si > 0, ou un double soft-delete si = 0), et la méthode retourne un résultat incohérent.

**Code actuel :**
```csharp
var stock = await _db.StocksDelegues
    .FirstOrDefaultAsync(s => s.Id_stock == idStock);
```

**Fix :**
```csharp
var stock = await _db.StocksDelegues
    .FirstOrDefaultAsync(s => s.Id_stock == idStock && !s.IsDeleted);
```

---

### BUG P2 — `StockDelegueDto.cs` : `NumeroLot` nullable warning CS8618

**Fichier :** `CynapCRM.Services.InventoryAPI/Models/Dto/StockDelegueDto.cs` ligne 11  
**Sévérité :** Modérée (avertissement compilateur, NullReferenceException possible)

**Description :**  
`StockDelegueDto.NumeroLot` est déclaré `string` sans initialisation. Avec nullable reference types activés (`<Nullable>enable</Nullable>` dans le projet), cela génère CS8618. Si le champ n'est pas renseigné dans une requête POST, `NumeroLot` sera `null`, alors que la contrainte DB la déclare `IS NOT NULL`.

**Code actuel :**
```csharp
public string NumeroLot { get; set; }
```

**Fix :**
```csharp
public string NumeroLot { get; set; } = string.Empty;
```

---

### BUG P3 — `stock-list.component.ts` : calcul du total approximatif

**Fichier :** `Cynapharm/src/app/features/inventory/stocks/stock-list/stock-list.component.ts` lignes 60-62  
**Sévérité :** Mineure (UX dégradée)

**Description :**  
Le total passé au composant paginator est une approximation. Quand la page est pleine, `total = page * pageSize + 1` indique qu'il y a au moins une page de plus, mais n'affiche pas le vrai total.

**Code actuel :**
```typescript
this.total = data.length < this.pageSize
  ? (this.page - 1) * this.pageSize + data.length
  : this.page * this.pageSize + 1;
```

**Fix complet (nécessite modification backend) :**  
Le backend doit retourner un objet `{ items: [...], totalCount: N }` au lieu d'une simple liste. Côté Angular, utiliser `data.totalCount` directement.

**Fix partiel (Angular only) :**  
Conserver l'approximation mais utiliser un indicateur "il y a une page suivante" dans le paginator plutôt qu'un faux total.

---

### BUG mineur — `stock-form.component.ts` : message d'erreur en anglais

**Fichier :** `Cynapharm/src/app/features/inventory/stocks/stock-form/stock-form.component.ts` ligne 217  
**Sévérité :** Cosmétique

**Code actuel :**
```typescript
this.submitError = 'Error saving stock.';
```

**Fix :**
```typescript
this.submitError = 'Erreur lors de l\'enregistrement du stock.';
```

---

## 8. Fonctionnalités Manquantes

### 8.1 Comptage total backend pour pagination correcte

Le `GET /api/stocks-delegue` ne retourne pas de `totalCount`. Le paginator Angular affiche un total approximatif. Pour une pagination correcte, il faudrait soit :
- Retourner `{ items, totalCount }` depuis le backend
- Ou ajouter un endpoint `GET /api/stocks-delegue/count`

### 8.2 Filtre Angular dans stock-list

La liste `stock-list` n'a pas de barre de filtre (pas de recherche par délégué, produit, ou numéro de lot). Comparé à `movement-list` qui a une filter bar complète, la `stock-list` manque de cette fonctionnalité.

### 8.3 TypeStock manquant dans le DTO

`StockDelegueDto` n'expose pas le discriminant `TypeStock`. Si un jour les stocks de différents types sont mélangés (BUG P1 non corrigé), l'UI ne peut pas les distinguer. Exposer `TypeStock` dans le DTO permettrait une meilleure visibilité.

### 8.4 `dateCreation` manquant dans le DTO

`StockDelegueDto` n'expose pas `DateCreation` alors que le modèle le stocke et le service `OrderBy` l'utilise. La liste frontend ne peut pas afficher la date de création.

### 8.5 Endpoint de comptage par délégué

`GET by-delegue/{id}` retourne tous les stocks sans pagination. Pour un délégué avec des centaines de stocks, cela peut être problématique.

### 8.6 Recherche par lot — `GetStockByLotAsync` retourne un seul résultat

Si plusieurs stocks partagent le même numéro de lot (different délégués), seul le premier est retourné. Un endpoint `GET by-lot/{numero}/all` retournant une liste serait plus complet.

---

## 9. Plan de Correction

> Note : les StatusCode(515) sont intentionnels et ne doivent pas être modifiés.

### Fix 1 — Service : filtrer TypeStock dans GetAllStocksAsync [P1]

**Fichier :** `CynapCRM.Services.InventoryAPI/Service/StockDelegueService.cs`

Dans `GetAllStocksAsync`, `GetStocksByDelegueAsync`, et `GetStockByProduitAsync`, ajouter :
```csharp
.Where(s => EF.Property<string>(s, "TypeStock") == "Standard" && !s.IsDeleted)
```

---

### Fix 2 — Service : UPDATE doit persister tous les champs [P1]

**Fichier :** `CynapCRM.Services.InventoryAPI/Service/StockDelegueService.cs`

Dans `CreateUpdateStockAsync`, bloc `else` (update), ajouter :
```csharp
stock.DateExpiration  = dto.DateExpiration;
stock.Id_User_Delegue = dto.Id_User_Delegue;
stock.Id_Produit      = dto.Id_Produit;
```

---

### Fix 3 — Service : DeleteStockAsync — ajouter filtre !IsDeleted [P2]

**Fichier :** `CynapCRM.Services.InventoryAPI/Service/StockDelegueService.cs`

```csharp
var stock = await _db.StocksDelegues
    .FirstOrDefaultAsync(s => s.Id_stock == idStock && !s.IsDeleted);
```

---

### Fix 4 — DTO : initialiser NumeroLot [P2]

**Fichier :** `CynapCRM.Services.InventoryAPI/Models/Dto/StockDelegueDto.cs`

```csharp
public string NumeroLot { get; set; } = string.Empty;
```

---

### Fix 5 — Angular : message d'erreur en français [cosmétique]

**Fichier :** `Cynapharm/src/app/features/inventory/stocks/stock-form/stock-form.component.ts`

```typescript
this.submitError = 'Erreur lors de l\'enregistrement du stock.';
```

---

### Fix 6 — Angular : faute de frappe getDelegrueName [cosmétique, optionnel]

**Fichier :** `Cynapharm/src/app/features/inventory/stocks/stock-list/stock-list.component.ts`

Renommer `getDelegrueName` en `getDeleagueName` (ou `getDelegateName`). Mettre à jour le template `.html` en conséquence.

---

## 10. Code Complet de Chaque Fichier Lu

---

### `CynapCRM.Services.InventoryAPI/Models/Stock_Delegue.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.InventoryAPI.Models
{
    public class Stock_Delegue
    {
        public int Id_stock { get; set; }

        public int Id_User_Delegue { get; set; }

        public int Id_Produit { get; set; }

        public string NumeroLot { get; set; } = string.Empty;
        public DateTime DateCreation { get; set; }


        public DateTime DateExpiration { get; set; }

        public int QteDisponible { get; set; }

        public int QteReservee { get; set; } = 0;
        public bool IsDeleted { get; internal set; } = false;


    }
}
```

---

### `CynapCRM.Services.InventoryAPI/Models/Dto/StockDelegueDto.cs`

```csharp
namespace CynapCRM.Services.InventoryAPI.Models.Dto
{
    public class StockDelegueDto
    {
        public int Id_stock { get; set; }

        public int Id_User_Delegue { get; set; }

        public int Id_Produit { get; set; }

        public string NumeroLot { get; set; }   // BUG : manque = string.Empty

        public DateTime DateExpiration { get; set; }

        public int QteDisponible { get; set; }

        public int QteReservee { get; set; }
    }
}
```

---

### `CynapCRM.Services.InventoryAPI/Service/IService/IStockDelegueService.cs`

```csharp
using CynapCRM.Services.InventoryAPI.Models;
using CynapCRM.Services.InventoryAPI.Models.Dto;

namespace CynapCRM.Services.InventoryAPI.Service.IService
{
    public interface IStockDelegueService
    {

        Task<IEnumerable<StockDelegueDto>> GetAllStocksAsync(int pageNumber, int pageSize);
        Task<StockDelegueDto?> CreateUpdateStockAsync(StockDelegueDto stockDto);
        Task<StockDelegueDto?> GetStockByIdAsync(int idStock);
        Task<IEnumerable<StockDelegueDto>> GetStocksByDelegueAsync(int idDelegue);
        Task<IEnumerable<StockDelegueDto>> GetStockByProduitAsync(int idProduit);
        Task<StockDelegueDto?> GetStockByLotAsync(string numeroLot);

        Task<bool> DeleteStockAsync(int idStock, StockType type);
    }
}
```

---

### `CynapCRM.Services.InventoryAPI/Controllers/StocksDelegueController.cs`

```csharp
using CynapCRM.Services.InventoryAPI.Models;
using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.InventoryAPI.Controllers
{

    // ═══════════════════════════════════════
    // StocksDelegueController.cs
    // ═══════════════════════════════════════

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
        public async Task<IActionResult> GetStocksByDelegue(int idDelegue)
        {
            try
            {
                if (idDelegue <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id délégué invalide.";
                    return BadRequest(_response);
                }
                var result = await _stockDelegueService.GetStocksByDelegueAsync(idDelegue);
                _response.Result = result;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpGet("by-produit/{idProduit:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetStocksByProduit(int idProduit)
        {
            try
            {
                if (idProduit <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id produit invalide.";
                    return BadRequest(_response);
                }
                var result = await _stockDelegueService.GetStockByProduitAsync(idProduit);
                _response.Result = result;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpGet("by-lot/{numeroLot}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetStockByLot(string numeroLot)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(numeroLot))
                {
                    _response.IsSuccess = false;
                    _response.Message = "Numéro de lot invalide.";
                    return BadRequest(_response);
                }
                var stock = await _stockDelegueService.GetStockByLotAsync(numeroLot);
                if (stock == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = $"Aucun stock trouvé pour le lot {numeroLot}.";
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

        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> CreateOrUpdateStock(
            [FromBody] StockDelegueDto stockDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de stock invalides.";
                    return BadRequest(_response);
                }
                var result = await _stockDelegueService.CreateUpdateStockAsync(stockDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur lors de la mise à jour du stock.";
                    return BadRequest(_response);
                }
                _response.Result = result;
                _response.Message = "Stock enregistré avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpDelete("{idStock:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteStock(
            int idStock,
            [FromQuery] StockType type)
        {
            try
            {
                if (idStock <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id stock invalide.";
                    return BadRequest(_response);
                }
                bool isDeleted = await _stockDelegueService.DeleteStockAsync(idStock, type);
                if (!isDeleted)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Suppression impossible (stock inexistant ou quantité restante > 0).";
                    return BadRequest(_response);
                }
                _response.Message = "Stock supprimé.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }
    }
}
```

---

### `CynapCRM.Services.InventoryAPI/Service/StockDelegueService.cs`

```csharp
using AutoMapper;
using CynapCRM.Services.InventoryAPI.Data;
using CynapCRM.Services.InventoryAPI.Models;
using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.InventoryAPI.Service
{
    public class StockDelegueService : IStockDelegueService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        public StockDelegueService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;

        }
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
                stock = new Stock_Delegue
                {
                    Id_User_Delegue = dto.Id_User_Delegue,
                    Id_Produit = dto.Id_Produit,
                    NumeroLot = dto.NumeroLot,
                    DateExpiration = dto.DateExpiration,
                    QteDisponible = dto.QteDisponible,
                    QteReservee = 0,
                    DateCreation = DateTime.UtcNow,
                    IsDeleted = false
                };
                _db.StocksDelegues.Add(stock);
            }
            else
            {
                stock = await _db.StocksDelegues
                    .FirstOrDefaultAsync(s => s.Id_stock == dto.Id_stock && !s.IsDeleted);

                if (stock == null) return null;

                stock.QteDisponible = dto.QteDisponible;
                stock.NumeroLot = dto.NumeroLot;
                // BUG : DateExpiration, Id_User_Delegue, Id_Produit non mis à jour
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<StockDelegueDto>(stock);
        }
        public async Task<StockDelegueDto?> GetStockByIdAsync(int idStock)
        {

            var stock = await _db.StocksDelegues
                            .AsNoTracking()
                            .FirstOrDefaultAsync(s => s.Id_stock == idStock && !s.IsDeleted);
            if (stock == null)
            {
                return null;
            }
            return _mapper.Map<StockDelegueDto>(stock);
        }
        public async Task<IEnumerable<StockDelegueDto>> GetStocksByDelegueAsync(int idDelegue)
        {

            var stocks = await _db.StocksDelegues
                            .AsNoTracking()
                            .Where(s => s.Id_User_Delegue == idDelegue && !s.IsDeleted)
                            .OrderByDescending(s => s.DateCreation)
                            .ToListAsync();

            return _mapper.Map<IEnumerable<StockDelegueDto>>(stocks);
        }
        public async Task<IEnumerable<StockDelegueDto>> GetStockByProduitAsync(int idProduit)
        {

            var stocks = await _db.StocksDelegues
                            .AsNoTracking()
                            .Where(s => s.Id_Produit == idProduit && !s.IsDeleted)
                            .ToListAsync();

            return _mapper.Map<IEnumerable<StockDelegueDto>>(stocks);
        }
        public async Task<StockDelegueDto?> GetStockByLotAsync(string numeroLot)
        {

            var stock = await _db.StocksDelegues
                            .AsNoTracking()
                            .FirstOrDefaultAsync(s =>s.NumeroLot == numeroLot && !s.IsDeleted);

            if (stock == null)
            {
                return null;
            }
            return _mapper.Map<StockDelegueDto>(stock);
        }
        public async Task<bool> DeleteStockAsync(int idStock, StockType type)
        {
            if (type != StockType.Delegue) return false;

            var stock = await _db.StocksDelegues
                .FirstOrDefaultAsync(s => s.Id_stock == idStock);
                // BUG : manque && !s.IsDeleted
            if (stock == null) return false;

            // Règle métier : ne pas supprimer un stock avec quantité restante
            if (stock.QteDisponible > 0) return false;

            stock.IsDeleted = true;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
```

---

### `CynapCRM.Services.InventoryAPI/Data/AppDbContext.cs`

```csharp
using CynapCRM.Services.InventoryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.InventoryAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Stock_Delegue> StocksDelegues { get; set; }
        public DbSet<Echantillon> Echantillons { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Héritage TPH
            modelBuilder.Entity<Stock_Delegue>()
                .HasDiscriminator<string>("TypeStock")
                .HasValue<Stock_Delegue>("Standard")
                .HasValue<Stock_Echantillon>("Echantillon")
                .HasValue<Stock_Gratuite>("Gratuite");

            // Clés primaires explicites
            modelBuilder.Entity<Stock_Delegue>().HasKey(s => s.Id_stock);
            modelBuilder.Entity<Echantillon>().HasKey(e => e.Id_Distribution);
            modelBuilder.Entity<StockMovement>().HasKey(m => m.Id_Movement);

            // Index
            modelBuilder.Entity<Stock_Delegue>().HasIndex(s => s.NumeroLot);
            modelBuilder.Entity<Echantillon>().HasIndex(e => e.NumeroLot);
            modelBuilder.Entity<Stock_Delegue>().HasIndex(s => s.Id_User_Delegue);
            modelBuilder.Entity<Echantillon>().HasIndex(e => e.Id_Medecin);
            modelBuilder.Entity<Echantillon>().HasIndex(e => e.Id_Pharmacien);
            modelBuilder.Entity<StockMovement>().HasIndex(m => m.Id_Stock);

            // Contraintes
            modelBuilder.Entity<Stock_Delegue>()
                .Property(s => s.NumeroLot).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Echantillon>()
                .Property(e => e.NumeroLot).IsRequired().HasMaxLength(100);

            // Relations
            modelBuilder.Entity<StockMovement>()
                .HasOne<Stock_Delegue>()
                .WithMany()
                .HasForeignKey(m => m.Id_Stock)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Stock_Delegue>()
                .Property(s => s.QteReservee).HasDefaultValue(0);
            modelBuilder.Entity<Echantillon>()
                .Property(e => e.DateDistribution).HasDefaultValueSql("GETUTCDATE()");
            modelBuilder.Entity<StockMovement>()
                .Property(m => m.DateMovement).HasDefaultValueSql("GETUTCDATE()");

            // Noms des tables
            modelBuilder.Entity<Stock_Delegue>().ToTable("Stocks");
            modelBuilder.Entity<Echantillon>().ToTable("Distributions_Echantillons");
            modelBuilder.Entity<StockMovement>().ToTable("Stock_Movements");
        }
    }
}
```

---

### `Cynapharm/src/app/features/inventory/stocks/services/stock.service.ts`

```typescript
import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../../core/services/api.service';
import { StockType } from '../../../../core/models/enums';

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
  constructor(private api: ApiService) {}
  private u<T>(r: any): T { return r?.Result ?? r?.result ?? r; }

  getAll(pageNumber = 1, pageSize = 20): Observable<StockDelegueDto[]> {
    const p = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.api.get<any>(this.base, p).pipe(map(r => this.u<StockDelegueDto[]>(r) ?? []));
  }
  getById(id: number)             { return this.api.get<any>(`${this.base}/${id}`).pipe(map(r => this.u<StockDelegueDto>(r))); }
  getByDelegue(id: number)        { return this.api.get<any>(`${this.base}/by-delegue/${id}`).pipe(map(r => this.u<StockDelegueDto[]>(r) ?? [])); }
  getByProduit(id: number)        { return this.api.get<any>(`${this.base}/by-produit/${id}`).pipe(map(r => this.u<StockDelegueDto[]>(r) ?? [])); }
  getByLot(numero: string)        { return this.api.get<any>(`${this.base}/by-lot/${numero}`).pipe(map(r => this.u<StockDelegueDto>(r))); }
  createOrUpdate(dto: StockDelegueDto): Observable<StockDelegueDto> {
    return this.api.post<any>(this.base, dto).pipe(map(r => this.u<StockDelegueDto>(r)));
  }
  delete(id: number, type: StockType): Observable<void> {
    return this.api.delete<void>(`${this.base}/${id}?type=${type}`);
  }
}
```

---

### `Cynapharm/src/app/features/inventory/stocks/stock-list/stock-list.component.ts`

```typescript
import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { StockService, StockDelegueDto } from '../services/stock.service';
import { UserService } from '../../../users/user.service';
import { ProductService } from '../../../products/product.service';
import { LotService } from '../../../lots/lot.service';
import { ToastService } from '../../../../shared/services/toast.service';
import { StockType } from '../../../../core/models/enums';
import { PaginatorComponent } from '../../../../shared/components/paginator/paginator.component';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-stock-list',
  standalone: true,
  imports: [CommonModule, RouterLink, PaginatorComponent, EmptyStateComponent],
  templateUrl: './stock-list.component.html',
  styleUrls: ['./stock-list.component.css']
})
export class StockListComponent implements OnInit, OnDestroy {
  stocks:   StockDelegueDto[] = [];
  loading   = false;
  error     = '';
  page      = 1;
  pageSize  = 20;
  total     = 0;

  delegueNames: Record<number, string> = {};
  productNames: Record<number, string> = {};
  lotDates:     Record<string, string> = {};

  showDeleteModal  = false;
  deletingStock:   StockDelegueDto | null = null;
  deleting         = false;

  private destroy$ = new Subject<void>();

  constructor(
    private svc:        StockService,
    private toast:      ToastService,
    private userSvc:    UserService,
    private productSvc: ProductService,
    private lotSvc:     LotService,
    private cdr:        ChangeDetectorRef
  ) {}

  ngOnInit():    void { this.load(); }
  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  load(): void {
    this.loading = true;
    this.svc.getAll(this.page, this.pageSize).pipe(takeUntil(this.destroy$)).subscribe({
      next: data => {
        this.stocks  = data;
        this.total   = data.length < this.pageSize
          ? (this.page - 1) * this.pageSize + data.length
          : this.page * this.pageSize + 1;  // BUG P3 : approximation
        this.loading = false;
        this.loadRelatedData(data);
        this.cdr.markForCheck();
      },
      error: () => { this.error = 'Erreur lors du chargement.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  onPage(p: number): void { this.page = p; this.load(); }

  openDelete(s: StockDelegueDto): void {
    this.deletingStock   = s;
    this.showDeleteModal = true;
    this.cdr.markForCheck();
  }

  cancelDelete(): void {
    this.showDeleteModal = false;
    this.deletingStock   = null;
    this.cdr.markForCheck();
  }

  confirmDelete(): void {
    if (!this.deletingStock?.id_stock) return;
    this.deleting = true;
    this.svc.delete(this.deletingStock.id_stock, StockType.Delegue)
      .pipe(takeUntil(this.destroy$)).subscribe({
        next: () => {
          this.toast.showSuccess('Stock supprimé.');
          this.showDeleteModal = false;
          this.deletingStock   = null;
          this.deleting        = false;
          this.load();
        },
        error: () => {
          this.toast.showError('Erreur lors de la suppression.');
          this.deleting = false;
          this.cdr.markForCheck();
        }
      });
  }

  private loadRelatedData(stocks: StockDelegueDto[]): void {
    const delegueIds = [...new Set(stocks.map(s => s.id_User_Delegue).filter(id => id > 0))];
    delegueIds.forEach(id => {
      if (this.delegueNames[id]) return;
      this.userSvc.getUserById(id).pipe(takeUntil(this.destroy$)).subscribe({
        next: (res: any) => {
          const u = res?.Result ?? res?.result ?? res;
          this.delegueNames[id] =
            u?.fullName ?? u?.FullName ?? u?.name ?? u?.Name ?? u?.email ?? `#${id}`;
          this.cdr.markForCheck();
        },
        error: () => { this.delegueNames[id] = `#${id}`; }
      });
    });

    const productIds = [...new Set(stocks.map(s => s.id_Produit).filter(id => id > 0))];
    productIds.forEach(id => {
      if (this.productNames[id]) return;
      this.productSvc.getProductById(id).pipe(takeUntil(this.destroy$)).subscribe({
        next: (data: any) => {
          const raw = data?.Result ?? data?.result ?? data;
          this.productNames[id] = raw?.Nom ?? raw?.nom ?? `#${id}`;
          this.cdr.markForCheck();
        },
        error: () => { this.productNames[id] = `#${id}`; }
      });
    });

    const lots = [...new Set(stocks.map(s => s.numeroLot).filter(n => !!n))];
    lots.forEach(num => {
      if (this.lotDates[num]) return;
      this.lotSvc.getLotByNumero(num).pipe(takeUntil(this.destroy$)).subscribe({
        next: lot => { this.lotDates[num] = lot.dateExpiration ?? ''; this.cdr.markForCheck(); },
        error: () => {}
      });
    });
  }

  getDelegrueName(id: number): string { return this.delegueNames[id] ?? `#${id}`; }
  getProductName(id: number):  string { return this.productNames[id] ?? `#${id}`; }
  getLotDate(num: string, fallback: string): string { return this.lotDates[num] ?? fallback; }
}
```

---

### `Cynapharm/src/app/features/inventory/stocks/stock-list/stock-list.component.html`

```html
<div class="page-wrapper">
  <div class="page-header">
    <div><h1 class="page-title">Stocks Délégués</h1><p class="page-sub">{{ stocks.length }} ligne(s)</p></div>
    <a routerLink="/inventory/stocks/new" class="btn-primary-link">+ Nouveau stock</a>
  </div>

  @if (error)   { <div class="alert-danger">{{ error }}</div> }
  @if (loading) { <div class="loading-block"><div class="spinner"></div> Chargement…</div> }
  @else if (stocks.length === 0) {
    <app-empty-state title="Aucun stock délégué" message="Commencez par créer un stock.">
      <a routerLink="/inventory/stocks/new" class="btn-primary-link">+ Créer</a>
    </app-empty-state>
  } @else {
    <div class="table-card">
      <div class="table-scroll">
      <table class="data-table">
        <thead>
          <tr>
            <th>Délégué</th>
            <th>Produit</th>
            <th>N° Lot</th>
            <th>Expiration</th>
            <th>Disponible</th>
            <th>Réservé</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          @for (s of stocks; track s.id_stock) {
            <tr>
              <td>{{ getDelegrueName(s.id_User_Delegue) }}</td>
              <td>{{ getProductName(s.id_Produit) }}</td>
              <td class="font-mono">{{ s.numeroLot }}</td>
              <td>{{ getLotDate(s.numeroLot, s.dateExpiration) | date:'dd/MM/yyyy' }}</td>
              <td class="text-center"><strong [class.text-danger]="s.qteDisponible === 0">{{ s.qteDisponible }}</strong></td>
              <td class="text-center">{{ s.qteReservee }}</td>
              <td class="actions-cell">
                <a [routerLink]="['/inventory/stocks', s.id_stock]" class="act-btn view">👁</a>
                <a [routerLink]="['/inventory/stocks', s.id_stock, 'edit']" class="act-btn edit">✏</a>
                <button class="act-btn delete" (click)="openDelete(s)" title="Supprimer">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <polyline points="3 6 5 6 21 6"/>
                    <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/>
                  </svg>
                </button>
              </td>
            </tr>
          }
        </tbody>
      </table>
      </div>
      <app-paginator [page]="page" [pageSize]="pageSize" [total]="total" (pageChange)="onPage($event)"></app-paginator>
    </div>
  }
</div>

@if (showDeleteModal && deletingStock) {
  <div class="modal-overlay" (click)="cancelDelete()">
    <div class="modal-box" (click)="$event.stopPropagation()">
      <div class="modal-icon-danger">
        <svg viewBox="0 0 24 24" fill="none" stroke="#dc2626" stroke-width="2">
          <polyline points="3 6 5 6 21 6"/>
          <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/>
        </svg>
      </div>
      <h3 class="modal-title">Supprimer ce stock ?</h3>
      <p class="modal-body">
        <strong>{{ getDelegrueName(deletingStock.id_User_Delegue) }}</strong> —
        {{ getProductName(deletingStock.id_Produit) }} /
        <span class="font-mono">{{ deletingStock.numeroLot }}</span><br>
        <small>Disponible : {{ deletingStock.qteDisponible }} · Réservé : {{ deletingStock.qteReservee }}</small><br><br>
        Cette action est irréversible.
      </p>
      <div class="modal-actions">
        <button class="btn-cancel" (click)="cancelDelete()" [disabled]="deleting">Annuler</button>
        <button class="btn-confirm-danger" (click)="confirmDelete()" [disabled]="deleting">
          @if (deleting) { <span class="spinner-sm"></span> } @else { Supprimer }
        </button>
      </div>
    </div>
  </div>
}
```

---

### `Cynapharm/src/app/features/inventory/stocks/stock-detail/stock-detail.component.ts`

```typescript
import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { StockService, StockDelegueDto } from '../services/stock.service';
import { UserService } from '../../../users/user.service';
import { ProductService } from '../../../products/product.service';
import { LotService } from '../../../lots/lot.service';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-stock-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, EmptyStateComponent],
  templateUrl: './stock-detail.component.html',
  styleUrls: ['./stock-detail.component.css']
})
export class StockDetailComponent implements OnInit, OnDestroy {
  stock:   StockDelegueDto | null = null;
  loading  = false;
  error    = '';

  delegeName   = '';
  productName  = '';
  lotDate      = '';

  private destroy$ = new Subject<void>();

  constructor(
    private route:      ActivatedRoute,
    private svc:        StockService,
    private userSvc:    UserService,
    private productSvc: ProductService,
    private lotSvc:     LotService,
    private cdr:        ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) { this.error = 'Identifiant invalide.'; return; }
    this.loading = true;
    this.svc.getById(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: data => {
        this.stock   = data;
        this.loading = false;
        if (data) this.loadRelated(data);
        this.cdr.markForCheck();
      },
      error: () => { this.error = 'Impossible de charger le stock.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  private loadRelated(s: StockDelegueDto): void {
    if (s.id_User_Delegue > 0) {
      this.userSvc.getUserById(s.id_User_Delegue).pipe(takeUntil(this.destroy$)).subscribe({
        next: (res: any) => {
          const u = res?.Result ?? res?.result ?? res;
          this.delegeName =
            u?.fullName ?? u?.FullName ??
            u?.name     ?? u?.Name     ??
            u?.userName ?? u?.UserName ??
            u?.email    ?? u?.Email    ?? `#${s.id_User_Delegue}`;
          this.cdr.markForCheck();
        },
        error: () => { this.delegeName = `#${s.id_User_Delegue}`; }
      });
    }

    if (s.id_Produit > 0) {
      this.productSvc.getProductById(s.id_Produit).pipe(takeUntil(this.destroy$)).subscribe({
        next: (data: any) => {
          const raw = data?.Result ?? data?.result ?? data;
          this.productName = raw?.Nom ?? raw?.nom ?? `#${s.id_Produit}`;
          this.cdr.markForCheck();
        },
        error: () => { this.productName = `#${s.id_Produit}`; }
      });
    }

    if (s.numeroLot) {
      this.lotSvc.getLotByNumero(s.numeroLot).pipe(takeUntil(this.destroy$)).subscribe({
        next: lot => { this.lotDate = lot.dateExpiration ?? s.dateExpiration; this.cdr.markForCheck(); },
        error: ()  => { this.lotDate = s.dateExpiration; }
      });
    } else {
      this.lotDate = s.dateExpiration;
    }
  }
}
```

---

### `Cynapharm/src/app/features/inventory/stocks/stock-detail/stock-detail.component.html`

```html
<div class="page">
  <div class="page-header">
    <a routerLink="/inventory/stocks" class="back-link">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M19 12H5M12 19l-7-7 7-7"/></svg>
      Retour aux stocks
    </a>
    <h1>Détail du stock</h1>
  </div>

  <div *ngIf="loading" class="loading-wrap">
    <div class="spinner"></div>
    <span>Chargement...</span>
  </div>

  <div *ngIf="error && !loading" class="error-banner">{{ error }}</div>

  <div *ngIf="!loading && !error && !stock">
    <app-empty-state title="Stock introuvable" message="Aucun stock trouvé pour cet identifiant."></app-empty-state>
  </div>

  <div *ngIf="stock && !loading" class="detail-card">
    <div class="detail-grid">
      <div class="detail-item">
        <span class="label">ID Stock</span>
        <span class="value">{{ stock.id_stock ?? '—' }}</span>
      </div>
      <div class="detail-item">
        <span class="label">Délégué</span>
        <span class="value">{{ delegeName || ('Chargement…') }}</span>
      </div>
      <div class="detail-item">
        <span class="label">Produit</span>
        <span class="value">{{ productName || ('Chargement…') }}</span>
      </div>
      <div class="detail-item">
        <span class="label">Numéro de lot</span>
        <span class="value mono">{{ stock.numeroLot }}</span>
      </div>
      <div class="detail-item">
        <span class="label">Date d'expiration</span>
        <span class="value">{{ (lotDate || stock.dateExpiration) | date:'dd/MM/yyyy' }}</span>
      </div>
      <div class="detail-item">
        <span class="label">Qté disponible</span>
        <span class="value qty">{{ stock.qteDisponible }}</span>
      </div>
      <div class="detail-item">
        <span class="label">Qté réservée</span>
        <span class="value qty">{{ stock.qteReservee }}</span>
      </div>
    </div>
    <div class="actions">
      <a [routerLink]="['/inventory/stocks', stock.id_stock, 'edit']" class="btn btn-primary">Modifier</a>
      <a routerLink="/inventory/stocks" class="btn btn-secondary">Retour</a>
    </div>
  </div>
</div>
```

---

### `Cynapharm/src/app/features/inventory/stocks/stock-form/stock-form.component.ts`

```typescript
import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { StockService, StockDelegueDto } from '../services/stock.service';
import { UserService } from '../../../users/user.service';
import { ProductService } from '../../../products/product.service';
import { LotService } from '../../../lots/lot.service';
import { LotDto } from '../../../lots/lot.model';

@Component({
  selector: 'app-stock-form',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './stock-form.component.html',
  styleUrls: ['./stock-form.component.css']
})
export class StockFormComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  isEdit      = false;
  editId: number | null = null;
  loadingData = false;
  saving      = false;
  fetchError  = '';
  submitError = '';
  successMsg  = '';

  delegues:      any[]    = [];
  products:      any[]    = [];
  lots:          LotDto[] = [];
  loadingLots    = false;

  lotDateDisplay = '';

  private destroy$ = new Subject<void>();

  constructor(
    private fb:         FormBuilder,
    private route:      ActivatedRoute,
    private router:     Router,
    private svc:        StockService,
    private userSvc:    UserService,
    private productSvc: ProductService,
    private lotSvc:     LotService,
    private cdr:        ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      id_User_Delegue: [null, [Validators.required]],
      id_Produit:      [null, [Validators.required]],
      numeroLot:       ['',   [Validators.required]],
      dateExpiration:  ['',   [Validators.required]],
      qteDisponible:   [null, [Validators.required, Validators.min(1)]]
    });

    this.form.get('numeroLot')!.disable();

    this.userSvc.getUsersByRole('DELEGUE').pipe(takeUntil(this.destroy$))
      .subscribe({
        next: users => {
          this.delegues = users.map(u => ({
            ...u,
            id:   u.id   ?? u.Id,
            name: u.name ?? u.Name ?? u.fullName ?? u.FullName ?? u.email ?? u.Email
          })).filter(u => u.id != null);
          this.cdr.markForCheck();
        },
        error: () => {}
      });

    this.productSvc.getVisibleProducts().pipe(takeUntil(this.destroy$))
      .subscribe({
        next: prods => {
          this.products = prods.map((p: any) => ({
            ...p,
            Id_Produit: p.Id_Produit ?? p.id_Produit,
            Nom:        p.Nom        ?? p.nom ?? ''
          }));
          this.cdr.markForCheck();
        },
        error: () => {}
      });

    this.form.get('id_Produit')!.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(id => {
      this.lots = [];
      this.lotDateDisplay = '';
      const lotCtrl = this.form.get('numeroLot')!;
      lotCtrl.setValue('', { emitEvent: false });
      lotCtrl.disable();
      this.form.patchValue({ dateExpiration: '' }, { emitEvent: false });
      if (id) this.loadLots(+id);
    });

    this.form.get('numeroLot')!.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(num => {
      const lot = this.lots.find(l => l.numero === num);
      if (lot?.dateExpiration) {
        const iso  = lot.dateExpiration.substring(0, 10);
        const [y, m, d] = iso.split('-');
        this.lotDateDisplay = `${d}/${m}/${y}`;
        this.form.patchValue({ dateExpiration: iso }, { emitEvent: false });
      } else {
        this.lotDateDisplay = '';
        this.form.patchValue({ dateExpiration: '' }, { emitEvent: false });
      }
      this.cdr.markForCheck();
    });

    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.isEdit     = true;
      this.editId     = id;
      this.loadingData = true;
      this.svc.getById(id).pipe(takeUntil(this.destroy$)).subscribe({
        next: data => {
          this.form.patchValue({
            id_User_Delegue: data.id_User_Delegue,
            id_Produit:      data.id_Produit,
            numeroLot:       data.numeroLot,
            dateExpiration:  data.dateExpiration?.substring(0, 10) ?? '',
            qteDisponible:   data.qteDisponible
          }, { emitEvent: false });

          const raw = data.dateExpiration?.substring(0, 10) ?? '';
          if (raw) {
            const [y, m, d] = raw.split('-');
            this.lotDateDisplay = `${d}/${m}/${y}`;
          }

          this.loadingData = false;
          this.cdr.markForCheck();

          if (data.id_Produit) {
            this.loadLots(data.id_Produit, data.numeroLot);
          }
        },
        error: () => { this.fetchError = 'Impossible de charger le stock.'; this.loadingData = false; this.cdr.markForCheck(); }
      });
    }
  }

  private loadLots(productId: number, preselectLot?: string): void {
    this.loadingLots = true;
    this.lotSvc.getLotsByProductId(productId).pipe(takeUntil(this.destroy$)).subscribe({
      next: lots => {
        this.lots = lots.filter(l => !l.isExpired);
        if (preselectLot && !this.lots.find(l => l.numero === preselectLot)) {
          const existing = lots.find(l => l.numero === preselectLot);
          if (existing) this.lots = [existing, ...this.lots];
        }
        this.loadingLots = false;
        this.form.get('numeroLot')!.enable();
        this.cdr.markForCheck();
      },
      error: () => { this.loadingLots = false; this.cdr.markForCheck(); }
    });
  }

  userName(u: any): string {
    return u?.name ?? u?.Name ?? u?.fullName ?? u?.email ?? `#${u?.id}`;
  }

  productName(p: any): string {
    return p?.Nom ?? p?.nom ?? `#${p?.Id_Produit ?? p?.id_Produit}`;
  }

  get f() { return this.form.controls; }

  lotLabel(l: LotDto): string {
    const exp = l.dateExpiration
      ? new Date(l.dateExpiration).toLocaleDateString('en-GB', { day: '2-digit', month: '2-digit', year: '2-digit' })
      : '—';
    return `${l.numero}  —  qty ${l.quantite}  ·  exp. ${exp}`;
  }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    this.saving     = true;
    this.submitError = '';
    this.successMsg  = '';

    const v = this.form.getRawValue();
    const dto: StockDelegueDto = {
      id_User_Delegue: +v.id_User_Delegue,
      id_Produit:      +v.id_Produit,
      numeroLot:       v.numeroLot,
      dateExpiration:  v.dateExpiration,
      qteDisponible:   +v.qteDisponible,
      qteReservee:     0,
      ...(this.isEdit && this.editId ? { id_stock: this.editId } : {})
    };

    this.svc.createOrUpdate(dto).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.saving     = false;
        this.successMsg = this.isEdit ? 'Stock mis à jour.' : 'Stock créé avec succès.';
        this.cdr.markForCheck();
        setTimeout(() => this.router.navigate(['/inventory/stocks']), 1200);
      },
      error: () => {
        this.submitError = 'Error saving stock.';  // BUG : en anglais
        this.saving      = false;
        this.cdr.markForCheck();
      }
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
```

---

### `Cynapharm/src/app/features/inventory/stocks/stock-form/stock-form.component.html`

```html
<div class="page">
  <div class="page-header">
    <a routerLink="/inventory/stocks" class="back-link">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M19 12H5M12 19l-7-7 7-7"/></svg>
      Retour
    </a>
    <h1>{{ isEdit ? 'Modifier le stock' : 'Nouveau stock' }}</h1>
  </div>

  <div *ngIf="loadingData" class="loading-wrap">
    <div class="spinner"></div><span>Chargement...</span>
  </div>

  <div *ngIf="fetchError" class="error-banner">{{ fetchError }}</div>

  <form *ngIf="!loadingData" [formGroup]="form" (ngSubmit)="submit()" class="form-card" novalidate>
    <div class="form-grid">

      <!-- 1. Délégué -->
      <div class="field">
        <label>Délégué *</label>
        <select formControlName="id_User_Delegue">
          <option [ngValue]="null">-- Sélectionner un délégué --</option>
          @for (u of delegues; track u.id) {
            <option [ngValue]="u.id">{{ userName(u) }}</option>
          }
        </select>
        <span *ngIf="f['id_User_Delegue'].touched && f['id_User_Delegue'].invalid" class="field-error">Délégué requis</span>
      </div>

      <!-- 2. Produit -->
      <div class="field">
        <label>Produit *</label>
        <select formControlName="id_Produit">
          <option [ngValue]="null">-- Sélectionner un produit --</option>
          @for (p of products; track p.Id_Produit) {
            <option [ngValue]="p.Id_Produit">{{ productName(p) }}</option>
          }
        </select>
        <span *ngIf="f['id_Produit'].touched && f['id_Produit'].invalid" class="field-error">Produit requis</span>
      </div>

      <!-- 3. Numéro de lot -->
      <div class="field">
        <label>Numéro de lot *</label>
        <select formControlName="numeroLot">
          <option value="">
            @if (!f['id_Produit'].value) { -- Sélectionner d'abord un produit -- }
            @else if (loadingLots) { Chargement des lots… }
            @else { -- Sélectionner un lot -- }
          </option>
          @for (lot of lots; track lot.numero) {
            <option [value]="lot.numero">
              {{ lot.numero }}
              (qté: {{ lot.quantite }} · exp: {{ lot.dateExpiration | date:'dd/MM/yyyy' }})
            </option>
          }
        </select>
        @if (f['id_Produit'].value && !loadingLots && lots.length === 0) {
          <span class="field-hint">Aucun lot disponible (non expiré) pour ce produit.</span>
        }
        <span *ngIf="f['numeroLot'].touched && f['numeroLot'].invalid" class="field-error">Lot requis</span>
      </div>

      <!-- 4. Date d'expiration (auto-filled readonly) -->
      <div class="field">
        <label>Date d'expiration</label>
        <div class="date-display" [class.empty]="!lotDateDisplay">
          {{ lotDateDisplay || '— auto-remplie depuis le lot sélectionné —' }}
        </div>
        <input type="hidden" formControlName="dateExpiration" />
      </div>

      <!-- 5. Qté disponible -->
      <div class="field">
        <label>Qté disponible *</label>
        <input type="number" formControlName="qteDisponible" min="1" placeholder="1" />
        <span *ngIf="f['qteDisponible'].touched && f['qteDisponible'].invalid" class="field-error">Valeur invalide (min 1)</span>
      </div>

    </div>

    <div *ngIf="submitError" class="error-banner" style="margin-top:16px">{{ submitError }}</div>
    <div *ngIf="successMsg"  class="success-banner">{{ successMsg }}</div>

    <div class="form-actions">
      <button type="submit" class="btn btn-primary" [disabled]="saving">
        <span *ngIf="saving" class="btn-spinner"></span>
        {{ saving ? 'Enregistrement...' : (isEdit ? 'Mettre à jour' : 'Créer') }}
      </button>
      <a routerLink="/inventory/stocks" class="btn btn-secondary">Annuler</a>
    </div>
  </form>
</div>
```

---

### `Cynapharm/src/app/features/inventory/inventory-routing.module.ts` (section stocks)

```typescript
{ path: 'stocks',
  loadComponent: () => import('./stocks/stock-list/stock-list.component').then(m => m.StockListComponent) },
{ path: 'stocks/new',
  loadComponent: () => import('./stocks/stock-form/stock-form.component').then(m => m.StockFormComponent) },
{ path: 'stocks/:id',
  loadComponent: () => import('./stocks/stock-detail/stock-detail.component').then(m => m.StockDetailComponent) },
{ path: 'stocks/:id/edit',
  loadComponent: () => import('./stocks/stock-form/stock-form.component').then(m => m.StockFormComponent) },
```

---

*Document généré le 2026-05-25*
