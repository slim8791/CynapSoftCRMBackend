# Gestion des Utilisateurs — Analyse complète
# Backend + Angular + MAUI

> Généré le 2026-05-29 — analyse STATIQUE uniquement, aucun fichier modifié.

---

## PARTIE 1 — BACKEND (AuthAPI)

---

### 1.1 Routes existantes

| Verb | URL | Roles autorisés | Description |
|------|-----|-----------------|-------------|
| POST | `/api/auth/register` | ADMIN, SUPERVISEUR, DELEGUE | Créer un compte. SUPERVISEUR → DELEGUE/CLIENT seulement. DELEGUE → CLIENT seulement. |
| POST | `/api/auth/login` | Public | Authentification, retourne JWT. Vérifie Turnstile sauf header `X-Client-Type: mobile`. |
| GET  | `/api/auth/users` | ADMIN, SUPERVISEUR | Liste tous les utilisateurs. SUPERVISEUR reçoit uniquement DELEGUE+CLIENT. |
| GET  | `/api/auth/users/{id}` | ADMIN, SUPERVISEUR | Détail d'un utilisateur par ID entier. DELEGUE → **403 Forbidden**. |
| GET  | `/api/auth/users/by-role/{role}` | ADMIN, SUPERVISEUR, DELEGUE | Liste par rôle. DELEGUE → CLIENT uniquement. SUPERVISEUR → DELEGUE ou CLIENT uniquement. |
| GET  | `/api/auth/users/search` | ADMIN, SUPERVISEUR | Recherche par mot-clé (≥3 chars). Filtre `isActive` optionnel. **Ne filtre PAS par rôle pour SUPERVISEUR.** |
| GET  | `/api/auth/disabled-users` | ADMIN | Liste des utilisateurs désactivés (`IsDeleted = true`). |
| PUT  | `/api/auth/update-profile` | Tout authentifié | Mise à jour profil. Lookup par **EMAIL**. Modifie Name, PhoneNumber, Adresse. |
| PUT  | `/api/auth/change-role` | ADMIN, SUPERVISEUR | Change le rôle d'un utilisateur (supprime tous les rôles, assigne le nouveau). |
| PUT  | `/api/auth/change-password` | Tout authentifié | Changement mot de passe. Vérifie que l'email JWT == email du body. |
| PUT  | `/api/auth/enable-user/{email}` | ADMIN | Réactive un utilisateur (`IsDeleted = false`). |
| PUT  | `/api/auth/delete-user/{email}` | ADMIN | Désactive un utilisateur (`IsDeleted = true`, soft delete). |
| PUT  | `/api/auth/add-role` | ADMIN, SUPERVISEUR | Ajoute un rôle sans supprimer les existants (via `RegistrationRequestDto.Email + Role`). |
| POST | `/api/auth/AssignRole` | ADMIN, SUPERVISEUR | Assigne un rôle par email + UserRole enum. |
| POST | `/api/auth/forgot-password` | Public | Génère token reset, envoie email. |
| PUT  | `/api/auth/reset-password` | Public | Réinitialise le mot de passe avec le token. |

**Résumé des restrictions par rôle appelant :**

| Action | ADMIN | SUPERVISEUR | DELEGUE |
|--------|-------|-------------|---------|
| Voir tous les utilisateurs | ✅ Tous | ✅ DELEGUE+CLIENT seulement | ❌ |
| Voir utilisateur par ID | ✅ | ✅ | ❌ **403** |
| Voir utilisateurs par rôle | ✅ Tous | ✅ DELEGUE ou CLIENT | ✅ CLIENT seulement |
| Créer un compte | ✅ Tous rôles | ✅ DELEGUE, CLIENT | ✅ CLIENT seulement |
| Changer rôle | ✅ | ✅ (sans restriction sur le nouveau rôle !) | ❌ |
| Désactiver/Réactiver | ✅ | ❌ | ❌ |
| Rechercher | ✅ | ✅ (non filtré !) | ❌ |

---

### 1.2 Modèle ApplicationUser (`Utilisateur.cs`)

```csharp
public class Utilisateur : IdentityUser<int>
{
    public string Name { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;
    // Hérité de IdentityUser<int> :
    // int Id, string Email, string UserName, string? PhoneNumber,
    // string NormalizedEmail, string PasswordHash, etc.
}
```

**Vérification des champs demandés :**

| Champ | Existe ? | Détail |
|-------|----------|--------|
| `IdRegion` | ❌ **ABSENT** | Ni dans `Utilisateur`, ni dans aucun sous-modèle. Filtrage par région impossible dans AuthAPI. |
| `TypeClient` | ❌ **Pas un champ DB** | Calculé à runtime via `GetTypeClient(user)` → retourne `"PHARMACIEN"` si instance de `Pharmacien`, `"GROSSISTE"` si `Grossiste`, `null` sinon. |
| `IsDeleted` | ✅ `bool IsDeleted = false` | Soft delete. Présent dans `Utilisateur`. |

**Hiérarchie de classes (héritage TPH/TPT) :**
- `Utilisateur` (base) → `Pharmacien` (NomOfficine, TypePharmacie), `Grossiste` (RaisonSociale)
- Tous les non-CLIENT (ADMIN, SUPERVISEUR, DELEGUE, MEDECIN) sont instanciés comme `Utilisateur` de base.

---

### 1.3 Création d'utilisateur

**Champs de `RegistrationRequestDto` :**

```csharp
public class RegistrationRequestDto
{
    public string Email         { get; set; }  // requis
    public string Name          { get; set; }  // requis
    public string PhoneNumber   { get; set; }  // requis
    public string Password      { get; set; }  // requis
    public string Adresse       { get; set; }  // requis
    public string? NomOfficine  { get; set; }  // Pharmacien seulement
    public string? TypePharmacie{ get; set; }  // Pharmacien seulement
    public string? RaisonSociale{ get; set; }  // Grossiste seulement
    public UserRole Role        { get; set; }  // CLIENT / ADMIN / SUPERVISEUR / DELEGUE / MEDECIN
    public UserType UserType    { get; set; }  // PHARMACIEN / GROSSISTE (obligatoire si Role=CLIENT)
}
```

**Vérification du rôle de l'appelant :**
- ✅ OUI — le backend vérifie `User.FindFirstValue(ClaimTypes.Role)` avant de créer.
- SUPERVISEUR → peut créer DELEGUE et CLIENT.
- DELEGUE → peut créer CLIENT seulement.
- Si non conforme → `Forbid()` (mais `_response` est déjà modifié avant le `return Forbid()` — le body 403 sera vide côté client).

**Limitation :** `UserType` est obligatoire si `Role == CLIENT` (le switch lève `ArgumentException` si `UserType` est invalide). La MAUI doit toujours envoyer un `UserType` valide pour les CLIENTs.

---

### 1.4 Mise à jour utilisateur

**Endpoint `PUT /auth/update-profile` :**
- ✅ Existe. Accessible à tout utilisateur authentifié (`[Authorize]` sans rôle).
- Champs modifiables : `Name`, `PhoneNumber`, `Adresse`.
- Lookup par **EMAIL** (pas par ID).
- Le role n'est **PAS** modifiable via cet endpoint.

