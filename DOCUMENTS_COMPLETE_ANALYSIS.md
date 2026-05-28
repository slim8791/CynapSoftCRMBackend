# Documents — Analyse complète : BC / BL / Facture
## Backend (DocAPI) + Angular + MAUI + Données

---

## Partie 1 — Architecture Backend (DocAPI)

### 1.1 Modèle de données — TPH (Table Per Hierarchy)

Le DocAPI utilise l'héritage TPH via EF Core : une seule table `Documents` stocke toutes les entités, discriminées par `TypeDocument`.

**Entité de base — `Document`**

| Propriété       | Type     | Contrainte                  | Valeur par défaut |
|-----------------|----------|-----------------------------|-------------------|
| `Numero_Doc`    | int (PK) | Identity, généré auto       | —                 |
| `Nom_Doc`       | string   | Required                    | `string.Empty`    |
| `DateCreation`  | DateTime | —                           | `DateTime.UtcNow` |
| `Id_Commande`   | int      | Required (lien OrderAPI)    | —                 |
| `Id_Client`     | int?     | Optionnel                   | null              |
| `TypeDocument`  | string   | Required                    | `string.Empty`    |
| `IsDeleted`     | bool     | Soft delete                 | `false`           |

**Entités dérivées**

- `BonCommande : Document` — aucun champ supplémentaire (entité discriminateur pure)
- `BonLivraison : Document` — aucun champ supplémentaire
- `Facture : Document` — 3 champs supplémentaires :

| Propriété     | Type     | Contrainte |
|---------------|----------|------------|
| `MontantHT`   | decimal  | Required   |
| `MontantTTC`  | decimal  | Required   |
| `DateFacture` | DateTime | Required   |

### 1.2 DTOs

**`DocumentDto`** (classe de base) :
```
Numero_Doc, Nom_Doc, DateCreation, Id_Commande, Id_Client, TypeDocument
```

**`BonCommandeDto : DocumentDto`** — aucun champ ajouté

**`BonLivraisonDto : DocumentDto`** — aucun champ ajouté

**`FactureDto : DocumentDto`** — champs supplémentaires : `Numero_Doc` *(re-déclaré)*, `MontantHT`, `MontantTTC`, `DateFacture`

> **Observation :** `FactureDto` re-déclare `Numero_Doc` alors qu'il est déjà hérité de `DocumentDto`. C'est une redondance sans effet fonctionnel mais qui crée de la confusion.

### 1.3 Endpoints

#### BonsCommandes — `api/bons-commandes`

| Méthode | Route                      | Rôles autorisés                                              | Description                   |
|---------|---------------------------|--------------------------------------------------------------|-------------------------------|
| GET     | `/`                        | ADMIN, SUPERVISEUR                                           | Liste paginée (`pageNumber`, `pageSize`) |
| GET     | `/{id}`                    | ADMIN, SUPERVISEUR, DELEGUE, PHARMACIEN, GROSSISTE, CLIENT   | Par ID                        |
| GET     | `/client/{idClient}`       | ADMIN, SUPERVISEUR, DELEGUE, PHARMACIEN, GROSSISTE, CLIENT   | Par client                    |
| GET     | `/commande/{idCommande}`   | ADMIN, SUPERVISEUR, DELEGUE, PHARMACIEN, GROSSISTE, CLIENT   | Par commande (ajouté via FIX) |
| GET     | `/by-date`                 | ADMIN, SUPERVISEUR                                           | Par plage de dates            |
| POST    | `/createUpdate`            | ADMIN, SUPERVISEUR                                           | Créer ou mettre à jour        |
| DELETE  | `/{id}`                    | ADMIN                                                        | Suppression douce (IsDeleted) |

#### BonsLivraisons — `api/bons-livraison`

Identique à BonsCommandes (mêmes routes et rôles).

#### Factures — `api/factures`

Identique à BonsCommandes (mêmes routes et rôles).

### 1.4 Logique des services

**Pattern Create/Update (BCService, BLService, FactureService) :**
```
Si Numero_Doc == 0  → INSERT (TypeDocument forcé à "BC"/"BL"/"FACTURE")
Sinon               → UPDATE (seul Nom_Doc mis à jour pour BC/BL ;
                               Nom_Doc + MontantHT + MontantTTC + DateFacture pour Facture)
```

