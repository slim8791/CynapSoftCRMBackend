# Stock Promotionnel — Analyse complète

> Generated: 2026-05-25 · All files read in full, no summarization.

---

## 1. Modèles backend

### 1.1 Stock_Delegue (classe de base)

Table DB : `Stocks`

| Champ | Type C# | Description |
|---|---|---|
| `Id_stock` | `int` | Clé primaire, auto-générée |
| `Id_User_Delegue` | `int` | FK vers l'utilisateur délégué |
| `Id_Produit` | `int` | FK vers le produit |
| `NumeroLot` | `string` | Numéro de lot (required, max 100) |
| `DateCreation` | `DateTime` | Date de création de l'enregistrement |
| `DateExpiration` | `DateTime` | Date d'expiration du lot |
| `QteDisponible` | `int` | Quantité disponible |
| `QteReservee` | `int` | Quantité réservée (default 0) |
| `IsDeleted` | `bool` | Soft delete (internal set, default false) |

### 1.2 Stock_Echantillon

Hérite de `Stock_Delegue`. Discriminateur : `"Echantillon"`

| Champ | Type C# | Description |
|---|---|---|
| `QteEchantillon` | `int` | Quantité totale allouée pour la campagne |
| `Description` | `string?` | Description de la campagne (nullable) |
| `DateDebut` | `DateTime?` | Début de la campagne (nullable) |
| `DateFin` | `DateTime?` | Fin de la campagne (nullable) |

Colonne DB correspondante : `DateDebut`, `DateFin` (noms non préfixés car traités en premier par EF Core).

### 1.3 Stock_Gratuite

Hérite de `Stock_Delegue`. Discriminateur : `"Gratuite"`

| Champ | Type C# | Description |
|---|---|---|
| `QteGratuite` | `int` | Quantité de stock disponible pour cette offre |
| `TypePromotion` | `string` | Libellé du type de promo (ex: "Gratuite") — non nullable, default `""` |
| `QuantiteAchat` | `int` | Seuil d'achat pour déclencher l'offre (ex: achetez 6) |
| `QuantiteGratuite` | `int` | Quantité offerte par déclenchement (ex: recevez 1) |
| `DateDebut` | `DateTime?` | Début de validité de l'offre (nullable) |
| `DateFin` | `DateTime?` | Fin de validité de l'offre (nullable) |

Colonnes DB correspondantes : `Stock_Gratuite_DateDebut`, `Stock_Gratuite_DateFin` (EF Core préfixe pour éviter la collision avec les colonnes de Stock_Echantillon).

### 1.4 Echantillon (modèle distinct — distributions)

Table DB : `Distributions_Echantillons`  
**Ce modèle N'A RIEN À VOIR avec Stock_Echantillon.** C'est un enregistrement de distribution réelle.

| Champ | Type C# | Description |
|---|---|---|
| `Id_Distribution` | `int` | Clé primaire |
| `Id_Delegue` | `int` | Délégué qui effectue la distribution |
| `Id_Medecin` | `int?` | Médecin destinataire (nullable) |
| `Id_Pharmacien` | `int?` | Pharmacien destinataire (nullable) |
| `Id_Stock` | `int` | FK vers le stock source |
| `Qte` | `int` | Quantité distribuée |
| `NumeroLot` | `string` | Numéro de lot |
| `DateDistribution` | `DateTime` | Date de distribution (GETUTCDATE()) |
| `IsDeleted` | `bool` | Soft delete |

### 1.5 StockMovement

Table DB : `Stock_Movements`

| Champ | Type C# | Description |
|---|---|---|
| `Id_Movement` | `int` | Clé primaire [Key] |
| `Id_Stock` | `int` | FK vers Stock_Delegue (cascade delete) |
| `Quantite` | `int` | Quantité (négatif = sortie) |
| `TypeMovement` | `string` | Ex: "Distribution", "Distribution-Annulée" |
| `DateMovement` | `DateTime` | GETUTCDATE() par défaut |
| `Description` | `string?` | Texte libre (nullable) |

---

## 2. Configuration TPH

```
Table DB :          Stocks
Colonne discriminateur : TypeStock (string)

Valeurs :
  Stock_Delegue    → "Standard"
  Stock_Echantillon → "Echantillon"
  Stock_Gratuite   → "Gratuite"
```

**Champs partagés** (toute la table) :
`Id_stock, Id_User_Delegue, Id_Produit, NumeroLot, DateCreation, DateExpiration, QteDisponible, QteReservee, IsDeleted`

**Champs spécifiques Stock_Echantillon** (NULL pour les autres types) :
`QteEchantillon, Description, DateDebut, DateFin`

**Champs spécifiques Stock_Gratuite** (NULL pour les autres types) :
`QteGratuite, TypePromotion, QuantiteAchat, QuantiteGratuite, Stock_Gratuite_DateDebut, Stock_Gratuite_DateFin`

**Conséquence critique** : Une ligne ne peut avoir qu'UN SEUL type. Un stock Standard, Echantillon et Gratuite ont des `Id_stock` différents et sont des lignes différentes dans la table.

---

## 3. DTOs backend

### StockDelegueDto
```
Id_stock, Id_User_Delegue, Id_Produit, NumeroLot, DateExpiration, QteDisponible, QteReservee
```
(NB : `DateCreation` et `IsDeleted` ne sont PAS dans le DTO)

### StockEchantillonDto (extends StockDelegueDto)
```
+ QteEchantillon, Description?, DateDebut?, DateFin?
```

