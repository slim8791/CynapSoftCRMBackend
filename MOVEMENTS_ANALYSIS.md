# Mouvements de Stock — Analyse complète

---

## 1. Modèle StockMovement

**Fichier:** `CynapCRM.Services.InventoryAPI/Models/StockMovement.cs`

| Champ | Type C# | JSON key | Description |
|---|---|---|---|
| `Id_Movement` | `int` (PK, [Key]) | `id_Movement` | Identifiant auto-incrémenté |
| `Id_Stock` | `int` (FK, [Required]) | `id_Stock` | FK → Stocks.Id_stock, CASCADE delete |
| `Quantite` | `int` ([Required]) | `quantite` | Quantité déplacée. Négatif pour Decrement, positif pour Increment et Transfer-Out (**incohérence, voir §7**) |
| `TypeMovement` | `string` ([Required]) | `typeMovement` | Valeurs réelles : `"Decrement"`, `"Increment"`, `"Transfer-Out"`, `"Transfer-In"` |
| `DateMovement` | `DateTime` | `dateMovement` | Date UTC, default `GETUTCDATE()` via EF |
| `Description` | `string?` (nullable) | `description` | Texte libre. Rempli uniquement pour Transfer-Out/In |

**Table SQL:** `Stock_Movements`
**Index:** `Id_Stock` (index non-unique pour performance)
**Relation:** `HasOne<Stock_Delegue>().WithMany().HasForeignKey(m => m.Id_Stock).OnDelete(DeleteBehavior.Cascade)`

---

## 2. Endpoints backend

**Controller:** `CynapCRM.Services.InventoryAPI/Controllers/StockMovementController.cs`
**Route base:** `api/stock-movements`
**Auth globale:** `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` au niveau controller

| Method | Route | Roles | Params | Description | Status |
|---|---|---|---|---|---|
| POST | `/api/stock-movements/decrement` | ADMIN, SUPERVISEUR | `?idStock=int&qte=int` (query) | Décrémente QteDisponible, crée un StockMovement TypeMovement="Decrement", Quantite=-qte | ⚠️ StatusCode(515) dans catch |
| POST | `/api/stock-movements/increment` | ADMIN, SUPERVISEUR | `?idStock=int&qte=int` (query) | Incrémente QteDisponible, crée StockMovement TypeMovement="Increment", Quantite=+qte | ⚠️ StatusCode(515) dans catch |
| POST | `/api/stock-movements/transfer` | ADMIN, SUPERVISEUR | `?idStockSource=int&idStockDestination=int&qte=int` (query) | Décrémente source, incrémente destination, crée 2 StockMovements dans une transaction | ⚠️ StatusCode(515) dans catch |
| GET | `/api/stock-movements/{idStock:int}` | ADMIN, SUPERVISEUR | `idStock` route param | Retourne tous les mouvements d'un stock, triés par date DESC | ⚠️ StatusCode(515) dans catch |
| GET | `/api/stock-movements/by-delegue/{idDelegue:int}` | ADMIN, SUPERVISEUR | `idDelegue` route param | Charge les Id_stock du délégué, puis retourne tous les mouvements sur ces stocks | ⚠️ StatusCode(515) dans catch |

**Note critique :** Aucun endpoint `GET /api/stock-movements` (liste globale) n'existe.

---

## 3. Ocelot routes

**Fichier:** `CynapCRM.Gateway/ocelot.json` (ligne 741-752)

| Upstream | Downstream | Methods | Auth | Status |
|---|---|---|---|---|
| `/inventory/stock-movements/{everything}` | `/api/stock-movements/{everything}` (host: cynapharminventories.runasp.net:80) | GET, POST, PUT, DELETE | Bearer JWT | ✅ Correct — GET et POST sont autorisés |

La route Ocelot est correcte et complète. Aucun problème côté gateway pour les mouvements.

---

## 4. Service layer

**Fichier:** `CynapCRM.Services.InventoryAPI/Service/StockMovementService.cs`

### 4.1 `GetMovementHistoryByDelegueAsync(int idDelegue)`
- **Paramètres :** `idDelegue` (int)
- **Ce qu'il fait :** Récupère d'abord tous les `Id_stock` des stocks du délégué (non supprimés), puis retourne tous les `StockMovements` dont `Id_Stock` est dans cette liste.
- **Query EF :**
  ```csharp
  var stockIds = await _db.StocksDelegues
      .Where(s => s.Id_User_Delegue == idDelegue && !s.IsDeleted)
      .Select(s => s.Id_stock).ToListAsync();
  return await _db.StockMovements.AsNoTracking()
      .Where(m => stockIds.Contains(m.Id_Stock))
      .OrderByDescending(m => m.DateMovement)
      .Select(m => new StockMovementDto { ... }).ToListAsync();
  ```
- **Retourne :** `IEnumerable<StockMovementDto>` — tous les mouvements de tous les stocks du délégué, triés par date DESC.
- **⚠️ Risque :** Pas de pagination. Si un délégué a beaucoup de stocks anciens, retourne tout en mémoire.

### 4.2 `DecrementStockAsync(int idStock, int qte)`
- **Paramètres :** `idStock`, `qte` (int)
- **Ce qu'il fait :** Vérifie qte > 0 et stock existant et suffisant. Décrémente `QteDisponible`, crée un `StockMovement` avec Quantite=-qte, TypeMovement="Decrement". Pas de Description.
- **Query EF :**
  ```csharp
  var stock = await _db.StocksDelegues
      .FirstOrDefaultAsync(s => s.Id_stock == idStock && !s.IsDeleted);
  stock.QteDisponible -= qte;
  _db.StockMovements.Add(new StockMovement { Id_Stock = idStock, Quantite = -qte, TypeMovement = "Decrement", DateMovement = DateTime.UtcNow });
  await _db.SaveChangesAsync();
  ```
