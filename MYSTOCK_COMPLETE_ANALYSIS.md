# Mon Stock — Analyse complète
# Échantillons + Stock Promo + Historique

> Document généré le 2026-05-28 à partir du code source complet.
> Tous les fichiers ont été lus ligne par ligne — aucun résumé.

---

═══════════════════════════════════════════════════════════════
## PARTIE 1 — MODÈLES
═══════════════════════════════════════════════════════════════

---

### 1.1 StockDelegue.cs
Utilisé pour : **segment Échantillons** (onglet 0)
Endpoint source : `GET inventory/stocks-delegue/by-delegue/{userId}`
Backend DTO source : `StockDelegueDto`

| Champ MAUI         | Type          | JsonPropertyName       | Champ Backend       | Match?          |
|--------------------|---------------|------------------------|---------------------|-----------------|
| `Id`               | `int`         | `"id_stock"`           | `Id_stock`          | ✅ Correct       |
| `IdDelegue`        | `int`         | `"id_User_Delegue"`    | `Id_User_Delegue`   | ✅ Correct       |
| `ProductId`        | `int`         | `"id_Produit"`         | `Id_Produit`        | ✅ Correct       |
| `NumeroLot`        | `string`      | `"numeroLot"`          | `NumeroLot`         | ✅ Correct       |
| `DateExpiration`   | `DateTime?`   | `"dateExpiration"`     | `DateExpiration`    | ✅ Correct       |
| `QuantiteRestante` | `int`         | `"qteDisponible"`      | `QteDisponible`     | ✅ Correct       |
| `QuantiteReservee` | `int`         | `"qteReservee"`        | `QteReservee`       | ✅ Correct       |
| `ProductNom`       | `string`      | `"nomProduit"`         | ❌ Absent du DTO    | ⚠️ Jamais rempli par JSON |
| `QuantiteAllouee`  | `int`         | aucun                  | ❌ Absent du DTO    | N/A (SQLite only) |

**Analyse `DateExpiration`** :
- `[JsonPropertyName("dateExpiration")]` est correct ✅
- Mais le backend `StockDelegueDto.DateExpiration` est `DateTime` (non nullable, pas `DateTime?`)
- Si le stock n'a pas de date d'expiration, le backend envoie `"dateExpiration": "0001-01-01T00:00:00"` (valeur par défaut `DateTime.MinValue`)
- Le champ MAUI `DateTime?` désérialise `"0001-01-01T00:00:00"` → `new DateTime(1,1,1)` — HasValue = **true**
- Conséquence : `s.DateExpiration.HasValue` retourne `true` même pour les stocks sans expiration
- Dans `RefreshDisplayedList()` : `ExpiryLabel = $"Exp. {s.DateExpiration.Value:dd/MM/yyyy}"` → **affiche "Exp. 01/01/0001"** ← 🐛 BUG

**Analyse `ProductNom`** :
- Absent du DTO backend → JSON ne contient jamais `nomProduit`
- `[JsonPropertyName("nomProduit")]` est une annotation forward-compat uniquement
- Résolution via `ProductService.GetProductByIdAsync(ProductId)` dans `MyStockViewModel.LoadAsync()` ✅ (FIX-5 appliqué)

---

### 1.2 StockPromo.cs
Utilisé pour : **segment Stock Promo** (onglet 1)
Endpoint source : `GET inventory/stocks-promotionnels/echantillon`
Backend DTO source : `StockEchantillonDto` (hérite de `StockDelegueDto`)

| Champ MAUI        | Type        | JsonPropertyName     | Champ Backend     | Match?          |
|-------------------|-------------|----------------------|-------------------|-----------------|
| `Id`              | `int`       | `"id_stock"`         | `Id_stock`        | ✅ Correct       |
| `ProductId`       | `int`       | `"id_Produit"`       | `Id_Produit`      | ✅ Correct       |
| `ProductNom`      | `string`    | `"nomProduit"`       | ❌ Absent du DTO  | ⚠️ Jamais rempli par JSON |
| `Quantite`        | `int`       | `"qteDisponible"`    | `QteDisponible`   | ✅ Correct       |
| `QteEchantillon`  | `int`       | `"qteEchantillon"`   | `QteEchantillon`  | ✅ Correct       |
| `NumeroLot`       | `string`    | `"numeroLot"`        | `NumeroLot`       | ✅ Correct       |
| `DateExpiration`  | `DateTime?` | `"dateExpiration"`   | `DateExpiration`  | ✅ Correct (même bug que StockDelegue si 0001-01-01) |

**Remarque** : `DateExpiration` dans `StockPromo` n'est pas utilisé dans `RefreshDisplayedList()` — la vue Stock Promo ne montre pas la date d'expiration actuellement. La valeur `Quantite` est mappée sur `qteDisponible`, ce qui est cohérent.

---

### 1.3 StockMouvement.cs
Utilisé pour : **segment Historique** (onglet 2)
Endpoint source : `GET inventory/stock-movements/by-delegue/{userId}`
Backend DTO source : `StockMovementDto`

| Champ MAUI      | Type        | JsonPropertyName     | Champ Backend    | Match?                                 |
|-----------------|-------------|----------------------|------------------|----------------------------------------|
| `Id`            | `int`       | `"id_Movement"`      | `Id_Movement`    | ✅ Correct                              |
| `IdStock`       | `int`       | `"id_Stock"`         | `Id_Stock`       | ✅ Correct                              |
| `Quantite`      | `int`       | `"quantite"`         | `Quantite`       | ✅ Correct                              |
| `TypeMouvement` | `string`    | `"typeMovement"`     | `TypeMovement`   | ✅ Correct (English → French bridged)   |
| `DateMouvement` | `DateTime`  | `"dateMovement"`     | `DateMovement`   | ✅ Correct (English → French bridged)   |
| `Description`   | `string?`   | `"description"`      | `Description`    | ✅ Correct                              |
| `ProductNom`    | `string`    | aucun                | ❌ Absent du DTO | Résolu via lookup stockId→productName  |

**Propriétés calculées** (toutes avec `[JsonIgnore]`) :

| Propriété      | [JsonIgnore]? | Formule                                    | Usage XAML           |
|----------------|---------------|--------------------------------------------|----------------------|
| `IsPositive`   | ✅ Oui        | `Quantite >= 0`                            | DataTrigger couleur  |
| `QuantiteLabel`| ✅ Oui        | `"+{Quantite}"` ou `"{Quantite}"`          | `{Binding QuantiteLabel}` |
| `DateDay`      | ✅ Oui        | `"dd/MM"` si année > 1, sinon `"—"`        | `{Binding DateDay}`  |
| `DateYear`     | ✅ Oui        | `"yyyy"` si année > 1, sinon `""`          | `{Binding DateYear}` |
| `DateLabel`    | ✅ Oui        | `"dd/MM/yyyy HH:mm"` si année > 1          | Non utilisé en XAML  |

