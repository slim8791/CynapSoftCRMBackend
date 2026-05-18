# Cynapharm Mobile — Documentation Technique Complète

## Table des matières

1. [Vue d'ensemble du projet](#1-vue-densemble-du-projet)
2. [Architecture générale](#2-architecture-générale)
3. [Configuration & démarrage](#3-configuration--démarrage)
4. [Authentification & sécurité](#4-authentification--sécurité)
5. [Couche réseau & API](#5-couche-réseau--api)
6. [Base de données locale & mode hors-ligne](#6-base-de-données-locale--mode-hors-ligne)
7. [Modules fonctionnels](#7-modules-fonctionnels)
   - 7.1 [Login & Profil](#71-login--profil)
   - 7.2 [Dashboard](#72-dashboard)
   - 7.3 [Visites & Rapports](#73-visites--rapports)
   - 7.4 [Planning](#74-planning)
   - 7.5 [Catalogue Produits](#75-catalogue-produits)
   - 7.6 [Commandes](#76-commandes)
   - 7.7 [Documents](#77-documents)
   - 7.8 [Stock & Distribution](#78-stock--distribution)
   - 7.9 [Objectifs & KPIs](#79-objectifs--kpis)
8. [Modèles de données](#8-modèles-de-données)
9. [Services & injection de dépendances](#9-services--injection-de-dépendances)
10. [Navigation & structure des pages](#10-navigation--structure-des-pages)
11. [Gestion des rôles & permissions](#11-gestion-des-rôles--permissions)
12. [Gestion des erreurs & résilience](#12-gestion-des-erreurs--résilience)
13. [UI, styles & ressources](#13-ui-styles--ressources)
14. [Structure des fichiers](#14-structure-des-fichiers)

---

## 1. Vue d'ensemble du projet

**Cynapharm Mobile** est une application CRM mobile destinée aux équipes commerciales pharmaceutiques. Elle est développée avec **.NET MAUI** (Multi-platform App UI) et cible Android, iOS, macOS Catalyst et Windows depuis une base de code unique.

| Attribut | Valeur |
|---|---|
| Framework | .NET MAUI |
| Langage | C# + XAML |
| Version | 1.0.0 |
| Plateformes | Android, iOS, macOS Catalyst, Windows |
| Marque | CynaSoft — Cynapharm CRM |
| Couleur primaire | `#1A6B3C` (vert foncé) |
| Couleur secondaire | `#F5A623` (orange) |
| Couleur accent | `#00B4D8` (cyan) |

L'application s'adresse à plusieurs types d'utilisateurs dans la chaîne de distribution pharmaceutique : délégués commerciaux, superviseurs, pharmaciens, grossistes, clients et administrateurs. Chaque rôle bénéficie d'une interface adaptée exposant uniquement les fonctionnalités pertinentes.

---

## 2. Architecture générale

L'application suit le patron **MVVM (Model-View-ViewModel)** en s'appuyant sur la bibliothèque **CommunityToolkit.Mvvm** pour la liaison de données, les commandes réactives et la validation des formulaires.

```
┌─────────────────────────────────────────┐
│              Views (XAML)               │
│  Liaison de données bidirectionnelle    │
└──────────────┬──────────────────────────┘
               │ Data Binding
┌──────────────▼──────────────────────────┐
│           ViewModels                    │
│  Héritent de BaseViewModel              │
│  Validation via ObservableValidator     │
└──────────────┬──────────────────────────┘
               │ Appels de service
┌──────────────▼──────────────────────────┐
│             Services                    │
│  AuthService, ProductService, etc.      │
└──────────────┬──────────────────────────┘
               │
       ┌───────┴────────┐
       │                │
┌──────▼──────┐  ┌──────▼──────────────┐
│  ApiService │  │ LocalDatabaseService │
│  (HTTP/JWT) │  │  (SQLite hors-ligne) │
└─────────────┘  └──────────────────────┘
```

### Points architecturaux clés

- **BaseViewModel** : classe parente commune à tous les ViewModels. Elle centralise `IsBusy`, `IsRefreshing`, `ErrorMessage`, la gestion de la connectivité, le cache fichier JSON, la validation et les retours haptiques.
- **Injection de dépendances** : configurée dans `MauiProgram.cs` via `IServiceCollection`. Les services d'état (auth, base de données, sync) sont des **singletons** ; les ViewModels et services métier sont **transients**.
- **Resilience HTTP** : pipeline Polly (retry × 3, circuit-breaker, timeout) attaché au `HttpClient`.
- **Offline-first** : chaque module tente d'abord de charger les données locales (SQLite ou cache JSON) avant de solliciter l'API, et synchronise en arrière-plan à la reconnexion.

---

## 3. Configuration & démarrage

### MauiProgram.cs

Punto d'entrée du bootstrap de l'application :

1. **Services critiques initialisés en premier** : `AuthService` (singleton), `LocalDatabaseService` (singleton).
2. **HttpClient** configuré avec deux `DelegatingHandler` chaînés :
   - `TokenValidationHandler` : inspecte le JWT avant chaque requête ; lève `SessionExpired` si le token expire dans moins de 5 minutes ou si la réponse est 401.
   - `HttpLoggingHandler` : journalise chaque requête/réponse dans `IAppLogger`.
3. **Pipeline de résilience Polly** :
   - Retry : 3 tentatives avec backoff exponentiel (1 s, 2 s, 4 s).
   - Circuit-breaker : déclenche à 50 % d'échecs sur une fenêtre glissante de 30 s.
   - Timeout par requête : 10 s ; timeout global : 60 s.
4. **Enregistrement des services**, ViewModels et Views (transients).
5. **Customisation des handlers** : suppression du soulignement Android sur les `Entry` pour un style uniforme.
6. **Polices personnalisées** : OpenSans Regular et Semibold.

### App.xaml.cs

- Charge la base de données SQLite locale de manière asynchrone au démarrage.
- Écoute `Connectivity.ConnectivityChanged` → appelle `SyncService.FlushPendingRapportsAsync()` à la reconnexion.
- Écoute `AuthService.SessionExpired` → redirige vers la page de login.
- En mode DEBUG, lit `CrashLogger` et affiche un overlay avec le log de crash.

### AppSettings.cs

```json
{
  "ApiGatewayBaseUrl": "http://cynapharmgateway.runasp.net/",
  "ApiGatewayBaseUrlProd": null
}
```

L'URL de production est injectée en mode `Release`.

---

## 4. Authentification & sécurité

### AuthService

| Opération | Endpoint | Détails |
|---|---|---|
| Login | `POST auth/login` | Retourne un JWT + `UserInfo` |
| Forgot password | `POST auth/forgot-password` | Envoi d'un email de réinitialisation |
| Change password | `POST auth/change-password` | Vérifie l'ancien mot de passe |
| Me | `GET auth/me` | Infos utilisateur courantes |
| Logout | (local) | Efface `SecureStorage` et l'en-tête `Authorization` |

**Stockage des tokens** (`SecureStorage` — chiffrement natif de la plateforme) :

| Clé | Contenu |
|---|---|
| `JwtToken` | Bearer token brut |
| `TokenExpiry` | Timestamp d'expiration (parsé depuis les claims JWT) |
| `UserRole` | Rôle de l'utilisateur |
| `UserId` | Identifiant numérique |
| `UserName` | Prénom et nom |

**Validation proactive** : `TokenValidationHandler` inspecte le token avant chaque requête. Si l'expiration est dans moins de 5 minutes, l'événement `SessionExpired` est levé sans attendre la réponse du serveur. Un verrou double-checked évite les conditions de course en multi-thread.

---

## 5. Couche réseau & API

### ApiService

Classe centrale pour toutes les communications HTTP. Chaque réponse est normalisée en `ApiResponse<T>` :

```csharp
class ApiResponse<T> {
    bool IsSuccess;
    T    Result;
    string Message;
    string[] Errors;
}
```

La désérialisation est tolérante : si la réponse n'est pas encapsulée dans ce format, elle est désérialisée directement. La sérialisation JSON est insensible à la casse pour absorber les conventions mixtes du backend (camelCase, PascalCase, snake_case).

**Messages d'erreur en français** mappés aux codes HTTP courants :

| Code | Message affiché |
|---|---|
| 400 | Données invalides |
| 401 | Session expirée |
| 403 | Accès refusé |
| 404 | Ressource introuvable |
| 515 | Erreur serveur |
| 503 | Service indisponible |
| Timeout | Délai dépassé |

### ApiRoutes (constantes)

```
Auth      : login, forgot-password, change-password, me
Products  : base, search, categories, lots, promos, marketing
Orders    : base, lines, complaints
Field     : visites, rapports, plannings, objectifs, kpi, regions
Inventory : stocks-delegue, stock-movements, distributions, stocks-promotionnels
Documents : factures, bons-commandes, bons-livraison
```

---

## 6. Base de données locale & mode hors-ligne

### LocalDatabaseService (SQLite via sqlite-net-pcl)

Tables créées au premier lancement :

| Table | Colonnes principales | Usage |
|---|---|---|
| `Product_Cache` | id, reference, nom, categorie, prix, image, actif | Catalogue offline |
| `Stock_Local` | productId, quantiteRestante, dateExpiration | Échantillons délégué |
| `Pending_Rapports` | visiteId, contenu, resultat, latitude, longitude, isSynced | File d'attente rapports offline |
| `Promotion_Cache` | productId, titre, remise%, dateDebut, dateFin | Promotions offline |
| `Log_Entries` | timestamp, level, message, exception, context | Logs (515 max) |

**Opérations clés** :

- `SeedProductsAsync` / `SeedPromotionsAsync` : chargement en masse depuis l'API.
- `GetActivePromotionAsync(productId)` : priorité promotion produit-spécifique, puis globale (`ProductId == 0`), filtrée sur les dates.
- `InsertPendingRapportAsync` / `GetPendingRapportsAsync` : gestion de la file d'attente rapports.
- `DeductStockAsync(productId, quantite)` : soustraction atomique d'un stock local.

### SyncService

Singleton qui écoute `Connectivity.ConnectivityChanged`. À la reconnexion, `FlushPendingRapportsAsync` :
1. Récupère tous les rapports non synchronisés (`isSynced = false`).
2. Soumet chacun via `VisiteService.CreateRapportAsync()`.
3. Marque le rapport comme synchronisé en cas de succès (les échecs individuels ne bloquent pas les autres).
4. Protection thread-safe via `Interlocked.CompareExchange` pour éviter les exécutions concurrentes.

---

## 7. Modules fonctionnels

### 7.1 Login & Profil

#### Page de Login

- Formulaire email + mot de passe avec bascule de visibilité.
- Lien « Mot de passe oublié » → `ForgotPasswordPage`.
- À la connexion réussie, redirection selon le rôle :
  - `DELEGUE`, `SUPERVISEUR`, `ADMIN` → Dashboard
  - `PHARMACIEN`, `GROSSISTE`, `CLIENT` → Liste des commandes

#### Page de Profil

- Affichage : nom, email, rôle, téléphone, adresse.
- Badge avatar avec les initiales de l'utilisateur.
- Modification : nom, téléphone, adresse (sauvegarde locale).
- Changement de mot de passe : vérification de l'ancien mot de passe (minimum 6 caractères).
- Déconnexion avec boîte de dialogue de confirmation.

---

### 7.2 Dashboard

Vue d'accueil différenciée selon le rôle.

**Vue DELEGUE** :
- Bannière de bienvenue avec le nom de l'utilisateur et la date du jour.
- Nombre de visites du jour (chargé depuis `VisiteService.GetVisitesAsync(today)`).
- Boutons d'accès rapide : « Mes visites », « Planning ».
- KPIs et objectifs mis en cache localement pour un accès hors-ligne.

**Vue SUPERVISEUR / ADMIN** :
- Bannière « Vue Superviseur ».
- Liste des objectifs avec barres de progression (valeur cible vs valeur réalisée).
- Indicateurs KPI : période, indicateur, valeur, date de calcul.
- Liste des régions supervisées.
- Bouton d'accès rapide vers les Objectifs.

---

### 7.3 Visites & Rapports

#### Liste des Visites

- Filtres combinables : plage de dates (défaut : 30 derniers jours) + statut (`PLANIFIEE`, `REALISEE`, `ANNULEE`, Tous).
- Debounce 400 ms sur le changement de filtre pour éviter les appels API excessifs.
- Bouton « Nouvelle visite » → `VisitDetailPage`.
- Tap sur une visite → détail puis création de rapport.

#### Détail d'une Visite

- Création / modification : nom du client, date, notes.
- Endpoints : `POST /fields/visites`, `PUT /fields/visites/{id}`.

#### Création de Rapport (`RapportPage`)

Formulaire complet de compte-rendu de visite avec :

**Champs validés** :

| Champ | Règle de validation |
|---|---|
| Contenu | Requis, min. 20 caractères |
| Résultat | Sélection obligatoire parmi : POSITIF, NEGATIF, EN_ATTENTE |
| Produits discutés | Multi-sélection via cases à cocher (liste chargée depuis le catalogue) |

**Géolocalisation** :
- `PreCaptureLocationAsync` : récupère la dernière position connue (rapide, sans dialog de permission).
- `CaptureLocationAsync` : demande la permission `LocationWhenInUse`, attend jusqu'à 10 s pour une position précise.
- Indicateurs d'état : « Localisation en cours… », « Position capturée », « Permission refusée ».
- Fallback à `null` si le GPS est indisponible.

**Soumission** :
- En ligne → `POST /fields/rapports` immédiat.
- Hors ligne → insertion dans `Pending_Rapports`, alerte « Enregistré hors ligne ».

**Payload envoyé au backend** :
```json
{
  "Id_Rapport": 0,
  "Id_Visite": 123,
  "Commentaire": "...",
  "Resultat": "POSITIF",
  "Id_User_Delegue": 42,
  "Latitude": 36.7538,
  "Longitude": 3.0588
}
```

---

### 7.4 Planning

Vue calendrier hebdomadaire des visites planifiées.

- Grille 7 jours (lundi → dimanche) avec mise en évidence du jour courant.
- Navigation semaine précédente / suivante (pas plus d'un an en arrière).
- Les entrées (`Planning`) affichent : nom du client, `HeureDebut`, `HeureFin`, `Etat`.
- Bouton « Ajouter » → `VisitDetailPage` (création de nouvelle visite).

---

### 7.5 Catalogue Produits

#### Liste des Produits

- **Recherche en temps réel** :
  - Debounce 300 ms, déclenché à partir de 3 caractères saisis.
  - Recherche locale (SQLite) en premier, fallback sur l'API si aucun résultat local.
  - Message d'aide « Entrez au moins 3 caractères… » pendant la saisie.
- **Filtres par catégorie** : puces défilables horizontalement chargées depuis `GetCategoriesAsync()`.
- Bandeau « Mode hors ligne — catalogue du dernier chargement » affiché si l'appareil est hors ligne.
- Navigation vers le détail produit via `GoToDetailAsync`.

#### Détail d'un Produit

- Fiche complète : description, image, prix.
- Lots associés (`GetLotsByProductAsync`).
- Promotions actives (`GetPromotionsAsync`).
- Supports marketing.

---

### 7.6 Commandes

#### Liste des Commandes

- **Filtre par statut** : Tous, `EN_ATTENTE`, `CONFIRMEE`, `LIVREE`, `ANNULEE`.
- **Pagination** : 20 commandes par page, chargement incrémental via « Charger plus ».
- Navigation vers le détail → `OrderDetailPage`.

#### Création de Commande (Wizard 3 étapes)

**Étape 1 — Construction du panier** :
- Recherche produits (online ou offline depuis le cache).
- Ajout au panier avec validation de quantité (`Range 1–9999`).
- **Moteur de promotions** :
  - Interroge SQLite (`GetActivePromotionAsync`).
  - Priorité : promotion produit-spécifique > promotion globale (`ProductId == 0`).
  - Filtre sur les dates (`DateDebut ≤ now ≤ DateFin`).
  - Affiche : prix original barré, % de remise, prix effectif, économies totales.
- Panier persisté dans `Preferences` (survit à la navigation).

**Étape 2 — Notes de livraison**.

**Étape 3 — Validation & confirmation**.

**Payload de commande** :
```json
{
  "Lignes": [
    {
      "Id_Produit": 5,
      "Quantite": 10,
      "PrixUnitaire": 450.00,
      "Remise": 10.0
    }
  ]
}
```

#### Réclamations

Disponibles depuis le détail commande via `CreateReclamationAsync(commandeId, motif)`.

---

### 7.7 Documents

Centralise tous les documents commerciaux disponibles.

**Types** (sélecteur segmenté) :

| Type | Champs affichés |
|---|---|
| Factures | NumeroFacture, DateFacture, Statut, MontantTTC |
| Bons de commande | NumeroBon, DateEmission, Statut, MontantTotal |
| Bons de livraison | NumeroBon, DateLivraison, Statut |

- Pagination par page/taille.
- Navigation vers le détail de chaque document.

---

### 7.8 Stock & Distribution

#### Mon Stock (`MyStockPage`)

Deux onglets :

**Onglet Échantillons** (StockDelegue) :
- Affiche : nom produit, quantité allouée / restante, date d'expiration.
- Bouton « Distribuer » pour chaque échantillon disponible (`CanDistribute = QuantiteRestante > 0`).

**Onglet Promotionnels** (StockPromo) :
- Affiche : nom produit, quantité disponible.

#### Flux de Distribution

1. L'utilisateur appuie sur « Distribuer ».
2. `DeductStockAsync` : soustraction atomique dans SQLite.
3. Mise à jour de la collection en mémoire.
4. Rafraîchissement de l'affichage.
5. Synchronisation en arrière-plan : `InventoryService.PostDistributionAsync(productId, quantite, latitude?, longitude?)` — non bloquant, best-effort.
6. Snackbar de succès avec retour haptique.

**Données GPS** jointes à chaque distribution comme preuve de présence terrain.

**Cache** : `MemoryCacheService` avec TTL de 5 minutes. Invalidé manuellement à chaque rafraîchissement.

---

### 7.9 Objectifs & KPIs

#### Page Objectifs (`ObjectifPage`)

- Liste des objectifs avec : type, période, valeur cible, valeur réalisée, barre de progression.
- `GlobalAchievement` : pourcentage moyen sur l'ensemble des objectifs.

**Mapping des types** :

| Code | Libellé |
|---|---|
| 1 | Visites |
| 2 | Chiffre d'affaires |
| 3 | Nouveaux clients |
| 4 | Fidélisation |

**Mapping des périodes** :

| Code | Libellé |
|---|---|
| 1 | Mensuel |
| 2 | Trimestriel |
| 3 | Annuel |

- `ProgressValue` : ratio `ValeurActuelle / ValeurCible` clamped entre 0 et 1.
- Accessible aux rôles `SUPERVISEUR` et `ADMIN` uniquement.

---

## 8. Modèles de données

### Authentification

```csharp
record LoginRequest(string UserName, string Password);

class UserInfo {
    int    Id;
    string Name, Email, Role;
    string Telephone, Adresse;
    int?   RegionId;
}
// Rôles : DELEGUE | SUPERVISEUR | PHARMACIEN | GROSSISTE | CLIENT | ADMIN
```

### Produits

```csharp
class Product {
    int     Id;           // id_Produit (JSON)
    string  Reference, Nom, Description, Categorie;
    decimal PrixUnitaire; // prix_Vente (JSON)
    string  ImageUrl;
    bool    Actif;        // isActive (JSON)
    bool    IsArchived;
}

class Promotion {
    int?    ProductId;    // null = globale
    string  Titre;
    decimal RemisePourcentage;
    DateTime DateDebut, DateFin;
}
```

### Commandes

```csharp
class Order {
    int      Id;               // id_Commande (JSON)
    string   NumeroCommande;   // calculé : "CMD-{Id:D5}"
    DateTime DateCommande;
    string   Statut;
    decimal  MontantTotal;     // montantTotalHT (JSON)
}

class CartLine {
    int     ProductId;
    string  ProductNom;
    int     Quantite;
    decimal PrixOriginal, PrixUnitaire, RemisePourcentage;
    string  PromoTitre;
    bool    HasPromo;
    decimal SousTotal, EconomieTotale;
}
```

### Terrain (Field)

```csharp
class Visite {
    int      Id;           // idVisite (JSON)
    string   ClientNom, ClientType;
    DateTime DateVisite;
    string   Notes, Statut;
    bool     HasRapport, IsCompleted;
}

class Rapport {
    int      Id, VisiteId;
    string   Contenu, Resultat;
    string   ProduitsDiscutes;
    DateTime DateSoumission;
    double?  Latitude, Longitude;
}

class Planning {
    int      Id;           // id_Planning (JSON)
    DateTime DatePlanifiee;
    TimeSpan HeureDebut, HeureFin;
    string   Etat, ClientNom, Objectif;
}
```

### Documents

```csharp
class Facture       { int Id; string NumeroFacture; DateTime DateFacture; string Statut; decimal MontantTTC; }
class BonCommande   { int Id; string NumeroBon; DateTime DateEmission; string Statut; decimal MontantTotal; }
class BonLivraison  { int Id; string NumeroBon; DateTime DateLivraison; string Statut; }
```

### Inventaire

```csharp
class StockDelegue {
    int      Id, ProductId;
    string   ProductNom;
    int      QuantiteAllouee, QuantiteRestante;
    DateTime? DateExpiration;
}

class StockPromo { int ProductId; string ProductNom; int Quantite; }
```

---

## 9. Services & injection de dépendances

### Durées de vie

| Service | Durée de vie | Rôle |
|---|---|---|
| `AuthService` | Singleton | Gestion des sessions JWT |
| `LocalDatabaseService` | Singleton | Base de données SQLite |
| `SyncService` | Singleton | Synchronisation hors-ligne |
| `IAppLogger` / `AppLogger` | Singleton | Journalisation applicative |
| `ICacheService` / `MemoryCacheService` | Singleton | Cache mémoire TTL |
| `ProductService` | Transient | Appels API produits |
| `OrderService` | Transient | Appels API commandes |
| `VisiteService` | Transient | Appels API visites/rapports |
| `PlanningService` | Transient | Appels API planning |
| `KpiService` | Transient | Appels API KPIs/objectifs |
| `InventoryService` | Transient | Appels API inventaire |
| `DocumentService` | Transient | Appels API documents |
| Tous les ViewModels | Transient | Logique de présentation |
| Toutes les Views | Transient | Pages XAML |

### Services transversaux

**IAppLogger / AppLogger** : méthodes `LogError`, `LogWarning`, `LogInformation`. Persiste dans `LocalDatabase` (max 515 entrées).

**ICacheService / MemoryCacheService** : pattern `GetOrCreateAsync<T>(key, factory, ttl)`. Invalidation unitaire ou globale.

**HapticService** : retours haptiques `Success()`, `Error()`, `Light()`, `Medium()`, `Heavy()` — implémentation spécifique à chaque plateforme.

**INavigationService / ShellNavigationService** : encapsule `Shell.Current.GoToAsync()` pour faciliter les tests.

**CrashLogger** (statique) : écrit dans `crash_log.txt`, lu et effacé au démarrage en DEBUG.

---

## 10. Navigation & structure des pages

L'application utilise le **Shell MAUI** avec un menu flyout latéral personnalisé (logo pharmacie en en-tête) dont la visibilité des items est contrôlée par le rôle.

### Routes enregistrées

```
/login
//dashboard
/visits
  /visits/detail
  /visits/rapport
/planning
/products
  /products/detail
/orders
  /orders/detail
  /orders/create
/documents
  /documents/detail
/stock
/objectifs
/profile
```

### Diagramme de navigation

```
Login
  └─> Dashboard (rôle DELEGUE/SUPERVISEUR/ADMIN)
        ├─> Visites ─> Détail Visite ─> Rapport
        ├─> Planning
        ├─> Objectifs
        └─> ...

  └─> Orders (rôle PHARMACIEN/GROSSISTE/CLIENT)
        ├─> Détail Commande ─> (Réclamation)
        └─> Créer Commande (wizard 3 étapes)

Flyout (toujours accessible)
  ├─> Catalogue ─> Détail Produit
  ├─> Documents ─> Détail Document
  ├─> Mon Stock
  └─> Profil ─> Mot de passe oublié
```

---

## 11. Gestion des rôles & permissions

| Fonctionnalité | DELEGUE | SUPERVISEUR | PHARMACIEN / GROSSISTE / CLIENT | ADMIN |
|---|:---:|:---:|:---:|:---:|
| Dashboard (visites du jour) | ✓ | — | — | ✓ |
| Dashboard (KPIs, objectifs, régions) | — | ✓ | — | ✓ |
| Visites & Rapports | ✓ | — | — | ✓ |
| Planning hebdomadaire | ✓ | — | — | ✓ |
| Catalogue produits | ✓ | ✓ | ✓ | ✓ |
| Commandes (voir & créer) | ✓ | ✓ | ✓ | ✓ |
| Documents (factures, BOC, BOL) | — | ✓ | ✓ | ✓ |
| Mon Stock & Distribution | ✓ | — | — | ✓ |
| Objectifs | — | ✓ | — | ✓ |
| Profil & Mot de passe | ✓ | ✓ | ✓ | ✓ |

---

## 12. Gestion des erreurs & résilience

### Couche réseau

| Mécanisme | Configuration |
|---|---|
| Retry | 3 tentatives, backoff exponentiel (1 s / 2 s / 4 s) |
| Circuit-breaker | 50 % d'échecs sur 30 s → ouverture du circuit |
| Timeout par requête | 10 s |
| Timeout global | 60 s |

### Couche ViewModel

`BaseViewModel.ExecuteAsync` est le wrapper standard pour toutes les opérations asynchrones :
1. Positionne `IsBusy = true`.
2. Exécute l'opération.
3. En cas d'exception : mappe le type (`ApiException`, `HttpRequestException`, `TaskCanceledException`) en message français lisible, déclenche le retour haptique d'erreur, appelle `IAppLogger.LogError`.
4. Remet `IsBusy = false` dans le `finally`.

### Gestion de session

- `TokenValidationHandler` lève `SessionExpired` à -5 min avant expiration.
- `App.xaml.cs` écoute cet événement et navigue vers `/login`.
- Toute réponse 401 déclenche immédiatement la déconnexion.

### Logs & crashes

- `AppLogger` conserve les 515 derniers logs dans SQLite.
- `CrashLogger` écrit `crash_log.txt` pour les exceptions non gérées.
- En mode DEBUG, un overlay à l'écran affiche le log de crash au démarrage.

---

## 13. UI, styles & ressources

### Palette de couleurs (`Colors.xaml`)

| Nom | Valeur | Usage |
|---|---|---|
| `Primary` | `#1A6B3C` | Boutons principaux, en-têtes |
| `Secondary` | `#F5A623` | Badges, accents |
| `BrandAccent` | `#00B4D8` | CTA, icônes actives |
| `Tertiary` | `#2C3E50` | Textes sombres |
| `PageBackgroundColor` | `#EEF3F8` | Fond général des pages |
| `CardBackgroundColor` | `White` | Fond des cartes |

Couleurs sémantiques pour les thèmes clair/sombre, états de champ et indicateurs de statut.

### Styles globaux (`Styles.xaml`)

| Style | Application |
|---|---|
| `Button` (global) | Fond `BrandAccent`, hauteur 52, coins arrondis, texte gras |
| `CardStyle` | Fond blanc, bordure 1 px, coins arrondis, ombre légère |
| `SectionTitleStyle` | 16 pt, gras |
| `PageTitleStyle` | 18 pt, gras |
| `EmptyStateStyle` | Centré, gris, italique |
| `Entry` (global) | Sans soulignement Android |

### Convertisseurs de valeurs

| Convertisseur | Entrée → Sortie |
|---|---|
| `IsNotNullOrEmptyConverter` | `string` → `bool` (visibilité) |
| `InvertedBoolConverter` | `bool` → `!bool` |
| `StatusColorConverter` | Statut commande → couleur (`EN_ATTENTE`=Orange, `CONFIRMEE`/`LIVREE`=Vert, `ANNULEE`=Rouge) |

---

## 14. Structure des fichiers

```
Cynapharm-Mobile/
├── App.xaml(.cs)                        # Point d'entrée, cycle de vie
├── AppShell.xaml(.cs)                   # Shell de navigation + menu flyout
├── MauiProgram.cs                       # DI, Polly, fonts, handlers
├── AppSettings.cs                       # URLs API
├── StorageKeys.cs                       # Constantes SecureStorage
│
├── Resources/
│   ├── Styles/
│   │   ├── Colors.xaml                  # Palette de marque
│   │   └── Styles.xaml                  # Styles globaux + convertisseurs
│   └── Raw/
│       └── appsettings.json             # Config runtime
│
├── Models/
│   ├── Auth/          (LoginRequest, LoginResponse, UserInfo, ChangePasswordRequest)
│   ├── Common/        (ApiResponse<T>, PagedResult<T>, LogEntry)
│   ├── Products/      (Product, Lot, Promotion, ProductCheckItem)
│   ├── Orders/        (Order, CartLine, LigneCommande, Reclamation)
│   ├── Field/         (Visite, Rapport, Planning, Objectif, Kpi, Region)
│   ├── Documents/     (DocumentSummary, Facture, BonCommande, BonLivraison)
│   └── Inventory/     (StockDelegue, StockPromo, StockMouvement, StockDisplayItem)
│
├── Services/
│   ├── AuthService.cs
│   ├── ApiService.cs
│   ├── LocalDatabaseService.cs
│   ├── SyncService.cs
│   ├── ProductService.cs
│   ├── OrderService.cs
│   ├── VisiteService.cs
│   ├── PlanningService.cs
│   ├── KpiService.cs
│   ├── InventoryService.cs
│   ├── DocumentService.cs
│   ├── Api/
│   │   ├── ApiRoutes.cs
│   │   ├── ApiException.cs
│   │   ├── TokenValidationHandler.cs
│   │   └── HttpLoggingHandler.cs
│   ├── Logging/       (IAppLogger, AppLogger)
│   ├── Cache/         (ICacheService, MemoryCacheService)
│   ├── Navigation/    (INavigationService, ShellNavigationService)
│   ├── Diagnostics/   (CrashLogger.cs)
│   └── Platform/      (HapticService.cs)
│
├── ViewModels/
│   ├── Base/          (BaseViewModel)
│   ├── Auth/          (LoginViewModel, ForgotPasswordViewModel)
│   ├── Dashboard/     (DashboardViewModel)
│   ├── Visites/       (VisitListViewModel, VisitDetailViewModel)
│   ├── Rapports/      (RapportViewModel)
│   ├── Planning/      (PlanningViewModel)
│   ├── Products/      (ProductListViewModel, ProductDetailViewModel)
│   ├── Orders/        (OrderListViewModel, OrderDetailViewModel, CreateOrderViewModel)
│   ├── Documents/     (DocumentListViewModel, DocumentDetailViewModel)
│   ├── Stock/         (MyStockViewModel)
│   ├── Objectifs/     (ObjectifViewModel)
│   └── Profile/       (ProfileViewModel)
│
├── Views/
│   ├── Auth/          (LoginPage, ForgotPasswordPage)
│   ├── Dashboard/     (DashboardPage)
│   ├── Visites/       (VisitListPage, VisitDetailPage)
│   ├── Rapports/      (RapportPage)
│   ├── Planning/      (PlanningPage)
│   ├── Products/      (ProductListPage, ProductDetailPage)
│   ├── Orders/        (OrderListPage, OrderDetailPage, CreateOrderPage)
│   ├── Documents/     (DocumentListPage, DocumentDetailPage)
│   ├── Stock/         (MyStockPage)
│   ├── Objectifs/     (ObjectifPage)
│   └── Profile/       (ProfilePage)
│
├── Converters/        (InvertedBoolConverter, IsNotNullOrEmptyConverter, StatusColorConverter)
│
└── Platforms/
    ├── Android/       (MainActivity, MainApplication, AndroidManifest.xml)
    ├── iOS/           (AppDelegate, Program)
    ├── MacCatalyst/   (AppDelegate, Program)
    └── Windows/       (App.xaml, Package.appxmanifest)
```

---

## Résumé des capacités offline

| Fonctionnalité | Hors ligne | Mécanisme |
|---|---|---|
| Catalogue produits | ✓ | SQLite `Product_Cache` |
| Recherche produits | ✓ | SQLite LINQ |
| Promotions | ✓ | SQLite `Promotion_Cache` |
| Panier commande | ✓ | `Preferences` |
| Création rapport | ✓ (différée) | SQLite `Pending_Rapports` + sync auto |
| Stock échantillons | ✓ | SQLite `Stock_Local` |
| Distribution | ✓ (locale) | SQLite + sync background |
| Dashboard | ✓ (cache) | Fichier JSON `AppDataDirectory` |
| Visites, Planning | ✗ | Données en temps réel uniquement |
| Documents | ✗ | Données en temps réel uniquement |
