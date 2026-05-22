# CYNAPHARM — Scénarios complets tous rôles

> **Généré le :** 2026-05-22
> **Sources lues :** 48 fichiers (AuthAPI · ProductAPI · FieldAPI · Angular · MAUI)
> **Gateway prod :** `http://cynapharmgateway.runasp.net`
> **AuthAPI downstream :** `cynapharmauth.runasp.net:80`
> **ProductAPI downstream :** `cynapharmproducts.runasp.net` (prefix `/products`)
> **FieldAPI downstream :** `cynapharmfields.runasp.net:80`

---

# SCENARIO 1 — Authentification complète

## PARTIE 1 — Login

### 1.1 Flow Angular (`login.component.ts` + `auth.service.ts`)

**Formulaire :**
```typescript
this.loginForm = this.fb.group({
  email:    ['', [Validators.required, Validators.email]],
  password: ['', [Validators.required, Validators.minLength(6)]]
});
```

**Étapes :**
1. Utilisateur remplit `email` + `password`.
2. Le composant vérifie que le formulaire est valide ET que `isTurnstileValid = true`.
3. Si le Turnstile n'est pas valide : `error = 'Veuillez compléter la vérification de sécurité.'` → blocage.
4. `authService.login(email, password, turnstileToken)` est appelé.
5. HTTP POST envoyé :
   ```
   POST http://cynapharmgateway.runasp.net/auth/login
   Body: { UserName: email, password, turnstileToken }
   ```
6. Sur succès : `localStorage.setItem('token', result.token)` + `localStorage.setItem('user', JSON.stringify(result.user))`
7. `currentUserSignal.set(result.user)` — reactive signal mis à jour.
8. Redirection selon rôle :

| Rôle | Redirection Angular |
|------|-------------------|
| `ADMIN` | `/dashboard` |
| `SUPERVISEUR` | `/dashboard` |
| `DELEGUE` | `/dashboard` |
| `MEDECIN` | `/products` |
| `CLIENT` | `/orders` |

> ⚠️ Si `returnUrl` est présent dans les query params, il prend priorité sur la redirection par rôle.

---

### 1.2 Flow MAUI (`LoginViewModel.cs` + `AuthService.cs`)

**Formulaire :**
- `Email` (ObservableProperty)
- `Password` (ObservableProperty, masqué par `IsPasswordHidden`)
- **Aucun Turnstile** — le header `X-Client-Type: mobile` est envoyé automatiquement par l'`ApiService`.

**Étapes :**
1. Validation locale : `Email` et `Password` non vides → sinon `ErrorMessage = "Veuillez renseigner..."`.
2. `CheckConnectivityAsync()` — si hors ligne : erreur.
3. `_authService.LoginAsync(new LoginRequest(Email, Password))` appelé.
4. HTTP POST :
   ```
   POST /auth/login
   Headers: X-Client-Type: mobile
   Body: { UserName: email, Password: password }
   ```
5. Backend détecte `X-Client-Type: mobile` → **Turnstile bypassed** entièrement.
6. Sur succès, stockage dans `SecureStorage` :

| Clé | Valeur |
|-----|--------|
| `JwtToken` | JWT string |
| `TokenExpiry` | `jwt.ValidTo.ToString("O")` (parsé depuis le JWT) |
| `UserRole` | `result.User.Role` |
| `UserId` | `result.User.Id.ToString()` |
| `UserName` | `result.User.Name` |
| `UserEmail` | `result.User.Email` |
| `UserTelephone_{userId}` | `result.User.Telephone` |
| `UserAdresse_{userId}` | `result.User.Adresse` |

7. `shell.ApplyRoleVisibility(role)` — configure la navigation.
8. Redirection :

| Rôle | Route MAUI |
|------|-----------|
| `DELEGUE` / `ADMIN` / `SUPERVISEUR` | `//dashboard` |
| `PHARMACIEN` / `GROSSISTE` / `CLIENT` | `//orders` |
| `MEDECIN` | `//products` |
| Défaut | `//orders` |

---

### 1.3 Turnstile CAPTCHA

**Backend (`AuthController.Login`) :**
```csharp
var clientType = Request.Headers["X-Client-Type"];
if (clientType != "mobile")
{
    if (string.IsNullOrEmpty(model.TurnstileToken))
        return BadRequest("Captcha requis");

    var isHuman = await _turnstileService.VerifyAsync(model.TurnstileToken);
    if (!isHuman)
        return BadRequest("Vérification échouée");
}
```

- **Web (Angular)** : Turnstile obligatoire. Token validé via Cloudflare API.
- **Mobile (MAUI)** : header `X-Client-Type: mobile` → Turnstile entièrement sauté.
- Clé Turnstile : `0x4AAAAAADT3F3_rsbtN4Xf4fAazy1R3GfU` (dans `appsettings.json`).

---

### 1.4 JWT Token — Claims stockées

**Générateur (`JwtTokenGenerator.cs`) :**
```csharp
var claims = new List<Claim>
{
    new Claim(JwtRegisteredClaimNames.Sub,         user.Id.ToString()),
    new Claim(ClaimTypes.NameIdentifier,           user.Id.ToString()),
    new Claim(JwtRegisteredClaimNames.Email,       user.Email),
    new Claim(ClaimTypes.Email,                    user.Email),
    new Claim(JwtRegisteredClaimNames.Jti,         Guid.NewGuid().ToString()),
    new Claim(JwtRegisteredClaimNames.Iat,         DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
};
// + ClaimTypes.Role pour chaque rôle
```

**Configuration JWT (`appsettings.json`) :**
```json
{
  "JwtOptions": {
    "Secret":        "9DbAfyhzmWNd6NFE0SSXmqTKGIEz2vg1MhOF6CQzLksNphwlnOW3XQrdPmFaPsj4",
    "Issuer":        "Cynap-AuthAPI",
    "Audience":      "Cynap-Customer",
    "ExpiryMinutes": 3600
  }
}
```

> 🔴 **ExpiryMinutes = 3600** → **60 heures = 2,5 jours** ! Ce token est extrêmement long-lived. Une valeur de 60 minutes serait standard.

**Claims accessibles dans les controllers backend :**
```csharp
User.FindFirst(ClaimTypes.NameIdentifier)  // userId (int)
User.FindFirstValue(ClaimTypes.Role)       // rôle uppercase
User.FindFirstValue(ClaimTypes.Email)      // email
```

---

### 1.5 Gestion expiration token

**Angular :**
```typescript
isAuthenticated(): boolean {
  return !!this.getToken();  // vérifie SEULEMENT la présence, PAS l'expiration!
}
```
> 🔴 **Bug :** Angular ne vérifie pas si le JWT est expiré. Un token expiré reste "valide" côté client jusqu'au prochain appel API qui retourne 401.

**MAUI :**
```csharp
public async Task<bool> IsAuthenticatedAsync()
{
    var token = await SecureStorage.GetAsync(StorageKeys.JwtToken);
    if (string.IsNullOrEmpty(token)) return false;
    var handler = new JwtSecurityTokenHandler();
    var jwt = handler.ReadJwtToken(token);
    return jwt.ValidTo > DateTime.UtcNow;  // ✅ vérifie bien l'expiration
}
```
MAUI vérifie correctement. `IsTokenExpiringSoonAsync(threshold)` permet aussi une alerte préventive.