**Valeurs de TypeMouvement envoyées par le backend** (d'après `StockMovementService.cs`) :
- `"Increment"` — IncrementStockAsync
- `"Decrement"` — DecrementStockAsync
- `"Transfer-In"` — TransferStockAsync (destination)
- `"Transfer-Out"` — TransferStockAsync (source)

Les DataTriggers XAML utilisent exactement ces valeurs → ✅ Match parfait.

**Historique du bug** : Avant la correction, aucun `[JsonPropertyName]` n'existait sur ce modèle. Le backend envoie `typeMovement` (anglais) mais le champ C# s'appelait `TypeMouvement` (français). Même en mode case-insensitive, `TypeMouvement` ≠ `typeMovement` (lettres différentes), donc aucune désérialisation n'avait lieu.

---

### 1.4 StockDisplayItem.cs
Objet intermédiaire pour l'affichage — construit dans `RefreshDisplayedList()`, jamais désérialisé depuis JSON.

| Champ                | Type       | [JsonIgnore]? | Source                                  |
|----------------------|------------|---------------|-----------------------------------------|
| `StockId`            | `int`      | Non           | `s.Id` (StockDelegue)                   |
| `NumeroLot`          | `string`   | Non           | `s.NumeroLot`                           |
| `ProductId`          | `int`      | Non           | `s.ProductId`                           |
| `ProductNom`         | `string`   | Non           | `s.ProductNom` (résolu via ProductSvc)  |
| `QuantiteLabel`      | `string`   | Non           | `$"Restant : {s.QuantiteRestante}"`     |
| `ExpiryLabel`        | `string?`  | Non           | Formaté ou null si pas d'expiration     |
| `QuantiteRestante`   | `int`      | Non           | `s.QuantiteRestante`                    |
| `IsEchantillon`      | `bool`     | Non           | `true` pour onglet 0, `false` onglet 1  |
| `HasExpiry`          | `bool`     | ✅ Oui        | `ExpiryLabel != null`                   |
| `CanDistribute`      | `bool`     | ✅ Oui        | `IsEchantillon && QuantiteRestante > 0` |

⚠️ **Bug critique** : Le XAML lie `{Binding ProgressValue}` sur la `ProgressBar` mais `StockDisplayItem` n'a **aucune propriété `ProgressValue`**. Cela cause une exception de binding silencieuse → la barre de progression reste toujours à 0%.

---

### 1.5 StockSummaryDto.cs
Utilisé uniquement par `DashboardViewModel` — non utilisé dans `MyStockViewModel`.

| Champ               | Type     | [JsonPropertyName]? |
|---------------------|----------|---------------------|
| `TotalProduits`     | `int`    | Aucun               |
| `TotalQteDisponible`| `int`    | Aucun               |
| `StocksVides`       | `int`    | Aucun               |
| `StocksFaibles`     | `int`    | Aucun               |
| `TotalDistributions`| `int`    | Aucun               |
| `TotalQteDistribuee`| `int`    | Aucun               |
| `DernierMouvement`  | `string` | Aucun               |

Pas de `[JsonPropertyName]` — fonctionne uniquement si le serveur envoie exactement les mêmes noms en PascalCase ou si `PropertyNameCaseInsensitive = true` est activé dans `ApiService`.

---

### 1.6 Fichiers absents
- `StockEchantillon.cs` — n'existe pas dans `Models/Inventory/`
- `DistributionRequest.cs` — n'existe pas ; le DTO de distribution est un objet anonyme construit dans `PostDistributionAsync()`

---

═══════════════════════════════════════════════════════════════
## PARTIE 2 — INVENTORYSERVICE
═══════════════════════════════════════════════════════════════

### 2.1 Tableau complet des méthodes

| Méthode                         | Verbe | URL MAUI (via gateway)                                        | Retour                        | Problèmes |
|---------------------------------|-------|---------------------------------------------------------------|-------------------------------|-----------|
| `GetStockMouvementsAsync`       | GET   | `inventory/stock-movements?productId=X&from=Y`                | `List<StockMouvement>?`       | Non utilisé dans MyStockViewModel |
| `GetStockDelegueAsync`          | GET   | `inventory/stocks-delegue/by-delegue/{userId}`                | `List<StockDelegue>?`         | ✅ URL correcte |
| `GetStockPromoAsync`            | GET   | `inventory/stocks-promotionnels/echantillon`                  | `List<StockPromo>?`           | ✅ URL correcte |
| `GetDistributionAsync`          | GET   | `inventory/distributions`                                     | `object?`                     | Non utilisé dans MyStockViewModel |
| `PostDistributionAsync`         | POST  | `inventory/distributions`                                     | `object?`                     | Voir § 2.2 |
| `GetStockSummaryAsync`          | GET   | `inventory/inventory-business/summary/{idDelegue}`            | `StockSummaryDto?`            | Non utilisé dans MyStockViewModel |
| `GetMovementsByDelegueAsync`    | GET   | `inventory/stock-movements/by-delegue/{idDelegue}`            | `List<StockMouvement>?`       | ✅ URL correcte |

---

### 2.2 Analyse détaillée de chaque méthode

#### `GetStockDelegueAsync()`
```
URL   : inventory/stocks-delegue/by-delegue/{userId}
Verbe : GET
UserId: lu via SecureStorage.GetAsync(StorageKeys.UserId) DANS cette méthode
Retour: List<StockDelegue>?
```
- ✅ URL correspond au backend `[Route("api/stocks-delegue")]` → `[HttpGet("by-delegue/{idDelegue}")]`
- Via Ocelot : `inventory/stocks-delegue/*` → `api/stocks-delegue/*`
- ⚠️ Le userId est lu ici ET dans `LoadAsync()`. Double lecture SecureStorage (mineur).

#### `GetStockPromoAsync()`
```
URL   : inventory/stocks-promotionnels/echantillon
Verbe : GET
Retour: List<StockPromo>?
```
- ✅ URL correspond au backend `[Route("api/stocks-promotionnels")]` → `[HttpGet("echantillon")]`
- Retourne `StockEchantillonDto[]` (hérite de `StockDelegueDto`)
- `StockPromo` est maintenant aligné avec ces champs via `[JsonPropertyName]`

#### `GetMovementsByDelegueAsync(int idDelegue)`
```
URL   : inventory/stock-movements/by-delegue/{idDelegue}
Verbe : GET
Retour: List<StockMouvement>?
Const : ApiRoutes.Inventory.MovementsByDelegue = "inventory/stock-movements/by-delegue"
```
- ✅ URL correspond au backend `[Route("api/stock-movements")]` → `[HttpGet("by-delegue/{idDelegue:int}")]`
- Backend appelle `GetMovementHistoryByDelegueAsync(idDelegue)` qui joint les stocks du délégué

#### `GetStockSummaryAsync(int idDelegue)`
```
URL   : inventory/inventory-business/summary/{idDelegue}
Verbe : GET
Retour: StockSummaryDto?
Const : ApiRoutes.Inventory.StockSummary = "inventory/inventory-business/summary"
```
- Non utilisé dans `MyStockViewModel` — uniquement pour le dashboard

#### `PostDistributionAsync(...)`
```
URL   : inventory/distributions
Verbe : POST
Const : (hardcodé "inventory/distributions")
Corps :
  {
    id_Distribution  : 0,
    id_Delegue       : userId,
    id_Medecin       : idMedecin (nullable),
    id_Pharmacien    : idPharmacien (nullable),
    id_Stock         : stockId,
    qte              : quantite,
    numeroLot        : numeroLot,
    dateDistribution : null
  }
```
- ✅ URL correspond au backend via Ocelot → `api/distributions`
- ⚠️ `dateDistribution: null` → le backend doit auto-remplir la date (à vérifier)
- ⚠️ La méthode lit à nouveau le userId via `SecureStorage` alors que le caller pourrait le passer en paramètre

---

═══════════════════════════════════════════════════════════════
## PARTIE 3 — MYSTOCKVIEWMODEL
═══════════════════════════════════════════════════════════════

### 3.1 Initialisation

**Lecture du userId** :
```csharp
var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
// StorageKeys.UserId = "user_id"
if (int.TryParse(userIdStr, out var userId))
```
- Clé : `"user_id"`
- Résistant à l'échec : si parse échoue, bloc `if` est ignoré silencieusement (pas d'alerte)
- ⚠️ Aucun message d'erreur si userId invalide → mouvements simplement absents

**Chargement des segments** :
- Les 3 sources (échantillons, promo, mouvements) sont chargées **séquentiellement** dans `LoadAsync()` :
  1. `_cache.GetOrCreateAsync(CacheKeyEchantillon, ...)` → échantillons
  2. `_cache.GetOrCreateAsync(CacheKeyPromo, ...)` → promo
  3. FIX-5 : résolution des noms via `ProductService` (séquentielle, par ID dédupliqué)
  4. `_localDb.SeedStockAsync(...)` → SQLite seed
  5. `GetMovementsByDelegueAsync(userId)` → mouvements
- Tout est encapsulé dans `ExecuteAsync(async () => {...})` qui gère `IsBusy`

**Stratégie cache** :
```csharp
private const string CacheKeyEchantillon = "stock:echantillon";
private const string CacheKeyPromo       = "stock:promo";
private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);
```
- Interface `ICacheService` avec méthodes `GetOrCreateAsync` et `Invalidate`
- TTL = 5 minutes
- `RefreshAsync()` invalide les deux clés avant de recharger
- ⚠️ Les mouvements ne sont **pas mis en cache** — rechargés à chaque `LoadAsync()`

**Fallback offline** :
```csharp
if (!await CheckConnectivityAsync())
{
    await LoadFromSqliteAsync();
    return;
}
```
- `LoadFromSqliteAsync()` lit depuis SQLite via `_localDb.GetStockAsync()`
- `_promoStock = new()` (liste vide en offline)
- `IsOffline = true` est positionné
- `StockMovements` n'est **pas rempli** en mode offline (liste vide)
- ✅ Fonctionnel pour les échantillons ; ⚠️ mouvements et promo absents offline

---

### 3.2 Segment Échantillons (ActiveSegment == 0)

**Collection liée au XAML** : `StockLines` (ObservableCollection<StockDisplayItem>)

**Comment `NomProduit` est affiché** :
1. `GetStockDelegueAsync()` retourne des `StockDelegue` avec `ProductNom = ""`
2. FIX-5 dans `LoadAsync()` : pour chaque `StockDelegue` avec `ProductNom` vide et `ProductId > 0`, appel `_productSvc.GetProductByIdAsync(ProductId)` → remplit `ProductNom`
3. Les IDs sont dédupliqués (un seul appel API par produit unique)
4. `RefreshDisplayedList()` copie `s.ProductNom` dans `StockDisplayItem.ProductNom`
5. XAML : `Text="{Binding ProductNom}"` ← ✅ correctement affiché si FIX-5 réussit

**Comment `DateExpiration` est affichée** :
```csharp
ExpiryLabel = s.DateExpiration.HasValue
    ? $"Exp. {s.DateExpiration.Value:dd/MM/yyyy}"
    : null,
```
- 🐛 **Bug persistant** : si backend envoie `"dateExpiration": "0001-01-01T00:00:00"` (DateTime par défaut pour stocks sans expiration), `HasValue` est `true` → affiche `"Exp. 01/01/0001"`
- Le guard `HasExpiry` dans `StockDisplayItem` est `ExpiryLabel != null` — ne protège pas contre les dates invalides
- **Fix nécessaire** : ajouter `&& s.DateExpiration.Value.Year > 1` dans la condition

**Comment `QuantiteDisponible` est affiché** :
```csharp
QuantiteLabel    = $"Restant : {s.QuantiteRestante}",
QuantiteRestante = s.QuantiteRestante,
```
- `s.QuantiteRestante` provient de `[JsonPropertyName("qteDisponible")]` ✅
- Affiché dans le badge : `Text="{Binding QuantiteLabel}"` → "Restant : 5"

**CanDistribute** :
```csharp
[JsonIgnore]
public bool CanDistribute => IsEchantillon && QuantiteRestante > 0;
```
- `IsEchantillon = true` pour onglet 0 ✅
- Si `QuantiteRestante <= 0` : bouton désactivé + bordure rouge + alerte "⚠️ Stock insuffisant"

**Bouton Distribuer — flux complet** :
1. Vérification `item.QuantiteRestante <= 0` → `ErrorMessage` si vide
2. `DisplayActionSheet("Distribuer à", "Annuler", null, "Médecin", "Pharmacien")`
3. `DisplayPromptAsync($"ID du {recipientType}", ..., Keyboard.Numeric)` → saisie d'un **ID numérique brut**
4. Parse `int recipientId` — si non numérique → return silencieux
5. Décrémentation locale SQLite : `_localDb.DeductStockAsync(item.ProductId, 1)`
6. Mise à jour de `_echantillonStock` en mémoire : `src.QuantiteRestante -= 1`
7. `RefreshDisplayedList()` → UI mise à jour immédiatement
8. POST asynchrone en fire-and-forget : `Task.Run(() => PostDistributionAsync(...))`
9. Snackbar de confirmation : `"✅ 1 unité de "{nom}" distribuée à {type} #{id}"`

