# Backend API Authorization Analysis

This document lists all API endpoints across the microservices, detailing their required permissions and roles.

## AuthController (CynapCRM.Services.AuthAPI)
**Base Route:** `api/auth`

**Class Level Authorization:** `None`

| Method | HTTP Verb | Route | Authorization | Access Control Explanation |
|---|---|---|---|---|
| Register | POST | `api/auth/register` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| Login | POST | `api/auth/login` | `None` | No explicit authorization rule found (might inherit global or fail open). |
| SearchUsers | GET | `api/auth/users/search` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| UpdateProfile | PUT | `api/auth/update-profile` | `[Authorize]` | Requires valid authentication token. |
| GetAllUsers | GET | `api/auth/users` | `[Authorize(Roles = "ADMIN")]` | Requires valid token AND user must have one of these roles: **ADMIN**. |
| AssignRole | POST | `api/auth/AssignRole` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| AddRole | PUT | `api/auth/add-role` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| ChangePassword | PUT | `api/auth/change-password` | `[Authorize]` | Requires valid authentication token. |
| ForgotPassword | POST | `api/auth/forgot-password` | `None` | No explicit authorization rule found (might inherit global or fail open). |
| ResetPassword | PUT | `api/auth/reset-password` | `None` | No explicit authorization rule found (might inherit global or fail open). |
| ChangeRole | PUT | `api/auth/change-role` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| EnableUser | PUT | `api/auth/enable-user/{email}` | `[Authorize(Roles = "ADMIN")]` | Requires valid token AND user must have one of these roles: **ADMIN**. |
| DeleteUser | PUT | `api/auth/delete-user/{email}` | `[Authorize(Roles = "ADMIN")]` | Requires valid token AND user must have one of these roles: **ADMIN**. |
| GetUserById | GET | `api/auth/users/{id}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetDisabledUsers | GET | `api/auth/disabled-users` | `[Authorize(Roles = "ADMIN")]` | Requires valid token AND user must have one of these roles: **ADMIN**. |

---

## BonsCommandesController (CynapCRM.Services.DocAPI)
**Base Route:** `api/bons-commandes`

**Class Level Authorization:** `[Authorize]`

| Method | HTTP Verb | Route | Authorization | Access Control Explanation |
|---|---|---|---|---|
| GetAllBonsCommande | GET | `api/bons-commandes` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetBonCommandeById | GET | `api/bons-commandes/{id:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT**. |
| GetBonsCommandeByClient | GET | `api/bons-commandes/client/{idClient:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT**. |
| GetBonsCommandeByCommande | GET | `api/bons-commandes/commande/{idCommande:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT**. |
| GetBonsCommandeByDate | GET | `api/bons-commandes/by-date` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| CreateOrUpdateBonCommande | POST | `api/bons-commandes/createUpdate` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| DeleteBonCommande | DELETE | `api/bons-commandes/{id:int}` | `[Authorize(Roles = "ADMIN")]` | Requires valid token AND user must have one of these roles: **ADMIN**. |

---

## BonsLivraisonsController (CynapCRM.Services.DocAPI)
**Base Route:** `api/bons-livraison`

**Class Level Authorization:** `[Authorize]`

| Method | HTTP Verb | Route | Authorization | Access Control Explanation |
|---|---|---|---|---|
| GetAllBonsLivraison | GET | `api/bons-livraison` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetBonLivraisonById | GET | `api/bons-livraison/{id:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT**. |
| GetBonsLivraisonByClient | GET | `api/bons-livraison/client/{idClient:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT**. |
| GetBonsLivraisonByCommande | GET | `api/bons-livraison/commande/{idCommande:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT**. |
| GetBonsLivraisonByDate | GET | `api/bons-livraison/by-date` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| CreateOrUpdateBonLivraison | POST | `api/bons-livraison/createUpdate` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| DeleteBonLivraison | DELETE | `api/bons-livraison/{id:int}` | `[Authorize(Roles = "ADMIN")]` | Requires valid token AND user must have one of these roles: **ADMIN**. |

---

## DocumentsController (CynapCRM.Services.DocAPI)
**Base Route:** `api/documents`

**Class Level Authorization:** `[Authorize]`

| Method | HTTP Verb | Route | Authorization | Access Control Explanation |
|---|---|---|---|---|
| GetAllDocuments | GET | `api/documents` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetDocumentById | GET | `api/documents/{numeroDoc:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT**. |
| GetDocumentsByClient | GET | `api/documents/client/{idClient:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT**. |
| GetDocumentsByCommande | GET | `api/documents/commande/{idCommande:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT**. |
| GetDocumentsByType | GET | `api/documents/type/{typeDocument}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT**. |
| GetDocumentsByClientAndType | GET | `api/documents/client/{idClient:int}/type/{typeDocument}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT**. |
| CreateUpdateDocument | POST | `api/documents/createUpdate` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| DeleteDocument | DELETE | `api/documents/{numeroDoc:int}` | `[Authorize(Roles = "ADMIN")]` | Requires valid token AND user must have one of these roles: **ADMIN**. |

---

## FacturesController (CynapCRM.Services.DocAPI)
**Base Route:** `api/factures`

**Class Level Authorization:** `[Authorize]`

| Method | HTTP Verb | Route | Authorization | Access Control Explanation |
|---|---|---|---|---|
| GetAllFactures | GET | `api/factures` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetFactureById | GET | `api/factures/{id:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT**. |
| GetFacturesByClient | GET | `api/factures/client/{idClient:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT**. |
| GetFacturesByCommande | GET | `api/factures/commande/{idCommande:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT**. |
| GetFacturesByDate | GET | `api/factures/by-date` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| CreateOrUpdateFacture | POST | `api/factures/createUpdate` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| DeleteFacture | DELETE | `api/factures/{id:int}` | `[Authorize(Roles = "ADMIN")]` | Requires valid token AND user must have one of these roles: **ADMIN**. |

---

## KPIController (CynapCRM.Services.FieldAPI)
**Base Route:** `api/kpi`

**Class Level Authorization:** `[Authorize]`

| Method | HTTP Verb | Route | Authorization | Access Control Explanation |
|---|---|---|---|---|
| GetNombreVisites | GET | `api/kpi/visites-count` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| HasVisiteAtDate | GET | `api/kpi/has-visite` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetHistoriqueActivite | GET | `api/kpi/historique/{idDelegue:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetClientFidelite | GET | `api/kpi/client-fidelite/{idClient:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetPerformance | GET | `api/kpi/performance/{idDelegue:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetPerformanceRate | GET | `api/kpi/performance-rate/{idDelegue:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetTauxConversion | GET | `api/kpi/taux-conversion/{idDelegue:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |

---

## ObjectifController (CynapCRM.Services.FieldAPI)
**Base Route:** `api/objectifs`

**Class Level Authorization:** `[Authorize]`

| Method | HTTP Verb | Route | Authorization | Access Control Explanation |
|---|---|---|---|---|
| GetAllObjectifs | GET | `api/objectifs` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetObjectifById | GET | `api/objectifs/{idObjectif:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetObjectifsByDelegue | GET | `api/objectifs/by-delegue/{idDelegue:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| CreateOrUpdateObjectif | POST | `api/objectifs` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| UpdateObjectifValue | PUT | `api/objectifs/{idObjectif:int}/value` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| DeleteObjectif | DELETE | `api/objectifs/{idObjectif:int}` | `[Authorize(Roles = "ADMIN")]` | Requires valid token AND user must have one of these roles: **ADMIN**. |

---

## PlanningVisiteController (CynapCRM.Services.FieldAPI)
**Base Route:** `api/plannings`

**Class Level Authorization:** `[Authorize]`

| Method | HTTP Verb | Route | Authorization | Access Control Explanation |
|---|---|---|---|---|
| CreateUpdatePlanningVisite | POST | `api/plannings` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetPlanningById | GET | `api/plannings/{idPlanning:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetAllPlannings | GET | `api/plannings` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetPlanningByDelegue | GET | `api/plannings/by-delegue/{idDelegue:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetPlanningsByDateRange | GET | `api/plannings/by-range` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetPlanningByDelegueAndDate | GET | `api/plannings/by-date` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| DeletePlanning | DELETE | `api/plannings/{idPlanning:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| ValidatePlanning | PUT | `api/plannings/{idPlanning:int}/validate` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |

---

## RapportsController (CynapCRM.Services.FieldAPI)
**Base Route:** `api/rapports`

**Class Level Authorization:** `[Authorize]`

| Method | HTTP Verb | Route | Authorization | Access Control Explanation |
|---|---|---|---|---|
| CreateOrUpdateRapport | POST | `api/rapports/createUpdate` | `[Authorize(Roles = "ADMIN,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,DELEGUE**. |
| GetRapportById | GET | `api/rapports/{id:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetRapportByVisiteId | GET | `api/rapports/by-visite/{idVisite:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetRapportsByDelegue | GET | `api/rapports/by-delegue/{idDelegue:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetAllRapports | GET | `api/rapports/all` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| DeleteRapport | DELETE | `api/rapports/{idRapport:int}` | `[Authorize(Roles = "DELEGUE,ADMIN")]` | Requires valid token AND user must have one of these roles: **DELEGUE,ADMIN**. |
| ValidateRapport | PUT | `api/rapports/{idRapport:int}/validate` | `[Authorize(Roles = "SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **SUPERVISEUR**. |
| CanCreateRapport | GET | `api/rapports/can-create/{idVisite:int}` | `[Authorize(Roles = "DELEGUE")]` | Requires valid token AND user must have one of these roles: **DELEGUE**. |
| HasRapport | GET | `api/rapports/has-rapport/{idVisite:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |

---

## RegionController (CynapCRM.Services.FieldAPI)
**Base Route:** `api/regions`

**Class Level Authorization:** `[Authorize]`

| Method | HTTP Verb | Route | Authorization | Access Control Explanation |
|---|---|---|---|---|
| GetAllRegions | GET | `api/regions/all` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| CreateOrUpdateRegion | POST | `api/regions` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetRegionById | GET | `api/regions/{idRegion:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetRegionsByDelegue | GET | `api/regions/by-delegue/{idDelegue:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetNombreRegionsCouvre | GET | `api/regions/count/{idDelegue:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| DeleteRegion | DELETE | `api/regions/{idRegion:int}` | `[Authorize(Roles = "ADMIN")]` | Requires valid token AND user must have one of these roles: **ADMIN**. |

---

## VisitesController (CynapCRM.Services.FieldAPI)
**Base Route:** `api/visites`

**Class Level Authorization:** `[Authorize]`

| Method | HTTP Verb | Route | Authorization | Access Control Explanation |
|---|---|---|---|---|
| CreateOrUpdateVisite | POST | `api/visites` | `[Authorize(Roles = "DELEGUE,ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **DELEGUE,ADMIN,SUPERVISEUR**. |
| GetVisiteById | GET | `api/visites/{idVisite:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetVisitesByDelegueId | GET | `api/visites/by-delegue/{idDelegue:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetVisitesByPlanning | GET | `api/visites/by-planning/{idPlanning:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetAllVisites | GET | `api/visites` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| DeleteVisite | DELETE | `api/visites/{idVisite:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| AffectVisiteToPlanning | PUT | `api/visites/{idVisite:int}/planning/{idPlanning:int}` | `[Authorize(Roles = "DELEGUE")]` | Requires valid token AND user must have one of these roles: **DELEGUE**. |
| CompleteVisite | PUT | `api/visites/{idVisite:int}/complete` | `[Authorize(Roles = "DELEGUE")]` | Requires valid token AND user must have one of these roles: **DELEGUE**. |

---

## DistributionController (CynapCRM.Services.InventoryAPI)
**Base Route:** `api/distributions`

**Class Level Authorization:** `[Authorize]`

| Method | HTTP Verb | Route | Authorization | Access Control Explanation |
|---|---|---|---|---|
| CreateOrUpdateDistribution | POST | `api/distributions` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetAllDistributions | GET | `api/distributions` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetDistributionById | GET | `api/distributions/{idDistribution:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")] // FIX: ajout restriction` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetDistributionsByMedecin | GET | `api/distributions/by-medecin/{idMedecin:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetDistributionsByDelegue | GET | `api/distributions/by-delegue/{idDelegue:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetDistributionsByPharmacien | GET | `api/distributions/by-pharmacien/{idPharmacien:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| DeleteDistribution | DELETE | `api/distributions/{idDistribution:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |

---

## InventoryBusinessController (CynapCRM.Services.InventoryAPI)
**Base Route:** `api/inventory-business`

**Class Level Authorization:** `[Authorize]`

| Method | HTTP Verb | Route | Authorization | Access Control Explanation |
|---|---|---|---|---|
| CheckStockAvailability | GET | `api/inventory-business/check-availability` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| DistributeEchantillon | POST | `api/inventory-business/distribute-echantillon` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| ApplyGratuite | POST | `api/inventory-business/apply-gratuite` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| ReserveStock | POST | `api/inventory-business/reserve-stock` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetStockSummary | GET | `api/inventory-business/summary/{idDelegue:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |

---

## StockMovementController (CynapCRM.Services.InventoryAPI)
**Base Route:** `api/stock-movements`

**Class Level Authorization:** `None`

| Method | HTTP Verb | Route | Authorization | Access Control Explanation |
|---|---|---|---|---|
| DecrementStock | POST | `api/stock-movements/decrement` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| IncrementStock | POST | `api/stock-movements/increment` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| TransferStock | POST | `api/stock-movements/transfer` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetStockMovements | GET | `api/stock-movements/{idStock:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetMovementsByDelegue | GET | `api/stock-movements/by-delegue/{idDelegue:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |

---

## StockPromotionnelController (CynapCRM.Services.InventoryAPI)
**Base Route:** `api/stocks-promotionnels`

**Class Level Authorization:** `[Authorize]`

| Method | HTTP Verb | Route | Authorization | Access Control Explanation |
|---|---|---|---|---|
| CreateOrUpdateGratuite | POST | `api/stocks-promotionnels/gratuite` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetAllGratuite | GET | `api/stocks-promotionnels/gratuite` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetStockGratuiteById | GET | `api/stocks-promotionnels/gratuite/{idStock:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| CreateOrUpdateEchantillonStock | POST | `api/stocks-promotionnels/echantillon` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetAllEchantillon | GET | `api/stocks-promotionnels/echantillon` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetStockEchantillonById | GET | `api/stocks-promotionnels/echantillon/{idStock:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |

---

## StocksDelegueController (CynapCRM.Services.InventoryAPI)
**Base Route:** `api/stocks-delegue`

**Class Level Authorization:** `[Authorize]`

| Method | HTTP Verb | Route | Authorization | Access Control Explanation |
|---|---|---|---|---|
| GetAllStocks | GET | `api/stocks-delegue` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetStockById | GET | `api/stocks-delegue/{idStock:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetStocksByDelegue | GET | `api/stocks-delegue/by-delegue/{idDelegue:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetStocksByProduit | GET | `api/stocks-delegue/by-produit/{idProduit:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetStockByLot | GET | `api/stocks-delegue/by-lot/{numeroLot}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| CreateOrUpdateStock | POST | `api/stocks-delegue` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| DeleteStock | DELETE | `api/stocks-delegue/{idStock:int}` | `[Authorize(Roles = "ADMIN")]` | Requires valid token AND user must have one of these roles: **ADMIN**. |

---

## LigneController (CynapCRM.Services.OrderAPI)
**Base Route:** `api/lignes`

**Class Level Authorization:** `[Authorize]`

| Method | HTTP Verb | Route | Authorization | Access Control Explanation |
|---|---|---|---|---|
| CreateOrUpdateLigneCommande | POST | `api/lignes` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT**. |
| DeleteLigneCommande | DELETE | `api/lignes/{ligneId:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT**. |

---

## OrderController (CynapCRM.Services.OrderAPI)
**Base Route:** `api/orders`

**Class Level Authorization:** `[Authorize]`

| Method | HTTP Verb | Route | Authorization | Access Control Explanation |
|---|---|---|---|---|
| GetAllOrders | GET | `api/orders` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetOrderById | GET | `api/orders/{orderId:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT**. |
| GetOrdersByClientId | GET | `api/orders/by-client/{clientId:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT**. |
| GetOrdersByStatus | GET | `api/orders/by-status` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT**. |
| GetOrdersByDateRange | GET | `api/orders/by-date` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetOrdersDashboard | GET | `api/orders/dashboard` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| CreateOrder | POST | `api/orders` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT**. |
| UpdateOrderStatus | PUT | `api/orders/status` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| CancelOrder | PUT | `api/orders/{idCommande:int}/cancel` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,PHARMACIEN,GROSSISTE,CLIENT")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,PHARMACIEN,GROSSISTE,CLIENT**. |
| DeleteOrder | DELETE | `api/orders/{idCommande:int}` | `[Authorize(Roles = "ADMIN")]` | Requires valid token AND user must have one of these roles: **ADMIN**. |

---

## ReclamationController (CynapCRM.Services.OrderAPI)
**Base Route:** `api/reclamations`

**Class Level Authorization:** `[Authorize]`

| Method | HTTP Verb | Route | Authorization | Access Control Explanation |
|---|---|---|---|---|
| GetAllReclamations | GET | `api/reclamations` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetReclamationsByOrder | GET | `api/reclamations/by-commande/{orderId:int}` | `[Authorize]` | Requires valid authentication token. |
| GetReclamationsByClient | GET | `api/reclamations/by-client/{idClient:int}` | `[Authorize]` | Requires valid authentication token. |
| GetReclamationById | GET | `api/reclamations/{idReclamation:int}` | `[Authorize]` | Requires valid authentication token. |
| CreateUpdateReclamation | POST | `api/reclamations` | `[Authorize]` | Requires valid authentication token. |
| UpdateReclamationStatus | PUT | `api/reclamations/{reclamationId:int}/status` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| DeleteReclamation | DELETE | `api/reclamations/{reclamationId:int}` | `[Authorize(Roles = "ADMIN,PHARMACIEN,GROSSISTE,CLIENT")]` | Requires valid token AND user must have one of these roles: **ADMIN,PHARMACIEN,GROSSISTE,CLIENT**. |

---

## LotController (CynapCRM.Services.ProductAPI)
**Base Route:** `api/lots`

**Class Level Authorization:** `[Authorize]`

| Method | HTTP Verb | Route | Authorization | Access Control Explanation |
|---|---|---|---|---|
| GetLotsByIdProduct | GET | `api/lots/{id:int}/lots` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| CreateOrUpdateLot | POST | `api/lots/lot` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| DeleteLot | DELETE | `api/lots/lot/{numeroLot}` | `[Authorize(Roles = "ADMIN")]` | Requires valid token AND user must have one of these roles: **ADMIN**. |
| GetAllLots | GET | `api/lots` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| AdjustStock | PUT | `api/lots/product/{productId}/adjust-stock` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| UpdateLotQuantity | PUT | `api/lots/lot/{numeroLot}/update-quantity` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| IsLotExpired | GET | `api/lots/lot/{numeroLot}/expired` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetLotsNearExpiration | GET | `api/lots/near-expiration` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetLotByNumero | GET | `api/lots/lot/{numeroLot}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetAvailableLots | GET | `api/lots/product/{productId}/available` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| IsLotOutOfStock | GET | `api/lots/lot/{numeroLot}/out-of-stock` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetExpiredLots | GET | `api/lots/expired` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |

---

## MarkettingController (CynapCRM.Services.ProductAPI)
**Base Route:** `api/marketting`

**Class Level Authorization:** `[Authorize]`

| Method | HTTP Verb | Route | Authorization | Access Control Explanation |
|---|---|---|---|---|
| GetSupportsByProduct | GET | `api/marketting/product/{productId}/supports` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetSupportById | GET | `api/marketting/support/{supportId}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| CreateOrUpdateSupport | POST | `api/marketting/support` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| AddFileToSupport | POST | `api/marketting/support/file` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| DeleteFile | DELETE | `api/marketting/file/{fichierId}` | `[Authorize(Roles = "ADMIN")]` | Requires valid token AND user must have one of these roles: **ADMIN**. |
| GetFilesBySupport | GET | `api/marketting/support/{supportId}/files` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| IsSupportActive | GET | `api/marketting/support/{supportId}/active` | `[Authorize]` | Requires valid authentication token. |
| GetVisibleSupportsByProduct | GET | `api/marketting/product/{productId}/visible-supports` | `[Authorize]` | Requires valid authentication token. |
| GetSupportsByCampaign | GET | `api/marketting/campaign/{campaignName}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetCampaigns | GET | `api/marketting/campaigns` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| DisableSupport | PUT | `api/marketting/support/{supportId}/disable` | `[Authorize(Roles = "ADMIN")]` | Requires valid token AND user must have one of these roles: **ADMIN**. |
| ActivateSupport | PUT | `api/marketting/support/{supportId}/activate` | `[Authorize(Roles = "ADMIN")]` | Requires valid token AND user must have one of these roles: **ADMIN**. |

---

## ProductController (CynapCRM.Services.ProductAPI)
**Base Route:** `api/products`

**Class Level Authorization:** `[Authorize]`

| Method | HTTP Verb | Route | Authorization | Access Control Explanation |
|---|---|---|---|---|
| GetAllProducts | GET | `api/products` | `[Authorize]` | Requires valid authentication token. |
| GetProductById | GET | `api/products/{id:int}` | `[Authorize]` | Requires valid authentication token. |
| GetVisibleProducts | GET | `api/products/visible` | `[Authorize]` | Requires valid authentication token. |
| CreateOrUpdateProduct | POST | `api/products` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| ArchiveProduct | PUT | `api/products/{productId:int}/archive` | `[Authorize(Roles = "ADMIN")]` | Requires valid token AND user must have one of these roles: **ADMIN**. |
| UnarchiveProduct | PUT | `api/products/{productId:int}/unarchive` | `[Authorize(Roles = "ADMIN")]` | Requires valid token AND user must have one of these roles: **ADMIN**. |
| ActivateProduct | PUT | `api/products/{productId:int}/activate` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| DeactivateProduct | PUT | `api/products/{id:int}/deactivate` | `[Authorize(Roles = "ADMIN")]` | Requires valid token AND user must have one of these roles: **ADMIN**. |
| DeleteProductPermanently | DELETE | `api/products/{id:int}` | `[Authorize(Roles = "ADMIN")]` | Requires valid token AND user must have one of these roles: **ADMIN**. |
| IsProductAvailable | GET | `api/products/{productId:int}/available` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetAvailableProducts | GET | `api/products/available` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetUnavailableProducts | GET | `api/products/unavailable` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetTotalStock | GET | `api/products/{productId:int}/stock` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetStockStatus | GET | `api/products/stock-status` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetLowStockProducts | GET | `api/products/low-stock` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| SearchProducts | GET | `api/products/search` | `[Authorize]` | Requires valid authentication token. |
| FilterProducts | GET | `api/products/filter` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetCategories | GET | `api/products/categories` | `[Authorize]` | Requires valid authentication token. |
| GetProductsByCategory | GET | `api/products/category/{category}` | `[Authorize]` | Requires valid authentication token. |
| ProductExists | GET | `api/products/exists` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| IsProductValid | GET | `api/products/{productId:int}/valid` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| CanArchiveProduct | GET | `api/products/{productId:int}/can-archive` | `[Authorize(Roles = "ADMIN")]` | Requires valid token AND user must have one of these roles: **ADMIN**. |
| GetTopProducts | GET | `api/products/top` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetProductDashboard | GET | `api/products/dashboard` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetExpiringLots | GET | `api/products/expiring-lots` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetProductsWithActivePromotions | GET | `api/products/with-promotions` | `[Authorize]` | Requires valid authentication token. |

---

## PromoController (CynapCRM.Services.ProductAPI)
**Base Route:** `api/promos`

**Class Level Authorization:** `[Authorize]`

| Method | HTTP Verb | Route | Authorization | Access Control Explanation |
|---|---|---|---|---|
| GetAllPromotions | GET | `api/promos` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetPromotionById | GET | `api/promos/{promotionId:int}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| CreateOrUpdatePromotion | POST | `api/promos` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| DeletePromotion | DELETE | `api/promos/{promotionId:int}` | `[Authorize(Roles = "ADMIN")]` | Requires valid token AND user must have one of these roles: **ADMIN**. |
| ApplyBestPromotion | GET | `api/promos/product/{productId:int}/apply` | `[Authorize]` | Requires valid authentication token. |
| IsProductInPromotion | GET | `api/promos/product/{productId:int}/in-promotion` | `[Authorize]` | Requires valid authentication token. |
| GetPromotionsByProduct | GET | `api/promos/product/{productId:int}` | `[Authorize]` | Requires valid authentication token. |
| GetPromotionsByLot | GET | `api/promos/lot/{numeroLot}` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| IsPromotionValid | GET | `api/promos/{promotionId:int}/valid` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| IsPromotionApplicable | GET | `api/promos/{promotionId:int}/applicable` | `[Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR,DELEGUE**. |
| GetPromotionCoverageRate | GET | `api/promos/coverage-rate` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |
| GetActivePromotionsCount | GET | `api/promos/active-count` | `[Authorize(Roles = "ADMIN,SUPERVISEUR")]` | Requires valid token AND user must have one of these roles: **ADMIN,SUPERVISEUR**. |

---

