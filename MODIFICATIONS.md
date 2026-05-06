# Plan des modifications — CynapSoft CRM

> Projet : **CynapSoftCRMBackend** (Angular 17+ frontend + .NET 9 microservices backend)
> Date : Mai 2026

---

## Vue d'ensemble

Ce document liste toutes les modifications effectuées sur le projet, organisées par domaine fonctionnel.

---

## 1. Correction des filtres Produits

### Problème
Le filtre "Archivé" affichait toujours 0 résultat car l'API `/products` exclut les produits archivés côté backend (`WHERE !p.IsArchived`).

### Fichiers modifiés
| Fichier | Modification |
|---|---|
| `features/products/product.service.ts` | Ajout de `getProductsAll()` qui appelle `/products/filter?allowArchived=true&pageSize=1000` |
| `features/products/product-list/product-list.component.ts` | Utilise `getProductsAll()` au lieu de `getProducts()`. Fallback vers `getProducts()` en cas de 403 (rôle insuffisant) |

---

## 2. Correction de l'onglet Promotions dans le détail produit

### Problème
L'onglet "Promotions" du détail produit était vide malgré une réponse 200 OK. Le template utilisait de mauvais noms de propriétés (`promo.Nom`, `promo.DateFin`, `promo.Id_Promotion`, `promo.Pourcentage`) qui n'existent pas dans le backend.

### Fichiers modifiés
| Fichier | Modification |
|---|---|
| `features/products/services/product-advanced.service.ts` | Correction de l'URL : `/api/promos/product/` → `/products/promos/product/` (passage par la gateway Ocelot). Ajout du déballage de la réponse (`unwrap`) |
| `features/products/product-detail/product-detail.component.html` | Réécriture complète de l'onglet "Promotions" : utilisation des vrais champs (`codePromo`, `pourcentage`, `dateDebut`, `dateExpiration`, `estActive`, `numeroLot`). Affichage en cartes avec badge de statut coloré (Active / Inactive / Expirée) |
| `features/products/product-detail/product-detail.component.ts` | Ajout de la méthode `isPromoExpired()` |
| `features/products/product-detail/product-detail.component.scss` | Ajout des styles CSS pour les cartes promotion (`.promo-cards`, `.promo-card-item`, `.promo-code-badge`, etc.) |

---

## 3. Correction des supports marketing (édition toujours crée au lieu de modifier)

### Problème
Cliquer sur "Modifier" un support créait toujours un nouveau support au lieu de mettre à jour l'existant. Cause : la méthode `normalizeSupport()` cherchait `s.idSupportMarketting` (sans underscore) alors que le backend avec la politique CamelCase envoie `s['id_SupportMarketting']` (avec underscore).

### Fichiers modifiés
| Fichier | Modification |
|---|---|
| `features/products/product-detail/product-detail.component.ts` | `normalizeSupport()` : ajout de `s['id_SupportMarketting']` comme premier test (notation crochet nécessaire pour les clés avec underscore). `normalizeFichier()` : même correction pour `id_Fichier`, `id_Support`. Condition `submitSupportForm()` : `editingSupportId !== null` au lieu de `editingSupportId` (falsy check incorrect sur `0`) |

---

## 4. Correction de l'affichage des promotions dans le détail de lot

### Problème
L'affichage des promotions dans la fiche lot montrait `−%` et des dates vides car le template utilisait `promo.description`, `promo.tauxReduction`, `promo.dateFin` — des champs qui n'existent pas dans le `PromotionDto` C#.

### Fichiers modifiés
| Fichier | Modification |
|---|---|
| `features/lots/lot.model.ts` | Correction de l'interface `PromotionDto` : `id_Promo`, `codePromo`, `pourcentage`, `dateExpiration`, `estActive`, `numeroLot` (au lieu de `id`, `description`, `tauxReduction`, `dateFin`) |
| `features/lots/lot-detail/lot-detail.component.ts` | Ajout de `getFormattedPromotion()` : formate la promo active en `CODE — −X%  · dd/MM/yy → dd/MM/yy`. Gère les deux casings (PascalCase et camelCase) |
| `features/lots/lot-detail/lot-detail.component.html` | Utilise `getFormattedPromotion()` dans la grille de détail |

---

## 5. Correction de la création de promotion depuis le détail de lot

### Problème
Le formulaire de promotion dans le détail lot envoyait une requête POST vers `/product/promos` (sans `s`) → 404. De plus, les champs du formulaire (`Nom`, `DateFin`, `IsActive`) ne correspondaient pas au `PromotionDto` backend.

