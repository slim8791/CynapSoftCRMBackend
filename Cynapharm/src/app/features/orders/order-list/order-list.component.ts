import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { OrderService, CommandeDto } from '../order.service';
import { AuthService, UserRole } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './order-list.component.html',
  styleUrls: ['./order-list.component.css'],
})
export class OrderListComponent implements OnInit, OnDestroy {

  orders:  CommandeDto[] = [];
  loading  = false;
  error    = '';

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

  private destroy$ = new Subject<void>();

  constructor(
    readonly svc:   OrderService,
    private auth:  AuthService,
    private toast: ToastService,
    private router: Router,
    private cdr:   ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    const role = this.auth.getUserRole();
    this.isAdmin = role === UserRole.ADMIN;
    this.load();
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  load(): void {
    this.loading = true;
    this.error   = '';
    this.cdr.markForCheck();

    this.svc.getOrders(this.currentPage, this.pageSize)
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
          if (status === 403) this.error = 'Accès refusé — rôle ADMIN, SUPERVISEUR ou DÉLÉGUÉ requis.';
          else if (status === 401) this.error = 'Session expirée, veuillez vous reconnecter.';
          else if (status === 0)   this.error = 'Serveur inaccessible — vérifiez que l\'OrderAPI est démarrée.';
          else this.error = err?.error?.Message ?? err?.message ?? 'Erreur lors du chargement des commandes.';
          this.loading = false;
          this.cdr.markForCheck();
        },
      });
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.load();
  }

  onView(id: number): void { this.router.navigate(['/orders', id]); }

  // ── Suppression avec modal ─────────────────────────────
  openDeleteModal(order: CommandeDto): void {
    this.deletingOrder  = order;
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

  getEtatLabel = (s: string) => this.svc.getEtatLabel(s);
  getEtatClass  = (s: string) => this.svc.getEtatClass(s);
  getLignesCount(o: CommandeDto): number { return o.Lignes?.length ?? 0; }
}
