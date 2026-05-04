import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Subject, Observable } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { CurrencyTNDPipe } from '../../../shared/pipes/currency-tnd.pipe';
import { ProductService } from '../product.service';
import { ToastService } from '../../../shared/services/toast.service';

type StatusFilter = 'all' | 'active' | 'inactive' | 'archived';
type ConfirmAction = 'deactivate' | 'archive' | 'activate' | 'unarchive';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, FormsModule, CurrencyTNDPipe, RouterLink],
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.scss']
})
export class ProductListComponent implements OnInit, OnDestroy {

  // ── État principal ───────────────────────────────────
  products: any[] = [];
  filteredProducts: any[] = [];
  loading = false;
  error = '';
  successMessage = '';

  // ── Filtres & pagination ─────────────────────────────
  searchTerm = '';
  statusFilter: StatusFilter = 'all';
  currentPage = 1;
  pageSize = 10;
  totalPages = 0;

  // ── Modal de confirmation ────────────────────────────
  showConfirmModal = false;
  confirmAction: ConfirmAction | '' = '';
  confirmProductId: number | null = null;
  confirmProductName = '';

  // ── Colonnes (utilisées dans le template @for) ───────
  readonly columns = [
    { key: 'Id_Produit',    label: 'ID' },
    { key: 'Nom',           label: 'Nom' },
    { key: 'Description',   label: 'Description' },
    { key: 'Prix_Vente',    label: 'Prix de vente' },
    { key: 'TVA',           label: 'TVA (%)' },
    { key: 'IsActive',      label: 'Statut' },
  ];

  protected readonly Math = Math;

  private readonly destroy$ = new Subject<void>();
  private readonly router = inject(Router);
  private readonly productService = inject(ProductService);
  private readonly toastService = inject(ToastService);
  private readonly cdr = inject(ChangeDetectorRef);

  // ── Lifecycle ────────────────────────────────────────