### Fichiers modifiés
| Fichier | Modification |
|---|---|
| `features/products/services/promotion-advanced.service.ts` | Toutes les URLs corrigées : `/product/promos` → `/products/promos` |
| `features/lots/lot-detail/lot-detail.component.ts` | Formulaire renommé : `Nom` → `codePromo`, `DateFin` → `dateExpiration`, `IsActive` → `estActive`, `Pourcentage` → `pourcentage`. Payload aligné sur le `PromotionDto` C# |
| `features/lots/lot-detail/lot-detail.component.html` | Labels et `formControlName` mis à jour pour correspondre aux nouveaux noms de champs |

---

## 6. Correction du service Promotion (détail vide)

### Problème
Le détail d'une promotion restait vide malgré une réponse 200 OK. Problème de détection de changement Angular + noms de propriétés potentiellement incohérents.

### Fichiers modifiés
| Fichier | Modification |
|---|---|
| `features/promotions/services/promotion.service.ts` | Ajout de `normalize()` : gère PascalCase ET camelCase pour tous les champs. Appliqué dans `getAll()` et `getById()` |
| `features/promotions/promotion-detail/promotion-detail.component.ts` | Ajout de `ChangeDetectorRef` + `markForCheck()` pour forcer la mise à jour de la vue après chargement asynchrone |

---

## 7. Refonte complète du module Orders (commandes & réclamations)

### Contexte
Analyse complète du backend `OrderAPI` (port 7004) pour aligner le frontend sur les vrais contrats.

**Découvertes clés du backend :**
- Pas de `JsonNamingPolicy.CamelCase` dans `Program.cs` → sérialisation par défaut ASP.NET Core
- `EtatCommande` : enum 0=Brouillon, 1=EnAttente, 2=Validée, 3=Expédiée, 4=Livrée, 5=Annulée
- `StatutReclamation` : enum 0=Ouverte, 1=EnCours, 2=Résolue
- `GET /orders/reclamations` retourne un **tableau direct** (pas de `ResponseDto`)
- `POST /orders` (créer commande) : rôle **CLIENT uniquement** — `Id_Client` injecté depuis le JWT côté serveur
- Mise à jour statut : `PUT /orders/status` avec `{Id_Commande, NouveauStatut: int}`

### 7.1 Services réécrits

| Fichier | Modification |
|---|---|
| `features/orders/order.service.ts` | Réécriture complète. `EtatCommande` enum + labels + classes CSS. `CommandeDto` et `LigneCommandeDto` avec vrais noms de champs. `getOrders(page, pageSize)`, `getOrderById()`, `getOrdersByClient()`, `updateOrderStatus()`, `getNextStatuses()` pour le workflow. `normalizeOrder()` + `normalizeLigne()` |
| `features/orders/services/reclamation.service.ts` | Réécriture complète. `ReclamationDto` avec `Id_Rec`, `Id_Commande`, `Id_Ligne`. Gestion du tableau direct de `getAll()`. 404 → tableau vide pour `getByOrder()`/`getByClient()`. `normalizeRec()` pour double casing |
| `features/orders/services/ligne.service.ts` | Correction du DTO : `CreateOrUpdateLigneDto` avec `Id_Commande`, `Id_Produit`, `Id_Ligne`, `Quantite`, `Remise`, `PrixUnitaire` (PascalCase) |

### 7.2 Composants réécrits

| Fichier | Modification |
|---|---|
| `features/orders/order-list/order-list.component.ts` | Pagination serveur (page/pageSize). Gestion d'erreurs explicite (401, 403, 0, 5xx). **Modal de confirmation de suppression** (remplace `confirm()`). Rôle admin pour le bouton supprimer |
| `features/orders/order-list/order-list.component.html` | Tableau avec colonnes : N°, Date, Client, Lignes, HT, TTC, Statut, Actions. Message d'erreur + bouton "Réessayer". Modal de suppression avec détails de la commande |
| `features/orders/order-list/order-list.component.css` | Styles complets : tableau, chips de statut, pagination, modal de confirmation |
| `features/orders/order-detail/order-detail.component.ts` | 3 onglets : Informations / Lignes / Réclamations. **Workflow de statut** : seules les transitions valides sont proposées (ex. Brouillon → EnAttente ou Annulée). **Modal de confirmation de suppression**. Chargement des réclamations de la commande |
| `features/orders/order-detail/order-detail.component.html` | KPIs (HT, TTC, nb lignes, nb réclamations). Onglets. Tableau de lignes avec sous-total calculé. Réclamations inline |
| `features/orders/order-detail/order-detail.component.css` | Styles complets |
| `features/orders/order-form/order-form.component.ts` | Formulaire dynamique avec `FormArray` pour les lignes. Case `IsFinalValidation` (brouillon vs en attente). Note : création réservée au rôle CLIENT |
| `features/orders/order-form/order-form.component.html` | Ajout/suppression dynamique de lignes. Champs : ID produit, quantité, prix unitaire, remise |
| `features/orders/reclamations/reclamation-list/reclamation-list.component.ts` | Gestion des rôles (ADMIN/SUPERVISEUR peuvent changer le statut). Filtre par commande ou client via queryParams. Gestion d'erreurs + retry |
| `features/orders/reclamations/reclamation-list/reclamation-list.component.html` | Tableau avec lien vers la commande parente. **Bouton "Voir" avec `[routerLink]`** (plus fiable que `(click)`). Dropdown inline de changement de statut pour ADMIN/SUPERVISEUR |
| `features/orders/reclamations/reclamation-detail/reclamation-detail.component.ts` | Utilise `Id_Rec` (au lieu de `idReclamation`) |
| `features/orders/reclamations/reclamation-detail/reclamation-detail.component.html` | Affiche `Id_Rec`, `Id_Commande`, `Id_Ligne`, `Id_Client`, `Message`, `Statut` avec vrais noms de champs |
| `features/orders/reclamations/reclamation-form/reclamation-form.component.ts` | Champs `Id_Commande` et `Id_Ligne` (PascalCase). Note informative : `Id_Client` injecté depuis le JWT côté serveur |
| `features/orders/reclamations/reclamation-form/reclamation-form.component.html` | Formulaire avec les vrais champs backend |