### StockGratuiteDto (extends StockDelegueDto)
```
+ QteGratuite, TypePromotion, QuantiteAchat, QuantiteGratuite, DateDebut?, DateFin?
```

### EchantillonDto (distribution — DIFFERENT)
```
Id_Distribution, Id_Delegue, Id_Medecin?, Id_Pharmacien?, Id_Stock, Qte, NumeroLot, DateDistribution
```

---

## 4. Endpoints backend

Route de base : `[Route("api/stocks-promotionnels")]`  
Via Ocelot : `/inventory/stocks-promotionnels`

| # | Method | Route | Roles | Body | Description |
|---|---|---|---|---|---|
| 1 | POST | `/gratuite` | ADMIN, SUPERVISEUR | `StockGratuiteDto` | Crée ou met à jour un Stock_Gratuite |
| 2 | GET | `/gratuite/{idStock:int}` | ADMIN, SUPERVISEUR, DELEGUE | — | Récupère un Stock_Gratuite par son `Id_stock` |
| 3 | POST | `/echantillon` | ADMIN, SUPERVISEUR | `StockEchantillonDto` | Crée ou met à jour un Stock_Echantillon |
| 4 | GET | `/echantillon/{idStock:int}` | ADMIN, SUPERVISEUR, DELEGUE | — | Récupère un Stock_Echantillon par son `Id_stock` |

**Endpoints MANQUANTS** :
- GET `/gratuite` (liste paginée)
- GET `/echantillon` (liste paginée)
- DELETE `/gratuite/{id}`
- DELETE `/echantillon/{id}`
- GET `/echantillon/by-delegue/{id}`

---

## 5. Service layer

### 5.1 CreateUpdateStockGratuiteAsync(StockGratuiteDto)

1. Cherche dans `_db.StocksDelegues.OfType<Stock_Gratuite>()` une entité avec `Id_stock == stockDto.Id_stock`
2. Si **non trouvée** : mappe le DTO → nouvel objet `Stock_Gratuite`, définit `DateCreation = UtcNow`, `IsDeleted = false`, puis `Add(entity)`
3. Si **trouvée** : mappe le DTO → entité existante (mise à jour)
4. `SaveChangesAsync()`
5. Retourne `StockGratuiteDto` mappé depuis l'entité
6. Retourne `null` uniquement si exception

**Validations** : AUCUNE validation métier (qte > 0, stock parent, dates cohérentes). Seule validation = ModelState côté controller.

### 5.2 GetStockGratuiteByIdAsync(int idStock)

1. Cherche dans `_db.StocksDelegues.OfType<Stock_Gratuite>()` avec `AsNoTracking()` et filtre `Id_stock == idStock && !IsDeleted`
2. Retourne `null` si non trouvé
3. Retourne `StockGratuiteDto` mappé

### 5.3 CreateUpdateStockEchantillonAsync(StockEchantillonDto)

1. Identique à la Gratuite mais avec `OfType<Stock_Echantillon>()`
2. Même absence de validation métier
3. Retourne `StockEchantillonDto`

### 5.4 GetStockEchantillonByIdAsync(int idStock)

1. Cherche dans `_db.StocksDelegues.OfType<Stock_Echantillon>()` avec `AsNoTracking()` et `Id_stock == idStock && !IsDeleted`
2. Retourne `null` si non trouvé
3. Retourne `StockEchantillonDto`

---

## 6. Angular — État actuel

### 6.1 promo-stock-detail

**Ce qui est affiché** :
- En-tête avec bouton "+ Nouveau stock promo"
- Dropdown de sélection de stock (charge `GET /inventory/stocks-delegue?page=1&size=100` — **stocks STANDARD uniquement**)
- Deux cards côte à côte : Gratuite (violet) et Echantillon (teal)
- Card Gratuite : liste info, règle de gratuité (si `quantiteAchat` présent), historique (jamais affiché — voir bug #2), formulaire de mise à jour
- Card Echantillon : stat-card (qteEchantillon), campaign info, barre de progression, liste info, historique (jamais affiché), formulaire de mise à jour

**APIs appelées dans `lookup()`** :
- `GET /inventory/stocks-promotionnels/gratuite/{stockId}`
- `GET /inventory/stocks-promotionnels/echantillon/{stockId}`

**Ce qui fonctionne** ✅ :
- Chargement de la liste des stocks Standard (dropdown)
- Affichage de la card Gratuite si données retournées
- Affichage de la card Echantillon avec stat-card, barre de progression
- `saveGratuite()` et `saveEchantillon()` : POST fonctionnels (mais avec données incomplètes, voir bugs)

**Ce qui est cassé** ❌ :
- **Bug critique #1** : Le dropdown affiche les **stocks Standard** (`TypeStock='Standard'`). Mais `lookup()` appelle les endpoints avec cet ID. Un stock Standard avec `Id_stock=5` ne sera JAMAIS un `Stock_Gratuite` ou `Stock_Echantillon` (TPH = types différents, IDs différents). Les deux appels retourneront toujours 404/null. **L'utilisateur ne peut jamais voir ses promo stocks.**
- **Bug #3** : `saveGratuite()` envoie `gratuiteForm.value` mais le formulaire de mise à jour N'A PAS les champs `QuantiteAchat`, `QuantiteGratuite`, `DateDebut`, `DateFin`. Ces champs seront écrasés par null/0 à chaque update.
- **Bug #4** : `saveEchantillon()` envoie `echantillonForm.value` mais le formulaire N'A PAS `Description`, `DateDebut`, `DateFin`. Ils sont écrasés à chaque update.

**Ce qui est vide/manquant** ⚠️ :
- Historique affiché dans le template mais jamais retourné par l'API (le DTO n'a pas de champ `historique`)
- Le formulaire de mise à jour Gratuite est incomplet (manque 4 champs)
- Le formulaire de mise à jour Echantillon est incomplet (manque 3 champs)
- Pas de navigation vers le détail d'un promo stock individuel
- Pas de liste des promo stocks existants

