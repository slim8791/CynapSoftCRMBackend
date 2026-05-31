# Dashboard Angular — Analyse complète

> Generated: 2026-05-28
> Branch: dev/Mobile-0001
> Files read: dashboard.component.ts, dashboard.component.html, dashboard.component.css,
>   dashboard.service.ts, order-api.service.ts, visite.service.ts,
>   kpi-dashboard.component.ts, kpi-dashboard.component.html, kpi.service.ts,
>   auth.service.ts, app.routes.ts

---

═══════════════════════════════════════
## PARTIE 1 — CE QUI EST AFFICHÉ
═══════════════════════════════════════

## 1.1 Structure visuelle

### Section 1 — En-tête (Header)
- Titre : **"Tableau de bord"** (h1, 28px bold)
- Sous-titre : *"Vue temps réel — commandes & chiffre d'affaires"*
- Bouton **"Actualiser"** (reload) à droite, avec icône SVG animé (spinning) pendant le chargement

### Section 2 — Erreur globale
- Bande rouge avec icône et message d'erreur si `error` est non vide et `loading` est false

### Section 3 — Skeleton loader
- Grille 4 colonnes de cartes grises animées (shimmer) pendant le chargement initial

### Section 4 — KPI Cards (4 cartes fixes)
Grille 4 colonnes, responsive (2 col → 1 col en mobile) :

