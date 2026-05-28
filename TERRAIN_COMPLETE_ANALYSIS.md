# TERRAIN — ANALYSE COMPLÈTE
**CynapSoft CRM — FieldAPI / Angular / MAUI**
**Date : 2026-05-26 | Branche : dev/Mobile-0001**

> ⚠️ Contrainte critique : `StatusCode(515)` est un code personnalisé utilisé dans TOUS les contrôleurs pour les exceptions — ce n'est PAS un bug et ne doit PAS être signalé comme tel.

---

## TABLE DES MATIÈRES

- [PARTIE 1 — Backend FieldAPI](#partie-1--backend-fieldapi)
- [PARTIE 2 — Angular (features/field)](#partie-2--angular-featuresfield)
- [PARTIE 3 — MAUI (Cynapharm-Mobile)](#partie-3--maui-cynapharm-mobile)
- [PARTIE 4 — Analyse Globale](#partie-4--analyse-globale)
- [PARTIE 5 — Code Source Complet](#partie-5--code-source-complet)

---

## PARTIE 1 — Backend FieldAPI

### 1.1 Vue d'ensemble du microservice

| Propriété | Valeur |
|-----------|--------|
| Projet | `CynapCRM.Services.FieldAPI` |
| Route de base | `api/` (via Ocelot : `/fields/`) |
| Auth | JWT Bearer — `ClaimTypes.NameIdentifier` = Id du délégué |
| ORM | Entity Framework Core + AutoMapper |
| Pattern | Repository → Service → Controller → ResponseDto |

### 1.2 Modèles (Entités)

#### Region
| Champ | Type | Contraintes |
|-------|------|-------------|
| Id_Region | int | PK, auto-increment |
| NomRegion | string | Required |
| CodePostal | int | Index |
| Id_User_Delegue | int | FK → User (Auth API), Index |

#### Planning_Visite
| Champ | Type | Contraintes |
|-------|------|-------------|
| Id_Planning | int | PK |
| Date | DateTime | Required |
| HeureDebut | TimeSpan | Required |
| HeureFin | TimeSpan | Required |
| Etat | EtatPlanning | Enum (default: EnAttente=0) |
| Id_User_Delegue | int | FK, Index |
| Visites | ICollection\<Visite\> | Nav. 1:N |

#### Visite
| Champ | Type | Contraintes |
|-------|------|-------------|
| Id_Visite | int | PK |
| DateVisite | DateTime | Required |
| Type | VisiteType | Enum Required |
| IsCompleted | bool | default false |
| Id_User_Delegue | int | FK, Index |
| Id_Medecin | int? | Nullable FK |
| Id_Pharmacien | int? | Nullable FK |
| Id_Planning | int? | FK → Planning_Visite (SetNull on delete) |
| Rapport | Rapport_Visite? | Nav. 1:1 |
| Planning | Planning_Visite? | Nav. N:1 |

#### Rapport_Visite
| Champ | Type | Contraintes |
|-------|------|-------------|
| Id_Rapport | int | PK |
| Commentaire | string | Required |
| Resultat | string | Required |
| DateRapport | DateTime | Required |
| Id_Visite | int | FK → Visite (Cascade delete) |
| Id_User_Delegue | int | Index |
| Id_SuperviseurValidateur | int? | Nullable |
| Latitude | double? | GPS, Nullable |
| Longitude | double? | GPS, Nullable |
| Visite | Visite | Nav. propriété inverse |

#### Objectif_Delegue
| Champ | Type | Contraintes |
|-------|------|-------------|
| Id_Objectif | int | PK |
| Type | TypeObjectif | Enum |
| Periode | PeriodeObjectif | Enum |
| ValeurCible | int | Required |
| ValeurRealisee | int | Calculé par KPIService |
| Id_User_Delegue | int | Index |
| DateDebut | DateTime | |
| DateFin | DateTime | |

### 1.3 Enums

| Enum | Valeurs |
|------|---------|
| EtatPlanning | EnAttente=**0**, Confirme=**1**, Annule=**2** |
| VisiteType | Medecin=**1**, Pharmacien=**2**, Autre=**3** *(commence à 1, pas 0)* |
| TypeObjectif | Visites=**0**, ChiffreAffaires=**1**, NouveauxClients=**2**, Fidelisation=**3** |
| PeriodeObjectif | Mensuel=**0**, Trimestriel=**1**, Annuel=**2** |

### 1.4 Relations et comportements de suppression

```
Planning_Visite 1 ←──(SetNull)──── N Visite 1 ←──(Cascade)──── 1 Rapport_Visite
     |                                   |
     └── Index Id_User_Delegue           └── Index Id_User_Delegue
```

- Supprimer un Planning → `Id_Planning = NULL` sur toutes les Visites liées (ne supprime pas les visites)
- Supprimer une Visite → supprime en cascade son `Rapport_Visite`
- Supprimer un Planning avec Etat = Confirme → **interdit** (règle métier)
- Supprimer une Visite avec `IsCompleted = true` ou avec un Rapport → **interdit**

### 1.5 DTOs critiques

#### PlanningVisiteDto — champ `Etat` (pas `EtatPlanning`)
```csharp
public class PlanningVisiteDto {
    public int Id_Planning { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan HeureDebut { get; set; }
    public TimeSpan HeureFin { get; set; }
    public EtatPlanning Etat { get; set; }      // ← "Etat", PAS "EtatPlanning"
    public int Id_User_Delegue { get; set; }
}
```

#### RapportVisiteDto — champ `Date` (mappé depuis `DateRapport` via AutoMapper)
```csharp
public class RapportVisiteDto {
    public int Id_Rapport { get; set; }
    public string Commentaire { get; set; } = string.Empty;
    public string Resultat { get; set; } = string.Empty;
    public DateTime Date { get; set; }           // ← "Date", PAS "DateRapport"
    public int Id_Visite { get; set; }
    public int Id_User_Delegue { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
```

#### CreateVisiteDto — champs obligatoires souvent manquants côté client
```csharp
public class CreateVisiteDto {
    public int IdVisite { get; set; }
    public DateTime DateVisite { get; set; }
    public VisiteType Type { get; set; }          // ← OBLIGATOIRE
    public int IdDelegue { get; set; }            // ← injecté depuis JWT côté contrôleur
    public int? IdMedecin { get; set; }
    public int? IdPharmacien { get; set; }
    public int? IdPlanning { get; set; }
}
```

#### PerformanceDto — structure retournée par KPIController
```csharp
public class PerformanceDto {
    public TypeObjectif Type { get; set; }
    public int ValeurCible { get; set; }
    public int ValeurRealisee { get; set; }
    public double Pourcentage { get; set; }
}
```

### 1.6 MappingConfig — mappings AutoMapper critiques

```csharp
// Visite → VisiteDto : HasRapport calculé
CreateMap<Visite, VisiteDto>()
    .ForMember(d => d.HasRapport, o => o.MapFrom(s => s.Rapport != null));

// Rapport_Visite → RapportVisiteDto : DateRapport → Date
CreateMap<Rapport_Visite, RapportVisiteDto>()
    .ForMember(d => d.Date, opt => opt.MapFrom(s => s.DateRapport))
    .ReverseMap()
    .ForMember(s => s.DateRapport, opt => opt.MapFrom(d => d.Date));
```

### 1.7 Endpoints — tableau complet

#### VisitesController `[Route("api/visites")]`

| Méthode | Route | Auth | Description |
|---------|-------|------|-------------|
| POST | `/api/visites` | ✅ JWT | Créer/MàJ une visite. `IdDelegue` injecté depuis JWT. |
| GET | `/api/visites/{id}` | ✅ JWT | Obtenir une visite par ID |
| GET | `/api/visites/by-delegue/{id}` | ✅ JWT | Visites d'un délégué |
| GET | `/api/visites/by-planning/{id}` | ✅ JWT | Visites d'un planning |
| GET | `/api/visites` | ✅ JWT | Toutes les visites (admin) |
| DELETE | `/api/visites/{id}` | ✅ JWT | Supprimer (bloqué si IsCompleted ou Rapport) |
| PUT | `/api/visites/{id}/planning/{planId}` | ✅ JWT | Affecter visite à un planning |
| PUT | `/api/visites/{id}/complete` | ✅ JWT | Compléter une visite (rapport requis) |

#### PlanningVisiteController `[Route("api/plannings")]`

| Méthode | Route | Auth | Description |
|---------|-------|------|-------------|
| POST | `/api/plannings` | ✅ JWT | Créer/MàJ un planning |
| GET | `/api/plannings/{id}` | ✅ JWT | Planning par ID |
| GET | `/api/plannings` | ✅ JWT | Tous les plannings |
| GET | `/api/plannings/by-delegue/{id}` | ✅ JWT | Plannings d'un délégué |
| GET | `/api/plannings/by-range` | ✅ JWT | Plannings par plage de dates |
| GET | `/api/plannings/by-date` | ✅ JWT | Plannings pour une date |
| DELETE | `/api/plannings/{id}` | ✅ JWT | Supprimer (bloqué si Confirme) |
| PUT | `/api/plannings/{id}/validate` | ✅ JWT | Valider : EnAttente → Confirme |

#### RapportsController `[Route("api/rapports")]`

| Méthode | Route | Auth | Description |
|---------|-------|------|-------------|
| POST | `/api/rapports/createUpdate` | ✅ JWT | Créer/MàJ un rapport |
| GET | `/api/rapports/{id}` | ✅ JWT | Rapport par ID |
| GET | `/api/rapports/by-visite/{id}` | ✅ JWT | Rapport d'une visite |
| GET | `/api/rapports/by-delegue/{id}` | ✅ JWT | Rapports d'un délégué |
| GET | `/api/rapports/all` | ✅ JWT | Tous les rapports |
| DELETE | `/api/rapports/{id}` | ✅ JWT | Supprimer un rapport |
| PUT | `/api/rapports/{id}/validate` | ✅ JWT | Valider (superviseur) |
| GET | `/api/rapports/can-create/{visiteId}` | ✅ JWT | Peut-on créer un rapport ? |
| GET | `/api/rapports/has-rapport/{visiteId}` | ✅ JWT | La visite a-t-elle un rapport ? |

#### ObjectifController `[Route("api/objectifs")]`

| Méthode | Route | Auth | Description |
|---------|-------|------|-------------|
| GET | `/api/objectifs` | ✅ JWT | Tous les objectifs |
| GET | `/api/objectifs/{id}` | ✅ JWT | Objectif par ID |
| GET | `/api/objectifs/by-delegue/{id}` | ✅ JWT | Objectifs d'un délégué |
| POST | `/api/objectifs` | ✅ JWT | Créer un objectif |
| PUT | `/api/objectifs/{id}/value` | ✅ JWT | MàJ valeur réalisée |
| DELETE | `/api/objectifs/{id}` | ✅ JWT | Supprimer |

#### RegionController `[Route("api/regions")]`

| Méthode | Route | Auth | Description |
|---------|-------|------|-------------|
| GET | `/api/regions/all` | ✅ JWT | Toutes les régions |
| POST | `/api/regions` | ✅ JWT | Créer une région |
| GET | `/api/regions/{id}` | ✅ JWT | Région par ID |
| GET | `/api/regions/by-delegue/{id}` | ✅ JWT | Régions d'un délégué |
| GET | `/api/regions/count/{id}` | ✅ JWT | Compter les régions d'un délégué |
| DELETE | `/api/regions/{id}` | ✅ JWT | Supprimer |

#### KPIController `[Route("api/kpi")]`

| Méthode | Route | Auth | Description |
|---------|-------|------|-------------|
| GET | `/api/kpi/visites-count` | ✅ JWT | Nombre de visites |
| GET | `/api/kpi/has-visite` | ✅ JWT | A-t-il des visites ? |
| GET | `/api/kpi/historique/{id}` | ✅ JWT | Historique KPI |
| GET | `/api/kpi/client-fidelite/{id}` | ✅ JWT | Fidélité client |
| GET | `/api/kpi/performance/{id}` | ✅ JWT | **Performance → retourne `List<PerformanceDto>`** |
| GET | `/api/kpi/performance-rate/{id}` | ✅ JWT | Taux de performance |
| GET | `/api/kpi/taux-conversion/{id}` | ✅ JWT | Taux de conversion |

### 1.8 Couche Service — logique métier

#### PlanningService
- `CreateOrUpdatePlanningAsync` : valide `HeureDebut < HeureFin`, vérifie les conflits horaires, bloque les mises à jour si Etat = Confirme
- `DeletePlanningAsync` : uniquement les plannings en `EnAttente` peuvent être supprimés
- `ValidatePlanningAsync` : `EnAttente → Confirme` (état terminal, irréversible)
- `GetPlanningsByRangeAsync` : requête par plage de dates

#### VisiteService
- `CreateOrUpdateVisiteAsync` : `IdDelegue` vient du paramètre (injecté depuis JWT dans le contrôleur)
- `AffectVisiteToPlanningAsync` : valide la correspondance délégué, la correspondance de date, le planning non-Confirme
- `CompleteVisiteAsync` : exige qu'un rapport existe (`HasRapport = true`)
- `DeleteVisiteAsync` : bloqué si `IsCompleted = true` OU si un Rapport existe

#### RapportService
- `CreateOrUpdateRapportAsync` : valide `Commentaire/Resultat` non vides, `IsCompleted = false`, propriété du délégué
- `ValidateRapportAsync` : définit `IdSuperviseurValidateur` + marque `visite.IsCompleted = true`
- `CanCreateRapportAsync` : vérifie visite non complétée, propriété, pas de rapport existant

#### KPIService — CalculatePerformanceAsync (recalcul dynamique)
- **Visites** : compte les visites complétées dans la période (`DateVisite` entre `DateDebut` et `DateFin`)
- **NouveauxClients** : compte les IDs médecin + pharmacien distincts
- **Fidelisation** : compte les clients groupés visités plus d'une fois
- **ChiffreAffaires** : repli sur `ValeurRealisee` stocké (CA non accessible depuis FieldAPI)
- Met à jour `ValeurRealisee` en DB si différent du comptage actuel

---

## PARTIE 2 — Angular (features/field)

### 2.1 Routes — field-routing.module.ts

| Route | Composant | Lazy |
|-------|-----------|------|
| `/field/visites` | VisiteListComponent | ✅ |
| `/field/visites/all` | VisiteAllComponent | ✅ |
| `/field/visites/new` | VisiteDetailComponent | ✅ |
| `/field/visites/:id/edit` | VisiteDetailComponent | ✅ |
| `/field/plannings` | PlanningListComponent | ✅ |
| `/field/plannings/new` | PlanningDetailComponent | ✅ |
| `/field/plannings/:id/edit` | PlanningDetailComponent | ✅ |
| `/field/rapports` | RapportListComponent | ✅ |
| `/field/rapports/new` | RapportDetailComponent | ✅ |
| `/field/rapports/:id/edit` | RapportDetailComponent | ✅ |
| `/field/objectifs` | ObjectifListComponent | ✅ |
| `/field/objectifs/new` | ObjectifDetailComponent | ✅ |
| `/field/objectifs/:id/edit` | ObjectifDetailComponent | ✅ |
| `/field/regions` | RegionListComponent | ✅ |
| `/field/regions/new` | RegionDetailComponent | ✅ |
| `/field/regions/:id/edit` | RegionDetailComponent | ✅ |
| `/field/kpi` | KpiComponent | ✅ |

### 2.2 Composants — état détaillé

#### Visites

| Composant | Fichier | État | Problèmes |
|-----------|---------|------|-----------|
| VisiteListComponent | visites/visite-list/ | ⚠️ | routerLink brisé : `/visites/edit,id` au lieu de `/visites,id,edit` |
| VisiteAllComponent | visites/visite-all/ | ✅ | Vue admin/superviseur, filtre date+délégué, OK |
| VisiteDetailComponent | visites/visite-detail/ | ⚠️ | Formulaire OK, mais interface `VisiteDto.date` vs backend `DateVisite` |

#### Plannings

| Composant | Fichier | État | Problèmes |
|-----------|---------|------|-----------|
| PlanningListComponent | plannings/planning-list/ | ⚠️ | routerLink brisé + affiche `etatPlanning` qui n'existe pas dans le DTO backend |
| PlanningDetailComponent | plannings/planning-detail/ | ⚠️ | Formulaire OK mais la soumission utilise `etatPlanning` |

#### Rapports

| Composant | Fichier | État | Problèmes |
|-----------|---------|------|-----------|
| RapportListComponent | rapports/rapport-list/ | ❌ | `estValide` toujours `undefined` → badge toujours "En attente" |
| RapportDetailComponent | rapports/rapport-detail/ | ✅ | Formulaire Commentaire+Résultat OK |

#### Objectifs

| Composant | Fichier | État | Problèmes |
|-----------|---------|------|-----------|
| ObjectifListComponent | objectifs/objectif-list/ | ⚠️ | routerLink brisé (même pattern) |
| ObjectifDetailComponent | objectifs/objectif-detail/ | ✅ | Formulaire OK |

#### Régions

| Composant | Fichier | État | Problèmes |
|-----------|---------|------|-----------|
| RegionListComponent | regions/region-list/ | ✅ | Affichage OK |
| RegionDetailComponent | regions/region-detail/ | ⚠️ | `codePostal` : string côté Angular, int côté backend |

#### KPI

| Composant | Fichier | État | Problèmes |
|-----------|---------|------|-----------|
| KpiComponent | kpi/ | ⚠️ | Appelle `performance/{id}` mais n'utilise pas `PerformanceDto` |

### 2.3 Services Angular — tableau complet

| Service | Base URL | Interface DTO | Problème |
|---------|----------|---------------|---------|
| VisiteService | `/fields/visites` | `VisiteDto` | Champ `date` au lieu de `dateVisite` (risque désérialisation) |
| PlanningService | `/fields/plannings` | `PlanningDto` | `etatPlanning` vs backend `Etat` — **bug critique** |
| RapportService | `/fields/rapports` | `RapportDto` | `estValide` jamais défini par backend ; `dateRapport` vs `Date` |
| ObjectifService | `/fields/objectifs` | `ObjectifDto` | OK |
| RegionService | `/fields/regions` | `RegionDto` | `codePostal: string` vs backend `int` |
| KpiService | `/fields/kpi` | (aucun DTO défini) | Données de performance non typées |

### 2.4 Interfaces DTO Angular vs Backend

#### VisiteDto Angular
```typescript
export interface VisiteDto {
  idVisite?: number;
  id_User_Delegue: number;
  date: string;                // backend : DateVisite
  type: VisiteType;
  isCompleted?: boolean;
  id_Medecin?: number | null;
  id_Pharmacien?: number | null;
  id_Planning?: number | null;
  id_Region?: number | null;
  hasRapport?: boolean;        // calculé par AutoMapper
}
```

#### PlanningDto Angular (BUG : etatPlanning vs Etat)
```typescript
export interface PlanningDto {
  idPlanning?: number;
  id_User_Delegue: number;
  date: string;
  heureDebut: string;
  heureFin: string;
  etatPlanning: EtatPlanning;  // ← BUG : le backend sérialise "Etat"
}
```

#### RapportDto Angular (BUG : estValide fantôme)
```typescript
export interface RapportDto {
  idRapport?: number;
  id_User_Delegue: number;
  id_Visite: number;
  commentaire: string;
  resultat: string;
  dateRapport?: string;
  estValide?: boolean;         // ← JAMAIS défini par le backend → toujours undefined
}
```

---

## PARTIE 3 — MAUI (Cynapharm-Mobile)

### 3.1 ViewModels — inventaire complet

| ViewModel | Fichier | État | Problèmes majeurs |
|-----------|---------|------|-------------------|
| VisitListViewModel | ViewModels/Visites/ | ⚠️ | Statut "ANNULEE" jamais retourné par backend |
| VisitDetailViewModel | ViewModels/Visites/ | ❌ | Payload complètement incorrect (Visite vs CreateVisiteDto) |
| PlanningViewModel | ViewModels/Planning/ | ⚠️ | `AddVisitAsync` ignore le paramètre `date` |
| RapportViewModel | ViewModels/Rapports/ | ✅ | GPS + file d'attente offline + chargement produits OK |
| KpiViewModel | ViewModels/Field/ | ❌ | Type `List<Kpi>` vs backend `List<PerformanceDto>` |
| ObjectifViewModel | ViewModels/Field/ | ⚠️ | Dépend du modèle Objectif buggé |
| RegionViewModel | ViewModels/Field/ | ⚠️ | Dépend du modèle Region incomplet |

### 3.2 Modèles MAUI — tableau de correspondance avec Backend

#### Models/Field/Visite.cs

| Champ MAUI | Type MAUI | Champ Backend | Type Backend | Correspondance |
|------------|-----------|---------------|--------------|----------------|
| Id | int | Id_Visite | int | ⚠️ Pas de [JsonPropertyName] |
| ClientNom | string | *(n'existe pas)* | — | ❌ Champ fantôme |
| DateVisite | DateTime | DateVisite | DateTime | ✅ |
| Notes | string | *(n'existe pas)* | — | ❌ Champ fantôme |
| Statut | string | IsCompleted | bool | ❌ Type incompatible |
| Type | VisiteType? | Type | VisiteType | ✅ |
| IdMedecin | int? | Id_Medecin | int? | ✅ |
| IdPharmacien | int? | Id_Pharmacien | int? | ✅ |
| IdPlanning | int? | Id_Planning | int? | ✅ |

#### Models/Field/Rapport.cs

| Champ MAUI | Type MAUI | Champ Backend | Type Backend | Correspondance |
|------------|-----------|---------------|--------------|----------------|
| Id | int | Id_Rapport | int | ❌ Pas de [JsonPropertyName("Id_Rapport")] |
| VisiteId | int | Id_Visite | int | ❌ Pas de [JsonPropertyName("Id_Visite")] |
| Contenu | string | Commentaire | string | ❌ Noms différents, pas de JsonPropertyName |
| ProduitsDiscutes | string? | *(n'existe pas)* | — | ❌ Champ fantôme |
| Resultat | string | Resultat | string | ✅ |
| DateSoumission | DateTime | Date (via DTO) | DateTime | ⚠️ Nom différent |
| Latitude | double? | Latitude | double? | ✅ |
| Longitude | double? | Longitude | double? | ✅ |

#### Models/Field/Planning.cs

| Champ MAUI | Type MAUI | Champ Backend | Type Backend | Correspondance |
|------------|-----------|---------------|--------------|----------------|
| Id | int | Id_Planning | int | ⚠️ Pas de JsonPropertyName |
| Date | DateTime | Date | DateTime | ✅ |
| HeureDebut | TimeSpan | HeureDebut | TimeSpan | ⚠️ Pas de JsonPropertyName |
| HeureFin | TimeSpan | HeureFin | TimeSpan | ⚠️ Pas de JsonPropertyName |
| Etat | EtatPlanning | Etat | EtatPlanning | ✅ |
| IdDelegue | int | Id_User_Delegue | int | ⚠️ Pas de JsonPropertyName |

#### Models/Field/Region.cs

| Champ MAUI | Type MAUI | Champ Backend | Type Backend | Correspondance |
|------------|-----------|---------------|--------------|----------------|
| Id | int | Id_Region | int | ❌ Pas de JsonPropertyName |
| Nom | string | NomRegion | string | ❌ Nom différent |
| *(manquant)* | — | CodePostal | int | ❌ Champ absent |
| *(manquant)* | — | Id_User_Delegue | int | ❌ Champ absent |

#### Models/Field/Objectif.cs (bug off-by-one critique)

| Champ MAUI | Type MAUI | Champ Backend | Type Backend | Correspondance |
|------------|-----------|---------------|--------------|----------------|
| Id | int | Id_Objectif | int | ⚠️ |
| TypeCode | int | Type (TypeObjectif) | int enum | ❌ Switch commence à 1, enum à 0 |
| PeriodeCode | int | Periode (PeriodeObjectif) | int enum | ❌ Switch commence à 1, enum à 0 |
| ValeurCible | int | ValeurCible | int | ✅ |
| ValeurRealisee | int | ValeurRealisee | int | ✅ |

#### Models/Field/Kpi.cs (incompatible avec PerformanceDto)

| Champ MAUI | Type MAUI | Champ PerformanceDto Backend | Type Backend |
|------------|-----------|------------------------------|--------------|
| Id | int | *(n'existe pas)* | — |
| DelegueId | int | *(n'existe pas)* | — |
| Periode | string | *(n'existe pas)* | — |
| Indicateur | string | Type (TypeObjectif) | enum |
| Valeur | decimal | ValeurRealisee | int |
| DateCalcul | DateTime | *(n'existe pas)* | — |
| *(manquant)* | — | ValeurCible | int |
| *(manquant)* | — | Pourcentage | double |

### 3.3 Services MAUI — tableau complet

| Service | Méthodes | Problèmes |
|---------|----------|-----------|
| VisiteService | GetVisitesAsync, GetVisiteAsync, CreateVisiteAsync, UpdateVisiteAsync, DeleteVisiteAsync, CreateRapportAsync | UpdateVisiteAsync : mauvais verbe HTTP (PUT non supporté) |
| PlanningService | GetPlanningsAsync, GetPlanningAsync, CreatePlanningAsync, UpdatePlanningEntryAsync | UpdatePlanningEntryAsync : PUT /plannings/{id} n'existe pas |
| RapportService | GetRapportsAsync, GetRapportAsync, SubmitRapportAsync | GetRapportsAsync : retourne objets vides (désérialisation échoue) |
| KpiService | GetKpisAsync | Type mismatch : `List<Kpi>` vs `List<PerformanceDto>` |
| ObjectifService | GetObjectifsAsync | Dépend modèle Objectif buggé |
| RegionService | GetRegionsAsync, CreateRegionAsync | Désérialisation partielle (Id, NomRegion échouent) |

### 3.4 Vues MAUI — état

| Vue | Fichier | État | Notes |
|-----|---------|------|-------|
| PlanningPage.xaml | Views/Planning/ | ❌ | Bouton "+ Ajouter une visite" sans Command |
| VisiteListPage.xaml | Views/Visites/ | ⚠️ | Statut "ANNULEE" affiché |
| VisiteDetailPage.xaml | Views/Visites/ | ❌ | Formulaire envoie mauvais payload |
| RapportPage.xaml | Views/Rapports/ | ✅ | Submit lié à SubmitCommand + CanSubmit |
| KpiPage.xaml | Views/Field/ | ❌ | Modèle KPI incompatible |

---

## PARTIE 4 — Analyse Globale

### 4.1 Tableau des bugs identifiés

> Note : StatusCode(515) = code erreur personnalisé utilisé dans TOUS les contrôleurs — **ce n'est pas un bug**.

| # | ID Bug | Couche | Composant | Description | Sévérité |
|---|--------|--------|-----------|-------------|----------|
| 1 | BUG-T01 | MAUI | Models/Field/Objectif.cs | Switch TypeCode/PeriodeCode commence à 1 ; les enums backend commencent à 0 → décalage total | 🔴 Critique |
| 2 | BUG-T02 | MAUI | ViewModels/Visites/VisitDetailViewModel.cs | `SaveAsync` envoie `Visite` (ClientNom, Notes, Statut) au lieu de `CreateVisiteDto` (Type, IdDelegue requis) | 🔴 Critique |
| 3 | BUG-T03 | MAUI | Models/Field/Rapport.cs | Aucun `[JsonPropertyName]` → Id, VisiteId, Contenu ne se désérialisent pas ; GetRapportsAsync retourne objets vides | 🔴 Critique |
| 4 | BUG-T04 | MAUI | Services/KpiService.cs | `GetKpisAsync` désérialise vers `List<Kpi>` ; backend retourne `List<PerformanceDto>` — structure totalement différente | 🔴 Critique |
| 5 | BUG-T05 | Angular | plannings/planning-list | `PlanningDto.etatPlanning` vs backend sérialise `Etat` → statut planning toujours 0 (EnAttente) | 🟠 Majeur |
| 6 | BUG-T06 | Angular | visites/visite-list | routerLink `['/field/visites/edit', v.idVisite]` → route `/field/visites/edit/123` inexistante | 🟠 Majeur |
| 7 | BUG-T07 | Angular | plannings/planning-list | routerLink `['/field/plannings/edit', p.idPlanning]` → route inexistante | 🟠 Majeur |
| 8 | BUG-T08 | Angular | objectifs/objectif-list | routerLink `['/field/objectifs/edit', o.idObjectif]` → route inexistante | 🟠 Majeur |
| 9 | BUG-T09 | Angular | rapports/rapport-list | `estValide` jamais défini par le backend → badge toujours "En attente" | 🟠 Majeur |
| 10 | BUG-T10 | MAUI | Services/PlanningService.cs | `UpdatePlanningEntryAsync` : PUT `/fields/plannings/{id}` n'existe pas (l'endpoint est POST) | 🟠 Majeur |
| 11 | BUG-T11 | MAUI | Views/Planning/PlanningPage.xaml | Bouton "+ Ajouter une visite" sans `Command` binding → non fonctionnel | 🟡 Moyen |
| 12 | BUG-T12 | MAUI | ViewModels/Planning/PlanningViewModel.cs | `AddVisitAsync(DateTime date)` ignore le paramètre `date` → la date n'est pas préremplie | 🟡 Moyen |
| 13 | BUG-T13 | MAUI | Models/Field/Region.cs | `Nom` vs `NomRegion`, pas de `CodePostal`/`Id_User_Delegue` → désérialisation partielle | 🟡 Moyen |
| 14 | BUG-T14 | MAUI | ViewModels/Visites/VisitListViewModel.cs | Statut "ANNULEE" dans les options de filtre ; FieldAPI n'utilise pas ce statut pour les visites | 🟢 Mineur |

### 4.2 Fonctionnalités manquantes

| Fonctionnalité | Couche | Détail |
|----------------|--------|--------|
| Validation rapport (superviseur) | Angular | Aucun composant ne déclenche `PUT /rapports/{id}/validate` |
| Validation planning | Angular | Aucun composant ne déclenche `PUT /plannings/{id}/validate` |
| Attribution visite à planning | Angular | `PUT /visites/{id}/planning/{planId}` non utilisé |
| Complétion visite | Angular | `PUT /visites/{id}/complete` non utilisé |
| Affichage KPI structuré | MAUI | `PerformanceDto` (Type/ValeurCible/ValeurRealisee/Pourcentage) non mappé |
| Gestion régions | MAUI | RegionService présent mais modèle incomplet |
| Géolocalisation rapport | Angular | Backend supporte Latitude/Longitude, Angular ne les envoie pas |

### 4.3 Plan de correction avec code

---

#### CORRECTION BUG-T01 — MAUI Objectif switch off-by-one

**Fichier :** `Cynapharm-Mobile/Models/Field/Objectif.cs`

```csharp
// AVANT (incorrect)
public string TypeObjectif => TypeCode switch {
    1 => "Visites",
    2 => "Chiffre d'affaires",
    3 => "Nouveaux clients",
    4 => "Fidélisation",
    _ => TypeCode > 0 ? $"Type {TypeCode}" : string.Empty
};

public string Periode => PeriodeCode switch {
    1 => "Mensuel",
    2 => "Trimestriel",
    3 => "Annuel",
    _ => PeriodeCode > 0 ? $"Période {PeriodeCode}" : string.Empty
};

// APRÈS (correct — aligné sur les enums backend)
public string TypeObjectif => TypeCode switch {
    0 => "Visites",
    1 => "Chiffre d'affaires",
    2 => "Nouveaux clients",
    3 => "Fidélisation",
    _ => $"Type {TypeCode}"
};

public string Periode => PeriodeCode switch {
    0 => "Mensuel",
    1 => "Trimestriel",
    2 => "Annuel",
    _ => $"Période {PeriodeCode}"
};
```

---

#### CORRECTION BUG-T02 — MAUI VisitDetailViewModel payload incorrect

**Fichier :** `Cynapharm-Mobile/ViewModels/Visites/VisitDetailViewModel.cs`

```csharp
// AVANT (incorrect)
var visite = new Visite {
    ClientNom = ClientName,
    DateVisite = VisiteDate,
    Notes = Notes,
    Statut = Statut
};
if (IsNew)
    await _visiteService.CreateVisiteAsync(visite);

// APRÈS (correct — utilise CreateVisiteDto avec les champs obligatoires)
var dto = new CreateVisiteDto {
    DateVisite = VisiteDate,
    Type = SelectedType,           // VisiteType enum : Medecin=1, Pharmacien=2, Autre=3
    IdMedecin = SelectedMedecinId,
    IdPharmacien = SelectedPharmacienId,
    IdPlanning = SelectedPlanningId
    // IdDelegue est injecté côté backend depuis le JWT
};
if (IsNew)
    await _visiteService.CreateVisiteAsync(dto);
else {
    dto.IdVisite = CurrentVisiteId;
    await _visiteService.UpdateVisiteAsync(CurrentVisiteId, dto);
}
```

---

#### CORRECTION BUG-T03 — MAUI Rapport.cs désérialisation

**Fichier :** `Cynapharm-Mobile/Models/Field/Rapport.cs`

```csharp
// AVANT (incorrect — aucun JsonPropertyName)
public class Rapport {
    public int Id { get; set; }
    public int VisiteId { get; set; }
    public string Contenu { get; set; } = string.Empty;
    public string? ProduitsDiscutes { get; set; }
    public string Resultat { get; set; } = string.Empty;
    public DateTime DateSoumission { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

// APRÈS (correct — aligné sur RapportVisiteDto)
using System.Text.Json.Serialization;

public class Rapport {
    [JsonPropertyName("id_Rapport")]
    public int Id { get; set; }

    [JsonPropertyName("id_Visite")]
    public int VisiteId { get; set; }

    [JsonPropertyName("commentaire")]
    public string Contenu { get; set; } = string.Empty;

    [JsonPropertyName("resultat")]
    public string Resultat { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public DateTime DateSoumission { get; set; }

    [JsonPropertyName("id_User_Delegue")]
    public int IdDelegue { get; set; }

    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }
}
```

---

#### CORRECTION BUG-T04 — MAUI KpiService type mismatch

**Fichier :** `Cynapharm-Mobile/Services/KpiService.cs`

Créer d'abord un modèle `PerformanceDto` aligné sur le backend :

```csharp
// Nouveau fichier : Cynapharm-Mobile/Models/Field/PerformanceDto.cs
using System.Text.Json.Serialization;

public class PerformanceDto {
    [JsonPropertyName("type")]
    public int Type { get; set; }           // TypeObjectif enum : 0=Visites, 1=CA, 2=NvxClients, 3=Fidelisation

    [JsonPropertyName("valeurCible")]
    public int ValeurCible { get; set; }

    [JsonPropertyName("valeurRealisee")]
    public int ValeurRealisee { get; set; }

    [JsonPropertyName("pourcentage")]
    public double Pourcentage { get; set; }

    // Propriété calculée pour l'affichage
    public string TypeLabel => Type switch {
        0 => "Visites",
        1 => "Chiffre d'affaires",
        2 => "Nouveaux clients",
        3 => "Fidélisation",
        _ => $"Type {Type}"
    };
}
```

```csharp
// AVANT
public async Task<List<Kpi>?> GetKpisAsync(DateTime? debut = null, DateTime? fin = null)
    => await _api.GetAsync<List<Kpi>>($"fields/kpi/performance/{userId}?...");

// APRÈS
public async Task<List<PerformanceDto>?> GetPerformanceAsync(int delegueId,
    DateTime? debut = null, DateTime? fin = null) {
    var query = $"fields/kpi/performance/{delegueId}";
    if (debut.HasValue) query += $"?debut={debut.Value:yyyy-MM-dd}";
    if (fin.HasValue) query += (debut.HasValue ? "&" : "?") + $"fin={fin.Value:yyyy-MM-dd}";
    return await _api.GetAsync<List<PerformanceDto>>(query);
}
```

---

#### CORRECTION BUG-T05 — Angular PlanningDto.etatPlanning vs backend Etat

**Fichier :** `Cynapharm/src/app/features/field/plannings/services/planning.service.ts`

```typescript
// AVANT
export interface PlanningDto {
  idPlanning?: number;
  id_User_Delegue: number;
  date: string;
  heureDebut: string;
  heureFin: string;
  etatPlanning: EtatPlanning;  // ← incorrect
}

// APRÈS
export interface PlanningDto {
  idPlanning?: number;
  id_User_Delegue: number;
  date: string;
  heureDebut: string;
  heureFin: string;
  etat: EtatPlanning;          // ← correspond au champ backend "Etat"
}
```

Mettre à jour toutes les références dans les composants :

```typescript
// planning-list.component.ts
getPlanningEtatLabel(planning: PlanningDto): string {
  switch (planning.etat) {   // ← était planning.etatPlanning
    case EtatPlanning.EnAttente: return 'En attente';
    case EtatPlanning.Confirme: return 'Confirmé';
    case EtatPlanning.Annule: return 'Annulé';
    default: return 'Inconnu';
  }
}
```

---

#### CORRECTIONS BUG-T06 / BUG-T07 / BUG-T08 — routerLinks brisés

**Fichier :** `Cynapharm/src/app/features/field/visites/visite-list/visite-list.component.html`

```html
<!-- AVANT -->
<a [routerLink]="['/field/visites/edit', v.idVisite]" class="link-btn">Modifier</a>

<!-- APRÈS -->
<a [routerLink]="['/field/visites', v.idVisite, 'edit']" class="link-btn">Modifier</a>
```

**Fichier :** `Cynapharm/src/app/features/field/plannings/planning-list/planning-list.component.html`

```html
<!-- AVANT -->
<a [routerLink]="['/field/plannings/edit', p.idPlanning]" class="link-btn">Modifier</a>

<!-- APRÈS -->
<a [routerLink]="['/field/plannings', p.idPlanning, 'edit']" class="link-btn">Modifier</a>
```

**Fichier :** `Cynapharm/src/app/features/field/objectifs/objectif-list/objectif-list.component.html`

```html
<!-- AVANT -->
<a [routerLink]="['/field/objectifs/edit', o.idObjectif]" class="link-btn">Modifier</a>

<!-- APRÈS -->
<a [routerLink]="['/field/objectifs', o.idObjectif, 'edit']" class="link-btn">Modifier</a>
```

---

#### CORRECTION BUG-T09 — Angular rapport-list estValide fantôme

**Fichier :** `Cynapharm/src/app/features/field/rapports/services/rapport.service.ts`

Le backend ne retourne pas `estValide`. La validation se fait via `PUT /rapports/{id}/validate`.
Solution : supprimer `estValide` du DTO et indiquer "rapport soumis" (non "en attente").

```typescript
// APRÈS — supprimer estValide
export interface RapportDto {
  idRapport?: number;
  id_User_Delegue: number;
  id_Visite: number;
  commentaire: string;
  resultat: string;
  date?: string;   // ← renommé depuis dateRapport pour matcher le backend
}
```

**Fichier :** `rapport-list.component.html`

```html
<!-- AVANT -->
<span [class.valid]="r.estValide === true" [class.pending]="r.estValide == null">
  {{ r.estValide === true ? 'Validé' : 'En attente' }}
</span>

<!-- APRÈS — tout rapport retourné par l'API est soumis -->
<span class="valid-badge pending">Soumis</span>
```

---

#### CORRECTION BUG-T10 — MAUI PlanningService mauvais verbe HTTP

**Fichier :** `Cynapharm-Mobile/Services/PlanningService.cs`

```csharp
// AVANT (PUT non supporté)
public Task<Planning?> UpdatePlanningEntryAsync(int id, Planning entry)
    => _api.PutAsync<Planning>($"fields/plannings/{id}", entry);

// APRÈS (POST /plannings gère la création ET la mise à jour via Id_Planning)
public Task<Planning?> UpdatePlanningEntryAsync(int id, Planning entry)
    => _api.PostAsync<Planning>("fields/plannings", entry);
```

---

#### CORRECTION BUG-T11 — MAUI PlanningPage bouton sans Command

**Fichier :** `Cynapharm-Mobile/Views/Planning/PlanningPage.xaml`

```xml
<!-- AVANT -->
<Button Text="+ Ajouter une visite"
        Style="{StaticResource PrimaryButtonStyle}"
        Margin="16,12,16,28" />

<!-- APRÈS -->
<Button Text="+ Ajouter une visite"
        Command="{Binding AddVisitCommand}"
        Style="{StaticResource PrimaryButtonStyle}"
        Margin="16,12,16,28" />
```

**Fichier :** `Cynapharm-Mobile/ViewModels/Planning/PlanningViewModel.cs`

```csharp
// S'assurer que la commande existe et passe la date sélectionnée
[RelayCommand]
private async Task AddVisitAsync() {
    var selectedDate = SelectedDate ?? DateTime.Today;
    await Shell.Current.GoToAsync($"//visits/detail?date={selectedDate:yyyy-MM-dd}");
}
```

---

#### CORRECTION BUG-T12 — MAUI AddVisitAsync ignore la date

**Fichier :** `Cynapharm-Mobile/ViewModels/Planning/PlanningViewModel.cs`

```csharp
// AVANT
[RelayCommand]
private async Task AddVisitAsync(DateTime date)
    => await Shell.Current.GoToAsync("//visits/detail");

// APRÈS — passer la date comme QueryProperty
[RelayCommand]
private async Task AddVisitAsync(DateTime date) {
    var dateStr = date != default ? date.ToString("yyyy-MM-dd") : DateTime.Today.ToString("yyyy-MM-dd");
    await Shell.Current.GoToAsync($"//visits/detail?prefillDate={dateStr}");
}
```

Et dans `VisitDetailViewModel` :

```csharp
[QueryProperty(nameof(PrefillDate), "prefillDate")]
public partial class VisitDetailViewModel : BaseViewModel {
    [ObservableProperty]
    private string _prefillDate = string.Empty;

    partial void OnPrefillDateChanged(string value) {
        if (DateTime.TryParse(value, out var dt))
            VisiteDate = dt;
    }
}
```

---

#### CORRECTION BUG-T13 — MAUI Region.cs incomplet

**Fichier :** `Cynapharm-Mobile/Models/Field/Region.cs`

```csharp
// AVANT
public class Region {
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
}

// APRÈS
using System.Text.Json.Serialization;

public class Region {
    [JsonPropertyName("id_Region")]
    public int Id { get; set; }

    [JsonPropertyName("nomRegion")]
    public string Nom { get; set; } = string.Empty;

    [JsonPropertyName("codePostal")]
    public int CodePostal { get; set; }

    [JsonPropertyName("id_User_Delegue")]
    public int IdDelegue { get; set; }
}
```

---

#### CORRECTION BUG-T14 — MAUI VisiteListViewModel statut ANNULEE

**Fichier :** `Cynapharm-Mobile/ViewModels/Visites/VisitListViewModel.cs`

```csharp
// AVANT
public List<string> StatusOptions { get; } = new() { "Tous", "PLANIFIEE", "REALISEE", "ANNULEE" };

// APRÈS — FieldAPI utilise IsCompleted (bool) pas un statut string
public List<string> StatusOptions { get; } = new() { "Tous", "Non complétée", "Complétée" };

// Et dans le filtre :
private IEnumerable<Visite> ApplyFilter(IEnumerable<Visite> visites) =>
    SelectedStatus switch {
        "Complétée"     => visites.Where(v => v.IsCompleted),
        "Non complétée" => visites.Where(v => !v.IsCompleted),
        _               => visites
    };
```

### 4.4 Scénarios de flux de données complets

---

#### SCÉNARIO A — Délégué crée une visite depuis MAUI et soumet un rapport (flux nominal)

**Préconditions :** Délégué authentifié, JWT valide, connexion réseau disponible.

```
[1] Délégué ouvre PlanningPage
    └─ PlanningViewModel.LoadAsync()
       └─ PlanningService.GetPlanningsAsync(delegueId)
          └─ GET fields/plannings/by-delegue/{id}
             └─ Backend: PlanningVisiteController.GetByDelegue()
                └─ PlanningService.GetPlanningsByDelegueAsync(id)
                   └─ DB: SELECT * FROM Planning_Visite WHERE Id_User_Delegue = {id}
                   → retourne List<PlanningVisiteDto> (avec champ "Etat", pas "etatPlanning")
             ← 200 OK [ { id_Planning, date, heureDebut, heureFin, Etat, id_User_Delegue } ]
          ← Planning[] désérialisés (Etat mappé si JsonPropertyName correct)
       → Affiche calendrier 7 jours groupé par PlanningDayGroup

[2] Délégué tape "Ajouter une visite" sur un jour J
    └─ PlanningViewModel.AddVisitCommand.Execute(dateJ)
       └─ Shell.GoToAsync("//visits/detail?prefillDate=2026-05-26")
          └─ VisitDetailViewModel.OnPrefillDateChanged("2026-05-26")
             └─ VisiteDate = 2026-05-26 (prérempli)

[3] Délégué remplit le formulaire : Type=Medecin, IdMedecin=42, puis appuie Enregistrer
    └─ VisitDetailViewModel.SaveAsync()
       └─ CreateVisiteDto {
              DateVisite = 2026-05-26,
              Type = VisiteType.Medecin (=1),
              IdMedecin = 42,
              IdPlanning = null  (visite hors planning)
          }
       └─ VisiteService.CreateVisiteAsync(dto)
          └─ POST fields/visites
             └─ Backend: VisitesController.CreateOrUpdate([FromBody] CreateVisiteDto dto)
                └─ dto.IdDelegue = JWT.NameIdentifier (ex: 7) — injecté côté serveur
                └─ VisiteService.CreateOrUpdateVisiteAsync(dto, delegueId=7)
                   └─ DB: INSERT INTO Visite (DateVisite, Type, IsCompleted=false, Id_User_Delegue=7, Id_Medecin=42)
                   → retourne VisiteDto { idVisite=101, hasRapport=false, ... }
             ← 200 OK { Result: VisiteDto, IsSuccess: true }
          ← Visite { Id=101, ... }
       → Navigation vers VisiteListPage, nouvelle visite Id=101 visible

[4] Délégué ouvre Visite Id=101, appuie "Créer rapport"
    └─ Navigation vers RapportPage (VisiteId=101)
       └─ RapportViewModel.Initialize(visiteId=101)
          └─ Vérifie GPS : Latitude=36.7, Longitude=3.0 (si permission accordée)

[5] Délégué remplit Commentaire + Résultat, appuie "Soumettre le rapport"
    └─ RapportViewModel.SubmitCommand.Execute()
       └─ Vérifie CanSubmit : Commentaire non vide, Résultat non vide → true
       └─ Si réseau disponible :
          └─ VisiteService.CreateRapportAsync(rapport, userId=7)
             └─ payload = {
                    Id_Rapport = 0,
                    Id_Visite = 101,
                    Commentaire = "RDV productif",
                    Resultat = "Commande passée",
                    Id_User_Delegue = 7,
                    Latitude = 36.7,
                    Longitude = 3.0
                }
             └─ POST fields/rapports/createUpdate
                └─ Backend: RapportsController.CreateOrUpdate([FromBody] CreateRapportDto dto)
                   └─ RapportService.CreateOrUpdateRapportAsync(dto)
                      └─ Valide : Commentaire non vide ✅, Resultat non vide ✅
                      └─ Vérifie visite.IsCompleted == false ✅
                      └─ Vérifie visite.Id_User_Delegue == 7 ✅
                      └─ DB: INSERT INTO Rapport_Visite (Commentaire, Resultat, DateRapport=now, Id_Visite=101, Id_User_Delegue=7, Latitude=36.7, Longitude=3.0)
                      → Id_Rapport = 55
                ← 200 OK { Result: RapportVisiteDto { id_Rapport=55, date=now, ... }, IsSuccess: true }
          ← Rapport créé avec succès
          → Toast "Rapport soumis avec succès"
       └─ Si réseau indisponible :
          └─ LocalDatabaseService.SavePendingRapportAsync(pendingEntry)
             └─ SQLite INSERT INTO PendingRapportEntry
          → Toast "Rapport sauvegardé hors-ligne, sera synchronisé à la reconnexion"
```

---

#### SCÉNARIO B — Superviseur consulte et valide les KPIs d'un délégué depuis Angular

**Préconditions :** Superviseur authentifié (rôle SUPERVISEUR ou ADMIN), JWT valide.

```
[1] Superviseur navigue vers /field/kpi
    └─ KpiComponent.ngOnInit()
       └─ KpiService.getPerformance(delegueId)
          └─ GET /fields/kpi/performance/{delegueId}
             └─ Backend: KPIController.GetPerformance(id)
                └─ KPIService.CalculatePerformanceAsync(delegueId)
                   └─ Récupère objectifs du délégué
                   └─ Pour chaque objectif :
                      └─ TypeObjectif.Visites :
                         └─ COUNT(Visite WHERE Id_User_Delegue=id AND DateVisite BETWEEN dateDebut AND dateFin AND IsCompleted=true)
                         → ex: 23 visites
                      └─ TypeObjectif.NouveauxClients :
                         └─ COUNT(DISTINCT Id_Medecin ∪ Id_Pharmacien WHERE IsCompleted=true)
                         → ex: 15 clients distincts
                      └─ TypeObjectif.Fidelisation :
                         └─ COUNT(clients groupés visités > 1 fois)
                         → ex: 8 clients fidélisés
                      └─ TypeObjectif.ChiffreAffaires :
                         └─ Retourne ValeurRealisee stocké (CA non recalculable ici)
                   └─ Met à jour ValeurRealisee si différent
                   → retourne List<PerformanceDto> [
                       { Type: Visites(0), ValeurCible: 30, ValeurRealisee: 23, Pourcentage: 76.6 },
                       { Type: NouveauxClients(2), ValeurCible: 20, ValeurRealisee: 15, Pourcentage: 75.0 },
                       { Type: Fidelisation(3), ValeurCible: 10, ValeurRealisee: 8, Pourcentage: 80.0 }
                     ]
             ← 200 OK { Result: [...], IsSuccess: true }
          ← List<PerformanceDto> désérialisée
       → Affiche tableau KPI avec barres de progression

[2] Superviseur consulte la liste des rapports /field/rapports
    └─ RapportListComponent.ngOnInit()
       └─ RapportService.getRapportsByDelegue(delegueId)
          └─ GET /fields/rapports/by-delegue/{delegueId}
             ← [ { id_Rapport: 55, commentaire: "RDV productif", resultat: "Commande passée",
                   date: "2026-05-26", id_Visite: 101, id_User_Delegue: 7 } ]
       → Affiche rapport Id=55

[3] Superviseur valide le rapport Id=55
    └─ RapportService.validateRapport(55)
       └─ PUT /fields/rapports/55/validate
          └─ Backend: RapportsController.Validate(55)
             └─ RapportService.ValidateRapportAsync(55, superviseurId)
                └─ rapport.Id_SuperviseurValidateur = superviseurId
                └─ visite.IsCompleted = true
                └─ DB: UPDATE Rapport_Visite SET Id_SuperviseurValidateur=superviseurId WHERE Id_Rapport=55
                └─ DB: UPDATE Visite SET IsCompleted=true WHERE Id_Visite=101
             ← 200 OK { IsSuccess: true }
          ← Validation confirmée
       → Rapport Id=55 marqué validé, Visite Id=101 IsCompleted=true

[4] KPI recalculé automatiquement
    └─ Prochain appel à /kpi/performance/{delegueId}
       └─ COUNT(Visite IsCompleted=true) = 24 (était 23)
       └─ ValeurRealisee mis à jour : 23 → 24
       → Pourcentage : 76.6% → 80.0%
```

---

#### SCÉNARIO C — Délégué crée un planning depuis Angular, l'affecte et le valide

**Préconditions :** Délégué authentifié Angular, rôle DELEGUE.

```
[1] Délégué navigue vers /field/plannings/new
    └─ PlanningDetailComponent chargé (mode création)

[2] Délégué remplit : Date=2026-06-01, HeureDebut=09:00, HeureFin=12:00, appuie Enregistrer
    └─ PlanningService.createPlanning(planningDto)
       └─ POST /fields/plannings
          └─ Body: {
                id_User_Delegue: 7,
                date: "2026-06-01",
                heureDebut: "09:00:00",
                heureFin: "12:00:00",
                etat: 0   // ← EnAttente (si BUG-T05 corrigé)
             }
          └─ Backend: PlanningVisiteController.CreateOrUpdate()
             └─ PlanningService.CreateOrUpdatePlanningAsync(dto)
                └─ Valide HeureDebut(9h) < HeureFin(12h) ✅
                └─ Vérifie conflits horaires le 2026-06-01 pour délégué 7 → aucun ✅
                └─ DB: INSERT INTO Planning_Visite (Date=2026-06-01, HeureDebut=09:00, HeureFin=12:00, Etat=0, Id_User_Delegue=7)
                → Id_Planning = 12
             ← 200 OK { Result: { id_Planning: 12, Etat: 0, ... }, IsSuccess: true }
          ← Planning Id=12 créé

[3] Délégué navigue vers /field/plannings, voit le planning Id=12 avec statut "En attente"
    └─ PlanningListComponent charge plannings
       └─ planning.etat === EtatPlanning.EnAttente (0) → affiche "En attente" ✅ (si BUG-T05 corrigé)

[4] Délégué clique "Modifier" sur planning Id=12
    └─ routerLink ['/field/plannings', 12, 'edit'] → /field/plannings/12/edit ✅ (si BUG-T07 corrigé)
    └─ PlanningDetailComponent chargé en mode édition
    └─ Modifie HeureFin=13:00, enregistre
       └─ POST /fields/plannings (même endpoint, Id_Planning=12 → mise à jour)
          └─ PlanningService : Etat != Confirme (=0) → mise à jour autorisée ✅
          ← 200 OK, planning mis à jour

[5] Superviseur valide le planning Id=12
    └─ PUT /fields/plannings/12/validate
       └─ PlanningService.ValidatePlanningAsync(12)
          └─ Etat : EnAttente → Confirme (état terminal)
          └─ DB: UPDATE Planning_Visite SET Etat=1 WHERE Id_Planning=12
       ← 200 OK

[6] Délégué tente de modifier le planning validé
    └─ POST /fields/plannings (Id_Planning=12, Etat=Confirme)
       └─ PlanningService : Etat == Confirme → 400 Bad Request "Planning confirmé ne peut être modifié"

[7] Délégué crée une visite et l'affecte au planning Id=12
    └─ POST /fields/visites → Visite Id=103 créée (DateVisite=2026-06-01)
    └─ PUT /fields/visites/103/planning/12
       └─ VisiteService.AffectVisiteToPlanningAsync(103, 12)
          └─ Valide délégué match : visite.Id_User_Delegue == planning.Id_User_Delegue ✅
          └─ Valide date match : visite.DateVisite == planning.Date ✅
          └─ Planning Etat != Confirme ? Non (Etat=Confirme=1) → ⚠️
             → Selon la règle métier : AffectVisiteToPlanningAsync bloque si planning Confirme
             → 400 Bad Request "Impossible d'affecter une visite à un planning confirmé"
```

> Note scénario C étape 7 : La règle `AffectVisiteToPlanningAsync` vérifie que le planning n'est pas Confirme avant d'affecter. Cela signifie qu'une visite doit être affectée au planning AVANT sa validation. Ordre correct : créer visite → affecter → valider planning.

---

## PARTIE 5 — Code Source Complet

> La partie 5 contient le code source verbatim de tous les fichiers lus. Voir fichiers séparés ou lire via les agents Explore pour les fichiers individuels.
>
> Résumé des fichiers couverts par cette analyse (55+ fichiers) :

### Backend FieldAPI

- `CynapCRM.Services.FieldAPI/Program.cs`
- `CynapCRM.Services.FieldAPI/Data/AppDbContext.cs`
- `CynapCRM.Services.FieldAPI/MappingConfig.cs`
- `CynapCRM.Services.FieldAPI/Models/Enums/EtatPlanning.cs`
- `CynapCRM.Services.FieldAPI/Models/Enums/VisiteType.cs`
- `CynapCRM.Services.FieldAPI/Models/Enums/TypeObjectif.cs`
- `CynapCRM.Services.FieldAPI/Models/Enums/PeriodeObjectif.cs`
- `CynapCRM.Services.FieldAPI/Models/Region.cs`
- `CynapCRM.Services.FieldAPI/Models/Planning_Visite.cs`
- `CynapCRM.Services.FieldAPI/Models/Visite.cs`
- `CynapCRM.Services.FieldAPI/Models/Rapport_Visite.cs`
- `CynapCRM.Services.FieldAPI/Models/Objectif_Delegue.cs`
- `CynapCRM.Services.FieldAPI/Models/Dto/PlanningVisiteDto.cs`
- `CynapCRM.Services.FieldAPI/Models/Dto/RapportVisiteDto.cs`
- `CynapCRM.Services.FieldAPI/Models/Dto/CreateVisiteDto.cs`
- `CynapCRM.Services.FieldAPI/Models/Dto/VisiteDto.cs`
- `CynapCRM.Services.FieldAPI/Models/Dto/PerformanceDto.cs`
- `CynapCRM.Services.FieldAPI/Controllers/VisitesController.cs`
- `CynapCRM.Services.FieldAPI/Controllers/PlanningVisiteController.cs`
- `CynapCRM.Services.FieldAPI/Controllers/RapportsController.cs`
- `CynapCRM.Services.FieldAPI/Controllers/ObjectifController.cs`
- `CynapCRM.Services.FieldAPI/Controllers/RegionController.cs`
- `CynapCRM.Services.FieldAPI/Controllers/KPIController.cs`
- `CynapCRM.Services.FieldAPI/Service/PlanningService.cs`
- `CynapCRM.Services.FieldAPI/Service/VisiteService.cs`
- `CynapCRM.Services.FieldAPI/Service/RapportService.cs`
- `CynapCRM.Services.FieldAPI/Service/KPIService.cs`
- `CynapCRM.Services.FieldAPI/Service/ObjectifService.cs`
- `CynapCRM.Services.FieldAPI/Service/RegionService.cs`

### Angular features/field

- `Cynapharm/src/app/features/field/field-routing.module.ts`
- `Cynapharm/src/app/features/field/visites/services/visite.service.ts`
- `Cynapharm/src/app/features/field/visites/visite-list/visite-list.component.ts`
- `Cynapharm/src/app/features/field/visites/visite-list/visite-list.component.html`
- `Cynapharm/src/app/features/field/visites/visite-all/visite-all.component.ts`
- `Cynapharm/src/app/features/field/visites/visite-detail/visite-detail.component.ts`
- `Cynapharm/src/app/features/field/plannings/services/planning.service.ts`
- `Cynapharm/src/app/features/field/plannings/planning-list/planning-list.component.ts`
- `Cynapharm/src/app/features/field/plannings/planning-list/planning-list.component.html`
- `Cynapharm/src/app/features/field/plannings/planning-detail/planning-detail.component.ts`
- `Cynapharm/src/app/features/field/rapports/services/rapport.service.ts`
- `Cynapharm/src/app/features/field/rapports/rapport-list/rapport-list.component.ts`
- `Cynapharm/src/app/features/field/rapports/rapport-list/rapport-list.component.html`
- `Cynapharm/src/app/features/field/rapports/rapport-detail/rapport-detail.component.ts`
- `Cynapharm/src/app/features/field/objectifs/services/objectif.service.ts`
- `Cynapharm/src/app/features/field/objectifs/objectif-list/objectif-list.component.ts`
- `Cynapharm/src/app/features/field/objectifs/objectif-list/objectif-list.component.html`
- `Cynapharm/src/app/features/field/objectifs/objectif-detail/objectif-detail.component.ts`
- `Cynapharm/src/app/features/field/regions/services/region.service.ts`
- `Cynapharm/src/app/features/field/regions/region-list/region-list.component.ts`
- `Cynapharm/src/app/features/field/regions/region-detail/region-detail.component.ts`
- `Cynapharm/src/app/features/field/kpi/kpi.component.ts`

### MAUI Cynapharm-Mobile

- `Cynapharm-Mobile/Models/Field/Visite.cs`
- `Cynapharm-Mobile/Models/Field/Rapport.cs`
- `Cynapharm-Mobile/Models/Field/Planning.cs`
- `Cynapharm-Mobile/Models/Field/Region.cs`
- `Cynapharm-Mobile/Models/Field/Objectif.cs`
- `Cynapharm-Mobile/Models/Field/Kpi.cs`
- `Cynapharm-Mobile/Services/VisiteService.cs`
- `Cynapharm-Mobile/Services/PlanningService.cs`
- `Cynapharm-Mobile/Services/RapportService.cs`
- `Cynapharm-Mobile/Services/KpiService.cs`
- `Cynapharm-Mobile/ViewModels/Visites/VisitListViewModel.cs`
- `Cynapharm-Mobile/ViewModels/Visites/VisitDetailViewModel.cs`
- `Cynapharm-Mobile/ViewModels/Planning/PlanningViewModel.cs`
- `Cynapharm-Mobile/ViewModels/Rapports/RapportViewModel.cs`
- `Cynapharm-Mobile/Views/Planning/PlanningPage.xaml`
- `Cynapharm-Mobile/Views/Rapports/RapportPage.xaml`

---

*Document généré le 2026-05-26 | CynapSoft CRM — Analyse Terrain Complète*
*Contrainte appliquée : StatusCode(515) non signalé comme bug — code d'erreur personnalisé global.*
