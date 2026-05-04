import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { ProductService } from '../product.service';
import { LotService } from '../../lots/lot.service';
import { MarketingService } from '../../marketing/marketing.service';
import { ToastService } from '../../../shared/services/toast.service';
import { CurrencyTNDPipe } from '../../../shared/pipes/currency-tnd.pipe';
import { LotStatusPipe } from '../../../shared/pipes/lot-status.pipe';

type TabId = 'info' | 'stock' | 'lots' | 'supports' | 'promotions' | 'dashboard';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    CurrencyTNDPipe,
    LotStatusPipe
  ],
  templateUrl: './product-detail.component.html',
  styleUrls: ['./product-detail.component.scss']
})
export class ProductDetailComponent implements OnInit, OnDestroy {

  // ── État ────────────────────────────────────────────
  product: any = null;
  lots: any[]        = [];
  supports: any[]    = [];
  promotions: any[]  = [];
  stock              = 0;
  error              = '';
  loading            = false;
  activeTab: TabId   = 'info';
  productId          = '';

  readonly tabs: { id: TabId; label: string; count?: () => number | string }[] = [
    { id: 'info',       label: 'Informations' },
    { id: 'stock',      label: 'Stock',       count: () => this.stock },
    { id: 'lots',       label: 'Lots',        count: () => this.lots.length },
    { id: 'supports',   label: 'Supports',    count: () => this.supports.length },
    { id: 'promotions', label: 'Promotions',  count: () => this.promotions.length },
    { id: 'dashboard',  label: 'Dashboard' },
  ];

  private readonly destroy$       = new Subject<void>();
  private readonly router         = inject(Router);
  private readonly route          = inject(ActivatedRoute);
  private readonly productService  = inject(ProductService);
  private readonly lotService     = inject(LotService);
  private readonly marketingService = inject(MarketingService);
  private readonly toastService   = inject(ToastService);
  private readonly cdr            = inject(ChangeDetectorRef);

  // ── Lifecycle ────────────────────────────────────────

