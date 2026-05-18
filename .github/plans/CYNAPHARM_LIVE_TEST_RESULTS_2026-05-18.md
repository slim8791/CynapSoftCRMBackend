# CYNAPHARM BACKEND TEST REPORT
**Date :** 2026-05-18  
**Gateway :** http://cynapharmgateway.runasp.net  
**Admin :** benjdidiaahmed@gmail.com  
**Status gateway :** LIVE — routing confirmé

---

## Résumé global

| Service | Total | ✅ Pass | ❌ Fail | 🚫 Blocked |
|---------|-------|---------|---------|-----------|
| AUTH | 7 | 5 | 2 | 0 |
| PROD | 9 | 4 | 4 | 1 |
| FIELD | 10 | 4 | 6 | 0 |
| DOC | 6 | 1 | 5 | 0 |
| INV | 7 | 0 | 5 | 2 |
| ORD | 10 | 2 | 8 | 0 |
| **Total** | **49** | **16** | **30** | **3** |

---

## AUTH TESTS

| Test | Résultat | Notes |
|------|----------|-------|
| AUTH-01 Register delegue | ✅ PASS | DELEGUE_ID=20 |
| AUTH-02 Register client (pharmacie) | ✅ PASS | CLIENT_ID=21 |
| AUTH-03 Login delegue | ✅ PASS | DELEGUE_TOKEN obtenu |
| AUTH-04 Login client | ✅ PASS | CLIENT_TOKEN obtenu |
| AUTH-05 GET /auth/users | ❌ FAIL | Retourne les users avec `IsDeleted=true` — filtre non appliqué |
| AUTH-06 PUT /auth/update-profile | ❌ FAIL | HTTP 404 — endpoint absent du binaire déployé |
| AUTH-07 PUT /auth/change-password | ✅ PASS | 200 — mot de passe changé |

---

## PRODUCT TESTS

| Test | Résultat | Notes |
|------|----------|-------|
| PROD-01 POST /products (create) | ❌ FAIL | `categorie` non retourné dans la réponse ; `prix_Vente` toujours 0 (mismatch nom de champ) |
| PROD-02 GET /products/{id} | ✅ PASS | Lots et Supports présents |
| PROD-03 GET /products/categories | ❌ FAIL | Retourne les **descriptions** produits au lieu des catégories — champ `categorie` absent de la DB |
| PROD-04 GET /products/filter?category=Antibiotiques | ❌ FAIL | Filtre ignoré — retourne tous les produits |
| PROD-05 GET /products/search?keyword=Amox | ✅ PASS | Amoxicilline trouvée (PRODUCT_ID=21) |
| PROD-06 GET /products/visible | ✅ PASS | 200, liste retournée |
| PROD-07 POST /api/lots | 🚫 BLOCKED | Chemin du test incorrect — chemin réel : `POST /products/lots/lot` |
| PROD-08 GET /products/with-promotions | ❌ FAIL | HTTP 404 — endpoint non déployé |
| PROD-09 GET /products/dashboard | ✅ PASS | TotalProducts=17 |

---

## FIELD TESTS

| Test | Résultat | Notes |
|------|----------|-------|
| FIELD-01 POST /fields/regions | ✅ PASS | REGION_ID=1001 |
| FIELD-02 POST /fields/plannings | ✅ PASS | PLANNING_ID=5 |
| FIELD-03 POST /fields/visites | ✅ PASS | `type` doit être `int 2` et non `"PHARMACIEN"` — VISITE_ID=27 |
| FIELD-04 POST /fields/rapports/createUpdate | ✅ PASS | RAPPORT_ID=4 — champ `date` retourne `0001-01-01` (bug AutoMapper connu) |
| FIELD-05 PUT /fields/visites/{id}/complete | ✅ PASS | 200 — visite complétée |
| FIELD-06 POST /fields/objectifs | ❌ FAIL | `type:0, periode:0` rejetés — l'API déployée refuse les enum à valeur 0 |
| FIELD-07 GET /fields/kpi/performance/{id} | ❌ FAIL | HTTP 403 — rôle DELEGUE absent de `[Authorize]` sur le contrôleur déployé |
| FIELD-08 GET /fields/kpi/taux-conversion/{id} | ❌ FAIL | HTTP 404 — endpoint non déployé |
| FIELD-09 GET /fields/visites (root GET) | ❌ FAIL | HTTP 405 — route racine non exposée via Ocelot (wildcard `{everything}` ne correspond pas) |
| FIELD-10 GET /fields/plannings (root GET) | ❌ FAIL | HTTP 405 — même problème Ocelot que FIELD-09 |

> **Note :** Les sous-chemins fonctionnent (`/fields/visites/by-delegue/20` → 200).