⚠️ **Problème UX** : L'utilisateur saisit l'ID numérique du médecin/pharmacien au clavier. Il n'a aucun moyen de savoir l'ID. Un vrai Picker avec noms serait nécessaire.

**Stock decrementé après distribution ?** :
- ✅ SQLite : immédiatement via `_localDb.DeductStockAsync`
- ✅ Mémoire locale : `src.QuantiteRestante -= 1`
- ✅ Backend : via POST fire-and-forget (erreur loguée mais pas surfacée)
- ⚠️ Si le POST backend échoue, la donnée locale et la donnée serveur divergent — aucune compensation

---

### 3.3 Segment Stock Promo (ActiveSegment == 1)

**Endpoint appelé** : `GetStockPromoAsync()` → `inventory/stocks-promotionnels/echantillon`

**Collection liée** : `StockLines` (même collection que Échantillons, re-remplie)

**Pourquoi `Qté:0` avec bordure rouge** :
```csharp
StockLines.Add(new StockDisplayItem
{
    ProductId        = s.ProductId,
    ProductNom       = s.ProductNom,
    QuantiteLabel    = $"Qté : {s.Quantite}",
    QuantiteRestante = s.Quantite,
    IsEchantillon    = false       ← clé !
});
```
- `CanDistribute = IsEchantillon && QuantiteRestante > 0`
- `IsEchantillon = false` → `CanDistribute` toujours `false`
- Conséquence : bordure rouge + badge fond `DangerLight` sur **tous** les stocks promo, même ceux avec quantité > 0
- Le bouton "Distribuer" est masqué (`IsVisible="{Binding IsEchantillon}"`) → cohérent, pas de distribution depuis promo
- ⚠️ Mais l'alerte "⚠️ Stock insuffisant" est masquée via DataTrigger `IsEchantillon=False` → ✅ OK
- Reste : la **bordure rouge** reste visible même pour des promos avec stock — visuellement trompeur

**NomProduit** : résolu par FIX-5 (même logique que échantillons, IDs partagés dans `productNameCache`)

**DateExpiration** : mappée dans `StockPromo` mais **non utilisée** dans `RefreshDisplayedList()` segment 1 — pas affichée

---

### 3.4 Segment Historique (ActiveSegment == 2)

**Endpoint appelé** : `GetMovementsByDelegueAsync(userId)` → `inventory/stock-movements/by-delegue/{userId}`

**Collection liée** : `StockMovements` (ObservableCollection<StockMouvement>)

**Champs affichés par carte** :
- `DateDay` / `DateYear` (block calendrier)
- `ProductNom` (résolu via lookup stockId)
- `TypeMouvement` (badge coloré)
- `QuantiteLabel` (±N, coloré bleu/rouge)

**DateMouvement** :
- ✅ Désérialisé correctement via `[JsonPropertyName("dateMovement")]`
- Si la date est invalide (year = 1), `DateDay` retourne `"—"` et `DateYear` retourne `""` ✅

**TypeMouvement** :
- ✅ Désérialisé correctement via `[JsonPropertyName("typeMovement")]`
- Valeurs : `"Increment"`, `"Decrement"`, `"Transfer-In"`, `"Transfer-Out"`
- DataTriggers XAML utilisent exactement ces mêmes valeurs ✅
- Badge coloré selon type :
  - `Increment` → fond `PrimaryLight` (bleu clair)
  - `Decrement` → fond `SecondaryLight`
  - `Transfer-In` → fond `AccentLight`
  - `Transfer-Out` → fond `DangerLight` (rouge clair)
- Barre latérale colorée selon type (même logique)

**NomProduit per mouvement** :
```csharp
var nameByStockId = _echantillonStock
    .Where(s => !string.IsNullOrEmpty(s.ProductNom))
    .ToDictionary(s => s.Id, s => s.ProductNom);

m.ProductNom = nameByStockId.TryGetValue(m.IdStock, out var nom)
    ? nom
    : $"Stock #{m.IdStock}";
```
- Résolution par `m.IdStock` → lookup dans les échantillons déjà chargés
- Si le stock n'existe plus dans la liste → `"Stock #{IdStock}"` (fallback visible)

**Quantite positive/négative** :
- Backend envoie `Quantite = -qte` pour les décréments et transferts sortants
- `IsPositive = Quantite >= 0`
- `QuantiteLabel = "+5"` ou `"-3"` ✅
- Couleur : bleu (`Primary`) pour positif, rouge (`Danger`) pour négatif ✅

---

### 3.5 Changement de segment

```csharp
[RelayCommand]
private void SetSegment(string segment)
{
    if (int.TryParse(segment, out var s)) ActiveSegment = s;
}

partial void OnActiveSegmentChanged(int value) => RefreshDisplayedList();
```

- `SetSegmentCommand` appelé depuis les boutons XAML avec `CommandParameter="0"`, `"1"`, `"2"`
- `ActiveSegment` changé → `OnActiveSegmentChanged` → `RefreshDisplayedList()`
- `RefreshDisplayedList()` re-remplit `StockLines` depuis les listes en mémoire (`_echantillonStock` ou `_promoStock`)
- **Les données ne sont pas rechargées** depuis l'API au changement d'onglet — seulement `StockLines` est reconstruit depuis les données en mémoire déjà chargées ✅ (performant)
- `IsStockSegment = ActiveSegment <= 1` → segments 0 et 1 partagent le même `RefreshView/CollectionView`
- `IsHistorySegment = ActiveSegment == 2` → affiché via second `RefreshView` (en `Grid.Row="4"`)

---

═══════════════════════════════════════════════════════════════
## PARTIE 4 — MYSTOCKPAGE XAML
═══════════════════════════════════════════════════════════════

### 4.1 Template — Segments 0 & 1 (StockDisplayItem)

**RefreshView** : `IsVisible="{Binding IsStockSegment}"` — visible pour segments 0 et 1
**CollectionView** : `ItemsSource="{Binding StockLines}"`, `x:DataType="models:StockDisplayItem"`

**Champs affichés et leurs bindings** :

| Élément XAML               | Binding                    | Valeur attendue              | Problème |
|----------------------------|----------------------------|------------------------------|----------|
| Nom du produit             | `{Binding ProductNom}`     | Nom résolu via ProductService | ✅ OK si FIX-5 réussit |
| Badge quantité             | `{Binding QuantiteLabel}`  | `"Restant : 5"` ou `"Qté : 3"` | ✅ OK |
| Couleur badge              | DataTrigger `CanDistribute`| PrimaryLight / DangerLight   | ✅ OK |
| Bordure carte              | DataTrigger `CanDistribute`| BorderColor / Danger         | ⚠️ Toujours rouge pour promo |
| Date expiration            | `{Binding ExpiryLabel}`    | `"Exp. 15/03/2026"` ou masqué | ⚠️ Peut afficher "Exp. 01/01/0001" |
| Visibilité date exp.       | `IsVisible="{Binding HasExpiry}"` | `ExpiryLabel != null`  | ✅ masqué si null |
| Barre de progression       | `{Binding ProgressValue}`  | ❌ Propriété ABSENTE          | 🐛 Binding error silencieuse |
| Alerte stock vide          | `IsVisible="{Binding CanDistribute, Converter=Inverted}"` | `true` si vide | ✅ OK (masqué pour promo via DataTrigger) |
| Bouton Distribuer          | `IsVisible="{Binding IsEchantillon}"` | `true` pour onglet 0 | ✅ Masqué pour promo |
| Bouton Distribuer activé   | `IsEnabled="{Binding CanDistribute}"` | `false` si stock = 0 | ✅ OK |

**Champs manquants** (non affichés) :
- `NumeroLot` — présent dans `StockDisplayItem` mais non affiché dans la carte
- Date d'expiration pour les promos (segment 1)

---

### 4.2 Template — Segment Historique (StockMouvement)

**RefreshView** : `IsVisible="{Binding IsHistorySegment}"` — visible pour segment 2
**CollectionView** : `ItemsSource="{Binding StockMovements}"`, `x:DataType="inv:StockMouvement"`

**Champs affichés et leurs bindings** :

| Élément XAML         | Binding                      | Valeur attendue           | État   |
|----------------------|------------------------------|---------------------------|--------|
| Date (dd/MM)         | `{Binding DateDay}`          | `"15/03"` ou `"—"`        | ✅ OK  |
| Date (yyyy)          | `{Binding DateYear}`         | `"2025"` ou `""`          | ✅ OK  |
| Nom produit          | `{Binding ProductNom}`       | Résolu via stockId lookup  | ✅ OK  |
| Type mouvement       | `{Binding TypeMouvement}`    | `"Increment"` etc.        | ✅ OK  |
| Badge fond type      | DataTrigger `TypeMouvement`  | PrimaryLight/DangerLight  | ✅ OK  |
| Barre latérale couleur| DataTrigger `TypeMouvement` | Primary/Secondary/Danger  | ✅ OK  |
| Quantité signée      | `{Binding QuantiteLabel}`    | `"+5"` ou `"-3"`          | ✅ OK  |
| Couleur quantité     | DataTrigger `IsPositive`     | Primary / Danger          | ✅ OK  |

**Champs non affichés** :
- `Description` — présent dans `StockMouvement` mais aucun binding XAML
- `IdStock` — interne, pas d'affichage

---

### 4.3 Flux bouton Distribuer

```
[Tap "Distribuer"]
       ↓
[item.QuantiteRestante <= 0?] ──YES──→ ErrorMessage affiché, return
       ↓ NO
[DisplayActionSheet "Distribuer à" → "Médecin" | "Pharmacien"]
       ↓
[DisplayPromptAsync "ID du Médecin/Pharmacien" (clavier numérique)]
       ↓
[Parse int recipientId — si échec → return silencieux]
       ↓
[_localDb.DeductStockAsync(item.ProductId, 1)]
       ↓
[_echantillonStock[matching].QuantiteRestante -= 1]
       ↓
[RefreshDisplayedList() → UI mis à jour]
       ↓
[Connectivity OK?] ──YES──→ Task.Run(PostDistributionAsync(stockId, 1, lot, ...))
       ↓ NO (offline)     └── Erreur loguée mais pas surfacée
[Snackbar "✅ 1 unité de X distribuée à Médecin #42"]
```