- **Retourne :** `bool` — false si insuffisant ou inexistant, true si succès.

### 4.3 `IncrementStockAsync(int idStock, int qte)`
- **Paramètres :** `idStock`, `qte` (int)
- **Ce qu'il fait :** Vérifie qte > 0 et stock existant. Incrémente `QteDisponible`, crée `StockMovement` avec Quantite=+qte, TypeMovement="Increment". Pas de Description.
- **Retourne :** `bool`

### 4.4 `TransferStockAsync(int idStockSource, int idStockDestination, int qte)`
- **Paramètres :** trois int
- **Ce qu'il fait :** Charge les deux stocks, vérifie source suffisante. Dans une **transaction DB** : décrémente source, incrémente destination, crée deux `StockMovements` ("Transfer-Out" sur source avec description "Transfert vers stock {dest}", "Transfer-In" sur destination avec "Transfert depuis stock {source}"). Rollback automatique si SaveChanges échoue.
- **Retourne :** `bool`

### 4.5 `GetStockMovementsAsync(int idStock)`
- **Paramètres :** `idStock` (int)
- **Ce qu'il fait :** Retourne tous les mouvements d'un stock donné, triés par date DESC.
- **Query EF :**
  ```csharp
  return await _db.StockMovements.AsNoTracking()
      .Where(m => m.Id_Stock == idStock)
      .OrderByDescending(m => m.DateMovement)
      .Select(m => new StockMovementDto { ... }).ToListAsync();
  ```
- **Retourne :** `IEnumerable<StockMovementDto>`

---

## 5. Angular — État actuel

### 5.1 Routes

**Fichier:** `Cynapharm/src/app/features/inventory/inventory-routing.module.ts`

| Path | Component | Paramètres |
|---|---|---|
| `inventory/movements` | `MovementListComponent` | `?idStock=int` (optionnel, query param) |

Pas de route `movements/:id` (detail). Pas de route `movements/new` (formulaire manuel).

### 5.2 movement-list component

**Fichier:** `movement-list.component.ts` + `.html`

**Ce qui est affiché :**
- Barre de filtres : ID Stock (input number), Date début, Date fin, Type mouvement (select), boutons Filtrer / Effacer
- Tableau : colonnes ID, ID Stock, Quantité (badge coloré), Type (chip coloré), Date, Description

**API appelée :**
- Si `?idStock=N` en query param → `GET /inventory/stock-movements/{N}` via `svc.getMovements(N)`
- Sinon → `GET /inventory/stock-movements/by-delegue/{userId}` via `svc.getMovementsByDelegue(userId)` au `ngOnInit`

**Comment les filtres fonctionnent :**
- Filtre ID Stock : déclenche un nouvel appel API (côté serveur) via `applyFilter()`
- Filtres Date début/fin et Type : filtrés côté client dans `applyClientFilters()` sur `allMovements`

**Ce qui fonctionne ✅ :**
- Affichage du tableau quand `movements.length > 0`
- Filtres date côté client (logique correcte)
- Loading / error banners
- Hint panel quand aucune recherche n'a été faite
- Chargement auto par délégué au ngOnInit (si pas de ?idStock)

**Ce qui est cassé ❌ :**
- **Filtre type "Transfer" ne matche jamais** : le select a `value="Transfer"` mais les TypeMovement en DB sont `"Transfer-Out"` et `"Transfer-In"`. Exact match → zéro résultat.
- **Chips Transfer sans couleur** : `[class.chip-other]="m.typeMovement?.toLowerCase() === 'transfer'"` — ne match jamais "transfer-out" ni "transfer-in". Ces lignes n'ont aucune classe de couleur.
- **Badge quantité Transfer-Out toujours neutre** : `[class.negative]="m.typeMovement?.toLowerCase() === 'decrement'"` — "transfer-out" (qui est bien une sortie) n'est pas coloré en rouge.
- **Mode délégué : empty state incorrect** : Quand chargé par délégué (activeStockId=null), si `movements.length === 0` après filtre, affiche le hint panel ("Entrez un ID") au lieu d'un empty state "Aucun mouvement trouvé".
- **DELEGUE reçoit 403** : le controller `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` bloque les utilisateurs DELEGUE. Pourtant le component appelle `getMovementsByDelegue(userId)` pour les DELEGUEs.

**Ce qui manque ⚠️ :**
- Pas de colonne "Numéro de lot" ou "Produit" (seulement ID Stock brut)
- Pas de pagination
- Pas de tri cliquable sur les colonnes
- Pas de bouton "Nouveau mouvement" (formulaire manual decrement/increment)
- Pas de route movements/new ni movements/:id

### 5.3 stock-movement.service.ts

**Fichier:** `Cynapharm/src/app/features/inventory/movements/services/stock-movement.service.ts`

| Method | HTTP | URL | Retourne |
|---|---|---|---|
| `getMovements(idStock)` | GET | `/inventory/stock-movements/{idStock}` | `Observable<StockMovementDto[]>` |
| `getMovementsByDelegue(idDelegue)` | GET | `/inventory/stock-movements/by-delegue/{idDelegue}` | `Observable<StockMovementDto[]>` |
| `decrement(idStock, qte)` | POST | `/inventory/stock-movements/decrement?idStock=&qte=` | `Observable<boolean>` |
| `increment(idStock, qte)` | POST | `/inventory/stock-movements/increment?idStock=&qte=` | `Observable<boolean>` |
| `transfer(idSource, idDest, qte)` | POST | `/inventory/stock-movements/transfer?idStockSource=&idStockDestination=&qte=` | `Observable<boolean>` |