  ngOnInit(): void {
    this.route.params
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        this.productId = params['id'];
        this.loadProduct();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── Chargement ───────────────────────────────────────

  loadProduct(): void {
    console.log('[ProductDetail] Starting loadProduct for ID:', this.productId);
    this.loading = true;
    this.error   = '';
    this.product = null;
    this.cdr.markForCheck();

    this.productService.getProductById(this.productId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data: any) => {
          console.log('[ProductDetail] API Response received:', data);
          let rawProduct = data?.Result ?? data;
          
          // Normaliser les noms de propriétés (camelCase → PascalCase)
          const normalized = {
            Id_Produit: rawProduct?.id_Produit ?? rawProduct?.Id_Produit,
            Nom: rawProduct?.nom ?? rawProduct?.Nom,
            Description: rawProduct?.description ?? rawProduct?.Description,
            Prix_Vente: rawProduct?.prix_Vente ?? rawProduct?.Prix_Vente ?? 0,
            Prix_Creation: rawProduct?.prix_Creation ?? rawProduct?.Prix_Creation ?? 0,
            TVA: rawProduct?.tva ?? rawProduct?.TVA ?? 0,
            IsActive: rawProduct?.isActive ?? rawProduct?.IsActive ?? true,
            IsArchived: rawProduct?.isArchived ?? rawProduct?.IsArchived ?? false,
            // Garder aussi les champs additionnels s'il y en a
            ...rawProduct
          };
          
          this.product = normalized;
          console.log('[ProductDetail] Product normalized to:', this.product);
          console.log('[ProductDetail] Setting loading to false');
          this.loading = false;
          console.log('[ProductDetail] Loading is now:', this.loading);
          this.cdr.markForCheck();
          this.loadLots();
        },
        error: (err: any) => {
          console.error('[ProductDetail] API Error:', err);
          this.error = 'Impossible de charger le produit.';
          this.toastService.showError('Erreur chargement produit');
          this.loading = false;
          this.cdr.markForCheck();
        }
      });
  }

  private loadLots(): void {
    this.lotService.getLotsByProductId(Number(this.productId))
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          console.log('[ProductDetail] Lots API response:', response);
          const raw = response?.Result ?? response;
          let lotsData = Array.isArray(raw) ? raw : [];
          
          // If the data is wrapped in an array, unwrap it
          if (lotsData.length === 1 && Array.isArray(lotsData[0])) {
            lotsData = lotsData[0];
          }
          
          this.lots = lotsData;
          this.stock = this.lots.reduce(
            (sum, lot) => sum + (lot.Quantite ?? lot.quantite ?? 0), 0
          );
          console.log('[ProductDetail] Loaded', this.lots.length, 'lots, stock:', this.stock);
          this.cdr.markForCheck();
          this.loadSupports();
        },
        error: (err: any) => {
          // Lots are optional — no toast, just empty array
          console.error('[ProductDetail] Error loading lots:', err.status, err.message);
          this.lots  = [];
          this.stock = 0;
          this.cdr.markForCheck();
          this.loadSupports();
        }
      });
  }

  private loadSupports(): void {
    this.marketingService.getSupportsByProductId(Number(this.productId))
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          console.log('[ProductDetail] Supports API response:', response);
          const raw = response?.Result ?? response;
          let supportsData = Array.isArray(raw) ? raw : [];
          
          if (supportsData.length === 1 && Array.isArray(supportsData[0])) {
            supportsData = supportsData[0];
          }
          
          this.supports = supportsData;
          console.log('[ProductDetail] Loaded', this.supports.length, 'marketing supports');
          this.cdr.markForCheck();
        },
        error: (err: any) => {
          // Supports are optional
          console.error('[ProductDetail] Error loading supports:', err.status, err.message);
          this.supports = [];
          this.cdr.markForCheck();
        }
      });
  }

  // ── Navigation & actions ──────────────────────────────

  setActiveTab(tab: TabId): void { this.activeTab = tab; }

  onEdit(): void {
    this.router.navigate(['/products', this.productId, 'edit']);
  }

  onArchive(): void {
    if (!this.product) return;
    this.productService.archiveProduct(this.productId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toastService.showSuccess('Produit archivé avec succès.');
          this.router.navigate(['/products']);
        },
        error: () => this.toastService.showError('Erreur lors de l\'archivage.')
      });
  }

  onDeactivate(): void {
    if (!this.product) return;
    this.productService.deleteProduct(String(this.productId))
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toastService.showSuccess('Produit désactivé avec succès.');
          this.cdr.markForCheck();
          this.loadProduct();
        },
        error: () => this.toastService.showError('Erreur lors de la désactivation.')
      });
  }

  onActivate(): void {
    if (!this.product) return;
    this.productService.activateProduct(String(this.productId))
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toastService.showSuccess('Produit activé avec succès.');
          this.cdr.markForCheck();
          this.loadProduct();
        },
        error: () => this.toastService.showError('Erreur lors de l\'activation.')
      });
  }

  goToLots(): void {
    this.router.navigate(['/lots'], {
      queryParams: { productId: this.productId, productName: this.product?.Nom }
    });
  }

  goToMarketing(): void {
    this.router.navigate(['/marketing/supports'], {
      queryParams: { productId: this.productId, productName: this.product?.Nom }
    });
  }

  // ── Helpers template ─────────────────────────────────

  getStatusText(product: any): string {
    if (!product) return '';
    if (product.IsArchived) return 'Archivé';
    return product.IsActive ? 'Actif' : 'Inactif';
  }

  getStatusClass(product: any): string {
    if (!product) return '';
    if (product.IsArchived) return 'badge-archived';
    return product.IsActive ? 'badge-active' : 'badge-inactive';
  }

  isProductArchived(): boolean {
    return this.product?.IsArchived ?? false;
  }

  isProductActive(): boolean {
    return this.product?.IsActive ?? false;
  }

  canEditProduct(): boolean {
    return !this.isProductArchived();
  }

  getActionButtonText(): string {
    if (!this.product) return '';
    if (this.isProductArchived()) return 'Produit archivé — non modifiable';
    if (this.isProductActive()) return 'Modifier';
    return 'Modifier';
  }

  getActionButtonClass(): string {
    if (!this.product) return 'btn-outline';
    if (this.isProductArchived()) return 'btn-outline disabled';
    return 'btn-outline';
  }

  getTertiaryActions(): Array<{ label: string; icon: string; action: () => void; show: boolean }> {
    return [
      {
        label: this.isProductActive() ? 'Désactiver' : 'Activer',
        icon: this.isProductActive() ? 'eye-off' : 'eye',
        action: () => this.isProductActive() ? this.onDeactivate() : this.onActivate(),
        show: !this.isProductArchived()
      },
      {
        label: 'Archiver',
        icon: 'archive',
        action: () => this.onArchive(),
        show: !this.isProductArchived()
      }
    ];
  }

  getLotStatusDays(lot: any): number {
    if (!lot) return -1;
    const dateExp = lot.DateExpiration ?? lot.dateExpiration;
    if (!dateExp) return -1;
    const today = new Date();
    const expDate = new Date(dateExp);
    const diffTime = expDate.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays;
  }

  getLotExpirationWarning(lot: any): string {
    const days = this.getLotStatusDays(lot);
    if (days < 0) return 'Expiré';
    if (days === 0) return 'Expire aujourd\'hui';
    if (days <= 7) return `Expire dans ${days} jour(s)`;
    return '';
  }
}