**Quand le token expire :**
- Angular : prochain appel API → 401 → aucun interceptor de refresh trouvé → l'utilisateur reste "connecté" localement mais toutes les requêtes échouent.
- MAUI : `IsAuthenticatedAsync()` retourne false → redirect vers login (selon le `TokenValidationHandler`).

> ⚠️ Aucun mécanisme de refresh token n'existe dans ce projet. Après 60 heures, l'utilisateur doit se reconnecter.

---

## PARTIE 2 — Register (Création de compte)

**Qui peut créer un compte (`[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]`) :**

| Rôle créateur | Rôles créables |
|--------------|----------------|
| `ADMIN` | Tous (ADMIN, SUPERVISEUR, DELEGUE, MEDECIN, CLIENT) |
| `SUPERVISEUR` | DELEGUE, MEDECIN, CLIENT |
| `DELEGUE` | MEDECIN, CLIENT uniquement |

**Endpoint :**
```
POST /auth/register
Authorization: Bearer {token}
Body (RegistrationRequestDto):
{
  "Email":          "user@example.com",
  "Name":           "Nom Prénom",
  "PhoneNumber":    "+21612345678",
  "Password":       "motdepasse123",
  "Adresse":        "Tunis, Tunisie",
  "Role":           "DELEGUE",          // enum UserRole
  "UserType":       "PHARMACIEN",       // enum UserType (pour CLIENT)
  "NomOfficine":    "Pharmacie X",      // si PHARMACIEN
  "TypePharmacie":  "Officine",         // si PHARMACIEN
  "RaisonSociale":  "SARL Y"            // si GROSSISTE
}
```