**UX problème majeur** : L'utilisateur doit connaître l'ID interne du médecin/pharmacien. Il n'existe pas de picker avec noms. Un délégué réel ne connaît pas ces IDs.

---

═══════════════════════════════════════════════════════════════
## PARTIE 5 — BUGS TROUVÉS
═══════════════════════════════════════════════════════════════

| #  | Segment        | Fichier                   | Issue                                    | Root Cause                                                                                        | Impact          | Priorité |
|----|----------------|---------------------------|------------------------------------------|---------------------------------------------------------------------------------------------------|-----------------|----------|
| B1 | Échantillons   | `StockDelegue.cs`         | NomProduit vide au chargement initial    | Backend ne renvoie pas `nomProduit` dans `StockDelegueDto` ; FIX-5 appliqué mais appels séquentiels | Haute visibilité | P1 ✅ résolu |
| B2 | Stock Promo    | `StockPromo.cs`           | NomProduit vide                          | Idem B1 ; FIX-5 appliqué                                                                          | Haute visibilité | P1 ✅ résolu |
| B3 | Historique     | `StockMouvement.cs`       | NomProduit vide                          | Résolu via lookup `stockId→productName` dans ViewModel                                             | Haute visibilité | P1 ✅ résolu |
| B4 | Échantillons   | `MyStockViewModel.cs`     | DateExpiration affiche "Exp. 01/01/0001" | Backend envoie `"0001-01-01T00:00:00"` pour stocks sans expiration ; `HasValue = true` sur `DateTime?` ; guard `Year > 1` absent dans `RefreshDisplayedList()` | Visibilité encadrant | P2 🐛 NON résolu |
| B5 | Stock Promo    | `MyStockViewModel.cs`     | Idem B4 si DateExpiration jamais affichée| Non affiché en promo → impact nul pour l'instant                                                  | Aucun actuellement | P3 |
| B6 | Tous segments  | `StockDisplayItem.cs`     | `ProgressBar` toujours à 0%             | Propriété `ProgressValue` absente de `StockDisplayItem` ; binding XAML silencieusement ignoré      | Visuel           | P2 🐛 NON résolu |
| B7 | Stock Promo    | `MyStockPage.xaml`        | Bordure rouge sur toutes les cartes promo | `CanDistribute = IsEchantillon && QuantiteRestante > 0` ; `IsEchantillon = false` → toujours false → rouge | Trompeur         | P3 🐛 NON résolu |
| B8 | Historique     | (avant correction)        | DateMouvement affichait "01/01/0001"     | Absence de `[JsonPropertyName("dateMovement")]` — French "Mouvement" ≠ English "Movement"         | ✅ Résolu         | — |
| B9 | Historique     | (avant correction)        | TypeMouvement vide, pas de couleurs      | Absence de `[JsonPropertyName("typeMovement")]`                                                    | ✅ Résolu         | — |
| B10| Distribution   | `MyStockViewModel.cs`     | Saisie d'ID brut (pas de Picker)         | `DisplayPromptAsync` avec clavier numérique — aucun picker de médecins/pharmaciens                 | UX pauvre        | P2 |
| B11| Distribution   | `MyStockViewModel.cs`     | Décrémentation par `ProductId` pas `StockId` | `_localDb.DeductStockAsync(item.ProductId, 1)` — si même produit a plusieurs lots, mauvais lot décrémenté | Logique incorrecte | P1 |
| B12| Offline        | `MyStockViewModel.cs`     | Mouvements absents en offline            | `StockMovements` non peuplé dans `LoadFromSqliteAsync()`                                           | Dégradé acceptable | P3 |
| B13| Tous           | `MyStockViewModel.cs`     | userId invalide → mouvements silencieusement absents | Pas de `DisplayAlert` si `int.TryParse(userIdStr)` échoue                             | Débogage difficile | P3 |

---

═══════════════════════════════════════════════════════════════
## PARTIE 6 — PLAN DE CORRECTION
═══════════════════════════════════════════════════════════════

### Fix B4 — DateExpiration affiche "01/01/0001" (PRIORITÉ P2)

**Fichier** : `Cynapharm-Mobile/ViewModels/Stock/MyStockViewModel.cs`
**Méthode** : `RefreshDisplayedList()`

```csharp
// AVANT (bugué) :
ExpiryLabel = s.DateExpiration.HasValue
    ? $"Exp. {s.DateExpiration.Value:dd/MM/yyyy}"
    : null,

// APRÈS (corrigé) :
ExpiryLabel = s.DateExpiration.HasValue && s.DateExpiration.Value.Year > 1
    ? $"Exp. {s.DateExpiration.Value:dd/MM/yyyy}"
    : null,
```

---

### Fix B6 — ProgressBar toujours à 0% (PRIORITÉ P2)

**Fichier** : `Cynapharm-Mobile/Models/Inventory/StockDisplayItem.cs`

```csharp
// Ajouter dans StockDisplayItem :

// Quota alloué au délégué — rempli depuis StockDelegue.QuantiteAllouee (SQLite) ou estimé
public int QuantiteAllouee { get; set; }

[JsonIgnore]
public float ProgressValue => QuantiteAllouee > 0
    ? Math.Clamp((float)QuantiteRestante / QuantiteAllouee, 0f, 1f)
    : 0f;
```

**Fichier** : `Cynapharm-Mobile/ViewModels/Stock/MyStockViewModel.cs` — `RefreshDisplayedList()`

```csharp
// Segment 0 :
StockLines.Add(new StockDisplayItem
{
    StockId          = s.Id,
    NumeroLot        = s.NumeroLot,
    ProductId        = s.ProductId,
    ProductNom       = s.ProductNom,
    QuantiteLabel    = $"Restant : {s.QuantiteRestante}",
    ExpiryLabel      = s.DateExpiration.HasValue && s.DateExpiration.Value.Year > 1
                        ? $"Exp. {s.DateExpiration.Value:dd/MM/yyyy}"
                        : null,
    QuantiteRestante = s.QuantiteRestante,
    QuantiteAllouee  = s.QuantiteAllouee,  // ← ajouté pour ProgressValue
    IsEchantillon    = true
});
```

---

### Fix B7 — Bordure rouge sur les cartes promo (PRIORITÉ P3)

**Fichier** : `Cynapharm-Mobile/Views/Stock/MyStockPage.xaml`

```xml
<!-- Remplacer le DataTrigger CanDistribute=False sur la Border carte : -->
<Border.Triggers>
    <DataTrigger TargetType="Border" Binding="{Binding CanDistribute}" Value="False">
        <!-- Appliquer uniquement si c'est un échantillon, pas une promo -->
    </DataTrigger>
</Border.Triggers>
```

Solution alternative dans `StockDisplayItem.cs` :

```csharp
[JsonIgnore]
public bool ShowLowStockBorder => IsEchantillon && !CanDistribute;
```

Et XAML :
```xml
<Border.Triggers>
    <DataTrigger TargetType="Border" Binding="{Binding ShowLowStockBorder}" Value="True">
        <Setter Property="Stroke" Value="{StaticResource Danger}" />
    </DataTrigger>
</Border.Triggers>
```

---

### Fix B10 — Distribution avec Picker (PRIORITÉ P2)

**Fichier** : `Cynapharm-Mobile/ViewModels/Stock/MyStockViewModel.cs`

```csharp
// Injecter UserService pour charger la liste
private readonly UserService _userSvc;

// Dans DistributeSampleAsync, remplacer DisplayPromptAsync par :
var userList = recipientType == "Médecin"
    ? await _userSvc.GetUsersByRoleAsync("MEDECIN")
    : await _userSvc.GetUsersByRoleAsync("CLIENT");

var names = userList?.Select(u => u.Name).ToArray() ?? Array.Empty<string>();
var selectedName = await Shell.Current.DisplayActionSheet(
    $"Choisir un {recipientType}", "Annuler", null, names);
if (selectedName is null or "Annuler") return;

var selected = userList!.First(u => u.Name == selectedName);
int recipientId = selected.Id;
```

---

### Fix B11 — Décrémentation par StockId pas ProductId (PRIORITÉ P1)

**Fichier** : `Cynapharm-Mobile/Services/LocalDatabaseService.cs`

```csharp
// La méthode DeductStockAsync devrait accepter stockId, pas productId
// Actuellement : DeductStockAsync(item.ProductId, 1)
// Devrait être  : DeductStockAsync(stockId: item.StockId, qty: 1)
```

**Fichier** : `Cynapharm-Mobile/ViewModels/Stock/MyStockViewModel.cs`

```csharp
// Remplacer :
var success = await _localDb.DeductStockAsync(item.ProductId, 1);
// Par :
var success = await _localDb.DeductStockAsync(item.StockId, 1);

// Et :
var src = _echantillonStock.FirstOrDefault(s => s.ProductId == item.ProductId);
// Par :
var src = _echantillonStock.FirstOrDefault(s => s.Id == item.StockId);
```

---

═══════════════════════════════════════════════════════════════
## PARTIE 7 — CODE COMPLET DE CHAQUE FICHIER
═══════════════════════════════════════════════════════════════

---

### MyStockViewModel.cs