**Endpoint `PUT /auth/users/{id}` :**
- ❌ **N'existe pas.** Pas de route PUT avec ID numérique.

**Endpoint `PUT /auth/change-role` :**
- ✅ Existe. Roles: ADMIN, SUPERVISEUR.
- Supprime tous les rôles actuels et assigne le nouveau.
- ⚠️ Aucune restriction sur le *nouveau* rôle demandé — un SUPERVISEUR pourrait techniquement assigner n'importe quel rôle (y compris ADMIN) via cet endpoint.

---

### 1.5 Filtrage par région

- ❌ `ApplicationUser` (`Utilisateur`) **n'a pas de champ IdRegion**.
- `Region` (FieldAPI) a `Id_User_Delegue` — un délégué par région.
- `Region` n'a **pas** de `Id_User_Superviseur`.
- Filtrage par région depuis AuthAPI : **IMPOSSIBLE sans modification du modèle ou appel cross-service.**

**Options pour un SUPERVISEUR qui veut voir uniquement les délégués de sa région :**

| Option | Coût | Risque |
|--------|------|--------|
| A) Ajouter `IdRegion` dans `Utilisateur` + migration AuthAPI | MOYEN | Migration DB requise |
| B) Appel Angular : FieldAPI → régions du superviseur → IDs des délégués → AuthAPI filtre | MOYEN | 2 appels réseau, logique Angular complexe |
| C) SUPERVISEUR voit tous les DELEGUE+CLIENT, sans filtre région | SIMPLE | Fonctionnel pour PFE |

**`Region.cs` (FieldAPI) :**
```csharp
public class Region
{
    [Key]
    public int Id_Region { get; set; }
    public string NomRegion { get; set; }
    public string CodePostal { get; set; }
    [Required]
    public int Id_User_Delegue { get; set; }
    // Pas de Id_User_Superviseur
    // Pas de collection Delegues
}
```

---

### 1.6 Ce qui manque dans le backend

| # | Problème | Gravité | Impact |
|---|----------|---------|--------|
| 1 | `GET /auth/users/{id}` interdit aux DELEGUE — bloque l'accès aux détails client depuis MAUI | **CRITIQUE** | MAUI ClientDetailPage non fonctionnelle pour DELEGUE |
| 2 | `GET /auth/users/search` ne filtre pas par rôle pour SUPERVISEUR | MINEUR | SUPERVISEUR peut voir noms d'ADMIN dans les résultats de recherche |
| 3 | `PUT /auth/change-role` ne vérifie pas le nouveau rôle côté SUPERVISEUR | MINEUR | SUPERVISEUR pourrait assigner ADMIN (peu probable en pratique) |
| 4 | `IdRegion` absent de `Utilisateur` | MOYEN | Filtrage région impossible côté AuthAPI |
| 5 | `UserListItem` dans MAUI ne contient que Id, Name, TypeClient | MOYEN | Liste clients sans email/téléphone/adresse |
| 6 | `UpdateProfileAsync` lookup par EMAIL, pas par ID | INFO | Implique que le DELEGUE doit avoir l'email du client pour le mettre à jour |

---

## PARTIE 2 — ANGULAR

---

### 2.1 UserService Angular — méthodes existantes

| Méthode | URL | Description |
|---------|-----|-------------|
| `getUsers()` | GET `/auth/users` | Tous les utilisateurs (ADMIN: tous, SUPERVISEUR: DELEGUE+CLIENT) |
| `getAllUsers()` | GET `/auth/users` | Doublon de `getUsers()` |
| `getUsersByRole(role)` | GET `/auth/users/by-role/{role}` | Par rôle, avec unwrap du `Result` |
| `getUserById(id)` | GET `/auth/users/{id}` | Détail par ID |
| `registerUser(payload)` | POST `/auth/register` | Créer un compte |
| `changeRole(payload)` | PUT `/auth/change-role` | Changer le rôle |
| `disableUser(email)` | PUT `/auth/delete-user/{email}` | Désactiver |
| `enableUser(email)` | PUT `/auth/enable-user/{email}` | Réactiver |
| `searchUsers(keyword, isActive?)` | GET `/auth/users/search?keyword=...` | Recherche backend |
| `getDisabledUsers()` | GET `/auth/users/disabled` | ⚠️ **URL INCORRECTE** — le backend expose `/auth/disabled-users` |

**Bug détecté :** `getDisabledUsers()` appelle `/auth/users/disabled` (404) au lieu de `/auth/disabled-users`.

---

### 2.2 UserListComponent — état actuel

**Accès :**
- Pas de guard de route visible dans ce fichier. La route elle-même doit être protégée via un `AuthGuard` dans `users-routing.module.ts`.
- Le composant vérifie le rôle via `AuthService.getUserRole()`.

**Filtrage par rôle :**
- ✅ SUPERVISEUR → appel `forkJoin([getUsersByRole('DELEGUE'), getUsersByRole('CLIENT')])`.
- ✅ ADMIN → appel `getUsers()`.
- Dropdown rôle : SUPERVISEUR voit `['DELEGUE', 'CLIENT']`, ADMIN voit tous les rôles.

**Boutons visibles :**
- Créer : `*ngIf="canCreate"` → `isAdmin || isSuperviseur` ✅
- Modifier : `*ngIf="canEdit"` → `isAdmin || isSuperviseur` ✅
- Désactiver : `*ngIf="isAdmin && !user.isDeleted"` ✅
- Réactiver : `*ngIf="isAdmin && user.isDeleted"` ✅
- Voir (œil) : visible pour tous, sans condition ✅

**Recherche :**
- ⚠️ La barre de recherche est visible pour tous les rôles (ADMIN et SUPERVISEUR). Pas de condition `*ngIf="isAdmin"` sur la barre de recherche.
- La recherche backend (`/auth/users/search`) ne filtre pas par rôle pour SUPERVISEUR — un SUPERVISEUR pourrait trouver des ADMIN dans les résultats.

---

### 2.3 UserFormComponent — état actuel

**Champs du formulaire :**

| Champ | En création | En édition | Note |
|-------|-------------|------------|------|
| `name` | ✅ requis | Readonly | |
| `email` | ✅ requis, email | Readonly | |
| `phoneNumber` | ✅ optionnel | Caché (section `!isEditMode`) | |
| `adresse` | ✅ requis | Caché | |
| `role` | ✅ requis, dropdown filtré | Readonly si SUPERVISEUR, dropdown si ADMIN | |
| `userType` | ✅ si role=CLIENT | Caché | |
| `password` | ✅ requis (≥6 chars, 1 spécial) | Caché | |

**Filtrage dropdown rôles :**
- ✅ `availableRoles` getter : SUPERVISEUR → `['DELEGUE', 'CLIENT']`, sinon tous les `UserRole`.
- SUPERVISEUR en mode édition → affiche un champ `input` readonly (ne peut pas modifier le rôle).

