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
    const act = entry?.action ?? entry?.Action ?? entry?.type ?? entry?.Type ?? entry?.event ?? entry?.Event;
    
    // Convertir les codes numériques du backend en texte lisible
    if (act == 1 || act === '1') return 'Démarrage de visite';
    if (act == 2 || act === '2') return 'Clôture de visite / Rapport';
    if (act == 3 || act === '3') return 'Création de commande';
    if (act == 4 || act === '4') return 'Distribution d\'échantillon';
    
    return act ? String(act) : '—';
  }

  historiqueDetail(entry: any): string {
    const detail = entry?.detail ?? entry?.Detail ?? entry?.description ?? entry?.Description ?? entry?.message ?? entry?.Message;
    return detail ? detail : 'Aucun détail fourni';
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
