import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { PromotionService, PromotionDto } from '../services/promotion.service';
import { ProductService } from '../../products/product.service';
import { ToastService } from '../../../shared/services/toast.service';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-promotion-list',
  standalone: true,
  imports: [CommonModule, RouterLink, ConfirmDialogComponent, EmptyStateComponent],
  templateUrl: './promotion-list.component.html',
  styleUrls: ['./promotion-list.component.css']
})
export class PromotionListComponent implements OnInit, OnDestroy {

  promotions: PromotionDto[] = [];
  loading = false;
  error   = '';

  showConfirm  = false;
  deletingId: number | null = null;
  deletingCode = '';

  // Product name mapping
  productNameMap: Map<number, string> = new Map();

  private destroy$ = new Subject<void>();

  constructor(
    private svc:   PromotionService,
    private productService: ProductService,
    private toast: ToastService,
    private cdr:   ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadProducts();
    this.load();
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  private loadProducts(): void {
    this.productService.getProductsAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (products: any[]) => {
          this.productNameMap.clear();
          products.forEach(p => {
            const name = p.nom || p.Nom || p.name || p.libelle || `Produit ${p.id_Produit ?? p.id}`;
            const productId = p.id_Produit ?? p.Id_Produit ?? p.idProduit ?? p.id;
            if (productId) {
              this.productNameMap.set(productId, name);
            }
          });
          console.log('Produits chargés :', Array.from(this.productNameMap.entries())); // Debug log
          this.cdr.markForCheck();
        },
        error: (err) => {
          console.error('Erreur lors du chargement des produits :', err);
        },
      });
  }

  private load(): void {
    this.loading = true;
    this.svc.getAll().pipe(takeUntil(this.destroy$)).subscribe({
      next: data => { this.promotions = data.filter(p => !this.isExpired(p)); this.loading = false; this.cdr.markForCheck(); },
      error: () => { this.error = 'Erreur lors du chargement des promotions.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  openDelete(promo: PromotionDto): void {
    this.deletingId   = promo.id_Promo ?? null;
    this.deletingCode = promo.codePromo;
    this.showConfirm  = true;
  }

  confirmDelete(): void {
    if (!this.deletingId) return;
    this.showConfirm = false;
    this.svc.delete(this.deletingId).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => { this.toast.showSuccess('Promotion supprimée.'); this.load(); },
      error: () => this.toast.showError('Erreur lors de la suppression.')
    });
  }

  isExpired(promo: PromotionDto): boolean {
    return new Date(promo.dateExpiration) < new Date();
  }

  getProductName(productId: number | undefined): string {
    if (!productId) return 'Unknown Product';
    return this.productNameMap.get(productId) || 'Unknown Product';
  }

  getTypeLabel(typePromotion: string | undefined): string {
    if ((typePromotion ?? 'Pourcentage') === 'Gratuite') {
      return 'Free';
    }
    return 'Discount';
  }
}
