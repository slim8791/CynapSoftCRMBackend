import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { OrderService, CommandeDto, EtatCommande, ETAT_LABELS } from '../order.service';
import { AuthService, UserRole } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/services/toast.service';
import { UserService } from '../../users/user.service';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './order-list.component.html',
  styleUrls: ['./order-list.component.css'],
})
export class OrderListComponent implements OnInit, OnDestroy {

  orders:  CommandeDto[] = [];
  loading  = false;
  error    = '';
  clientNames: Record<number, string> = {};

  // Filtres
  statusFilter = '';
  startDate    = '';
  endDate      = '';
  readonly statuses = [
    { value: '',               label: 'Tous' },
    { value: 'Brouillon',     label: 'Brouillon' },
    { value: 'EnAttente',     label: 'En attente' },
    { value: 'Confirmee',     label: 'Confirmée' },
    { value: 'EnPreparation', label: 'En préparation' },
    { value: 'Expediee',      label: 'Expédiée' },
    { value: 'Livree',        label: 'Livrée' },
    { value: 'Annulee',       label: 'Annulée' },
  ];

  // Pagination
  currentPage = 1;
  readonly pageSize = 15;
  hasMore = true;

  // Rôles
  isAdmin = false;

  // Modal suppression
  showDeleteModal   = false;
  deletingOrder: CommandeDto | null = null;
  deleting          = false;

  // Changement de statut
  activeStatusMenuId: number | null = null;
  changingStatus = false;

  // Réclamations
  reclamationsTotal = 0;

  private destroy$ = new Subject<void>();

  constructor(
    readonly svc:     OrderService,
    private auth:     AuthService,
    private toast:    ToastService,
    private router:   Router,
    private cdr:      ChangeDetectorRef,
    private userSvc:  UserService,
  ) {}

  ngOnInit(): void {
    const role = this.auth.getUserRole();
    this.isAdmin = role === UserRole.ADMIN;
    this.loadClients();
    this.loadDashboard();
    this.load();
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  // ── Clients ───────────────────────────────────────────

  private loadClients(): void {
    this.userSvc.getUsers().pipe(takeUntil(this.destroy$)).subscribe({
      next: (res: any) => {
        const raw: any[] = Array.isArray(res)
          ? res
          : res?.Result ?? res?.result ?? res?.data ?? [];
        raw.forEach(u => {
          const id: number = u.id ?? u.Id;
          if (id != null && id > 0) {
            this.clientNames[id] =
              u.fullName ?? u.FullName ??
              u.name     ?? u.Name     ??
              u.userName ?? u.UserName ??
              u.email    ?? u.Email    ?? `Client #${id}`;
          }
        });
        this.cdr.markForCheck();
      },
      error: () => { /* non-critical */ },
    });
  }

  getClientName(id: number): string {
    if (!id || id === 0) return 'Client inconnu';
    return this.clientNames[id] ?? `Client #${id}`;
  }

  // ── Dashboard (réclamations count) ───────────────────

  private loadDashboard(): void {
    this.svc.getOrdersDashboard().pipe(takeUntil(this.destroy$)).subscribe({
      next: d => {
        this.reclamationsTotal = (d.ReclamationsOuvertes ?? 0) + (d.ReclamationsEnCours ?? 0);
        this.cdr.markForCheck();
      },
      error: () => { /* non-critical */ },
    });
  }

  // ── Chargement commandes ──────────────────────────────

  load(): void {
    this.loading = true;
    this.error   = '';
    this.cdr.markForCheck();

    this.svc.getOrders(
      this.currentPage, this.pageSize,
      this.statusFilter || undefined,
      this.startDate    || undefined,
      this.endDate      || undefined
    )
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: data => {
          this.orders  = data;
          this.hasMore = data.length === this.pageSize;
          this.loading = false;
          this.cdr.markForCheck();
        },
        error: (err: any) => {
          const status = err?.status;
          if      (status === 403) this.error = 'Accès refusé — rôle ADMIN, SUPERVISEUR ou DÉLÉGUÉ requis.';
          else if (status === 401) this.error = 'Session expirée, veuillez vous reconnecter.';
          else if (status === 0)   this.error = 'Serveur inaccessible — vérifiez que l\'OrderAPI est démarrée.';
          else this.error = err?.error?.Message ?? err?.message ?? 'Erreur lors du chargement.';
          this.loading = false;
          this.cdr.markForCheck();
        },
      });
  }

  onPageChange(page: number): void { this.currentPage = page; this.load(); }
  onFilterChange(): void { this.currentPage = 1; this.load(); }
  clearFilters(): void {
    this.statusFilter = ''; this.startDate = ''; this.endDate = '';
    this.onFilterChange();
  }

  onView(id: number): void { this.router.navigate(['/orders', id]); }

  // ── Suppression ───────────────────────────────────────

  /** Fix 1 — only Brouillon(0) or Annulee(6) may be deleted */
  canDelete(order: CommandeDto): boolean {
    const n = this.svc.statutToNumber(order.Statut);
    return n === EtatCommande.Brouillon || n === EtatCommande.Annulee;
  }

  openDeleteModal(order: CommandeDto): void {
    this.deletingOrder   = order;
    this.showDeleteModal = true;
  }

  cancelDelete(): void {
    this.showDeleteModal = false;
    this.deletingOrder   = null;
  }

  confirmDelete(): void {
    if (!this.deletingOrder) return;
    this.deleting = true;
    this.svc.deleteOrder(this.deletingOrder.Id_Commande)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toast.showSuccess(`Commande #${this.deletingOrder!.Id_Commande} supprimée.`);
          this.showDeleteModal = false;
          this.deletingOrder   = null;
          this.deleting        = false;
          this.load();
        },
        error: () => {
          this.toast.showError('Erreur lors de la suppression.');
          this.deleting = false;
          this.cdr.markForCheck();
        },
      });
  }

  // ── Changement de statut ──────────────────────────────

  getNextStatuses(order: CommandeDto) {
    return this.svc.getNextStatuses(order.Statut);
  }

  toggleStatusMenu(orderId: number): void {
    this.activeStatusMenuId = this.activeStatusMenuId === orderId ? null : orderId;
    this.cdr.markForCheck();
  }

  closeStatusMenu(): void {
    this.activeStatusMenuId = null;
    this.cdr.markForCheck();
  }

  changeStatus(order: CommandeDto, newStatut: EtatCommande): void {
    this.activeStatusMenuId = null;
    this.changingStatus     = true;
    this.svc.updateOrderStatus({ Id_Commande: order.Id_Commande, NouveauStatut: newStatut })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toast.showSuccess(`Statut mis à jour → ${ETAT_LABELS[newStatut]}`);
          this.changingStatus = false;
          this.load();
        },
        error: () => {
          this.toast.showError('Erreur lors du changement de statut.');
          this.changingStatus = false;
          this.cdr.markForCheck();
        },
      });
  }

  // ── Helpers ───────────────────────────────────────────

  getEtatLabel  = (s: string | number) => this.svc.getEtatLabel(s);
  getEtatClass  = (s: string | number) => this.svc.getEtatClass(s);
  getLignesCount(o: CommandeDto): number { return o.Lignes?.length ?? 0; }
}
