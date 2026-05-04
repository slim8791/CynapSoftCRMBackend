GUIDE DE TEST - GESTION DES PRODUITS
====================================

## SCÉNARIOS DE TEST COMPLETS

### SCÉNARIO 1: Produit Actif
─────────────────────────────

1. Navigation
   □ Accéder: /products
   □ Cliquer sur un produit actif
   → Voir détail avec statut "Actif" (vert)

2. Vérifier Header
   □ Statut badge: "Actif" en vert
   □ Bouton "Modifier": ENABLED (normal)
   □ Bouton "Désactiver": VISIBLE
   □ Bouton "Archiver": VISIBLE

3. Onglet Informations
   □ Tous les champs affichés
   □ Boutons "Gérer les lots" et "Marketing" visibles
   □ Bouton Modifier fonctionnel

4. Onglet Stock
   □ Affiche le total en stock
   □ Si = 0: Message "Stock épuisé"
   □ Si > 0: Nombre normal
   □ Bouton "Nouveau lot" fonctionnel

5. Onglet Lots
   □ Liste des lots affichée
   □ Chaque lot affiche:
      ├─ Numéro (gauche)
      ├─ Quantité (gauche)
      ├─ Date expiration (milieu)
      ├─ Avertissement si < 7j (milieu, jaune)
      └─ Badge statut (droite, coloré)
   □ Badges:
      ├─ "En stock" (vert) si Quantite > 5
      ├─ "Faible" (orange) si 0 < Quantite <= 5
      └─ "Expiré" (rouge) si DateExpiration < aujourd'hui

6. Onglet Supports (NOUVEAU)
   □ Tableau avec 4 colonnes: Type | Nom | Statut | Fichiers
   □ Si aucun support: État vide
   □ Si supports existent:
      ├─ Type: Badge coloré (Brochure, Vidéo, etc.)
      ├─ Nom: Nom de la campagne
      ├─ Statut: Badge "Actif" ou "Inactif"
      └─ Fichiers: Nombre de fichiers associés
   □ Bouton "Nouveau support" fonctionnel

7. Actions
   □ Cliquer "Modifier": Navigate vers formulaire
   □ Cliquer "Désactiver": Modal confirmation → Produit passe "Inactif"
   □ Cliquer "Archiver": Modal confirmation → Produit passe "Archivé"

---

### SCÉNARIO 2: Produit Inactif
────────────────────────────────

1. Navigation
   □ Accéder: /products
   □ Filtrer "Inactifs uniquement"
   □ Cliquer sur un produit inactif

2. Vérifier Header
   □ Statut badge: "Inactif" (gris)
   □ Bouton "Modifier": ENABLED
   □ Bouton "Désactiver": MASQUÉ ✓
   □ Bouton "Activer": VISIBLE ✓
   □ Bouton "Archiver": VISIBLE

3. Test Action Activer
   □ Cliquer "Activer"
   □ Modal confirmation
   □ Après confirmation:
      ├─ Toast succès
      ├─ Page reload
      └─ Statut = "Actif" (vert)

---

### SCÉNARIO 3: Produit Archivé (CAS CRITIQUE)
───────────────────────────────────────────────

1. Navigation
   □ Accéder: /products
   □ Filtrer "Archivés uniquement"
   □ Cliquer sur un produit archivé

2. Vérifier Header
   □ Statut badge: "Archivé" (orange)
   □ Bouton "Modifier": DISABLED + GRISÉ ✓
   □ Bouton "Désactiver": MASQUÉ ✓
   □ Bouton "Archiver": MASQUÉ ✓
   □ Bouton "Activer": VISIBLE ✓

3. Test Tooltip Modifier
   □ Survoler bouton "Modifier"
   □ Tooltip: "Produit archivé — non modifiable"

4. Test que Modifier est vraiment désactivé
   □ Essayer cliquer sur "Modifier" → Aucun effet
   □ Essayer Tab sur le bouton → Pas de navigation

