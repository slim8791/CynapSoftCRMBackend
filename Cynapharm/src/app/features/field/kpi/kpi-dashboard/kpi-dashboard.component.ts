import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil, catchError, of } from 'rxjs';
import { KpiService } from '../services/kpi.service';

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

  private d$ = new Subject<void>();
  constructor(private svc: KpiService, private cdr: ChangeDetectorRef) {}
  ngOnInit() {}
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

    this.svc.getHistorique(id)
      .pipe(takeUntil(this.d$), catchError(() => of([])))
      .subscribe(h => {
        this.historique = h;
        this.loading    = false;
        this.loaded     = true;
        this.cdr.markForCheck();
      });
  }
}