**En mode édition :**
- Appelle `changeRole({ email: userEmail, newRole: form.role })` → PUT `/auth/change-role`.
- SUPERVISEUR est autorisé par le backend sur cet endpoint.
- ⚠️ Seul le rôle est envoyé en édition — Name/PhoneNumber/Adresse ne sont pas modifiables via cette interface. L'interface affiche "En mode édition, seul le rôle peut être modifié."

---

### 2.4 Ce qui manque dans Angular

| Feature | Statut | Impact |
|---------|--------|--------|
| Bug `getDisabledUsers()` → mauvaise URL `/auth/users/disabled` | ❌ Bug | Page utilisateurs désactivés brisée |
| Recherche SUPERVISEUR non filtrée par rôle (résultats peuvent inclure ADMIN) | ⚠️ Sécurité légère | Fuite d'information mineure |
| Modifier le profil complet en édition (pas seulement le rôle) | ❌ Manquant | Pas de mise à jour Name/Tel/Adresse depuis Angular |
| Filtre région pour SUPERVISEUR | ❌ Manquant | SUPERVISEUR voit tous DELEGUE+CLIENT sans distinction de région |

---

## PARTIE 3 — MAUI

---

### 3.1 UserService MAUI — méthodes existantes

| Méthode | URL | Description |
|---------|-----|-------------|
| `GetUsersByRoleAsync(string role)` | GET `auth/users/by-role/{role}` | Liste par rôle. DELEGUE peut appeler avec "CLIENT". |
| `GetUserByIdAsync(int id)` | GET `auth/users/{id}` | ⚠️ **BLOQUÉ pour DELEGUE** — endpoint ADMIN+SUPERVISEUR seulement |
| `CreateUserAsync(CreateUserDto dto)` | POST `auth/register` | Créer compte. DELEGUE peut créer CLIENT. |
| `UpdateUserAsync(UpdateUserDto dto)` | PUT `auth/update-profile` | Mise à jour profil. Lookup par EMAIL. |

**Bug critique :** `GetUserByIdAsync` appelle `GET auth/users/{id}` qui est `[Authorize(Roles = "ADMIN,SUPERVISEUR")]`. Un DELEGUE connecté recevra **403 Forbidden** — la page `ClientDetailPage` plantera pour tout DELEGUE.

---

### 3.2 Modèles User/UserDto MAUI

**`UserInfo.cs`** (résultat de `GetUserByIdAsync`) :

```csharp
public class UserInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string? RegionId { get; set; }          // ⚠️ FANTÔME — pas dans le backend UserDto
    [JsonPropertyName("phoneNumber")]
    public string? Telephone { get; set; }
    public string? Adresse { get; set; }
}
```

**`UserListItem.cs`** (résultat de `GetUsersByRoleAsync`) :

```csharp
public class UserListItem
{
    public int    Id   { get; set; }
    public string Name { get; set; }
    [JsonPropertyName("typeClient")]
    public string? TypeClient { get; set; }        // "PHARMACIEN" ou "GROSSISTE" pour CLIENTs
    // ⚠️ MANQUE : Email, PhoneNumber, Adresse, Role
}
```

**`CreateUserDto.cs`** :

```csharp
public class CreateUserDto
{
    public string  Email       { get; set; }   // [JsonPropertyName("email")]
    public string  Name        { get; set; }   // [JsonPropertyName("name")]
    public string  Password    { get; set; }   // [JsonPropertyName("password")]
    public string? PhoneNumber { get; set; }   // [JsonPropertyName("phoneNumber")]
    public string? Adresse     { get; set; }   // [JsonPropertyName("adresse")]
    public int     Role        { get; set; }   // int (enum) — CLIENT = 4
    public int     UserType    { get; set; }   // int (enum) — PHARMACIEN = 0, GROSSISTE = 1
}
```

⚠️ `Role` et `UserType` sont envoyés comme **int**, mais le backend `RegistrationRequestDto` déclare `Role` comme `UserRole` (enum) et `UserType` comme `UserType` (enum). La désérialisation JSON de System.Text.Json convertit automatiquement les int vers les enum — **cela fonctionne** à condition que les valeurs numériques correspondent.

Vérification de l'enum backend :
```csharp
public enum UserRole { ADMIN=0, SUPERVISEUR=1, DELEGUE=2, MEDECIN=3, CLIENT=4 }
```
Donc `Role = 4` → CLIENT ✅. Mais si le backend change l'ordre de l'enum, la MAUI sera brisée.

**`UpdateUserDto.cs`** :

```csharp
public class UpdateUserDto
{
    public string  Email       { get; set; }   // requis par backend (lookup)
    public string? Name        { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Adresse     { get; set; }
}
```

**Vérification des champs demandés :**

| Champ | UserInfo | UserListItem | Note |
|-------|----------|--------------|------|
| `IdRegion` | `RegionId` (string?) ⚠️ | ❌ absent | `RegionId` dans UserInfo est **fantôme** — le backend ne le renvoie pas (pas dans UserDto). Sera toujours null. |
| `TypeClient` | ❌ absent | ✅ `TypeClient` string? | Absent de UserInfo — pas visible dans la vue détail. |
| `IsDeleted` | ❌ absent | ❌ absent | Ni UserInfo ni UserListItem n'exposent IsDeleted. |

---

### 3.3 Navigation actuelle

**FlyoutItems dans AppShell.xaml :**

| FlyoutItem | Route | Page | Condition IsVisible |
|------------|-------|------|---------------------|
| FlyoutDashboard | `dashboard` | DashboardPage | isDelegue |
| FlyoutVisites | `visits` | VisitListPage | isDelegue |
| FlyoutPlanning | `planning` | PlanningPage | isDelegue |
| *(sans nom)* | `products` | ProductListPage | Toujours visible |
| FlyoutOrders | `orders` | OrderListPage | isClient \|\| isDelegue |
| FlyoutDocuments | `documents` | DocumentListPage | isClient |
| *(sans nom)* | `profile` | ProfilePage | Toujours visible |
| FlyoutStock | `stock` | MyStockPage | isDelegue |
| **FlyoutClients** | **`clients`** | **MesClientsPage** | **isDelegue** ✅ |
| FlyoutObjectifs | `objectifs` | ObjectifPage | isDelegue |
| FlyoutReclamations | `reclamations` | ReclamationListPage | isClient |

**Note importante :** `isDelegue` dans AppShell est défini comme :
```csharp
bool isDelegue = role is "DELEGUE" or "ADMIN" or "SUPERVISEUR";
```
→ "Mes clients" est visible pour DELEGUE **et** pour ADMIN **et** pour SUPERVISEUR.

**Routes secondaires enregistrées :**
```csharp
Routing.RegisterRoute("clients/detail", typeof(ClientDetailPage));
Routing.RegisterRoute("clients/form",   typeof(ClientFormPage));
```
✅ Ces routes sont déjà enregistrées dans AppShell.xaml.cs.

**Existence des pages :**
- `MesClientsPage` : référencée dans AppShell → **doit exister** dans `Views/Clients/`.
- `ClientDetailPage` : référencée dans `Routing.RegisterRoute` → **doit exister**.
- `ClientFormPage` : référencée dans `Routing.RegisterRoute` → **doit exister**.