---

## 6. Business Logic Analysis

### Quand un mouvement est-il créé ?

| Opération | Mouvement créé ? | TypeMovement | Auteur |
|---|---|---|---|
| `POST /decrement` (manuel, admin) | ✅ Oui | `"Decrement"` | StockMovementService |
| `POST /increment` (manuel, admin) | ✅ Oui | `"Increment"` | StockMovementService |
| `POST /transfer` (admin) | ✅ Oui (×2) | `"Transfer-Out"` + `"Transfer-In"` | StockMovementService |
| Création d'une distribution (Echantillon) | ❌ **NON** | — | DistributionService ne fait pas appel à StockMovementService |
| Suppression d'une distribution | ❌ **NON** | — | Non tracé |
| Création d'un stock promo (Echantillon/Gratuite) | ❌ **NON** | — | StockPromotionnelService ne trace pas |
| Mise à jour d'un stock (PUT /api/stock/{id}) | ❌ **NON** | — | Non tracé |
| Commande client (OrderAPI) | ❌ **NON** | — | Service externe, pas de hook |

### Types de mouvements existants

| TypeMovement | Qui le crée | Quantite | Description |
|---|---|---|---|
| `"Decrement"` | DecrementStockAsync | négatif (-qte) | null |
| `"Increment"` | IncrementStockAsync | positif (+qte) | null |
| `"Transfer-Out"` | TransferStockAsync | positif (+qte) ⚠️ incohérent | "Transfert vers stock {dest}" |
| `"Transfer-In"` | TransferStockAsync | positif (+qte) | "Transfert depuis stock {source}" |

### Ce qui est tracé vs non tracé

**Tracé ✅ :** Opérations manuelles admin via les 3 endpoints (decrement, increment, transfer)

**Non tracé ❌ :**
- Distributions d'échantillons aux médecins/pharmaciens
- Création de stocks promotionnels (qui alloue une quantité)
- Mises à jour directes de stock (PUT sur StockDelegueController)
- Retours de stock
- Expirations

---

## 7. Bugs Found