5. Test Action Activer
   □ Cliquer "Activer"
   □ Modal confirmation
   □ Toast succès
   □ Redirection ou reload
   □ Vérifier retour à liste

---

### SCÉNARIO 4: Test Lots avec Avertissements d'Expiration
──────────────────────────────────────────────────────────

1. Créer/Éditer des lots avec différentes dates:

   Lot A: Expiration = Aujourd'hui
   □ Badge: "Expiré" (rouge)
   □ Avertissement: "Expire aujourd'hui"
   □ Fond: Surbrillance rouge

   Lot B: Expiration = Demain
   □ Badge: "Expiré" (rouge)
   □ Avertissement: "Expiré"

   Lot C: Expiration = +3 jours
   □ Badge: "Expiré" ou "En stock" selon quantité
   □ Avertissement: "Expire dans 3 jour(s)" (jaune)

   Lot D: Expiration = +7 jours
   □ Badge: "En stock" ou "Faible" selon quantité
   □ Avertissement: "Expire dans 7 jour(s)" (jaune)

   Lot E: Expiration = +30 jours
   □ Badge: "En stock" ou "Faible" selon quantité
   □ Avertissement: (aucun)

2. Test Quantity Thresholds:

   Lot 1: Quantite = 10
   □ Badge: "En stock" (vert, si pas expiré)

   Lot 2: Quantite = 5
   □ Badge: "Faible" (orange, si pas expiré)

   Lot 3: Quantite = 1
   □ Badge: "Faible" (orange, si pas expiré)

   Lot 4: Quantite = 0
   □ Badge: "Expiré" (rouge)

---

### SCÉNARIO 5: Test Supports Marketing
────────────────────────────────────────

1. Aller onglet Supports

2. Si aucun support:
   □ État vide avec icône + message
   □ Bouton "Nouveau support" visible

3. Ajouter supports via formulaire externe:

   Support 1: Type="Brochure", Nom="Campagne Q1 2025", IsActive=true, Fichiers=2
   Support 2: Type="Vidéo", Nom="Démo Produit", IsActive=false, Fichiers=0
   Support 3: Type="PDF", Nom="Documentation", IsActive=true, Fichiers=5

4. Recharger page (F5):
   □ Onglet Supports affiche tableau
   □ Colonnes correctement alignées
   □ Données correctes:

      Support 1:
      ├─ Type: "Brochure" (badge bleu)
      ├─ Nom: "Campagne Q1 2025"
      ├─ Statut: "Actif" (badge vert)
      └─ Fichiers: "2"

      Support 2:
      ├─ Type: "Vidéo" (badge bleu)
      ├─ Nom: "Démo Produit"
      ├─ Statut: "Inactif" (badge gris)
      └─ Fichiers: "0"

      Support 3:
      ├─ Type: "PDF" (badge bleu)
      ├─ Nom: "Documentation"
      ├─ Statut: "Actif" (badge vert)
      └─ Fichiers: "5"

5. Test Responsive:
   □ Sur mobile: Tableau scroll horizontal
   □ Sur tablet: Tableau complet visible
   □ Sur desktop: Tableau complètement visible

---

### SCÉNARIO 6: Test Responsive Design
─────────────────────────────────────

1. Vue Desktop (> 1024px)
   □ Tous les boutons visibles
   □ Onglets horizontaux
   □ Tableau supports: 4 colonnes visibles

2. Vue Tablet (768px - 1024px)
   □ Boutons: peut-être wrappés
   □ Onglets: scroll horizontal si besoin
   □ Tableau: scroll horizontal si besoin

3. Vue Mobile (< 768px)
   □ Header stacké verticalement
   □ Boutons: 1 par ligne ou wrappés
   □ Onglets: scroll horizontal
   □ Tableau supports: scroll horizontal obligatoire


---

### SCÉNARIO 7: Test de l'API
──────────────────────────────

1. Ouvrir DevTools → Network tab