**Validations backend :**
1. Email déjà utilisé → 400 `"Un compte avec cet email existe déjà."`
2. Si CLIENT + UserType = PHARMACIEN → crée un `Pharmacien` (sous-classe d'`Utilisateur`)
3. Si CLIENT + UserType = GROSSISTE → crée un `Grossiste`
4. Rôle créé en base si inexistant
5. `_userManager.CreateAsync(user, password)` — ASP.NET Identity valide le mot de passe

**Réponse succès (200 OK) :**
```json
{ "isSuccess": true, "message": "Utilisateur créé avec succès." }
```
> ⚠️ **Aucun JWT retourné.** L'utilisateur doit se connecter séparément après création.

**Après inscription :** Aucune validation d'email n'est implémentée. Le compte est immédiatement actif.

---

## PARTIE 3 — Forgot Password / Reset Password

### 3.1 Forgot Password

**Étapes :**
1. Utilisateur saisit son email sur `/forgot-password`.
2. `authService.forgotPassword(email)` appelé.
3. `POST /auth/forgot-password` — `{ Email: email }` (header `Email` en PascalCase).
4. Backend :
   - Cherche l'utilisateur par email normalisé.
   - Si non trouvé ou `IsDeleted` → 404 + `"Aucun compte n'est associé à cet e-mail."`
   - Génère token : `_userManager.GeneratePasswordResetTokenAsync(user)`
   - URL-encode le token
   - Construit le lien :
     ```
     http://localhost:4200/reset-password?email={email}&token={encoded_token}
     ```
   - Envoie l'email via SMTP Gmail (port 587)
5. Réponse : `200 OK` + `"Un e-mail de réinitialisation a été envoyé."`

> 🔴 **Bug critique :** Le lien est hardcodé sur `http://localhost:4200` → ne fonctionne pas en production!

### 3.2 Reset Password

**Étapes :**
1. Utilisateur clique sur le lien → Angular lit `email` et `token` depuis `queryParams`.
2. Si l'un ou l'autre est absent → `error = 'Lien de réinitialisation invalide.'`
3. Formulaire : `newPassword` (min 6) + `confirmPassword` avec `passwordMatchValidator`.
4. `authService.resetPassword(email, token, newPassword)` appelé.
5. `PUT /auth/reset-password` — `{ Email, Token, NewPassword }`.
6. Backend :
   - User introuvable → 400
   - Nouveau mot de passe = ancien → 400 `"Le nouveau mot de passe doit être différent..."`
   - `_userManager.ResetPasswordAsync(user, token, newPassword)` — token validé ici
   - Si token invalide/expiré → 400 `"Lien de réinitialisation invalide ou expiré."`

**Token reset (ASP.NET Identity) :**
- Généré par Identity DataProtectorTokenProvider
- Expiration par défaut : **1 journée** (configurable dans `AddIdentity`)
- Non stocké en base — inclus dans l'URL encodé en base64
- Usage unique : invalidé après utilisation

---

## PARTIE 4 — Business Logic Verification

| Étape | Attendu | Réel | Statut |
|-------|---------|------|--------|
| Angular vérifie expiration JWT | Oui | NON — présence seulement | ❌ BUG |
| MAUI vérifie expiration JWT | Oui | OUI — `jwt.ValidTo > DateTime.UtcNow` | ✅ OK |
| Turnstile bypassable mobile | Oui (par design) | OUI — header `X-Client-Type: mobile` | ✅ OK |
| Token expiré après login | Après N minutes | 3600 min = 60h (trop long) | ⚠️ TROP LONG |
| Lien reset password production | URL prod | URL localhost:4200 hardcodée | ❌ BUG |
| Email confirmé après inscription | Non requis ici | Non implémenté | ⚠️ ABSENT |
| Refresh token | Non implémenté | Non implémenté | ⚠️ ABSENT |
| Compte désactivé bloque login | Oui | `if (loginResponse.User.IsDeleted) return Forbid()` | ✅ OK |
| Double rôle possible | Non | `AssignRole` vérifie `IsInRoleAsync` avant ajout | ✅ OK |

---

## PARTIE 5 — Features manquantes

| Manque | Impact |
|--------|--------|
| `isAuthenticated()` Angular ne vérifie pas l'expiration | 401 silencieux sur appels API |
| Lien reset hardcodé `localhost:4200` | Inutilisable en production |
| Pas de refresh token | Re-login obligatoire après 60h |
| Pas de validation d'email à l'inscription | Comptes fantômes possibles |
| `ExpiryMinutes: 3600` (60h) | Risque de sécurité si token volé |
| MAUI n'a pas d'écran de Register | Le DELEGUE ne peut pas créer depuis mobile |
| Pas d'interceptor Angular pour 401 | L'utilisateur n'est pas redirigé vers /login |

---

## PARTIE 6 — Bugs prioritaires (Auth)

| Priorité | Bug | Fichier | Correction |
|----------|-----|--------|-----------|
| 🔴 P1 | URL reset hardcodée `localhost:4200` | `AuthController.cs:324` | Injecter `_env.IsDevelopment()` + config `FrontendUrl` |
| 🔴 P1 | `isAuthenticated()` Angular ignore expiration | `auth.service.ts:116` | Décoder le JWT et vérifier `exp` claim |
| 🟠 P2 | `ExpiryMinutes: 3600` (60 heures) | `appsettings.json` | Passer à 60 minutes + ajouter refresh token |
| 🟠 P2 | Aucun interceptor 401 Angular | Absent | Créer `AuthInterceptor` → redirect `/login` sur 401 |
| 🟡 P3 | Pas de validation email à l'inscription | `AuthService.Register` | Ajouter `EmailConfirmation` ou flag `IsEmailConfirmed` |
| 🟡 P3 | JWT secret dans `appsettings.json` en clair | `appsettings.json:21` | Utiliser variables d'environnement ou Azure Key Vault |

---

# SCENARIO 2 — Gestion catalogue produits

## PARTIE 1 — Création produit (ADMIN Angular)

### Étapes :

1. ADMIN navigue vers `/products/new`.
2. Formulaire `product-form.component.ts` affiché :

```typescript
this.productForm = this.fb.group({
  Nom:           ['', [Validators.required, Validators.maxLength(200)]],
  Description:   ['', [Validators.maxLength(1000)]],
  Categorie:     ['', [Validators.required]],
  Prix_Vente:    ['', [Validators.required, Validators.min(0)]],
  Prix_Creation: ['', [Validators.required, Validators.min(0)]],
  TVA:           [19, [Validators.required, Validators.min(0), Validators.max(100)]],
  isActive:      [true]
}, { validators: priceOrderValidator });
```

**Validator personnalisé :** `priceBelowCreation` → bloque si `PrixVente < Prix_Creation`.

3. La catégorie est soit sélectionnée (dropdown dynamique depuis `GET /products/categories`) soit saisie libre (valeur `__new__`).

4. Soumission → **API appelée :**
   ```
   POST http://cynapharmgateway.runasp.net/products
   Authorization: Bearer {token}
   Roles: ADMIN, SUPERVISEUR
   Body:
   {
     "Id_Produit":    0,
     "Nom":           "Paracétamol 500mg",
     "Description":   "Antalgique et antipyrétique",
     "Categorie":     "Antalgiques",
     "PrixVente":     12.500,
     "Prix_Creation": 8.000,
     "TVA":           19,
     "IsActive":      true,
     "IsArchived":    false
   }
   ```

5. **Statut initial :** `IsActive = true`, `IsArchived = false` — le produit est **actif immédiatement**.

**Réponse succès (200 OK) :**
```json
{
  "isSuccess": true,
  "message":   "Produit enregistré avec succès.",
  "result":    { "id_Produit": 42, "nom": "Paracétamol 500mg", ... }
}
```

6. Redirect vers `/products` après 1200 ms.

**Validations backend (`ProductService.CreateOrUpdateProductAsync`) :**
- Si `Id_Produit` existe en base et `IsArchived = true` → retourne `null` → 200 mais sans résultat (bug : devrait être 400)
- Aucune validation de doublon sur `Nom` (sauf via `ProductExists` endpoint séparé)
- `PrixVente` peut être 0 → **non bloqué** par le backend

---

## PARTIE 2 — Gestion des lots

### Création d'un lot

**API appelée :**
```
POST http://cynapharmgateway.runasp.net/products/lots/lot
Authorization: Bearer {token}
Roles: ADMIN, SUPERVISEUR
Body (LotDto):
{
  "Numero":          "LOT-2024-001",
  "DateExpiration":  "2027-06-30T00:00:00",
  "Quantite":        500,
  "Id_Produit":      42
}
```
> **Attention :** La route est `/lots/lot` (mot "lot" en double) — c'est la route réelle du controller.

**Validations Angular (`lot-form.component.ts`) :**
- `numero` : required, min 3 / max 50 chars, pattern `/^[a-zA-Z0-9\-_]+$/`
- `dateExpiration` : doit être ≥ aujourd'hui en mode création → `futureDateValidator`
- En mode édition : la date originale est acceptée même si passée (lot peut être déjà expiré)
- `quantite` : required, 1 ≤ q ≤ 999 999

**Validations backend :** `ModelState.IsValid` seulement — aucune logique métier supplémentaire dans `CreateOrUpdateLotAsync`.

**`IsExpired` et `IsOutOfStock` :**
Ces champs sont calculés dynamiquement :
- `IsExpired = lot.DateExpiration <= DateTime.UtcNow` (via `IsLotExpiredAsync`)
- `IsOutOfStock = lot.Quantite <= 0` (via `IsLotOutOfStockAsync`)
- Ils ne sont **pas persistés** en base — calculés à la demande
- Le mapper AutoMapper doit configurer ces computed fields pour qu'ils apparaissent dans `LotDto`

**Promotion form filtre les lots :**
```typescript
this.availableLots = lots.filter(l => !l.isExpired && !l.isOutOfStock);
```
→ Seuls les lots non-expirés et non-vides apparaissent dans le dropdown promo.

---

## PARTIE 3 — Upload image produit Cloudinary

### Flow Angular (`product-detail.component.ts` + `cloudinary.service.ts`)

**Étapes :**
1. ADMIN sélectionne un fichier (JPG, PNG, WEBP, max 5 Mo).
2. Format validé : `['image/jpeg', 'image/png', 'image/webp']`.
3. Prévisualisation locale via `FileReader`.
4. `cloudinaryService.uploadImage(file)` appelé :
   ```
   POST https://api.cloudinary.com/v1_1/dezdp9rcc/image/upload
   FormData: { file, upload_preset: "cynapharm_upload" }
   ```
   > Cloudinary Cloud Name : `dezdp9rcc`
   > Preset : `cynapharm_upload` (Signing: Unsigned, resource type: Image)

5. Retourne `secure_url`.
6. Si un support Image actif existe déjà → `marketingService.disableSupport(oldSupportId)` (désactivé, pas supprimé).
7. Création nouveau support :
   ```
   POST /marketting/support
   Body: { Type: "Image", CampaignName: "Photo produit", IsActive: true, Id_Produit: 42 }
   ```
8. Ajout du fichier au support :
   ```
   POST /marketting/support/file
   Body: { NomFichier, Url: secureUrl, Extension: "jpg", Taille: bytes, Id_Support: supportId }
   ```

**URL sauvegardée :** Dans la table `Fichiers` → champ `Url` (Cloudinary `secure_url`).

**Chargement MAUI (`ProductService.ExtractImageUrl`) :**
```csharp
private static string? ExtractImageUrl(Product p) =>
    p.Supports?
        .FirstOrDefault(s => s.IsActive &&
            string.Equals(s.Type, "Image", StringComparison.OrdinalIgnoreCase))
        ?.Fichiers?
        .FirstOrDefault(f => _imageExts.Contains(f.Extension))
        ?.Url;
```
→ Si pas de support Image actif → `ImageUrl = null`.

**Placeholder si pas d'image :** Dans MAUI, `Product.ImageUrl = null`. L'UI doit gérer ce cas via `FallbackValue` ou un placeholder XAML.

---

## PARTIE 4 — Upload support marketing (PDF/documents)

### Cloudinary presets

| Type fichier | Preset | Endpoint Cloudinary |
|-------------|--------|---------------------|
| Images (jpg, png, webp, gif) | `cynapharm_upload` | `/image/upload` |
| Tout autre fichier (PDF, doc, mp4...) | `cynapharm_raw` | `/raw/upload` |

**Flow Angular :**
1. Admin crée un support via modal (type + campaignName).
2. Upload fichier → `cloudinaryService.uploadFile(file)`.
3. Si extension dans `{jpg, png, webp, gif}` → `/image/upload` + preset `cynapharm_upload`.
4. Sinon → `/raw/upload` + preset `cynapharm_raw` → retourne URL `/raw/upload/...`.
5. URL sauvegardée dans `Fichier.Url` via `POST /marketting/support/file`.

**Téléchargement MAUI (`ProductDetailViewModel.OpenDocumentAsync`) :**
1. Si URL contient `/raw/upload/` → injecte `fl_attachment` :
   ```csharp
   url.Replace("/raw/upload/", "/raw/upload/fl_attachment/")
   ```
   → Force le téléchargement (évite les problèmes CORS Cloudinary).
2. `_productService.DownloadFileAsync(downloadUrl)` → télécharge les bytes.
3. Sauvegarde dans `FileSystem.CacheDirectory`.
4. `Launcher.OpenAsync(new OpenFileRequest(..., MimeTypeFor(fileName)))` → ouvre avec l'app native.

**Visibilité des documents pour MEDECIN :**
Dans `ProductDetailViewModel` :
```csharp
if (!CanSeePrices && s.Fichiers != null)
    s.Fichiers = s.Fichiers
        .Where(f => !imageExts.Contains(f.Extension))  // imageExts = {jpg,jpeg,png,webp,gif}
        .ToList();
```
→ MEDECIN voit tous les fichiers **SAUF les images** → PDF, doc, docx, mp4 etc. sont accessibles.

> ⚠️ Bug Angular (`isFileBroken()`): Si un fichier non-image a été uploadé via `/image/upload` (preset image), son URL contient `/image/upload/` alors qu'il devrait être dans `/raw/upload/`. Ces fichiers retournent 401 de Cloudinary. Le composant les détecte et affiche un avertissement.

---

## PARTIE 5 — Création promotion

### Comment une promotion est liée à un lot

**Modèle :**
```
Promotion → NumeroLot (FK vers Lot.NumeroLot)
```
- Une promotion référence un lot par son numéro
- Un lot peut avoir plusieurs promotions

**Étapes Angular (`promotion-form.component.ts`) :**
1. Chargement des lots disponibles : `GET /lots` → filtrés `!isExpired && !isOutOfStock`.
2. Formulaire :
   - `codePromo` : required, max 50, pas d'espaces
   - `pourcentage` : required, **1 ≤ p ≤ 100** (pas de promo à 0%)
   - `numeroLot` : required — lie la promo à un lot
   - `dateDebut` : today par défaut
   - `dateExpiration` : required
   - Validator `dateRangeValidator` : `dateExpiration > dateDebut`
   - `estActive` : true par défaut

**API appelée :**
```
POST http://cynapharmgateway.runasp.net/products/api/promos
Body:
{
  "codePromo":            "PROMO-ETE",
  "typePromotion":        "Pourcentage",
  "pourcentage":          15.0,
  "porteeSurTousLesLots": false,
  "numeroLot":            "LOT-2024-001",
  "dateDebut":            "2026-06-01",
  "dateExpiration":       "2026-08-31",
  "estActive":            true
}
```

**Backend (`PromoService.CreateOrUpdatePromotionAsync`) :**
1. Vérifie que le lot avec `NumeroLot` existe → sinon retourne `null`.
2. Crée ou met à jour la promotion.

**`IsValid` calculé comme :**
```csharp
EstActive == true
&& DateDebut != null
&& DateDebut <= DateTime.UtcNow
&& DateExpiration >= DateTime.UtcNow
```

**Quand la promotion expire :** `DateExpiration < now` → `IsValid = false` automatiquement au prochain appel. Aucun job de nettoyage en base.

---

## PARTIE 6 — Activation / Désactivation / Archivage

| Action | Route | Role | Effet en base |
|--------|-------|------|--------------|
| Activer | `PUT /products/{id}/activate` | ADMIN, SUPERVISEUR | `IsActive = true` |
| Désactiver | `PUT /products/{id}/deactivate` | ADMIN | `IsActive = false` (IsArchived inchangé) |
| Archiver | `PUT /products/{id}/archive` | ADMIN | `IsActive = false` **ET** `IsArchived = true` |
| Désarchiver | `PUT /products/{id}/unarchive` | ADMIN | `IsArchived = false` (IsActive reste false) |
| Suppression définitive | `DELETE /products/{id}` | ADMIN | Hard delete (seulement si IsArchived=true ET stock=0) |

**`CanArchiveProduct` :** vérifie si stock total = 0 :
```csharp
public async Task<bool> CanArchiveProductAsync(int productId)
{
    var totalStock = await GetTotalStockAsync(productId);
    return totalStock == 0;
}
```
> ⚠️ **Bug :** `ArchiveProductAsync` dans `ProductController` n'appelle PAS `CanArchiveProductAsync` avant d'archiver! Un produit peut être archivé avec du stock restant.

**Effet sur la visibilité MAUI :**

| État | `GetVisibleProductsAsync()` | Visible MEDECIN/CLIENT |
|------|----------------------------|----------------------|
| `IsActive=true, IsArchived=false` | ✅ Inclus | ✅ OUI |
| `IsActive=false, IsArchived=false` | ❌ Exclu | ❌ NON |
| `IsActive=false, IsArchived=true` | ❌ Exclu | ❌ NON |

**`GetAllProductsAsync()` (ADMIN) :**
- Filtre : `!IsArchived` seulement → les produits désactivés restent visibles pour ADMIN.

---

## PARTIE 7 — Consultation MAUI (MEDECIN vs CLIENT)

### `ProductListViewModel.cs`

```csharp
var role = await SecureStorage.GetAsync(StorageKeys.UserRole);
CanSeePrices = role is not "MEDECIN";
_useVisibleEndpoint = role is "MEDECIN" or "PHARMACIEN" or "GROSSISTE" or "CLIENT";
```

| Rôle | Endpoint API | Filtre backend |
|------|-------------|---------------|
| MEDECIN | `GET /products/visible` | `IsActive=true && !IsArchived` |
| CLIENT (PHARMACIEN/GROSSISTE) | `GET /products/visible` | `IsActive=true && !IsArchived` |
| DELEGUE/ADMIN | `GET /products` | `!IsArchived` (client-side: `!IsArchived`) |

**Prix : montré à CLIENT, caché à MEDECIN :**
- `CanSeePrices = role is not "MEDECIN"`
- MAUI `ProductDetailViewModel.HasInformations = CanSeePrices` → bloc INFORMATIONS masqué pour MEDECIN
- `Product.PrixUnitaire` est toujours dans la réponse backend (`prixVente` dans `ProduitDto`) — caché uniquement côté UI

**MEDECIN — champs cachés :**
- `PrixUnitaire` (INFORMATIONS card cachée via `HasInformations = false`)
- `Prix_Creation` (dans INFORMATIONS)
- `TVA` (dans INFORMATIONS)
- Lots (403 sur `GET /lots/product/{id}` car MEDECIN non autorisé)
- Promotions (fetched silencieusement, aucun blocage serveur)
- Fichiers images dans les supports

**MEDECIN — champs visibles :**
- `Nom`, `Description`, `Categorie`
- `ImageUrl` (extrait du support Image actif)
- Documents non-image (PDF, doc, docx via supports marketing)
- Promotions (via `GET /promos/product/{id}`)

**CLIENT — tout visible** (même visibilité que MEDECIN sauf le prix affiché via `PrixDisplay`).

---

## PARTIE 8 — Business Logic Verification

| Étape | Attendu | Réel | Statut |
|-------|---------|------|--------|
| Prix peut être 0 | Bloqué | NON bloqué — `Validators.min(0)` accepte 0 | ❌ BUG |
| Lot créé avec date passée | Bloqué (Angular) | Bloqué en création, pas en édition | ⚠️ PARTIEL |
| Promotion % = 0 | Bloqué | Angular : `Validators.min(1)` bloque / Backend : aucune validation | ⚠️ PARTIEL |
| Stock vérifié avant archivage | Oui (CanArchive) | `CanArchiveProduct` existe MAIS non appelé dans `ArchiveProductAsync` | ❌ BUG |
| Même lot peut avoir 2 promos actives | Idempotence | OUI possible — aucune contrainte unique | ❌ BUG |
| Désarchivage réactive `IsActive` | Oui | NON — `UnarchiveProduct` ne remet pas `IsActive = true` | ❌ BUG |
| MEDECIN bloqué sur lots | Oui | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` → 403 | ✅ OK |
| Suppression physique sécurisée | IsArchived=true + stock=0 | OUI — vérification présente | ✅ OK |
| Image upload → preset correct | cynapharm_upload pour images | OUI — ext détectée côté Angular | ✅ OK |
| PDF upload → preset raw | cynapharm_raw pour PDFs | OUI — ext détectée côté Angular | ✅ OK |

---

## PARTIE 9 — Features manquantes

| Manque | Impact |
|--------|--------|
| `ArchiveProductAsync` n'appelle pas `CanArchiveProduct` | Archivage possible avec stock > 0 |
| `UnarchiveProductAsync` ne remet pas `IsActive = true` | Produit désarchivé reste invisible |
| Prix 0 non bloqué backend | Produit gratuit non intentionnel |
| Lot peut avoir date expirée après création (édition) | Mauvaise hygiène des données |
| 2 promotions actives sur même lot autorisées | `ApplyBestPromotion` traite les doublons mais crée de la confusion |
| `Promotion.IsValid` non stocké — recalculé à chaque appel | Pas de cache / pas de flag de statut persisté |
| MEDECIN voit les promotions via `/promos/product/{id}` — est-ce intentionnel ? | Promotions visibles → prix remisés révèlent le prix de base |
| Aucun écran MAUI pour ADMIN (gestion catalogue) | Tout doit passer par Angular |
| `ProduitDto` ne contient pas `ImageUrl` directe | MAUI doit traverser `Supports → Fichiers` pour trouver l'image |

---

# SCENARIO 3 — Scénario MEDECIN complet (MAUI)

## PARTIE 1 — Login MEDECIN

1. MEDECIN saisit email + password → `LoginAsync()`.
2. Redirect après login : **`//products`** (catalogue direct).
3. `ApplyRoleVisibility("MEDECIN")` appelé :
   ```csharp
   bool isMedecin = role is "MEDECIN";
   ShowDashboard    = false;
   ShowVisites      = false;
   ShowPlanning     = false;
   ShowCatalogue    = true;   // toujours visible
   ShowOrders       = false;
   ShowDocuments    = false;
   ShowReclamations = false;
   ShowStock        = false;
   ShowObjectifs    = false;
   Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);  // Flyout désactivé
   ```
4. **`FlyoutBehavior.Disabled`** → pas de menu hamburger.
5. Seuls **Catalogue** et **Profil** sont accessibles (tab bar seulement).

---

## PARTIE 2 — Catalogue produits (MEDECIN)

**API appelée :**
```
GET http://cynapharmgateway.runasp.net/products/visible
Authorization: Bearer {token}
```

**Filtre backend (`ProductService.GetVisibleProductsAsync`) :**
```csharp
_db.Produits
    .Where(p => p.IsActive && !p.IsArchived)
    .Include(p => p.Lots).ThenInclude(l => l.Promotions)
    .Include(p => p.Supports).ThenInclude(s => s.Fichiers)
```
→ Produits actifs non archivés, avec lots + promotions + supports inclus.

**`ProductListViewModel` pour MEDECIN :**
- `CanSeePrices = false` → prix non affichés dans la liste
- `_useVisibleEndpoint = true` → `/products/visible`
- Recherche locale : `Nom`, `Reference`, `Description`
- Catégories chargées via `GET /products/categories`
- Offline : SQLite avec dernière cache

**Champs CACHÉS pour MEDECIN (liste) :**
- Prix (`PrixUnitaire`) — `CanSeePrices = false`

**Champs AFFICHÉS (liste) :**
- `Nom`, `Categorie`, `ImageUrl`, badge `Actif` (via `Actif`)

---

## PARTIE 3 — Fiche produit (MEDECIN)

**API appelée :**
```
GET http://cynapharmgateway.runasp.net/products/{id}
Authorization: Bearer {token}
```

**`ProductDetailViewModel` pour MEDECIN :**
```csharp
CanSeePrices    = false
HasInformations = false   // => INFORMATIONS card masquée
```

### Sections VISIBLES pour MEDECIN :

| Section | Contenu |
|---------|---------|
| Résumé produit | Nom, Description, Catégorie, Image |
| Documents liés | Supports marketing actifs (hors type "Image") — fichiers non-image seulement |
| Bannière | "Contactez votre délégué pour les détails commerciaux" (si implémentée en XAML) |

### Sections CACHÉES pour MEDECIN :

| Section | Raison |
|---------|--------|
| INFORMATIONS | `HasInformations = false` → `PrixVente`, `Prix_Creation`, `TVA` non affichés |
| Lots | Endpoint `GET /lots/product/{id}` retourne **403** (rôle DELEGUE+ requis) |
| Stock | Non accessible |

### Documents (Supports Marketing) :

**Logique filtrage :**
```csharp
foreach (var s in product.Supports.Where(s =>
    s.IsActive &&
    !string.Equals(s.Type, "Image", StringComparison.OrdinalIgnoreCase)))  // exclut type Image
{
    if (!CanSeePrices && s.Fichiers != null)
        s.Fichiers = s.Fichiers
            .Where(f => !imageExts.Contains(f.Extension))  // exclut jpg, jpeg, png, webp, gif
            .ToList();
    Supports.Add(s);
}
```

**Résultat :** MEDECIN voit :
- Supports actifs de tout type **SAUF "Image"** (l'image produit n'est pas dans la section documents)
- Dans ces supports : tous les fichiers **SAUF les images** (jpg/jpeg/png/webp/gif)
- **PDF, doc, docx, mp4, pptx → visibles et téléchargeables**

**Téléchargement document (`OpenDocumentAsync`) :**
1. URL Cloudinary `raw/upload/...` → injecte `fl_attachment`
2. `DownloadFileAsync` → bytes locaux
3. `Launcher.OpenAsync` → lecteur natif (PDF viewer, etc.)

---

## PARTIE 4 — Profil MEDECIN

**Champs affichés :**
- Nom, Email, Téléphone, Adresse (depuis SecureStorage)

**Modification profil (`PUT /auth/update-profile`) :**
```json
{
  "Email":       "medecin@example.com",
  "Name":        "Dr. Ben Ali",
  "PhoneNumber": "+21612345678",
  "Adresse":     "Tunis"
}
```

> ⚠️ L'email est l'identifiant de recherche dans `UpdateProfileAsync`. Le MEDECIN peut modifier son nom, téléphone et adresse mais **pas son email** (l'email est en lecture seule dans Identity, changement d'email requiert une procédure séparée).

**MAUI : `AuthService.UpdateProfileAsync` :**
```csharp
public async Task<ApiResponse<object>> UpdateProfileAsync(UpdateProfileDto request)
{
    await _api.PutAsync<object>(ApiRoutes.Auth.UpdateProfile, request);
    return new ApiResponse<object> { IsSuccess = true };
}
```

---

## PARTIE 5 — Business Logic Verification

| Étape | Attendu | Réel | Statut |
|-------|---------|------|--------|
| MEDECIN ne voit pas les prix | Oui | `CanSeePrices = false` → INFORMATIONS cachée | ✅ OK |
| MEDECIN voit les documents PDF | Oui | Fichiers non-image inclus | ✅ OK |
| MEDECIN ne voit pas les lots | Oui | 403 sur endpoint lots (silencieusement ignoré) | ✅ OK |
| Flyout désactivé pour MEDECIN | Oui | `FlyoutBehavior.Disabled` | ✅ OK |
| MEDECIN peut modifier son profil | Oui | `PUT /auth/update-profile` | ✅ OK |
| MEDECIN voit les promotions | Non requis | OUI — endpoint `/promos/product/{id}` accessible | ⚠️ À VÉRIFIER |
| Prix non dans la réponse API | Non — prix dans ProduitDto | Prix TOUJOURS dans la réponse, masqué UI seulement | ⚠️ PARTIEL |

---

## PARTIE 6 — Features manquantes (MEDECIN)

| Manque | Impact |
|--------|--------|
| Prix dans `ProduitDto` même pour MEDECIN | Interceptable côté réseau |
| `GET /products/visible` retourne `prixVente` | MEDECIN peut voir les prix dans la réponse HTTP |
| Pas d'endpoint `/products/visible-medecin` sans prix | Nécessite filtrage backend par rôle |
| Bannière "contactez votre délégué" non implémentée dans XAML ? | À vérifier dans les XAML non lus |
| MEDECIN ne peut pas créer de réclamation (MAUI) | `ShowReclamations = false` pour MEDECIN |

---

# SCENARIO 4 — KPI et objectifs délégué

## PARTIE 1 — Création objectif (ADMIN Angular)

**Formulaire `objectif-form.component.ts` :**
```typescript
this.form = this.fb.group({
  id_User_Delegue: [null, [Validators.required]],
  type:            ['',   [Validators.required]],       // TypeObjectif enum
  periode:         ['',   [Validators.required]],       // PeriodeObjectif enum
  valeurCible:     [null, [Validators.required, Validators.min(1)]],
  dateDebut:       ['',   [Validators.required]],
  dateFin:         ['',   [Validators.required]]
});
```

**Auto-calcul des dates par période :**
```typescript
private applyPeriodeDates(periode: number): void {
  switch (periode) {
    case PeriodeObjectif.Mensuel:     // 1er → dernier du mois courant
    case PeriodeObjectif.Trimestriel: // 1er du trimestre → dernier du trimestre
    case PeriodeObjectif.Annuel:      // 1er janvier → 31 décembre
  }
  this.form.patchValue({ dateDebut, dateFin });
}
```

**TypeObjectif options :**
| Valeur | Label |
|--------|-------|
| `Visites` | Visites |
| `ChiffreAffaires` | Chiffre d'affaires |
| `NouveauxClients` | Nouveaux clients |
| `Fidelisation` | Fidélisation |

**PeriodeObjectif options :** Mensuel / Trimestriel / Annuel

**API appelée :**
```
POST http://cynapharmgateway.runasp.net/fields/objectifs
Body (ObjectifDelegueDto):
{
  "id_User_Delegue": 7,
  "type":            0,   // TypeObjectif.Visites
  "periode":         0,   // PeriodeObjectif.Mensuel
  "valeurCible":     20,
  "valeurRealisee":  0
}
```

> 🔴 **Bug critique :** `ObjectifDelegueDto` **ne contient pas `DateDebut` / `DateFin`**! Le frontend les envoie mais le backend les ignore. Les champs `DateDebut` et `DateFin` de `Objectif_Delegue` restent à `DateTime.MinValue` (01/01/0001).

**Validations backend (`ObjectifService.CreateOrUpdateObjectifAsync`) :**
```csharp
if (dto.ValeurCible <= 0)   return null;  // ValeurCible doit être > 0
if (!Enum.IsDefined(typeof(TypeObjectif), dto.Type))   return null;
if (!Enum.IsDefined(typeof(PeriodeObjectif), dto.Periode)) return null;
```
- Pas de validation de dates passées
- Pas de limite sur `ValeurCible`

---

## PARTIE 2 — Suivi objectif (Angular)

**`valeurRealisee` est-elle automatique ou manuelle ?**

**Pour `TypeObjectif.ChiffreAffaires` :** **Manuelle** (le CA vient du service Orders, non accessible depuis FieldAPI). Update via :
```
PUT /fields/objectifs/{id}/value?nouvelleValeur=15000
Roles: ADMIN, SUPERVISEUR
```

**Pour tous les autres types (Visites, NouveauxClients, Fidelisation) :** **Calculée automatiquement** lors de l'appel à `GET /fields/kpi/performance/{idDelegue}`. Le service `KPIService.CalculatePerformanceAsync()` recalcule `ValeurRealisee` et le **met à jour en base** :
```csharp
if (valeurRealisee != o.ValeurRealisee)
{
    var objectifToUpdate = await _db.Objectifs.FirstOrDefaultAsync(...);
    objectifToUpdate.ValeurRealisee = valeurRealisee;
    await _db.SaveChangesAsync();
}
```

**Progress bar (si implémentée dans le HTML) :** `Pourcentage = min(100, (ValeurRealisee / ValeurCible) * 100)`.

**Completion :** Aucun flag "complété" — `Pourcentage >= 100` indique que l'objectif est atteint, mais le statut n'est pas persisté.

---

## PARTIE 3 — KPI Dashboard (Angular)

**Composant `kpi-dashboard.component.ts` :**
```typescript
this.svc.getNombreVisites(id, dateDebut, dateFin)     // visitesCount
this.svc.getPerformanceRate(id)                       // performanceRate
this.svc.getTauxConversion(id, dateDebut, dateFin)    // tauxConversion
this.svc.getHistorique(id)                            // historique
```

**API 1 — visitesCount :**
```
GET /fields/kpi/visites-count?idDelegue={id}&debut={date}&fin={date}
Roles: ADMIN, SUPERVISEUR, DELEGUE
```
```csharp
// Compte les visites IsCompleted=true dans la période
return await _db.Visites.CountAsync(v =>
    v.Id_User_Delegue == idDelegue &&
    v.IsCompleted &&
    v.DateVisite >= debut && v.DateVisite <= fin);
```

**API 2 — performanceRate :**
```
GET /fields/kpi/performance-rate/{idDelegue}
```
```csharp
// Moyenne des % de tous les objectifs du délégué
return performances.Average(p => p.Pourcentage);
```

**API 3 — tauxConversion :**
```
GET /fields/kpi/taux-conversion/{idDelegue}?debut={date}&fin={date}
```
```csharp
// (rapports POSITIF dans période) / (total visites dans période) * 100
var totalVisites    = await _db.Visites.CountAsync(v =>
    v.Id_User_Delegue == idDelegue && debut <= v.DateVisite && v.DateVisite <= fin);
var visitePositives = await _db.Rapports.CountAsync(r =>
    r.Id_User_Delegue == idDelegue && r.Resultat == "POSITIF" &&
    debut <= r.DateRapport && r.DateRapport <= fin);
return Math.Round((double)visitePositives / totalVisites * 100, 2);
```
> ⚠️ `totalVisites` compte **toutes les visites**, y compris les non-complétées. Résultat différent de `visitesCount`.

**API 4 — historique :**
```
GET /fields/kpi/historique/{idDelegue}
```
```csharp
// Liste des visites ordonnées par date DESC
return await _db.Visites
    .Where(v => v.Id_User_Delegue == idDelegue)
    .OrderByDescending(v => v.DateVisite)
    .Select(v => new ActiviteHistoriqueDto
    {
        Id_Visite  = v.Id_Visite,
        Date       = v.DateVisite,
        Type       = v.Type,         // VisiteType enum
        HasRapport = v.Rapport != null
    })
    .ToListAsync();
```

**Angular `historiqueAction` / `historiqueDate` / `historiqueDetail` :** Lit plusieurs variantes de casing (`action`, `Action`, `type`, `Type`...) — robuste mais verbose.

---

## PARTIE 4 — Business Logic Verification

| Étape | Attendu | Réel | Statut |
|-------|---------|------|--------|
| `valeurRealisee` auto-mise à jour | Pour Visites/Clients/Fidelisation | OUI — dans `CalculatePerformanceAsync` | ✅ OK |
| `valeurRealisee` ChiffreAffaires | Manuelle | OUI — `PUT /objectifs/{id}/value` | ✅ OK |
| Objectif créé avec dates passées | Bloqué | NON — `dateDebut`/`dateFin` non dans DTO | ❌ BUG |
| KPI endpoint fonctionnel | Oui | OUI — tous implémentés | ✅ OK |
| `tauxConversion` = 0 si 0 visites | Oui | `if (totalVisites == 0) return 0` | ✅ OK |
| `performanceRate` clamped à 100 | Oui | `Math.Min(100, ...)` | ✅ OK |
| DELEGUE voit ses propres KPI | Oui | Roles ADMIN, SUPERVISEUR, DELEGUE | ✅ OK |
| `DateDebut`/`DateFin` persistés | Oui | NON — absents du DTO | ❌ BUG |
| `ObjectifDto` contient dates | Oui | NON — `ObjectifDelegueDto` sans dates | ❌ BUG |
| Historique retourne type de visite | Oui | `VisiteType` enum retourné | ✅ OK |

---

## PARTIE 5 — Features manquantes (KPI / Objectifs)

| Manque | Impact |
|--------|--------|
| `DateDebut`/`DateFin` non dans `ObjectifDelegueDto` | Auto-calcul Angular perdu côté backend |
| Pas de flag "objectif atteint" | Aucune notification ni alerte |
| `ChiffreAffaires` non connecté à OrderAPI | Toujours manuel |
| Pas d'objectif sur les distributions d'échantillons | Scénario DELEGUE incomplet |
| `kpi-dashboard` Angular n'a pas de date par défaut | `dateDebut`/`dateFin` vides → `tauxConversion` non calculé |
| Historique ne contient que les visites (pas les rapports, distributions) | Historique incomplet |
| Pas de KPI MAUI (aucun écran KPI dans le projet mobile) | DELEGUE ne peut pas voir ses KPI sur mobile |

---

# PARTIE GLOBALE — Endpoints complets utilisés dans ces scénarios

## AuthAPI (`cynapharmauth.runasp.net`)

| Méthode | Route Gateway | Downstream | Roles | Description |
|---------|--------------|-----------|-------|-------------|
| `POST` | `/auth/login` | `/api/auth/login` | Public | Login + Turnstile |
| `POST` | `/auth/register` | `/api/auth/register` | ADMIN, SUPERVISEUR, DELEGUE | Créer utilisateur |
| `POST` | `/auth/forgot-password` | `/api/auth/forgot-password` | Public | Demander reset |
| `PUT` | `/auth/reset-password` | `/api/auth/reset-password` | Public | Réinitialiser mdp |
| `PUT` | `/auth/change-password` | `/api/auth/change-password` | Any authenticated | Changer mdp |
| `PUT` | `/auth/update-profile` | `/api/auth/update-profile` | Any authenticated | MAJ profil |
| `GET` | `/auth/users` | `/api/auth/users` | ADMIN | Liste tous users |
| `GET` | `/auth/users/{id}` | `/api/auth/users/{id}` | ADMIN, SUPERVISEUR | User par ID |
| `GET` | `/auth/users/search` | `/api/auth/users/search?keyword=` | ADMIN, SUPERVISEUR | Recherche user |
| `GET` | `/auth/disabled-users` | `/api/auth/disabled-users` | ADMIN | Comptes désactivés |
| `PUT` | `/auth/delete-user/{email}` | `/api/auth/delete-user/{email}` | ADMIN | Soft-delete user |
| `PUT` | `/auth/enable-user/{email}` | `/api/auth/enable-user/{email}` | ADMIN | Réactiver user |
| `PUT` | `/auth/change-role` | `/api/auth/change-role` | ADMIN, SUPERVISEUR | Changer rôle |
| `POST` | `/auth/AssignRole` | `/api/auth/AssignRole` | ADMIN, SUPERVISEUR | Assigner rôle |

## ProductAPI (`prefix /products`)

| Méthode | Route Gateway | Roles | Description |
|---------|--------------|-------|-------------|
| `GET` | `/products` | Any authenticated | Tous produits non archivés |
| `GET` | `/products/visible` | Any authenticated | Produits actifs non archivés |
| `GET` | `/products/{id}` | Any authenticated | Produit par ID |
| `GET` | `/products/categories` | Any authenticated | Liste catégories |
| `GET` | `/products/search?keyword=` | Any authenticated | Recherche keyword ≥ 3 chars |
| `GET` | `/products/filter` | ADMIN, SUPERVISEUR, DELEGUE | Filtre paginé |
| `POST` | `/products` | ADMIN, SUPERVISEUR | Créer/MAJ produit |
| `PUT` | `/products/{id}/activate` | ADMIN, SUPERVISEUR | Activer produit |
| `PUT` | `/products/{id}/deactivate` | ADMIN | Désactiver produit |
| `PUT` | `/products/{id}/archive` | ADMIN | Archiver produit |
| `PUT` | `/products/{id}/unarchive` | ADMIN | Désarchiver produit |
| `DELETE` | `/products/{id}` | ADMIN | Suppression définitive (si archivé + stock=0) |
| `GET` | `/products/lots/lot/{numero}` | ADMIN, SUPERVISEUR | Lot par numéro |
| `GET` | `/products/lots/product/{id}` | ADMIN, SUPERVISEUR, DELEGUE | Lots d'un produit |
| `POST` | `/products/lots/lot` | ADMIN, SUPERVISEUR | Créer/MAJ lot |
| `DELETE` | `/products/lots/lot/{numero}` | ADMIN | Supprimer lot |
| `GET` | `/products/lots` | ADMIN, SUPERVISEUR, DELEGUE | Tous les lots |
| `GET` | `/products/lots/expired` | ADMIN, SUPERVISEUR | Lots expirés |
| `GET` | `/products/lots/near-expiration?daysThreshold=` | ADMIN, SUPERVISEUR, DELEGUE | Lots proches expiration |
| `GET` | `/products/promos` | ADMIN, SUPERVISEUR, DELEGUE | Toutes les promos |
| `GET` | `/products/promos/{id}` | ADMIN, SUPERVISEUR, DELEGUE | Promo par ID |
| `POST` | `/products/promos` | ADMIN, SUPERVISEUR | Créer/MAJ promo |
| `DELETE` | `/products/promos/{id}` | ADMIN | Supprimer promo |
| `GET` | `/products/promos/product/{id}` | Any authenticated | Promos d'un produit |
| `GET` | `/products/promos/lot/{numero}` | ADMIN, SUPERVISEUR, DELEGUE | Promos d'un lot |
| `GET` | `/products/promos/{id}/valid` | ADMIN, SUPERVISEUR, DELEGUE | Promo valide ? |
| `GET` | `/products/promos/product/{id}/apply?initialPrice=` | Any authenticated | Prix après meilleure promo |
| `GET` | `/products/marketting/product/{id}/supports` | ADMIN, SUPERVISEUR, DELEGUE | Supports d'un produit |
| `POST` | `/products/marketting/support` | ADMIN, SUPERVISEUR | Créer/MAJ support |
| `POST` | `/products/marketting/support/file` | ADMIN, SUPERVISEUR | Ajouter fichier |
| `DELETE` | `/products/marketting/file/{id}` | ADMIN | Supprimer fichier |
| `GET` | `/products/marketting/support/{id}/files` | ADMIN, SUPERVISEUR, DELEGUE | Fichiers d'un support |
| `PUT` | `/products/marketting/support/{id}/disable` | ADMIN | Désactiver support |
| `PUT` | `/products/marketting/support/{id}/activate` | ADMIN | Activer support |
| `GET` | `/products/marketting/product/{id}/visible-supports` | Any authenticated | Supports visibles |

## FieldAPI — KPI / Objectifs (`prefix /fields`)

| Méthode | Route Gateway | Roles | Description |
|---------|--------------|-------|-------------|
| `GET` | `/fields/objectifs` | ADMIN, SUPERVISEUR | Tous les objectifs |
| `GET` | `/fields/objectifs/{id}` | ADMIN, SUPERVISEUR, DELEGUE | Objectif par ID |
| `GET` | `/fields/objectifs/by-delegue/{id}` | ADMIN, SUPERVISEUR, DELEGUE | Objectifs d'un délégué |
| `POST` | `/fields/objectifs` | ADMIN, SUPERVISEUR | Créer/MAJ objectif |
| `PUT` | `/fields/objectifs/{id}/value?nouvelleValeur=` | ADMIN, SUPERVISEUR | MAJ valeur réalisée |
| `DELETE` | `/fields/objectifs/{id}` | ADMIN | Supprimer objectif |
| `GET` | `/fields/kpi/visites-count?idDelegue=&debut=&fin=` | ADMIN, SUPERVISEUR, DELEGUE | Nombre visites complétées |
| `GET` | `/fields/kpi/performance/{idDelegue}` | ADMIN, SUPERVISEUR, DELEGUE | Performance par objectif |
| `GET` | `/fields/kpi/performance-rate/{idDelegue}` | ADMIN, SUPERVISEUR, DELEGUE | Taux moyen performance |
| `GET` | `/fields/kpi/taux-conversion/{idDelegue}?debut=&fin=` | ADMIN, SUPERVISEUR, DELEGUE | Taux conversion visites |
| `GET` | `/fields/kpi/historique/{idDelegue}` | ADMIN, SUPERVISEUR, DELEGUE | Historique activité |
| `GET` | `/fields/kpi/has-visite?idDelegue=&date=` | ADMIN, SUPERVISEUR, DELEGUE | Visite à une date ? |
| `GET` | `/fields/kpi/client-fidelite/{idClient}` | ADMIN, SUPERVISEUR | Fidélité client |

---

# Résumé global des bugs prioritaires

| Priorité | Bug | Fichier | Correction |
|----------|-----|--------|-----------|
| 🔴 P1 | URL reset password hardcodée `localhost:4200` | `AuthController.cs:324` | Config `FrontendUrl` injectée |
| 🔴 P1 | Angular `isAuthenticated()` ignore expiration JWT | `auth.service.ts:116` | Parser claim `exp` depuis le JWT |
| 🔴 P1 | `ObjectifDelegueDto` sans `DateDebut`/`DateFin` | `ObjectifDelegueDto.cs` | Ajouter ces deux champs |
| 🔴 P1 | `ArchiveProductAsync` n'appelle pas `CanArchiveProduct` | `ProductService.cs:93` | Ajouter check stock = 0 avant archivage |
| 🔴 P1 | `UnarchiveProductAsync` ne remet pas `IsActive = true` | `ProductService.cs:105` | Ajouter `product.IsActive = false` (actif = false puis l'admin active manuellement ? À décider) |
| 🟠 P2 | `ExpiryMinutes: 3600` (60 heures) | `appsettings.json` | Réduire à 60 min, ajouter refresh token |
| 🟠 P2 | 2 promos actives sur même lot possible | `PromoService.cs` | Contrainte unique `(NumeroLot, EstActive)` |
| 🟠 P2 | `PrixVente = 0` non bloqué backend | `ProductService.cs:71` | `if (produitDto.PrixVente <= 0) return null` |
| 🟠 P2 | `prixVente` inclus dans `/products/visible` pour MEDECIN | `ProduitDto.cs` + `ProductService.cs` | Créer `ProduitDtoMedecin` sans champs sensibles |
| 🟠 P2 | Aucun interceptor 401 Angular | Absent | Créer `AuthInterceptor` |
| 🟡 P3 | JWT secret en clair dans `appsettings.json` | `appsettings.json:21` | Variables d'environnement / Key Vault |
| 🟡 P3 | `kpi-dashboard` sans dates par défaut | `kpi-dashboard.component.ts` | Initialiser `dateDebut`/`dateFin` sur le mois courant |
| 🟡 P3 | `ChiffreAffaires` toujours manuel (non connecté à OrderAPI) | `KPIService.cs:196` | Créer appel HTTP vers OrderAPI pour CA |
| 🟡 P3 | Pas de KPI sur mobile (MAUI) | Absent | Créer `KpiViewModel` + `KpiPage` |
