import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DistributionService, EchantillonDto } from '../services/distribution.service';
import { UserService } from '../../../users/user.service';
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
    private cdr:     ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.userSvc.getUsersByRole('DELEGUE').pipe(takeUntil(this.destroy$))
      .subscribe({ next: u => { this.delegues = u; this.buildCache(u, this.delegueNames); this.cdr.markForCheck(); }, error: () => {} });
    this.userSvc.getUsersByRole('MEDECIN').pipe(takeUntil(this.destroy$))
      .subscribe({ next: u => { this.medecins = u; this.buildCache(u, this.medecinNames); this.cdr.markForCheck(); }, error: () => {} });
    this.userSvc.getUsersByRole('PHARMACIEN').pipe(takeUntil(this.destroy$))
      .subscribe({ next: u => { this.pharmaciens = u; this.buildCache(u, this.pharmacienNames); this.cdr.markForCheck(); }, error: () => {} });
  }

  private buildCache(users: any[], cache: Record<number, string>): void {
    users.forEach(u => {
      const id = u?.id ?? u?.Id;
      if (id != null) cache[id] = u?.name ?? u?.Name ?? u?.email ?? `#${id}`;
    });
  }

  userName(u: any): string {
    return u?.name ?? u?.Name ?? u?.fullName ?? u?.email ?? `#${u?.id}`;
  }

  getDelegrueName(id: number): string    { return this.delegueNames[id]    ?? `#${id}`; }
  getMedecinName(id: number): string     { return this.medecinNames[id]    ?? `#${id}`; }
  getPharmacienName(id: number): string  { return this.pharmacienNames[id] ?? `#${id}`; }

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
    if (reset) { this.allDistributions = []; this.allPage = 1; this.hasMore = true; }
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
      error: () => { this.loadingAll = false; this.cdr.markForCheck(); }
    });
  }

  loadMore(): void { this.loadAll(); }
  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
