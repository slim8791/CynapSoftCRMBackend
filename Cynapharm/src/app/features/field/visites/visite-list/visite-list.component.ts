import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { firstValueFrom, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { VisiteService, VisiteDto } from '../services/visite.service';
import { VisiteType } from '../../../../core/models/enums/index';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { UserService } from '../../../users/user.service';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-visite-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, EmptyStateComponent],
  templateUrl: './visite-list.component.html',
  styleUrls: ['./visite-list.component.css']
})
export class VisiteListComponent implements OnInit, OnDestroy {
  visites: VisiteDto[] = [];
  loading = false;
  error = '';
  searched = false;
  delegueId: number | null = null;
  VisiteType = VisiteType;
  delegues: any[] = [];
  delegueNames: Record<number, string> = {};
  medecinNames: Partial<Record<number, string>> = {};
  pharmacienNames: Partial<Record<number, string>> = {};

  private destroy$ = new Subject<void>();

  get isAdmin():       boolean { return this.authSvc.getUserRole()?.toUpperCase() === 'ADMIN'; }
  get isSuperviseur(): boolean { return this.authSvc.getUserRole()?.toUpperCase() === 'SUPERVISEUR'; }
  get isDelegue():     boolean { return this.authSvc.getUserRole()?.toUpperCase() === 'DELEGUE'; }

  constructor(
    private svc: VisiteService,
    private userSvc: UserService,
    private authSvc: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    if (this.isDelegue) {
      this.delegueId = this.authSvc.getUserId();
      this.load();
      return;
    }
    this.userSvc.getUsersByRole('DELEGUE').pipe(takeUntil(this.destroy$)).subscribe({
      next: users => {
        if (!users.length) {
          this.loadDelegueOptionsFromVisites();
          return;
        }
        this.delegues = users;
        users.forEach(u => {
          const id = this.userSvc.userId(u);
          if (id != null) this.delegueNames[id] = this.userName(u);
        });
        this.cdr.markForCheck();
      },
      error: () => this.loadDelegueOptionsFromVisites()
    });
  }

  load(): void {
    if (!this.delegueId) return;
    this.loading = true;
    this.error = '';
    this.searched = true;
    this.svc.getByDelegue(this.delegueId).pipe(takeUntil(this.destroy$)).subscribe({
      next: async data => {
        this.visites = data;
        this.resolveDelegueNames(data.map(v => v.id_User_Delegue), true);
        try {
          await this.resolveParticipantNames(data);
        } finally {
          this.loading = false;
          this.cdr.markForCheck();
        }
      },
      error: () => { this.error = 'Impossible de charger les visites.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  visiteTypeLabel(t: VisiteType): string {
    switch (t) {
      case VisiteType.Medecin:    return 'Médecin';
      case VisiteType.Pharmacien: return 'Pharmacien';
      default:                    return 'Autre';
    }
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

  private loadDelegueOptionsFromVisites(): void {
    this.svc.getAll().pipe(takeUntil(this.destroy$)).subscribe({
      next: visites => this.resolveDelegueNames(visites.map(v => v.id_User_Delegue), true),
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

  private async resolveParticipantNames(visites: VisiteDto[]): Promise<void> {
    const medecinIds = [...new Set(visites.filter(v => v.id_Medecin).map(v => v.id_Medecin!))];
    const pharmacienIds = [...new Set(visites.filter(v => v.id_Pharmacien).map(v => v.id_Pharmacien!))];

    await Promise.all([
      ...medecinIds.filter(id => this.medecinNames[id] == null).map(async id => {
        this.medecinNames[id] = await this.resolveUserName(id);
      }),
      ...pharmacienIds.filter(id => this.pharmacienNames[id] == null).map(async id => {
        this.pharmacienNames[id] = await this.resolveUserName(id);
      })
    ]);
  }

  private async resolveUserName(id: number): Promise<string> {
    try {
      const response = await firstValueFrom(this.userSvc.getUserById(id));
      return this.userSvc.displayName(response, id);
    } catch {
      return `#${id}`;
    }
  }

  // Modale de suppression
  showDeleteModal = false;
  deletingItem: any = null;
  deleting = false;

  deleteVisite(o: any): void {
    let id = o.idVisite || o.IdVisite || o.id_Visite || o.Id_Visite || o.id || o.Id;
    if (!id) {
      const idKey = Object.keys(o).find(k => k.toLowerCase().includes('id'));
      if (idKey) id = o[idKey];
    }
    if (!id || id === 0) {
      alert('Impossible de trouver l\'ID de la visite.');
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