**Soft delete :** `IsDeleted = true` (pas de DELETE physique)

**Toutes les requêtes filtrent `!IsDeleted`** — les enregistrements supprimés n'apparaissent jamais dans les listes.

**AutoMapper :** utilisé pour la lecture (`_mapper.Map<BonCommandeDto>(bc)`), mais les méthodes Create/Update construisent manuellement le DTO de retour (non cohérent).

### 1.5 Pattern de réponse

Tous les contrôleurs utilisent `ResponseDto` :
```json
{
  "isSuccess": true,
  "result": { ... },
  "message": ""
}
```

**`StatusCode(515)`** : code personnalisé utilisé dans TOUS les contrôleurs de l'application pour les exceptions non gérées. Ce n'est pas un bug.

---

## Partie 2 — Module Angular (Documents)

### 2.1 Structure du module

```
features/documents/
├── documents.module.ts              (NgModule: importe DocumentsRoutingModule)
├── documents-routing.module.ts      (Routes)
├── documents-general/
│   ├── services/document.service.ts
│   └── document-list/               (DocumentListComponent)
├── document-detail/                 (DocumentDetailComponent — partagé)
├── bons-commandes/
│   ├── services/bon-commande.service.ts
│   └── bon-commande-list/           (BonCommandeListComponent)
├── bons-livraison/
│   ├── services/bon-livraison.service.ts
│   └── bon-livraison-list/          (BonLivraisonListComponent)
└── factures/
    ├── services/facture.service.ts
    └── facture-list/                (FactureListComponent)
```

### 2.2 Routing

```
/documents                → redirect → general
/documents/general        → DocumentListComponent
/documents/bons-commandes → BonCommandeListComponent
/documents/bons-commandes/:id → DocumentDetailComponent (data: { documentKind: 'bon-commande' })
/documents/bons-livraison → BonLivraisonListComponent
/documents/bons-livraison/:id → DocumentDetailComponent (data: { documentKind: 'bon-livraison' })
/documents/factures       → FactureListComponent
/documents/factures/:id   → DocumentDetailComponent (data: { documentKind: 'facture' })
```

> **Observation :** `DocumentDetailComponent` est partagé pour les 3 types. Le type est passé via `route.snapshot.data['documentKind']`.

### 2.3 Services Angular

#### `DocumentService` (general) — base `/documents`

| Méthode              | Appel API                                 |
|----------------------|-------------------------------------------|
| `getAll(page, size)` | `GET /documents?pageNumber&pageSize`      |
| `getById(numero)`    | `GET /documents/{numero}`                 |
| `getByClient(id)`    | `GET /documents/client/{id}`              |
| `getByCommande(id)`  | `GET /documents/commande/{id}`            |
| `getByType(type)`    | `GET /documents/type/{type}?pageNumber&pageSize` |
| `getByClientAndType` | `GET /documents/client/{id}/type/{type}`  |
| `createOrUpdate(dto)`| `POST /documents/createUpdate`            |
| `delete(numero)`     | `DELETE /documents/{numero}`              |

#### `BonCommandeService` — base `/documents/bons-commandes`

| Méthode                     | Appel API                                              |
|-----------------------------|--------------------------------------------------------|
| `getAll(page, size)`        | `GET /documents/bons-commandes?pageNumber&pageSize`    |
| `getById(id)`               | `GET /documents/bons-commandes/{id}`                   |
| `getByClient(id)`           | `GET /documents/bons-commandes/client/{id}`            |
| `getByDate(start, end)`     | `GET /documents/bons-commandes/by-date?startDate&endDate` |
| `getByCommande(id)`         | `GET /documents/bons-commandes/commande/{id}`          |
| `createOrUpdate(dto)`       | `POST /documents/bons-commandes/createUpdate`          |
| `delete(id)`                | `DELETE /documents/bons-commandes/{id}`                |

`BonLivraisonService` et `FactureService` suivent le même pattern (base `/documents/bons-livraison` et `/documents/factures`).