```csharp
using System.Collections.ObjectModel;
using System.Net;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Inventory;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Stock;

public partial class MyStockViewModel : BaseViewModel
{
    private readonly InventoryService     _inventoryService;
    private readonly LocalDatabaseService _localDb;
    private readonly ICacheService        _cache;
    private readonly ProductService       _productSvc;

    private const string CacheKeyEchantillon = "stock:echantillon";
    private const string CacheKeyPromo       = "stock:promo";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private List<StockDelegue>  _echantillonStock = new();
    private List<StockPromo>    _promoStock       = new();

    public ObservableCollection<StockDisplayItem> StockLines     { get; } = new();
    public ObservableCollection<StockMouvement>   StockMovements { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStockSegment))]
    [NotifyPropertyChangedFor(nameof(IsHistorySegment))]
    private int _activeSegment;

    public bool IsStockSegment   => ActiveSegment <= 1;
    public bool IsHistorySegment => ActiveSegment == 2;

    public MyStockViewModel(
        InventoryService inventoryService,
        LocalDatabaseService localDb,
        ICacheService cache,
        ProductService productSvc)
    {
        _inventoryService = inventoryService;
        _localDb          = localDb;
        _cache            = cache;
        _productSvc       = productSvc;
        Title = "Mon Stock";
    }

    partial void OnActiveSegmentChanged(int value) => RefreshDisplayedList();

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (!await CheckConnectivityAsync())
        {
            await LoadFromSqliteAsync();
            return;
        }

        await ExecuteAsync(async () =>
        {
            _echantillonStock = await _cache.GetOrCreateAsync(
                CacheKeyEchantillon,
                async () => await _inventoryService.GetStockDelegueAsync(),
                CacheTtl) ?? new();

            // Promo stocks — 404 means no promo data for this tenant; show empty list, not error
            try
            {
                _promoStock = await _cache.GetOrCreateAsync(
                    CacheKeyPromo,
                    async () => await _inventoryService.GetStockPromoAsync(),
                    CacheTtl) ?? new();
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _promoStock = new();
            }

            // ── FIX-5: resolve product names from ProductService ─────────────
            // Backend StockDelegueDto does not include nomProduit.
            // Deduplicate by ProductId to avoid redundant API calls.
            var echantillonIdsToResolve = _echantillonStock
                .Where(s => string.IsNullOrEmpty(s.ProductNom) && s.ProductId > 0)
                .Select(s => s.ProductId)
                .Distinct()
                .ToList();

            var productNameCache = new Dictionary<int, string>();
            foreach (var pid in echantillonIdsToResolve)
            {
                var product = await _productSvc.GetProductByIdAsync(pid);
                productNameCache[pid] = product?.Nom ?? $"Produit #{pid}";
            }
            foreach (var s in _echantillonStock.Where(s => string.IsNullOrEmpty(s.ProductNom) && productNameCache.ContainsKey(s.ProductId)))
                s.ProductNom = productNameCache[s.ProductId];

            // Same for promo stock
            var promoIdsToResolve = _promoStock
                .Where(s => string.IsNullOrEmpty(s.ProductNom) && s.ProductId > 0)
                .Select(s => s.ProductId)
                .Distinct()
                .Except(productNameCache.Keys)
                .ToList();
            foreach (var pid in promoIdsToResolve)
            {
                var product = await _productSvc.GetProductByIdAsync(pid);
                productNameCache[pid] = product?.Nom ?? $"Produit #{pid}";
            }
            foreach (var s in _promoStock.Where(s => string.IsNullOrEmpty(s.ProductNom) && productNameCache.ContainsKey(s.ProductId)))
                s.ProductNom = productNameCache[s.ProductId];
            // ─────────────────────────────────────────────────────────────────

            await _localDb.SeedStockAsync(_echantillonStock);

            var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
            if (int.TryParse(userIdStr, out var userId))
            {
                var movements = await _inventoryService.GetMovementsByDelegueAsync(userId);
                StockMovements.Clear();
                if (movements != null)
                {
                    // Build a quick stockId→productName lookup from the already-resolved echantillon list
                    var nameByStockId = _echantillonStock
                        .Where(s => !string.IsNullOrEmpty(s.ProductNom))
                        .ToDictionary(s => s.Id, s => s.ProductNom);

                    foreach (var m in movements)
                    {
                        if (string.IsNullOrEmpty(m.ProductNom))
                        {
                            m.ProductNom = nameByStockId.TryGetValue(m.IdStock, out var nom)
                                ? nom
                                : $"Stock #{m.IdStock}";
                        }
                        StockMovements.Add(m);
                    }
                }
            }

            RefreshDisplayedList();
        });
    }

    [RelayCommand]
    private Task RefreshAsync()
    {
        _cache.Invalidate(CacheKeyEchantillon);
        _cache.Invalidate(CacheKeyPromo);
        return LoadAsync();
    }

    protected override Task RetryAsync() => LoadAsync();

    [RelayCommand]
    private void SetSegment(string segment)
    {
        if (int.TryParse(segment, out var s)) ActiveSegment = s;
    }

    [RelayCommand]
    private async Task DistributeSampleAsync(StockDisplayItem? item)
    {
        if (item == null) return;

        if (item.QuantiteRestante <= 0)
        {
            ErrorMessage = "⚠️ Stock insuffisant pour ce lot.";
            return;
        }

        // ── Ask for recipient before committing any local change ──────────────
        var recipientType = await Shell.Current.DisplayActionSheet(
            "Distribuer à", "Annuler", null, "Médecin", "Pharmacien");
        if (recipientType is null or "Annuler") return;

        var recipientIdStr = await Shell.Current.DisplayPromptAsync(
            $"ID du {recipientType}",
            $"Saisissez l'identifiant du {recipientType} :",
            keyboard: Keyboard.Numeric);
        if (!int.TryParse(recipientIdStr, out var recipientId)) return;

        int? idMedecin    = recipientType == "Médecin"    ? recipientId : (int?)null;
        int? idPharmacien = recipientType == "Pharmacien" ? recipientId : (int?)null;
        // ─────────────────────────────────────────────────────────────────────

        ClearError();

        var success = await _localDb.DeductStockAsync(item.ProductId, 1);
        if (!success)
        {
            ErrorMessage = "⚠️ Stock insuffisant pour ce lot.";
            return;
        }

        var src = _echantillonStock.FirstOrDefault(s => s.ProductId == item.ProductId);
        if (src != null) src.QuantiteRestante = Math.Max(0, src.QuantiteRestante - 1);

        RefreshDisplayedList();

        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _inventoryService.PostDistributionAsync(
                        item.StockId, 1, item.NumeroLot, idMedecin, idPharmacien);
                }
                catch (Exception ex)
                {
                    Logger?.LogError($"Distribution POST failed for product {item.ProductId}", ex, nameof(MyStockViewModel));
                }
            });
        }

        HapticService.Success();
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var snackbar = Snackbar.Make(
                $"✅ 1 unité de \"{item.ProductNom}\" distribuée à {recipientType} #{recipientId}",
                duration: TimeSpan.FromSeconds(3));
            await snackbar.Show();
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void RefreshDisplayedList()
    {
        StockLines.Clear();
        if (ActiveSegment == 0)
        {
            foreach (var s in _echantillonStock)
                StockLines.Add(new StockDisplayItem
                {
                    StockId          = s.Id,
                    NumeroLot        = s.NumeroLot,
                    ProductId        = s.ProductId,
                    ProductNom       = s.ProductNom,
                    QuantiteLabel    = $"Restant : {s.QuantiteRestante}",
                    ExpiryLabel      = s.DateExpiration.HasValue
                                        ? $"Exp. {s.DateExpiration.Value:dd/MM/yyyy}"
                                        : null,
                    QuantiteRestante = s.QuantiteRestante,
                    IsEchantillon    = true
                });
        }
        else if (ActiveSegment == 1)
        {
            foreach (var s in _promoStock)
                StockLines.Add(new StockDisplayItem
                {
                    ProductId        = s.ProductId,
                    ProductNom       = s.ProductNom,
                    QuantiteLabel    = $"Qté : {s.Quantite}",
                    QuantiteRestante = s.Quantite,
                    IsEchantillon    = false
                });
        }
    }

    private async Task LoadFromSqliteAsync()
    {
        try
        {
            var entries = await _localDb.GetStockAsync();
            _echantillonStock = entries.Select(e => new StockDelegue
            {
                Id               = e.Id,
                ProductId        = e.ProductId,
                ProductNom       = e.ProductNom,
                QuantiteAllouee  = e.QuantiteAllouee,
                QuantiteRestante = e.QuantiteRestante,
                DateExpiration   = e.DateExpirationTicks.HasValue
                                    ? new DateTime(e.DateExpirationTicks.Value)
                                    : null
            }).ToList();
            _promoStock = new();
            RefreshDisplayedList();
            IsOffline = true;
        }
        catch { /* show empty list on failure */ }
    }
}
```

---