---

### 3.4 Ce qui manque dans MAUI

| Feature | Statut | Impact |
|---------|--------|--------|
| `GetUserByIdAsync` bloqué pour DELEGUE (403) | ❌ Bug critique | ClientDetailPage inutilisable |
| `UserListItem` sans email/téléphone/adresse | ⚠️ Données manquantes | Affichage liste clients incomplet |
| `UserInfo` sans `TypeClient` | ⚠️ Données manquantes | Impossible d'afficher PHARMACIEN/GROSSISTE dans détail |
| `UserInfo.RegionId` fantôme | ⚠️ Confusion | Le champ sera toujours null |
| `IsDeleted` absent des modèles MAUI | INFO | Impossible de savoir si client est désactivé |
| `CreateUserDto.Role` envoyé comme `int` | ⚠️ Fragilité | Si l'enum backend change d'ordre, bug silencieux |

---

## PARTIE 4 — GAPS ET DÉCISIONS REQUISES

---

### 4.1 Gap critique — `GET /auth/users/{id}` interdit au DELEGUE

**Constat :** `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` sur `GET /auth/users/{id}`.

Le DELEGUE en MAUI appelle `GetUserByIdAsync(clientId)` → 403. `ClientDetailPage` est brisée pour tout DELEGUE.

**Solutions :**

| Option | Effort | Recommandation |
|--------|--------|----------------|
| A) Ajouter DELEGUE à la liste des rôles autorisés sur l'endpoint, avec restriction : DELEGUE ne peut voir que les CLIENT | **SIMPLE (30 min)** | ✅ **RECOMMANDÉ** |
| B) Créer un endpoint dédié `GET /auth/clients/{id}` accessible DELEGUE | MOYEN | Duplication de code |
| C) Ne pas charger les détails client — afficher uniquement les données de la liste | SIMPLE | Perte de fonctionnalité |

**Correction recommandée pour Option A :**
```csharp
[HttpGet("users/{id}")]
[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
public async Task<IActionResult> GetUserById(int id)
{
    var user = await _authService.GetUserByIdAsync(id);
    if (user == null) { ... }

    // DELEGUE peut voir seulement les CLIENTs
    var callerRole = User.FindFirstValue(ClaimTypes.Role);
    if (callerRole == UserRole.DELEGUE.ToString() &&
        user.Role != UserRole.CLIENT.ToString())
    {
        _response.IsSuccess = false;
        _response.Message = "Accès refusé.";
        return Forbid();
    }
    ...
}
```

---

### 4.2 Gap — Filtrage région pour SUPERVISEUR

**Constat :** `Utilisateur` n'a pas `IdRegion`. Impossible de filtrer par région dans AuthAPI.

| Option | Effort | Recommandation PFE |
|--------|--------|--------------------|
| A) Ajouter `IdRegion` dans `Utilisateur` + migration EF | MOYEN (migration risquée) | Éviter en fin de PFE |
| B) Appel Angular cross-service : `RegionService.getByDelegue(supId)` → liste IDs délégués → filtrer | MOYEN | Possible si SUPERVISEUR a un IdUser |
| C) SUPERVISEUR voit DELEGUE+CLIENT sans filtre région | **SIMPLE** | ✅ **RECOMMANDÉ pour PFE** |

**Note :** L'implémentation actuelle (backend + Angular) utilise déjà l'Option C. C'est acceptable pour un PFE.

---

### 4.3 Gap — `UserListItem` trop minimal

**Constat :** `UserListItem` retourné par `GetUsersByRoleAsync` contient seulement `Id`, `Name`, `TypeClient`.
La page `MesClientsPage` ne peut pas afficher Téléphone, Adresse, Email sans ces données.

**Solutions :**

| Option | Effort | Recommandation |
|--------|--------|----------------|
| A) Enrichir `UserListItem` MAUI (Email, Telephone, Adresse) et s'assurer que le backend les renvoie | SIMPLE | ✅ **RECOMMANDÉ** — le backend `UserDto` contient déjà ces champs |
| B) Appeler `GetUserByIdAsync` pour chaque client de la liste | COMPLEXE | N appels réseau, lent, bloqué si 403 |

**Le backend renvoie déjà ces champs :** `UserDto` contient `Email`, `PhoneNumber`, `Adresse`, `Role`, `IsDeleted`, `TypeClient`. Il suffit d'enrichir `UserListItem` côté MAUI.

---

### 4.4 Gap — Historique visites par client

**Constat :** Il n'existe pas d'endpoint `GET /fields/visites/by-pharmacien/{id}` dans FieldAPI (d'après le plan d'implémentation).

**Solutions :**

| Option | Effort | Recommandation |
|--------|--------|--------------------|
| A) Sauter l'historique visites pour l'instant | SIMPLE | ✅ **RECOMMANDÉ** pour MVP |
| B) Filtrer côté MAUI depuis `GET /fields/visites/by-delegue/{delegueId}` where `IdPharmacien == clientId` | SIMPLE | Fonctionne mais charge toutes les visites du délégué |
| C) Créer un nouvel endpoint FieldAPI | MOYEN | Coût API supplémentaire |

---

### 4.5 Gap — `CreateUserDto.Role` envoyé comme int

**Constat :** MAUI envoie `Role = 4` (int). Le backend `RegistrationRequestDto.Role` est de type `UserRole` (enum). System.Text.Json désérialise correctement `4` → `UserRole.CLIENT` **seulement si `JsonSerializerOptions.Converters` n'inclut pas `JsonStringEnumConverter`**.

Si le backend active `JsonStringEnumConverter` (ce qui est courant en ASP.NET Core), il attendra `"CLIENT"` (string) et rejettera `4` (int).

**Recommandation SIMPLE :** Changer `CreateUserDto` pour envoyer des strings :
```csharp
[JsonPropertyName("role")]
public string Role { get; set; } = "CLIENT";  // "CLIENT", "DELEGUE", etc.
```

---

### 4.6 Recommandation finale par gap

| Gap | Complexité | Recommandation |
|-----|-----------|----------------|
| DELEGUE ne peut pas appeler `GET /auth/users/{id}` | **SIMPLE** | Ajouter DELEGUE à l'authorize + vérification rôle CLIENT |
| `UserListItem` trop minimal | **SIMPLE** | Ajouter Email, PhoneNumber, Adresse, Role dans la classe MAUI |
| `CreateUserDto.Role` envoyé comme int | **SIMPLE** | Changer vers string |
| `UserInfo.RegionId` fantôme | **SIMPLE** | Supprimer le champ ou le renommer en `null` documenté |
| `UserInfo` sans `TypeClient` | **SIMPLE** | Ajouter `TypeClient string?` dans `UserInfo` |
| Recherche SUPERVISEUR non filtrée | **SIMPLE** | Ajouter filtre rôle dans `SearchUsersAsync` |
| `getDisabledUsers()` Angular mauvaise URL | **SIMPLE** | Corriger `/auth/users/disabled` → `/auth/disabled-users` |
| Historique visites client | **SIMPLE** | Option B (filtrage côté MAUI) ou sauter |
| Filtrage région SUPERVISEUR | **COMPLEXE** | Garder Option C (sans filtre) pour PFE |
| Ajouter IdRegion dans Utilisateur | **COMPLEXE** | Éviter — migration DB risquée |