---

## DOC TESTS

| Test | Résultat | Notes |
|------|----------|-------|
| DOC-01 POST /documents/createUpdate | ❌ FAIL | HTTP 405 — POST bloqué par le gateway sur ce chemin |
| DOC-02 GET /documents/client/{id}/type/FACTURE | ❌ FAIL | HTTP 404 — wildcard `{everything}` Ocelot ne matche pas les chemins multi-segments |
| DOC-03 POST /documents/factures/createUpdate | ✅ PASS | FACTURE_ID=6, montantHT=1000, montantTTC=1190 |
| DOC-04 GET /documents/factures/client/{id} (CLIENT_TOKEN) | ❌ FAIL | HTTP 403 — rôle CLIENT absent de `[Authorize]` sur l'endpoint déployé |
| DOC-05 GET /documents/bons-livraison/commande/1 | ❌ FAIL | HTTP 404 — endpoint non déployé |
| DOC-06 DELETE /documents/factures/{id} | ❌ FAIL | HTTP 405 — DELETE bloqué par le gateway |

---

## INVENTORY TESTS

| Test | Résultat | Notes |
|------|----------|-------|
| INV-01 POST /inventory/stocks-delegue | ❌ FAIL | HTTP 405 — POST sur chemin racine bloqué par Ocelot |
| INV-02 POST /inventory/stocks-delegue (update) | 🚫 BLOCKED | Pas de STOCK_ID (INV-01 échoué) |
| INV-03 POST distribute-echantillon | 🚫 BLOCKED | Pas de stock de test créé |
| INV-04 GET /inventory/distributions/by-delegue/{id} | ❌ FAIL | Message d'erreur incorrect : dit "médecin" au lieu de "délégué" (copier-coller) |
| INV-05 GET /inventory/inventory-business/summary/{id} | ❌ FAIL | HTTP 404 — endpoint non déployé |
| INV-06 GET /inventory/stock-movements/by-delegue/{id} | ❌ FAIL | HTTP 404 — endpoint non déployé |
| INV-07 DELETE /inventory/stocks-delegue/{id}?type=0 | ❌ FAIL | `type=0` échoue la validation du modèle ; aucun stock de test disponible |

---

## ORDER TESTS

| Test | Résultat | Notes |
|------|----------|-------|
| ORD-01 POST /orders (create) | ✅ PASS | ORDER_ID=21, Statut=Brouillon — mais `result=null` dans la réponse |
| ORD-02 PUT /orders/status (→EnAttente=1) | ✅ PASS | 200 |
| ORD-03 PUT /orders/status (→5, transition invalide) | ❌ FAIL | HTTP 200 au lieu de 400 — machine d'état non appliquée dans le déployé |
| ORD-04 PUT /orders/status (→Confirmee=2) | ❌ FAIL | HTTP 404 — commande en état terminal suite à ORD-03 |
| ORD-05 GET /orders/by-client/{id} (CLIENT_TOKEN) | ❌ FAIL | HTTP 403 — rôle CLIENT absent de `[Authorize]` |
| ORD-06 GET /orders/by-status?statut=2 | ❌ FAIL | HTTP 404 — endpoint non déployé |
| ORD-07 PUT /orders/{id}/cancel | ❌ FAIL | HTTP 404 — endpoint non déployé |
| ORD-08 POST /orders/reclamations | ✅ PASS | REC_ID=2, Statut=Ouverte — `Statut` doit être string, pas int |
| ORD-09 PUT /orders/reclamations/{id}/status (→2, invalide) | ❌ FAIL | HTTP 404 au lieu de 400 — fix non déployé |
| ORD-10 GET /orders/dashboard | ❌ FAIL | HTTP 404 — endpoint non déployé |

---

## Causes racines

### Cause 1 — Déploiement obsolète (~18 échecs)

Les endpoints suivants existent dans le code source mais sont absents du binaire en production :

| Endpoint | Microservice |
|----------|-------------|
| `PUT /auth/update-profile` | AuthAPI |
| `GET /products/with-promotions` | ProductAPI |
| Logique filtre/catégories produits | ProductAPI |
| `GET /fields/kpi/taux-conversion/{id}` | FieldAPI |
| Rôle DELEGUE sur `/kpi/performance` | FieldAPI |
| `GET /documents/bons-livraison/commande/{id}` | DocAPI |
| `DELETE /documents/factures/{id}` | DocAPI |
| `GET /inventory/inventory-business/summary/{id}` | InventoryAPI |
| `GET /inventory/stock-movements/by-delegue/{id}` | InventoryAPI |
| `GET /orders/by-status` | OrderAPI |
| `PUT /orders/{id}/cancel` | OrderAPI |
| `GET /orders/dashboard` | OrderAPI |
| Machine d'état commandes (transitions) | OrderAPI |
| Validation transitions réclamations | OrderAPI |