### 6.2 promo-stock-form

**Ce qui fonctionne** ✅ :
- Dropdown stock de base (Standard) : OK
- Résumé du stock sélectionné : OK
- Section Echantillon : qteEchantillon, description, dateDebut, dateFin, info banner
- Section Gratuité : quantiteAchat, quantiteGratuite, preview, qteDisponible, dates
- Validation cross-field (typeValidator) : OK
- `onStockSelected()` met à jour les validators max
- Toast + navigate après succès : OK
- `submitError` affiché : OK

**Ce qui est cassé** ❌ :
- **Bug #5** : Pour Echantillon, le DTO envoyé utilise `id_stock: 0` (création). La création réelle insère une nouvelle ligne `Stock_Echantillon` dans la table `Stocks` avec un nouvel `Id_stock` auto-généré. Mais `base.qteDisponible` est la valeur du **stock Standard source**, pas la qte allouée pour l'offre — c'est sémantiquement incorrect (voir section 7.1).
- **Bug #6** : Pour Gratuité, `qteGratuite: v.qteDisponible` — le champ du formulaire s'appelle `qteDisponible` (label "Stock disponible pour cette offre") et est mappé à `qteGratuite` du DTO. Ce n'est pas intuitif et peut prêter à confusion.
- **Bug #7** : La validation `typeValidator` pour `echantillon` retourne `{ qteRequired: true }` si `qte <= 0`, mais le template n'affiche le message d'erreur `qteRequired` que si `form.touched`. Si le form n'est jamais touché (submit direct), l'utilisateur ne voit pas l'erreur malgré `markAllAsTouched()`.

