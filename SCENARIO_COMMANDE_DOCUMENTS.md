# Scénario complet : Distribution d'échantillons (DÉLÉGUÉ)

> **Généré le :** 2026-05-22
> **Sources lues :** 17 fichiers backend + 25 fichiers Angular + 6 fichiers MAUI + ocelot.json
> **Gateway prod :** `http://cynapharmgateway.runasp.net`
> **FieldAPI downstream :** `cynapharmfields.runasp.net:80`
> **InventoryAPI downstream :** `cynapharminventories.runasp.net:80`

---

## Table des matières

1. [Consultation du stock (DÉLÉGUÉ)](#partie-1--consultation-du-stock-délégué-mauiangular)
2. [Planification d'une visite (DÉLÉGUÉ)](#partie-2--planification-dune-visite-délégué)
3. [Création du rapport de visite](#partie-3--création-du-rapport-de-visite)
4. [Distribution d'échantillons](#partie-4--distribution-déchantillons)
5. [Mise à jour du stock](#partie-5--mise-à-jour-du-stock)
6. [Business Logic Verification](#partie-6--business-logic-verification)
7. [Features manquantes](#partie-7--features-manquantes)
8. [Tableau complet des endpoints](#partie-8--tableau-complet-des-endpoints-utilisés)

---

## PARTIE 1 — Consultation du stock (DÉLÉGUÉ MAUI/Angular)

### 1.1 Vue MAUI — `MyStockViewModel.cs`

Le DÉLÉGUÉ accède à son stock via **3 segments** dans `MyStockPage` :

| Segment | `ActiveSegment` | Données affichées |
|---------|----------------|------------------|
| Échantillons | `0` | `_echantillonStock` (liste `StockDelegue`) |
| Promotionnels | `1` | `_promoStock` (liste `StockPromo`) |
| Historique | `2` | `StockMovements` (liste `StockMouvement`) |

**Champs affichés par ligne (segment 0 — Échantillons) :**

```
ProductNom        → Nom commercial du produit
QuantiteLabel     → "Restant : {QuantiteRestante}"
ExpiryLabel       → "Exp. {DateExpiration:dd/MM/yyyy}"  (null si pas de date)
QuantiteRestante  → entier, utilisé pour le contrôle de distribution
```

**Modèle MAUI `StockDelegue.cs` :**
```csharp
public class StockDelegue
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductNom { get; set; }
    public int QuantiteAllouee { get; set; }
    public int QuantiteRestante { get; set; }
    public DateTime? DateExpiration { get; set; }
}
```

> ⚠️ **Mismatch de noms :** Le backend expose `QteDisponible` / `QteReservee` (via `StockDelegueDto`).
> Le modèle MAUI utilise `QuantiteAllouee` / `QuantiteRestante`. Sans `[JsonPropertyName]` configurés,
> ces champs seront toujours `0` après désérialisation.

**API appelée par MAUI (`InventoryService.GetStockDelegueAsync`) :**
```
GET /inventory/stocks-delegue
→ downstream : GET http://cynapharminventories.runasp.net/api/stocks-delegue
```

> 🔴 **BUG CRITIQUE :** Cette route (`GET /api/stocks-delegue`) est restreinte à `ADMIN, SUPERVISEUR`
> uniquement (cf. `StocksDelegueController.GetAllStocks [Authorize(Roles = "ADMIN,SUPERVISEUR")]`).
> Un DÉLÉGUÉ obtiendra **403 Forbidden**.
>
> La route correcte pour un DÉLÉGUÉ est :
> `GET /inventory/stocks-delegue/by-delegue/{idDelegue}`
> (autorisée `ADMIN, SUPERVISEUR, DELEGUE`).

**Mécanismes de cache et fallback :**
- Cache TTL 5 min : `CacheKeyEchantillon = "stock:echantillon"` / `CacheKeyPromo = "stock:promo"`
- Si réseau indisponible → SQLite local via `_localDb.GetStockAsync()`
- `IsOffline = true` affiché dans l'UI
- `RefreshAsync()` invalide les deux caches puis recharge

**Chargement des mouvements (segment Historique) :**
```
GET /inventory/stock-movements/by-delegue/{userId}
→ downstream : GET http://cynapharminventories.runasp.net/api/stock-movements/by-delegue/{userId}
```
> ⚠️ Route déclarée dans ocelot.json mais **aucun controller/service backend trouvé** dans InventoryAPI.
> Retournera **404 Not Found**.

---

### 1.2 Vue Angular — `stock-list.component` / `stock-detail.component`

**Endpoint ADMIN (tous les stocks paginés) :**
```
GET http://cynapharmgateway.runasp.net/inventory/stocks-delegue?pageNumber=1&pageSize=20
```

**Endpoint DÉLÉGUÉ/SUPERVISEUR (stocks d'un délégué) :**
```
GET http://cynapharmgateway.runasp.net/inventory/stocks-delegue/by-delegue/{idDelegue}
```

**Champs affichés (`StockDelegueDto`) :**

| Champ backend C# | Champ Angular TS | Description |
|-----------------|-----------------|-------------|
| `Id_stock` | `id_stock` | Identifiant unique |
| `Id_User_Delegue` | `id_User_Delegue` | ID du délégué |
| `Id_Produit` | `id_Produit` | ID du produit |
| `NumeroLot` | `numeroLot` | Numéro de lot |
| `DateExpiration` | `dateExpiration` | Date d'expiration (ISO 8601) |
| `QteDisponible` | `qteDisponible` | Quantité disponible |
| `QteReservee` | `qteReservee` | Quantité réservée |

**Ce qui se passe si stock = 0 :**

| Contexte | Comportement |
|----------|-------------|
| MAUI `DistributeSampleAsync` | `if (item.QuantiteRestante <= 0)` → `ErrorMessage = "⚠️ Stock insuffisant"` → bloqué localement |
| MAUI `_localDb.DeductStockAsync` | Retourne `false` si stock insuffisant → ErrorMessage affiché |
| Angular `distribution-form` | **Aucun contrôle** — l'utilisateur peut saisir n'importe quelle quantité |
| Backend `CreateOrUpdateEchantillonAsync` | **Aucun contrôle** — distribution enregistrée même si stock = 0 |

---

## PARTIE 2 — Planification d'une visite (DÉLÉGUÉ)

### 2.1 MAUI — absence d'écran de planning

**Aucun écran de planning n'existe dans MAUI.** Les fichiers lus ne contiennent ni `PlanningListPage`, ni `PlanningFormPage`, ni ViewModel correspondant. Le DÉLÉGUÉ **ne peut pas créer de planning depuis l'application mobile**.

### 2.2 Angular — `PlanningFormComponent`

**Formulaire réactif (`planning-form.component.ts`) :**
```typescript
this.form = this.fb.group({
  id_User_Delegue: [null, [Validators.required]],   // sélection délégué (dropdown)
  date:            ['',   [Validators.required]],   // date du planning
  heureDebut:      [''],                            // heure début (non obligatoire)
  heureFin:        [''],                            // heure fin (non obligatoire)
  etatPlanning:    [EtatPlanning.EnAttente]          // statut initial par défaut
});
```

**Options statut `EtatPlanning` :**
| Valeur | Label affiché |
|--------|--------------|
| `EnAttente` (0) | "En attente" |
| `Confirme` (1) | "Confirmé" |
| `Annule` (2) | "Annulé" |

### 2.3 API appelée — Création/MAJ

```
POST http://cynapharmgateway.runasp.net/fields/plannings/{everything}
→ downstream : POST http://cynapharmfields.runasp.net/api/plannings
→ Roles autorisés : ADMIN, SUPERVISEUR, DELEGUE
```

> ⚠️ **Problème Ocelot :** La route est `/fields/plannings/{everything}`. Un POST vers `/fields/plannings`
> (sans segment après) peut ne pas être captée si `{everything}` ne matche pas la chaîne vide.
> Les appels avec sous-chemin (ex: `/fields/plannings/by-delegue/5`) fonctionnent.

**Corps de la requête (`PlanningVisiteDto`) :**
```json
{
  "id_Planning":      0,
  "date":             "2026-06-01T00:00:00",
  "heureDebut":       "08:00:00",
  "heureFin":         "17:00:00",
  "etat":             0,
  "id_User_Delegue":  7
}
```

**Réponse succès (200 OK) :**
```json
{
  "isSuccess": true,
  "message":  "Planning enregistré avec succès.",
  "result": {
    "id_Planning":     42,
    "date":            "2026-06-01T00:00:00",
    "heureDebut":      "08:00:00",
    "heureFin":        "17:00:00",
    "etat":            0,
    "id_User_Delegue": 7
  }
}
```

**Réponse erreur conflit horaire (400 Bad Request) :**
```json
{
  "isSuccess": false,
  "message":   "Erreur — conflit horaire ou données invalides."
}
```

### 2.4 Règles de validation

**Angular (FormBuilder) :**
- `id_User_Delegue` : obligatoire
- `date` : obligatoire
- `heureDebut` / `heureFin` : non obligatoires

**Backend — `PlanningVisiteController.CreateUpdatePlanningVisite` :**
- `!ModelState.IsValid` → 400 `"Données de planning invalides."`
- Si service retourne `null` → 400 `"Erreur — conflit horaire ou données invalides."`

**Backend — `PlanningService.CreateOrUpdatePlanningAsync` (comportement inféré) :**
- Retourne `null` en cas de conflit horaire ou données invalides
- MAJ possible si `id_Planning > 0` (chargement entité existante)

### 2.5 Ce que voit l'ADMIN dans Angular

| Action | Endpoint |
|--------|---------|
| Voir tous les plannings | `GET /fields/plannings?startDate=&endDate=` — Roles : ADMIN, SUPERVISEUR |
| Voir plannings d'un délégué | `GET /fields/plannings/by-delegue/{idDelegue}` — Roles : ADMIN, SUPERVISEUR, DELEGUE |
| Voir planning par plage dates | `GET /fields/plannings/by-range?idDelegue=&startDate=&endDate=` |
| Valider un planning | `PUT /fields/plannings/{idPlanning}/validate` — Roles : ADMIN, SUPERVISEUR |
| Supprimer un planning | `DELETE /fields/plannings/{idPlanning}` — Roles : ADMIN, SUPERVISEUR |

**Règle suppression :** Impossible si `Etat == Confirme` → 400 `"Suppression impossible (planning confirmé ou introuvable)."`

**Angular `planning-list.component` affiche :**
- Filtre dropdown par délégué
- Badges statut colorés : EnAttente (gris), Confirmé (vert), Annulé (rouge)
- Colonnes : ID, Délégué, Date, HeureDebut, HeureFin, Statut, Actions (Valider / Supprimer)

---

## PARTIE 3 — Création du rapport de visite

### 3.1 Timing : avant ou après la visite ?

**Le rapport est créé APRÈS la visite mais AVANT sa clôture.**

Chronologie obligatoire imposée par le backend :
```
1. Visite créée (IsCompleted = false, Rapport = null)
         ↓
2. Rapport créé (Id_Rapport = 0 → INSERT)
         ↓
3. Visite clôturée (IsCompleted = true)
         [via PUT /visites/{id}/complete  OU  PUT /rapports/{id}/validate]
```

**`CanCreateRapportAsync` (vérifié avant toute création) :**
```csharp
if (visite == null)                                  return false;  // visite inexistante
if (visite.IsCompleted || visite.Rapport != null)    return false;  // déjà clôturée OU rapport existe
return true;
```

**Relation 1-pour-1 :** `Visite` → `Rapport_Visite?` (navigation `virtual Rapport_Visite? Rapport`).
Un seul rapport par visite, jamais deux.

### 3.2 API appelée — Création (MAUI)

```
POST http://cynapharmgateway.runasp.net/fields/rapports/createUpdate
→ downstream : POST http://cynapharmfields.runasp.net/api/rapports/createUpdate
→ Roles : ADMIN, DELEGUE
```

**Payload MAUI (`VisiteService.CreateRapportAsync`) :**
```json
{
  "Id_Rapport":      0,
  "Id_Visite":       15,
  "Commentaire":     "Visite productive. Le médecin a montré de l'intérêt pour le produit X...",
  "Resultat":        "POSITIF",
  "Id_User_Delegue": 7,
  "Latitude":        36.8065,
  "Longitude":       10.1815
}
```

> ℹ️ **Mapping MAUI → Backend :**
> - `Rapport.Contenu` (MAUI) → `Commentaire` (backend)
> - `Rapport.Id` → `Id_Rapport` (0 = création)
> - Mapping manuel dans `VisiteService.CreateRapportAsync`

### 3.3 API appelée — Création (Angular)

```
POST http://cynapharmgateway.runasp.net/fields/rapports/createUpdate
→ Roles : ADMIN, DELEGUE
```

**Payload Angular (`RapportFormComponent.submit`) :**
```json
{
  "idRapport":        0,
  "id_User_Delegue":  7,
  "id_Visite":        15,
  "commentaire":      "Texte libre du rapport",
  "resultat":         "POSITIF"
}
```

> ⚠️ **Casse JSON :** Angular envoie `commentaire` (minuscule camelCase). Le `RapportVisiteDto` C# attend
> `Commentaire` (PascalCase). Fonctionne uniquement si `Program.cs` configure
> `JsonNamingPolicy.CamelCase` ou `JsonSerializerOptions` permissifs.

### 3.4 Champs requis (`RapportVisiteDto` + validations service)

| Champ | Obligatoire | Validation |
|-------|------------|-----------|
| `Id_Visite` | ✅ | Visite doit exister en base |
| `Commentaire` | ✅ | `[Required]` + `!string.IsNullOrWhiteSpace` |
| `Resultat` | ✅ | `[Required]` + `!string.IsNullOrWhiteSpace` |
| `Id_User_Delegue` | ✅ | Doit = `visite.Id_User_Delegue` (ownership check) |
| `Id_Rapport` | — | 0 = création, >0 = mise à jour |
| `Latitude` | ❌ | `double?` nullable — GPS optionnel |
| `Longitude` | ❌ | `double?` nullable — GPS optionnel |

**Validations complètes dans `RapportService.CreateOrUpdateRapportAsync` :**
```csharp
if (visite == null)                                return null;  // visite inexistante
if (string.IsNullOrWhiteSpace(dto.Commentaire))   return null;  // commentaire vide
if (string.IsNullOrWhiteSpace(dto.Resultat))      return null;  // résultat vide
if (visite.IsCompleted)                            return null;  // visite déjà clôturée
if (visite.Id_User_Delegue != dto.Id_User_Delegue) return null; // ownership check
// Si création (Id_Rapport == 0) :
if (visite.Rapport != null)                        return null; // rapport existe déjà
// Si MAJ (Id_Rapport > 0) :
// rapport doit matcher Id_Visite et Id_User_Delegue
```

**Validation minimale MAUI :** `Contenu` ≥ 20 caractères (annotation MVVM `[MinLength(20)]`).

### 3.5 GPS dans MAUI (`RapportViewModel`)

```
OnAppearing → PreCaptureLocationAsync()
    → GetLastKnownLocationAsync()  (rapide, pas de dialog)
    → GeoStatus = "📍 Dernière position : lat, lon (il y a N min)"

OnSubmit → CaptureLocationAsync()
    → RequestAsync<Permissions.LocationWhenInUse>()
    → GetLocationAsync(accuracy: Medium, timeout: 10 sec)
    → (lat, lon) inclus dans le payload
```

Si GPS refusé ou indisponible : `Latitude = null`, `Longitude = null` → payload envoyé quand même.

**Mode hors ligne :** Sauvegarde dans `PendingRapportEntry` (SQLite) :
```csharp
await _localDb.InsertPendingRapportAsync(new PendingRapportEntry {
    VisiteId = rapport.VisiteId,
    Contenu  = rapport.Contenu,
    Resultat = rapport.Resultat,
    Latitude = lat, Longitude = lon,
    IsSynced = false
});
```
> ⚠️ Aucun mécanisme de synchronisation automatique des rapports `IsSynced = false` n'a été trouvé.

### 3.6 Validation du rapport par ADMIN/SUPERVISEUR

**Seul le rôle `SUPERVISEUR` peut valider** (pas `ADMIN`) :
```
PUT http://cynapharmgateway.runasp.net/fields/rapports/{idRapport}/validate?idSuperviseur={id}
→ Roles : SUPERVISEUR uniquement
```

**Effet dans `ValidateRapportAsync` :**
```csharp
rapport.Visite.IsCompleted = true;  // Clôture la visite
await _db.SaveChangesAsync();
return true;
```

> ⚠️ **Bug traçabilité :** `idSuperviseur` est reçu en query param mais **non utilisé** dans l'implémentation.
> Impossible de savoir quel superviseur a validé quel rapport.

**Clôture alternative directe par le DÉLÉGUÉ :**
```
PUT http://cynapharmgateway.runasp.net/fields/visites/{idVisite}/complete
→ Roles : DELEGUE
→ Condition obligatoire : visite.Rapport != null
```

**ADMIN voit tous les rapports :**
```
GET http://cynapharmgateway.runasp.net/fields/rapports/all
→ Roles : ADMIN, SUPERVISEUR
→ Tri : DateRapport DESC
```

Angular `rapport-list.component` affiche : délégué, visite liée, commentaire, résultat, date, statut validation.

---

## PARTIE 4 — Distribution d'échantillons

### Étape 1 : DÉLÉGUÉ sélectionne le stock + lot

**Angular (`DistributionFormComponent`) :**
1. Dropdown `id_Delegue` → valueChanges déclenche `loadStocks(+id)`
2. Appel : `GET /inventory/stocks-delegue/by-delegue/{idDelegue}`
3. Dropdown `id_Stock` → affiche `NumeroLot — QteDisponible unités`
4. Sélection stock → auto-rempli `numeroLot` via `valueChanges` :
   ```typescript
   const stock = this.stocks.find(s => s.id_stock === +id);
   if (stock) this.form.patchValue({ numeroLot: stock.numeroLot });
   ```

**MAUI (`MyStockViewModel.DistributeSampleAsync`) :**
- Segment "Échantillons" uniquement
- Utilisateur appuie sur item → distribution immédiate de **1 unité**
- Pas de sélection de lot spécifique : basé sur `ProductId`

### Étape 2 : DÉLÉGUÉ sélectionne le destinataire

**Angular :** Dropdown `id_Medecin` OU `id_Pharmacien` (au moins un requis)

Validator personnalisé du formulaire :
```typescript
function recipientRequired(form: AbstractControl): ValidationErrors | null {
  const med = form.get('id_Medecin')?.value;
  const pha = form.get('id_Pharmacien')?.value;
  return (!med && !pha) ? { recipientRequired: true } : null;
}
// Erreur visible : this.form.errors?.['recipientRequired'] && this.form.touched
```

**MAUI :** **Aucune sélection de destinataire.** `PostDistributionAsync` n'inclut ni `Id_Medecin` ni `Id_Pharmacien`.

### Étape 3 : DÉLÉGUÉ saisit la quantité

**Angular :** Champ `qte` — `[Validators.required, Validators.min(1)]`. Pas de `Validators.max(stock.qteDisponible)`.

**MAUI :** Fixé à **1 unité** (aucune saisie possible) :
```csharp
await _inventoryService.PostDistributionAsync(item.ProductId, 1);
```

### Étape 4 : API appelée

**URL :**
```
POST http://cynapharmgateway.runasp.net/inventory/distributions/{everything}
→ downstream : POST http://cynapharminventories.runasp.net/api/distributions
→ Roles : ADMIN, SUPERVISEUR, DELEGUE
```

**Corps ATTENDU par le backend (`EchantillonDto`) :**
```json
{
  "id_Distribution": 0,
  "id_Delegue":      7,
  "id_Medecin":      12,
  "id_Pharmacien":   null,
  "id_Stock":        3,
  "qte":             2,
  "numeroLot":       "LOT-2024-001",
  "dateDistribution": null
}
```
> `dateDistribution` est ignoré côté backend — remplacé par `DateTime.UtcNow` dans le controller.

**Corps ENVOYÉ par MAUI (`PostDistributionAsync`) :**
```json
{
  "ProductId":          42,
  "QuantiteDistribuee": 1,
  "DateDistribution":   "2026-05-22T10:30:00Z",
  "Latitude":           null,
  "Longitude":          null
}
```

> 🔴 **BUG CRITIQUE (MAUI) :** Le payload MAUI est **entièrement incompatible** avec `EchantillonDto`.
> Le backend attend `Id_Delegue`, `Id_Stock`, `Qte`, `NumeroLot`.
> MAUI envoie `ProductId`, `QuantiteDistribuee`.
> → `ModelState` invalide → 400 Bad Request.
> → L'appel est **fire-and-forget** — l'erreur est loggée silencieusement, le DÉLÉGUÉ ne voit rien.

**Réponse succès (200 OK) :**
```json
{
  "isSuccess": true,
  "message":   "Distribution enregistrée avec succès."
}
```
> ℹ️ `result` est `null` — le service retourne `bool`, pas l'objet `Echantillon` créé.

**Réponse erreur (400 Bad Request) :**
```json
{
  "isSuccess": false,
  "message":   "Erreur lors de l'enregistrement de la distribution."
}
```

### Étape 5 : Ce qui se passe au stock après distribution

**Code complet de `DistributionService.CreateOrUpdateEchantillonAsync` :**
```csharp
public async Task<bool> CreateOrUpdateEchantillonAsync(Echantillon echantillon)
{
    var distribution = await _db.Echantillons
        .FirstOrDefaultAsync(e => e.Id_Distribution == echantillon.Id_Distribution);

    if (distribution == null)
    {
        echantillon.DateDistribution = DateTime.UtcNow;
        echantillon.IsDeleted = false;
        _db.Echantillons.Add(echantillon);  // Crée l'enregistrement
    }
    else
    {
        _mapper.Map(echantillon, distribution);  // Met à jour
    }

    await _db.SaveChangesAsync();
    return true;
    // FIN — aucun accès à Stock_Delegue
}
```

| Question | Réponse | Preuve code |
|----------|---------|------------|
| `QteDisponible` décrémenté ? | **NON** | Aucun accès à `_db.StocksDelegues` dans `DistributionService` |
| `StockMouvement` créé ? | **NON** | Aucun `_db.StockMouvements.Add(...)` |
| `Id_Stock` validé (existence) ? | **NON** | Pas de `FirstOrDefaultAsync` sur `StocksDelegues` |
| Quantité disponible vérifiée ? | **NON** | Aucune comparaison `qte vs QteDisponible` |
| Lot expiré bloqué ? | **NON** | `DateExpiration` non vérifiée |

---

## PARTIE 5 — Mise à jour du stock

### 5.1 Comportement réel après une distribution

**Backend :** `Stock_Delegue.QteDisponible` **n'est jamais décrémenté** lors d'une distribution.
Le service `DistributionService` n'accède pas à la table `StocksDelegues`.

**MAUI :** Décrémentation **locale uniquement** (deux endroits) :
```csharp
// 1. SQLite local
var success = await _localDb.DeductStockAsync(item.ProductId, 1);

// 2. In-memory (liste affichée)
var src = _echantillonStock.FirstOrDefault(s => s.ProductId == item.ProductId);
if (src != null) src.QuantiteRestante = Math.Max(0, src.QuantiteRestante - 1);
```
Au prochain `RefreshAsync()`, le rechargement depuis le serveur **écrase** ces décrément locaux.

### 5.2 Y a-t-il un `StockMouvement` créé ?

**NON.** Aucune création de `StockMouvement` dans `DistributionService` ni dans tout autre service analysé.

L'endpoint `/inventory/stock-movements/by-delegue/{id}` est déclaré dans ocelot.json et appelé par MAUI,
mais **aucun controller backend** n'a été trouvé pour `/api/stock-movements`.

### 5.3 Seule façon de mettre à jour le stock (ADMIN/SUPERVISEUR uniquement)

```
POST http://cynapharmgateway.runasp.net/inventory/stocks-delegue/{everything}
→ POST /api/stocks-delegue
→ Roles : ADMIN, SUPERVISEUR
```

Corps :
```json
{
  "id_stock":        3,
  "id_User_Delegue": 7,
  "id_Produit":      12,
  "numeroLot":       "LOT-2024-001",
  "dateExpiration":  "2027-12-31T00:00:00",
  "qteDisponible":   45,
  "qteReservee":     0
}
```

**`CreateUpdateStockAsync` met à jour UNIQUEMENT :**
```csharp
stock.QteDisponible = dto.QteDisponible;
stock.NumeroLot = dto.NumeroLot;
// DateExpiration, Id_User_Delegue, Id_Produit ne sont PAS mis à jour en mode UPDATE
```

### 5.4 Ce que l'ADMIN voit

**Distributions (paginé) :**
```
GET /inventory/distributions?pageNumber=1&pageSize=20
→ Roles : ADMIN, SUPERVISEUR
```
Colonnes disponibles : `Id_Distribution`, `Id_Delegue`, `Id_Medecin`, `Id_Pharmacien`, `Id_Stock`, `Qte`, `NumeroLot`, `DateDistribution`

**Aucune vue mouvements de stock** accessible via un endpoint fonctionnel confirmé.

### 5.5 Suppression d'une distribution (soft delete)

```
DELETE /inventory/distributions/{idDistribution}
→ Roles : ADMIN, SUPERVISEUR
```

```csharp
distribution.IsDeleted = true;  // Soft delete uniquement
await _db.SaveChangesAsync();
// Stock NON réincrémenté
```

---

## PARTIE 6 — Business Logic Verification

### Tableau de vérification

| Étape | Attendu | Réel | Statut |
|-------|---------|------|--------|
| DÉLÉGUÉ distribue plus que stock dispo | Bloqué (400 / erreur UI) | Non bloqué — distribution créée même si `Qte > QteDisponible` | ❌ BUG |
| Vérification expiration lot avant distribution | Lot expiré → refusé | Non vérifié (ni backend ni frontend) | ❌ MANQUANT |
| Stock = 0 bloqué | Distribution impossible | MAUI : oui localement / Angular : non / Backend : non | ⚠️ PARTIEL |
| Même lot distribué deux fois | Idempotence ou erreur | Possible — aucune contrainte unique sur `(Id_Stock, Id_Delegue)` | ❌ BUG |
| `Id_Medecin` ET `Id_Pharmacien` null | 400 ou validation bloquante | Angular : bloqué (validator) / MAUI : non / Backend : non | ⚠️ PARTIEL |
| `Id_Stock` inexistant | 400 | Non vérifié dans `CreateOrUpdateEchantillonAsync` | ❌ MANQUANT |
| `QteDisponible` décrémenté après distribution | Oui | Non | ❌ BUG |
| `StockMouvement` créé après distribution | Oui (traçabilité) | Non | ❌ MANQUANT |
| Stock réincrémenté si distribution supprimée | Oui | Non | ❌ BUG |
| Rapport obligatoire avant clôture visite | Oui | ✅ `CompleteVisiteAsync` bloque si `Rapport == null` | ✅ OK |
| 1 seul rapport par visite | Oui | ✅ `CreateOrUpdateRapportAsync` bloque si `visite.Rapport != null` | ✅ OK |
| Ownership délégué sur rapport | Oui | ✅ `visite.Id_User_Delegue != dto.Id_User_Delegue → return null` | ✅ OK |
| Visite complétée non modifiable | Oui | ✅ `if (visite.IsCompleted) return null` dans service | ✅ OK |
| Planning confirmé non supprimable | Oui | ✅ Bloqué si `Etat == Confirme` | ✅ OK |
| DÉLÉGUÉ accède à son stock MAUI | Oui | MAUI appelle `GET /api/stocks-delegue` (ADMIN only) → 403 | ❌ BUG |
| Payload distribution MAUI compatible backend | Oui | Payload totalement différent — 400 silencieux | ❌ BUG |
| `idSuperviseur` tracé lors validation rapport | Oui | Paramètre reçu mais ignoré dans le service | ⚠️ PARTIEL |
| Rapport hors ligne synchronisé automatiquement | Oui | Sauvegardé SQLite mais sync automatique absente | ⚠️ PARTIEL |
| DÉLÉGUÉ peut créer planning depuis MAUI | Oui | Aucun écran planning dans MAUI | ❌ MANQUANT |

### Réponses précises aux questions

**Can DELEGUE distribute more than available stock?**
**OUI côté backend.** `DistributionService.CreateOrUpdateEchantillonAsync` n'accède jamais à `Stock_Delegue`.
Seule protection : MAUI vérifie `QuantiteRestante <= 0` localement (non rechargée en temps réel).

**Is lot expiration checked before distribution?**
**NON.** Ni `DistributionService`, ni `DistributionFormComponent` Angular, ni `DistributeSampleAsync` MAUI
ne vérifient `DateExpiration`. Un lot expiré peut être distribué.

**Is stock = 0 blocked?**
**PARTIEL.** MAUI bloque si `QuantiteRestante <= 0` (local SQLite). Angular pas de max validation.
Backend aucune validation.

**Can same lot be distributed twice?**
**OUI.** `Id_Distribution == 0` → `_db.Echantillons.Add(echantillon)` crée toujours un nouvel enregistrement.
Aucune contrainte unique sur `(NumeroLot, Id_Delegue, Id_Medecin)` dans la table `Echantillons`.

**What happens if médecin AND pharmacien are both null?**
- Angular : bloqué via `recipientRequired` validator (formulaire invalide, soumission impossible)
- MAUI : `PostDistributionAsync` n'envoie aucun `Id_Medecin` / `Id_Pharmacien` — backend reçoit null pour les deux
- Backend : **aucune validation** — les deux champs sont `int?` nullable, distribution enregistrée sans destinataire

---

## PARTIE 7 — Features manquantes

### 7.1 Validations backend manquantes

| Validation manquante | Fichier à modifier | Impact |
|---------------------|------------------|--------|
| `qte <= QteDisponible` avant création `Echantillon` | `DistributionService.cs` | Sur-distribution possible |
| Décrémentation `QteDisponible -= qte` après distribution | `DistributionService.cs` | Stock jamais mis à jour |
| Réincrémentation `QteDisponible += qte` à la suppression | `DistributionService.cs` | Annulation sans effet sur le stock |
| Vérification `DateExpiration >= DateTime.UtcNow.Date` | `DistributionService.cs` | Lots expirés distribuables |
| Validation `Id_Stock` existe dans `StocksDelegues` | `DistributionService.cs` | Distribution sur stock fantôme |
| Au moins un destinataire (`Id_Medecin OR Id_Pharmacien`) | `DistributionService.cs` | Distribution sans traçabilité |
| Création d'un `StockMouvement` à chaque distribution | `DistributionService.cs` | Aucun historique de mouvement |
| `idSuperviseur` sauvegardé dans la validation rapport | `RapportService.cs` | Qui a validé ? Inconnu |

### 7.2 Écrans MAUI manquants

| Écran manquant | Description |
|----------------|-------------|
| `PlanningListPage` | Liste les plannings du DÉLÉGUÉ |
| `PlanningFormPage` | Créer/modifier un planning depuis mobile |
| `DistributionFormPage` | Formulaire complet : sélection destinataire (médecin/pharmacien) + lot + quantité |
| `VisiteDetailPage` (Field) | Détail d'une visite avec bouton "Compléter" |
| Sync automatique rapports pending | Mécanisme de synchro au retour réseau des `PendingRapportEntry.IsSynced = false` |

### 7.3 Bugs MAUI à corriger

| Bug | Fichier | Correction |
|----|--------|-----------|
| `GetStockDelegueAsync()` appelle la route ADMIN | `InventoryService.cs` | Remplacer par `GET /inventory/stocks-delegue/by-delegue/{userId}` |
| `PostDistributionAsync` envoie le mauvais payload | `InventoryService.cs` | Envoyer `EchantillonDto` complet |
| Décrémentation locale non persistée au refresh | `MyStockViewModel.cs` | Le backend doit décrémenter le stock |
| Erreur distribution invisible (fire-and-forget) | `MyStockViewModel.cs` | Afficher l'erreur à l'utilisateur |
| Mismatch noms propriétés `StockDelegue` MAUI | `StockDelegue.cs` | Ajouter `[JsonPropertyName("qteDisponible")]` etc. |

### 7.4 Endpoints manquants / non fonctionnels

| Endpoint | Problème |
|----------|---------|
| `GET /api/stock-movements/by-delegue/{id}` | Ocelot route déclarée, appelée par MAUI, mais aucun controller trouvé |
| `GET /api/inventory-business/summary/{id}` | Ocelot route déclarée, appelée par MAUI, mais aucun controller trouvé |
| `GET /api/stocks-promotionnels` | Ocelot route déclarée, appelée par MAUI, mais aucun controller trouvé |
| `POST /fields/visites` (base path) | Ocelot route `/{everything}` peut ne pas capturer le chemin de base |
| `POST /inventory/distributions` (base path) | Idem |

### 7.5 UI Angular manquante

| Fonctionnalité | Composant | Ce qui manque |
|---------------|-----------|--------------|
| Contrôle max quantité vs `qteDisponible` | `distribution-form.component` | `Validators.max(selectedStock.qteDisponible)` |
| Warning lot expiré | `distribution-form.component` | Alert si `dateExpiration < today` |
| Vérification "déjà un rapport" | `rapport-form.component` | Appel `GET /fields/rapports/can-create/{idVisite}` avant affichage |
| Capture GPS dans rapport Angular | `rapport-form.component` | Pas de géolocalisation |
| Bouton "Compléter" visite (DÉLÉGUÉ) | `visite-list/detail.component` | `PUT /fields/visites/{id}/complete` non utilisé en Angular |

### 7.6 Problèmes de configuration

| Problème | Localisation | Impact |
|----------|-------------|--------|
| Status code **515** retourné (non standard) | Tous les controllers (bloc catch) | Devrait être 500 |
| `ADMIN` ne peut pas valider un rapport | `RapportsController.ValidateRapport` | Rôle `SUPERVISEUR` uniquement — ADMIN bloqué |
| `ADMIN` ne peut pas valider un planning | Idem — rôle ADMIN inclus | OK ici |

---

## PARTIE 8 — Tableau complet des endpoints utilisés

### FieldAPI — `cynapharmfields.runasp.net`

| Méthode | Route Gateway (Ocelot) | Route Downstream | Rôles autorisés | Description |
|---------|----------------------|-----------------|----------------|-------------|
| `POST` | `/fields/visites` | `/api/visites` | DELEGUE, ADMIN, SUPERVISEUR | Créer/MAJ une visite (IdDelegue extrait du JWT) |
| `GET` | `/fields/visites/{idVisite}` | `/api/visites/{idVisite}` | DELEGUE, ADMIN, SUPERVISEUR | Détail d'une visite |
| `GET` | `/fields/visites/by-delegue/{idDelegue}` | `/api/visites/by-delegue/{idDelegue}` | DELEGUE, ADMIN, SUPERVISEUR | Toutes les visites d'un délégué |
| `GET` | `/fields/visites/by-planning/{idPlanning}` | `/api/visites/by-planning/{idPlanning}` | DELEGUE, ADMIN, SUPERVISEUR | Visites rattachées à un planning |
| `GET` | `/fields/visites` | `/api/visites?startDate=&endDate=` | ADMIN, SUPERVISEUR | Toutes les visites (filtres optionnels) |
| `DELETE` | `/fields/visites/{idVisite}` | `/api/visites/{idVisite}` | ADMIN, SUPERVISEUR | Supprimer visite (bloqué si complétée ou avec rapport) |
| `PUT` | `/fields/visites/{idVisite}/planning/{idPlanning}` | `/api/visites/{idVisite}/planning/{idPlanning}` | DELEGUE | Affecter visite à un planning |
| `PUT` | `/fields/visites/{idVisite}/complete` | `/api/visites/{idVisite}/complete` | DELEGUE | Clôturer une visite (rapport obligatoire) |
| `POST` | `/fields/rapports/createUpdate` | `/api/rapports/createUpdate` | ADMIN, DELEGUE | Créer/MAJ un rapport de visite |
| `GET` | `/fields/rapports/{id}` | `/api/rapports/{id}` | DELEGUE, ADMIN, SUPERVISEUR | Rapport par ID |
| `GET` | `/fields/rapports/by-visite/{idVisite}` | `/api/rapports/by-visite/{idVisite}` | DELEGUE, ADMIN, SUPERVISEUR | Rapport d'une visite spécifique |
| `GET` | `/fields/rapports/by-delegue/{idDelegue}` | `/api/rapports/by-delegue/{idDelegue}` | ADMIN, SUPERVISEUR | Tous les rapports d'un délégué |
| `GET` | `/fields/rapports/all` | `/api/rapports/all` | ADMIN, SUPERVISEUR | Tous les rapports (tri DateRapport DESC) |
| `DELETE` | `/fields/rapports/{idRapport}` | `/api/rapports/{idRapport}` | DELEGUE, ADMIN | Supprimer rapport (bloqué si visite complétée) |
| `PUT` | `/fields/rapports/{idRapport}/validate` | `/api/rapports/{idRapport}/validate?idSuperviseur=` | SUPERVISEUR | Valider rapport → clôture visite |
| `GET` | `/fields/rapports/can-create/{idVisite}` | `/api/rapports/can-create/{idVisite}` | DELEGUE | Peut-on créer un rapport pour cette visite ? |
| `GET` | `/fields/rapports/has-rapport/{idVisite}` | `/api/rapports/has-rapport/{idVisite}` | DELEGUE, ADMIN, SUPERVISEUR | La visite a-t-elle déjà un rapport ? |
| `POST` | `/fields/plannings` | `/api/plannings` | DELEGUE, ADMIN, SUPERVISEUR | Créer/MAJ un planning de visite |
| `GET` | `/fields/plannings/{idPlanning}` | `/api/plannings/{idPlanning}` | DELEGUE, ADMIN, SUPERVISEUR | Détail d'un planning |
| `GET` | `/fields/plannings` | `/api/plannings?startDate=&endDate=` | ADMIN, SUPERVISEUR | Tous les plannings (filtres optionnels) |
| `GET` | `/fields/plannings/by-delegue/{idDelegue}` | `/api/plannings/by-delegue/{idDelegue}` | DELEGUE, ADMIN, SUPERVISEUR | Plannings d'un délégué |
| `GET` | `/fields/plannings/by-range` | `/api/plannings/by-range?idDelegue=&startDate=&endDate=` | DELEGUE, ADMIN, SUPERVISEUR | Plannings par plage de dates |
| `GET` | `/fields/plannings/by-date` | `/api/plannings/by-date?idDelegue=&date=` | DELEGUE, ADMIN, SUPERVISEUR | Planning d'un délégué à une date précise |
| `DELETE` | `/fields/plannings/{idPlanning}` | `/api/plannings/{idPlanning}` | ADMIN, SUPERVISEUR | Supprimer planning (bloqué si Confirmé) |
| `PUT` | `/fields/plannings/{idPlanning}/validate` | `/api/plannings/{idPlanning}/validate` | ADMIN, SUPERVISEUR | Valider planning (Etat → Confirme) |

---

### InventoryAPI — `cynapharminventories.runasp.net`

| Méthode | Route Gateway (Ocelot) | Route Downstream | Rôles autorisés | Description |
|---------|----------------------|-----------------|----------------|-------------|
| `GET` | `/inventory/stocks-delegue` | `/api/stocks-delegue?pageNumber=&pageSize=` | ADMIN, SUPERVISEUR | Tous les stocks délégués (paginé) |
| `GET` | `/inventory/stocks-delegue/{idStock}` | `/api/stocks-delegue/{idStock}` | DELEGUE, ADMIN, SUPERVISEUR | Stock par ID |
| `GET` | `/inventory/stocks-delegue/by-delegue/{idDelegue}` | `/api/stocks-delegue/by-delegue/{idDelegue}` | DELEGUE, ADMIN, SUPERVISEUR | Stocks d'un délégué |
| `GET` | `/inventory/stocks-delegue/by-produit/{idProduit}` | `/api/stocks-delegue/by-produit/{idProduit}` | ADMIN, SUPERVISEUR | Stocks d'un produit donné |
| `GET` | `/inventory/stocks-delegue/by-lot/{numeroLot}` | `/api/stocks-delegue/by-lot/{numeroLot}` | ADMIN, SUPERVISEUR | Stock par numéro de lot |
| `POST` | `/inventory/stocks-delegue` | `/api/stocks-delegue` | ADMIN, SUPERVISEUR | Créer/MAJ un stock délégué |
| `DELETE` | `/inventory/stocks-delegue/{idStock}` | `/api/stocks-delegue/{idStock}?type=Delegue` | ADMIN | Supprimer stock (bloqué si QteDisponible > 0) |
| `POST` | `/inventory/distributions` | `/api/distributions` | DELEGUE, ADMIN, SUPERVISEUR | Enregistrer une distribution d'échantillon |
| `GET` | `/inventory/distributions` | `/api/distributions?pageNumber=&pageSize=` | ADMIN, SUPERVISEUR | Toutes les distributions (paginé) |
| `GET` | `/inventory/distributions/{idDistribution}` | `/api/distributions/{idDistribution}` | DELEGUE, ADMIN, SUPERVISEUR | Détail d'une distribution |
| `GET` | `/inventory/distributions/by-medecin/{idMedecin}` | `/api/distributions/by-medecin/{idMedecin}` | DELEGUE, ADMIN, SUPERVISEUR | Distributions vers un médecin |
| `GET` | `/inventory/distributions/by-delegue/{idDelegue}` | `/api/distributions/by-delegue/{idDelegue}` | DELEGUE, ADMIN, SUPERVISEUR | Distributions faites par un délégué |
| `GET` | `/inventory/distributions/by-pharmacien/{idPharmacien}` | `/api/distributions/by-pharmacien/{idPharmacien}` | DELEGUE, ADMIN, SUPERVISEUR | Distributions vers un pharmacien |
| `DELETE` | `/inventory/distributions/{idDistribution}` | `/api/distributions/{idDistribution}` | ADMIN, SUPERVISEUR | Soft-delete une distribution (stock non réincrémenté) |
| `GET` | `/inventory/stock-movements/by-delegue/{id}` | `/api/stock-movements/by-delegue/{id}` | — | ⚠️ Ocelot déclaré, MAUI l'appelle — **aucun controller trouvé** |
| `GET` | `/inventory/inventory-business/summary/{id}` | `/api/inventory-business/summary/{id}` | — | ⚠️ Ocelot déclaré, MAUI l'appelle — **aucun controller trouvé** |
| `GET` | `/inventory/stocks-promotionnels` | `/api/stocks-promotionnels` | — | ⚠️ Ocelot déclaré, MAUI l'appelle — **aucun controller trouvé** |

---

## Résumé des bugs prioritaires

| Priorité | Bug | Fichier | Correction suggérée |
|----------|-----|--------|---------------------|
| 🔴 P1 | `DistributionService` ne décrémente pas `QteDisponible` | `DistributionService.cs` | Charger le `Stock_Delegue`, vérifier et décrémenter `QteDisponible -= Qte` |
| 🔴 P1 | `DistributionService` ne valide pas `Qte <= QteDisponible` | `DistributionService.cs` | `if (stock.QteDisponible < echantillon.Qte) return false` |
| 🔴 P1 | MAUI `PostDistributionAsync` envoie le mauvais payload | `InventoryService.cs` | Construire un `EchantillonDto` complet avec `Id_Delegue`, `Id_Stock`, `Qte`, `NumeroLot` |
| 🔴 P1 | MAUI `GetStockDelegueAsync()` appelle la route ADMIN | `InventoryService.cs` | Remplacer par `GET /inventory/stocks-delegue/by-delegue/{userId}` |
| 🟠 P2 | Aucun `StockMouvement` créé lors d'une distribution | `DistributionService.cs` | Créer `StockMouvement(Type=Sortie, Qte, ...)` après chaque distribution |
| 🟠 P2 | Lot expiré distribuable | `DistributionService.cs` | `if (stock.DateExpiration < DateTime.UtcNow.Date) return false` |
| 🟠 P2 | Distribution sans destinataire acceptée | `DistributionService.cs` | `if (e.Id_Medecin == null && e.Id_Pharmacien == null) return false` |
| 🟠 P2 | Stock non réincrémenté à la suppression d'une distribution | `DistributionService.cs` | `stock.QteDisponible += distribution.Qte` dans `DeleteEchantillonAsync` |
| 🟡 P3 | `idSuperviseur` ignoré dans la validation rapport | `RapportService.cs` | Ajouter `rapport.IdSuperviseurValidateur = idSuperviseur` |
| 🟡 P3 | Aucun écran planning dans MAUI | MAUI project | Créer `PlanningListPage` + `PlanningFormPage` |
| 🟡 P3 | MAUI : pas de `DistributionFormPage` avec destinataire | MAUI project | Créer écran de sélection médecin/pharmacien + quantité |
| 🟡 P3 | Status code 515 non standard | Tous les controllers | Remplacer par 500 |
| 🟡 P3 | Mismatch noms propriétés `StockDelegue` MAUI vs backend | `StockDelegue.cs` MAUI | Ajouter `[JsonPropertyName]` sur les propriétés |