### MyStockPage.xaml

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:Cynapharm_Mobile.ViewModels.Stock"
             xmlns:models="clr-namespace:Cynapharm_Mobile.Models.Inventory"
             xmlns:inv="clr-namespace:Cynapharm_Mobile.Models.Inventory"
             xmlns:controls="clr-namespace:Cynapharm_Mobile.Controls"
             x:Class="Cynapharm_Mobile.Views.Stock.MyStockPage"
             x:DataType="vm:MyStockViewModel"
             Title="{Binding Title}"
             Shell.NavBarIsVisible="False"
             BackgroundColor="{StaticResource PageBackground}">

    <Grid RowDefinitions="Auto,Auto,Auto,Auto,*">

        <!-- ── Header ─────────────────────────────────────────────────────── -->
        <controls:AppHeader Grid.Row="0"
                            Title="Mon stock"
                            Subtitle="Échantillons, promos et historique"
                            ShowHamburger="True" />

        <!-- Error banner -->
        <controls:ErrorBanner Grid.Row="1"
                              Message="{Binding ErrorMessage}"
                              Margin="16,8,16,0" />

        <!-- ── Tab bar ───────────────────────────────────────────────────── -->
        <Grid Grid.Row="2"
              ColumnDefinitions="*,*,*"
              BackgroundColor="{StaticResource CardBackground}">

            <VerticalStackLayout Grid.Column="0" Spacing="0">
                <Button Text="Échantillons"
                        Command="{Binding SetSegmentCommand}" CommandParameter="0"
                        BackgroundColor="Transparent"
                        HeightRequest="48" FontSize="13" Margin="0">
                    <Button.Triggers>
                        <DataTrigger TargetType="Button" Binding="{Binding ActiveSegment}" Value="0">
                            <Setter Property="TextColor"     Value="{StaticResource Primary}" />
                            <Setter Property="FontAttributes" Value="Bold" />
                        </DataTrigger>
                        <DataTrigger TargetType="Button" Binding="{Binding ActiveSegment}" Value="1">
                            <Setter Property="TextColor" Value="{StaticResource TextSecondary}" />
                        </DataTrigger>
                        <DataTrigger TargetType="Button" Binding="{Binding ActiveSegment}" Value="2">
                            <Setter Property="TextColor" Value="{StaticResource TextSecondary}" />
                        </DataTrigger>
                    </Button.Triggers>
                </Button>
                <BoxView HeightRequest="2.5" CornerRadius="2">
                    <BoxView.Triggers>
                        <DataTrigger TargetType="BoxView" Binding="{Binding ActiveSegment}" Value="0">
                            <Setter Property="BackgroundColor" Value="{StaticResource Primary}" />
                        </DataTrigger>
                        <DataTrigger TargetType="BoxView" Binding="{Binding ActiveSegment}" Value="1">
                            <Setter Property="BackgroundColor" Value="Transparent" />
                        </DataTrigger>
                        <DataTrigger TargetType="BoxView" Binding="{Binding ActiveSegment}" Value="2">
                            <Setter Property="BackgroundColor" Value="Transparent" />
                        </DataTrigger>
                    </BoxView.Triggers>
                </BoxView>
            </VerticalStackLayout>

            <VerticalStackLayout Grid.Column="1" Spacing="0">
                <Button Text="Stock Promo"
                        Command="{Binding SetSegmentCommand}" CommandParameter="1"
                        BackgroundColor="Transparent"
                        HeightRequest="48" FontSize="13" Margin="0">
                    <Button.Triggers>
                        <DataTrigger TargetType="Button" Binding="{Binding ActiveSegment}" Value="0">
                            <Setter Property="TextColor" Value="{StaticResource TextSecondary}" />
                        </DataTrigger>
                        <DataTrigger TargetType="Button" Binding="{Binding ActiveSegment}" Value="1">
                            <Setter Property="TextColor"     Value="{StaticResource Primary}" />
                            <Setter Property="FontAttributes" Value="Bold" />
                        </DataTrigger>
                        <DataTrigger TargetType="Button" Binding="{Binding ActiveSegment}" Value="2">
                            <Setter Property="TextColor" Value="{StaticResource TextSecondary}" />
                        </DataTrigger>
                    </Button.Triggers>
                </Button>
                <BoxView HeightRequest="2.5" CornerRadius="2">
                    <BoxView.Triggers>
                        <DataTrigger TargetType="BoxView" Binding="{Binding ActiveSegment}" Value="0">
                            <Setter Property="BackgroundColor" Value="Transparent" />
                        </DataTrigger>
                        <DataTrigger TargetType="BoxView" Binding="{Binding ActiveSegment}" Value="1">
                            <Setter Property="BackgroundColor" Value="{StaticResource Primary}" />
                        </DataTrigger>
                        <DataTrigger TargetType="BoxView" Binding="{Binding ActiveSegment}" Value="2">
                            <Setter Property="BackgroundColor" Value="Transparent" />
                        </DataTrigger>
                    </BoxView.Triggers>
                </BoxView>
            </VerticalStackLayout>

            <VerticalStackLayout Grid.Column="2" Spacing="0">
                <Button Text="Historique"
                        Command="{Binding SetSegmentCommand}" CommandParameter="2"
                        BackgroundColor="Transparent"
                        HeightRequest="48" FontSize="13" Margin="0">
                    <Button.Triggers>
                        <DataTrigger TargetType="Button" Binding="{Binding ActiveSegment}" Value="0">
                            <Setter Property="TextColor" Value="{StaticResource TextSecondary}" />
                        </DataTrigger>
                        <DataTrigger TargetType="Button" Binding="{Binding ActiveSegment}" Value="1">
                            <Setter Property="TextColor" Value="{StaticResource TextSecondary}" />
                        </DataTrigger>
                        <DataTrigger TargetType="Button" Binding="{Binding ActiveSegment}" Value="2">
                            <Setter Property="TextColor"     Value="{StaticResource Primary}" />
                            <Setter Property="FontAttributes" Value="Bold" />
                        </DataTrigger>
                    </Button.Triggers>
                </Button>
                <BoxView HeightRequest="2.5" CornerRadius="2">
                    <BoxView.Triggers>
                        <DataTrigger TargetType="BoxView" Binding="{Binding ActiveSegment}" Value="0">
                            <Setter Property="BackgroundColor" Value="Transparent" />
                        </DataTrigger>
                        <DataTrigger TargetType="BoxView" Binding="{Binding ActiveSegment}" Value="1">
                            <Setter Property="BackgroundColor" Value="Transparent" />
                        </DataTrigger>
                        <DataTrigger TargetType="BoxView" Binding="{Binding ActiveSegment}" Value="2">
                            <Setter Property="BackgroundColor" Value="{StaticResource Primary}" />
                        </DataTrigger>
                    </BoxView.Triggers>
                </BoxView>
            </VerticalStackLayout>
        </Grid>

        <ActivityIndicator Grid.Row="3"
                           IsRunning="{Binding IsBusy}"
                           IsVisible="{Binding IsBusy}"
                           Color="{StaticResource Primary}"
                           HorizontalOptions="Center"
                           Margin="0,8" />

        <!-- ── Stock list (segments 0 & 1) ──────────────────────────────── -->
        <RefreshView Grid.Row="4"
                     Command="{Binding RefreshCommand}"
                     IsRefreshing="{Binding IsRefreshing}"
                     IsVisible="{Binding IsStockSegment}"
                     RefreshColor="{StaticResource Primary}">
            <CollectionView ItemsSource="{Binding StockLines}">
                <CollectionView.Header>
                    <BoxView HeightRequest="8" Color="Transparent" />
                </CollectionView.Header>
                <CollectionView.EmptyView>
                    <VerticalStackLayout VerticalOptions="Center" HorizontalOptions="Center"
                                         Spacing="16" Padding="40" Margin="0,40,0,0">
                        <Border BackgroundColor="{StaticResource PrimaryLight}"
                                StrokeShape="RoundRectangle 50" Stroke="Transparent"
                                WidthRequest="80" HeightRequest="80" HorizontalOptions="Center">
                            <Label Text="📦" FontSize="38"
                                   HorizontalOptions="Center" VerticalOptions="Center" />
                        </Border>
                        <Label Text="Aucun stock disponible"
                               FontSize="16" FontAttributes="Bold"
                               TextColor="{StaticResource TextPrimary}"
                               HorizontalOptions="Center" />
                    </VerticalStackLayout>
                </CollectionView.EmptyView>
                <CollectionView.ItemTemplate>
                    <DataTemplate x:DataType="models:StockDisplayItem">
                        <Border Margin="16,0,16,10"
                                BackgroundColor="{StaticResource CardBackground}"
                                StrokeShape="RoundRectangle 16"
                                Stroke="{StaticResource BorderColor}" StrokeThickness="1">
                            <Border.Shadow>
                                <Shadow Brush="Black" Offset="0,2" Radius="6" Opacity="0.05" />
                            </Border.Shadow>
                            <Border.Triggers>
                                <DataTrigger TargetType="Border" Binding="{Binding CanDistribute}" Value="False">
                                    <Setter Property="Stroke" Value="{StaticResource Danger}" />
                                </DataTrigger>
                            </Border.Triggers>
                            <VerticalStackLayout Spacing="12" Padding="14,14">
                                <!-- Product name + quantity badge -->
                                <Grid ColumnDefinitions="*,Auto" ColumnSpacing="10">
                                    <Label Grid.Column="0"
                                           Text="{Binding ProductNom}"
                                           FontAttributes="Bold" FontSize="15"
                                           FontFamily="OpenSansSemibold"
                                           TextColor="{StaticResource TextPrimary}" />
                                    <Border Grid.Column="1"
                                            Padding="12,5"
                                            StrokeShape="RoundRectangle 12"
                                            Stroke="Transparent" VerticalOptions="Center">
                                        <Border.Triggers>
                                            <DataTrigger TargetType="Border" Binding="{Binding CanDistribute}" Value="True">
                                                <Setter Property="BackgroundColor" Value="{StaticResource PrimaryLight}" />
                                            </DataTrigger>
                                            <DataTrigger TargetType="Border" Binding="{Binding CanDistribute}" Value="False">
                                                <Setter Property="BackgroundColor" Value="{StaticResource DangerLight}" />
                                            </DataTrigger>
                                        </Border.Triggers>
                                        <Label Text="{Binding QuantiteLabel}"
                                               FontAttributes="Bold" FontSize="13">
                                            <Label.Triggers>
                                                <DataTrigger TargetType="Label" Binding="{Binding CanDistribute}" Value="True">
                                                    <Setter Property="TextColor" Value="{StaticResource Primary}" />
                                                </DataTrigger>
                                                <DataTrigger TargetType="Label" Binding="{Binding CanDistribute}" Value="False">
                                                    <Setter Property="TextColor" Value="{StaticResource Danger}" />
                                                </DataTrigger>
                                            </Label.Triggers>
                                        </Label>
                                    </Border>
                                </Grid>

                                <!-- Expiry date -->
                                <Grid ColumnDefinitions="Auto,*" ColumnSpacing="6"
                                      IsVisible="{Binding HasExpiry}">
                                    <Label Grid.Column="0" Text="📅" FontSize="12"
                                           VerticalOptions="Center" />
                                    <Label Grid.Column="1"
                                           Text="{Binding ExpiryLabel}"
                                           FontSize="12" TextColor="{StaticResource TextSecondary}"
                                           VerticalOptions="Center" />
                                </Grid>

                                <!-- Progress bar -->
                                <ProgressBar Progress="{Binding ProgressValue}" HeightRequest="6" />

                                <!-- Alert + Distribute -->
                                <Grid ColumnDefinitions="*,Auto" ColumnSpacing="10">
                                    <Border Grid.Column="0"
                                            BackgroundColor="{StaticResource DangerLight}"
                                            StrokeShape="RoundRectangle 8" Stroke="Transparent"
                                            Padding="10,6"
                                            IsVisible="{Binding CanDistribute, Converter={StaticResource InvertedBoolConverter}}">
                                        <Border.Triggers>
                                            <DataTrigger TargetType="Border"
                                                         Binding="{Binding IsEchantillon}" Value="False">
                                                <Setter Property="IsVisible" Value="False" />
                                            </DataTrigger>
                                        </Border.Triggers>
                                        <Label Text="⚠️ Stock insuffisant"
                                               TextColor="{StaticResource Danger}" FontSize="12"
                                               FontAttributes="Bold" />
                                    </Border>

                                    <Button Grid.Column="1"
                                            Text="Distribuer"
                                            FontSize="12"
                                            HeightRequest="40"
                                            CornerRadius="10"
                                            Margin="0"
                                            IsEnabled="{Binding CanDistribute}"
                                            Command="{Binding Source={RelativeSource AncestorType={x:Type ContentPage}}, Path=BindingContext.DistributeSampleCommand}"
                                            CommandParameter="{Binding .}"
                                            IsVisible="{Binding IsEchantillon}">
                                        <Button.Triggers>
                                            <DataTrigger TargetType="Button" Binding="{Binding CanDistribute}" Value="True">
                                                <Setter Property="BackgroundColor" Value="{StaticResource Primary}" />
                                                <Setter Property="TextColor" Value="White" />
                                                <Setter Property="Opacity" Value="1" />
                                            </DataTrigger>
                                            <DataTrigger TargetType="Button" Binding="{Binding CanDistribute}" Value="False">
                                                <Setter Property="BackgroundColor" Value="{StaticResource BorderColor}" />
                                                <Setter Property="TextColor" Value="{StaticResource TextMuted}" />
                                                <Setter Property="Opacity" Value="0.7" />
                                            </DataTrigger>
                                        </Button.Triggers>
                                    </Button>
                                </Grid>
                            </VerticalStackLayout>
                        </Border>
                    </DataTemplate>
                </CollectionView.ItemTemplate>
                <CollectionView.Footer>
                    <BoxView HeightRequest="24" Color="Transparent" />
                </CollectionView.Footer>
            </CollectionView>
        </RefreshView>

        <!-- ── History list (segment 2) ──────────────────────────────────── -->
        <RefreshView Grid.Row="4"
                     Command="{Binding RefreshCommand}"
                     IsRefreshing="{Binding IsRefreshing}"
                     IsVisible="{Binding IsHistorySegment}"
                     RefreshColor="{StaticResource Primary}">
            <CollectionView ItemsSource="{Binding StockMovements}">
                <CollectionView.Header>
                    <BoxView HeightRequest="8" Color="Transparent" />
                </CollectionView.Header>
                <CollectionView.EmptyView>
                    <VerticalStackLayout VerticalOptions="Center" HorizontalOptions="Center"
                                         Spacing="16" Padding="40" Margin="0,40,0,0">
                        <Border BackgroundColor="{StaticResource PrimaryLight}"
                                StrokeShape="RoundRectangle 50" Stroke="Transparent"
                                WidthRequest="80" HeightRequest="80" HorizontalOptions="Center">
                            <Label Text="📋" FontSize="38"
                                   HorizontalOptions="Center" VerticalOptions="Center" />
                        </Border>
                        <Label Text="Aucun mouvement de stock"
                               FontSize="16" FontAttributes="Bold"
                               TextColor="{StaticResource TextPrimary}"
                               HorizontalOptions="Center" />
                    </VerticalStackLayout>
                </CollectionView.EmptyView>
                <CollectionView.ItemTemplate>
                    <DataTemplate x:DataType="inv:StockMouvement">
                        <Border Margin="16,0,16,10"
                                BackgroundColor="{StaticResource CardBackground}"
                                StrokeShape="RoundRectangle 14"
                                Stroke="{StaticResource BorderColor}" StrokeThickness="1"
                                Padding="0">
                            <Border.Shadow>
                                <Shadow Brush="Black" Offset="0,1" Radius="4" Opacity="0.05" />
                            </Border.Shadow>
                            <Grid ColumnDefinitions="5,*">
                                <BoxView Grid.Column="0" VerticalOptions="Fill">
                                    <BoxView.Triggers>
                                        <DataTrigger TargetType="BoxView" Binding="{Binding TypeMouvement}" Value="Increment">
                                            <Setter Property="BackgroundColor" Value="{StaticResource Primary}" />
                                        </DataTrigger>
                                        <DataTrigger TargetType="BoxView" Binding="{Binding TypeMouvement}" Value="Decrement">
                                            <Setter Property="BackgroundColor" Value="{StaticResource Secondary}" />
                                        </DataTrigger>
                                        <DataTrigger TargetType="BoxView" Binding="{Binding TypeMouvement}" Value="Transfer-In">
                                            <Setter Property="BackgroundColor" Value="{StaticResource Accent}" />
                                        </DataTrigger>
                                        <DataTrigger TargetType="BoxView" Binding="{Binding TypeMouvement}" Value="Transfer-Out">
                                            <Setter Property="BackgroundColor" Value="{StaticResource Danger}" />
                                        </DataTrigger>
                                    </BoxView.Triggers>
                                </BoxView>
                                <Grid Grid.Column="1"
                                      ColumnDefinitions="56,*,Auto,Auto"
                                      ColumnSpacing="10" Padding="12,12">
                                    <!-- Date block -->
                                    <Border Grid.Column="0"
                                            BackgroundColor="{StaticResource SurfaceBackground}"
                                            StrokeShape="RoundRectangle 10" Stroke="{StaticResource BorderColor}"
                                            Padding="8,6" VerticalOptions="Center">
                                        <VerticalStackLayout Spacing="1" HorizontalOptions="Center">
                                            <Label Text="{Binding DateDay}"
                                                   FontSize="13" FontAttributes="Bold"
                                                   TextColor="{StaticResource TextPrimary}"
                                                   HorizontalOptions="Center" />
                                            <Label Text="{Binding DateYear}"
                                                   FontSize="10" TextColor="{StaticResource TextMuted}"
                                                   HorizontalOptions="Center" />
                                        </VerticalStackLayout>
                                    </Border>

                                    <!-- Product name -->
                                    <Label Grid.Column="1"
                                           Text="{Binding ProductNom}"
                                           FontSize="13" FontAttributes="Bold"
                                           TextColor="{StaticResource TextPrimary}"
                                           VerticalOptions="Center"
                                           LineBreakMode="TailTruncation" />

                                    <!-- Type badge -->
                                    <Border Grid.Column="2"
                                            Padding="8,4"
                                            StrokeShape="RoundRectangle 8"
                                            Stroke="Transparent"
                                            VerticalOptions="Center">
                                        <Border.Triggers>
                                            <DataTrigger TargetType="Border" Binding="{Binding TypeMouvement}" Value="Increment">
                                                <Setter Property="BackgroundColor" Value="{StaticResource PrimaryLight}" />
                                            </DataTrigger>
                                            <DataTrigger TargetType="Border" Binding="{Binding TypeMouvement}" Value="Decrement">
                                                <Setter Property="BackgroundColor" Value="{StaticResource SecondaryLight}" />
                                            </DataTrigger>
                                            <DataTrigger TargetType="Border" Binding="{Binding TypeMouvement}" Value="Transfer-In">
                                                <Setter Property="BackgroundColor" Value="{StaticResource AccentLight}" />
                                            </DataTrigger>
                                            <DataTrigger TargetType="Border" Binding="{Binding TypeMouvement}" Value="Transfer-Out">
                                                <Setter Property="BackgroundColor" Value="{StaticResource DangerLight}" />
                                            </DataTrigger>
                                        </Border.Triggers>
                                        <Label Text="{Binding TypeMouvement}"
                                               FontSize="10" FontAttributes="Bold"
                                               TextColor="{StaticResource TextPrimary}" />
                                    </Border>

                                    <!-- Quantity (signed, coloured) -->
                                    <Label Grid.Column="3"
                                           Text="{Binding QuantiteLabel}"
                                           FontSize="18" FontAttributes="Bold"
                                           VerticalOptions="Center"
                                           HorizontalOptions="End"
                                           MinimumWidthRequest="30">
                                        <Label.Triggers>
                                            <DataTrigger TargetType="Label" Binding="{Binding IsPositive}" Value="True">
                                                <Setter Property="TextColor" Value="{StaticResource Primary}" />
                                            </DataTrigger>
                                            <DataTrigger TargetType="Label" Binding="{Binding IsPositive}" Value="False">
                                                <Setter Property="TextColor" Value="{StaticResource Danger}" />
                                            </DataTrigger>
                                        </Label.Triggers>
                                    </Label>
                                </Grid>
                            </Grid>
                        </Border>
                    </DataTemplate>
                </CollectionView.ItemTemplate>
                <CollectionView.Footer>
                    <BoxView HeightRequest="24" Color="Transparent" />
                </CollectionView.Footer>
            </CollectionView>
        </RefreshView>

    </Grid>