#### DTOs Angular

```typescript
// DocumentService
interface DocumentDto {
  numeroDoc?: string;   // ← camelCase (différent du backend Numero_Doc)
  type: string;
  id_Client: number;
  id_Commande?: number | null;
  dateDocument?: string;
}

// BonCommandeService
interface BonCommandeDto {
  numero_Doc: number;
  nom_Doc?: string;
  id_Client?: number;
  id_Commande?: number;
  dateCreation?: string;
  typeDocument?: string;
  cloudinaryUrl?: string;   // ← non présent dans le backend
  url?: string;             // ← non présent dans le backend
}

// FactureDto ajoute:
interface FactureDto extends BonCommandeDto {
  montantHT?: number;
  montantTTC?: number;
  dateFacture?: string;
}
```

#### Normalisation de réponse

Tous les services utilisent : `r?.Result ?? r?.result ?? r` pour extraire les données de `ResponseDto`.

### 2.4 Composants Angular

#### `DocumentListComponent`

- Affiche tous les documents avec onglets de filtrage : Tous | Factures | BC | BL
- `setTypeFilter()` → recharge la liste via `getAll()` ou `getByType()`
- Résolution des noms clients : `UserService.getUserById(id)` avec cache `clientNames: Record<number, string>`
- Colonnes : N° Doc, Type, Client, Commande, Date
- Pagination via `PaginatorComponent`
- **Pas d'actions** (voir/supprimer) — vue lecture seule

#### `BonCommandeListComponent`

- Colonnes : N° Doc, Nom, Client, Commande, Date création, Actions
- Actions : Voir (👁 → routerLink `/documents/bons-commandes/{id}`), Télécharger (⬇), Supprimer (🗑)
- Téléchargement Cloudinary : cherche `cloudinaryUrl ?? CloudinaryUrl ?? url ?? Url ?? documentUrl ?? ...` et insère `fl_attachment` dans l'URL
- **Problème :** ces champs n'existent pas dans le `BonCommandeDto` backend — le téléchargement échouera toujours (affiche l'erreur "Aucun fichier Cloudinary disponible")

#### `BonLivraisonListComponent`

- Identique à BonCommandeListComponent
- Utilise `PdfService` injecté mais jamais appelé (injection inutile)

#### `FactureListComponent`

- Identique + colonnes supplémentaires : Montant HT, Montant TTC
- Pipe `CurrencyTNDPipe` pour le formatage
- Utilise `PdfService` injecté mais jamais appelé

#### `DocumentDetailComponent` (partagé)

- Lit `documentKind` depuis `route.snapshot.data`
- Lit `id` depuis `route.snapshot.paramMap.get('id')`
- Dispatche vers `factureSvc.getById()`, `bcSvc.getById()`, ou `blSvc.getById()`
- Résolution du nom client via `UserService`
- Normalisation de la date : `doc.dateFacture ?? doc.DateFacture ?? doc.dateCreation ?? doc.DateCreation`
- Affiche les champs Montant HT/TTC uniquement si `kind === 'facture'`
- Bouton "Imprimer / PDF" → `window.print()`

---

## Partie 3 — Module MAUI (Mobile)

### 3.1 Modèles MAUI

#### `DocumentSummary` — liste unifiée

| Propriété MAUI | JsonPropertyName  | Origine backend                     | Note                         |
|----------------|-------------------|-------------------------------------|------------------------------|
| `Id`           | `numero_Doc`      | `Document.Numero_Doc`               |                              |
| `Numero`       | `nom_Doc`         | `Document.Nom_Doc`                  | Affiché comme identifiant    |
| `Date`         | `dateCreation`    | `Document.DateCreation`             |                              |
| `Type`         | `typeDocument`    | `Document.TypeDocument`             | "FACTURE", "BC", "BL"        |
| `CommandeId`   | `id_Commande`     | `Document.Id_Commande`              |                              |
| `Url`          | `url_Document`    | *(non présent dans le backend DTO)* | Sera toujours `null`         |
| `Statut`       | *(aucun)*         | *(non présent dans le backend DTO)* | Toujours `string.Empty`      |
| `Montant`      | *(aucun)*         | *(non présent dans le backend DTO)* | Toujours `null`              |

