import { Component, OnInit, OnDestroy, ChangeDetectorRef, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { VisiteService, VisiteDto } from '../services/visite.service';
import { UserService } from '../../../users/user.service';
import { AuthService } from '../../../../core/services/auth.service';
import { VisiteType } from '../../../../core/models/enums';

@Component({
  selector: 'app-visite-all',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './visite-all.component.html',
  encapsulation: ViewEncapsulation.None,
  styles: [`
    /* ── All Visits — scoped via .va-page wrapper ─────── */
    .va-page {
      padding: 32px 40px;
      background: #f8fafc;
      min-height: 100vh;
      font-family: 'Inter', system-ui, sans-serif;
      color: #1e293b;
    }
    .va-page .va-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 32px;
    }
    .va-page .va-back {
      display: inline-block;
      font-size: 13px;
      color: #00b4d8;
      text-decoration: none;
      margin-bottom: 8px;
      font-weight: 500;
    }
    .va-page .va-back:hover { text-decoration: underline; }
    .va-page .va-title {
      font-size: 28px;
      font-weight: 800;
      color: #0f172a;
      margin: 0 0 6px;
      letter-spacing: -0.5px;
    }
    .va-page .va-sub {
      font-size: 14px;
      font-weight: 500;
      color: #64748b;
      margin: 0;
    }
    .va-page .va-error {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 14px 18px;
      background: #fef2f2;
      border: 1px solid #fecdd3;
      border-radius: 14px;
      color: #e11d48;
      font-size: 14px;
      font-weight: 500;
      margin-bottom: 24px;
    }
    /* ── Filter bar ─────────────────────────────────── */
    .va-page .va-filters {
      display: flex;
      align-items: flex-end;
      gap: 16px;
      margin-bottom: 24px;
      flex-wrap: wrap;
      background: #ffffff;
      border: 1px solid #e2e8f0;
      border-radius: 16px;
      padding: 16px 20px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.03);
    }
    .va-page .va-field {
      display: flex !important;
      flex-direction: column !important;
      gap: 6px;
    }
    .va-page .va-field > label {
      display: block !important;
      font-size: 12px !important;
      font-weight: 700 !important;
      color: #64748b !important;
      text-transform: uppercase;
      letter-spacing: 0.04em;
      margin: 0 !important;
    }
    .va-page .va-field > input,
    .va-page .va-field > select {
      display: block !important;
      padding: 10px 14px !important;
      border: 1.5px solid #e2e8f0 !important;
      border-radius: 10px !important;
      font-size: 14px !important;
      font-family: inherit !important;
      background: #f8fafc !important;
      color: #0f172a !important;
      outline: none !important;
      min-width: 160px;
      box-shadow: none !important;
    }
    .va-page .va-field > input:focus,
    .va-page .va-field > select:focus {
      border-color: #00b4d8 !important;
      background: #ffffff !important;
      box-shadow: 0 0 0 3px rgba(0,180,216,0.12) !important;
    }
    .va-page .va-field.narrow > input,
    .va-page .va-field.narrow > select { min-width: 110px; width: 110px; }
    .va-page .va-btn-refresh {
      padding: 10px 22px !important;
      border-radius: 12px !important;
      font-size: 14px !important;
      font-weight: 600 !important;
      border: 1.5px solid #e2e8f0 !important;
      background: #ffffff !important;
      color: #475569 !important;
      cursor: pointer;
      font-family: inherit !important;
      box-shadow: 0 1px 3px rgba(0,0,0,0.04) !important;
      transition: background 0.15s, border-color 0.15s;
      align-self: flex-end;
    }
    .va-page .va-btn-refresh:hover:not([disabled]) {
      background: #f1f5f9 !important;
      border-color: #cbd5e1 !important;
    }
    .va-page .va-btn-refresh[disabled] { opacity: 0.5; cursor: not-allowed; }
    /* ── Loading / Empty ─────────────────────────────── */
    .va-page .va-loading {
      display: flex;
      align-items: center;
      gap: 14px;
      color: #64748b;
      font-size: 14px;
      padding: 60px;
      justify-content: center;
    }
    .va-page .va-spinner {
      width: 22px;
      height: 22px;
      border: 3px solid #f1f5f9;
      border-top-color: #00b4d8;
      border-radius: 50%;
      animation: va-spin 0.8s linear infinite;
      flex-shrink: 0;
    }
    @keyframes va-spin { to { transform: rotate(360deg); } }
    .va-page .va-empty {
      text-align: center;
      padding: 60px;
      color: #94a3b8;
      font-size: 15px;
      font-weight: 500;
    }
    /* ── Table card ──────────────────────────────────── */
    .va-page .va-card {
      background: #ffffff;
      border: 1px solid #e2e8f0;
      border-radius: 20px;
      overflow: hidden;
      box-shadow: 0 4px 12px rgba(0,0,0,0.04);
    }
    .va-page .va-scroll { overflow-x: auto; }
    .va-page .va-table {
      width: 100%;
      border-collapse: collapse !important;
      font-size: 14px;
      text-align: left;
    }
    .va-page .va-table thead tr {
      background: #f8fafc !important;
    }
    .va-page .va-table th {
      padding: 14px 20px !important;
      font-size: 11px !important;
      font-weight: 700 !important;
      text-transform: uppercase;
      letter-spacing: 0.06em;
      color: #64748b !important;
      border-bottom: 1px solid #e2e8f0 !important;
      border-top: none !important;
      white-space: nowrap;
      background: transparent !important;
    }
    .va-page .va-table td {
      padding: 15px 20px !important;
      border-bottom: 1px solid #f1f5f9 !important;
      border-top: none !important;
      color: #374151;
      vertical-align: middle !important;
    }
    .va-page .va-table tbody tr:last-child td { border-bottom: none !important; }
    .va-page .va-table tbody tr { cursor: pointer; transition: background 0.12s; }
    .va-page .va-table tbody tr:hover td { background: #f8fafc !important; }
    .va-page .va-mono {
      font-family: 'Courier New', monospace;
      font-size: 12px;
      font-weight: 700;
      background: #f1f5f9;
      color: #475569;
      padding: 3px 8px;
      border-radius: 6px;
      border: 1px solid #e2e8f0;
    }
    /* ── Badges ──────────────────────────────────────── */
    .va-page .va-badge {
      display: inline-flex;
      align-items: center;
      padding: 4px 11px;
      border-radius: 6px;
      font-size: 11px;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.04em;
      white-space: nowrap;
    }
    .va-page .va-done    { background: #ecfdf5; color: #059669; border: 1px solid #a7f3d0; }
    .va-page .va-pending { background: #fffbeb; color: #d97706; border: 1px solid #fde68a; }

    @media (max-width: 900px) {
      .va-page { padding: 16px 20px; }
      .va-page .va-filters { gap: 12px; }
    }
  `]
})
export class VisiteAllComponent implements OnInit, OnDestroy {
  visites:  VisiteDto[] = [];
  filtered: VisiteDto[] = [];
  loading    = false;
  error      = '';
  startDate  = '';
  endDate    = '';
  delegueIdFilter: number | null = null;
  delegues: { id: number; nom: string }[] = [];
  delegueNames: Record<number, string> = {};
  VisiteType = VisiteType;

  private destroy$ = new Subject<void>();

  get isAdmin():       boolean { return this.authSvc.getUserRole()?.toUpperCase() === 'ADMIN'; }
  get isSuperviseur(): boolean { return this.authSvc.getUserRole()?.toUpperCase() === 'SUPERVISEUR'; }
  get isDelegue():     boolean { return this.authSvc.getUserRole()?.toUpperCase() === 'DELEGUE'; }

  constructor(
    private svc:     VisiteService,
    private userSvc: UserService,
    private authSvc: AuthService,
    private router:  Router,
    private cdr:     ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.userSvc.getUsersByRole('DELEGUE').pipe(takeUntil(this.destroy$)).subscribe({
      next: users => {
        this.delegues = users
          .map(u => this.userSvc.toUserOption(u))
          .filter((d): d is { id: number; nom: string } => d !== null);
        users.forEach(u => {
          const id = this.userSvc.userId(u);
          if (id != null) this.delegueNames[id] = this.userName(u, id);
        });
        this.cdr.markForCheck();
      }
    });
    this.load();
  }
  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  delegueName(id: number): string {
    return this.delegueNames[id] ?? `#${id}`;
  }

  private userName(u: any, id: number): string {
    return this.userSvc.displayName(u, id);
  }

  load(): void {
    this.loading = true;
    this.error   = '';
    this.svc.getAll(this.startDate || undefined, this.endDate || undefined)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: data => {
          this.visites = data;
          this.resolveDelegueNames(data.map(v => v.id_User_Delegue), true);
          this.applyDelegueFilter();
          this.loading = false;
          this.cdr.markForCheck();
        },
        error: () => {
          this.error   = 'Impossible de charger les visites.';
          this.loading = false;
          this.cdr.markForCheck();
        }
      });
  }

  private resolveDelegueNames(ids: number[], syncOptions = false): void {
    const uniqueIds = [...new Set(ids.filter(id => id > 0))];
    const missingIds = uniqueIds.filter(id => !this.delegueNames[id]);
    if (!missingIds.length) {
      if (syncOptions) this.syncDelegueOptions(uniqueIds);
      return;
    }

    this.userSvc.getDisplayNamesByIds(missingIds).pipe(takeUntil(this.destroy$)).subscribe({
      next: names => {
        this.delegueNames = { ...this.delegueNames, ...names };
        if (syncOptions) this.syncDelegueOptions(uniqueIds);
        this.cdr.markForCheck();
      },
      error: () => {}
    });
  }

  private syncDelegueOptions(ids: number[]): void {
    const existingIds = new Set(this.delegues.map(d => d.id));
    const additions = ids
      .filter(id => !existingIds.has(id))
      .map(id => ({ id, nom: this.delegueNames[id] ?? `#${id}` }));

    if (additions.length) this.delegues = [...this.delegues, ...additions];
  }

  applyDelegueFilter(): void {
    let list = this.delegueIdFilter
      ? this.visites.filter(v => v.id_User_Delegue === this.delegueIdFilter)
      : [...this.visites];

    // Trier par date (la plus récente d'abord)
    this.filtered = list.sort((a, b) => {
      const d1 = new Date(a.dateVisite || 0).getTime();
      const d2 = new Date(b.dateVisite || 0).getTime();
      return d2 - d1;
    });
  }

  onDelegueFilter(): void {
    this.applyDelegueFilter();
    this.cdr.markForCheck();
  }

  onRow(v: VisiteDto): void {
    if (v.idVisite) this.router.navigate(['/field/visites', v.idVisite]);
  }

  deleteVisite(id: number, isCompleted: boolean): void {
    if (isCompleted) return;
    if (!confirm('Supprimer cette visite ?')) return;
    this.svc.delete(id).subscribe({
      next: () => this.load(),
      error: () => alert('Erreur lors de la suppression.')
    });
  }

  typeLabel(t: VisiteType): string {
    switch (t) {
      case VisiteType.Medecin:    return 'Médecin';
      case VisiteType.Pharmacien: return 'Pharmacien';
      default:                    return 'Autre';
    }
  }
}
