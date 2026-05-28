import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DistributionService, EchantillonDto } from '../services/distribution.service';
import { UserService } from '../../../users/user.service';
import { ToastService } from '../../../../shared/services/toast.service';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';

type TabKey = 'by-delegue' | 'by-medecin' | 'by-pharmacien' | 'all';

@Component({
  selector: 'app-distribution-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, EmptyStateComponent],
  templateUrl: './distribution-list.component.html',
  styleUrls: ['./distribution-list.component.css']
})
export class DistributionListComponent implements OnInit, OnDestroy {
  distributions: EchantillonDto[] = [];
  loading   = false;
  error     = '';
  searched  = false;
  activeTab: TabKey = 'by-delegue';
  selectedUserId: number | null = null;

  allDistributions: EchantillonDto[] = [];
  loadingAll = false;
  allPage    = 1;
  allPageSize = 20;
  hasMore    = true;
  errorAll   = '';

  // user lists for filter dropdowns
  delegues:    any[] = [];
  medecins:    any[] = [];
  pharmaciens: any[] = [];

  // resolved name caches
  delegueNames:    Record<number, string> = {};
  medecinNames:    Record<number, string> = {};
  pharmacienNames: Record<number, string> = {};

  tabs = [
    { key: 'by-delegue'    as TabKey, label: 'Par délégué' },
    { key: 'by-medecin'    as TabKey, label: 'Par médecin' },
    { key: 'by-pharmacien' as TabKey, label: 'Par pharmacien' },
    { key: 'all'           as TabKey, label: 'Toutes' }
  ];

  get tabLabel(): string {
    return this.tabs.find(t => t.key === this.activeTab)?.label.replace('Par ', '') ?? '';
  }

  get filterUsers(): any[] {
    if (this.activeTab === 'by-delegue')    return this.delegues;
    if (this.activeTab === 'by-medecin')    return this.medecins;
    if (this.activeTab === 'by-pharmacien') return this.pharmaciens;
    return [];
  }

  private destroy$ = new Subject<void>();

  constructor(
    private svc:     DistributionService,
    private userSvc: UserService,
    private toast:   ToastService,
    private cdr:     ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.userSvc.getAllUsers().pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res: any) => {
          const users = res?.result ?? res?.Result ?? res ?? [];

          // Build ONE global cache with ALL users so every id resolves regardless of role
          this.buildCache(users, this.delegueNames);
          this.buildCache(users, this.medecinNames);
          this.buildCache(users, this.pharmacienNames);

          // Filter for dropdowns
          this.delegues = users.filter((x: any) =>
            (x.role ?? x.Role ?? '').toLowerCase() === 'delegue' && !x.isDeleted
          );
          this.medecins = users.filter((x: any) =>
            (x.role ?? x.Role ?? '').toLowerCase() === 'medecin' && !x.isDeleted
          );
          this.pharmaciens = users.filter((x: any) =>
            ['client', 'pharmacien', 'grossiste'].includes(
              (x.role ?? x.Role ?? '').toLowerCase()
            ) && !x.isDeleted
          );

          console.log('All users loaded:', users.length);
          console.log('delegueNames:', this.delegueNames);

          setTimeout(() => {
            console.log('=== DISTRIBUTION DEBUG ===');
            console.log('delegueNames cache:', this.delegueNames);
            console.log('medecinNames cache:', this.medecinNames);
            console.log('pharmacienNames cache:', this.pharmacienNames);
            console.log('allDistributions:', this.allDistributions);
            if (this.allDistributions.length > 0) {
              const d = this.allDistributions[0];
              console.log('first dist id_Delegue:', d.id_Delegue, typeof d.id_Delegue);
              console.log('cache lookup result:', this.delegueNames[d.id_Delegue]);
              console.log('cache lookup +id:', this.delegueNames[+d.id_Delegue]);
            }
          }, 2000);

          this.cdr.markForCheck();
        },
        error: (err) => {
          console.error('Failed to load users:', err);
        }
      });
  }

  private buildCache(users: any[], cache: Record<number, string>): void {
    users.forEach(u => {
      const id = +(u?.id ?? u?.Id ?? u?.id_User ?? u?.Id_User ?? 0);
      if (!id) return;
      const name = u?.fullName ?? u?.FullName ?? u?.name ?? u?.Name ?? u?.email ?? `#${id}`;
      cache[id] = name;
      console.log(`buildCache: id=${id} → name=${name}`);
    });
  }

  userName(u: any): string {
    return u?.name ?? u?.Name ?? u?.fullName ?? u?.email ?? `#${u?.id}`;
  }

  getDelegrueName(id: any): string {
    const numId = +id;
    const name = this.delegueNames[numId];
    console.log(`getDelegrueName(${id}) → numId=${numId}, name=${name}`);
    return name ?? `#${id}`;
  }
  getMedecinName(id: any): string    { const numId = +id; return this.medecinNames[numId]    ?? `#${id}`; }
  getPharmacienName(id: any): string { const numId = +id; return this.pharmacienNames[numId] ?? `#${id}`; }

  switchTab(key: TabKey): void {
    this.activeTab = key;
    this.distributions = [];
    this.selectedUserId = null;
    this.searched = false;
    this.error = '';
    if (key === 'all' && this.allDistributions.length === 0) this.loadAll(true);
  }

  load(): void {
    if (!this.selectedUserId) return;
    this.loading = true;
    this.error   = '';
    this.searched = true;

    const id = this.selectedUserId;
    const obs$ = this.activeTab === 'by-delegue'
      ? this.svc.getByDelegue(id)
      : this.activeTab === 'by-medecin'
        ? this.svc.getByMedecin(id)
        : this.svc.getByPharmacien(id);

    obs$.pipe(takeUntil(this.destroy$)).subscribe({
      next: data => { this.distributions = data; this.loading = false; this.cdr.markForCheck(); },
      error: () => { this.error = 'Impossible de charger les distributions.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  loadAll(reset = false): void {
    if (reset) { this.allDistributions = []; this.allPage = 1; this.hasMore = true; this.errorAll = ''; }
    if (this.loadingAll) return;
    this.loadingAll = true;
    this.svc.getAll(this.allPage, this.allPageSize).pipe(takeUntil(this.destroy$)).subscribe({
      next: data => {
        this.allDistributions = [...this.allDistributions, ...data];
        this.hasMore = data.length === this.allPageSize;
        this.allPage++;
        this.loadingAll = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.errorAll = 'Impossible de charger les distributions. Vérifiez vos droits d\'accès.';
        this.loadingAll = false;
        this.cdr.markForCheck();
      }
    });
  }

  loadMore(): void { this.loadAll(); }

  // ── Delete modal ──────────────────────────────────────
  showDeleteModal  = false;
  pendingDeleteId: number | null = null;

  onDelete(id: number): void {
    this.pendingDeleteId = id;
    this.showDeleteModal = true;
    this.cdr.markForCheck();
  }

  confirmDelete(): void {
    if (this.pendingDeleteId == null) return;
    const id = this.pendingDeleteId;
    this.showDeleteModal  = false;
    this.pendingDeleteId  = null;
    this.svc.delete(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toast.showSuccess('Distribution supprimée.');
        if (this.activeTab === 'all') this.loadAll(true);
        else this.load();
      },
      error: () => this.toast.showError('Erreur de suppression.')
    });
  }

  cancelDelete(): void {
    this.showDeleteModal  = false;
    this.pendingDeleteId  = null;
    this.cdr.markForCheck();
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