#### `Facture`

| Propriété MAUI    | JsonPropertyName | Origine backend         | Note                         |
|-------------------|------------------|-------------------------|------------------------------|
| `Id`              | `numero_Doc`     | `Numero_Doc`            |                              |
| `NumeroFacture`   | `nom_Doc`        | `Nom_Doc`               | Utilisé comme titre          |
| `DateFacture`     | `dateFacture`    | `DateFacture`           |                              |
| `CommandeId`      | `id_Commande`    | `Id_Commande`           |                              |
| `MontantHT`       | *(auto)*         | `MontantHT`             |                              |
| `MontantTTC`      | *(auto)*         | `MontantTTC`            |                              |
| `TVA`             | *(aucun)*        | *(non présent backend)* | Toujours `0` — calculer côté client si nécessaire |
| `Statut`          | *(aucun)*        | *(non présent backend)* | Toujours `string.Empty`      |

#### `BonCommande`

| Propriété MAUI  | JsonPropertyName | Origine backend         | Note                         |
|-----------------|------------------|-------------------------|------------------------------|
| `Id`            | `numero_Doc`     | `Numero_Doc`            |                              |
| `NumeroBon`     | `nom_Doc`        | `Nom_Doc`               |                              |
| `DateEmission`  | `dateCreation`   | `DateCreation`          |                              |
| `CommandeId`    | `id_Commande`    | `Id_Commande`           |                              |
| `MontantTotal`  | *(aucun)*        | *(non présent backend)* | Toujours `0`                 |
| `Statut`        | *(aucun)*        | *(non présent backend)* | Toujours `string.Empty`      |

`BonLivraison` suit le même pattern que `BonCommande`.

### 3.2 `DocumentService` (MAUI)

| Méthode                            | Appel API                                              |
|------------------------------------|--------------------------------------------------------|
| `GetFacturesAsync(page, size)`     | `GET documents/factures?page={page}&size={size}`       |
| `GetFactureByIdAsync(id)`          | `GET documents/factures/{id}`                          |
| `GetBonsCommandeAsync(page, size)` | `GET documents/bons-commandes?page={page}&size={size}` |
| `GetBonCommandeByIdAsync(id)`      | `GET documents/bons-commandes/{id}`                    |
| `GetBonsLivraisonAsync(page, size)`| `GET documents/bons-livraison?page={page}&size={size}` |
| `GetBonLivraisonByIdAsync(id)`     | `GET documents/bons-livraison/{id}`                    |
| `GetDocumentsByClientAndTypeAsync` | `GET documents/client/{id}/type/{type}`                |
| `GetFacturesByCommandeAsync`       | `GET {ApiRoutes.Documents.FacturesByCommande}/{id}`    |
| `GetBCByCommandeAsync`             | `GET {ApiRoutes.Documents.BCByCommande}/{id}`          |
| `GetBLByCommandeAsync`             | `GET {ApiRoutes.Documents.BLByCommande}/{id}`          |

> **Observation :** `GetFacturesAsync` utilise `page` et `size` comme paramètres query, mais le backend attend `pageNumber` et `pageSize`. Les paramètres seront ignorés et la pagination par défaut s'appliquera.

### 3.3 `DocumentListViewModel`

- Tabs : Factures (index 0), Bons de commande (index 1), Bons de livraison (index 2)
- `OnSelectedTypeIndexChanged` → change `DocumentType` → `LoadAsync()`
- `LoadAsync` : lit `clientId` depuis `SecureStorage[StorageKeys.UserId]` → appelle `GetDocumentsByClientAndTypeAsync(clientId, apiType)`
- **Conséquence :** affiche uniquement les documents du client correspondant à l'utilisateur connecté. Un ADMIN/SUPERVISEUR ne verra que ses propres documents (ceux liés à son Id en tant que client).
- Navigation : `GoToDetailCommand` → `//documents/detail?documentType={doc.Type}&documentId={doc.Id}`
- `OpenDocumentCommand` → `Launcher.OpenAsync(new Uri(doc.Url))` — `doc.Url` sera toujours `null` (voir section 3.1)

