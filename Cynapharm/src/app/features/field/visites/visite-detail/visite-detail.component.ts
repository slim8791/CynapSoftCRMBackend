import { Component, OnInit, OnDestroy, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { VisiteService, VisiteDto } from '../services/visite.service';
import { RapportService, RapportDto } from '../../rapports/services/rapport.service';
import { UserService } from '../../../users/user.service';
import { PlanningService } from '../../plannings/services/planning.service';
import { VisiteType } from '../../../../core/models/enums';

@Component({
  selector: 'app-visite-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './visite-detail.component.html',
  encapsulation: ViewEncapsulation.None,
  styles: [`
    .vd-page { padding: 32px 40px; background: #f8fafc; min-height: 100vh; font-family: 'Inter', system-ui, sans-serif; color: #1e293b; }
    .vd-page .vd-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 32px; flex-wrap: wrap; gap: 16px; }
    .vd-page .vd-back { display: inline-flex; align-items: center; gap: 6px; font-size: 13px; font-weight: 500; color: #00b4d8; text-decoration: none; }
    .vd-page .vd-back:hover { text-decoration: underline; }
    .vd-page .vd-title-row { display: flex; align-items: center; gap: 12px; flex-wrap: wrap; margin-top: 8px; }
    .vd-page .vd-title { font-size: 26px; font-weight: 800; color: #0f172a; margin: 0; letter-spacing: -0.4px; }
    .vd-page .vd-badge { display: inline-flex; align-items: center; padding: 5px 14px; border-radius: 8px; font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.04em; }
    .vd-page .vd-done    { background: #ecfdf5; color: #059669; border: 1px solid #a7f3d0; }
    .vd-page .vd-pending { background: #fffbeb; color: #d97706; border: 1px solid #fde68a; }
    .vd-page .vd-header-actions { display: flex; gap: 10px; }
    .vd-page .vd-btn-edit { display: inline-flex; align-items: center; gap: 6px; padding: 10px 20px; background: #ffffff; border: 1.5px solid #e2e8f0; border-radius: 12px; font-size: 14px; font-weight: 600; color: #374151; text-decoration: none; cursor: pointer; transition: all 0.15s; }
    .vd-page .vd-btn-edit:hover { background: #f8fafc; border-color: #cbd5e1; }
    .vd-page .vd-btn-complete { display: inline-flex; align-items: center; gap: 6px; padding: 10px 20px; background: linear-gradient(135deg, #00b4d8, #0077b6); border: none; border-radius: 12px; font-size: 14px; font-weight: 600; color: #ffffff; cursor: pointer; box-shadow: 0 4px 12px rgba(0,119,182,0.25); transition: all 0.15s; }
    .vd-page .vd-btn-complete:hover:not(:disabled) { transform: translateY(-1px); box-shadow: 0 6px 16px rgba(0,119,182,0.35); }
    .vd-page .vd-btn-complete:disabled { opacity: 0.6; cursor: not-allowed; }
    /* Cards */
    .vd-page .vd-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; margin-bottom: 20px; }
    .vd-page .vd-card { background: #ffffff; border: 1px solid #e2e8f0; border-radius: 20px; padding: 24px 28px; box-shadow: 0 2px 8px rgba(0,0,0,0.03); }
    .vd-page .vd-card-title { font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.06em; color: #94a3b8; margin: 0 0 18px; }
    .vd-page .vd-rows { display: flex; flex-direction: column; gap: 14px; }
    .vd-page .vd-row { display: flex; align-items: flex-start; gap: 12px; }
    .vd-page .vd-row-icon { width: 32px; height: 32px; border-radius: 8px; background: #f1f5f9; display: flex; align-items: center; justify-content: center; flex-shrink: 0; font-size: 15px; }
    .vd-page .vd-row-body { display: flex; flex-direction: column; gap: 2px; }
    .vd-page .vd-row-label { font-size: 11px; font-weight: 600; color: #94a3b8; text-transform: uppercase; letter-spacing: 0.04em; }
    .vd-page .vd-row-val { font-size: 14px; font-weight: 600; color: #1e293b; }
    .vd-page .vd-row-val.muted { color: #94a3b8; font-weight: 400; font-style: italic; }
    /* Rapport card */
    .vd-page .vd-rapport-card { background: #ffffff; border: 1px solid #e2e8f0; border-radius: 20px; padding: 24px 28px; box-shadow: 0 2px 8px rgba(0,0,0,0.03); }
    .vd-page .vd-rapport-empty { display: flex; flex-direction: column; align-items: center; gap: 12px; padding: 24px 0 8px; text-align: center; }
    .vd-page .vd-rapport-empty-icon { width: 48px; height: 48px; background: #f1f5f9; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-size: 22px; }
    .vd-page .vd-rapport-empty p { font-size: 14px; color: #64748b; margin: 0; }
    .vd-page .vd-btn-rapport { display: inline-flex; align-items: center; gap: 8px; padding: 10px 22px; background: linear-gradient(135deg, #00b4d8, #0077b6); color: #fff; border: none; border-radius: 12px; font-size: 14px; font-weight: 600; text-decoration: none; cursor: pointer; box-shadow: 0 4px 12px rgba(0,119,182,0.22); transition: all 0.15s; }
    .vd-page .vd-btn-rapport:hover { transform: translateY(-1px); box-shadow: 0 6px 16px rgba(0,119,182,0.32); }
    .vd-page .vd-rapport-exists { display: flex; flex-direction: column; gap: 14px; }
    .vd-page .vd-rapport-meta { display: flex; align-items: center; gap: 10px; flex-wrap: wrap; }
    .vd-page .vd-resultat { display: inline-flex; align-items: center; padding: 4px 12px; border-radius: 6px; font-size: 12px; font-weight: 700; background: #f0f9ff; color: #0077b6; border: 1px solid #bae6fd; }
    .vd-page .vd-rapport-date { font-size: 12px; color: #94a3b8; }
    .vd-page .vd-rapport-comment { font-size: 14px; color: #475569; line-height: 1.6; background: #f8fafc; border-radius: 10px; padding: 12px 14px; border: 1px solid #f1f5f9; }
    .vd-page .vd-btn-rapport-edit { display: inline-flex; align-items: center; gap: 6px; padding: 9px 18px; background: #fff; border: 1.5px solid #e2e8f0; border-radius: 10px; font-size: 13px; font-weight: 600; color: #374151; text-decoration: none; }
    .vd-page .vd-btn-rapport-edit:hover { background: #f8fafc; border-color: #cbd5e1; }
    /* Loading / error */
    .vd-page .vd-loading { display: flex; align-items: center; gap: 14px; color: #64748b; font-size: 14px; padding: 80px; justify-content: center; }
    .vd-page .vd-spinner { width: 24px; height: 24px; border: 3px solid #f1f5f9; border-top-color: #00b4d8; border-radius: 50%; animation: vd-spin 0.8s linear infinite; }
    @keyframes vd-spin { to { transform: rotate(360deg); } }
    .vd-page .vd-error { padding: 14px 18px; background: #fef2f2; border: 1px solid #fecdd3; border-radius: 14px; color: #e11d48; font-size: 14px; font-weight: 500; margin-bottom: 24px; }
    .vd-page .vd-success { padding: 14px 18px; background: #f0fdf4; border: 1px solid #a7f3d0; border-radius: 14px; color: #059669; font-size: 14px; font-weight: 500; margin-bottom: 24px; }
    @media (max-width: 900px) { .vd-page { padding: 16px 20px; } .vd-page .vd-grid { grid-template-columns: 1fr; } }
  `]
})
export class VisiteDetailComponent implements OnInit, OnDestroy {
  visite: VisiteDto | null = null;
  rapport: RapportDto | null = null;
  delegueName = '';
  medecinName = '';
  pharmacienName = '';
  loading     = true;
  completing  = false;
  error       = '';
  successMsg  = '';
  planningLabel = '';

  private destroy$ = new Subject<void>();

  constructor(
    private route:       ActivatedRoute,
    private router:      Router,
    private visiteSvc:   VisiteService,
    private rapportSvc:  RapportService,
    private userSvc:     UserService,
    private planningSvc: PlanningService,
    private cdr:         ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) { this.router.navigate(['/field/visites/all']); return; }

    this.visiteSvc.getById(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: v => {
        this.visite  = v;
        this.loading = false;
        this.cdr.markForCheck();
        this.loadDelegheName(v.id_User_Delegue);
        if (v.id_Medecin) this.loadLinkedUserName(v.id_Medecin, name => this.medecinName = name);
        if (v.id_Pharmacien) this.loadLinkedUserName(v.id_Pharmacien, name => this.pharmacienName = name);
        this.loadRapport(id);
        if (v.id_Planning) this.loadPlanningLabel(v.id_Planning);
      },
      error: () => { this.error = 'Impossible de charger la visite.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  private loadDelegheName(id: number): void {
    this.loadLinkedUserName(id, name => this.delegueName = name);
  }

  private loadLinkedUserName(id: number, assign: (name: string) => void): void {
    this.userSvc.getUserById(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: r => { assign(this.userSvc.displayName(r, id)); this.cdr.markForCheck(); },
      error: () => { assign(`#${id}`); this.cdr.markForCheck(); }
    });
  }

  private loadRapport(visiteId: number): void {
    this.rapportSvc.getByVisite(visiteId).pipe(takeUntil(this.destroy$)).subscribe({
      next: (list: any) => {
        const arr: RapportDto[] = Array.isArray(list) ? list : (list?.Result ?? list?.result ?? []);
        this.rapport = arr.length ? arr[0] : null;
        this.cdr.markForCheck();
      },
      error: () => { this.rapport = null; this.cdr.markForCheck(); }
    });
  }

  private loadPlanningLabel(idPlanning: number): void {
    this.planningSvc.getById(idPlanning).pipe(takeUntil(this.destroy$)).subscribe({
      next: (p: any) => {
        if (p) {
          const date = new Date(p.date).toLocaleDateString('fr-FR');
          const debut = p.heureDebut?.substring(0, 5) ?? '';
          const fin   = p.heureFin?.substring(0, 5)   ?? '';
          this.planningLabel = `Planning du ${date} ${debut}–${fin}`;
        } else {
          this.planningLabel = `#${idPlanning}`;
        }
        this.cdr.markForCheck();
      },
      error: () => { this.planningLabel = `#${idPlanning}`; this.cdr.markForCheck(); }
    });
  }

  complete(): void {
    if (!this.visite?.idVisite || this.completing) return;
    this.completing = true;
    this.visiteSvc.complete(this.visite.idVisite).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.visite!.isCompleted = true;
        this.completing = false;
        this.successMsg = 'Visite marquée comme terminée.';
        this.cdr.markForCheck();
      },
      error: () => { this.error = 'Erreur lors de la mise à jour.'; this.completing = false; this.cdr.markForCheck(); }
    });
  }

  typeLabel(t: VisiteType): string {
    switch (t) {
      case VisiteType.Medecin:    return 'Médecin';
      case VisiteType.Pharmacien: return 'Pharmacien';
      default:                    return 'Autre';
    }
  }

  typeIcon(t: VisiteType): string {
    switch (t) {
      case VisiteType.Medecin:    return '🩺';
      case VisiteType.Pharmacien: return '💊';
      default:                    return '📋';
    }
  }

  get rapportNewLink(): string {
    return `/field/rapports/new${this.visite?.idVisite ? '?visiteId=' + this.visite.idVisite : ''}`;
  }
}