---

## 8. Correction de la liste vide + IDs `undefined`

### Problème principal
Les listes commandes et réclamations affichaient des données avec tous les IDs à `undefined`, ce qui causait :
- Les boutons "Voir" ne naviguaient nulle part
- Les suppressions envoyaient `DELETE /orders/undefined` ou `DELETE /orders/reclamations/undefined`

### Cause
ASP.NET Core sérialise par défaut en camelCase (première lettre en minuscule) :
| Propriété C# | JSON reçu par Angular |
|---|---|
| `Id_Commande` | `id_Commande` |
| `Id_Rec` | `id_Rec` |
| `MontantTTC` | `montantTTC` |

Le frontend lisait `order.Id_Commande` (majuscule) → `undefined`.

### Fichiers modifiés
| Fichier | Modification |
|---|---|
| `features/orders/order.service.ts` | `normalizeOrder()` : teste `Id_Commande` OU `id_Commande` OU `idCommande` → prend le premier non-null. Idem pour tous les champs. Appliqué dans `getOrders()`, `getOrderById()`, `getOrdersByClient()` |
| `features/orders/services/reclamation.service.ts` | `normalizeRec()` : même approche pour `Id_Rec`, `Id_Commande`, `Id_Ligne`, `Id_Client`, `Statut` |

### Correction Ocelot (Gateway)
| Fichier | Modification |
|---|---|
| `CynapCRM.Gateway/ocelot.json` | Ajout de 2 routes explicites : `GET /orders` → `/api/orders` (liste paginée) et `GET /orders/reclamations` → `/api/reclamations` (liste toutes réclamations). Nécessaire car `{everything}` ne matche pas le chemin vide |

> ⚠️ **Redémarrer la Gateway** (`CynapCRM.Gateway`) pour que les nouvelles routes Ocelot prennent effet.

---

## 9. Corrections diverses de l'interface

### Filtres & pagination
| Fichier | Modification |
|---|---|
| `features/products/product-list/product-list.component.ts` | Méthode `applyFilters()` remet la page à 1 lors d'un changement de filtre |
| `features/users/user-list/user-list.component.ts` | Ajout de la **pagination** (10 utilisateurs par page) : `currentPage`, `paginatedUsers`, `pageNumbers`, `onPageChange()` |
| `features/users/user-list/user-list.component.html` | Barre de pagination en bas du tableau |
| `features/users/user-list/user-list.component.css` | Styles `.ul-pagination`, `.pag-btn`, `.pag-active` |

### Suppression de l'import `catchError(() => of([]))`
Dans les services commandes et réclamations, les `catchError` qui retournaient silencieusement des tableaux vides en cas d'erreur ont été supprimés. Les erreurs remontent maintenant au composant qui affiche un message explicite + bouton "Réessayer".

---

## Résumé des fichiers backend modifiés

| Fichier | Modification |
|---|---|
| `CynapCRM.Gateway/ocelot.json` | +2 routes : `GET /orders` et `GET /orders/reclamations` |

> Tous les autres changements sont **uniquement frontend** (dossier `Cynapharm/`).

---

## Points d'attention pour les tests

1. **Redémarrer la Gateway** après modification de `ocelot.json`
2. **Rôles requis** pour accéder aux endpoints Orders :
   - Lecture commandes : `ADMIN`, `SUPERVISEUR`, `DELEGUE`
   - Création commande : `CLIENT` uniquement
   - Suppression commande : `ADMIN` uniquement
   - Lecture/modification statut réclamation : `ADMIN`, `SUPERVISEUR`
3. La création de commande via le formulaire ne fonctionnera qu'avec un compte de rôle `CLIENT`
4. Le workflow de statut respecte les transitions valides : `Brouillon → EnAttente → Validée → Expédiée → Livrée` (annulation possible à toutes les étapes)