  ngOnInit(): void {
    this.loadProducts();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── Chargement ───────────────────────────────────────

  private loadProducts(): void {
    this.loading = true;
    this.error = '';
    this.successMessage = '';

    this.productService.getProducts()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          const raw: any[] = Array.isArray(response) ? response : response ? [response] : [];
          this.products = raw.map(p => this.normalizeProduct(p));
          this.applyFilters();
          this.loading = false;
          this.cdr.markForCheck();
        },
        error: (err: any) => {
          console.error('[ProductList] Load failed:', err);
          this.error = this.resolveErrorMessage(err);
          this.loading = false;
          this.cdr.markForCheck();
        }
      });
  }

  // ── Filtres ──────────────────────────────────────────

  applyFilters(): void {
    let result = [...this.products];

    if (this.statusFilter !== 'all') {
      result = result.filter(p => {
        switch (this.statusFilter) {
          case 'active':   return p.IsActive && !p.IsArchived;
          case 'inactive': return !p.IsActive && !p.IsArchived;
          case 'archived': return p.IsArchived;
        }
      });
    }

    const term = this.searchTerm.trim().toLowerCase();
    // ✅ La recherche ne commence qu'à partir de 3 caractères
    if (term.length >= 3) {
      result = result.filter(p =>
        p.Nom?.toLowerCase().includes(term) ||
        p.Description?.toLowerCase().includes(term)
      );
    } else if (term.length > 0 && term.length < 3) {
      // Si l'utilisateur a saisi 1 ou 2 caractères, afficher tous les produits
      result = [...this.products];
      if (this.statusFilter !== 'all') {
        result = result.filter(p => {
          switch (this.statusFilter) {
            case 'active':   return p.IsActive && !p.IsArchived;
            case 'inactive': return !p.IsActive && !p.IsArchived;
            case 'archived': return p.IsArchived;
          }
        });
      }
    }

    this.filteredProducts = result;
    this.totalPages = Math.ceil(result.length / this.pageSize);
    if (this.currentPage > this.totalPages) this.currentPage = 1;
  }

  onSearch(): void            { this.currentPage = 1; this.applyFilters(); }
  onStatusFilterChange(): void { this.currentPage = 1; this.applyFilters(); }
  onPageSizeChange(): void    { this.currentPage = 1; this.applyFilters(); }

  // ── Pagination ───────────────────────────────────────

  get paginatedProducts(): any[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredProducts.slice(start, start + this.pageSize);
  }

  get pageNumbers(): number[] {
    const maxVisible = 5;
    let start = Math.max(1, this.currentPage - Math.floor(maxVisible / 2));
    const end = Math.min(this.totalPages, start + maxVisible - 1);
    start = Math.max(1, end - maxVisible + 1);
    return Array.from({ length: end - start + 1 }, (_, i) => start + i);
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) this.currentPage = page;
  }

  // ── Navigation ───────────────────────────────────────

  onView(id: number): void { this.router.navigate(['/products', id]); }
  onEdit(id: number): void { this.router.navigate(['/products', id, 'edit']); }

  // ── Actions avec confirmation ─────────────────────────

  onDelete(id: number):    void { this.openConfirm(id, 'deactivate'); }
  onArchive(id: number):   void { this.openConfirm(id, 'archive'); }
  onActivate(id: number):  void { this.openConfirm(id, 'activate'); }

  onUnarchive(id: number): void { this.openConfirm(id, 'unarchive'); }
  private openConfirm(id: number, action: ConfirmAction): void {
    const product = this.products.find(p => p.Id_Produit === id);
    this.confirmProductId   = id;
    this.confirmProductName = product?.Nom || `Produit ${id}`;
    this.confirmAction      = action;
    this.showConfirmModal   = true;
  }

  cancelAction(): void {
    this.showConfirmModal   = false;
    this.confirmProductId   = null;
    this.confirmProductName = '';
    this.confirmAction      = '';
  }

  confirmAction_execute(): void {
  if (this.confirmProductId === null || !this.confirmAction) return;

  const actionMap: Record<ConfirmAction, Observable<any>> = {
    deactivate: this.productService.deleteProduct(String(this.confirmProductId)),
    archive:    this.productService.archiveProduct(String(this.confirmProductId)),
    unarchive:  this.productService.unarchiveProduct(String(this.confirmProductId)),
    activate:   this.productService.activateProduct(String(this.confirmProductId)),
  };

  this.loading = true;

  actionMap[this.confirmAction]
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: () => {
        // ✅ 1. Fermer la modale COMME Annuler
        this.cancelAction();

        // ✅ 2. Rafraîchir la liste
        this.loadProducts();

        this.successMessage = 'Action effectuée avec succès.';
        this.loading = false;
      },
      error: (err: any) => {
        console.error('[ProductList] Action failed:', err);
        this.error = this.resolveErrorMessage(err);
        this.cancelAction();  // Fermer la modal même sur erreur
        this.loading = false;
      }
    });
}

  // ── Helpers template ─────────────────────────────────

  getStatusText(product: any): string {
    if (product.IsArchived) return 'Archivé';
    return product.IsActive ? 'Actif' : 'Inactif';
  }

  getActionText(): string {
    const map: Partial<Record<ConfirmAction, string>> = {
      deactivate: 'désactiver',
      archive:    'archiver',
      unarchive:  'désarchiver',
      activate:   'activer',
    };
    return map[this.confirmAction as ConfirmAction] ?? '';
  }

  getConfirmMessage(): string {
    const name = this.confirmProductName;
    const map: Partial<Record<ConfirmAction, string>> = {
      deactivate: `Désactiver le produit "${name}" ?`,
      unarchive:  `Désarchiver le produit "${name}" ?`,
      archive:    `Archiver le produit "${name}" ?`,
      activate:   `Activer le produit "${name}" ?`,
    };
    return map[this.confirmAction as ConfirmAction] ?? '';
  }

  // ── Utilitaires privés ───────────────────────────────

  private normalizeProduct(p: any): any {
    return {
      Id_Produit:    p.Id_Produit    ?? p.id_Produit    ?? null,
      Nom:           p.Nom           ?? p.nom           ?? '',
      Description:   p.Description   ?? p.description   ?? '',
      Prix_Vente:    p.Prix_Vente    ?? p.prix_Vente    ?? 0,
      Prix_Creation: p.Prix_Creation ?? p.prix_Creation ?? 0,
      TVA:           p.TVA           ?? p.tVA           ?? p.tva ?? 0,
      IsActive:      p.IsActive      ?? p.isActive      ?? false,
      IsArchived:    p.IsArchived    ?? p.isArchived    ?? false,
    };
  }

  private resolveErrorMessage(err: any): string {
    const statusMessages: Record<number, string> = {
      0:   'Serveur inaccessible — vérifiez la connexion.',
      401: 'Session expirée — veuillez vous reconnecter.',
      403: 'Accès refusé — droits insuffisants.',
      404: 'Ressource introuvable.',
      500: 'Erreur interne du serveur.',
    };
    return statusMessages[err.status] ?? err.message ?? 'Une erreur inattendue s\'est produite.';
  }
  
}