| # | Fichier | Issue | Impact | Priorité |
|---|---|---|---|---|
| 1 | `StockMovementController.cs` (lignes 57, 89, 131, 153) | `StatusCode(515, _response)` — code HTTP invalide (515 n'existe pas) | Les clients reçoivent une réponse non standard ; certains parseurs HTTP rejettent 515 | P1 |
| 2 | `movement-list.component.html` (ligne 24) | `<option value="Transfer">` mais TypeMovement réels = "Transfer-Out" / "Transfer-In". Match exact → filtre toujours vide | Filtre "Transfer" complètement inopérant | P1 |
| 3 | `movement-list.component.html` (ligne 72) | `[class.chip-other]="m.typeMovement?.toLowerCase() === 'transfer'"` — ne match jamais "transfer-out" ni "transfer-in" | Toutes les lignes Transfer s'affichent sans couleur (texte brut noir) | P2 |
| 4 | `movement-list.component.html` (ligne 65) | `[class.negative]` uniquement pour "decrement" — "transfer-out" (sortie de stock) n'est pas coloré en rouge | Transfer-Out visuellement indiscernable de Transfer-In | P2 |
| 5 | `StockMovementController.cs` (ligne 17) | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` au niveau controller — bloque DELEGUE | `getMovementsByDelegue` appelé pour les DELEGUEs → 403 à chaque chargement | P1 |
| 6 | `StockMovementService.cs` (ligne 119) | `TransferStockAsync` crée Transfer-Out avec `Quantite = qte` (positif) au lieu de `-qte` | La quantité affichée pour une sortie est positive → lecture ambiguë | P2 |
| 7 | `movement-list.component.ts` (ligne 43-44) | Mode délégué : empty state avec condition `activeStockId` → affiche le hint panel au lieu d'empty state si 0 résultats | UX incohérente pour les délégués sans mouvements | P3 |
| 8 | `DistributionService.cs` (non lu ici mais connu) | Distributions ne créent pas de StockMovement | Toute activité de distribution invisible dans l'historique | P1 |

---

## 8. Missing Features

| Feature | Où | Impact |
|---|---|---|
| Pagination des mouvements (backend + frontend) | `GetStockMovementsAsync`, `GetMovementHistoryByDelegueAsync`, movement-list | Perf : peut retourner des milliers de lignes |
| Endpoint `GET /api/stock-movements` (liste globale paginée) | Controller + IService + Service | Admin ne peut pas voir tous les mouvements sans connaître un idStock |
| TypeMovement "Distribution" tracé depuis DistributionService | DistributionService | Traçabilité complète des échantillons |
| Colonne "Numéro de lot" dans le tableau | movement-list HTML + StockMovementDto + backend JOIN | L'ID stock brut est illisible pour l'utilisateur |
| Formulaire manuel decrement/increment | Nouvelle route movements/new, nouveau composant | Aucun moyen UI de créer un mouvement sans passer par le formulaire stock |
| Export CSV des mouvements | movement-list | Fonctionnalité audit standard |
| Tri cliquable sur colonnes | movement-list | UX |

---

## 9. Fix Plan

### Bug #1 — StatusCode(515) → StatusCode(500)

**Fichier:** `CynapCRM.Services.InventoryAPI/Controllers/StockMovementController.cs`

Remplacer les 4 occurrences de `StatusCode(515, _response)` par `StatusCode(500, _response)`.

```csharp
// Ligne 57, 89, 131, 153 — remplacer :
return StatusCode(515, _response);
// par :
return StatusCode(500, _response);
```

---

### Bug #2 — Filtre "Transfer" dans le select

**Fichier:** `Cynapharm/src/app/features/inventory/movements/movement-list/movement-list.component.html`

```html
<!-- Remplacer : -->
<option value="Transfer">Transfer</option>

<!-- Par : -->
<option value="Transfer-Out">Transfer-Out</option>
<option value="Transfer-In">Transfer-In</option>
```

Et dans `applyClientFilters()` (.ts ligne 101), le match est déjà exact (`=== this.typeMovement.toLowerCase()`) donc il fonctionnera correctement une fois les options corrigées.

---

### Bug #3 — Chips sans couleur pour Transfer

**Fichier:** `movement-list.component.html`

```html
<!-- Remplacer : -->
[class.chip-other]="m.typeMovement?.toLowerCase() === 'transfer'"

<!-- Par : -->
[class.chip-out]="m.typeMovement?.toLowerCase() === 'transfer-out'"
[class.chip-in]="m.typeMovement?.toLowerCase() === 'transfer-in'"
```

---

### Bug #4 — Badge quantité Transfer-Out

**Fichier:** `movement-list.component.html`

```html
<!-- Remplacer la condition du badge quantité : -->
[class.positive]="m.typeMovement?.toLowerCase() === 'increment'"
[class.negative]="m.typeMovement?.toLowerCase() === 'decrement'"

<!-- Par : -->
[class.positive]="m.typeMovement?.toLowerCase() === 'increment' || m.typeMovement?.toLowerCase() === 'transfer-in'"
[class.negative]="m.typeMovement?.toLowerCase() === 'decrement' || m.typeMovement?.toLowerCase() === 'transfer-out'"
```

---

### Bug #5 — DELEGUE bloqué (403)

**Fichier:** `CynapCRM.Services.InventoryAPI/Controllers/StockMovementController.cs`

Retirer l'`Authorize` global du controller et appliquer par méthode :

```csharp
// Supprimer au niveau controller :
[Authorize(Roles = "ADMIN,SUPERVISEUR")]

// Ajouter sur chaque action :
[HttpPost("decrement")]
[Authorize(Roles = "ADMIN,SUPERVISEUR")]
public async Task<IActionResult> DecrementStock(...) { ... }

[HttpPost("increment")]
[Authorize(Roles = "ADMIN,SUPERVISEUR")]
public async Task<IActionResult> IncrementStock(...) { ... }

[HttpPost("transfer")]
[Authorize(Roles = "ADMIN,SUPERVISEUR")]
public async Task<IActionResult> TransferStock(...) { ... }

[HttpGet("{idStock:int}")]
[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
public async Task<IActionResult> GetStockMovements(int idStock) { ... }

[HttpGet("by-delegue/{idDelegue:int}")]
[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
public async Task<IActionResult> GetMovementsByDelegue(int idDelegue) { ... }
```

---

### Bug #6 — Quantite Transfer-Out positif

**Fichier:** `CynapCRM.Services.InventoryAPI/Service/StockMovementService.cs`

```csharp
// Ligne 117-123, remplacer :
_db.StockMovements.Add(new StockMovement
{
    Id_Stock = idStockSource,
    TypeMovement = "Transfer-Out",
    Quantite = qte,   // <-- positif, incohérent
    ...
});

// Par :
_db.StockMovements.Add(new StockMovement
{
    Id_Stock = idStockSource,
    TypeMovement = "Transfer-Out",
    Quantite = -qte,  // négatif = sortie de stock
    ...
});
```

---

### Bug #7 — Empty state mode délégué

**Fichier:** `movement-list.component.html`

```html
<!-- Remplacer la condition empty state : -->
<div *ngIf="!loading && !error && movements.length === 0 && activeStockId">
  <app-empty-state ...></app-empty-state>
</div>

<!-- Par (ajouter aussi allMovements.length > 0 pour distinguer "pas de filtre" de "chargé mais vide") : -->
<div *ngIf="!loading && !error && movements.length === 0 && (activeStockId || allMovements.length > 0)">
  <app-empty-state title="Aucun mouvement" message="Aucun mouvement trouvé pour ce stock."></app-empty-state>
</div>
```

---

## 10. Complete Code of Every File Read

---

### `CynapCRM.Services.InventoryAPI/Models/StockMovement.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.InventoryAPI.Models
{
    public class StockMovement
    {

        [Key]
        public int Id_Movement { get; set; }

        [Required]
        public int Id_Stock { get; set; }

        [Required]
        public int Quantite { get; set; }


        [Required]
        public string TypeMovement { get; set; } = string.Empty;

        public DateTime DateMovement { get; set; } = DateTime.UtcNow;

        public string? Description { get; set; }

    }
}
```

---

### `CynapCRM.Services.InventoryAPI/Models/Dto/StockMovementDto.cs`

```csharp
namespace CynapCRM.Services.InventoryAPI.Models.Dto
{
    public class StockMovementDto
    {

        public int Id_Movement { get; set; }
        public int Id_Stock { get; set; }
        public int Quantite { get; set; }
        public string TypeMovement { get; set; } = string.Empty;
        public DateTime DateMovement { get; set; }
        public string? Description { get; set; }

    }
}
```

---

### `CynapCRM.Services.InventoryAPI/Service/IService/IStockMovementService.cs`

```csharp
using CynapCRM.Services.InventoryAPI.Models.Dto;

namespace CynapCRM.Services.InventoryAPI.Service.IService
{
    public interface IStockMovementService
    {
        Task<IEnumerable<StockMovementDto>> GetMovementHistoryByDelegueAsync(
    int idDelegue);
        //  movement stocks
        Task<bool> DecrementStockAsync(int idStock, int qte);
        Task<bool> IncrementStockAsync(int idStock, int qte);
        // Historique des mouvements 
        Task<IEnumerable<StockMovementDto>> GetStockMovementsAsync(int idStock);
        Task<bool> TransferStockAsync(int idStockSource, int idStockDestination, int qte);
    }
}
```

---

### `CynapCRM.Services.InventoryAPI/Service/StockMovementService.cs`

```csharp
using AutoMapper;
using CynapCRM.Services.InventoryAPI.Data;
using CynapCRM.Services.InventoryAPI.Models;
using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.InventoryAPI.Service
{
    public class StockMovementService : IStockMovementService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public StockMovementService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<IEnumerable<StockMovementDto>> GetMovementHistoryByDelegueAsync(
    int idDelegue)
        {
            var stockIds = await _db.StocksDelegues
                .Where(s => s.Id_User_Delegue == idDelegue && !s.IsDeleted)
                .Select(s => s.Id_stock)
                .ToListAsync();

            return await _db.StockMovements
                .AsNoTracking()
                .Where(m => stockIds.Contains(m.Id_Stock))
                .OrderByDescending(m => m.DateMovement)
                .Select(m => new StockMovementDto
                {
                    Id_Movement = m.Id_Movement,
                    Id_Stock = m.Id_Stock,
                    Quantite = m.Quantite,
                    TypeMovement = m.TypeMovement,
                    DateMovement = m.DateMovement,
                    Description = m.Description
                })
                .ToListAsync();
        }
        public async Task<bool> DecrementStockAsync(int idStock, int qte)
        {
            if (qte <= 0)
            {
                return false;
            }

            var stock = await _db.StocksDelegues
                .FirstOrDefaultAsync(s => s.Id_stock == idStock && !s.IsDeleted);
            if (stock == null || stock.QteDisponible < qte)
            {
                return false;
            }
            stock.QteDisponible -= qte;
            _db.StockMovements.Add(new StockMovement
            {
                Id_Stock = idStock,
                Quantite = -qte,
                DateMovement = DateTime.UtcNow,
                TypeMovement = "Decrement"
            });

            await _db.SaveChangesAsync();
            return true;

        }
        public async Task<bool> IncrementStockAsync(int idStock, int qte)
        {
            if (qte <= 0)
            {
                return false;
            }

            var stock = await _db.StocksDelegues
                            .FirstOrDefaultAsync(s => s.Id_stock == idStock && !s.IsDeleted);
            if (stock == null)
            {
                return false;
            }
            stock.QteDisponible += qte;
            _db.StockMovements.Add(new StockMovement
            {
                Id_Stock = idStock,
                Quantite = qte,
                DateMovement = DateTime.UtcNow,
                TypeMovement = "Increment"
            });
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> TransferStockAsync(int idStockSource, int idStockDestination, int qte)
        {
            if (qte <= 0) 
            {
                return false;
            }

            var source = await _db.StocksDelegues
                            .FirstOrDefaultAsync(s => s.Id_stock == idStockSource && !s.IsDeleted);

            var destination = await _db.StocksDelegues
                .FirstOrDefaultAsync(s => s.Id_stock == idStockDestination && !s.IsDeleted);

            if (source == null || destination == null || source.QteDisponible < qte)
            {
                return false;
            }
            
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                source.QteDisponible -= qte;
                destination.QteDisponible += qte;

                _db.StockMovements.Add(new StockMovement
                {
                    Id_Stock = idStockSource,
                    TypeMovement = "Transfer-Out",
                    Quantite = qte,
                    DateMovement = DateTime.UtcNow,
                    Description = $"Transfert vers stock {idStockDestination}"
                });

                _db.StockMovements.Add(new StockMovement
                {
                    Id_Stock = idStockDestination,
                    TypeMovement = "Transfer-In",
                    Quantite = qte,
                    DateMovement = DateTime.UtcNow,
                    Description = $"Transfert depuis stock {idStockSource}"
                });

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
        public async Task<IEnumerable<StockMovementDto>> GetStockMovementsAsync(int idStock)
        {

            return await _db.StockMovements
                            .AsNoTracking()
                            .Where(m => m.Id_Stock == idStock)
                            .OrderByDescending(m => m.DateMovement)
                            .Select(m => new StockMovementDto
                            {
                                Id_Movement = m.Id_Movement,
                                Id_Stock = m.Id_Stock,
                                Quantite = m.Quantite,
                                TypeMovement = m.TypeMovement,
                                DateMovement = m.DateMovement,
                                Description = m.Description
                            })
                            .ToListAsync();
        }
    }
}
```

---

### `CynapCRM.Services.InventoryAPI/Controllers/StockMovementController.cs`

```csharp
using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.InventoryAPI.Controllers
{



    // ═══════════════════════════════════════
    // StockMovementController.cs
    // ═══════════════════════════════════════

    [Route("api/stock-movements")]
    [ApiController]
    [Authorize(Roles = "ADMIN,SUPERVISEUR")] // correct — niveau controller
    public class StockMovementController : ControllerBase
    {
        private readonly IStockMovementService _stockMovementService;
        protected ResponseDto _response;

        public StockMovementController(IStockMovementService stockMovementService)
        {
            _stockMovementService = stockMovementService;
            _response = new ResponseDto();
        }

        [HttpPost("decrement")]
        public async Task<IActionResult> DecrementStock(
            [FromQuery] int idStock,
            [FromQuery] int qte)
        {
            try
            {
                // FIX: validation idStock manquante
                if (idStock <= 0 || qte <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "IdStock et Qte doivent être supérieurs à zéro.";
                    return BadRequest(_response);
                }
                bool result = await _stockMovementService.DecrementStockAsync(idStock, qte);
                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Stock insuffisant ou inexistant.";
                    return BadRequest(_response);
                }
                _response.Message = "Stock décrémenté et mouvement enregistré.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpPost("increment")]
        public async Task<IActionResult> IncrementStock(
            [FromQuery] int idStock,
            [FromQuery] int qte)
        {
            try
            {
                // FIX: validation idStock manquante
                if (idStock <= 0 || qte <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "IdStock et Qte doivent être supérieurs à zéro.";
                    return BadRequest(_response);
                }
                bool result = await _stockMovementService.IncrementStockAsync(idStock, qte);
                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Stock introuvable.";
                    return BadRequest(_response);
                }
                _response.Message = "Stock incrémenté et mouvement enregistré.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> TransferStock(
            [FromQuery] int idStockSource,
            [FromQuery] int idStockDestination,
            [FromQuery] int qte)
        {
            try
            {
                // FIX: validation ids manquante
                if (idStockSource <= 0 || idStockDestination <= 0 || qte <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Paramètres de transfert invalides.";
                    return BadRequest(_response);
                }
                // FIX: vérifier source ≠ destination
                if (idStockSource == idStockDestination)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Source et destination ne peuvent pas être identiques.";
                    return BadRequest(_response);
                }
                bool result = await _stockMovementService.TransferStockAsync(
                    idStockSource, idStockDestination, qte);
                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Transfert impossible : vérifiez les stocks.";
                    return BadRequest(_response);
                }
                _response.Message = "Transfert effectué et mouvements tracés.";
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
        public async Task<IActionResult> GetStockMovements(int idStock)
        {
            try
            {
                if (idStock <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id stock invalide.";
                    return BadRequest(_response);
                }
                var result = await _stockMovementService.GetStockMovementsAsync(idStock);
                _response.Result = result; // FIX: résultat non assigné dans l'original
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        // FIX: endpoint manquant — historique par délégué
        [HttpGet("by-delegue/{idDelegue:int}")]
        public async Task<IActionResult> GetMovementsByDelegue(int idDelegue)
        {
            try
            {
                if (idDelegue <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id délégué invalide.";
                    return BadRequest(_response);
                }
                var result = await _stockMovementService
                    .GetMovementHistoryByDelegueAsync(idDelegue);
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
    }
}
```

---

### `CynapCRM.Services.InventoryAPI/Data/AppDbContext.cs` (section mouvements)

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

### `Cynapharm/src/app/features/inventory/movements/services/stock-movement.service.ts`

```typescript
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../../core/services/api.service';

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
  private readonly base = '/inventory/stock-movements';
  constructor(private api: ApiService) {}
  private u<T>(r: any): T { return r?.Result ?? r?.result ?? r; }

  getMovements(idStock: number): Observable<StockMovementDto[]> {
    return this.api.get<any>(`${this.base}/${idStock}`).pipe(map(r => this.u<StockMovementDto[]>(r) ?? []));
  }
  getMovementsByDelegue(idDelegue: number): Observable<StockMovementDto[]> {
    return this.api.get<any>(`${this.base}/by-delegue/${idDelegue}`).pipe(map(r => this.u<StockMovementDto[]>(r) ?? []));
  }
  decrement(idStock: number, qte: number): Observable<boolean> {
    return this.api.post<any>(`${this.base}/decrement?idStock=${idStock}&qte=${qte}`, {}).pipe(map(r => this.u<boolean>(r)));
  }
  increment(idStock: number, qte: number): Observable<boolean> {
    return this.api.post<any>(`${this.base}/increment?idStock=${idStock}&qte=${qte}`, {}).pipe(map(r => this.u<boolean>(r)));
  }
  transfer(idSource: number, idDest: number, qte: number): Observable<boolean> {
    return this.api.post<any>(`${this.base}/transfer?idStockSource=${idSource}&idStockDestination=${idDest}&qte=${qte}`, {}).pipe(map(r => this.u<boolean>(r)));
  }
}
```

---

### `Cynapharm/src/app/features/inventory/movements/movement-list/movement-list.component.ts`

```typescript
import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { StockMovementService, StockMovementDto } from '../../movements/services/stock-movement.service';
import { AuthService } from '../../../../core/services/auth.service';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-movement-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, EmptyStateComponent],
  templateUrl: './movement-list.component.html',
  styleUrls: ['./movement-list.component.css']
})
export class MovementListComponent implements OnInit, OnDestroy {
  allMovements: StockMovementDto[] = [];
  movements: StockMovementDto[] = [];
  loading = false;
  error = '';
  filterStockId: number | null = null;
  activeStockId: number | null = null;
  dateDebut = '';
  dateFin = '';
  typeMovement = '';

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private svc:   StockMovementService,
    private auth:  AuthService,
    private cdr:   ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const idStock = this.route.snapshot.queryParamMap.get('idStock');
    if (idStock) {
      this.filterStockId = Number(idStock);
      this.applyFilter();
    } else {
      const userId = this.auth.getCurrentUser()?.id;
      if (userId) {
        this.loading = true;
        this.svc.getMovementsByDelegue(userId).pipe(takeUntil(this.destroy$)).subscribe({
          next: data => {
            this.allMovements = data;
            this.applyClientFilters();
            this.loading = false;
            this.cdr.markForCheck();
          },
          error: () => { this.loading = false; this.cdr.markForCheck(); }
        });
      }
    }
  }

  applyFilter(): void {
    if (!this.filterStockId) return;
    this.activeStockId = this.filterStockId;
    this.loading = true;
    this.error = '';
    this.svc.getMovements(this.activeStockId).pipe(takeUntil(this.destroy$)).subscribe({
      next: data => {
        this.allMovements = data;
        this.applyClientFilters();
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => { this.error = 'Impossible de charger les mouvements.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  clearFilter(): void {
    this.filterStockId = null;
    this.activeStockId = null;
    this.dateDebut = '';
    this.dateFin = '';
    this.typeMovement = '';
    this.allMovements = [];
    this.movements = [];
    this.error = '';
  }

  applyClientFilters(): void {
    let result = this.allMovements;

    if (this.dateDebut) {
      const start = new Date(`${this.dateDebut}T00:00:00`);
      result = result.filter(m => m.dateMovement && new Date(m.dateMovement) >= start);
    }

    if (this.dateFin) {
      const end = new Date(`${this.dateFin}T23:59:59`);
      result = result.filter(m => m.dateMovement && new Date(m.dateMovement) <= end);
    }

    if (this.typeMovement) {
      result = result.filter(m => m.typeMovement?.toLowerCase() === this.typeMovement.toLowerCase());
    }

    this.movements = result;
    this.cdr.markForCheck();
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
```

---

### `Cynapharm/src/app/features/inventory/movements/movement-list/movement-list.component.html`

```html
<div class="page">
      <div class="page-header">
        <h1>Mouvements de stock</h1>
        <p class="subtitle">Historique des mouvements</p>
      </div>

      <div class="filter-bar">
        <div class="search-field">
          <label>ID Stock</label>
          <input type="number" [(ngModel)]="filterStockId" placeholder="Ex: 12" (keyup.enter)="applyFilter()" />
        </div>
        <div class="search-field">
          <label>Date début</label>
          <input type="date" [(ngModel)]="dateDebut" (ngModelChange)="applyClientFilters()" />
        </div>
        <div class="search-field">
          <label>Date fin</label>
          <input type="date" [(ngModel)]="dateFin" (ngModelChange)="applyClientFilters()" />
        </div>
        <div class="search-field">
          <label>Type mouvement</label>
          <select [(ngModel)]="typeMovement" (ngModelChange)="applyClientFilters()">
            <option value="">Tous</option>
            <option value="Increment">Increment</option>
            <option value="Decrement">Decrement</option>
            <option value="Transfer">Transfer</option>
          </select>
        </div>
        <button class="btn btn-primary" (click)="applyFilter()" [disabled]="!filterStockId">Filtrer</button>
        <button class="btn btn-secondary" (click)="clearFilter()" *ngIf="activeStockId">Effacer</button>
      </div>

      <div *ngIf="loading" class="loading-wrap">
        <div class="spinner"></div><span>Chargement...</span>
      </div>

      <div *ngIf="error && !loading" class="error-banner">{{ error }}</div>

      <div *ngIf="!loading && !error && movements.length === 0 && activeStockId">
        <app-empty-state title="Aucun mouvement" message="Aucun mouvement trouvé pour ce stock."></app-empty-state>
      </div>

      <div *ngIf="!loading && !error && !activeStockId && movements.length === 0" class="hint-panel">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
        <p>Entrez un ID de stock et cliquez sur <strong>Filtrer</strong> pour voir les mouvements.</p>
      </div>

      <div *ngIf="!loading && movements.length > 0" class="table-wrap">
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>ID Stock</th>
              <th>Quantité</th>
              <th>Type</th>
              <th>Date</th>
              <th>Description</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let m of movements">
              <td class="mono">{{ m.id_Movement }}</td>
              <td>{{ m.id_Stock }}</td>
              <td>
                <span class="qty-badge" [class.positive]="m.typeMovement?.toLowerCase() === 'increment'"
                      [class.negative]="m.typeMovement?.toLowerCase() === 'decrement'">
                  {{ m.quantite }}
                </span>
              </td>
              <td>
                <span class="type-chip" [class.chip-in]="m.typeMovement?.toLowerCase() === 'increment'"
                      [class.chip-out]="m.typeMovement?.toLowerCase() === 'decrement'"
                      [class.chip-other]="m.typeMovement?.toLowerCase() === 'transfer'">
                  {{ m.typeMovement }}
                </span>
              </td>
              <td>{{ m.dateMovement | date:'dd/MM/yyyy HH:mm' }}</td>
              <td class="desc">{{ m.description ?? '—' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
```

---

### `Cynapharm/src/app/features/inventory/movements/movement-list/movement-list.component.css`

```css
/* ── Movement List (Inventaire) — Premium Enterprise CRM ── */
.page {
  max-width: 1060px;
  margin: 0 auto;
  padding: 32px 40px;
  font-family: 'Inter', system-ui, sans-serif;
  color: #1e293b;
}

/* ── Header ─────────────────────────────────────────── */
.page-header { margin-bottom: 32px; }

h1 {
  font-size: 28px;
  font-weight: 800;
  color: #0f172a;
  margin: 0 0 6px;
  letter-spacing: -0.5px;
}

.subtitle {
  font-size: 14px;
  font-weight: 500;
  color: #64748b;
  margin: 0;
}

/* ── Filter bar ─────────────────────────────────────── */
.filter-bar {
  display: flex;
  align-items: flex-end;
  gap: 16px;
  margin-bottom: 24px;
  flex-wrap: wrap;
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 16px;
  padding: 16px 20px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.02);
}

.search-field {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.search-field label {
  font-size: 12px;
  font-weight: 700;
  color: #64748b;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.search-field input,
.search-field select {
  padding: 11px 14px;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  font-size: 14px;
  width: 200px;
  font-family: inherit;
  background: #f8fafc;
  color: #0f172a;
  transition: all 0.2s;
  outline: none;
}

.search-field input:focus,
.search-field select:focus {
  border-color: #00b4d8;
  background: #ffffff;
  box-shadow: 0 0 0 3px rgba(0,180,216,0.1);
}

/* ── Buttons ─────────────────────────────────────────── */
.btn {
  padding: 11px 20px;
  border-radius: 12px;
  font-size: 14px;
  font-weight: 600;
  border: none;
  cursor: pointer;
  transition: all 0.2s;
  font-family: inherit;
  display: inline-flex;
  align-items: center;
  gap: 8px;
}

.btn-primary {
  background: linear-gradient(135deg, #00b4d8, #0077b6);
  color: #ffffff;
  box-shadow: 0 4px 12px rgba(0,119,182,0.25);
}

.btn-primary:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(0,119,182,0.35);
}

.btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }

.btn-secondary {
  background: #ffffff;
  border: 1px solid #e2e8f0;
  color: #475569;
  box-shadow: 0 1px 3px rgba(0,0,0,0.03);
}

.btn-secondary:hover { background: #f8fafc; border-color: #cbd5e1; }

/* ── Loading ─────────────────────────────────────────── */
.loading-wrap {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 80px;
  justify-content: center;
  color: #64748b;
  font-size: 14px;
  font-weight: 500;
}

.spinner {
  width: 24px;
  height: 24px;
  border: 3px solid #f1f5f9;
  border-top-color: #00b4d8;
  border-radius: 50%;
  animation: spin 0.8s cubic-bezier(0.4,0,0.2,1) infinite;
  flex-shrink: 0;
}

@keyframes spin { to { transform: rotate(360deg); } }

/* ── Error ───────────────────────────────────────────── */
.error-banner {
  display: flex;
  align-items: center;
  gap: 12px;
  background: #fef2f2;
  border: 1px solid #fecdd3;
  color: #e11d48;
  padding: 14px 18px;
  border-radius: 14px;
  font-size: 14px;
  font-weight: 500;
  margin-bottom: 24px;
}

/* ── Hint panel (search prompt) ──────────────────────── */
.hint-panel {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
  padding: 80px 16px;
  color: #94a3b8;
  text-align: center;
  border: 1px dashed #cbd5e1;
  border-radius: 20px;
  background: #ffffff;
}

.hint-panel svg { width: 48px; height: 48px; opacity: 0.3; }
.hint-panel p { font-size: 15px; font-weight: 500; margin: 0; }

/* ── Table wrapper ───────────────────────────────────── */
.table-wrap {
  overflow-x: auto;
  border: 1px solid #e2e8f0;
  border-radius: 20px;
  background: #ffffff;
  box-shadow: 0 4px 6px -1px rgba(0,0,0,0.02);
}

table {
  width: 100%;
  border-collapse: collapse;
  font-size: 14px;
}

thead { background: #f8fafc; }

th {
  padding: 16px 20px;
  text-align: left;
  font-size: 12px;
  font-weight: 700;
  color: #64748b;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  white-space: nowrap;
  border-bottom: 1px solid #e2e8f0;
}

td {
  padding: 16px 20px;
  border-bottom: 1px solid #f1f5f9;
  color: #374151;
  vertical-align: middle;
}

tr:last-child td { border-bottom: none; }
tr:hover td { background: #f8fafc; }

/* ── Mono code ──────────────────────────────────────── */
.mono {
  font-family: 'JetBrains Mono', 'Courier New', monospace;
  font-size: 12px;
  font-weight: 600;
  background: #f1f5f9;
  padding: 3px 8px;
  border-radius: 6px;
  color: #475569;
}

/* ── Description ellipsis ───────────────────────────── */
.desc {
  max-width: 200px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  color: #64748b;
}

/* ── Quantity badge ─────────────────────────────────── */
.qty-badge {
  font-weight: 800;
  font-size: 16px;
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-variant-numeric: tabular-nums;
}

.qty-badge.positive { color: #059669; }
.qty-badge.negative { color: #e11d48; }

/* ── Type chips ─────────────────────────────────────── */
.type-chip {
  display: inline-flex;
  align-items: center;
  padding: 5px 12px;
  border-radius: 6px;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  white-space: nowrap;
}

.chip-in    { background: #ecfdf5; color: #059669; border: 1px solid #a7f3d0; }
.chip-out   { background: #fef2f2; color: #e11d48; border: 1px solid #fecdd3; }
.chip-other { background: #eff6ff; color: #1d4ed8; border: 1px solid #bfdbfe; }

/* ── Stock list card ─────────────────────────────────── */
.stock-card {
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 20px;
  overflow: hidden;
  box-shadow: 0 4px 6px -1px rgba(0,0,0,0.02);
}
```

---

### `Cynapharm/src/app/features/inventory/inventory-routing.module.ts` (routes mouvements)

```typescript
{
  path: 'movements',
  loadComponent: () => import('./movements/movement-list/movement-list.component')
    .then(m => m.MovementListComponent)
}
```

Route unique — pas de `movements/new`, pas de `movements/:id`.

---

*Généré le 2026-05-25 — CynapSoftCRM BackendPFE*
