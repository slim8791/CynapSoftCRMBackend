# Catalogue — Analyse complète (ProductAPI · Angular · MAUI)

> Généré le 2026-05-25. Couvre : backend ProductAPI (C#), frontend Angular (Cynapharm), mobile MAUI (Cynapharm-Mobile).

---

## TABLE DES MATIÈRES

1. [PART 1 — Backend ProductAPI](#part-1--backend-productapi)
   - 1.1 Modèles & relations EF Core
   - 1.2 DTOs
   - 1.3 Interfaces de service
   - 1.4 Services (logique métier)
   - 1.5 Contrôleurs & endpoints
2. [PART 2 — Angular (Cynapharm)](#part-2--angular-cynapharm)
   - 2.1 Routage
   - 2.2 Services Angular
   - 2.3 Composants Produits
   - 2.4 Composants Lots
   - 2.5 Composants Promotions
3. [PART 3 — MAUI (Cynapharm-Mobile)](#part-3--maui-cynapharm-mobile)
   - 3.1 Modèles MAUI
   - 3.2 ProductService MAUI
   - 3.3 ProductListViewModel
   - 3.4 ProductDetailViewModel
   - 3.5 Vues XAML
4. [PART 4 — Analyse globale](#part-4--analyse-globale)
   - 4.1 Tableau des bugs
   - 4.2 Fonctionnalités manquantes
   - 4.3 Plan de correction
   - 4.4 Scénarios de flux de données
5. [Code complet de tous les fichiers lus](#part-5--code-complet)

---

## PART 1 — Backend ProductAPI

### 1.1 Modèles & relations EF Core

#### `Models/Produit.cs`
```csharp
public class Produit {
    public int Id_Produit { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Categorie { get; set; } = string.Empty;
    [Column(TypeName = "decimal(18,2)")] public decimal PrixVente { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Prix_Creation { get; set; }
    public int TVA { get; set; }
    public bool IsArchived { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public virtual ICollection<Lot>? Lots { get; set; }
    public virtual ICollection<Support_Marketting>? Supports { get; set; }
}
```

#### `Models/Lot.cs`
```csharp
public class Lot {
    [Key] public string NumeroLot { get; set; } = string.Empty;   // PK string
    public DateTime DateExpiration { get; set; }
    public int Quantite { get; set; }
    public int Id_Produit { get; set; }
    [ForeignKey("Id_Produit")] public virtual Produit? Produit { get; set; }
    public int? Id_Promo { get; set; }   // colonne orpheline, non utilisée dans la relation EF
    [ForeignKey("Id_Promo")] public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
}
```

#### `Models/Promotion.cs`
```csharp
public class Promotion {
    [Key] public int Id_Promo { get; set; }
    public TypePromotion TypePromotion { get; set; }   // enum: Pourcentage=0, Gratuite=1
    public string CodePromo { get; set; } = string.Empty;
    public float? Pourcentage { get; set; }
    public DateTime? DateDebut { get; set; }
    public DateTime DateExpiration { get; set; }
    public bool EstActive { get; set; } = true;
    public string? NumeroLot { get; set; } = string.Empty;   // FK principal → Lot.NumeroLot
    public virtual Lot? Lot { get; set; }
}
```

#### `Models/Support_Marketting.cs`
```csharp
public class Support_Marketting {
    [Key] public int Id_SupportMarketting { get; set; }
    public string Type { get; set; } = string.Empty;
    public int Id_Produit { get; set; }
    [ForeignKey("Id_Produit")] public virtual Produit? Produit { get; set; }
    public bool IsActive { get; set; } = true;
    public string CampaignName { get; set; } = string.Empty;
    public virtual ICollection<Fichier>? Fichiers { get; set; }
}
```

#### `Models/Fichier.cs`
```csharp
public class Fichier {
    [Key] public int Id_Fichier { get; set; }
    public string NomFichier { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;      // URL Cloudinary stockée directement
    public string Extension { get; set; } = string.Empty;
    public long Taille { get; set; }
    public int Id_Support { get; set; }
    [ForeignKey(nameof(Id_Support))] public virtual Support_Marketting? Support { get; set; }
}
```

#### `Models/TypePromotion.cs`
```csharp
public enum TypePromotion { Pourcentage = 0, Gratuite = 1 }
```

#### Relations EF Core (`Data/AppDbContext.cs` — OnModelCreating)

| Relation | Type | FK | OnDelete |
|---|---|---|---|
| Lot → Produit | HasOne.WithMany | `Id_Produit` | Cascade |
| Promotion → Lot | HasOne.WithMany | `NumeroLot` (PrincipalKey: `Lot.NumeroLot`) | SetNull |
| Support_Marketting → Produit | HasOne.WithMany | `Id_Produit` | Cascade |
| Fichier → Support_Marketting | HasOne.WithMany | `Id_Support` | Cascade |

**Note importante** : La relation Promotion→Lot utilise `Lot.NumeroLot` comme clé principale (HasPrincipalKey) au lieu de la clé primaire standard. Cela signifie qu'une Promotion est liée à un Lot par son numéro de lot (string), pas par un ID entier. La colonne `Lot.Id_Promo` est une relique non utilisée dans la navigation EF réelle.

---

### 1.2 DTOs

#### `ProduitDto`
Miroir de Produit : `Id_Produit`, `Nom`, `Description`, `Categorie`, `PrixVente`, `Prix_Creation`, `TVA`, `IsActive`, `IsArchived`.

#### `LotDto`
```
NumeroLot, DateExpiration, Quantite, Id_Produit
IsExpired (calculé), IsOutOfStock (calculé)
Promotions: List<PromotionDto>?
```

#### `PromotionDto`
```
Id_Promo, TypePromotion, CodePromo, Pourcentage, DateDebut, DateExpiration
EstActive, NumeroLot, IsValid (calculé)
```

#### `SupportMarketingDto`
```
Id_SupportMarketting, Type, Id_Produit, IsActive, CampaignName
Fichiers: List<FichierDto>?
```

#### `FichierDto`
```
Id_Fichier, NomFichier, Url, Extension, Taille, Id_Support
```

#### `ProductDashboardDto`
```
TotalProducts, ActiveProducts, ArchivedProducts, InactiveProducts
TotalStock, LowStockProducts, ExpiringLotsCount, ActivePromotionsCount
```

---

### 1.3 Interfaces de service

#### `IProductService` — 20 méthodes
- Catalogue : `GetAllProductsAsync`, `GetProductByIdAsync`, `GetVisibleProductsAsync`
- Cycle de vie : `CreateOrUpdateProductAsync`, `ArchiveProductAsync`, `UnarchiveProductAsync`, `ActivateProductAsync`, `DeactivateProductAsync`, `DeleteProductAsync`
- Disponibilité : `IsProductAvailableAsync`, `GetAvailableProductsAsync`, `GetUnavailableProductsAsync`
- Stock : `GetTotalStockAsync`, `GetStockStatusAsync`, `GetLowStockProductsAsync`
- Recherche : `SearchProductsAsync`, `FilterProductsAsync`, `GetCategoriesAsync`, `GetProductsByCategoryAsync`
- Règles métier : `ProductExistsAsync`, `IsProductValidAsync`, `CanArchiveProductAsync`
- Pilotage : `GetTopProductsAsync`, `GetProductDashboardAsync`, `GetProductsWithExpiringLotsAsync`, `GetProductsWithActivePromotionsAsync`

#### `ILotService` — 11 méthodes
`GetLotByNumeroAsync`, `GetLotsByProductIdAsync`, `GetAvailableLotsAsync`, `CreateOrUpdateLotAsync`, `DeleteLotAsync`, `AdjustStockAsync` (FEFO), `UpdateLotQuantityAsync`, `IsLotOutOfStockAsync`, `IsLotExpiredAsync`, `GetLotsNearExpirationAsync`, `GetExpiredLotsAsync`, `GetAllLotsAsync`

#### `IPromoService` — 12 méthodes
`GetAllPromotionsAsync`, `GetPromotionByIdAsync`, `CreateOrUpdatePromotionAsync`, `DeletePromotionAsync`, `ApplyBestPromotionAsync`, `IsProductInPromotionAsync`, `GetPromotionsByProductAsync`, `GetPromotionsByLotAsync`, `IsPromotionValidAsync`, `IsPromotionApplicableAsync`, `GetPromotionCoverageRateAsync`, `GetActivePromotionsCountAsync`

#### `IMarkettingService` — 10 méthodes
`GetSupportsByProductAsync`, `GetSupportByIdAsync`, `CreateOrUpdateSupportAsync`, `DisableSupportAsync`, `ActivateSupportAsync`, `AddFileToSupportAsync`, `DeleteFileAsync`, `GetFilesBySupportAsync`, `IsSupportActiveAsync`, `GetVisibleSupportsByProductAsync`, `GetSupportsByCampaignAsync`, `GetCampaignsAsync`

---

### 1.4 Services (logique métier)

#### `ProductService.cs` — points clés

| Méthode | Logique |
|---|---|
| `GetAllProductsAsync` | `.Where(p => !p.IsArchived)` — inclut les inactifs |
| `GetVisibleProductsAsync` | `.Where(p => p.IsActive && !p.IsArchived)` + eager load Lots+Promotions+Supports |
| `CreateOrUpdateProductAsync` | **Retourne null si PrixVente <= 0** (pas de message d'erreur) ; bloque update si IsArchived |
| `ArchiveProductAsync` | Appelle CanArchiveProductAsync (totalStock==0) ; met IsActive=false |
| `UnarchiveProductAsync` | Met IsActive=false — le produit reste inactif après désarchivage, re-activation manuelle obligatoire |
| `DeleteProductAsync` | Exige IsArchived==true && totalStock==0 |
| `IsProductAvailableAsync` | IsActive && !IsArchived && au moins un Lot avec Quantite>0 && DateExpiration>UtcNow |
| `SearchProductsAsync` | Minimum 3 caractères ; cherche dans Nom, Description, Categorie, PrixVente.ToString() |
| `FilterProductsAsync` | 4 filtres cumulables + pagination : keyword, isActive, allowArchived, category |

#### `LotService.cs` — points clés

| Méthode | Logique |
|---|---|
| `AdjustStockAsync` | **FEFO** : `OrderBy(l => l.DateExpiration)` — soustrait des lots en commençant par le plus proche de l'expiration |
| `CreateOrUpdateLotAsync` | Upsert par NumeroLot (string PK) |
| `IsLotExpiredAsync` | Retourne **true si lot non trouvé** (comportement défensif) |
| `IsLotOutOfStockAsync` | Retourne **true si lot non trouvé** (comportement défensif) |
| `GetAvailableLotsAsync` | Lots avec Quantite>0 && DateExpiration>UtcNow |

#### `PromoService.cs` — points clés et bugs

| Méthode | Logique |
|---|---|
| `GetAllPromotionsAsync` | Filtre `Lot != null && NumeroLot != null && DateDebut != null` — **exclut les promotions product-wide** (NumeroLot null) |
| `CreateOrUpdatePromotionAsync` | Vérifie existence du lot ; bloque si lot a déjà une promo EstActive==true (create uniquement) ; **BUG : retourne `promotionDto` (l'entrée) pas l'entité sauvegardée — Id_Promo=0 pour les nouvelles promos** |
| `ApplyBestPromotionAsync` | Seulement TypePromotion.Pourcentage ; prend le % le plus élevé ; **Gratuite ignoré** |
| `IsPromotionValidAsync` | EstActive && DateDebut!=null && DateDebut<=now && DateExpiration>=now |

#### `MarkettingService.cs` — points clés

| Méthode | Logique |
|---|---|
| `AddFileToSupportAsync` | **Lance une exception si Id_Support invalide** — non retourné comme null/bool — propagé au contrôleur |
| `GetCampaignsAsync` | **Pas de filtre IsActive** — retourne les noms de campagnes inactives aussi |
| `GetVisibleSupportsByProductAsync` | Filtre IsActive==true |

---

### 1.5 Contrôleurs & endpoints

#### `ProductController` — `api/products`

| Méthode | Endpoint | Rôles | Description |
|---|---|---|---|
| GET | `/` | [Authorize] | Tous les produits non archivés (inclut inactifs) |
| GET | `/{id}` | [Authorize] | Par ID, non archivé |
| GET | `/visible` | [Authorize] | IsActive && !IsArchived |
| POST | `/` | ADMIN, SUPERVISEUR | Créer ou mettre à jour |
| PUT | `/{id}/archive` | ADMIN | Archiver (exige stock=0) |
| PUT | `/{id}/unarchive` | ADMIN | Désarchiver (met IsActive=false) |
| PUT | `/{id}/activate` | ADMIN, SUPERVISEUR | Activer |
| PUT | `/{id}/deactivate` | ADMIN | Désactiver |
| DELETE | `/{id}` | ADMIN | Suppression physique (exige archivé && stock=0) |
| GET | `/{id}/available` | ADMIN, SUPERVISEUR, DELEGUE | Produit disponible ? |
| GET | `/available` | ADMIN, SUPERVISEUR, DELEGUE | Liste produits disponibles |
| GET | `/unavailable` | ADMIN, SUPERVISEUR | Produits non disponibles |
| GET | `/{id}/stock` | ADMIN, SUPERVISEUR | Stock total par produit |
| GET | `/stock-status` | ADMIN, SUPERVISEUR | Statut stock global (OK/faible/rupture) |
| GET | `/low-stock?seuil=` | ADMIN, SUPERVISEUR | Produits avec stock faible |
| GET | `/search?keyword=&isActive=&allowArchived=&limit=` | [Authorize] | Recherche (min 3 chars) |
| GET | `/filter?keyword=&category=&allowArchived=&isActive=&page=&pageSize=` | ADMIN, SUPERVISEUR, DELEGUE | Filtre paginé |
| GET | `/categories` | [Authorize] | Liste des catégories |
| GET | `/category/{category}` | [Authorize] | Produits par catégorie |
| GET | `/exists?productName=` | ADMIN, SUPERVISEUR | Existence par nom |
| GET | `/{id}/valid` | ADMIN, SUPERVISEUR, DELEGUE | Produit valide ? |
| GET | `/{id}/can-archive` | ADMIN | Archivage possible ? |
| GET | `/top?topN=5` | ADMIN, SUPERVISEUR | Top produits |
| GET | `/dashboard` | ADMIN, SUPERVISEUR | Tableau de bord agrégé |
| GET | `/expiring-lots?days=30` | ADMIN, SUPERVISEUR | Produits avec lots expirant bientôt |
| GET | `/with-promotions` | [Authorize] | Produits ayant des promotions actives |

#### `LotController` — `api/lots`

| Méthode | Endpoint | Rôles | Description |
|---|---|---|---|
| GET | `/product/{id}` | ADMIN, SUPERVISEUR, DELEGUE | Lots d'un produit |
| POST | `/lot` | ADMIN, SUPERVISEUR | Créer/mettre à jour un lot |
| DELETE | `/lot/{numeroLot}` | ADMIN | Supprimer un lot |
| GET | `/` | ADMIN, SUPERVISEUR, DELEGUE | Tous les lots |
| PUT | `/product/{id}/adjust-stock?quantityChange=` | ADMIN, SUPERVISEUR | Ajustement FEFO |
| PUT | `/lot/{numeroLot}/update-quantity?quantityChange=` | ADMIN, SUPERVISEUR | Modification quantité directe |
| GET | `/lot/{numeroLot}/expired` | ADMIN, SUPERVISEUR, DELEGUE | Lot expiré ? |
| GET | `/near-expiration?daysThreshold=` | ADMIN, SUPERVISEUR, DELEGUE | Lots proches expiration |
| GET | `/lot/{numeroLot}` | ADMIN, SUPERVISEUR | Lot par numéro |
| GET | `/product/{id}/available` | ADMIN, SUPERVISEUR, DELEGUE | Lots disponibles d'un produit |
| GET | `/lot/{numeroLot}/out-of-stock` | ADMIN, SUPERVISEUR, DELEGUE | Lot en rupture ? |
| GET | `/expired` | ADMIN, SUPERVISEUR | Tous les lots expirés |

#### `PromoController` — `api/promos`

| Méthode | Endpoint | Rôles | Description |
|---|---|---|---|
| GET | `/` | ADMIN, SUPERVISEUR, DELEGUE | Toutes les promotions (filtre Lot!=null) |
| GET | `/{id}` | ADMIN, SUPERVISEUR, DELEGUE | Par ID |
| POST | `/` | ADMIN, SUPERVISEUR | Créer/mettre à jour |
| DELETE | `/{id}` | ADMIN | Supprimer |
| GET | `/product/{id}/apply?initialPrice=` | [Authorize] | Prix après meilleure promotion % |
| GET | `/product/{id}/in-promotion` | [Authorize] | Produit en promotion ? |
| GET | `/product/{id}` | [Authorize] | Promotions d'un produit |
| GET | `/lot/{numeroLot}` | ADMIN, SUPERVISEUR, DELEGUE | Promotions d'un lot |
| GET | `/{id}/valid` | ADMIN, SUPERVISEUR, DELEGUE | Promotion valide ? |
| GET | `/{id}/applicable?referenceDate=` | ADMIN, SUPERVISEUR, DELEGUE | Applicable à une date ? |
| GET | `/coverage-rate` | ADMIN, SUPERVISEUR | Taux de couverture promotionnelle |
| GET | `/active-count` | ADMIN, SUPERVISEUR | Nombre de promos actives |

#### `MarkettingController` — `api/marketting`

| Méthode | Endpoint | Rôles | Description |
|---|---|---|---|
| GET | `/product/{id}/supports` | ADMIN, SUPERVISEUR, DELEGUE | Supports marketing d'un produit |
| GET | `/support/{id}` | ADMIN, SUPERVISEUR, DELEGUE | Support par ID |
| POST | `/support` | ADMIN, SUPERVISEUR | Créer/mettre à jour un support |
| POST | `/support/file` | ADMIN, SUPERVISEUR | Ajouter un fichier (URL déjà uploadée sur Cloudinary) |
| DELETE | `/file/{id}` | ADMIN | Supprimer un fichier |
| GET | `/support/{id}/files` | ADMIN, SUPERVISEUR, DELEGUE | Fichiers d'un support |
| GET | `/support/{id}/active` | [Authorize] | Support actif ? |
| GET | `/product/{id}/visible-supports` | [Authorize] | Supports visibles d'un produit |
| GET | `/campaign/{name}` | ADMIN, SUPERVISEUR, DELEGUE | Supports d'une campagne |
| GET | `/campaigns` | ADMIN, SUPERVISEUR, DELEGUE | Liste des campagnes |
| PUT | `/support/{id}/disable` | ADMIN | Désactiver un support |
| PUT | `/support/{id}/activate` | ADMIN | Activer un support |

---

## PART 2 — Angular (Cynapharm)

### 2.1 Routage

#### `products-routing.module.ts`
```
/products           → ProductListComponent
/products/new       → ProductFormComponent
/products/:id/edit  → ProductFormComponent
/products/:id       → ProductDetailComponent
```

#### `lots-routing.module.ts` (déduit du code)
```
/lots                    → LotListComponent  (queryParam: productId, productName)
/lots/new                → LotFormComponent
/lots/:numero/edit       → LotFormComponent
/lots/:numero            → LotDetailComponent
```

#### `promotions-routing.module.ts` (déduit du code)
```
/promotions              → PromotionListComponent
/promotions/new          → PromotionFormComponent
/promotions/:id/edit     → PromotionFormComponent
/promotions/analytics    → PromotionAnalyticsComponent
```

---

### 2.2 Services Angular

#### `product.service.ts`
- Route de base (Ocelot) : `/products`
- `getProducts()` → GET `/products` — non archivés, inclut inactifs
- `getProductsAll()` → GET `/products/filter?page=1&pageSize=1000` — tout (actif+inactif+archivé)
- `getVisibleProducts()` → GET `/products/visible`
- `getProductById(id)` → GET `/products/{id}`
- `createProduct(data)` / `updateProduct(_id, data)` → POST `/products` (même endpoint)
- `deleteProduct(id)` → PUT `/products/{id}/deactivate` (naming trompeur en service : "delete" mais désactive)
- `hardDeleteProduct(id)` → DELETE `/products/{id}`
- `activateProduct(id)` → PUT `/products/{id}/activate`
- `archiveProduct(id)` → PUT `/products/{id}/archive`
- `unarchiveProduct(id)` → PUT `/products/{id}/unarchive`
- `searchProducts(keyword, isActive, allowArchived, limit)` → GET `/products/search`
- `filterProducts(page, pageSize, keyword?, isActive?, allowArchived?, category?)` → GET `/products/filter`
- `getLotsByProduct(productId)` → GET `/products/lots/product/{productId}` (**duplique lot.service.ts**)

#### `lot.service.ts`
- Route de base (Ocelot) : `/products/lots`
- `getAllLots()` → GET `/products/lots`
- `getLotsByProductId(productId)` → GET `/products/lots/product/{productId}`
- `getLotByNumero(numero)` → GET `/products/lots/lot/{numero}` — normalise Pascal/camelCase
- `createOrUpdateLot(lot)` → POST `/products/lots/lot` — transforme `idProduit` → `id_Produit`
- `deleteLot(numeroLot)` → DELETE `/products/lots/{numeroLot}`
- Méthode `unwrap()` privée : normalise `Result`/`result`/tableau direct + casing

#### `promotion.service.ts` (Angular)
- Route de base (Ocelot) : `/products/promos`
- `getAll()`, `getById(id)`, `createOrUpdate(dto)`, `delete(id)`
- `getActiveCount()`, `getCoverageRate()`, `isValid(id)`, `isApplicable(id, referenceDate)`
- `getByProduct(productId)`, `isProductInPromotion(productId)`, `applyBest(productId, initialPrice)`
- `getByLot(numeroLot)`
- PromotionDto Angular enrichi : `porteeSurTousLesLots`, `seuilAchat`, `quantiteGratuite` (champs définis mais jamais utilisés dans les formulaires)

---

### 2.3 Composants Produits

#### `ProductListComponent`
**État** : `products[]`, `filteredProducts[]`, KPIs (totalProducts/Active/Inactive/Archived), filtres (searchTerm, statusFilter, categoryFilter), pagination (currentPage, pageSize, totalPages), modal de confirmation, expandedRows/lotsCache (lots en ligne).

**Chargement** : `getProductsAll()` (page=1, pageSize=1000) → charge tout en mémoire. Fallback sur `getProducts()` si 403/404 (DELEGUE/MEDECIN).

**Filtres côté client** :
```
statusFilter 'active' → IsActive && !IsArchived
statusFilter 'inactive' → !IsActive && !IsArchived
statusFilter 'archived' → IsArchived
categoryFilter → Categorie == categoryFilter
searchTerm (≥3 chars) → Nom | Description | Categorie
```

**Actions** : `deactivate`, `archive` (avec vérification stock avant), `unarchive`, `activate`, `harddelete` — toutes avec modal de confirmation.

**Vérification stock avant archivage** : charge les lots du produit (ou utilise le cache) et bloque si stock > 0, avant même d'appeler le backend.

**Lots en ligne** : `toggleRow(id)` → expand/collapse, charge les lots via `LotService.getLotsByProductId()` avec cache.

#### `ProductDetailComponent`
**Onglets** : `info`, `stock`, `lots`, `supports`, `promotions`, `dashboard`

**Chargement en chaîne** : `loadProduct()` → `loadLots()` → `loadSupports()` + `loadInventoryStocks()` → `loadPromotions()`

**Supports marketing** :
- Modal créer/modifier support
- Toggle actif/inactif
- Expand/collapse fichiers par support
- Upload Cloudinary : `cloudinaryService.uploadFile()` → URL → `marketingService.addFileToSupport()`
- Image produit : support de type 'Image' avec IsActive=true ; flux upload : désactiver l'ancien → créer nouveau support → ajouter fichier

**Permissions** : `canManageMarketing = role === ADMIN || role === SUPERVISEUR`

**Helpers** :
- `isFileBroken(file)` : détecte les fichiers non-image uploadés via `/image/upload/` Cloudinary
- `getFileHref(file)` : injecte `fl_attachment` pour les fichiers raw Cloudinary (force téléchargement)
- `isPromoExpired(promo)` : `new Date(exp) < new Date()`

#### `ProductFormComponent`
**Formulaire** : `Nom` (required, max 200), `Description` (max 1000), `Categorie` (required), `Prix_Vente` (required, ≥0), `Prix_Creation` (required, ≥0), `TVA` (0-100), `isActive`.

**Validateur croisé** : `priceOrderValidator` — erreur si `Prix_Vente < Prix_Creation`.

**Catégories** : select avec option `__new__` pour saisir une nouvelle catégorie.

**Submit** : POST `/products` pour create ET update (backend upsert par Id_Produit). Préserve `IsArchived=this.loadedIsArchived` pour ne pas modifier l'état d'archivage.

---

### 2.4 Composants Lots

#### `lot.model.ts`
```typescript
export type LotStatus = 'active' | 'low-stock' | 'out-of-stock' | 'expired';
const LOW_STOCK_THRESHOLD = 5;

// Priorité : expired > out-of-stock > low-stock (≤5) > active
export function getLotStatus(lot: LotDto): LotStatus { ... }
```
Les statuts calculés (`isExpired`, `isOutOfStock`) viennent du backend, jamais recalculés côté front.

#### `LotListComponent`
- Charge tous les lots (`getAllLots()`) ou par produit (`getLotsByProductId(id)`) selon `queryParams.productId`
- Charge les noms de produits en parallèle via `getProducts()`
- KPIs : total, active (getLotStatus='active'), low-stock, expired
- Filtres client : status, recherche sur numéro (min 2 chars)
- Suppression avec modal de confirmation

#### `LotFormComponent`
- Validateur date future : `futureDateValidator` — autorise la date originale en mode édition (lot déjà expiré)
- Pattern lot : `/^[a-zA-Z0-9\-_]+$/`
- Quantite min=1, max=999999
- Numéro désactivé en mode édition
- `getRawValue()` pour inclure les champs disabled

#### `LotDetailComponent`
- `canManagePromo = role === ADMIN || role === SUPERVISEUR`
- Charge stock InventoryAPI via `InventoryService.getStockByLot()`
- `isEditDisabled()` : lot expiré → édition bloquée
- `isDeleteDisabled()` : stock délégué disponible > 0 → suppression bloquée
- Modal de création de promotion (type Pourcentage uniquement)
- Utilise `PromotionAdvancedService.createOrUpdatePromotion()` (pas le service standard)

---

### 2.5 Composants Promotions

#### `PromotionListComponent`
- Charge tous les produits (`getProductsAll()`) pour la map nom-produit
- Filtre **côté client** : `data.filter(p => (p.pourcentage ?? 0) > 0)` — **seules les promotions de type Pourcentage sont affichées**
- Suppression avec `ConfirmDialogComponent`
- `isExpired(promo)` : `new Date(promo.dateExpiration) < new Date()`

#### `PromotionFormComponent`
- Champs : `codePromo`, `pourcentage` (1-100), `numeroLot` (select), `dateDebut`, `dateExpiration`, `estActive`
- Lots disponibles : charge `getAllLots()` et filtre `!isExpired && !isOutOfStock`
- Submit : envoie toujours `typePromotion: 'Pourcentage'`, `porteeSurTousLesLots: false`
- **Le type Gratuite n'est jamais proposé dans l'UI**
- Validateurs : `dateRangeValidator` (debut < expiration), `noWhitespaceValidator` sur codePromo

#### `PromotionAnalyticsComponent`
- `forkJoin` avec timeout 5s sur `getActiveCount()` + `getCoverageRate()`
- Affiche : nombre de promotions actives + taux de couverture

---

## PART 3 — MAUI (Cynapharm-Mobile)

### 3.1 Modèles MAUI

#### `Models/Products/Product.cs`
```csharp
[JsonPropertyName("id_Produit")] public int Id { get; set; }
public string Reference { get; set; } = string.Empty;   // non présent dans backend ProduitDto
public string Nom { get; set; } = string.Empty;
public string? Description { get; set; }
public string Categorie { get; set; } = string.Empty;
[JsonPropertyName("prixVente")] public decimal PrixUnitaire { get; set; }
public bool IsPriceDefined => PrixUnitaire > 0;
public string PrixDisplay => PrixUnitaire > 0 ? PrixUnitaire.ToString("N", _tndFormat) + " TND" : "Prix non défini";
public string? ImageUrl { get; set; }   // injecté par ProductService.ExtractImageUrl
[JsonPropertyName("isActive")] public bool Actif { get; set; }
public bool IsArchived { get; set; }
public List<SupportMarketing>? Supports { get; set; }
```

**Mapping JSON correct** : `id_Produit` → `Id`, `prixVente` → `PrixUnitaire`, `isActive` → `Actif`.

**`Reference`** : champ présent dans le modèle MAUI mais absent de ProduitDto backend — toujours vide.

#### `Models/Products/Lot.cs` — DÉSYNCHRONISÉ
```csharp
public class Lot {
    public int Id { get; set; }              // backend: string NumeroLot (PK string)
    public int ProductId { get; set; }       // backend: int Id_Produit
    public string NumeroLot { get; set; } = string.Empty;
    public DateTime DateFabrication { get; set; }   // N'EXISTE PAS dans le backend LotDto
    public DateTime DateExpiration { get; set; }
    public int QuantiteDisponible { get; set; }      // backend: Quantite (pas QuantiteDisponible)
}
```

**Désynchronisations** :
- `int Id` vs backend string PK `NumeroLot` — toujours 0
- `DateFabrication` — champ inexistant côté backend, toujours `DateTime.MinValue`
- `QuantiteDisponible` vs backend `Quantite` — toujours 0 (désérialisation échoue silencieusement)
- `ProductId` vs backend `Id_Produit` (ou `id_Produit` en camelCase) — toujours 0

#### `Models/Products/Promotion.cs` — FORTEMENT DÉSYNCHRONISÉ
```csharp
public class Promotion {
    public int Id { get; set; }              // backend: Id_Promo
    public int? ProductId { get; set; }      // backend: pas de ProductId direct
    public string Titre { get; set; } = string.Empty;       // N'EXISTE PAS dans backend
    public string? Description { get; set; }                // N'EXISTE PAS dans backend
    public decimal? RemisePourcentage { get; set; }         // backend: float? Pourcentage
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }                   // backend: DateExpiration
}
```

**Désynchronisations** :
- `Titre` → n'existe pas (toujours vide)
- `Description` → n'existe pas (toujours null)
- `RemisePourcentage` → backend `Pourcentage` (float) — toujours null
- `DateFin` → backend `DateExpiration` — toujours `DateTime.MinValue`

**Conséquence** : les promotions s'affichent avec des données vides dans ProductDetailPage.

#### `Models/Products/Product.cs` (classes imbriquées)
```csharp
public class SupportMarketing {
    public string Type { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? CampaignName { get; set; }
    public List<Fichier>? Fichiers { get; set; }
}

public class Fichier {
    public string NomFichier { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
}
```

Ces classes sont correctement désérialisées depuis le backend (champs présents dans ProduitDto/Supports).

---

### 3.2 ProductService MAUI

```csharp
// GET products ou products/search?keyword=...&isActive=true&limit=100
public Task<List<Product>?> GetProductsAsync(string? search = null, int limit = 100)

// GET products/{id} + extraction ImageUrl
public Task<Product?> GetProductByIdAsync(int id)

// GET products/lots/{productId}  ← NOTE: pas de /product/ intermédiaire
public Task<List<Lot>?> GetLotsByProductAsync(int productId)

// GET products/promos?productId={id}  ← MISMATCH avec backend api/promos/product/{id}
public Task<List<Promotion>?> GetPromotionsAsync(int? productId)

// GET products/visible — actifs + non archivés
public Task<List<Product>?> GetVisibleProductsAsync()

// GET products/categories
public Task<List<string>?> GetCategoriesAsync()

// Download Cloudinary file
public Task<byte[]?> DownloadFileAsync(string url)
```

**Extraction ImageUrl** : `ExtractImageUrl(p)` cherche dans `p.Supports` le premier support actif de type "Image" dont le premier fichier a une extension jpg/png/webp.

**Gateway** : `AppSettings.ApiGatewayBaseUrl = "http://cynapharmgateway.runasp.net/"` (Debug/Release).

---

### 3.3 ProductListViewModel

**Rôles & endpoint** :
- `MEDECIN`, `PHARMACIEN`, `GROSSISTE`, `CLIENT` → `GetVisibleProductsAsync()` (actifs+non-archivés)
- `DELEGUE`, `ADMIN` → `GetProductsAsync()` + filtre `!p.IsArchived` côté client

**CanSeePrices** : `false` uniquement pour `MEDECIN` — masque le prix dans la liste et la fiche.

**Recherche** : debounce 300ms, min 3 chars. Si résultat local vide et connecté : appel serveur `GetProductsAsync(query)`.

**Catégories** : chips horizontaux scrollables, `SetSelectedCategoryCommand(category)`.

**Mode hors ligne** : fallback SQLite (`LocalDatabaseService.SearchProductsAsync`). Banner "Mode hors ligne" affiché. `SeedProductsAsync` peuple la base locale après chaque chargement réseau.

**`ShowSearchHint`** : vrai si 1-2 chars saisis — affiche "Entrez au moins 3 caractères".

---

### 3.4 ProductDetailViewModel

**Réception de paramètre** : `[QueryProperty(nameof(ProductId), "productId")]` → `OnProductIdChanged` → `InitAsync`.

**Flux de chargement** :
1. Détermine `CanSeePrices` depuis SecureStorage (role ≠ MEDECIN)
2. `GetProductByIdAsync(ProductId)` — affiche titre, image, supports
3. `GetLotsByProductAsync(ProductId)` — 404/403 swallowed silently (MEDECIN n'a pas accès)
4. `GetPromotionsAsync(ProductId)` — 404/403 swallowed silently

**Supports** : filtre `s.IsActive && type ≠ "Image"` pour la liste des documents. Pour MEDECIN : filtres les fichiers image (jpg/jpeg/png/webp/gif).

**AddToOrder** : MEDECIN → `DisplayAlert("Accès refusé", "Les médecins ne peuvent pas passer commande...")`. Autres → navigate `//orders/create?productId={ProductId}`.

**OpenDocument** : détecte URL Cloudinary raw → injecte `fl_attachment` → télécharge bytes → écrit en cache → `Launcher.OpenAsync` avec MIME type.

**ViewDocument** : navigue vers `///products/detail/viewer` avec url, nomFichier, extension en queryParams.

---

### 3.5 Vues XAML

#### `ProductListPage.xaml`
- Header : gradient Primary, titre "Catalogue", badge compteur produits
- Barre de recherche : `Entry` lié à `SearchQuery` (deux-voies)
- Banner hors ligne : `IsVisible="{Binding IsOffline}"`
- Chips catégories : `BindableLayout` sur `Categories` avec `DataTrigger` pour la sélection active
- Hint recherche : "Entrez au moins 3 caractères" si `ShowSearchHint`
- Liste produits : `CollectionView` avec `RefreshView`, `MeasureFirstItem`
- Carte produit : image produit (ou placeholder 💊), nom, badge catégorie, prix (masqué MEDECIN), badge "✓ Disponible" (visible seulement MEDECIN)
- Tap → `GoToDetailCommand`

#### `ProductDetailPage.xaml`
- Grille 3 lignes : header fixe | corps scrollable | CTA sticky
- **Header** : bouton retour, "Fiche produit", badge Actif/Inactif (masqué MEDECIN via `DataTrigger`)
- **Hero** : 200px — placeholder ou `Product.ImageUrl`
- **Carte nom** : `Nom`, chips catégorie + référence, bloc prix (masqué MEDECIN)
- **Description** : visible si `Product.Description` non vide
- **Informations** (masqué MEDECIN via `HasInformations`) : catégorie, statut, prix vente
- **Banner MEDECIN** : "Pour toute demande d'échantillons, contactez votre délégué Cynapharm."
- **Documents** : `BindableLayout` sur `Supports` → sous-liste `Fichiers` avec boutons view (in-app) + download (Launcher)
- **Lots** (masqué MEDECIN) : `CollectionView` sur `Lots` → `NumeroLot`, `DateExpiration`, `QuantiteDisponible`
- **Promotions** (masqué MEDECIN) : `CollectionView` sur `Promotions` → `Titre`, `DateFin`, `RemisePourcentage`
- **CTA sticky** (masqué MEDECIN) : résumé prix + bouton "Ajouter à commande"

---

## PART 4 — Analyse globale

### 4.1 Tableau des bugs

| # | Module | Fichier | Bug | Impact | Sévérité |
|---|---|---|---|---|---|
| B1 | Backend | `PromoService.cs` | `CreateOrUpdatePromotionAsync` retourne `promotionDto` (input) au lieu de l'entité sauvegardée → `Id_Promo=0` pour les nouvelles promotions | La promotion créée ne peut pas être référencée immédiatement ; Angular/MAUI reçoit Id=0 | HAUTE |
| B2 | Backend | `PromoService.cs` | `GetAllPromotionsAsync` filtre `Lot != null && NumeroLot != null` → les promotions product-wide (NumeroLot null) sont systématiquement exclues | Promotions globales invisibles dans la liste | MOYENNE |
| B3 | Backend | `MarkettingService.cs` | `AddFileToSupportAsync` lève une exception si `Id_Support` invalide au lieu de retourner null/false → propagé au contrôleur non géré | Crash 500 sur upload avec mauvais ID support | MOYENNE |
| B4 | Backend | `MarkettingService.cs` | `GetCampaignsAsync` sans filtre `IsActive` → retourne les noms de campagnes inactives | Données inutiles dans le sélecteur de campagne | FAIBLE |
| B5 | Backend | `ProductService.cs` | `CreateOrUpdateProductAsync` retourne `null` si `PrixVente <= 0` sans message d'erreur | Création silencieusement échouée sans feedback | MOYENNE |
| B6 | MAUI | `Models/Products/Lot.cs` | `int Id` vs string PK, `DateFabrication` inexistant, `QuantiteDisponible` vs `Quantite`, `ProductId` vs `Id_Produit` → désérialisation JSON échoue silencieusement | Lots toujours affichés vides dans ProductDetailPage | HAUTE |
| B7 | MAUI | `Models/Products/Promotion.cs` | `Titre`, `Description`, `RemisePourcentage`, `DateFin` ne correspondent à aucun champ du backend `PromotionDto` | Promotions toujours affichées vides dans ProductDetailPage | HAUTE |
| B8 | MAUI | `Services/ProductService.cs` | `GetPromotionsAsync` appelle `products/promos?productId=X` ; le backend attend `api/promos/product/{id}` (paramètre de chemin, pas querystring) | 404 ou retour vide pour les promotions | HAUTE |
| B9 | Angular | `promotion-list.component.ts` | Filtre `p.pourcentage > 0` → les promotions de type `Gratuite` sont systématiquement masquées | Type Gratuite inaccessible dans la liste | FAIBLE |
| B10 | Angular | `product.service.ts` | `getLotsByProduct(id)` duplique `lot.service.ts` en appelant `/products/lots/product/{id}` — méthode non utilisée dans les composants officiels | Duplication de code, risque de dérive | FAIBLE |

---

### 4.2 Fonctionnalités manquantes

| # | Module | Fonctionnalité | Description |
|---|---|---|---|
| M1 | Angular + Backend | Promotions product-wide (`porteeSurTousLesLots=true`) | Backend supporte, Angular envoie toujours `false`, aucune UI pour créer ce type |
| M2 | Angular + Backend | Promotions de type `Gratuite` | Backend supporte (TypePromotion.Gratuite=1), Angular masque ces promos et ne permet pas leur création |
| M3 | MAUI | Mode hors ligne pour ProductDetail | Seule la liste a un fallback SQLite ; la fiche produit nécessite le réseau |
| M4 | MAUI | Lots avec données réelles | Modèle Lot désynchronisé (B6) → aucune donnée de lot réelle n'atteint l'UI |
| M5 | MAUI | Promotions avec données réelles | Modèle Promotion désynchronisé (B7+B8) → aucune donnée de promo réelle |
| M6 | Angular | Dashboard produit | `GET /products/dashboard` existe mais aucune page Angular dédiée pour l'afficher |
| M7 | Angular | Ajustement de stock (FEFO) | `PUT /lots/product/{id}/adjust-stock` existe mais pas exposé dans l'UI |
| M8 | MAUI | Référence produit | `Product.Reference` défini dans le modèle mais absent du backend ProduitDto — toujours vide |

---

### 4.3 Plan de correction (par priorité)

#### Corrections HAUTE priorité

**Fix MAUI-B6 — Corriger le modèle Lot**
```csharp
// Cynapharm-Mobile/Models/Products/Lot.cs
namespace Cynapharm_Mobile.Models.Products;
public class Lot
{
    [JsonPropertyName("numeroLot")]
    public string NumeroLot { get; set; } = string.Empty;

    [JsonPropertyName("dateExpiration")]
    public DateTime DateExpiration { get; set; }

    [JsonPropertyName("quantite")]
    public int Quantite { get; set; }

    [JsonPropertyName("id_Produit")]
    public int IdProduit { get; set; }

    [JsonPropertyName("isExpired")]
    public bool IsExpired { get; set; }

    [JsonPropertyName("isOutOfStock")]
    public bool IsOutOfStock { get; set; }
}
```

**Fix MAUI-B7 — Corriger le modèle Promotion**
```csharp
// Cynapharm-Mobile/Models/Products/Promotion.cs
namespace Cynapharm_Mobile.Models.Products;
public class Promotion
{
    [JsonPropertyName("id_Promo")]
    public int Id { get; set; }

    [JsonPropertyName("codePromo")]
    public string CodePromo { get; set; } = string.Empty;

    [JsonPropertyName("typePromotion")]
    public string TypePromotion { get; set; } = "Pourcentage";

    [JsonPropertyName("pourcentage")]
    public float? Pourcentage { get; set; }

    [JsonPropertyName("dateDebut")]
    public DateTime? DateDebut { get; set; }

    [JsonPropertyName("dateExpiration")]
    public DateTime DateExpiration { get; set; }

    [JsonPropertyName("estActive")]
    public bool EstActive { get; set; }

    [JsonPropertyName("numeroLot")]
    public string? NumeroLot { get; set; }
}
```

Mettre à jour `ProductDetailPage.xaml` : remplacer `{Binding Titre}` → `{Binding CodePromo}`, `{Binding DateFin}` → `{Binding DateExpiration}`, `{Binding RemisePourcentage, StringFormat='-{0}%'}` → `{Binding Pourcentage, StringFormat='-{0}%'}`.

**Fix MAUI-B8 — Corriger l'URL GetPromotionsAsync**
```csharp
// Cynapharm-Mobile/Services/ProductService.cs
public Task<List<Promotion>?> GetPromotionsAsync(int productId)
    => _api.GetAsync<List<Promotion>>($"products/promos/product/{productId}");
```

**Fix Backend-B1 — PromoService retourne l'entité sauvegardée**
```csharp
// Dans CreateOrUpdatePromotionAsync, remplacer return promotionDto; par :
return _mapper.Map<PromotionDto>(promotion);
```

#### Corrections MOYENNE priorité

**Fix Backend-B3 — MarkettingService exception handling**
```csharp
// AddFileToSupportAsync : remplacer le throw par retour null ou vérification préalable
var support = await _db.SupportsMarketing.FindAsync(fichierDto.Id_Support);
if (support == null) return null!;   // ou throw une NotFoundException structurée
```

**Fix Backend-B5 — Message d'erreur PrixVente<=0**
```csharp
// ProductService.CreateOrUpdateProductAsync : retourner une exception métier
if (produitDto.PrixVente <= 0)
    throw new ArgumentException("Le prix de vente doit être supérieur à zéro.");
```

---

### 4.4 Scénarios de flux de données

#### Scénario 1 — ADMIN crée un produit complet

```
1. ADMIN → GET /products/categories
   ← ["Antibiotique", "Analgésique", ...]

2. ADMIN soumet formulaire → ProductFormComponent.onSubmit()
   → POST /products { Nom, Description, Categorie, PrixVente, Prix_Creation, TVA, IsActive=true }
   ← { isSuccess:true, result: { Id_Produit:42, ... } }

3. ADMIN crée un lot → LotFormComponent.onSubmit()
   → POST /lots/lot { numero:"LOT-2026-001", dateExpiration:"2027-12-31", quantite:500, id_Produit:42 }
   ← { isSuccess:true, result: { numeroLot:"LOT-2026-001", ... } }

4. ADMIN ajoute un support marketing → ProductDetailComponent.submitSupportForm()
   → POST /marketting/support { Type:"Brochure", CampaignName:"Lancement 2026", IsActive:true, Id_Produit:42 }
   ← { isSuccess:true, result: { Id_SupportMarketting:7, ... } }

5. ADMIN uploade un PDF
   → CloudinaryService.uploadFile(file) → URL Cloudinary
   → POST /marketting/support/file { NomFichier:"brochure.pdf", Url:"https://...", Extension:"pdf", Taille:2048000, Id_Support:7 }
   ← { isSuccess:true, result: { Id_Fichier:15, ... } }

6. ADMIN crée une promotion → PromotionFormComponent.onSubmit()
   → POST /promos { codePromo:"PROMO26", typePromotion:"Pourcentage", pourcentage:15, numeroLot:"LOT-2026-001", dateDebut:"2026-06-01", dateExpiration:"2026-08-31", estActive:true }
   ← { isSuccess:true, result: { id_Promo:3, ... } }
   ⚠️ BUG B1 : id_Promo retourné=0 (l'input est retourné, pas l'entité)
```

#### Scénario 2 — MEDECIN consulte le catalogue (MAUI)

```
1. App MAUI démarre → ProductListViewModel.LoadAsync()
   → SecureStorage.GetAsync("UserRole") = "MEDECIN"
   → CanSeePrices=false, _useVisibleEndpoint=true

2. → GET products/visible
   ← [{ id_Produit:42, nom:"Amoxicilline", prixVente:12.500, isActive:true, isArchived:false, supports:[...] }]
   Prix masqué dans la liste (CanSeePrices=false → badge "✓ Disponible" affiché)

3. MEDECIN tape "Amoxi" (4 chars, debounce 300ms)
   → ApplyFilterAsync() : filtre local _allProducts où Nom.Contains("Amoxi")
   → 1 résultat affiché

4. MEDECIN tape sur le produit → GoToDetailCommand
   → Shell.GoToAsync("//products/detail?productId=42")
   → ProductDetailViewModel.OnProductIdChanged(42) → InitAsync()
   → CanSeePrices=false (MEDECIN)
   → GET products/42 ← { ... }

5. → GET products/lots/42
   ← 404 (MEDECIN n'a pas l'accès DELEGUE requis)
   → ApiException(HttpStatusCode.NotFound) swallowed → Lots=vide

6. → GET products/promos?productId=42
   ← 404 ou mismatch URL (B8)
   → swallowed → Promotions=vide

7. UI affiche :
   - Hero image (si ImageUrl extrait)
   - Nom + Description
   - Banner MEDECIN : "Pour toute demande d'échantillons..."
   - Documents (supports actifs, type≠Image)
   - Lots section : masquée (CanSeePrices=false)
   - Promotions section : masquée (CanSeePrices=false)
   - CTA "Ajouter à commande" : masqué (CanSeePrices=false)
```

#### Scénario 3 — DELEGUE consulte un produit puis passe une commande

```
1. ProductListViewModel.LoadAsync()
   → role="DELEGUE" → _useVisibleEndpoint=false
   → GET products → tous les produits non archivés
   → filtre !p.IsArchived côté client
   → CanSeePrices=true → prix visibles

2. DELEGUE sélectionne un produit → ProductDetailViewModel.LoadAsync()
   → GET products/42 ← { ... }
   → GET products/lots/42 ← [{ numeroLot:"LOT-2026-001", quantite:500, ... }]
   ⚠️ BUG B6 : Lot.Quantite=0 (QuantiteDisponible vs Quantite mismatch JSON)
   → Affiche 0 unités dans la liste lots MAUI

3. DELEGUE clique "Ajouter à commande"
   → AddToOrderAsync() : role="DELEGUE" → pas de blocage
   → Shell.GoToAsync("//orders/create?productId=42")
```

---

## PART 5 — Code complet

### Backend ProductAPI

#### `CynapCRM.Services.ProductAPI/Service/IService/IProductService.cs`
```csharp
public interface IProductService
{
    Task<IEnumerable<ProduitDto>> GetProductsWithExpiringLotsAsync(int daysThreshold = 30);
    Task<IEnumerable<ProduitDto>> GetProductsWithActivePromotionsAsync();
    Task<IEnumerable<ProduitDto>> GetAllProductsAsync();
    Task<ProduitDto?> GetProductByIdAsync(int productId);
    Task<IEnumerable<ProduitDto>> GetVisibleProductsAsync();
    Task<ProduitDto> CreateOrUpdateProductAsync(ProduitDto produitDto);
    Task<bool> ArchiveProductAsync(int productId);
    Task<bool> UnarchiveProductAsync(int productId);
    Task<bool> ActivateProductAsync(int productId);
    Task<bool> DeactivateProductAsync(int productId);
    Task<bool> DeleteProductAsync(int productId);
    Task<bool> IsProductAvailableAsync(int productId);
    Task<IEnumerable<ProduitDto>> GetAvailableProductsAsync();
    Task<IEnumerable<ProduitDto>> GetUnavailableProductsAsync();
    Task<int> GetTotalStockAsync(int productId);
    Task<IEnumerable<StockStatusDto>> GetStockStatusAsync();
    Task<IEnumerable<ProduitDto>> GetLowStockProductsAsync(int threshold);
    Task<IEnumerable<ProduitDto>> SearchProductsAsync(string keyword, bool isActive, bool allowArchived, int limit = 10);
    Task<IEnumerable<ProduitDto>> FilterProductsAsync(string? keyword, bool? isActive, bool? allowArchived, string? category, int page, int pageSize);
    Task<IEnumerable<string>> GetCategoriesAsync();
    Task<IEnumerable<ProduitDto>> GetProductsByCategoryAsync(string category);
    Task<bool> ProductExistsAsync(string productName);
    Task<bool> IsProductValidAsync(int productId);
    Task<bool> CanArchiveProductAsync(int productId);
    Task<IEnumerable<ProduitDto>> GetTopProductsAsync(int topN);
    Task<ProductDashboardDto> GetProductDashboardAsync();
}
```

#### `CynapCRM.Services.ProductAPI/Service/IService/ILotService.cs`
```csharp
public interface ILotService
{
    Task<LotDto?> GetLotByNumeroAsync(string numeroLot);
    Task<IEnumerable<LotDto>> GetLotsByProductIdAsync(int productId);
    Task<IEnumerable<LotDto>> GetAvailableLotsAsync(int productId);
    Task<LotDto> CreateOrUpdateLotAsync(LotDto lotDto);
    Task<bool> DeleteLotAsync(string numeroLot);
    Task<bool> AdjustStockAsync(int productId, int quantityChange);
    Task<bool> UpdateLotQuantityAsync(string numeroLot, int quantityChange);
    Task<bool> IsLotOutOfStockAsync(string numeroLot);
    Task<bool> IsLotExpiredAsync(string numeroLot);
    Task<IEnumerable<LotDto>> GetLotsNearExpirationAsync(int daysThreshold);
    Task<IEnumerable<LotDto>> GetExpiredLotsAsync();
    Task<IEnumerable<LotDto>> GetAllLotsAsync();
}
```

#### `CynapCRM.Services.ProductAPI/Service/IService/IPromoService.cs`
```csharp
public interface IPromoService
{
    Task<IEnumerable<PromotionDto>> GetAllPromotionsAsync();
    Task<PromotionDto?> GetPromotionByIdAsync(int promotionId);
    Task<PromotionDto> CreateOrUpdatePromotionAsync(PromotionDto promotionDto);
    Task<bool> DeletePromotionAsync(int promotionId);
    Task<decimal> ApplyBestPromotionAsync(int productId, decimal initialPrice);
    Task<bool> IsProductInPromotionAsync(int productId);
    Task<IEnumerable<PromotionDto>> GetPromotionsByProductAsync(int productId);
    Task<IEnumerable<PromotionDto>> GetPromotionsByLotAsync(string numeroLot);
    Task<bool> IsPromotionValidAsync(int promotionId);
    Task<bool> IsPromotionApplicableAsync(int promotionId, DateTime referenceDate);
    Task<double> GetPromotionCoverageRateAsync();
    Task<int> GetActivePromotionsCountAsync();
}
```

#### `CynapCRM.Services.ProductAPI/Service/IService/IMarkettingService.cs`
```csharp
public interface IMarkettingService
{
    Task<IEnumerable<SupportMarketingDto>> GetSupportsByProductAsync(int productId);
    Task<SupportMarketingDto?> GetSupportByIdAsync(int supportId);
    Task<SupportMarketingDto> CreateOrUpdateSupportAsync(SupportMarketingDto supportDto);
    Task<bool> DisableSupportAsync(int supportId);
    Task<bool> ActivateSupportAsync(int supportId);
    Task<FichierDto> AddFileToSupportAsync(FichierDto fichierDto);
    Task<bool> DeleteFileAsync(int fichierId);
    Task<IEnumerable<FichierDto>> GetFilesBySupportAsync(int supportId);
    Task<bool> IsSupportActiveAsync(int supportId);
    Task<IEnumerable<SupportMarketingDto>> GetVisibleSupportsByProductAsync(int productId);
    Task<IEnumerable<SupportMarketingDto>> GetSupportsByCampaignAsync(string campaignName);
    Task<IEnumerable<string>> GetCampaignsAsync();
}
```

---

### Angular — `product.service.ts`
```typescript
@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly endpoint = '/products';
  constructor(private apiService: ApiService) {}

  private unwrapResult<T>(response: any): T {
    if (response == null) return response;
    if (response.Result !== undefined) return response.Result;
    if (response.result !== undefined) return response.result;
    return response;
  }

  getProducts(): Observable<any[]> { return this.apiService.get<any>(this.endpoint).pipe(map(r => this.unwrapResult<any[]>(r))); }

  getProductsAll(): Observable<any[]> {
    const params = new HttpParams().set('page', '1').set('pageSize', '1000');
    return this.apiService.get<any>(`${this.endpoint}/filter`, params).pipe(
      map(r => this.unwrapResult<any[]>(r) ?? []),
      catchError(err => { if (err.status === 404) return of([]); throw err; })
    );
  }

  getCategories(): Observable<string[]> { return this.apiService.get<any>(`${this.endpoint}/categories`).pipe(map(r => this.unwrapResult<string[]>(r) ?? []), catchError(() => of([]))); }
  getVisibleProducts(): Observable<any[]> { return this.apiService.get<any>(`${this.endpoint}/visible`).pipe(map(r => this.unwrapResult<any[]>(r) ?? []), catchError(() => of([]))); }
  getProductById(id: string | number): Observable<any> { return this.apiService.get<any>(`${this.endpoint}/${id}`).pipe(map(r => this.unwrapResult<any>(r))); }
  createProduct(data: any): Observable<any> { return this.apiService.post<any>(this.endpoint, data); }
  updateProduct(_id: string | number, data: any): Observable<any> { return this.apiService.post<any>(this.endpoint, data); }
  deleteProduct(id: string): Observable<any> { return this.apiService.put<any>(`${this.endpoint}/${id}/deactivate`, {}); }
  hardDeleteProduct(id: string): Observable<any> { return this.apiService.delete<any>(`${this.endpoint}/${id}`); }
  activateProduct(id: string): Observable<any> { return this.apiService.put<any>(`${this.endpoint}/${id}/activate`, {}); }
  archiveProduct(id: string): Observable<any> { return this.apiService.put<any>(`${this.endpoint}/${id}/archive`, {}); }
  unarchiveProduct(id: string): Observable<any> { return this.apiService.put<any>(`${this.endpoint}/${id}/unarchive`, {}); }

  searchProducts(keyword: string, isActive: boolean, allowArchived: boolean, limit = 10): Observable<any[]> {
    const params = new HttpParams().set('keyword', keyword).set('isActive', String(isActive)).set('allowArchived', String(allowArchived)).set('limit', String(limit));
    return this.apiService.get<any>(`${this.endpoint}/search`, params).pipe(map(r => this.unwrapResult<any[]>(r) ?? []));
  }

  filterProducts(page: number, pageSize: number, keyword?: string, isActive?: boolean, allowArchived?: boolean, category?: string): Observable<any[]> {
    let params = new HttpParams().set('page', String(page)).set('pageSize', String(pageSize));
    if (keyword) params = params.set('keyword', keyword);
    if (isActive !== undefined) params = params.set('isActive', String(isActive));
    if (allowArchived !== undefined) params = params.set('allowArchived', String(allowArchived));
    if (category) params = params.set('category', category);
    return this.apiService.get<any>(`${this.endpoint}/filter`, params).pipe(map(r => this.unwrapResult<any[]>(r) ?? []));
  }
}
```

---

### Angular — `lot.service.ts`
```typescript
@Injectable({ providedIn: 'root' })
export class LotService {
  private readonly baseUrl = '/products/lots';
  constructor(private apiService: ApiService) {}

  getAllLots(): Observable<LotDto[]> { return this.apiService.get<ApiResponse<LotDto[]>>(this.baseUrl).pipe(map(res => this.unwrap(res))); }
  getLotsByProductId(productId: number): Observable<LotDto[]> { return this.apiService.get<ApiResponse<LotDto[]>>(`${this.baseUrl}/product/${productId}`).pipe(map(res => this.unwrap(res))); }

  getLotByNumero(numeroLot: string): Observable<LotDto> {
    return this.apiService.get<ApiResponse<LotDto>>(`${this.baseUrl}/lot/${numeroLot}`).pipe(map(res => {
      const raw: any = res?.Result ?? res?.result ?? res;
      return { numero: raw.numero ?? raw.Numero ?? '', quantite: raw.quantite ?? raw.Quantite ?? 0, dateExpiration: raw.dateExpiration ?? raw.DateExpiration ?? null, idProduit: raw.idProduit ?? raw.IdProduit ?? (raw as any).id_Produit ?? 0, isExpired: raw.isExpired ?? raw.IsExpired ?? false, isOutOfStock: raw.isOutOfStock ?? raw.IsOutOfStock ?? false, promotions: raw.promotions ?? raw.Promotions ?? [] } as LotDto;
    }));
  }

  createOrUpdateLot(lot: LotDto): Observable<LotDto> {
    const payload: LotPayload = { numero: (lot.numero || '').trim(), dateExpiration: lot.dateExpiration || '', quantite: Number(lot.quantite) || 0, id_Produit: Number(lot.idProduit) || 0 };
    return this.apiService.post<LotDto>(`${this.baseUrl}/lot`, payload);
  }

  deleteLot(numeroLot: string): Observable<void> { return this.apiService.delete<void>(`${this.baseUrl}/${numeroLot}`); }

  private unwrap(res: ApiResponse<LotDto[]> | LotDto[]): LotDto[] {
    const raw = (res as ApiResponse<LotDto[]>)?.Result ?? (res as ApiResponse<LotDto[]>)?.result ?? res as LotDto[];
    if (!Array.isArray(raw)) return [];
    return raw.map(lot => ({ ...lot, numero: lot.numero ?? (lot as any).Numero ?? '', dateExpiration: lot.dateExpiration ?? (lot as any).DateExpiration ?? '', quantite: lot.quantite ?? (lot as any).Quantite ?? 0, idProduit: (lot as any)['id_Produit'] ?? lot.idProduit ?? (lot as any).Id_Produit ?? 0, isExpired: lot.isExpired ?? (lot as any).IsExpired ?? false, isOutOfStock: lot.isOutOfStock ?? (lot as any).IsOutOfStock ?? false }));
  }
}
```

---

### Angular — `promotion.service.ts`
```typescript
export interface PromotionDto {
  id_Promo?: number;
  codePromo: string;
  typePromotion: 'Pourcentage' | 'Gratuite';
  pourcentage?: number;
  seuilAchat?: number;
  quantiteGratuite?: number;
  porteeSurTousLesLots: boolean;
  numeroLot?: string;
  id_Produit?: number;
  dateDebut: string;
  dateExpiration: string;
  estActive: boolean;
  isValid?: boolean;
}

@Injectable({ providedIn: 'root' })
export class PromotionService {
  private readonly base = '/products/promos';
  constructor(private api: ApiService) {}

  private unwrap<T>(r: any): T { if (r?.Result !== undefined) return r.Result; if (r?.result !== undefined) return r.result; return r; }

  private normalize(r: any): PromotionDto {
    return { id_Promo: r.id_Promo ?? r.Id_Promo ?? undefined, codePromo: r.codePromo ?? r.CodePromo ?? '', typePromotion: r.typePromotion ?? r.TypePromotion ?? 'Pourcentage', pourcentage: r.pourcentage ?? r.Pourcentage, seuilAchat: r.seuilAchat ?? r.SeuilAchat, quantiteGratuite: r.quantiteGratuite ?? r.QuantiteGratuite, porteeSurTousLesLots: r.porteeSurTousLesLots ?? r.PorteeSurTousLesLots ?? false, numeroLot: r.numeroLot ?? r.NumeroLot, id_Produit: r.id_Produit ?? r.Id_Produit, dateDebut: r.dateDebut ?? r.DateDebut ?? '', dateExpiration: r.dateExpiration ?? r.DateExpiration ?? '', estActive: r.estActive ?? r.EstActive ?? false, isValid: r.isValid ?? r.IsValid ?? false };
  }

  getAll(): Observable<PromotionDto[]> { return this.api.get<any>(this.base).pipe(map(r => { const raw = this.unwrap<any[]>(r) ?? []; return Array.isArray(raw) ? raw.map(p => this.normalize(p)) : []; })); }
  getById(id: number): Observable<PromotionDto> { return this.api.get<any>(`${this.base}/${id}`).pipe(map(r => this.normalize(this.unwrap<any>(r)))); }
  getActiveCount(): Observable<number> { return this.api.get<any>(`${this.base}/active-count`).pipe(map(r => this.unwrap<number>(r) ?? 0)); }
  getCoverageRate(): Observable<number> { return this.api.get<any>(`${this.base}/coverage-rate`).pipe(map(r => this.unwrap<number>(r) ?? 0)); }
  isValid(id: number): Observable<boolean> { return this.api.get<any>(`${this.base}/${id}/valid`).pipe(map(r => this.unwrap<boolean>(r) ?? false)); }
  isApplicable(id: number, referenceDate: string): Observable<boolean> { const params = new HttpParams().set('referenceDate', referenceDate); return this.api.get<any>(`${this.base}/${id}/applicable`, params).pipe(map(r => this.unwrap<boolean>(r) ?? false)); }
  getByProduct(productId: number): Observable<PromotionDto[]> { return this.api.get<any>(`${this.base}/product/${productId}`).pipe(map(r => this.unwrap<PromotionDto[]>(r) ?? [])); }
  isProductInPromotion(productId: number): Observable<boolean> { return this.api.get<any>(`${this.base}/product/${productId}/in-promotion`).pipe(map(r => this.unwrap<boolean>(r) ?? false)); }
  applyBest(productId: number, initialPrice: number): Observable<number> { const params = new HttpParams().set('initialPrice', String(initialPrice)); return this.api.get<any>(`${this.base}/product/${productId}/apply`, params).pipe(map(r => this.unwrap<number>(r) ?? initialPrice)); }
  getByLot(numeroLot: string): Observable<PromotionDto[]> { return this.api.get<any>(`${this.base}/lot/${numeroLot}`).pipe(map(r => this.unwrap<PromotionDto[]>(r) ?? [])); }
  createOrUpdate(dto: PromotionDto): Observable<PromotionDto> { return this.api.post<any>(this.base, dto).pipe(map(r => this.unwrap<PromotionDto>(r))); }
  delete(id: number): Observable<void> { return this.api.delete<void>(`${this.base}/${id}`); }
}
```

---

### MAUI — `Services/ProductService.cs`
```csharp
public class ProductService
{
    private readonly ApiService _api;
    public ProductService(ApiService api) { _api = api; }

    private static readonly HashSet<string> _imageExts =
        new(StringComparer.OrdinalIgnoreCase) { "jpg", "png", "webp" };

    private static string? ExtractImageUrl(Product p) =>
        p.Supports?.FirstOrDefault(s => s.IsActive && string.Equals(s.Type, "Image", StringComparison.OrdinalIgnoreCase))
                   ?.Fichiers?.FirstOrDefault(f => _imageExts.Contains(f.Extension))?.Url;

    public async Task<List<Product>?> GetProductsAsync(string? search = null, int limit = 100) {
        List<Product>? result;
        if (!string.IsNullOrWhiteSpace(search))
            result = await _api.GetAsync<List<Product>>($"products/search?keyword={Uri.EscapeDataString(search)}&isActive=true&limit={limit}");
        else
            result = await _api.GetAsync<List<Product>>("products");
        if (result != null) foreach (var p in result) p.ImageUrl ??= ExtractImageUrl(p);
        return result;
    }

    public async Task<Product?> GetProductByIdAsync(int id) {
        var product = await _api.GetAsync<Product>($"products/{id}");
        if (product != null) product.ImageUrl ??= ExtractImageUrl(product);
        return product;
    }

    public Task<List<Lot>?> GetLotsByProductAsync(int productId)
        => _api.GetAsync<List<Lot>>($"products/lots/{productId}");

    public Task<List<Promotion>?> GetPromotionsAsync(int? productId) {
        var url = "products/promos";
        if (productId.HasValue) url += $"?productId={productId.Value}";  // ← BUG B8
        return _api.GetAsync<List<Promotion>>(url);
    }

    public async Task<List<Product>?> GetVisibleProductsAsync() {
        var result = await _api.GetAsync<List<Product>>("products/visible");
        if (result != null) foreach (var p in result) p.ImageUrl ??= ExtractImageUrl(p);
        return result;
    }

    public Task<List<string>?> GetCategoriesAsync() => _api.GetAsync<List<string>>("products/categories");
    public Task<byte[]?> DownloadFileAsync(string url) => _api.DownloadFileAsync(url);
}
```

---

### MAUI — `Models/Products/Product.cs`
```csharp
public class Product
{
    [JsonPropertyName("id_Produit")] public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;   // absent du backend
    public string Nom { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Categorie { get; set; } = string.Empty;
    [JsonPropertyName("prixVente")] public decimal PrixUnitaire { get; set; }
    public bool IsPriceDefined => PrixUnitaire > 0;
    public string PrixDisplay => PrixUnitaire > 0 ? PrixUnitaire.ToString("N", _tndFormat) + " TND" : "Prix non défini";
    public string? ImageUrl { get; set; }
    [JsonPropertyName("isActive")] public bool Actif { get; set; }
    public bool IsArchived { get; set; }
    public List<SupportMarketing>? Supports { get; set; }
}

public class SupportMarketing { public string Type { get; set; } = string.Empty; public bool IsActive { get; set; }  public string? CampaignName { get; set; }  public List<Fichier>? Fichiers { get; set; } }
public class Fichier { public string NomFichier { get; set; } = string.Empty;  public string Url { get; set; } = string.Empty;  public string Extension { get; set; } = string.Empty; }
```

---

### MAUI — `Models/Products/Lot.cs` (état actuel — désynchronisé)
```csharp
public class Lot {
    public int Id { get; set; }              // ← devrait être string NumeroLot
    public int ProductId { get; set; }       // ← devrait être [JsonPropertyName("id_Produit")]
    public string NumeroLot { get; set; } = string.Empty;
    public DateTime DateFabrication { get; set; }   // ← n'existe pas côté backend
    public DateTime DateExpiration { get; set; }
    public int QuantiteDisponible { get; set; }     // ← devrait être Quantite
}
```

### MAUI — `Models/Products/Promotion.cs` (état actuel — désynchronisé)
```csharp
public class Promotion {
    public int Id { get; set; }             // ← devrait être [JsonPropertyName("id_Promo")]
    public int? ProductId { get; set; }
    public string Titre { get; set; } = string.Empty;       // ← n'existe pas côté backend
    public string? Description { get; set; }                // ← n'existe pas côté backend
    public decimal? RemisePourcentage { get; set; }         // ← devrait être Pourcentage (float)
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }                   // ← devrait être DateExpiration
}
```

---

### MAUI — `AppSettings.cs`
```csharp
public class AppSettings {
    public string ApiGatewayBaseUrl { get; set; } = "http://cynapharmgateway.runasp.net/";
    public string? ApiGatewayBaseUrlProd { get; set; }
}
```

---

*Fin de l'analyse — 10 bugs identifiés (3 HAUTE, 4 MOYENNE, 3 FAIBLE), 8 fonctionnalités manquantes, 3 scénarios de flux documentés.*