---

## PARTIE 5 — CODE COMPLET

---

### 5.1 AuthController.cs (complet)

```csharp
using CynapCRM.Services.AuthAPI.Models;
using CynapCRM.Services.AuthAPI.Models.Dto;
using CynapCRM.Services.AuthAPI.Service;
using CynapCRM.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace CynapCRM.Services.AuthAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        protected ResponseDto _response;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _env;
        private readonly TurnstileService _turnstileService;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, IEmailService emailService, IWebHostEnvironment env, TurnstileService turnstileService, IConfiguration configuration)
        {
            _authService = authService;
            _response = new();
            _emailService = emailService;
            _env = env;
            _turnstileService = turnstileService;
            _configuration = configuration;
        }
        [HttpPost("register")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")] 
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDto model)
        {
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            // SUPERVISEUR can only create DELEGUE and CLIENT
            if (currentUserRole == UserRole.SUPERVISEUR.ToString())
            {
                if (model.Role != UserRole.DELEGUE &&
                    model.Role != UserRole.CLIENT)
                {
                    _response.IsSuccess = false;
                    _response.Message = $"Vous n'êtes pas autorisé à créer un compte avec le rôle {model.Role}.";
                    return Forbid();
                }
            }

            // DELEGUE can only create CLIENT
            if (currentUserRole == UserRole.DELEGUE.ToString())
            {
                if (model.Role != UserRole.CLIENT)
                {
                    _response.IsSuccess = false;
                    _response.Message = $"Vous n'êtes pas autorisé à créer un compte avec le rôle {model.Role}.";
                    return Forbid();
                }
            }
            var result = await _authService.Register(model);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            _response.IsSuccess = true;
            _response.Message = "Utilisateur créé avec succès.";
            return Ok(_response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            var clientType = Request.Headers["X-Client-Type"];

            if (clientType != "mobile")
            {
                if (string.IsNullOrEmpty(model.TurnstileToken))
                {
                    return BadRequest("Captcha requis");
                }

                var isHuman = await _turnstileService.VerifyAsync(model.TurnstileToken);
                if (!isHuman)
                {
                    return BadRequest("Vérification échouée");
                }
            }

            var loginResponse = await _authService.Login(model);
            if (loginResponse.User == null )
            {
                _response.IsSuccess = false;
                _response.Message = "Identifiants incorrects ";
                return Unauthorized(_response);
            }
            if (loginResponse.User.IsDeleted)
            {
                _response.IsSuccess = false;
                _response.Message = "Compte désactivé.";
                return Forbid();
            }
            _response.Result = loginResponse;
            return Ok(_response);
        }

        [HttpGet("users/search")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> SearchUsers(
            [FromQuery] string keyword,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword) || keyword.Trim().Length < 3)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Le mot-clé doit contenir au moins 3 caractères.";
                    return BadRequest(_response);
                }

                var users = await _authService.SearchUsersAsync(keyword.Trim(), isActive);
                _response.IsSuccess = true;
                _response.Result = users.ToList();
                _response.Message = $"{users.Count()} utilisateur(s) trouvé(s).";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Erreur de recherche : {ex.Message}";
                return StatusCode(515, _response);
            }
        }

        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données invalides.";
                    return BadRequest(_response);
                }

                var result = await _authService.UpdateProfileAsync(model);

                if (!result.IsSuccess)
                {
                    _response.IsSuccess = false;
                    _response.Message = result.Message;
                    return BadRequest(_response);
                }

                _response.IsSuccess = true;
                _response.Message = result.Message;
                _response.Result = result.Result;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpGet("users")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _authService.GetAllUsersAsync();
                if (users == null || !users.Any())
                {
                    _response.IsSuccess = true;
                    _response.Result = new List<UserDto>();
                    _response.Message = "Aucun utilisateur trouvé.";
                    return Ok(_response);
                }

                // SUPERVISEUR sees only DELEGUE + CLIENT users
                var callerRole = User.FindFirstValue(ClaimTypes.Role);
                var userList = users.ToList();
                if (callerRole == UserRole.SUPERVISEUR.ToString())
                {
                    userList = userList.Where(u =>
                        u.Role == UserRole.DELEGUE.ToString() ||
                        u.Role == UserRole.CLIENT.ToString()
                    ).ToList();
                }

                _response.IsSuccess = true;
                _response.Result = userList;
                _response.Message = "Liste de tous les utilisateurs.";

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Server error: {ex.Message}";
                return StatusCode(515, _response);
            }
        }

        [HttpPost("AssignRole")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var assignRoleSuccessful = await _authService.AssignRole(model.UserId, model.Role);
            if (!assignRoleSuccessful)
            {
                _response.IsSuccess = false;
                _response.Message = "Erreur lors de l'attribution du rôle";
                return BadRequest(_response);
            }
            _response.IsSuccess = true;
            _response.Message = "Rôle attribué avec succès";
            return Ok(_response);
        }

        [HttpPut("add-role")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> AddRole([FromBody] RegistrationRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.AddRole(model.Email, model.Role);
            if (!result)
            {
                _response.IsSuccess = false;
                _response.Message = "Échec de l'ajout du rôle.";
                return NotFound(_response); 
            }

            _response.IsSuccess = true;
            _response.Message = $"Rôle {model.Role} ajouté avec succès à l'utilisateur {model.Email}.";
            return Ok(_response); 
        }

        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var emailFromToken = User.FindFirstValue(ClaimTypes.Email);

                if (string.IsNullOrWhiteSpace(emailFromToken))
                {
                    _response.IsSuccess = false;
                    _response.Message = "Utilisateur non authentifié.";
                    return Unauthorized(_response);
                }

                if (!string.Equals(emailFromToken, model.Email, StringComparison.OrdinalIgnoreCase))
                {
                    _response.IsSuccess = false;
                    _response.Message = "Vous ne pouvez changer que votre propre mot de passe.";
                    return Forbid();
                }

                var result = await _authService.ChangePassword(model);

                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Mot de passe actuel incorrect.";
                    return BadRequest(_response);
                }

                _response.IsSuccess = true;
                _response.Message = "Mot de passe changé avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Erreur lors du changement de mot de passe : {ex.Message}";
                return StatusCode(515, _response);
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var response = await _authService.GeneratePasswordResetToken(model.Email);

            if (!response.IsSuccess) return NotFound(response);
            var token = response.Result.ToString();
            var encodedToken = Uri.EscapeDataString(token);
            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:4200";
            string resetLink = $"{frontendUrl}/reset-password?email={Uri.EscapeDataString(model.Email)}&token={encodedToken}";
            string subject = "Réinitialisation de mot de passe - CynapCRM";
            string message = $@"<div style='...'><a href='{resetLink}'>Réinitialiser</a></div>";
            await _emailService.SendEmailAsync(model.Email, subject, message);
            response.Message = "Un e-mail de réinitialisation a été envoyé.";
            response.Result = null;
            return Ok(response);
        }

        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authService.ResetPassword(model);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("change-role")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> ChangeRole([FromBody] ChangeRoleDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authService.ChangeRole(model);
            if (result == null)
            {
                _response.IsSuccess = false;
                _response.Message = "Échec du changement de rôle.";
                return NotFound(_response);
            }
            _response.IsSuccess = true;
            _response.Message = "Rôle changé avec succès.";
            return Ok(_response);
        }

        [HttpPut("enable-user/{email}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> EnableUser(string email)
        {
            var result = await _authService.EnableUser(email);
            if (!result)
            {
                _response.IsSuccess = false;
                _response.Message = "Échec de la réactivation.";
                return NotFound(_response);
            }
            _response.IsSuccess = true;
            _response.Message = "Utilisateur réactivé.";
            return Ok(_response);
        }

        [HttpPut("delete-user/{email}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteUser(string email)
        {
            var result = await _authService.DisableUser(email);
            if (!result)
            {
                _response.IsSuccess = false;
                _response.Message = "Échec de la suppression.";
                return NotFound(_response);
            }
            _response.IsSuccess = true;
            _response.Message = "Utilisateur supprimé.";
            return Ok(_response);
        }

        [HttpGet("users/by-role/{role}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetUsersByRole(string role)
        {
            var callerRole = User.FindFirstValue(ClaimTypes.Role);
            if (callerRole == UserRole.DELEGUE.ToString() &&
                !string.Equals(role, UserRole.CLIENT.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                _response.IsSuccess = false;
                _response.Message = "Accès refusé. Vous ne pouvez consulter que les clients.";
                return Forbid();
            }
            if (callerRole == UserRole.SUPERVISEUR.ToString() &&
                !string.Equals(role, UserRole.DELEGUE.ToString(), StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(role, UserRole.CLIENT.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                _response.IsSuccess = false;
                _response.Message = "Accès refusé.";
                return Forbid();
            }

            var result = await _authService.GetUsersByRoleAsync(role);
            return Ok(new ResponseDto { IsSuccess = true, Result = result });
        }

        [HttpGet("users/{id}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _authService.GetUserByIdAsync(id);
                if (user == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Utilisateur non trouvé.";
                    return NotFound(_response);
                }
                _response.IsSuccess = true;
                _response.Result = user;
                _response.Message = "Détails de l'utilisateur récupérés.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Erreur: {ex.Message}";
                return StatusCode(515, _response);
            }
        }

        [HttpGet("disabled-users")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetDisabledUsers()
        {
            try
            {
                var users = await _authService.GetDisabledUsersAsync();
                if (users == null || !users.Any())
                {
                    _response.IsSuccess = true;
                    _response.Result = new List<UserDto>();
                    _response.Message = "Aucun utilisateur désactivé trouvé.";
                    return Ok(_response);
                }
                _response.IsSuccess = true;
                _response.Result = users;
                _response.Message = "Liste des utilisateurs désactivés.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "Erreur lors de la récupération des utilisateurs désactivés.";
                return StatusCode(515, _response);
            }
        }
    }
}
```