### 3.4 `DocumentDetailViewModel`

- `[QueryProperty(nameof(DocumentType), "documentType")]`
- `[QueryProperty(nameof(DocumentId), "documentId")]`
- `OnDocumentIdChanged` et `OnDocumentTypeChanged` déclenchent `LoadAsync()` dès que les deux propriétés sont renseignées
- `LoadAsync` dispatche selon `DocumentType.ToLowerInvariant()` :
  - `"facture"` → `GetFactureByIdAsync`
  - `"bon-commande"` ou `"bc"` → `GetBonCommandeByIdAsync`
  - `"bon-livraison"` ou `"bl"` → `GetBonLivraisonByIdAsync`
- `ShareAsync` utilise `Share.RequestAsync` avec format `{0:C2}` (devise système) — incohérent avec le TND

### 3.5 Vues MAUI

#### `DocumentListPage.xaml`

- En-tête Primary avec titre "Documents"
- `ErrorBanner` pour les erreurs
- Barre de 3 onglets : `Button` + `BoxView` (indicateur souligné) contrôlés par `SelectedTypeIndex` via `DataTrigger`
- `RefreshView` + `CollectionView` avec `ItemsSource=Documents`
- `DataTemplate x:DataType="DocumentSummary"` : affiche `Numero`, `Date`, `Montant` (si non null), bouton download (si Url non null)
- Tap → `GoToDetailCommand`

#### `DocumentDetailPage.xaml`

- En-tête avec bouton retour (‹), titre dynamique
- `ToolbarItem` "Partager" → `ShareCommand`
- 3 sections conditionnelles (`IsVisible="{Binding IsFacture/IsBonCommande/IsBonLivraison}"`) :
  - **Facture** : NumeroFacture, DateFacture, MontantHT, TVA, TTC surligné en primary
  - **BonCommande** : NumeroBon, DateEmission, MontantTotal surligné en primary
  - **BonLivraison** : NumeroBon, DateLivraison, Statut
- Bouton sticky en bas : "Partager ce document"

---

## Partie 4 — Problèmes et écarts identifiés

### 4.1 Backend

| N° | Fichier                    | Problème                                                                                                         | Impact    |
|----|----------------------------|------------------------------------------------------------------------------------------------------------------|-----------|
| B1 | `FactureDto.cs`            | Re-déclare `Numero_Doc` (déjà hérité de `DocumentDto`)                                                          | Faible    |
| B2 | `BCService.cs` l.49        | `GetBonsCommandeByCommandeAsync` interroge `_db.BonsCommandes` sans `.OfType<BonCommande>()` contrairement aux autres méthodes | Faible (TPH discrimine via EF) |
| B3 | `Document.cs`              | Aucun champ d'URL Cloudinary dans l'entité ou les DTOs — les Angular/MAUI cherchent `cloudinaryUrl`/`url_Document` qui n'existent jamais | Fonctionnel critique |
| B4 | `BCService/BLService`      | `CreateOrUpdate` ne met à jour que `Nom_Doc` pour BC et BL — `Id_Commande`, `Id_Client` ne sont pas modifiables après création | Fonctionnel |
| B5 | `FacturesController.cs`    | `GetAllFactures` ne valide pas `pageNumber`/`pageSize` ≤ 0 (contrairement à BC et BL controllers)               | Mineur    |

### 4.2 Angular

