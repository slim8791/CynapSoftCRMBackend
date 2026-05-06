import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { ReclamationService, ReclamationDto, StatutReclamation } from '../../services/reclamation.service';
import { AuthService, UserRole } from '../../../../core/services/auth.service';
import { ToastService } from '../../../../shared/services/toast.service';

@Component({
  selector: 'app-reclamation-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './reclamation-list.component.html',
  styleUrls: ['./reclamation-list.component.css'],
})
export class ReclamationListComponent implements OnInit, OnDestroy {

  reclamations: ReclamationDto[] = [];
  loading     = false;
  error       = '';
  orderId:    number | null = null;
  clientId:   number | null = null;

  isAdmin      = false;
  isSuperviseur = false;

  // Status update inline
  updatingId:   number | null = null;
  readonly statuts = [
    { value: StatutReclamation.Ouverte, label: 'Ouverte' },
    { value: StatutReclamation.EnCours, label: 'En cours' },
    { value: StatutReclamation.Resolue, label: 'Résolue' },
  ];

  private destroy$ = new Subject<void>();

  constructor(
    readonly svc:   ReclamationService,
    private auth:   AuthService,
    private toast:  ToastService,
    private route:  ActivatedRoute,
    private router: Router,
    private cdr:    ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    const role      = this.auth.getUserRole();
    this.isAdmin      = role === UserRole.ADMIN;
    this.isSuperviseur = role === UserRole.SUPERVISEUR;

    const oid = this.route.snapshot.queryParamMap.get('orderId');
    const cid = this.route.snapshot.queryParamMap.get('clientId');
    this.orderId  = oid ? Number(oid)  : null;
    this.clientId = cid ? Number(cid)  : null;
    this.load();
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  load(): void {
    this.loading = true;
    const src$ = this.orderId  ? this.svc.getByOrder(this.orderId)
               : this.clientId ? this.svc.getByClient(this.clientId)
               : this.svc.getAll();

    src$.pipe(takeUntil(this.destroy$)).subscribe({
      next: data => { this.reclamations = data; this.loading = false; this.cdr.markForCheck(); },
      error: (err: any) => {
        const s = err?.status;
        if (s === 403) this.error = 'Accès refusé — rôle ADMIN ou SUPERVISEUR requis.';
        else if (s === 401) this.error = 'Session expirée, veuillez vous reconnecter.';
        else if (s === 0)   this.error = 'Serveur inaccessible.';
        else this.error = err?.error?.Message ?? 'Erreur lors du chargement.';
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }

  onUpdateStatus(id: number, status: StatutReclamation): void {
    this.updatingId = id;
    this.svc.updateStatus(id, status).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => { this.toast.showSuccess('Statut mis à jour.'); this.updatingId = null; this.load(); },
      error: () => { this.toast.showError('Erreur mise à jour.'); this.updatingId = null; this.cdr.markForCheck(); },
    });
  }

  onDelete(id: number): void {
    if (!confirm('Supprimer cette réclamation ?')) return;
    this.svc.delete(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => { this.toast.showSuccess('Réclamation supprimée.'); this.load(); },
      error: () => this.toast.showError('Erreur lors de la suppression.'),
    });
  }

  onView(id: number):   void { this.router.navigate(['/orders/reclamations', id]); }

  getStatutLabel = (s?: string) => this.svc.getStatutLabel(s);
  getStatutClass = (s?: string) => this.svc.getStatutClass(s);
  canManageStatus(): boolean { return this.isAdmin || this.isSuperviseur; }
}