---

### 5.2 Utilisateur.cs (ApplicationUser)

```csharp
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.AuthAPI.Models
{
    public class Utilisateur : IdentityUser<int>
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Adresse { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;
    }
}
```

---

### 5.3 RegistrationRequestDto.cs

```csharp
namespace CynapCRM.Services.AuthAPI.Models.Dto
{
    public class RegistrationRequestDto
    {
        public string Email         { get; set; } = string.Empty;
        public string Name          { get; set; } = string.Empty;
        public string PhoneNumber   { get; set; } = string.Empty;
        public string Password      { get; set; } = string.Empty;
        public string Adresse       { get; set; } = string.Empty;
        public string? NomOfficine  { get; set; }
        public string? TypePharmacie{ get; set; }
        public string? RaisonSociale{ get; set; }
        public UserRole Role        { get; set; }
        public UserType UserType    { get; set; }
    }
}
```

---

### 5.4 UpdateProfileDto.cs (backend)

```csharp
using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.AuthAPI.Models.Dto
{
    public class UpdateProfileDto
    {
        [Required]
        public string Email       { get; set; } = string.Empty;
        public string? Name       { get; set; }
        public string? PhoneNumber{ get; set; }
        public string? Adresse    { get; set; }
    }
}
```

---

### 5.5 UserDto.cs (backend)

```csharp
namespace CynapCRM.Services.AuthAPI.Models.Dto
{
    public class UserDto
    {
        public int    Id          { get; set; }
        public string Name        { get; set; } = string.Empty;
        public string Email       { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Adresse     { get; set; } = string.Empty;
        public string Role        { get; set; } = string.Empty;
        public bool   IsDeleted   { get; set; } = false;
        public string? TypeClient { get; set; }
    }
}
```

---

### 5.6 LoginRequestDto.cs

```csharp
namespace CynapCRM.Services.AuthAPI.Models.Dto
{
    public class LoginRequestDto
    {
        public string UserName      { get; set; } = string.Empty;
        public string Password      { get; set; } = string.Empty;
        public string? TurnstileToken { get; set; }
    }
}
```

---

### 5.7 UserRole.cs (enum)

```csharp
namespace CynapCRM.Services.AuthAPI.Models
{
    public enum UserRole
    {
        ADMIN,       // 0
        SUPERVISEUR, // 1
        DELEGUE,     // 2
        MEDECIN,     // 3
        CLIENT       // 4
    }
}
```

---

### 5.8 Region.cs (FieldAPI)

```csharp
using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.FieldAPI.Models
{
    public class Region
    {
        [Key]
        public int    Id_Region      { get; set; }
        public string NomRegion      { get; set; } = string.Empty;
        public string CodePostal     { get; set; } = string.Empty;
        [Required]
        public int    Id_User_Delegue { get; set; }
        // Pas de Id_User_Superviseur
        // Pas de navigation property Delegues
    }
}
```

---

### 5.9 AuthService.cs (méthodes clés)

Voir sections `Register`, `GetAllUsersAsync`, `GetUsersByRoleAsync`, `GetUserByIdAsync`, `UpdateProfileAsync`, `GetTypeClient` dans le fichier complet `CynapCRM.Services.AuthAPI/Service/AuthService.cs`.

Points clés :
- `Register` : crée `Pharmacien` ou `Grossiste` si `Role == CLIENT`, sinon `Utilisateur` de base.
- `GetTypeClient(user)` : retourne `"PHARMACIEN"`, `"GROSSISTE"` ou `null` selon le type runtime de l'objet.
- `UpdateProfileAsync` : lookup par EMAIL (`FindByEmailAsync`), met à jour Name/PhoneNumber/Adresse.
- `GetUserByIdAsync` : retourne `null` si `user.IsDeleted == true`.
- `GetUsersByRoleAsync` : inclut les utilisateurs `IsDeleted = true` dans les résultats (pas de filtre actifs).