| N° | Fichier                                   | Problème                                                                                       | Impact    |
|----|-------------------------------------------|-----------------------------------------------------------------------------------------------|-----------|
| A1 | `bon-commande-list.component.ts`          | `downloadUrl()` cherche des champs (`cloudinaryUrl`, `url`, `documentUrl`, `fileUrl`, `pdfUrl`) absents du backend DTO → erreur "Aucun fichier Cloudinary" toujours affichée | Critique  |
| A2 | `bon-livraison-list.component.ts`         | Même problème que A1                                                                          | Critique  |
| A3 | `facture-list.component.ts`               | Même problème que A1                                                                          | Critique  |
| A4 | `bon-livraison-list.component.ts`         | `PdfService` injecté mais jamais utilisé                                                     | Mineur    |
| A5 | `facture-list.component.ts`               | `PdfService` injecté mais jamais utilisé                                                     | Mineur    |
| A6 | `document.service.ts` (general)           | `DocumentDto.numeroDoc` est en camelCase mais le backend renvoie `Numero_Doc` (snake_case) — sans normalisation de champs | Fonctionnel |
| A7 | `document-detail.component.html`          | Affiche `doc.numero_Doc ?? doc.numeroDoc` — le champ `numeroDoc` n'existe pas dans le DTO backend, `Numero_Doc` est la vraie propriété | Mineur    |
| A8 | `documents-routing.module.ts`             | Le routing vers `DocumentDetailComponent` pour BC/BL passe l'`id` en tant que `numero_Doc` (int), mais la route utilise `/:id` sans contrainte de type | Fonctionnel (marche si numérique) |

### 4.3 MAUI

| N° | Fichier                        | Problème                                                                                         | Impact    |
|----|--------------------------------|--------------------------------------------------------------------------------------------------|-----------|
| M1 | `DocumentService.cs` l.11,17,23| Paramètres `page` et `size` dans les URLs mais backend attend `pageNumber` et `pageSize`         | Fonctionnel (pagination ignorée) |
| M2 | `DocumentSummary.cs`          | `Url` mappe `url_Document` qui n'existe pas dans le backend → `Url` sera toujours `null` → bouton download jamais visible | Critique  |
| M3 | `Facture.cs`                  | `TVA` et `Statut` n'existent pas dans le backend — affichent toujours 0 et "" dans le XAML     | UX        |
| M4 | `BonCommande.cs`              | `MontantTotal` et `Statut` n'existent pas dans le backend — affichent toujours 0 et ""          | UX        |
| M5 | `DocumentListViewModel.cs` l.54| `GetDocumentsByClientAndTypeAsync(clientId, ...)` utilise l'ID de l'utilisateur connecté comme clientId — ADMIN/SUPERVISEUR sans Id_Client correspondant verront une liste vide | Fonctionnel |
| M6 | `DocumentDetailViewModel.cs`  | `ShareAsync` formate montants avec `{0:C2}` (devise système) au lieu de `{0:N3} TND`            | UX        |
| M7 | `DocumentListPage.xaml` l.207 | `Montant` affiché avec `{0:#,##0.000} TND` mais `DocumentSummary.Montant` est toujours `null`  | UX (jamais visible) |

---

## Partie 5 — Flux de données complet

### 5.1 Flux liste de documents (Angular)

```
FactureListComponent.load()
  → FactureService.getAll(page, pageSize)
    → GET /documents/factures?pageNumber=1&pageSize=20
      → FacturesController.GetAllFactures()
        → FactureService.GetAllFacturesAsync()
          → _db.Factures.Where(!IsDeleted).Skip().Take()
            → AutoMapper → List<FactureDto>
      → ResponseDto { IsSuccess: true, Result: [...] }
    → map(r => r?.Result ?? r?.result ?? r)
  → FactureDto[] (camelCase via JSON) → factures[]
→ loadClientNames(factures)
  → UserService.getUserById(id) × N clients uniques
```

### 5.2 Flux liste de documents (MAUI)

```
DocumentListViewModel.LoadAsync()
  → SecureStorage.GetAsync(UserId) → clientId
  → _apiTypeMap[DocumentType] → apiType ("FACTURE"|"BC"|"BL")
  → DocumentService.GetDocumentsByClientAndTypeAsync(clientId, apiType)
    → GET documents/client/{clientId}/type/{apiType}
      → (DocumentController — note: cet endpoint vient de DocumentService général)
        → renvoie List<DocumentSummary>
  → Documents.Clear() + Documents.Add(...)
```

### 5.3 Flux détail de document (MAUI)

```
Navigation : //documents/detail?documentType=facture&documentId=123
  → DocumentDetailViewModel
    → OnDocumentTypeChanged + OnDocumentIdChanged → LoadAsync()
      → switch(documentType):
          "facture" → DocumentService.GetFactureByIdAsync(123)
            → GET documents/factures/123
              → FacturesController.GetFactureById(123)
                → FactureService.GetFactureByIdAsync(123)
                  → _db.Factures.OfType<Facture>().FirstOrDefault(id == 123 && !IsDeleted)
                    → AutoMapper → FactureDto
          → Facture = mapped result
          → Title = "Facture {NumeroFacture}"
```