**Manquant** ⚠️ :
- Validation `dateFin >= dateDebut`
- Validation que le stock Standard source a une `TypeStock = 'Standard'` (il pourrait y avoir des stocks Echantillon ou Gratuite dans la liste si l'API retourne tout)

---

## 7. Business Logic Analysis

### 7.1 Flux Échantillon complet

**Intention métier** : Un responsable crée un stock d'échantillons alloué à un délégué. Ce délégué distribue ensuite ces échantillons aux médecins via le module Distributions.

**Flux attendu** :
1. **ADMIN/SUPERVISEUR** crée un `Stock_Echantillon` via `POST /echantillon`
   - Spécifie le délégué, le produit, le lot, la quantité allouée, la description, les dates
   - Le backend crée une nouvelle ligne `TypeStock='Echantillon'` dans la table `Stocks`
   - **PROBLÈME** : La création actuelle copie `QteDisponible` du stock standard source dans le nouveau `Stock_Echantillon`. Ce devrait être `QteEchantillon` qui définit la quantité allouée, et `QteDisponible` commence à la même valeur (puis diminue lors des distributions).

2. **DELEGUE** consulte ses stocks échantillons disponibles
   - Endpoint manquant : `GET /echantillon/by-delegue/{id}` n'existe pas

3. **DELEGUE** crée une distribution via le module `Distributions`
   - `POST /distributions` avec `Id_Stock = Id du Stock_Echantillon`
   - `DistributionService.CreateOrUpdateEchantillonAsync()` décrémente `QteDisponible` du `Stock_Echantillon`
   - Enregistre un `StockMovement` de type "Distribution"

4. **Suivi** : `getDistributed(item) = item.qteEchantillon - item.qteDisponible`
   - Cette formule est correcte : qteEchantillon = total alloué, qteDisponible = restant → distribué = alloué - restant

**Ce qui est effectivement implémenté** :
- Création via form ✅ (mais avec qteDisponible du parent, pas une valeur indépendante ⚠️)
- Distribution via `Distributions` : oui, le `DistributionService` utilise `StocksDelegues` qui inclut les Echantillons
- Suivi dans la UI (barre de progression) ✅

### 7.2 Flux Gratuité complet

**Intention métier** : "Achetez 6 unités → recevez 1 gratuite". Applicable lors d'une commande client.

**Règle** : `QuantiteAchat = 6`, `QuantiteGratuite = 1`, `QteGratuite = 50` (stock total alloué à cette offre).

**Qui valide** : La logique de validation lors d'une commande n'est PAS implémentée dans InventoryAPI. Il n'y a pas de service qui applique la règle "si commande >= QuantiteAchat, ajouter QuantiteGratuite gratuits". C'est une logique qui devrait être dans OrderAPI.

**Flux actuel** :
1. ADMIN/SUPERVISEUR crée le Stock_Gratuite via le form (OK)
2. Le stock existe en DB avec la règle définie
3. **RIEN** ne consomme cette règle dans le code actuel — aucun endpoint d'OrderAPI ne consulte `Stock_Gratuite`
4. `QteGratuite` peut être décrémenté manuellement via le formulaire de mise à jour mais pas automatiquement

**Ce qui est implémenté** : La création et le stockage de la règle. L'application de la règle lors des commandes est entièrement absente.

---

## 8. Missing Fields

| Modèle | Champ manquant | Pourquoi nécessaire |
|---|---|---|
| `Stock_Echantillon` | `Id_Stock_Standard` (FK) | Lier le stock Echantillon au stock Standard d'origine |
| `Stock_Echantillon` | `Id_Campagne` | Grouper plusieurs stocks par campagne |
| `Stock_Gratuite` | `Id_Stock_Standard` (FK) | Lier au stock Standard de référence |
| `Stock_Gratuite` | `IsActive` (bool) | Désactiver une offre sans la supprimer |
| `StockGratuiteDto` | Tous les champs de Stock_Gratuite | AutoMapper ne mappe pas les nouveaux champs si non déclarés dans le DTO |
| `EchantillonDto` | `IsDeleted` | Exclure les distributions annulées des calculs |

---

## 9. Missing Features

| Feature | Où | Impact |
|---|---|---|
| Liste GET promo stocks | Backend controller | Impossible de lister les promo stocks créés |
| Browse promo stocks | promo-stock-detail | Le dropdown montre les Standard — jamais de résultat |
| Application règle gratuité | OrderAPI | La règle achat→gratuit n'est jamais appliquée |
| DELETE promo stock | Backend + UI | Pas de suppression |
| by-delegue endpoint | Backend | Délégué ne peut pas consulter ses échantillons |
| Champs manquants dans form update | promo-stock-detail | QuantiteAchat/Gratuite/Description écrasés |
| Validation dates | promo-stock-form | dateFin peut être avant dateDebut |
| AutoMapper config pour nouveaux champs | Backend | À vérifier — les champs DateDebut/DateFin/QuantiteAchat peuvent ne pas être mappés |

---

## 10. Bugs Found

| # | Fichier | Issue | Impact | Priorité |
|---|---|---|---|---|
| 1 | `promo-stock-detail.component.ts` | `lookup()` utilise un ID de stock Standard pour chercher Gratuite/Echantillon — toujours 404 | **CRITIQUE** — la page de détail ne fonctionne jamais | P0 |
| 2 | `promo-stock-detail.component.html` | Section "Historique" toujours vide — `historyEntries()` cherche `historique/history` mais le DTO n'a pas ces champs | Fonctionnalité morte | P2 |
| 3 | `promo-stock-detail.component.html` | `gratuiteForm` n'a pas les champs `QuantiteAchat`, `QuantiteGratuite`, `DateDebut`, `DateFin` → écrasés à null à chaque saveGratuite | Perte de données silencieuse | P1 |
| 4 | `promo-stock-detail.component.html` | `echantillonForm` n'a pas `Description`, `DateDebut`, `DateFin` → écrasés à null à chaque saveEchantillon | Perte de données silencieuse | P1 |
| 5 | `promo-stock-form.component.ts` | `base.qteDisponible = selectedStock.qteDisponible` copie le QteDisponible du stock Standard dans le nouveau promo stock — sémantiquement incorrect pour Echantillon | Donnée incorrecte | P1 |
| 6 | `StockPromotionnelService.cs` | `CreateUpdateStockGratuiteAsync` et `CreateUpdateStockEchantillonAsync` : aucune validation (qte > 0, dates, stock parent existant). Accepte `Id_stock = 0` comme création même si le DTO a des champs invalides | Données corrompues possibles | P1 |
| 7 | `AppDbContext.cs` | Aucun `DbSet<Stock_Echantillon>` ni `DbSet<Stock_Gratuite>` — uniquement `StocksDelegues`. Les requêtes `OfType<>()` fonctionnent mais il est impossible de faire des requêtes directes sans passer par le DbSet parent | Design fragile | P2 |
| 8 | `promo-stock-detail.component.ts` | `loadingLookup` est mis à `false` dans le callback Gratuite mais PAS dans le callback Echantillon → le spinner reste actif après le chargement du second appel | UX cassée | P2 |
| 9 | `inventory-routing.module.ts` | La route `promo-stocks/new` doit être AVANT `promo-stocks` pour que le router ne confonde pas `new` avec un id de stock. **C'est déjà correct.** | — | OK |
| 10 | `StockPromotionnelController.cs` | Status code 515 utilisé pour les erreurs serveur (`StatusCode(515, _response)`) — 515 n'est pas un code HTTP standard. Devrait être 500. | Non-conformité HTTP | P3 |

---

## 11. Fix Plan

### Fix Bug #1 — CRITIQUE : lookup utilise des IDs de stocks Standard

**Cause racine** : La page de détail doit parcourir les promo stocks existants, mais le dropdown liste uniquement les stocks Standard. Il faut des endpoints GET liste pour les promo stocks.

**Option A (recommandée)** : Ajouter des endpoints GET liste au backend + changer le dropdown pour lister les promo stocks.

**Fichiers à modifier** :
- `IStockPromotionnelService.cs` : ajouter `Task<IEnumerable<StockGratuiteDto>> GetAllGratuiteAsync(int page, int size)` et idem pour Echantillon
- `StockPromotionnelService.cs` : implémenter les deux méthodes
- `StockPromotionnelController.cs` : ajouter `GET /gratuite` et `GET /echantillon`
- `promo-stock.service.ts` : ajouter `getAllGratuite()` et `getAllEchantillon()`
- `promo-stock-detail.component.ts` : charger les deux listes, séparer les deux dropdowns ou utiliser un dropdown type + dropdown id
- `promo-stock-detail.component.html` : deux sections distinctes avec leur propre sélecteur

### Fix Bug #3 — gratuiteForm incomplet

**Fichier** : `promo-stock-detail.component.ts`
```typescript
// Remplacer le gratuiteForm par :
this.gratuiteForm = this.fb.group({
  id_User_Delegue:  [null, Validators.required],
  id_Produit:       [null, Validators.required],
  numeroLot:        ['',   Validators.required],
  qteDisponible:    [0,    [Validators.required, Validators.min(0)]],
  qteReservee:      [0,    [Validators.required, Validators.min(0)]],
  qteGratuite:      [0,    [Validators.required, Validators.min(0)]],
  typePromotion:    ['',   Validators.required],
  quantiteAchat:    [0],
  quantiteGratuite: [0],
  dateDebut:        [null],
  dateFin:          [null]
});
```
**Fichier** : `promo-stock-detail.component.html` — ajouter dans le mini-grid de gratuiteForm :
```html
<div class="field"><label>Achat déclencheur</label><input type="number" formControlName="quantiteAchat" min="0" /></div>
<div class="field"><label>Qté offerte</label><input type="number" formControlName="quantiteGratuite" min="0" /></div>
<div class="field"><label>Date début</label><input type="date" formControlName="dateDebut" /></div>
<div class="field"><label>Date fin</label><input type="date" formControlName="dateFin" /></div>
```

### Fix Bug #4 — echantillonForm incomplet

**Fichier** : `promo-stock-detail.component.ts`
```typescript
// Remplacer le echantillonForm par :
this.echantillonForm = this.fb.group({
  id_User_Delegue: [null, Validators.required],
  id_Produit:      [null, Validators.required],
  numeroLot:       ['',   Validators.required],
  qteDisponible:   [0,    [Validators.required, Validators.min(0)]],
  qteReservee:     [0,    [Validators.required, Validators.min(0)]],
  qteEchantillon:  [0,    [Validators.required, Validators.min(0)]],
  description:     [null],
  dateDebut:       [null],
  dateFin:         [null]
});
```
**Fichier** : `promo-stock-detail.component.html` — ajouter dans le mini-grid echantillonForm :
```html
<div class="field"><label>Description</label><input type="text" formControlName="description" /></div>
<div class="field"><label>Date début</label><input type="date" formControlName="dateDebut" /></div>
<div class="field"><label>Date fin</label><input type="date" formControlName="dateFin" /></div>
```
Et dans `saveEchantillon()`, patcher aussi les nouveaux champs lors du `patchValue()`.

### Fix Bug #8 — loadingLookup reste actif

**Fichier** : `promo-stock-detail.component.ts`
```typescript
// Dans le subscribe de getEchantillon, ajouter loadingLookup = false :
this.svc.getEchantillon(id).pipe(takeUntil(this.destroy$)).subscribe({
  next: d => {
    this.echantillonData = d;
    if (d) this.echantillonForm.patchValue({ ...d, id_stock: undefined });
    this.loadingLookup = false;  // ← AJOUTER
    this.cdr.markForCheck();
  },
  error: () => {
    this.loadingLookup = false;  // ← AJOUTER
    this.cdr.markForCheck();
  }
});
```

### Fix Bug #10 — Status code 515

**Fichier** : `StockPromotionnelController.cs` — remplacer `StatusCode(515, _response)` par `StatusCode(500, _response)` (4 occurrences).

---

## 12. Code complet de chaque fichier lu

### Models/Stock_Delegue.cs
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

### Models/Stock_Echantillon.cs
```csharp
namespace CynapCRM.Services.InventoryAPI.Models
{
    public class Stock_Echantillon : Stock_Delegue
    {
        public int       QteEchantillon { get; set; }
        public string?   Description    { get; set; }
        public DateTime? DateDebut      { get; set; }
        public DateTime? DateFin        { get; set; }
    }
}
```

### Models/Stock_Gratuite.cs
```csharp
namespace CynapCRM.Services.InventoryAPI.Models
{
    public class Stock_Gratuite : Stock_Delegue
    {
        public int QteGratuite { get; set; }
        public string TypePromotion { get; set; } = string.Empty;
        public int QuantiteAchat { get; set; }
        public int QuantiteGratuite { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
    }
}
```

### Models/Echantillon.cs (distributions)
```csharp
using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.InventoryAPI.Models
{
    public class Echantillon
    {
        public int Id_Distribution { get; set; }

        public int Id_Delegue { get; set; }

        public int? Id_Medecin { get; set; }

        public int? Id_Pharmacien { get; set; }
        public int Id_Stock { get; set; }
        public int Qte { get; set; }

        public string NumeroLot { get; set; } = string.Empty;

        public DateTime DateDistribution { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; internal set; } = false;
    }
}
```

### Models/Dto/StockDelegueDto.cs
```csharp
namespace CynapCRM.Services.InventoryAPI.Models.Dto
{
    public class StockDelegueDto
    {
        public int Id_stock { get; set; }

        public int Id_User_Delegue { get; set; }

        public int Id_Produit { get; set; }

        public string NumeroLot { get; set; }

        public DateTime DateExpiration { get; set; }

        public int QteDisponible { get; set; }

        public int QteReservee { get; set; }
    }
}
```

### Models/Dto/StockEchantillonDto.cs
```csharp
namespace CynapCRM.Services.InventoryAPI.Models.Dto
{
    public class StockEchantillonDto : StockDelegueDto
    {
        public int       QteEchantillon { get; set; }
        public string?   Description    { get; set; }
        public DateTime? DateDebut      { get; set; }
        public DateTime? DateFin        { get; set; }
    }
}
```

### Models/Dto/StockGratuiteDto.cs
```csharp
namespace CynapCRM.Services.InventoryAPI.Models.Dto
{
    public class StockGratuiteDto : StockDelegueDto
    {
        public int QteGratuite { get; set; }
        public string TypePromotion { get; set; } = string.Empty;
        public int QuantiteAchat { get; set; }
        public int QuantiteGratuite { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
    }
}
```

### Models/Dto/EchantillonDto.cs (distributions)
```csharp
namespace CynapCRM.Services.InventoryAPI.Models.Dto
{
    public class EchantillonDto
    {
        public int Id_Distribution { get; set; }

        public int Id_Delegue { get; set; }

        public int? Id_Medecin { get; set; }

        public int? Id_Pharmacien { get; set; }

        public int Id_Stock { get; set; }

        public int Qte { get; set; }

        public string NumeroLot { get; set; }

        public DateTime DateDistribution { get; set; }
    }
}
```

### Controllers/StockPromotionnelController.cs
```csharp
using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.InventoryAPI.Controllers
{

    // ═══════════════════════════════════════
    // StockPromotionnelController.cs
    // ═══════════════════════════════════════

    [ApiController]
    [Route("api/stocks-promotionnels")]
    [Authorize]
    public class StockPromotionnelController : ControllerBase
    {
        private readonly IStockPromotionnelService _stockPromotionnelService;
        protected ResponseDto _response;

        public StockPromotionnelController(
            IStockPromotionnelService stockPromotionnelService)
        {
            _stockPromotionnelService = stockPromotionnelService;
            _response = new ResponseDto();
        }

        // FIX: ajout restriction de rôle
        [HttpPost("gratuite")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> CreateOrUpdateGratuite(
            [FromBody] StockGratuiteDto gratuiteDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données invalides.";
                    return BadRequest(_response);
                }
                var result = await _stockPromotionnelService
                    .CreateUpdateStockGratuiteAsync(gratuiteDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur lors du traitement de la gratuité.";
                    return BadRequest(_response); // FIX: NotFound → BadRequest
                }
                _response.Result = result;
                _response.Message = "Stock de gratuité mis à jour.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpGet("gratuite/{idStock:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetStockGratuiteById(int idStock)
        {
            try
            {
                if (idStock <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id stock invalide.";
                    return BadRequest(_response);
                }
                var result = await _stockPromotionnelService
                    .GetStockGratuiteByIdAsync(idStock);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Gratuité introuvable.";
                    return NotFound(_response);
                }
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

        [HttpPost("echantillon")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> CreateOrUpdateEchantillonStock(
            [FromBody] StockEchantillonDto echantillonDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données invalides.";
                    return BadRequest(_response);
                }
                var result = await _stockPromotionnelService
                    .CreateUpdateStockEchantillonAsync(echantillonDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur lors du traitement du stock échantillon.";
                    return BadRequest(_response); // FIX: NotFound → BadRequest
                }
                _response.Result = result;
                _response.Message = "Stock échantillon mis à jour avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpGet("echantillon/{idStock:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetStockEchantillonById(int idStock)
        {
            try
            {
                if (idStock <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id stock invalide.";
                    return BadRequest(_response);
                }
                var result = await _stockPromotionnelService
                    .GetStockEchantillonByIdAsync(idStock);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Stock échantillon introuvable.";
                    return NotFound(_response);
                }
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

### Service/IService/IStockPromotionnelService.cs
```csharp
using CynapCRM.Services.InventoryAPI.Models.Dto;

namespace CynapCRM.Services.InventoryAPI.Service.IService
{
    public interface IStockPromotionnelService
    {
        // STOCK GRATUITÉ
        Task<StockGratuiteDto?> CreateUpdateStockGratuiteAsync(StockGratuiteDto stockDto);
        Task<StockGratuiteDto?> GetStockGratuiteByIdAsync(int idStock);

        //  STOCK ÉCHANTILLON
        Task<StockEchantillonDto?> CreateUpdateStockEchantillonAsync(StockEchantillonDto stockDto);
        Task<StockEchantillonDto?> GetStockEchantillonByIdAsync(int idStock);
    }
}
```

### Service/StockPromotionnelService.cs
```csharp
using CynapCRM.Services.InventoryAPI.Models;
using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CynapCRM.Services.InventoryAPI.Data;

namespace CynapCRM.Services.InventoryAPI.Service
{
    public class StockPromotionnelService : IStockPromotionnelService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        public StockPromotionnelService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;

        }
        public async Task<StockGratuiteDto?> CreateUpdateStockGratuiteAsync(StockGratuiteDto stockDto)
        {

            var entity = await _db.StocksDelegues
                            .OfType<Stock_Gratuite>() 
                            .FirstOrDefaultAsync(s => s.Id_stock == stockDto.Id_stock);

            if (entity == null)
            {
                entity = _mapper.Map<Stock_Gratuite>(stockDto);
                entity.DateCreation = DateTime.UtcNow;
                entity.IsDeleted = false;

                _db.StocksDelegues.Add(entity);
            }
            else
            {
                _mapper.Map(stockDto, entity);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<StockGratuiteDto>(entity);

        }
        public async Task<StockGratuiteDto?> GetStockGratuiteByIdAsync(int idStock)
        {

            var entity = await _db.StocksDelegues
                            .OfType<Stock_Gratuite>() 
                            .AsNoTracking()
                            .FirstOrDefaultAsync(s =>
                                s.Id_stock == idStock &&
                                !s.IsDeleted);

            if (entity == null)
            {
                return null;
            }
            return _mapper.Map<StockGratuiteDto>(entity);
        }
        public async Task<StockEchantillonDto?> CreateUpdateStockEchantillonAsync(StockEchantillonDto stockDto)
        {

            var entity = await _db.StocksDelegues
                            .OfType<Stock_Echantillon>() 
                            .FirstOrDefaultAsync(s => s.Id_stock == stockDto.Id_stock);

            if (entity == null)
            {
                entity = _mapper.Map<Stock_Echantillon>(stockDto);
                entity.DateCreation = DateTime.UtcNow;
                entity.IsDeleted = false;

                _db.StocksDelegues.Add(entity);
            }
            else
            {
                _mapper.Map(stockDto, entity);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<StockEchantillonDto>(entity);

        }
        public async Task<StockEchantillonDto?> GetStockEchantillonByIdAsync(int idStock)
        {

            var entity = await _db.StocksDelegues
                            .OfType<Stock_Echantillon>() 
                            .AsNoTracking()
                            .FirstOrDefaultAsync(s =>
                                s.Id_stock == idStock &&
                                !s.IsDeleted);
            if (entity == null)
            {
                return null;
            }
            return _mapper.Map<StockEchantillonDto>(entity);
        }
    }
}
```

### Data/AppDbContext.cs
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
            modelBuilder.Entity<Stock_Delegue>()
                .HasKey(s => s.Id_stock);

            modelBuilder.Entity<Echantillon>()
                .HasKey(e => e.Id_Distribution);

            modelBuilder.Entity<StockMovement>()
                .HasKey(m => m.Id_Movement);

            // Index (Performance )
            modelBuilder.Entity<Stock_Delegue>().HasIndex(s => s.NumeroLot);
            modelBuilder.Entity<Echantillon>().HasIndex(e => e.NumeroLot);

            modelBuilder.Entity<Stock_Delegue>().HasIndex(s => s.Id_User_Delegue);
            modelBuilder.Entity<Echantillon>().HasIndex(e => e.Id_Medecin);
            modelBuilder.Entity<Echantillon>().HasIndex(e => e.Id_Pharmacien);
            modelBuilder.Entity<StockMovement>().HasIndex(m => m.Id_Stock);

            // Contraintes (data clean )
            modelBuilder.Entity<Stock_Delegue>()
                .Property(s => s.NumeroLot)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Echantillon>()
                .Property(e => e.NumeroLot)
                .IsRequired()
                .HasMaxLength(100);

            // Relations


            modelBuilder.Entity<StockMovement>()
                .HasOne<Stock_Delegue>()
                .WithMany()
                .HasForeignKey(m => m.Id_Stock)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Stock_Delegue>()
                .Property(s => s.QteReservee)
                .HasDefaultValue(0);

            modelBuilder.Entity<Echantillon>()
                .Property(e => e.DateDistribution)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<StockMovement>()
                .Property(m => m.DateMovement)
                .HasDefaultValueSql("GETUTCDATE()");

            
            // Noms des tables
            modelBuilder.Entity<Stock_Delegue>().ToTable("Stocks");
            modelBuilder.Entity<Echantillon>().ToTable("Distributions_Echantillons");

            modelBuilder.Entity<StockMovement>().ToTable("Stock_Movements");
        }
    }
}
```

### promo-stock-detail.component.ts
```typescript
import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { PromoStockService, StockGratuiteDto, StockEchantillonDto } from '../services/promo-stock.service';
import { StockService, StockDelegueDto } from '../../stocks/services/stock.service';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-promo-stock-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, FormsModule, EmptyStateComponent],
  templateUrl: './promo-stock-detail.component.html',
  styleUrls: ['./promo-stock-detail.component.css']
})
export class PromoStockDetailComponent implements OnInit, OnDestroy {
  stockId: number | null = null;
  loadingLookup = false;
  loadingStocks = false;
  allStocks: StockDelegueDto[] = [];
  lookupError = '';
  searched = false;

  gratuiteData: StockGratuiteDto | null = null;
  echantillonData: StockEchantillonDto | null = null;

  gratuiteForm!: FormGroup;
  echantillonForm!: FormGroup;

  savingGratuite = false;
  savingEchantillon = false;
  gratuiteSuccess = '';
  gratuiteError = '';
  echantillonSuccess = '';
  echantillonError = '';

  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private svc: PromoStockService,
    private stockSvc: StockService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadingStocks = true;
    this.stockSvc.getAll(1, 100).pipe(takeUntil(this.destroy$)).subscribe({
      next: data => { this.allStocks = data; this.loadingStocks = false; this.cdr.markForCheck(); },
      error: ()   => { this.loadingStocks = false; this.cdr.markForCheck(); }
    });

    this.gratuiteForm = this.fb.group({
      id_User_Delegue: [null, Validators.required],
      id_Produit:      [null, Validators.required],
      numeroLot:       ['',   Validators.required],
      qteDisponible:   [0,    [Validators.required, Validators.min(0)]],
      qteReservee:     [0,    [Validators.required, Validators.min(0)]],
      qteGratuite:     [0,    [Validators.required, Validators.min(0)]],
      typePromotion:   ['',   Validators.required]
    });
    this.echantillonForm = this.fb.group({
      id_User_Delegue: [null, Validators.required],
      id_Produit:      [null, Validators.required],
      numeroLot:       ['',   Validators.required],
      qteDisponible:   [0,    [Validators.required, Validators.min(0)]],
      qteReservee:     [0,    [Validators.required, Validators.min(0)]],
      qteEchantillon:  [0,    [Validators.required, Validators.min(0)]]
    });
  }

  lookup(): void {
    if (!this.stockId) return;
    this.loadingLookup = true;
    this.lookupError = '';
    this.searched = true;
    this.gratuiteData = null;
    this.echantillonData = null;

    const id = this.stockId;

    this.svc.getGratuite(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: d => { this.gratuiteData = d; if (d) this.gratuiteForm.patchValue({ ...d, id_stock: undefined }); this.loadingLookup = false; this.cdr.markForCheck(); },
      error: () => { this.loadingLookup = false; this.cdr.markForCheck(); }
    });

    this.svc.getEchantillon(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: d => { this.echantillonData = d; if (d) this.echantillonForm.patchValue({ ...d, id_stock: undefined }); this.cdr.markForCheck(); },
      error: () => { this.cdr.markForCheck(); }
    });
  }

  saveGratuite(): void {
    this.gratuiteForm.markAllAsTouched();
    if (this.gratuiteForm.invalid || !this.stockId) return;
    this.savingGratuite = true;
    this.gratuiteError = '';
    this.gratuiteSuccess = '';
    const dto: StockGratuiteDto = { ...this.gratuiteForm.value, id_stock: this.stockId };
    this.svc.createOrUpdateGratuite(dto).pipe(takeUntil(this.destroy$)).subscribe({
      next: d => { this.gratuiteData = d; this.savingGratuite = false; this.gratuiteSuccess = 'Enregistré.'; this.cdr.markForCheck(); },
      error: () => { this.gratuiteError = 'Erreur lors de l\'enregistrement.'; this.savingGratuite = false; this.cdr.markForCheck(); }
    });
  }

  saveEchantillon(): void {
    this.echantillonForm.markAllAsTouched();
    if (this.echantillonForm.invalid || !this.stockId) return;
    this.savingEchantillon = true;
    this.echantillonError = '';
    this.echantillonSuccess = '';
    const dto: StockEchantillonDto = { ...this.echantillonForm.value, id_stock: this.stockId };
    this.svc.createOrUpdateEchantillon(dto).pipe(takeUntil(this.destroy$)).subscribe({
      next: d => { this.echantillonData = d; this.savingEchantillon = false; this.echantillonSuccess = 'Enregistré.'; this.cdr.markForCheck(); },
      error: () => { this.echantillonError = 'Erreur lors de l\'enregistrement.'; this.savingEchantillon = false; this.cdr.markForCheck(); }
    });
  }

  historyEntries(data: unknown): any[] {
    const raw = data as any;
    const history = raw?.historique ?? raw?.Historique ?? raw?.history ?? raw?.History ?? [];
    return Array.isArray(history) ? history : [];
  }

  historyDate(entry: any): string | null {
    return entry?.date ?? entry?.Date ?? entry?.dateMovement ?? entry?.DateMovement ?? entry?.createdAt ?? entry?.CreatedAt ?? null;
  }

  historyType(entry: any): string {
    return entry?.type ?? entry?.Type ?? entry?.typeMovement ?? entry?.TypeMovement ?? entry?.action ?? entry?.Action ?? '—';
  }

  historyQuantity(entry: any): string {
    return String(entry?.quantite ?? entry?.Quantite ?? entry?.qte ?? entry?.Qte ?? '—');
  }

  historyDescription(entry: any): string {
    return entry?.description ?? entry?.Description ?? entry?.detail ?? entry?.Detail ?? '—';
  }

  getDistributed(item: any): number {
    if (!item?.qteEchantillon) return 0;
    return Math.max(0, item.qteEchantillon - (item.qteDisponible ?? 0));
  }

  getUsagePercent(item: any): number {
    if (!item?.qteEchantillon) return 0;
    const distributed = this.getDistributed(item);
    return Math.min(100, (distributed / item.qteEchantillon) * 100);
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
```

### promo-stock-detail.component.html
*(voir fichier — 162 lignes, contenu reproduit intégralement dans la section lecture ci-dessus)*

### promo-stock-form.component.ts
*(voir fichier — 186 lignes, contenu reproduit intégralement dans la section lecture ci-dessus)*

### promo-stock-form.component.html
*(voir fichier — 170 lignes, contenu reproduit intégralement dans la section lecture ci-dessus)*

### inventory-routing.module.ts (routes promo-stocks)
```typescript
{
  path: 'promo-stocks/new',
  loadComponent: () => import('./promo-stocks/promo-stock-form/promo-stock-form.component')
    .then(m => m.PromoStockFormComponent)
},
{
  path: 'promo-stocks',
  loadComponent: () => import('./promo-stocks/promo-stock-detail/promo-stock-detail.component')
    .then(m => m.PromoStockDetailComponent)
}
```
**Note** : Pas de route `promo-stocks/:id` — il n'existe pas de page de détail individuelle par ID.

---

## 13. Synthèse priorités

| Priorité | Action |
|---|---|
| **P0** | Ajouter endpoints GET liste (Gratuite + Echantillon) au backend + refaire la page detail avec les bons dropdowns |
| **P1** | Corriger `gratuiteForm` et `echantillonForm` pour inclure tous les champs (bugs #3 et #4) |
| **P1** | Corriger `loadingLookup` dans le callback Echantillon (bug #8) |
| **P2** | Corriger status code 515 → 500 (bug #10) |
| **P2** | Supprimer les sections "Historique" ou implémenter l'endpoint correspondant |
| **P3** | Implémenter la logique d'application des gratuités dans OrderAPI |
