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

    // Taux de livraison = Livrées / (Total - Annulées) × 100
    const denominator = stats.totalOrders - stats.countAnnulees;
    this.tauxLivraison = denominator > 0
      ? Math.round((stats.countLivrees / denominator) * 100)
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