⚠️ **`GetUsersByRoleAsync` n'exclut pas les utilisateurs désactivés** — la liste de clients MAUI peut inclure des comptes désactivés.

---

### 5.10 UserService (Angular) — complet

```typescript
import { Injectable } from '@angular/core';
import { ApiService } from '../../core/services/api.service';
import { forkJoin, Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class UserService {
  private baseUrl = '/auth';
  constructor(private apiService: ApiService) {}

  getUsers():                          Observable<any[]>   { return this.apiService.get<any[]>(`${this.baseUrl}/users`); }
  getAllUsers():                        Observable<any>     { return this.apiService.get<any>(`${this.baseUrl}/users`); }
  getUsersByRole(role: string):        Observable<any[]>   { return this.apiService.get<any>(`${this.baseUrl}/users/by-role/${encodeURIComponent(role)}`).pipe(map(r => r?.Result ?? r?.result ?? r ?? [])); }
  getUserById(id: number):             Observable<any>     { return this.apiService.get<any>(`${this.baseUrl}/users/${id}`); }
  registerUser(payload: any):          Observable<any>     { return this.apiService.post<any>(`${this.baseUrl}/register`, payload); }
  changeRole(payload: any):            Observable<any>     { return this.apiService.put<any>(`${this.baseUrl}/change-role`, payload); }
  disableUser(email: string):          Observable<any>     { return this.apiService.put<any>(`${this.baseUrl}/delete-user/${encodeURIComponent(email)}`, {}); }
  enableUser(email: string):           Observable<any>     { return this.apiService.put<any>(`${this.baseUrl}/enable-user/${encodeURIComponent(email)}`, {}); }
  searchUsers(keyword: string, isActive?: boolean): Observable<any> {
    let url = `${this.baseUrl}/users/search?keyword=${encodeURIComponent(keyword)}`;
    if (isActive !== undefined) url += `&isActive=${isActive}`;
    return this.apiService.get<any>(url);
  }
  getDisabledUsers(): Observable<any> {
    // ⚠️ BUG: URL incorrecte — backend expose /auth/disabled-users pas /auth/users/disabled
    return this.apiService.get<any>(`${this.baseUrl}/users/disabled`);
  }
  // ... helpers: unwrapList, unwrapUser, userId, displayName, getDisplayNamesByIds
}
```

---

### 5.11 UserListComponent.ts (complet)

Voir fichier `Cynapharm/src/app/features/users/user-list/user-list.component.ts`.

Points clés résumés :
- `isAdmin` : role === 'ADMIN'
- `isSuperviseur` : role === 'SUPERVISEUR'
- `canCreate` / `canEdit` : isAdmin || isSuperviseur
- `ROLES` getter filtré par rôle
- `loadUsers()` : branche SUPERVISEUR → `forkJoin` DELEGUE+CLIENT
- Boutons disable/enable : `*ngIf="isAdmin"`
- Bouton edit : `*ngIf="canEdit"`
- Bouton create : `*ngIf="canCreate"`

---

### 5.12 UserListComponent.html (complet)

Voir fichier `Cynapharm/src/app/features/users/user-list/user-list.component.html`.

Points clés : table avec colonnes Utilisateur/Email/Téléphone/Rôle/Statut/Actions. Modale de confirmation pour disable/enable.

---

### 5.13 UserFormComponent.ts (complet)

Voir fichier `Cynapharm/src/app/features/users/user-form/user-form.component.ts`.

Points clés :
- `availableRoles` getter filtré par rôle appelant
- Edit mode → `changeRole()` seulement
- Create mode → `registerUser()` avec tous les champs
- SUPERVISEUR edit → champ rôle readonly

---

### 5.14 UserFormComponent.html (complet)

Voir fichier `Cynapharm/src/app/features/users/user-form/user-form.component.html`.

Points clés : sections Identité / Coordonnées (création seulement) / Rôle&Accès / Sécurité (création seulement). Note info en édition.

---

### 5.15 UserService.cs (MAUI — complet)

```csharp
using Cynapharm_Mobile.Models.Auth;

namespace Cynapharm_Mobile.Services;

public class UserService
{
    private readonly ApiService _api;
    public UserService(ApiService api) { _api = api; }

    public Task<List<UserListItem>?> GetUsersByRoleAsync(string role)
        => _api.GetAsync<List<UserListItem>>($"auth/users/by-role/{Uri.EscapeDataString(role)}");

    // ⚠️ BLOQUÉ pour DELEGUE — endpoint [Authorize(Roles = "ADMIN,SUPERVISEUR")]
    public Task<UserInfo?> GetUserByIdAsync(int id)
        => _api.GetAsync<UserInfo>($"auth/users/{id}");

    public Task<object?> CreateUserAsync(CreateUserDto dto)
        => _api.PostAsync<object>("auth/register", dto);

    public Task<object?> UpdateUserAsync(UpdateUserDto dto)
        => _api.PutAsync<object>("auth/update-profile", dto);
}
```

---

### 5.16 UserInfo.cs (MAUI)

```csharp
using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Auth;

public class UserInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? RegionId { get; set; }                      // ⚠️ fantôme — pas dans UserDto backend
    [JsonPropertyName("phoneNumber")]
    public string? Telephone { get; set; }
    public string? Adresse { get; set; }
    // ⚠️ TypeClient absent — pas visible dans la vue détail
    // ⚠️ IsDeleted absent
}
```

---

### 5.17 UserListItem.cs (MAUI)

```csharp
using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Auth;

public class UserListItem
{
    public int    Id   { get; set; }
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("typeClient")]
    public string? TypeClient { get; set; }
    // ⚠️ Manque Email, PhoneNumber, Adresse, Role — backend UserDto les contient
}
```

---

### 5.18 CreateUserDto.cs (MAUI)

```csharp
using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Auth;

public class CreateUserDto
{
    [JsonPropertyName("email")]       public string  Email       { get; set; } = string.Empty;
    [JsonPropertyName("name")]        public string  Name        { get; set; } = string.Empty;
    [JsonPropertyName("password")]    public string  Password    { get; set; } = string.Empty;
    [JsonPropertyName("phoneNumber")] public string? PhoneNumber { get; set; }
    [JsonPropertyName("adresse")]     public string? Adresse     { get; set; }
    [JsonPropertyName("role")]        public int     Role        { get; set; }     // CLIENT = 4 (⚠️ int, non string)
    [JsonPropertyName("userType")]    public int     UserType    { get; set; }     // PHARMACIEN = 0, GROSSISTE = 1
}
```

---

### 5.19 UpdateUserDto.cs (MAUI)

```csharp
using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Auth;

public class UpdateUserDto
{
    [JsonPropertyName("email")]       public string  Email       { get; set; } = string.Empty;
    [JsonPropertyName("name")]        public string? Name        { get; set; }
    [JsonPropertyName("phoneNumber")] public string? PhoneNumber { get; set; }
    [JsonPropertyName("adresse")]     public string? Adresse     { get; set; }
}
```

---

### 5.20 AppShell.xaml — FlyoutItems et routes (extrait)