| # | Label | Valeur affichée | Couleur |
|---|-------|-----------------|---------|
| 1 | Commandes aujourd'hui | `commandesAujourdhui` | Bleu (#0077b6) |
| 2 | Commandes en attente | `commandesEnAttente` | Orange (#d97706) |
| 3 | Chiffre d'affaires total | `caTotal \| currencyTND` | Vert (#059669) |
| 4 | Taux de livraison | `tauxLivraison`% + barre de progression | Violet (#7c3aed) |

### Section 5 — Statistiques des commandes (API dashboard)
Bloc `@if (orderDash)` — visible seulement si `getOrdersDashboard()` retourne des données.
Grille auto-fill (150px min) de 13 mini-cartes :

| Label | Champ source |
|-------|-------------|
| Total | `orderDash.TotalCommandes` |
| En attente | `orderDash.EnAttente` |
| Confirmées | `orderDash.Confirmees` |
| En préparation | `orderDash.EnPreparation` |
| Expédiées | `orderDash.Expediees` |
| Livrées | `orderDash.Livrees` |
| Annulées | `orderDash.Annulees` |
| Total HT (TND) | `orderDash.MontantTotalHT \| number:'1.0-0'` |
| Total TTC (TND) | `orderDash.MontantTotalTTC \| number:'1.0-0'` |
| Réclam. ouvertes | `orderDash.ReclamationsOuvertes` |
| Réclam. en cours | `orderDash.ReclamationsEnCours` |
| Aujourd'hui | `orderDash.CommandesAujourdHui` |
| Ce mois | `orderDash.CommandesCeMois` |

### Section 6 — Graphiques ligne 1 (2 charts côte à côte)
- **Chart gauche** : Barres horizontales — commandes par statut (ApexCharts, type `bar`)
- **Chart droit** : Donut — commandes par statut (ApexCharts, type `donut`, largeur fixe 380px)

### Section 7 — Graphique ligne 2 (plein écran)
- **Area chart** : Volume de commandes — 7 derniers jours (ApexCharts, type `area`, courbe lissée)

---

## 1.2 Rôles et visibilité

Le composant `DashboardComponent` **n'effectue aucune vérification de rôle**.
La route `/dashboard` est protégée uniquement par `authGuard` (authentification), sans `roleGuard`.

| Section | ADMIN | SUPERVISEUR | DÉLÉGUÉ |
|---------|-------|-------------|---------|
| En-tête + Actualiser | ✅ | ✅ | ✅ |
| KPI Cards (4 cartes) | ✅ | ✅ | ✅ (idem ADMIN) |
| Statistiques orderDash | ✅ | ✅ | ✅ (si API répond) |
| Chart barres statuts | ✅ | ✅ | ✅ |
| Chart donut statuts | ✅ | ✅ | ✅ |
| Area chart 7 jours | ✅ | ✅ | ✅ |

**Conclusion** : ADMIN, SUPERVISEUR et DÉLÉGUÉ voient exactement la même chose.
Aucune section n'est filtrée par rôle. Un DÉLÉGUÉ voit les données de TOUS les délégués.

---

═══════════════════════════════════════
## PARTIE 2 — DONNÉES ET APIs
═══════════════════════════════════════

## 2.1 API calls on load (ngOnInit → loadAll)

| # | Méthode | URL | Retourne | Utilisé pour |
|---|---------|-----|----------|--------------|
| 1 | GET | `/orders` | `Commande[]` | KPI cards + 3 charts (computeStats côté front) |
| 2 | GET | `/orders/dashboard` | `OrderDashboardDto` | Section "Statistiques des commandes" (13 mini-cartes) |

> **Note** : `DashboardService` (qui expose `/dashboard`, `/dashboard/metrics`, `/dashboard/recent-activity`) existe mais **n'est jamais utilisé** par le composant.

---

## 2.2 KPI cards

### KPI 1 — Commandes aujourd'hui
- **Label** : "Commandes aujourd'hui"
- **Source** : `stats.countToday` calculé côté front depuis `getAllOrders()` en comparant `dateCommande` au jour courant (ISO date slice 0-10)
- **Format** : entier brut (`{{ commandesAujourdhui }}`)
- **Couleur** : Bleu (#0077b6)
- **Doublon** : `orderDash.CommandesAujourdHui` affiche la même valeur dans la section suivante

### KPI 2 — Commandes en attente
- **Label** : "Commandes en attente"
- **Source** : `stats.countEnAttente` — compte les commandes avec `etat === 'EnAttente'` OU `etatNum === 1`
- **Format** : entier brut (`{{ commandesEnAttente }}`)
- **Couleur** : Orange (#d97706)
- **Doublon** : `orderDash.EnAttente` affiche la même valeur

### KPI 3 — Chiffre d'affaires total
- **Label** : "Chiffre d'affaires total"
- **Source** : `stats.totalCA` — somme des `montantTTC` de toutes les commandes (y compris annulées)
- **Format** : `| currencyTND` (pipe personnalisé)
- **Couleur** : Vert (#059669)
- **Bug** : inclut les commandes annulées dans le CA total

### KPI 4 — Taux de livraison
- **Label** : "Taux de livraison"
- **Source** : `Math.round((stats.countLivrees / stats.totalOrders) * 100)`
- **Format** : `{{ tauxLivraison }}%` + barre de progression CSS
- **Couleur** : Violet (#7c3aed)

---

## 2.3 Charts

| Chart | Bibliothèque | Type | Données |
|-------|-------------|------|---------|
| Barres horizontales | ng-apexcharts | `bar` horizontal | `computeStats().countByStatus` — labels = statuts, data = nombres |
| Donut | ng-apexcharts | `donut` | `computeStats().countByStatus` — mêmes données |
| Area / Courbe | ng-apexcharts | `area` | `computeStats().last7Days` — 7 derniers jours, x=date(MM-DD), y=count |

Tous les charts sont construits **côté front** à partir de la liste brute `/orders`.
Le backend `/orders/dashboard` n'est pas utilisé pour alimenter les charts.

---

═══════════════════════════════════════
## PARTIE 3 — BUGS ET PROBLÈMES
═══════════════════════════════════════

## 3.1 Bugs found

| # | Issue | Impact | Priorité |
|---|-------|--------|----------|
| 1 | **DÉLÉGUÉ voit toutes les commandes** : `getAllOrders()` appelle `/orders` sans filtrer par `idDelegue`. Un DÉLÉGUÉ voit le CA et les commandes de tous les autres. | Données confidentielles exposées | 🔴 CRITIQUE |
| 2 | **CA total inclut commandes annulées** : `totalCA` somme `montantTTC` pour TOUS les statuts, y compris `Annulee` | CA incorrect, surévalué | 🔴 CRITIQUE |
| 3 | **Duplication des données** : Les KPI cards (Aujourd'hui, En attente) sont calculés côté front ET récupérés via `/orders/dashboard`. Deux appels API pour les mêmes chiffres, risque de désynchronisation | UX incohérente si les deux sources divergent | 🟡 MOYEN |
| 4 | **`DashboardService` est du code mort** : Le service `dashboard.service.ts` expose `/dashboard`, `/dashboard/metrics`, `/dashboard/recent-activity` mais **n'est jamais injecté ni utilisé** par `DashboardComponent` | Code mort, confusion sur l'API réelle | 🟡 MOYEN |
| 5 | **Montants HT/TTC sans pipe currencyTND** : Dans la section `orderDash`, `MontantTotalHT` et `MontantTotalTTC` utilisent `\| number:'1.0-0'` au lieu de `\| currencyTND`. Incohérence de format avec le KPI 3 | Format incohérent (pas d'unité "TND" visible) | 🟠 FAIBLE |
| 6 | **Normalisation statut fragile** : `computeStats()` tente de normaliser le champ statut avec 4 variantes (`Statut`, `statut`, `etatCommande`, `EtatCommande`) et compare à des strings comme `'EnAttente'` qui ne correspondent pas aux valeurs de l'enum (`'En attente'`). Si le backend renvoie l'enum numérique le code string `etat === 'EnAttente'` ne match jamais. | Comptage erroné des statuts | 🔴 CRITIQUE |
| 7 | **KpiDashboardComponent** (field/kpi) est une page séparée non intégrée au dashboard principal. Le DÉLÉGUÉ n'a pas de dashboard personnalisé. | DÉLÉGUÉ sans vue adaptée | 🟡 MOYEN |
| 8 | **`console.log` en production** dans `visite.service.ts:getById()` | Performance + sécurité | 🟢 COSMÉTIQUE |
| 9 | **`tauxLivraison` calculé sur 0 commandes** : si `/orders` retourne `[]` (erreur catchée), `tauxLivraison = 0` et les charts sont vides mais pas d'erreur affichée à l'utilisateur | UX dégradée silencieuse | 🟠 FAIBLE |

---

## 3.2 Missing features

| Feature | Impact |
|---------|--------|
| Dashboard DÉLÉGUÉ filtré par son `idDelegue` | Un DÉLÉGUÉ devrait voir uniquement ses propres commandes et KPIs |
| Dashboard SUPERVISEUR filtré par région/équipe | Un SUPERVISEUR devrait voir les délégués sous sa supervision |
| Intégration du KpiDashboardComponent dans le dashboard principal | Les KPIs terrain (visites, performance, taux conversion) ne sont pas visibles sur le dashboard |
| Filtres de date sur le dashboard | Impossible de voir les commandes d'une période spécifique |
| Tableau des dernières commandes | Pas de liste tabulaire des commandes récentes |
| Indicateur `ReclamationsResolues` manquant | `OrderDashboardDto` a le champ mais il n'est pas affiché |
| Export/téléchargement des données | Pas de bouton d'export CSV/Excel |
| Tendance (flèche ↑↓) sur les KPI cards | Pas de comparaison avec la période précédente |

---

## 3.3 Role-based issues

**ADMIN :**
- Voit toutes les commandes de tous les délégués. ✅ Correct.
- Voit les stats globales via `/orders/dashboard`. ✅ Correct.

**SUPERVISEUR :**
- Voit toutes les commandes (idem ADMIN). ⚠️ Devrait voir uniquement les délégués de sa région.
- Aucune différence avec ADMIN. Bug de conception.

**DÉLÉGUÉ :**
- Voit TOUTES les commandes de tout le monde. ❌ **Critique** — violation de confidentialité.
- Devrait voir uniquement ses propres commandes (`/orders/by-delegue/{id}`).
- Devrait voir ses KPIs (visites, performance) via le `KpiDashboardComponent`.

**Sections qui devraient être cachées pour certains rôles :**
- Section "Statistiques des commandes" (orderDash) → devrait être ADMIN/SUPERVISEUR uniquement, ou filtrée pour DÉLÉGUÉ
- Charts globaux (barres, donut, area) → idem
- Un DÉLÉGUÉ devrait avoir ses propres charts (ses visites, ses commandes personnelles)

---

═══════════════════════════════════════
## PARTIE 4 — ANALYSE COMPLÈTE
═══════════════════════════════════════

## 4.1 Ce qui fonctionne correctement ✅

- Le skeleton loader s'affiche bien pendant le chargement
- Le bouton Actualiser fonctionne (`reload()` → `loadAll()`)
- Le pipe `currencyTND` est utilisé sur le KPI CA total
- La barre de progression du taux de livraison s'anime via CSS
- Les charts ApexCharts ont des états vides gérés (`chart-empty`)
- `catchError(() => of([]))` évite un crash si `/orders` échoue
- La destruction (`takeUntil(destroy$)`) évite les memory leaks
- L'authentification est bien vérifiée via `authGuard`
- Le `KpiDashboardComponent` gère correctement les 3 rôles (ADMIN, SUPERVISEUR, DÉLÉGUÉ) pour la sélection du délégué
- `VisiteService.getAll()` et `getByDelegue()` sont bien typés avec `VisiteDto`

---

## 4.2 Ce qui est cassé ❌

1. **DÉLÉGUÉ voit toutes les commandes** (violation données)
2. **CA total inclut les annulées** (valeur incorrecte)
3. **Normalisation des statuts fragile** : `'EnAttente'` ne correspond pas à `'En attente'` → comptage à 0 pour certains statuts
4. **`DashboardService` jamais utilisé** (code mort et endpoints backend non exploités)
5. **Montants TND sans pipe** dans la section orderDash

---

## 4.3 Ce qui manque ⚠️

1. Vue dashboard personnalisée pour DÉLÉGUÉ (ses commandes, ses KPIs terrain)
2. Vue dashboard pour SUPERVISEUR (son équipe uniquement)
3. Intégration des KPIs terrain (visites, performance, taux conversion) dans le dashboard principal
4. Filtres temporels
5. Tableau des commandes récentes
6. `ReclamationsResolues` non affiché
7. Tendances comparatives (vs période précédente)

---

## 4.4 Fix plan

### FIX 1 — CRITIQUE : Filtrer les commandes selon le rôle (dashboard.component.ts)

```typescript
// Ajouter AuthService au constructeur
constructor(
  private orderApi: OrderApiService,
  private authSvc: AuthService,
  private cdr: ChangeDetectorRef
) {}

private loadAll(): void {
  this.loading = true;
  this.error   = '';

  const role = this.authSvc.getUserRole()?.toUpperCase();
  const userId = this.authSvc.getUserId();

  const orders$ = (role === 'DELEGUE')
    ? this.orderApi.getOrdersByClient(userId)   // ou une méthode getOrdersByDelegue
    : this.orderApi.getAllOrders();

  orders$
    .pipe(catchError(() => of([] as Commande[])), takeUntil(this.destroy$))
    .subscribe({
      next: orders => {
        this.buildOrderCharts(orders);
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.error   = 'Erreur lors du chargement du dashboard.';
        this.loading = false;
        this.cdr.markForCheck();
      }
    });

  // orderDash : uniquement pour ADMIN et SUPERVISEUR
  if (role !== 'DELEGUE') {
    this.loadingDash = true;
    this.orderApi.getOrdersDashboard()
      .pipe(catchError(() => of(null)), takeUntil(this.destroy$))
      .subscribe(d => { this.orderDash = d; this.loadingDash = false; this.cdr.markForCheck(); });
  }
}
```

### FIX 2 — CRITIQUE : Exclure les commandes annulées du CA total (order-api.service.ts)

```typescript
// Dans computeStats(), modifier l'accumulation du CA :
const ttc = (o as any).MontantTTC ?? (o as any).montantTTC ?? o.montantTTC ?? 0;
const etatNum2 = typeof etat === 'number' ? etat : undefined;
const isAnnulee = (etat === 'Annulée' || etat === 'Annulee' || etatNum2 === EtatCommande.Annulee);
if (!isAnnulee) totalCA += ttc;  // ← N'additionner que les non-annulées
```

### FIX 3 — CRITIQUE : Corriger la normalisation des statuts (order-api.service.ts)

Le problème est que `ETAT_LABELS` retourne `'En attente'` (avec espace) mais la comparaison
est `etat === 'EnAttente'` (sans espace). Corriger :

```typescript
// Dans computeStats() — remplacer les comparaisons string par des comparaisons d'enum numérique
const etatNum = typeof etat === 'number'
  ? etat
  : (Object.values(EtatCommande).indexOf(etat as any) >= 0
      ? parseInt(etat as any)
      : undefined);

// Utiliser uniquement etatNum pour les comptages
if (etatNum === EtatCommande.EnAttente)  countEnAttente++;
if (etatNum === EtatCommande.Livree)     countLivrees++;
if (etatNum === EtatCommande.Annulee)    countAnnulees++;
```

### FIX 4 — MOYEN : Corriger les montants TND dans la section orderDash (dashboard.component.html)

```html
<!-- Remplacer : -->
<span class="kpi-value" style="font-size:1rem">{{ orderDash.MontantTotalHT | number:'1.0-0' }}</span>
<span class="kpi-value" style="font-size:1rem">{{ orderDash.MontantTotalTTC | number:'1.0-0' }}</span>

<!-- Par : -->
<span class="kpi-value" style="font-size:1rem">{{ orderDash.MontantTotalHT | currencyTND }}</span>
<span class="kpi-value" style="font-size:1rem">{{ orderDash.MontantTotalTTC | currencyTND }}</span>
```

### FIX 5 — MOYEN : Masquer la section orderDash pour DÉLÉGUÉ (dashboard.component.html)

```html
<!-- Remplacer : -->
@if (orderDash) {

<!-- Par : -->
@if (orderDash && !isDelegue) {
```

Et dans `dashboard.component.ts` ajouter :
```typescript
get isDelegue(): boolean { return this.authSvc.getUserRole()?.toUpperCase() === 'DELEGUE'; }
```

### FIX 6 — MOYEN : Masquer section orderDash charts globaux pour DÉLÉGUÉ (dashboard.component.html)

```html
<!-- Entourer les sections charts d'une condition rôle si nécessaire -->
<ng-container *ngIf="!isDelegue">
  <div class="charts-row"> ... </div>
  <div class="chart-card chart-card--full"> ... </div>
</ng-container>

<!-- Et afficher les KPIs terrain pour DÉLÉGUÉ -->
<ng-container *ngIf="isDelegue">
  <app-kpi-dashboard></app-kpi-dashboard>
</ng-container>
```

### FIX 7 — COSMÉTIQUE : Supprimer les console.log de production (visite.service.ts)

```typescript
// Supprimer les lignes :
console.log('API Response for getById:', r, 'Unwrapped:', unwrapped);
console.log('Normalized Visite:', normalized);
```

### FIX 8 — MOYEN : Ajouter ReclamationsResolues dans la section orderDash (dashboard.component.html)

```html
<!-- Après la carte ReclamationsEnCours : -->
<div class="kpi-card kpi-green"><span class="kpi-label">Réclam. résolues</span><span class="kpi-value">{{ orderDash.ReclamationsResolues }}</span></div>
```

---

**Ordre de priorité des fixes :**
1. FIX 1 — Filtrer commandes par rôle (sécurité données)
2. FIX 2 — Exclure annulées du CA (valeur correcte)
3. FIX 3 — Normalisation statuts (comptages corrects)
4. FIX 5 + FIX 6 — Masquage sections selon rôle
5. FIX 4 — Format TND cohérent
6. FIX 8 — Champ manquant ReclamationsResolues
7. FIX 7 — Nettoyage console.log

---

═══════════════════════════════════════
## PARTIE 5 — COMPLETE CODE
═══════════════════════════════════════

---

### FILE 1 : dashboard.component.ts
**Path** : `Cynapharm/src/app/features/dashboard/dashboard.component.ts`

```typescript
import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, of } from 'rxjs';
import { takeUntil, catchError } from 'rxjs/operators';
import { NgApexchartsModule } from 'ng-apexcharts';

import { OrderApiService, Commande, OrderStats, OrderDashboardDto } from './services/order-api.service';
import { CardComponent } from '../../shared/components/card/card.component';
import { CurrencyTNDPipe } from '../../shared/pipes/currency-tnd.pipe';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, NgApexchartsModule, CardComponent, CurrencyTNDPipe],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit, OnDestroy {

  loading = true;
  error = '';

  // ── KPI Cards ─────────────────────────────────────────
  commandesAujourdhui = 0;
  commandesEnAttente  = 0;
  caTotal             = 0;
  tauxLivraison       = 0;
  orderDash: OrderDashboardDto | null = null;
  loadingDash = false;

  // ── Chart : Commandes par statut — barres ────────────
  statutBarSeries: any[] = [];
  statutBarOptions = {
    chart:       { type: 'bar' as const, height: 300, toolbar: { show: false } },
    plotOptions: { bar: { borderRadius: 6, horizontal: true } },
    dataLabels:  { enabled: false },
    xaxis:       { categories: [] as string[], labels: { style: { fontSize: '12px' } } },
    colors:      ['#0077b6'],
    tooltip:     { x: { show: true } },
    title:       { text: 'Commandes par statut', style: { fontSize: '14px', fontWeight: '600' } }
  };

  // ── Chart : Commandes par statut (donut) ─────────────
  statutChartSeries: number[] = [];
  statutChartLabels: string[] = [];
  statutChartOptions = {
    chart:    { type: 'donut' as const, height: 300 },
    colors:   ['#adb5bd', '#ffc107', '#0077b6', '#17a2b8', '#28a745', '#dc3545'],
    dataLabels: { enabled: true },
    legend:   { position: 'bottom' as const },
    tooltip:  { y: { formatter: (v: number) => `${v} commandes` } },
    title:    { text: 'Commandes par statut', style: { fontSize: '14px', fontWeight: '600' } }
  };

  // ── Chart : Volume commandes 7 jours (courbe) ─────────
  volumeChartSeries: any[] = [];
  volumeChartOptions = {
    chart:   { type: 'area' as const, height: 300, toolbar: { show: false } },
    stroke:  { curve: 'smooth' as const, width: 2 },
    fill:    { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: 0.4, opacityTo: 0.05 } },
    dataLabels: { enabled: false },
    xaxis:   { categories: [] as string[], labels: { style: { fontSize: '11px' } } },
    colors:  ['#0077b6'],
    tooltip: { y: { formatter: (v: number) => `${v} commande(s)` } },
    title:   { text: 'Volume de commandes — 7 derniers jours', style: { fontSize: '14px', fontWeight: '600' } }
  };

  private readonly destroy$ = new Subject<void>();

  constructor(
    private orderApi: OrderApiService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadAll();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── Chargement de toutes les données ──────────────────

  private loadAll(): void {
    this.loading = true;
    this.error   = '';

    this.orderApi.getAllOrders()
      .pipe(catchError(() => of([] as Commande[])), takeUntil(this.destroy$))
      .subscribe({
        next: orders => {
          this.buildOrderCharts(orders);
          this.loading = false;
          this.cdr.markForCheck();
        },
        error: () => {
          this.error   = 'Erreur lors du chargement du dashboard.';
          this.loading = false;
          this.cdr.markForCheck();
        }
      });

    this.loadingDash = true;
    this.orderApi.getOrdersDashboard()
      .pipe(catchError(() => of(null)), takeUntil(this.destroy$))
      .subscribe(d => { this.orderDash = d; this.loadingDash = false; this.cdr.markForCheck(); });
  }

  // ── Construction de tous les graphiques depuis les commandes ──

  private buildOrderCharts(orders: Commande[]): void {
    const stats: OrderStats = this.orderApi.computeStats(orders);

    // KPI cards
    this.commandesAujourdhui = stats.countToday;
    this.commandesEnAttente  = stats.countEnAttente;
    this.caTotal             = stats.totalCA;

    // Taux de livraison = Livrées / total commandes × 100
    this.tauxLivraison = stats.totalOrders > 0
      ? Math.round((stats.countLivrees / stats.totalOrders) * 100)
      : 0;

    // Barres horizontales : commandes par statut
    const statusLabels  = Object.keys(stats.countByStatus);
    const statusCounts  = statusLabels.map(l => stats.countByStatus[l]);
    this.statutBarSeries  = [{ name: 'Commandes', data: statusCounts }];
    this.statutBarOptions = {
      ...this.statutBarOptions,
      xaxis: { ...this.statutBarOptions.xaxis, categories: statusLabels }
    };

    // Donut statuts
    this.statutChartLabels  = statusLabels;
    this.statutChartSeries  = statusCounts;
    this.statutChartOptions = { ...this.statutChartOptions } as any;

    // Courbe 7 jours
    const days    = stats.last7Days.map(d => d.date.slice(5));
    const counts7 = stats.last7Days.map(d => d.count);
    this.volumeChartSeries  = [{ name: 'Commandes', data: counts7 }];
    this.volumeChartOptions = {
      ...this.volumeChartOptions,
      xaxis: { ...this.volumeChartOptions.xaxis, categories: days }
    };
  }

  reload(): void { this.loadAll(); }
}
```

---

### FILE 2 : dashboard.component.html
**Path** : `Cynapharm/src/app/features/dashboard/dashboard.component.html`

```html
<div class="dashboard-wrapper">

  <!-- ── En-tête ──────────────────────────────────────── -->
  <div class="dash-header">
    <div>
      <h1 class="dash-title">Tableau de bord</h1>
      <p class="dash-subtitle">Vue temps réel — commandes &amp; chiffre d'affaires</p>
    </div>
    <button class="btn-refresh" (click)="reload()" [disabled]="loading" title="Actualiser">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" [class.spinning]="loading">
        <polyline points="23 4 23 10 17 10"/>
        <path d="M20.49 15a9 9 0 1 1-2.12-9.36L23 10"/>
      </svg>
      Actualiser
    </button>
  </div>

  <!-- ── Erreur globale ─────────────────────────────────  -->
  <div *ngIf="error && !loading" class="dash-error">
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
      <circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/>
      <line x1="12" y1="16" x2="12.01" y2="16"/>
    </svg>
    {{ error }}
  </div>

  <!-- ── Skeleton loader ───────────────────────────────── -->
  <div *ngIf="loading" class="skeleton-grid">
    <div class="skeleton-card" *ngFor="let i of [1,2,3,4]"></div>
  </div>

  <ng-container *ngIf="!loading">

    <!-- ── KPI Cards ──────────────────────────────────── -->
    <div class="kpi-grid">

      <!-- Commandes aujourd'hui -->
      <div class="kpi-card kpi-blue">
        <div class="kpi-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <rect x="2" y="3" width="20" height="14" rx="2"/>
            <path d="M8 21h8M12 17v4"/>
          </svg>
        </div>
        <div class="kpi-body">
          <span class="kpi-label">Commandes aujourd'hui</span>
          <span class="kpi-value">{{ commandesAujourdhui }}</span>
        </div>
      </div>

      <!-- Commandes en attente -->
      <div class="kpi-card kpi-orange">
        <div class="kpi-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="9" cy="21" r="1"/><circle cx="20" cy="21" r="1"/>
            <path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6"/>
          </svg>
        </div>
        <div class="kpi-body">
          <span class="kpi-label">Commandes en attente</span>
          <span class="kpi-value">{{ commandesEnAttente }}</span>
        </div>
      </div>

      <!-- CA total -->
      <div class="kpi-card kpi-green">
        <div class="kpi-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <line x1="12" y1="1" x2="12" y2="23"/>
            <path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/>
          </svg>
        </div>
        <div class="kpi-body">
          <span class="kpi-label">Chiffre d'affaires total</span>
          <span class="kpi-value">{{ caTotal | currencyTND }}</span>
        </div>
      </div>

      <!-- Taux de livraison -->
      <div class="kpi-card kpi-purple">
        <div class="kpi-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/>
          </svg>
        </div>
        <div class="kpi-body">
          <span class="kpi-label">Taux de livraison</span>
          <span class="kpi-value">{{ tauxLivraison }}%</span>
        </div>
        <div class="kpi-progress">
          <div class="kpi-bar" [style.width.%]="tauxLivraison"></div>
        </div>
      </div>

    </div>

    <!-- ── Statistiques commandes (API dashboard) ───────── -->
    @if (orderDash) {
      <div style="margin-bottom:1.5rem">
        <h2 style="font-size:1rem;font-weight:600;color:#374151;margin:0 0 .75rem">Statistiques des commandes</h2>
        <div style="display:grid;grid-template-columns:repeat(auto-fill,minmax(150px,1fr));gap:.75rem;margin-bottom:.75rem">
          <div class="kpi-card kpi-blue"  style="padding:.875rem"><span class="kpi-label">Total</span><span class="kpi-value">{{ orderDash.TotalCommandes }}</span></div>
          <div class="kpi-card kpi-orange"><span class="kpi-label">En attente</span><span class="kpi-value">{{ orderDash.EnAttente }}</span></div>
          <div class="kpi-card"           ><span class="kpi-label">Confirmées</span><span class="kpi-value">{{ orderDash.Confirmees }}</span></div>
          <div class="kpi-card"           ><span class="kpi-label">En préparation</span><span class="kpi-value">{{ orderDash.EnPreparation }}</span></div>
          <div class="kpi-card"           ><span class="kpi-label">Expédiées</span><span class="kpi-value">{{ orderDash.Expediees }}</span></div>
          <div class="kpi-card kpi-green" ><span class="kpi-label">Livrées</span><span class="kpi-value">{{ orderDash.Livrees }}</span></div>
          <div class="kpi-card"  style="background:#fef2f2"><span class="kpi-label">Annulées</span><span class="kpi-value" style="color:#b91c1c">{{ orderDash.Annulees }}</span></div>
          <div class="kpi-card kpi-green" ><span class="kpi-label">Total HT (TND)</span><span class="kpi-value" style="font-size:1rem">{{ orderDash.MontantTotalHT | number:'1.0-0' }}</span></div>
          <div class="kpi-card kpi-green" ><span class="kpi-label">Total TTC (TND)</span><span class="kpi-value" style="font-size:1rem">{{ orderDash.MontantTotalTTC | number:'1.0-0' }}</span></div>
          <div class="kpi-card kpi-orange"><span class="kpi-label">Réclam. ouvertes</span><span class="kpi-value">{{ orderDash.ReclamationsOuvertes }}</span></div>
          <div class="kpi-card kpi-orange"><span class="kpi-label">Réclam. en cours</span><span class="kpi-value">{{ orderDash.ReclamationsEnCours }}</span></div>
          <div class="kpi-card kpi-blue"  ><span class="kpi-label">Aujourd'hui</span><span class="kpi-value">{{ orderDash.CommandesAujourdHui }}</span></div>
          <div class="kpi-card kpi-blue"  ><span class="kpi-label">Ce mois</span><span class="kpi-value">{{ orderDash.CommandesCeMois }}</span></div>
        </div>
      </div>
    }

    <!-- ── Graphiques ligne 1 ─────────────────────────── -->
    <div class="charts-row">

      <!-- Chart 1 : Barres horizontales — commandes par statut -->
      <div class="chart-card">
        <div *ngIf="statutBarSeries.length === 0 || statutBarSeries[0]?.data?.length === 0" class="chart-empty">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
            <rect x="3" y="3" width="18" height="18" rx="2"/>
            <path d="M3 9h18M9 21V9"/>
          </svg>
          <p>Aucune donnée de commandes disponible.</p>
        </div>
        <apx-chart
          *ngIf="statutBarSeries.length > 0 && statutBarSeries[0]?.data?.length > 0"
          [series]="statutBarSeries"
          [chart]="statutBarOptions.chart"
          [plotOptions]="statutBarOptions.plotOptions"
          [dataLabels]="statutBarOptions.dataLabels"
          [xaxis]="statutBarOptions.xaxis"
          [colors]="statutBarOptions.colors"
          [title]="statutBarOptions.title">
        </apx-chart>
      </div>

      <!-- Chart 2 : Donut commandes par statut -->
      <div class="chart-card chart-card--small">
        <div *ngIf="statutChartSeries.length === 0" class="chart-empty">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
            <circle cx="12" cy="12" r="10"/>
            <path d="M12 8v4M12 16h.01"/>
          </svg>
          <p>Aucune commande disponible.</p>
        </div>
        <apx-chart
          *ngIf="statutChartSeries.length > 0"
          [series]="statutChartSeries"
          [chart]="statutChartOptions.chart"
          [labels]="statutChartLabels"
          [colors]="statutChartOptions.colors"
          [dataLabels]="statutChartOptions.dataLabels"
          [legend]="statutChartOptions.legend"
          [tooltip]="statutChartOptions.tooltip"
          [title]="statutChartOptions.title">
        </apx-chart>
      </div>

    </div>

    <!-- ── Graphique ligne 2 : Volume 7 jours ───────────  -->
    <div class="chart-card chart-card--full">
      <div *ngIf="volumeChartSeries.length === 0 || volumeChartSeries[0]?.data?.length === 0" class="chart-empty">
        <p>Aucune donnée de commandes sur 7 jours.</p>
      </div>
      <apx-chart
        *ngIf="volumeChartSeries.length > 0 && volumeChartSeries[0]?.data?.length > 0"
        [series]="volumeChartSeries"
        [chart]="volumeChartOptions.chart"
        [stroke]="volumeChartOptions.stroke"
        [fill]="volumeChartOptions.fill"
        [dataLabels]="volumeChartOptions.dataLabels"
        [xaxis]="volumeChartOptions.xaxis"
        [colors]="volumeChartOptions.colors"
        [tooltip]="volumeChartOptions.tooltip"
        [title]="volumeChartOptions.title">
      </apx-chart>
    </div>

  </ng-container>
</div>
```

---

### FILE 3 : dashboard.component.css
**Path** : `Cynapharm/src/app/features/dashboard/dashboard.component.css`

```css
/* ─── Dashboard Premium Design ──────────────────────── */
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap');

/* ─── Wrapper ────────────────────────────────────────── */
.dashboard-wrapper {
  padding: 32px 40px;
  font-family: 'Inter', system-ui, sans-serif;
  background: #f8fafc;
  min-height: 100vh;
}

/* ─── Header ─────────────────────────────────────────── */
.dash-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 36px;
}

.dash-title {
  font-size: 28px;
  font-weight: 800;
  color: #0f172a;
  margin: 0 0 6px 0;
  letter-spacing: -0.5px;
}

.dash-subtitle {
  font-size: 14px;
  color: #64748b;
  margin: 0;
  font-weight: 500;
}

.btn-refresh {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 12px 20px;
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  font-size: 14px;
  font-weight: 600;
  color: #475569;
  cursor: pointer;
  transition: all 0.2s;
  box-shadow: 0 1px 3px rgba(0,0,0,0.04);
}

.btn-refresh:hover:not(:disabled) {
  border-color: #00b4d8;
  color: #0077b6;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(0, 180, 216, 0.15);
}

.btn-refresh:disabled { opacity: 0.5; cursor: not-allowed; }

.btn-refresh svg {
  width: 16px;
  height: 16px;
}

.btn-refresh svg.spinning {
  animation: spin 1s linear infinite;
}

@keyframes spin { to { transform: rotate(360deg); } }

/* ─── Erreur ─────────────────────────────────────────── */
.dash-error {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px 20px;
  background: #fef2f2;
  border: 1px solid #fecdd3;
  border-radius: 16px;
  color: #e11d48;
  font-size: 14px;
  font-weight: 500;
  margin-bottom: 28px;
}

.dash-error svg { width: 20px; height: 20px; flex-shrink: 0; }

/* ─── Skeleton ───────────────────────────────────────── */
.skeleton-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 24px;
  margin-bottom: 28px;
}

.skeleton-card {
  height: 120px;
  border-radius: 20px;
  background: linear-gradient(90deg, #e2e8f0 25%, #f1f5f9 50%, #e2e8f0 75%);
  background-size: 200% 100%;
  animation: shimmer 1.4s infinite;
}

@keyframes shimmer { to { background-position: -200% 0; } }

/* ─── KPI Grid ───────────────────────────────────────── */
.kpi-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 24px;
  margin-bottom: 28px;
}

.kpi-card {
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 20px;
  padding: 24px;
  display: flex;
  align-items: flex-start;
  gap: 18px;
  box-shadow: 0 4px 6px -1px rgba(0,0,0,0.02), 0 2px 4px -2px rgba(0,0,0,0.02);
  position: relative;
  overflow: hidden;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.kpi-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 24px -8px rgba(0,0,0,0.08);
  border-color: #cbd5e1;
}

/* Decorative gradient blob */
.kpi-card::after {
  content: '';
  position: absolute;
  width: 80px;
  height: 80px;
  border-radius: 50%;
  right: -20px;
  top: -20px;
  opacity: 0.07;
}

.kpi-blue::after  { background: #0077b6; }
.kpi-orange::after { background: #f59e0b; }
.kpi-green::after  { background: #059669; }
.kpi-purple::after { background: #7c3aed; }

.kpi-icon {
  width: 48px;
  height: 48px;
  border-radius: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.kpi-icon svg { width: 22px; height: 22px; }

.kpi-body { flex: 1; }

.kpi-label {
  display: block;
  font-size: 12px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: #94a3b8;
  margin-bottom: 8px;
}

.kpi-value {
  display: block;
  font-size: 30px;
  font-weight: 800;
  line-height: 1;
  font-variant-numeric: tabular-nums;
  letter-spacing: -0.5px;
}

/* Progress bar (taux de livraison) */
.kpi-progress {
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: #f1f5f9;
  border-radius: 0 0 20px 20px;
}

.kpi-bar {
  height: 100%;
  border-radius: 0 0 0 20px;
  transition: width 1s cubic-bezier(0.4, 0, 0.2, 1);
  background: linear-gradient(90deg, #8b5cf6, #7c3aed);
}

/* Color variants */
.kpi-blue  .kpi-icon  { background: #eff6ff; color: #0077b6; }
.kpi-blue  .kpi-value { color: #0077b6; }
.kpi-orange .kpi-icon  { background: #fffbeb; color: #d97706; }
.kpi-orange .kpi-value { color: #d97706; }
.kpi-green .kpi-icon  { background: #ecfdf5; color: #059669; }
.kpi-green .kpi-value { color: #059669; }
.kpi-purple .kpi-icon  { background: #f5f3ff; color: #7c3aed; }
.kpi-purple .kpi-value { color: #7c3aed; }

/* ─── Charts ─────────────────────────────────────────── */
.charts-row {
  display: grid;
  grid-template-columns: 1fr 380px;
  gap: 24px;
  margin-bottom: 24px;
}

.chart-card {
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 20px;
  padding: 24px 20px;
  box-shadow: 0 4px 6px -1px rgba(0,0,0,0.02);
  min-height: 360px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: box-shadow 0.2s;
}

.chart-card:hover {
  box-shadow: 0 8px 16px rgba(0,0,0,0.04);
}

.chart-card--small { min-height: 360px; }

.chart-card--full {
  width: 100%;
  min-height: 320px;
  margin-bottom: 28px;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Empty state dans les charts */
.chart-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 14px;
  color: #94a3b8;
  font-size: 14px;
  font-weight: 500;
  text-align: center;
}

.chart-empty svg {
  width: 52px;
  height: 52px;
  opacity: 0.3;
}

/* ─── Responsive ─────────────────────────────────────── */
@media (max-width: 1200px) {
  .kpi-grid    { grid-template-columns: repeat(2, 1fr); }
  .charts-row  { grid-template-columns: 1fr; }
}

@media (max-width: 600px) {
  .dashboard-wrapper { padding: 16px; }
  .dash-title        { font-size: 22px; }
  .kpi-grid          { grid-template-columns: 1fr; gap: 16px; }
  .kpi-card          { padding: 18px; }
}
```

---

### FILE 4 : dashboard.service.ts (NON UTILISÉ — code mort)
**Path** : `Cynapharm/src/app/features/dashboard/dashboard.service.ts`

```typescript
import { Injectable } from '@angular/core';
import { ApiService } from '../../core/services/api.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private endpoint = '/dashboard';

  constructor(private apiService: ApiService) { }

  getDashboardData(): Observable<any> {
    return this.apiService.get<any>(this.endpoint);
  }

  getMetrics(): Observable<any> {
    return this.apiService.get<any>(`${this.endpoint}/metrics`);
  }

  getRecentActivity(): Observable<any> {
    return this.apiService.get<any>(`${this.endpoint}/recent-activity`);
  }
}
```

---

### FILE 5 : order-api.service.ts — getOrdersDashboard() method
**Path** : `Cynapharm/src/app/features/dashboard/services/order-api.service.ts`

```typescript
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../core/services/api.service';

/** Correspond à EtatCommande enum backend (7 états) */
export enum EtatCommande {
  Brouillon     = 0,
  EnAttente     = 1,
  Confirmee     = 2,
  EnPreparation = 3,
  Expediee      = 4,
  Livree        = 5,
  Annulee       = 6,
}

export const ETAT_LABELS: Record<EtatCommande, string> = {
  [EtatCommande.Brouillon]:     'Brouillon',
  [EtatCommande.EnAttente]:     'En attente',
  [EtatCommande.Confirmee]:     'Confirmée',
  [EtatCommande.EnPreparation]: 'En préparation',
  [EtatCommande.Expediee]:      'Expédiée',
  [EtatCommande.Livree]:        'Livrée',
  [EtatCommande.Annulee]:       'Annulée',
};

export interface OrderDashboardDto {
  TotalCommandes:       number;
  EnAttente:            number;
  Confirmees:           number;
  EnPreparation:        number;
  Expediees:            number;
  Livrees:              number;
  Annulees:             number;
  MontantTotalHT:       number;
  MontantTotalTTC:      number;
  ReclamationsOuvertes: number;
  ReclamationsEnCours:  number;
  ReclamationsResolues: number;
  CommandesAujourdHui:  number;
  CommandesCeMois:      number;
}

export interface Commande {
  id_Commande: number;
  dateCommande: string;
  montantHT: number;
  montantTTC: number;
  etatCommande: EtatCommande;
  id_Client: number;
}

export interface OrderStats {
  countByStatus:  Record<string, number>;
  totalCA:        number;
  countEnAttente: number;
  countLivrees:   number;
  countAnnulees:  number;
  countToday:     number;
  totalOrders:    number;
  last7Days:      { date: string; count: number; ca: number }[];
}

@Injectable({ providedIn: 'root' })
export class OrderApiService {

  constructor(private api: ApiService) {}

  private unwrap<T>(r: any): T {
    if (r?.Result !== undefined) return r.Result;
    if (r?.result !== undefined) return r.result;
    return r;
  }

  /** Toutes les commandes (ADMIN/SUPERVISEUR) */
  getAllOrders(): Observable<Commande[]> {
    return this.api.get<any>(`/orders`).pipe(
      map(r => {
        const data = this.unwrap<any>(r);
        return Array.isArray(data) ? data : [];
      })
    );
  }

  /** Tableau de bord commandes */
  getOrdersDashboard(): Observable<OrderDashboardDto> {
    return this.api.get<any>(`/orders/dashboard`).pipe(
      map(r => this.unwrap<OrderDashboardDto>(r))
    );
  }

  /** Commandes d'un client */
  getOrdersByClient(idClient: number): Observable<Commande[]> {
    return this.api.get<any>(`/orders/by-client/${idClient}`).pipe(
      map(r => {
        const data = this.unwrap<any>(r);
        return Array.isArray(data) ? data : [];
      })
    );
  }

  /**
   * Calcule les statistiques côté front à partir de la liste brute.
   * Évite un aller-retour supplémentaire au backend.
   */
  computeStats(orders: Commande[]): OrderStats {
    const countByStatus: Record<string, number> = {};
    let totalCA = 0;
    let countEnAttente = 0;
    let countLivrees = 0;

    // Données des 7 derniers jours
    const now   = new Date();
    const todayStr = now.toISOString().slice(0, 10);
    const last7: { date: string; count: number; ca: number }[] = [];
    for (let i = 6; i >= 0; i--) {
      const d = new Date(now);
      d.setDate(now.getDate() - i);
      last7.push({ date: d.toISOString().slice(0, 10), count: 0, ca: 0 });
    }

    let countAnnulees = 0;
    let countToday    = 0;

    for (const o of orders) {
      // Normalise le champ statut (PascalCase ou camelCase selon backend)
      const etat: any = (o as any).Statut ?? (o as any).statut
                     ?? (o as any).etatCommande ?? (o as any).EtatCommande;
      const label = typeof etat === 'string'
        ? etat
        : (ETAT_LABELS[etat as EtatCommande] ?? 'Inconnu');

      countByStatus[label] = (countByStatus[label] ?? 0) + 1;

      const ttc = (o as any).MontantTTC ?? (o as any).montantTTC ?? o.montantTTC ?? 0;
      totalCA += ttc;

      const etatNum = typeof etat === 'number' ? etat : undefined;
      if (etat === 'EnAttente'    || etatNum === EtatCommande.EnAttente)  countEnAttente++;
      if (etat === 'Livree'       || etatNum === EtatCommande.Livree)     countLivrees++;
      if (etat === 'Annulee'      || etatNum === EtatCommande.Annulee)    countAnnulees++;

      const dateStr = ((o as any).DateCommande ?? (o as any).dateCommande ?? o.dateCommande ?? '')
                        .slice(0, 10);
      if (dateStr === todayStr) countToday++;

      const bucket = last7.find(b => b.date === dateStr);
      if (bucket) { bucket.count++; bucket.ca += ttc; }
    }

    return {
      countByStatus, totalCA, countEnAttente, countLivrees,
      countAnnulees, countToday, totalOrders: orders.length,
      last7Days: last7,
    };
  }
}
```

---

### FILE 6 : visite.service.ts — getAll() and getByDelegue() methods
**Path** : `Cynapharm/src/app/features/field/visites/services/visite.service.ts`

```typescript
import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../../core/services/api.service';
import { VisiteType } from '../../../../core/models/enums';

export interface VisiteDto {
  idVisite?:       number;
  id_User_Delegue: number;
  dateVisite:      string;
  type:            VisiteType;
  isCompleted?:    boolean;
  id_Medecin?:     number | null;
  id_Pharmacien?:  number | null;
  id_Planning?:    number | null;
  id_Region?:      number | null;
}

@Injectable({ providedIn: 'root' })
export class VisiteService {
  private readonly base = '/fields/visites';
  constructor(private api: ApiService) {}
  private u<T>(r: any): T { return r?.Result ?? r?.result ?? r; }

  private normalize(r: any): VisiteDto {
    return {
      idVisite:        r.idVisite        ?? r.IdVisite        ?? undefined,
      id_User_Delegue: r.id_User_Delegue ?? r.Id_User_Delegue ?? r.idUserDelegue ?? 0,
      dateVisite:      r.date            ?? r.dateVisite      ?? r.DateVisite     ?? r.Date ?? '',
      type:            r.type            ?? r.Type            ?? 0,
      isCompleted:     r.isCompleted     ?? r.IsCompleted     ?? false,
      id_Medecin:      r.id_Medecin      ?? r.Id_Medecin      ?? r.idMedecin      ?? null,
      id_Pharmacien:   r.id_Pharmacien   ?? r.Id_Pharmacien   ?? r.idPharmacien   ?? null,
      id_Planning:     r.id_Planning     ?? r.Id_Planning     ?? r.idPlanning     ?? null,
      id_Region:       r.id_Region       ?? r.Id_Region       ?? r.idRegion       ?? null,
    };
  }

  // ── getAll() ─────────────────────────────────────────
  getAll(startDate?: string, endDate?: string): Observable<VisiteDto[]> {
    let p = new HttpParams();
    if (startDate) p = p.set('startDate', startDate);
    if (endDate)   p = p.set('endDate',   endDate);
    return this.api.get<any>(this.base, p).pipe(
      map(r => (this.u<any[]>(r) ?? []).map((x: any) => this.normalize(x)))
    );
  }

  // ── getByDelegue() ───────────────────────────────────
  getByDelegue(id: number): Observable<VisiteDto[]> {
    return this.api.get<any>(`${this.base}/by-delegue/${id}`).pipe(
      map(r => (this.u<any[]>(r) ?? []).map((x: any) => this.normalize(x)))
    );
  }
}
```

---

### FILE 7 : kpi-dashboard.component.ts
**Path** : `Cynapharm/src/app/features/field/kpi/kpi-dashboard/kpi-dashboard.component.ts`

```typescript
import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil, catchError, of } from 'rxjs';
import { KpiService } from '../services/kpi.service';
import { UserService } from '../../../users/user.service';
import { AuthService } from '../../../../core/services/auth.service';
import { VisiteService } from '../../visites/services/visite.service';

@Component({
  selector: 'app-kpi-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './kpi-dashboard.component.html',
  styleUrls: ['./kpi-dashboard.component.css']
})
export class KpiDashboardComponent implements OnInit, OnDestroy {
  idDelegue:       number | null = null;
  dateDebut        = '';
  dateFin          = '';
  loading          = false;
  loaded           = false;
  error            = '';
  visitesCount     = 0;
  performanceRate  = 0;
  tauxConversion: number | null = null;
  loadingTaux      = false;
  historique: any[] = [];
  performances: any[] = [];
  delegues: { id: number; nom: string }[] = [];

  private d$ = new Subject<void>();

  get isAdmin():       boolean { return this.authSvc.getUserRole()?.toUpperCase() === 'ADMIN'; }
  get isSuperviseur(): boolean { return this.authSvc.getUserRole()?.toUpperCase() === 'SUPERVISEUR'; }
  get isDelegue():     boolean { return this.authSvc.getUserRole()?.toUpperCase() === 'DELEGUE'; }

  constructor(
    private svc: KpiService,
    private userSvc: UserService,
    private authSvc: AuthService,
    private visiteSvc: VisiteService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const now = new Date();
    const firstDay = new Date(now.getFullYear(), now.getMonth(), 1);
    this.dateDebut = firstDay.toISOString().split('T')[0];
    this.dateFin   = now.toISOString().split('T')[0];

    if (this.isDelegue) {
      this.idDelegue = this.authSvc.getUserId();
      this.load();
      return;
    }

    this.userSvc.getUsersByRole('DELEGUE').pipe(takeUntil(this.d$)).subscribe({
      next: users => {
        if (!users.length) {
          this.loadDeleguesFromVisites();
          return;
        }
        this.delegues = users
          .map(u => this.userSvc.toUserOption(u))
          .filter((d): d is { id: number; nom: string } => d !== null);
        this.cdr.markForCheck();
      },
      error: () => this.loadDeleguesFromVisites()
    });
  }

  ngOnDestroy() { this.d$.next(); this.d$.complete(); }

  load(): void {
    if (!this.idDelegue) return;
    this.loading = true; this.error = '';
    const id = this.idDelegue;

    this.svc.getNombreVisites(id, this.dateDebut || undefined, this.dateFin || undefined)
      .pipe(takeUntil(this.d$), catchError(() => of(0)))
      .subscribe(v => { this.visitesCount = typeof v === 'number' ? v : (v as any)?.count ?? 0; });

    this.svc.getPerformanceRate(id)
      .pipe(takeUntil(this.d$), catchError(() => of(0)))
      .subscribe(r => { this.performanceRate = r; });

    if (this.dateDebut && this.dateFin) {
      this.loadingTaux = true;
      this.svc.getTauxConversion(id, this.dateDebut, this.dateFin)
        .pipe(takeUntil(this.d$), catchError(() => of(null)))
        .subscribe(t => { this.tauxConversion = t; this.loadingTaux = false; this.cdr.markForCheck(); });
    } else {
      this.tauxConversion = null;
    }

    this.svc.getPerformance(id)
      .pipe(takeUntil(this.d$), catchError(() => of([])))
      .subscribe(p => {
        this.performances = Array.isArray(p) ? p : [];
        this.cdr.markForCheck();
      });

    this.svc.getHistorique(id)
      .pipe(takeUntil(this.d$), catchError(() => of([])))
      .subscribe(h => {
        this.historique = h;
        this.loading    = false;
        this.loaded     = true;
        this.cdr.markForCheck();
      });
  }

  historiqueDate(entry: any): string | null {
    return entry?.date ?? entry?.Date ?? entry?.dateAction ?? entry?.DateAction ?? entry?.createdAt ?? entry?.CreatedAt ?? null;
  }

  historiqueAction(entry: any): string {
    return entry?.action ?? entry?.Action ?? entry?.type ?? entry?.Type ?? entry?.event ?? entry?.Event ?? '—';
  }

  historiqueDetail(entry: any): string {
    return entry?.detail ?? entry?.Detail ?? entry?.description ?? entry?.Description ?? entry?.message ?? entry?.Message ?? '—';
  }

  private loadDeleguesFromVisites(): void {
    this.visiteSvc.getAll().pipe(takeUntil(this.d$)).subscribe({
      next: visites => this.resolveDelegueOptions(visites.map(v => v.id_User_Delegue)),
      error: () => {}
    });
  }

  private resolveDelegueOptions(ids: number[]): void {
    const uniqueIds = [...new Set(ids.filter(id => id > 0))];
    if (!uniqueIds.length) return;

    this.userSvc.getDisplayNamesByIds(uniqueIds).pipe(takeUntil(this.d$)).subscribe({
      next: names => {
        this.delegues = uniqueIds.map(id => ({ id, nom: names[id] ?? `#${id}` }));
        this.cdr.markForCheck();
      },
      error: () => {}
    });
  }
}
```

---

### FILE 8 : kpi-dashboard.component.html
**Path** : `Cynapharm/src/app/features/field/kpi/kpi-dashboard/kpi-dashboard.component.html`

```html
<div class="pw">
  <a routerLink="/field" class="back">← Field</a>
  <h1>KPI Délégués</h1>

  <div class="filters">
    @if (isAdmin || isSuperviseur) {
      <div class="field">
        <label>Délégué</label>
        <select [(ngModel)]="idDelegue" class="inp">
          <option [ngValue]="null">-- Sélectionner un délégué --</option>
          <option *ngFor="let d of delegues" [ngValue]="d.id">{{ d.nom }}</option>
        </select>
      </div>
    }
    <div class="field">
      <label>Date début</label>
      <input type="date" [(ngModel)]="dateDebut" class="inp">
    </div>
    <div class="field">
      <label>Date fin</label>
      <input type="date" [(ngModel)]="dateFin" class="inp">
    </div>
    @if (!isDelegue) {
      <button class="btn-load" (click)="load()" [disabled]="loading || !idDelegue">
        {{ loading ? '…' : 'Charger' }}
      </button>
    }
  </div>

  @if (error) { <div class="err-box">{{ error }}</div> }

  @if (loaded) {
    <div class="kpi-row">
      <div class="kpi-card">
        <span class="kv">{{ visitesCount }}</span>
        <span class="kl">Visites</span>
      </div>
      <div class="kpi-card kpi-blue">
        <span class="kv">{{ performanceRate | number:'1.0-1' }}%</span>
        <span class="kl">Performance</span>
      </div>
      <div class="kpi-card kpi-green">
        @if (loadingTaux) {
          <span class="kv">…</span>
        } @else if (tauxConversion !== null) {
          <span class="kv">{{ tauxConversion | number:'1.0-1' }}%</span>
        } @else {
          <span class="kv" title="Sélectionnez une période">—</span>
        }
        <span class="kl">Taux de conversion</span>
        @if (tauxConversion === null && !loadingTaux) {
          <span style="font-size:0.7rem;opacity:.6">Période requise</span>
        }
      </div>
    </div>

    @if (performances.length > 0) {
      <div class="perf-section">
        <h3>Objectifs & Performance</h3>
        <div class="perf-cards">
          @for (p of performances; track $index) {
            <div class="perf-card">
              <div class="perf-header">
                <span class="perf-type">{{ p.type ?? p.Type }}</span>
                <span class="perf-pct">{{ (p.pourcentage ?? p.Pourcentage) | number:'1.0-1' }}%</span>
              </div>
              <div class="progress-bar-bg">
                <div class="progress-bar-fill"
                     [style.width.%]="p.pourcentage ?? p.Pourcentage ?? 0"
                     [class.complete]="(p.pourcentage ?? p.Pourcentage) >= 100">
                </div>
              </div>
              <div class="perf-values">
                {{ p.valeurRealisee ?? p.ValeurRealisee ?? 0 }} / {{ p.valeurCible ?? p.ValeurCible ?? 0 }}
              </div>
            </div>
          }
        </div>
      </div>
    }

    @if (historique.length > 0) {
      <div class="hist-card">
        <h3>Historique</h3>
        <table class="hist-table">
          <thead>
            <tr>
              <th>Date</th>
              <th>Action</th>
              <th>Détail</th>
            </tr>
          </thead>
          <tbody>
            @for (h of historique; track $index) {
              <tr>
                <td>{{ historiqueDate(h) ? (historiqueDate(h) | date:'dd/MM/yyyy HH:mm') : '—' }}</td>
                <td>{{ historiqueAction(h) }}</td>
                <td>{{ historiqueDetail(h) }}</td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    }
  }
</div>
```

---

### FILE 9 : kpi.service.ts
**Path** : `Cynapharm/src/app/features/field/kpi/services/kpi.service.ts`

```typescript
import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../../core/services/api.service';

@Injectable({ providedIn: 'root' })
export class KpiService {
  private readonly base = '/fields/kpi';
  constructor(private api: ApiService) {}
  private u<T>(r: any): T { return r?.Result ?? r?.result ?? r; }

  getNombreVisites(idDelegue: number, debut?: string, fin?: string): Observable<any> {
    let p = new HttpParams().set('idDelegue', idDelegue);
    if (debut) p = p.set('debut', debut);
    if (fin)   p = p.set('fin', fin);
    return this.api.get<any>(`${this.base}/visites-count`, p).pipe(map(r => this.u<any>(r)));
  }

  hasVisiteAtDate(idDelegue: number, date: string): Observable<boolean> {
    const p = new HttpParams().set('idDelegue', idDelegue).set('date', date);
    return this.api.get<any>(`${this.base}/has-visite`, p).pipe(map(r => this.u<boolean>(r) ?? false));
  }

  getHistorique(idDelegue: number): Observable<any[]> {
    return this.api.get<any>(`${this.base}/historique/${idDelegue}`).pipe(map(r => this.u<any[]>(r) ?? []));
  }

  getClientFidelite(idClient: number): Observable<any> {
    return this.api.get<any>(`${this.base}/client-fidelite/${idClient}`).pipe(map(r => this.u<any>(r)));
  }

  getPerformance(idDelegue: number): Observable<any> {
    return this.api.get<any>(`${this.base}/performance/${idDelegue}`).pipe(map(r => this.u<any>(r)));
  }

  getPerformanceRate(idDelegue: number): Observable<number> {
    return this.api.get<any>(`${this.base}/performance-rate/${idDelegue}`).pipe(map(r => this.u<number>(r) ?? 0));
  }

  getTauxConversion(idDelegue: number, debut: string, fin: string): Observable<number> {
    const p = new HttpParams().set('debut', debut).set('fin', fin);
    return this.api.get<any>(`${this.base}/taux-conversion/${idDelegue}`, p).pipe(map(r => this.u<number>(r) ?? 0));
  }
}
```

---

### FILE 10 : auth.service.ts — getUserRole() and getUserId()
**Path** : `Cynapharm/src/app/core/services/auth.service.ts`

```typescript
// Relevant excerpt — getUserRole() and getUserId()

export enum UserRole {
  ADMIN = 'ADMIN',
  SUPERVISEUR = 'SUPERVISEUR',
  DELEGUE = 'DELEGUE',
  MEDECIN = 'MEDECIN',
  CLIENT = 'CLIENT'
}

export interface User {
  id: number;
  name: string;
  email: string;
  phoneNumber: string;
  adresse: string;
  role: UserRole;
  type?: UserType;
  isDeleted: boolean;
}

// Signal holding the current authenticated user
private currentUserSignal = signal<User | null>(null);

/**
 * Returns the role of the current user from the in-memory signal.
 * Returns null if no user is authenticated.
 */
getUserRole(): UserRole | null {
  return this.currentUserSignal()?.role ?? null;
}

/**
 * Returns the numeric ID of the current user.
 * Returns 0 if no user is authenticated.
 */
getUserId(): number {
  return this.currentUserSignal()?.id ?? 0;
}
```

---

### FILE 11 : app.routes.ts — dashboard route
**Path** : `Cynapharm/src/app/app.routes.ts`

```typescript
// Relevant excerpt — dashboard route

import { authGuard } from './core/guards/auth.guard';
// Note: roleGuard is imported but NOT applied to the dashboard route

// Default redirect
{ path: '', redirectTo: 'dashboard', pathMatch: 'full' },

// Dashboard route — protected by authGuard only (no roleGuard)
// ALL authenticated roles (ADMIN, SUPERVISEUR, DELEGUE, MEDECIN, CLIENT) can access it
{
  path: 'dashboard',
  loadChildren: () => import('./features/dashboard/dashboard.module').then(m => m.DashboardModule),
  canActivate: [authGuard]
},

// Wildcard fallback
{ path: '**', redirectTo: 'dashboard' }
```

---

*End of DASHBOARD_ANGULAR_ANALYSIS.md*