---

## Partie 6 — Résumé et recommandations

### 6.1 Ce qui fonctionne correctement

- Architecture TPH claire et cohérente sur les 3 types de documents
- Pattern CRUD uniforme (CreateOrUpdate POST, soft delete, pagination)
- Pattern `ResponseDto` cohérent dans tous les contrôleurs
- Normalisation `r?.Result ?? r?.result ?? r` dans tous les services Angular
- `DocumentDetailComponent` partagé via `documentKind` — bonne conception
- MAUI `DocumentListViewModel` utilise l'endpoint `client/{id}/type/{type}` — efficient (un seul appel)
- MAUI `DocumentDetailPage.xaml` avec sections conditionnelles IsFacture/IsBonCommande/IsBonLivraison

### 6.2 Priorités de correction

**CRITIQUE — Cloudinary/URLs (B3, A1, A2, A3, M2) :**

Le backend `Document` n'a aucun champ d'URL pour les fichiers PDF/Cloudinary. Pour activer le téléchargement, il faut soit :
- Ajouter `string? CloudinaryUrl` au modèle `Document` et aux DTOs backend
- OU utiliser un service de stockage externe séparé dont l'URL est construite côté client

**FONCTIONNEL — Pagination MAUI (M1) :**

Remplacer `page={page}&size={size}` par `pageNumber={page}&pageSize={size}` dans `DocumentService.cs`.

**FONCTIONNEL — Champs fictifs MAUI (M3, M4) :**

`TVA`, `Statut`, `MontantTotal` ne viennent pas du backend. Options :
- Calculer `TVA = MontantTTC - MontantHT` côté MAUI
- Supprimer l'affichage de `Statut` et `MontantTotal` du XAML (ou afficher "-")

**FONCTIONNEL — Visibilité ADMIN/SUPERVISEUR (M5) :**

`DocumentListViewModel` filtre par `clientId` de l'utilisateur connecté. Pour les rôles ADMIN/SUPERVISEUR, appeler `GetBonsCommandeAsync()` / `GetFacturesAsync()` (liste globale paginée) au lieu de `GetDocumentsByClientAndTypeAsync`.

**MINEUR — Formatage devise (M6) :**

`ShareAsync` : remplacer `{0:C2}` par `{0:N3} TND`.

**MINEUR — Injection inutile (A4, A5) :**

Retirer `PdfService` des constructeurs de `BonLivraisonListComponent` et `FactureListComponent`.

### 6.3 Tableau de bord des entités

| Entité         | Backend DTO    | Angular DTO         | MAUI Model          | Champs fictifs MAUI           |
|----------------|----------------|---------------------|---------------------|-------------------------------|
| Document (base)| DocumentDto    | DocumentDto         | DocumentSummary     | Statut, Montant, Url          |
| BonCommande    | BonCommandeDto | BonCommandeDto      | BonCommande         | MontantTotal, Statut          |
| BonLivraison   | BonLivraisonDto| BonLivraisonDto     | BonLivraison        | MontantTotal, Statut          |
| Facture        | FactureDto     | FactureDto          | Facture             | TVA, Statut                   |

### 6.4 Endpoints manquants / non couverts

| Endpoint backend disponible           | Angular          | MAUI           |
|---------------------------------------|------------------|----------------|
| `GET /by-date` (BC, BL, Factures)     | Présent (service) | ✗ Non implémenté |
| `GET /commande/{id}` (BC, BL, Factures)| Présent (service) | Présent (service) |
| `POST /createUpdate` (BC, BL, Factures)| Présent (service) | ✗ Non implémenté (lecture seule) |
| `DELETE /{id}` (BC, BL, Factures)     | Présent (component) | ✗ Non implémenté |
| `GET /type/{type}` (Documents général)| Présent (service) | ✗ Non utilisé (utilise client+type) |