```xml
<!-- FlyoutItems (tous FlyoutItemIsVisible="False" — navigation custom) -->
<FlyoutItem x:Name="FlyoutDashboard"  FlyoutItemIsVisible="False" IsVisible="False">
    <ShellContent Route="dashboard"   ContentTemplate="{DataTemplate dashboard:DashboardPage}" ... />
</FlyoutItem>
<FlyoutItem x:Name="FlyoutVisites"    FlyoutItemIsVisible="False" IsVisible="False">
    <ShellContent Route="visits"      ContentTemplate="{DataTemplate visites:VisitListPage}" ... />
</FlyoutItem>
<FlyoutItem x:Name="FlyoutPlanning"   FlyoutItemIsVisible="False" IsVisible="False">
    <ShellContent Route="planning"    ContentTemplate="{DataTemplate planning:PlanningPage}" ... />
</FlyoutItem>
<FlyoutItem FlyoutItemIsVisible="False">
    <ShellContent Route="products"    ContentTemplate="{DataTemplate products:ProductListPage}" ... />
</FlyoutItem>
<FlyoutItem x:Name="FlyoutOrders"     FlyoutItemIsVisible="False" IsVisible="False">
    <ShellContent Route="orders"      ContentTemplate="{DataTemplate orders:OrderListPage}" ... />
</FlyoutItem>
<FlyoutItem x:Name="FlyoutDocuments"  FlyoutItemIsVisible="False" IsVisible="False">
    <ShellContent Route="documents"   ContentTemplate="{DataTemplate documents:DocumentListPage}" ... />
</FlyoutItem>
<FlyoutItem FlyoutItemIsVisible="False">
    <ShellContent Route="profile"     ContentTemplate="{DataTemplate profile:ProfilePage}" ... />
</FlyoutItem>
<FlyoutItem Title="Mon Stock"   x:Name="FlyoutStock"        FlyoutItemIsVisible="False">
    <ShellContent Route="stock"       ContentTemplate="{DataTemplate stock:MyStockPage}" ... />
</FlyoutItem>
<FlyoutItem Title="Mes clients" x:Name="FlyoutClients"      FlyoutItemIsVisible="False" IsVisible="False">
    <ShellContent Route="clients"     ContentTemplate="{DataTemplate clients:MesClientsPage}" ... />
</FlyoutItem>
<FlyoutItem Title="Objectifs"   x:Name="FlyoutObjectifs"    FlyoutItemIsVisible="False">
    <ShellContent Route="objectifs"   ContentTemplate="{DataTemplate objectifs:ObjectifPage}" ... />
</FlyoutItem>
<FlyoutItem Title="Réclamations" x:Name="FlyoutReclamations" FlyoutItemIsVisible="False">
    <ShellContent Route="reclamations" ContentTemplate="{DataTemplate reclamations:ReclamationListPage}" ... />
</FlyoutItem>
```

---

### 5.21 AppShell.xaml.cs — ApplyRoleVisibility() (complet)

```csharp
public void ApplyRoleVisibility(string role)
{
    Role = role;

    // SUPERVISEUR role is deprecated in UI. Treated as DELEGUE for navigation.
    bool isDelegue = role is "DELEGUE" or "ADMIN" or "SUPERVISEUR";
    bool isClient  = role is "PHARMACIEN" or "GROSSISTE" or "CLIENT";
    bool isMedecin = role is "MEDECIN";

    ShowDashboard    = isDelegue;
    ShowVisites      = isDelegue;
    ShowPlanning     = isDelegue;
    ShowCatalogue    = isDelegue || isClient || isMedecin;
    ShowOrders       = isClient || isDelegue;
    ShowDocuments    = isClient;
    ShowReclamations = isClient;
    ShowStock        = isDelegue;
    ShowObjectifs    = isDelegue;
    ShowClients      = isDelegue;

    _ = LoadUserInfoAsync();
    NotifyAll();

    Shell.SetFlyoutBehavior(this, isMedecin ? FlyoutBehavior.Disabled : FlyoutBehavior.Flyout);

    if (FlyoutDashboard  is not null) FlyoutDashboard.IsVisible  = isDelegue;
    if (FlyoutVisites    is not null) FlyoutVisites.IsVisible    = isDelegue;
    if (FlyoutPlanning   is not null) FlyoutPlanning.IsVisible   = isDelegue;
    if (FlyoutOrders     is not null) FlyoutOrders.IsVisible     = isClient || isDelegue;
    if (FlyoutDocuments  is not null) FlyoutDocuments.IsVisible  = isClient;
    if (FlyoutStock        is not null) FlyoutStock.IsVisible        = isDelegue;
    if (FlyoutObjectifs    is not null) FlyoutObjectifs.IsVisible    = isDelegue;
    if (FlyoutReclamations is not null) FlyoutReclamations.IsVisible = isClient;
    if (FlyoutClients      is not null) FlyoutClients.IsVisible      = isDelegue;
}
```

---

### 5.22 RegionService.ts (Angular)

```typescript
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../../core/services/api.service';

export interface RegionDto {
  id_Region?:      number;
  nomRegion:       string;
  codePostal:      string;
  id_User_Delegue: number;
}

@Injectable({ providedIn: 'root' })
export class RegionService {
  private readonly base = '/fields/regions';
  constructor(private api: ApiService) {}
  private u<T>(r: any): T { return r?.Result ?? r?.result ?? r; }

  getAll()                { return this.api.get<any>(`${this.base}/all`).pipe(map(r => this.u<RegionDto[]>(r) ?? [])); }
  getById(id: number)     { return this.api.get<any>(`${this.base}/${id}`).pipe(map(r => this.u<RegionDto>(r))); }
  getByDelegue(id: number){ return this.api.get<any>(`${this.base}/by-delegue/${id}`).pipe(map(r => this.u<RegionDto[]>(r) ?? [])); }
  getCount(id: number): Observable<number> { return this.api.get<any>(`${this.base}/count/${id}`).pipe(map(r => this.u<number>(r) ?? 0)); }
  createOrUpdate(dto: RegionDto): Observable<RegionDto> { return this.api.post<any>(this.base, dto).pipe(map(r => this.u<RegionDto>(r))); }
  delete(id: number): Observable<void> { return this.api.delete<void>(`${this.base}/${id}`); }
}
```

---

### 5.23 AuthService.ts — getUserRole() et getUserId() (extrait)

```typescript
export enum UserRole {
  ADMIN = 'ADMIN', SUPERVISEUR = 'SUPERVISEUR', DELEGUE = 'DELEGUE',
  MEDECIN = 'MEDECIN', CLIENT = 'CLIENT'
}
export enum UserType { PHARMACIEN = 'PHARMACIEN', GROSSISTE = 'GROSSISTE' }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private currentUserSignal = signal<User | null>(null);

  getUserRole(): UserRole | null {
    return this.currentUserSignal()?.role ?? null;
  }

  getUserId(): number {
    return this.currentUserSignal()?.id ?? 0;
  }

  hasRole(roles: UserRole[]): boolean {
    const role = this.getUserRole();
    return role ? roles.includes(role) : false;
  }
}
```

---

*Fin de l'analyse — 2026-05-29*
