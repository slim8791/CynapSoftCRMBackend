# Plan de Contrôle d'Accès - Frontend Cynapharm

## 1. AUTORISATIONS DES RÔLES

### **ADMIN** (Accès complet)
- ✅ Menu Users (voir, créer, éditer, enable/disable)
- ✅ Menu Products (voir, créer, éditer, archiver, désactiver)
- ✅ Menu Dashboard
- ✅ Menu Marketing
- ✅ Menu Orders
- ✅ Menu Lots
- ✅ Tous les boutons d'action

### **SUPERVISEUR**
- ✅ Menu Users (voir uniquement, créer DELEGUE/MEDECIN/CLIENT)
- ✅ Menu Products (voir, créer, éditer, activer)
- ❌ Archiver/Désactiver produits (ADMIN only)
- ✅ Voir stock, disponibilité
- ✅ Menu Dashboard, Marketing, Orders, Lots (si implémentés)

### **DELEGUE**
- ❌ Menu Users (MASQUER)
- ✅ Menu Products (voir uniquement)
- ✅ Voir produits disponibles
- ✅ Menu Dashboard, Orders
- ❌ Boutons créer/éditer produits

### **MEDECIN**
- ❌ Menu Users (MASQUER)
- ❌ Menu Products (MASQUER)
- ✅ Menu Dashboard
- ✅ Menu Orders

### **CLIENT**
- ❌ Menu Users (MASQUER)
- ❌ Menu Products (MASQUER)
- ❌ Menu Dashboard, Marketing, Lots, Orders
- ✅ Profil personnel

---

## 2. IMPLÉMENTATION FRONTEND

### **A. Créer un service d'autorisation**
```
src/app/core/services/authorization.service.ts
```
- Vérifier le rôle de l'utilisateur connecté
- Fournir des méthodes: `canAccess(module)`, `canEdit()`, `canDelete()`, etc.

### **B. Masquer les menus selon les rôles**
```
Mise à jour: src/app/core/layouts/sidebar.component.ts/html
```
- `*ngIf="authService.canAccessUsers()"` → Masquer Users pour DELEGUE/MEDECIN/CLIENT
- `*ngIf="authService.canAccessProducts()"` → Masquer Products pour MEDECIN/CLIENT
- `*ngIf="authService.canAccessOrders()"` → Selon rôle

### **C. Masquer les boutons d'action**
```
Mise à jour: user-list.component.html, product-list.component.html
```
- Créer: `*ngIf="authService.canCreateUsers()"`
- Éditer: `*ngIf="authService.canEditUsers()"`
- Supprimer: `*ngIf="authService.canDeleteUsers()"`
- Archive: `*ngIf="authService.canArchiveProducts()"` (ADMIN only)

### **D. Redirection automatique**
```
Mise à jour: auth.guard.ts
```
- Si utilisateur DELEGUE → /users → Redirection vers /dashboard
- Si utilisateur MEDECIN → /products → Redirection vers /dashboard
- Si utilisateur CLIENT → /admin/* → Redirection vers /profile

### **E. Gestion des pages**
- Créer un **module routing guard** pour vérifier le rôle avant d'afficher une page
- Si l'utilisateur n'a pas les droits → 403 Forbidden

---

## 3. MATRICE DE PERMISSIONS DÉTAILLÉE

| Module/Action | ADMIN | SUPERVISEUR | DELEGUE | MEDECIN | CLIENT |
|---|---|---|---|---|---|
| **USERS** | | | | | |
| Voir liste | ✅ | ✅ | ❌ | ❌ | ❌ |
| Créer | ✅ | ✅ (limité) | ⚠️ | ❌ | ❌ |
| Éditer rôle | ✅ | ✅ | ❌ | ❌ | ❌ |
| Enable/Disable | ✅ | ✅ | ❌ | ❌ | ❌ |
| **PRODUCTS** | | | | | |
| Voir liste | ✅ | ✅ | ✅ | ❌ | ❌ |
| Créer/Éditer | ✅ | ✅ | ❌ | ❌ | ❌ |
| Archive | ✅ | ❌ | ❌ | ❌ | ❌ |
| Désactiver | ✅ | ❌ | ❌ | ❌ | ❌ |
| Voir stock | ✅ | ✅ | ❌ | ❌ | ❌ |
| **DASHBOARD** | ✅ | ✅ | ✅ | ✅ | ❌ |
| **MARKETING** | ✅ | ✅ | ❌ | ❌ | ❌ |
| **ORDERS** | ✅ | ✅ | ✅ | ❌ | ❌ |
| **LOTS** | ✅ | ✅ | ❌ | ❌ | ❌ |

---

## 4. TÂCHES À FAIRE

1. ✅ **Créer `authorization.service.ts`**
2. ✅ **Créer `authorization.guard.ts`** (Route guard)
3. ✅ **Mettre à jour sidebar** - Masquer menus selon rôle
4. ✅ **Mettre à jour user-list** - Masquer/Afficher boutons
5. ✅ **Mettre à jour product-list** - Masquer Archive/Désactiver
6. ✅ **Mettre à jour user-form** - Valider les droits de création
7. ✅ **Redirection 403** - Créer page d'erreur 403
8. ✅ **Mettre à jour routing module** - Ajouter guards sur les routes

---

## 5. PRIORITÉ D'IMPLÉMENTATION

1. **URGENT**: Masquer menu Users pour DELEGUE/MEDECIN/CLIENT
2. **URGENT**: Masquer boutons archive/désactiver produits pour non-ADMIN
3. **IMPORTANT**: Mettre en place le route guard
4. **IMPORTANT**: Redirection intelligente au login selon rôle
5. **NORMAL**: Page 403 custom

