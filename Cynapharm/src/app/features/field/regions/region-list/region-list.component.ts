import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { RegionService, RegionDto } from '../services/region.service';
import { UserService } from '../../../../features/users/user.service';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-region-list',
  standalone: true,
  imports: [CommonModule, RouterLink, EmptyStateComponent],
  templateUrl: './region-list.component.html',
  styleUrls: ['./region-list.component.css']
})
export class RegionListComponent implements OnInit, OnDestroy {
  regions: RegionDto[] = [];
  loading  = false;
  error    = '';

  delegueNames: Record<number, string> = {};

  private destroy$ = new Subject<void>();

  get isAdmin():       boolean { return this.authSvc.getUserRole()?.toUpperCase() === 'ADMIN'; }
  get isSuperviseur(): boolean { return this.authSvc.getUserRole()?.toUpperCase() === 'SUPERVISEUR'; }
  get isDelegue():     boolean { return this.authSvc.getUserRole()?.toUpperCase() === 'DELEGUE'; }

  constructor(
    private svc:     RegionService,
    private userSvc: UserService,
    private authSvc: AuthService,
    private cdr:     ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.load();
    if (this.isDelegue) return;
    this.userSvc.getUsersByRole('SUPERVISEUR').pipe(takeUntil(this.destroy$)).subscribe({
      next: users => {
        users.forEach(u => {
          const id = this.userSvc.userId(u);
          if (id != null) this.delegueNames[id] = this.userSvc.displayName(u, id);
        });
        this.cdr.markForCheck();
      },
      error: () => {}
    });
  }

  load(): void {
    this.loading = true;
    this.error   = '';
    const source$ = this.isDelegue
      ? this.svc.getByDelegue(this.authSvc.getUserId())
      : this.svc.getAll();
    source$.pipe(takeUntil(this.destroy$)).subscribe({
      next: data => {
        this.regions = data;
        this.resolveDelegueNames(data.filter(r => r.id_Superviseur).map(r => r.id_Superviseur!));
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => { this.error = 'Impossible de charger les régions.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  getDelegrueName(id: number): string { return this.delegueNames[id] ?? `#${id}`; }

  private resolveDelegueNames(ids: number[]): void {
    const missingIds = ids.filter(id => id && !this.delegueNames[id]);
    if (!missingIds.length) return;

    this.userSvc.getDisplayNamesByIds(missingIds).pipe(takeUntil(this.destroy$)).subscribe({
      next: names => {
        this.delegueNames = { ...this.delegueNames, ...names };
        this.cdr.markForCheck();
      },
      error: () => {}
    });
  }

  // Modale de suppression
  showDeleteModal = false;
  deletingItem: any = null;
  deleting = false;

  deleteRegion(o: any): void {
    let id = o.idRegion || o.IdRegion || o.id_Region || o.Id_Region || o.id || o.Id;
    if (!id) {
      const idKey = Object.keys(o).find(k => k.toLowerCase().includes('id'));
      if (idKey) id = o[idKey];
    }
    if (!id || id === 0) {
      alert('Impossible de trouver l\'ID de la région.');
      return;
    }
    this.deletingItem = { ...o, __computedId: id };
    this.showDeleteModal = true;
  }

  cancelDelete(): void {
    this.showDeleteModal = false;
    this.deletingItem = null;
  }

  confirmDelete(): void {
    if (!this.deletingItem || !this.deletingItem.__computedId) return;
    this.deleting = true;
    this.svc.delete(this.deletingItem.__computedId).subscribe({
      next: () => {
        this.deleting = false;
        this.showDeleteModal = false;
        this.deletingItem = null;
        this.load();
      },
      error: (err) => {
        this.deleting = false;
        this.showDeleteModal = false;
        alert('Erreur lors de la suppression. Code: ' + (err?.status || 'Inconnu'));
      }
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