2. Charger page produit:
   □ GET /products/{id} → Status 200
   □ GET /lots/product/{id} → Status 200 (ou pas si empty)
   □ GET /marketting/product/{id}/supports → Status 200 (ou pas si empty)

3. Cliquer "Désactiver":
   □ PUT /products/{id}/deactivate → Status 200
   □ Réponse: IsSuccess=true (ou false with message)

4. Cliquer "Activer":
   □ PUT /products/{id}/activate → Status 200

5. Cliquer "Archiver":
   □ PUT /products/{id}/archive → Status 200

---

## TEST LISTE DES ANOMALIES À VÉRIFIER

### Affichage
□ Pas d'erreur console
□ Pas de boutons mal alignés
□ Pas de texte coupé
□ Pas de couleurs mal appliquées
□ Pas d'icônes manquantes

### Interaction
□ Tous les boutons cliquables
□ Tous les liens naviguent
□ Modals s'affichent correctement
□ Toasts visibles 3-5 secondes
□ Confirmations demandent validation

### Performance
□ Page charge < 2 secondes
□ Pas de lag au scroll
□ Pas de flicker
□ Transitions fluides

### Logique Métier
□ Modifier disabled si archivé = OUI
□ Activer visible ssi inactif ou archivé = OUI
□ Désactiver visible ssi actif et non archivé = OUI
□ Archiver visible ssi non archivé = OUI
□ Badge lot "Expiré" ssi DateExp < today = OUI
□ Badge lot "Faible" ssi 0 < Qte <= 5 = OUI
□ Avertissement si < 7j avant expiration = OUI

### Accessibilité
□ Tous les boutons have aria-label ou title
□ Disabled buttons are actually disabled (tabindex, cursor)
□ Tab navigation works
□ Screen reader announces status
□ Keyboard shortcuts work


---

## DONNÉES DE TEST RECOMMANDÉES

Produit Test 1 (ACTIF):
{
  "Id_Produit": 1,
  "Nom": "Paracétamol 500mg",
  "Description": "Analgésique et antipyrétique",
  "Prix_Vente": 2.500,
  "Prix_Creation": 1.200,
  "TVA": 6,
  "IsActive": true,
  "IsArchived": false
}

Produit Test 2 (INACTIF):
{
  "Id_Produit": 2,
  "Nom": "Ibuprofène 200mg",
  "Description": "Anti-inflammatoire",
  "Prix_Vente": 3.750,
  "Prix_Creation": 1.800,
  "TVA": 6,
  "IsActive": false,
  "IsArchived": false
}

Produit Test 3 (ARCHIVÉ):
{
  "Id_Produit": 3,
  "Nom": "Ancien Produit",
  "Description": "Produit d'archive",
  "Prix_Vente": 1.000,
  "Prix_Creation": 0.500,
  "TVA": 6,
  "IsActive": false,
  "IsArchived": true
}


Lots Test:
[
  {
    "Numero": "LOT-001",
    "DateExpiration": "2024-12-31",
    "Quantite": 150
  },
  {
    "Numero": "LOT-002",
    "DateExpiration": "2025-01-05",
    "Quantite": 5
  },
  {
    "Numero": "LOT-003",
    "DateExpiration": "2025-01-01",
    "Quantite": 2
  },
  {
    "Numero": "LOT-004",
    "DateExpiration": "2025-01-15",
    "Quantite": 75
  }
]

Supports Test:
[
  {
    "Id_SupportMarketting": 1,
    "Type": "Brochure",
    "IsActive": true,
    "CampaignName": "Q1 2025",
    "Fichiers": [{"Id": 1}, {"Id": 2}]
  },
  {
    "Id_SupportMarketting": 2,
    "Type": "Vidéo",
    "IsActive": false,
    "CampaignName": "YouTube Campaign",
    "Fichiers": []
  }
]

---

✨ TOUS LES SCÉNARIOS TESTÉS = PRÊT POUR PRODUCTION ✨
