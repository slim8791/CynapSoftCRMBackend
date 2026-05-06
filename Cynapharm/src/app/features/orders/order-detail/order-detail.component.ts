import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { OrderService, CommandeDto, EtatCommande } from '../order.service';
import { ReclamationService, ReclamationDto } from '../services/reclamation.service';
import { AuthService, UserRole } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './order-detail.component.html',
  styleUrls: ['./order-detail.component.css'],
})
export class OrderDetailComponent implements OnInit, OnDestroy {

  order:        CommandeDto | null = null;
  reclamations: ReclamationDto[]   = [];
  loading      = true;
  loadingRec   = false;
  error        = '';
  activeTab: 'info' | 'lignes' | 'reclamations' = 'info';

  isAdmin = false;
  showStatusModal = false;
  showDeleteModal = false;
  deleting        = false;
  statusOptions: { label: string; value: EtatCommande }[] = [];

  private orderId = 0;
  private destroy$ = new Subject<void>();

  constructor(
    private route:    ActivatedRoute,
    private router:   Router,
    private svc:      OrderService,
    private recSvc:   ReclamationService,
    private auth:     AuthService,
    private toast:    ToastService,
    private cdr:      ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.isAdmin  = this.auth.getUserRole() === UserRole.ADMIN;
    this.orderId  = Number(this.route.snapshot.paramMap.get('id'));
    this.loadOrder();
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  private loadOrder(): void {
    this.loading = true;
    this.svc.getOrderById(this.orderId).pipe(takeUntil(this.destroy$)).subscribe({
      next: order => {
        this.order   = order;
        this.loading = false;
        if (order) {
          this.statusOptions = this.svc.getNextStatuses(order.Statut);
          this.loadReclamations();
        }
        this.cdr.markForCheck();
      },
      error: () => {
        this.error   = 'Commande introuvable.';
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }

  private loadReclamations(): void {
    this.loadingRec = true;
    this.recSvc.getByOrder(this.orderId).pipe(takeUntil(this.destroy$)).subscribe({
      next: data => { this.reclamations = data; this.loadingRec = false; this.cdr.markForCheck(); },
      error: ()   => { this.reclamations = []; this.loadingRec = false; this.cdr.markForCheck(); },
    });
  }

  setTab(t: 'info' | 'lignes' | 'reclamations'): void { this.activeTab = t; }

  // Status workflow
  openStatusModal(): void {
    if (!this.order) return;
    this.statusOptions = this.svc.getNextStatuses(this.order.Statut);
    this.showStatusModal = true;
  }

  applyStatus(newStatus: EtatCommande): void {
    if (!this.order) return;
    this.showStatusModal = false;
    this.svc.updateOrderStatus({ Id_Commande: this.order.Id_Commande, NouveauStatut: newStatus })
      .pipe(takeUntil(this.destroy$)).subscribe({
        next: () => { this.toast.showSuccess('Statut mis à jour.'); this.loadOrder(); },
        error: () => this.toast.showError('Erreur lors de la mise à jour du statut.'),
      });
  }

  openDeleteModal(): void   { this.showDeleteModal = true; }
  cancelDeleteOrder(): void { this.showDeleteModal = false; }

  confirmDeleteOrder(): void {
    if (!this.order) return;
    this.deleting = true;
    this.svc.deleteOrder(this.order.Id_Commande).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => { this.toast.showSuccess('Commande supprimée.'); this.router.navigate(['/orders']); },
      error: () => { this.toast.showError('Erreur lors de la suppression.'); this.deleting = false; this.cdr.markForCheck(); },
    });
  }

  onDeleteRec(id: number): void {
    if (!confirm('Supprimer cette réclamation ?')) return;
    this.recSvc.delete(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => { this.toast.showSuccess('Réclamation supprimée.'); this.loadReclamations(); },
      error: () => this.toast.showError('Erreur lors de la suppression.'),
    });
  }

  getTotalLignes(): number { return this.order?.Lignes?.length ?? 0; }

  getEtatLabel  = (s: string) => this.svc.getEtatLabel(s);
  getEtatClass  = (s: string) => this.svc.getEtatClass(s);
  getRecLabel   = (s?: string) => this.recSvc.getStatutLabel(s);
  getRecClass   = (s?: string) => this.recSvc.getStatutClass(s);
  canChangeStatus(): boolean { return this.statusOptions.length > 0; }
}