</ContentPage>
```

---

### MyStockPage.xaml.cs

```csharp
using Cynapharm_Mobile.ViewModels.Stock;

namespace Cynapharm_Mobile.Views.Stock;

public partial class MyStockPage : ContentPage
{
    public MyStockPage() : this(MauiProgram.Services.GetRequiredService<MyStockViewModel>()) { }

    public MyStockPage(MyStockViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MyStockViewModel vm) _ = vm.LoadCommand.ExecuteAsync(null);
    }
}
```

---

### InventoryService.cs

```csharp
using Cynapharm_Mobile.Models.Inventory;

namespace Cynapharm_Mobile.Services;

public class InventoryService
{
    private readonly ApiService _api;
    public InventoryService(ApiService api) { _api = api; }

    public Task<List<StockMouvement>?> GetStockMouvementsAsync(int? productId, DateTime? from)
    {
        var query = "inventory/stock-movements?";
        if (productId.HasValue) query += $"productId={productId}&";
        if (from.HasValue) query += $"from={from.Value:yyyy-MM-dd}&";
        return _api.GetAsync<List<StockMouvement>>(query.TrimEnd('&', '?'));
    }

    public async Task<List<StockDelegue>?> GetStockDelegueAsync()
    {
        var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
        if (!int.TryParse(userIdStr, out var userId)) return null;
        return await _api.GetAsync<List<StockDelegue>>($"inventory/stocks-delegue/by-delegue/{userId}");
    }

