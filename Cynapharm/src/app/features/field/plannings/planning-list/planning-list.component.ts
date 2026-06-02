import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { PlanningService, PlanningDto } from '../services/planning.service';
import { EtatPlanning, PLANNING_STATUS_LABELS } from '../../../../core/models/enums/index';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { UserService } from '../../../users/user.service';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-planning-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, EmptyStateComponent],
  templateUrl: './planning-list.component.html',
  styleUrls: ['./planning-list.component.css']
})
export class PlanningListComponent implements OnInit, OnDestroy {
  plannings: PlanningDto[] = [];
  loading = false;
  error = '';
  searched = false;
  delegueId: number | null = null;
  EtatPlanning = EtatPlanning;
  delegues: any[] = [];
  delegueNames: Record<number, string> = {};
  currentUserId = 0;

  private destroy$ = new Subject<void>();

  get isAdmin():       boolean { return this.authSvc.getUserRole()?.toUpperCase() === 'ADMIN'; }
  get isSuperviseur(): boolean { return this.authSvc.getUserRole()?.toUpperCase() === 'SUPERVISEUR'; }
  get isDelegue():     boolean { return this.authSvc.getUserRole()?.toUpperCase() === 'DELEGUE'; }

  constructor(
    private svc: PlanningService,
    private userSvc: UserService,
    private authSvc: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.currentUserId = this.authSvc.getUserId();
    if (this.isDelegue) {
      this.delegueId = this.currentUserId;
      this.load();
      return;
    }
    this.userSvc.getUsersByRole('DELEGUE').pipe(takeUntil(this.destroy$)).subscribe({
      next: users => {
        if (!users.length) {
          this.loadDelegueOptionsFromPlannings();
          return;
        }
        this.delegues = users;
        users.forEach(u => {
          const id = this.userSvc.userId(u);
          if (id != null) this.delegueNames[id] = this.userName(u);
        });
        this.cdr.markForCheck();
      },
      error: () => this.loadDelegueOptionsFromPlannings()
    });
  }

  load(): void {
    if (!this.delegueId) return;
    this.loading = true;
    this.error = '';
    this.searched = true;
    this.svc.getByDelegue(this.delegueId).pipe(takeUntil(this.destroy$)).subscribe({
      next: data => {
        this.plannings = data;
        this.resolveDelegueNames(data.map(p => p.id_User_Delegue), true);
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => { this.error = 'Impossible de charger les plannings.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  statusLabel(e: EtatPlanning): string {
    return PLANNING_STATUS_LABELS[e] ?? String(e);
  }

  userName(u: any): string {
    return this.userSvc.displayName(u, this.userSvc.userId(u) ?? undefined);
  }

  delegueName(id: number): string {
    return this.delegueNames[id] ?? `#${id}`;
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

  private loadDelegueOptionsFromPlannings(): void {
    this.svc.getAll().pipe(takeUntil(this.destroy$)).subscribe({
      next: plannings => this.resolveDelegueNames(plannings.map(p => p.id_User_Delegue), true),
      error: () => {}
    });
  }

  private syncDelegueOptions(ids: number[]): void {
    const existingIds = new Set(
      this.delegues
        .map(d => this.userSvc.userId(d))
        .filter((id): id is number => id != null)
    );
    const additions = ids
      .filter(id => !existingIds.has(id))
      .map(id => ({ id, nom: this.delegueNames[id] ?? `#${id}` }));

    if (additions.length) this.delegues = [...this.delegues, ...additions];
  }

  // Modale d'activation
  showActivateModal = false;
  activatingItem: any = null;
  activating = false;

  validatePlanning(p: any): void {
    if (p.etat === EtatPlanning.Confirme) return;
    this.activatingItem = p;
    this.showActivateModal = true;
    this.cdr.markForCheck();
  }

  cancelActivate(): void {
    this.showActivateModal = false;
    this.activatingItem = null;
    this.cdr.markForCheck();
  }

  confirmActivate(): void {
    if (!this.activatingItem) return;
    let id = this.activatingItem.idPlanning || this.activatingItem.IdPlanning || this.activatingItem.id_Planning || this.activatingItem.Id_Planning || this.activatingItem.id || this.activatingItem.Id;
    if (!id) {
      const idKey = Object.keys(this.activatingItem).find(k => k.toLowerCase().includes('id'));
      if (idKey) id = this.activatingItem[idKey];
    }
    if (!id) {
      alert("Impossible de trouver l'ID du planning.");
      return;
    }
    this.activating = true;
    this.cdr.markForCheck();
    this.svc.validate(id).subscribe({
      next: () => {
        this.activating = false;
        this.showActivateModal = false;
        this.activatingItem = null;
        this.load();
        this.cdr.markForCheck();
      },
      error: () => {
        this.activating = false;
        this.showActivateModal = false;
        alert('Erreur lors de la validation.');
        this.cdr.markForCheck();
      }
    });
  }

  // Modale de suppression
  showDeleteModal = false;
  deletingItem: any = null;
  deleting = false;

  deletePlanning(o: any): void {
    let id = o.idPlanning || o.IdPlanning || o.id_Planning || o.Id_Planning || o.id || o.Id;
    if (!id) {
      const idKey = Object.keys(o).find(k => k.toLowerCase().includes('id'));
      if (idKey) id = o[idKey];
    }
    if (!id || id === 0) {
      alert('Impossible de trouver l\'ID du planning.');
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
        if (err.status === 400)
          alert('Impossible de supprimer un planning confirmé.');
        else
          alert('Erreur lors de la suppression. Code: ' + (err?.status || 'Inconnu'));
      }
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