### Cause 2 — Lacunes routing Ocelot (~8 échecs)

Le wildcard `/{service}/{everything}` dans `ocelot.json` ne couvre pas :
- Les actions sur le **chemin racine** (`POST /api/stocks-delegue`, `GET /api/visites`) → HTTP 405
- Les **chemins multi-segments** (`/client/{id}/type/{type}`) → HTTP 404
- Certaines méthodes DELETE/POST bloquées sur les chemins de base

---

## Anomalies hors plan de test

| # | Anomalie |
|---|----------|
| 1 | **PROD-01** — `categorie` jamais stocké ni retourné (champ absent de la DB — migration manquante) |
| 2 | **PROD-01** — `prix_Vente` toujours 0 (mismatch `prixVente` vs `prix_Vente` dans le DTO) |
| 3 | **PROD-03** — `/products/categories` retourne des descriptions, pas des catégories |
| 4 | **PROD-04** — Filtre `?category=` entièrement ignoré |
| 5 | **PROD-05** — `GET /products/promos` retourne HTTP 515 + NullReferenceException |
| 6 | **FIELD-04** — `date` du rapport retourne `0001-01-01` (bug AutoMapper `DateRapport` ↔ `Date`) |
| 7 | **FIELD-06** — Enum `type:0` et `periode:0` rejetés par l'API déployée |
| 8 | **ORD-01** — `result=null` sur création de commande (OrderID récupéré via GET uniquement) |
| 9 | **INV-04** — Message d'erreur `by-delegue` dit "médecin" (copier-coller dans le service) |
| 10 | **AUTH-05** — `GET /auth/users` retourne les comptes soft-deleted (`IsDeleted=true`) |

---

## Corrections recommandées (par priorité)

### 🔴 Critique

1. **Redéployer tous les microservices** avec le code source actuel — environ 18 endpoints manquants en production
2. **Corriger le routing Ocelot** — ajouter des routes explicites pour les chemins racines et les chemins multi-segments
3. **Migration `Categorie` sur ProductAPI** — `Add-Migration AddCategorieToProduct` + `Update-Database`
4. **Corriger `AssignRole`** — utilise `FindByEmailAsync` alors que le client envoie un userId numérique
5. **Corriger `ForgotPassword`** — URL hardcodée `http://localhost:4200` — à externaliser dans `appsettings.json`

### 🟡 Moyen

6. **Ajouter rôle CLIENT** sur `GET /orders/by-client`, `GET /factures/client/{id}`, `GET /documents/...`
7. **Ajouter rôle DELEGUE** sur `GET /kpi/performance/{id}`
8. **Corriger AutoMapper** — `Rapport_Visite.DateRapport` ↔ `RapportVisiteDto.Date` (ForMember manquant)
9. **Corriger `LotDto.IsExpired` / `IsOutOfStock`** — toujours `false` (aucun ForMember dans MappingConfig)
10. **Corriger message d'erreur** `by-delegue` dans DistributionService (dit "médecin" au lieu de "délégué")
11. **Remplacer HTTP 515 par 500** dans tous les microservices (AuthAPI, ProductAPI, FieldAPI, DocAPI, InventoryAPI)

### 🟢 Mineur

12. **Corriger namespace** de `MappingConfig.cs` dans FieldAPI (`DocAPI` → `FieldAPI`)
13. **Ajouter `ForMember`** pour `VisiteDto.ClientNom` ou supprimer le champ
14. **Corriger `ORD-01`** — assigner `_response.Result` avec le DTO de commande créée
15. **Corriger `AUTH-05`** — filtrer `IsDeleted=true` dans `GetAllUsersAsync()`

---

## Données créées lors des tests

| Entité | ID | Valeur |
|--------|----|--------|
| Delegue | 20 | test.delegue@cynapharm.dz |
| Client (pharmacie) | 21 | pharmacie.test@cynapharm.dz |
| Produit | 21 | Amoxicilline 1g |
| Région | 1001 | Alger Centre |
| Planning | 5 | 2026-06-01 08h-12h |
| Visite | 27 | Type=PHARMACIEN |
| Rapport | 4 | Résultat=POSITIF |
| Facture | 6 | FAC-2026-001, 1190 DZD TTC |
| Commande | 21 | Statut=Annulee (suite ORD-03) |
| Réclamation | 2 | Statut=Ouverte |

---

*Rapport généré le 2026-05-18 — CynapCRM Backend Test Agent*