    public Task<List<StockPromo>?> GetStockPromoAsync()
        => _api.GetAsync<List<StockPromo>>("inventory/stocks-promotionnels/echantillon");

    public Task<object?> GetDistributionAsync()
        => _api.GetAsync<object>("inventory/distributions");

    /// <summary>
    /// Records a sample distribution on the backend.
    /// Gateway: POST /inventory/distributions → InventoryAPI POST /api/distributions
    /// Exactly one of idMedecin or idPharmacien must be non-null.
    /// </summary>
    public async Task<object?> PostDistributionAsync(
        int stockId,
        int quantite,
        string numeroLot,
        int? idMedecin    = null,
        int? idPharmacien = null)
    {
        var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
        int.TryParse(userIdStr, out var userId);

        var dto = new
        {
            id_Distribution  = 0,
            id_Delegue       = userId,
            id_Medecin       = idMedecin,
            id_Pharmacien    = idPharmacien,
            id_Stock         = stockId,
            qte              = quantite,
            numeroLot        = numeroLot,
            dateDistribution = (DateTime?)null
        };
        return await _api.PostAsync<object>("inventory/distributions", dto);
    }

    // ── New endpoints ─────────────────────────────────────────────────────────

    /// <summary>
    /// GET inventory/inventory-business/summary/{idDelegue}
    /// Returns stock KPIs for the delegate's dashboard.
    /// </summary>
    public Task<StockSummaryDto?> GetStockSummaryAsync(int idDelegue)
        => _api.GetAsync<StockSummaryDto>($"{ApiRoutes.Inventory.StockSummary}/{idDelegue}");

    /// <summary>
    /// GET inventory/stock-movements/by-delegue/{idDelegue}
    /// Returns all stock movements for the delegate's History tab.
    /// </summary>
    public Task<List<StockMouvement>?> GetMovementsByDelegueAsync(int idDelegue)
        => _api.GetAsync<List<StockMouvement>>($"{ApiRoutes.Inventory.MovementsByDelegue}/{idDelegue}");
}
```

---

### StockDisplayItem.cs

```csharp
using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Inventory;

public class StockDisplayItem
{
    // Backend stock identifier — required to build the correct EchantillonDto on distribution
    public int StockId { get; set; }
    public string NumeroLot { get; set; } = string.Empty;

    public int ProductId { get; set; }
    public string ProductNom { get; set; } = string.Empty;
    public string QuantiteLabel { get; set; } = string.Empty;
    public string? ExpiryLabel { get; set; }

    // Raw remaining quantity — used by quota enforcement
    public int QuantiteRestante { get; set; }

    // True for échantillon rows; false for promo stock rows
    public bool IsEchantillon { get; set; }

    [JsonIgnore]
    public bool HasExpiry => ExpiryLabel != null;

    // Distribute button is only active for samples that still have stock
    [JsonIgnore]
    public bool CanDistribute => IsEchantillon && QuantiteRestante > 0;
}
```

---

### StockDelegue.cs

```csharp
using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Inventory;

public class StockDelegue
{
    [JsonPropertyName("id_stock")]
    public int Id { get; set; }

    [JsonPropertyName("id_User_Delegue")]
    public int IdDelegue { get; set; }

    [JsonPropertyName("id_Produit")]
    public int ProductId { get; set; }

    [JsonPropertyName("numeroLot")]
    public string NumeroLot { get; set; } = string.Empty;

    [JsonPropertyName("dateExpiration")]
    public DateTime? DateExpiration { get; set; }

    [JsonPropertyName("qteDisponible")]
    public int QuantiteRestante { get; set; }

    [JsonPropertyName("qteReservee")]
    public int QuantiteReservee { get; set; }

    // Enriched from product catalog — backend StockDelegueDto does not include this field;
    // [JsonPropertyName] is kept as a forward-compat hint in case the API adds it later.
    [JsonPropertyName("nomProduit")]
    public string ProductNom { get; set; } = string.Empty;

    // Kept for offline SQLite compatibility
    public int QuantiteAllouee { get; set; }
}
```

---

### StockPromo.cs

```csharp
using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Inventory;

/// <summary>
/// Represents one row from GET /stocks-promotionnels/echantillon.
/// The backend returns StockEchantillonDto (inherits StockDelegueDto) with
/// underscore-prefixed field names serialised as camelCase.
/// </summary>
public class StockPromo
{
    [JsonPropertyName("id_stock")]
    public int Id { get; set; }

    [JsonPropertyName("id_Produit")]
    public int ProductId { get; set; }

    /// <summary>
    /// Resolved from ProductService after load — not in the backend DTO.
    /// [JsonPropertyName] is a forward-compat hint.
    /// </summary>
    [JsonPropertyName("nomProduit")]
    public string ProductNom { get; set; } = string.Empty;

    /// <summary>Available quantity — maps from qteDisponible.</summary>
    [JsonPropertyName("qteDisponible")]
    public int Quantite { get; set; }

    // Optional echantillon-specific fields — kept for potential future use
    [JsonPropertyName("qteEchantillon")]
    public int QteEchantillon { get; set; }

    [JsonPropertyName("numeroLot")]
    public string NumeroLot { get; set; } = string.Empty;

    [JsonPropertyName("dateExpiration")]
    public DateTime? DateExpiration { get; set; }
}
```

---

### StockMouvement.cs

```csharp
using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Inventory;

public class StockMouvement
{
    // ── Backend fields ────────────────────────────────────────────────────────
    // The backend StockMovementDto uses English "Movement" (not French "Mouvement")
    // — [JsonPropertyName] is mandatory here to bridge that difference.

    [JsonPropertyName("id_Movement")]
    public int Id { get; set; }

    /// <summary>Id of the StockDelegue row this movement belongs to.</summary>
    [JsonPropertyName("id_Stock")]
    public int IdStock { get; set; }

    [JsonPropertyName("quantite")]
    public int Quantite { get; set; }

    /// <summary>Backend values: "Increment", "Decrement", "Transfer-In", "Transfer-Out".</summary>
    [JsonPropertyName("typeMovement")]
    public string TypeMouvement { get; set; } = string.Empty;

    [JsonPropertyName("dateMovement")]
    public DateTime DateMouvement { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    // ── Display-only — resolved after loading, not in the API response ────────

    /// <summary>
    /// Filled in MyStockViewModel after load, by matching IdStock against the
    /// echantillon list (or ProductService for unmatched stocks).
    /// </summary>
    public string ProductNom { get; set; } = string.Empty;

    // ── Computed helpers ──────────────────────────────────────────────────────

    [JsonIgnore]
    public bool IsPositive => Quantite >= 0;

    /// <summary>Shows signed quantity: "+5" for additions, "-3" for deductions.</summary>
    [JsonIgnore]
    public string QuantiteLabel => IsPositive ? $"+{Quantite}" : $"{Quantite}";

    /// <summary>Day/month part: "15/03" — returns "—" when date is unset.</summary>
    [JsonIgnore]
    public string DateDay => DateMouvement.Year > 1
        ? DateMouvement.ToString("dd/MM")
        : "—";

    /// <summary>Year part: "2025" — returns empty string when date is unset.</summary>
    [JsonIgnore]
    public string DateYear => DateMouvement.Year > 1
        ? DateMouvement.ToString("yyyy")
        : string.Empty;

    /// <summary>Full formatted date for tooltips/accessibility — "—" when unset.</summary>
    [JsonIgnore]
    public string DateLabel => DateMouvement.Year > 1
        ? DateMouvement.ToString("dd/MM/yyyy HH:mm")
        : "—";
}
```

---

### StockSummaryDto.cs

```csharp
namespace Cynapharm_Mobile.Models.Inventory;

public class StockSummaryDto
{
    public int    TotalProduits      { get; set; }
    public int    TotalQteDisponible { get; set; }
    public int    StocksVides        { get; set; }
    public int    StocksFaibles      { get; set; }
    public int    TotalDistributions { get; set; }
    public int    TotalQteDistribuee { get; set; }
    public string DernierMouvement   { get; set; } = string.Empty;
}
```

---

### ApiRoutes.cs — section Inventory uniquement

```csharp
public static class Inventory
{
    public const string Stocks             = "api/stocks-delegue";
    public const string Movements          = "api/stock-movements";
    public const string Distributions      = "api/distributions";
    public const string StocksPromo        = "api/stocks-promotionnels";
    public const string Business           = "api/inventory-business";
    public const string StockSummary       = "inventory/inventory-business/summary";
    public const string MovementsByDelegue = "inventory/stock-movements/by-delegue";
    public const string AllDistributions   = "inventory/distributions";
}
```

Note : Les constantes `Stocks`, `Movements`, `Distributions`, `StocksPromo`, `Business` utilisent le préfixe `api/` (accès direct backend, non routé via gateway). Celles avec `inventory/` passent par Ocelot. `GetStockDelegueAsync()` et `GetStockPromoAsync()` dans `InventoryService` utilisent des URLs hardcodées avec `inventory/`, pas ces constantes.

---

### StorageKeys.cs

```csharp
namespace Cynapharm_Mobile;
public static class StorageKeys
{
    public const string JwtToken    = "jwt_token";
    public const string TokenExpiry = "jwt_expiry";
    public const string UserRole    = "user_role";
    public const string UserId      = "user_id";
    public const string UserName    = "user_name";
    public const string UserEmail   = "user_email";

    // Per-user keys — keyed by userId so each account on the device has independent storage.
    public static string UserTelephone(string userId) => $"user_telephone_{userId}";
    public static string UserAdresse(string userId)   => $"user_adresse_{userId}";
}
```

---

*Fin du document — généré à partir des fichiers sources complets, aucun résumé.*
