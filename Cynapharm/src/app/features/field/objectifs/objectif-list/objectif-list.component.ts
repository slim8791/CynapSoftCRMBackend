import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ObjectifService, ObjectifDto } from '../services/objectif.service';
import { PeriodeObjectif } from '../../../../core/models/enums/index';
import { UserService } from '../../../../features/users/user.service';
import { AuthService } from '../../../../core/services/auth.service';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-objectif-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, EmptyStateComponent],
  templateUrl: './objectif-list.component.html',
  styleUrls: ['./objectif-list.component.css']
})
export class ObjectifListComponent implements OnInit, OnDestroy {
  objectifs: ObjectifDto[] = [];
  loading   = false;
  error     = '';

  delegueNames: Record<number, string> = {};
  delegues: any[] = [];
  selectedDelegueId: number | null = null;

  private destroy$ = new Subject<void>();

  get isAdmin():       boolean { return this.authSvc.getUserRole()?.toUpperCase() === 'ADMIN'; }
  get isSuperviseur(): boolean { return this.authSvc.getUserRole()?.toUpperCase() === 'SUPERVISEUR'; }
  get isDelegue():     boolean { return this.authSvc.getUserRole()?.toUpperCase() === 'DELEGUE'; }

  constructor(
    private svc:     ObjectifService,
    private userSvc: UserService,
    private authSvc: AuthService,
    private cdr:     ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.load();
    if (this.isDelegue) return;
    this.userSvc.getUsersByRole('DELEGUE').pipe(takeUntil(this.destroy$)).subscribe({
      next: users => {
        this.delegues = users;
        users.forEach(u => {
          const id = this.userSvc.userId(u);
          if (id != null) this.delegueNames[id] = this.userName(u);
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
        this.objectifs = data;
        this.resolveDelegueNames(data.map(o => o.id_User_Delegue), true);
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => { this.error = 'Impossible de charger les objectifs.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  getDelegrueName(id: number): string { return this.delegueNames[id] ?? `#${id}`; }

  get filteredObjectifs(): ObjectifDto[] {
    return this.selectedDelegueId
      ? this.objectifs.filter(o => o.id_User_Delegue === this.selectedDelegueId)
      : this.objectifs;
  }

  userName(u: any): string {
    return this.userSvc.displayName(u, this.userSvc.userId(u) ?? undefined);
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

  periodeLabel(p: number): string {
    switch (p) {
      case PeriodeObjectif.Mensuel:     return 'Mensuel';
      case PeriodeObjectif.Trimestriel: return 'Trimestriel';
      case PeriodeObjectif.Annuel:      return 'Annuel';
      default:                          return String(p);
    }
  }

  typeLabel(t: number | string): string {
    switch (Number(t)) {
      case 0: return 'Visites';
      case 1: return 'Chiffre d\'affaires';
      case 2: return 'Nouveaux clients';
      case 3: return 'Fidélisation';
      default: return String(t);
    }
  }

  progressPct(realise: number, cible: number): number {
    if (!cible || cible <= 0) return 0;
    return Math.min(100, Math.round((realise / cible) * 100));
  }

  // Variables pour la modale de suppression
  showDeleteModal = false;
  deletingObjectif: any = null;
  deleting = false;

  deleteObjectif(o: any): void {
    // Hack pour trouver l'ID peu importe la casse ou le nom exact renvoyé par le backend
    let id = o.idObjectif || o.IdObjectif || o.id_Objectif || o.Id_Objectif || o.id || o.Id;
    if (!id) {
      const idKey = Object.keys(o).find(k => k.toLowerCase().includes('id'));
      if (idKey) id = o[idKey];
    }
    
    if (!id || id === 0) {
      alert('Impossible de trouver l\'ID de cet objectif.');
      return;
    }

    this.deletingObjectif = { ...o, __computedId: id };
    this.showDeleteModal = true;
  }

  cancelDelete(): void {
    this.showDeleteModal = false;
    this.deletingObjectif = null;
  }

  confirmDelete(): void {
    console.log('Bouton Supprimer cliqué', this.deletingObjectif);
    if (!this.deletingObjectif || !this.deletingObjectif.__computedId) {
      alert('Erreur interne : ID manquant.');
      return;
    }
    this.deleting = true;
    this.svc.delete(this.deletingObjectif.__computedId).subscribe({
      next: () => {
        console.log('Suppression réussie');
        this.deleting = false;
        this.showDeleteModal = false;
        this.deletingObjectif = null;
        this.load();
      },
      error: (err) => {
        console.log('Erreur suppression', err);
        this.deleting = false;
        this.showDeleteModal = false; // Fermer la modale même en cas d'erreur
        alert('Erreur lors de la suppression. Vérifiez la console pour plus de détails. Code: ' + (err?.status || 'Inconnu'));
        console.error('Delete error:', err);
      }
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
