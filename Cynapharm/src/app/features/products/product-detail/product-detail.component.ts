// ── product-detail.component.ts ──────────────────────────────────────────────
import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { ProductService }         from '../product.service';
import { LotService }             from '../../lots/lot.service';
import { MarketingService, SupportMarketingDto, FichierDto } from '../../marketing/marketing.service';
import { ProductAdvancedService } from '../services/product-advanced.service';
import { InventoryService, StockDelegue } from '../../inventory/inventory.service';
import { AuthService, UserRole }  from '../../../core/services/auth.service';
import { ToastService }           from '../../../shared/services/toast.service';
import { CurrencyTNDPipe }        from '../../../shared/pipes/currency-tnd.pipe';
import { LotStatusPipe }          from '../../../shared/pipes/lot-status.pipe';

type TabId = 'info' | 'stock' | 'lots' | 'supports' | 'promotions' | 'dashboard';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterLink,
    CurrencyTNDPipe,
    LotStatusPipe,
  ],
  templateUrl: './product-detail.component.html',
  styleUrls: ['./product-detail.component.scss'],
})
export class ProductDetailComponent implements OnInit, OnDestroy {

  // ── État produit ──────────────────────────────────────────────────────────
  product: any                    = null;
  lots: any[]                     = [];
  supports: any[]                 = [];
  promotions: any[]               = [];
  inventoryStocks: StockDelegue[] = [];
  stock                           = 0;
  stockDisponible                 = 0;
  loadingInventory                = false;
  error                           = '';
  loading                         = false;
  activeTab: TabId                = 'info';
  productId                       = '';

  // ── Supports — gestion inline ─────────────────────────────────────────────
  showSupportModal                    = false;
  supportSaving                       = false;
  supportFormError                    = '';
  editingSupportId: number | null     = null;
  supportForm!: FormGroup;
  expandedSupports                    = new Set<number>();
  loadingFilesFor                     = new Set<number>();
  supportFiles: Record<number, any[]> = {};

  // ── Fichiers — gestion inline ─────────────────────────────────────────────
  showFileModal          = false;
  fileSaving             = false;
  fileFormError          = '';
  fileTargetSupport: any = null;
  fileForm!: FormGroup;

  // ── Permissions ───────────────────────────────────────────────────────────
  canManageMarketing = false;

  readonly supportTypes = [
    'Brochure', 'Flyer', 'Catalogue', 'Vidéo',
    'Présentation', 'Affiche', 'Document technique', 'Autre',
  ];

  readonly tabs: { id: TabId; label: string; count?: () => number | string }[] = [
    { id: 'info',       label: 'Informations' },
    { id: 'stock',      label: 'Stock',      count: () => this.stockDisponible > 0 ? this.stockDisponible : this.stock },
    { id: 'lots',       label: 'Lots',       count: () => this.lots.length },
    { id: 'supports',   label: 'Supports',   count: () => this.supports.length },
    { id: 'promotions', label: 'Promotions', count: () => this.promotions.length },
    { id: 'dashboard',  label: 'Dashboard' },
  ];

  private readonly destroy$         = new Subject<void>();
  private readonly router           = inject(Router);
  private readonly route            = inject(ActivatedRoute);
  private readonly fb               = inject(FormBuilder);
  private readonly productService   = inject(ProductService);
  private readonly lotService       = inject(LotService);
  private readonly marketingService = inject(MarketingService);
  private readonly productAdvanced  = inject(ProductAdvancedService);
  private readonly inventoryService = inject(InventoryService);
  private readonly authService      = inject(AuthService);
  private readonly toastService     = inject(ToastService);
  private readonly cdr              = inject(ChangeDetectorRef);

  // ── Cycle de vie ──────────────────────────────────────────────────────────

  ngOnInit(): void {
    const role = this.authService.getUserRole();
    this.canManageMarketing = role === UserRole.ADMIN || role === UserRole.SUPERVISEUR;

    this.initSupportForm();
    this.initFileForm();

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

  // ── Formulaires ───────────────────────────────────────────────────────────

  private initSupportForm(): void {
    this.supportForm = this.fb.group({
      type:         ['', Validators.required],
      campaignName: ['', Validators.required],
      isActive:     [true],
    });
  }

  private initFileForm(): void {
    this.fileForm = this.fb.group({
      nomFichier: ['', Validators.required],
      url:        ['', Validators.required],
      extension:  [''],
      taille:     [0, Validators.min(0)],
    });
  }

  // ── Chargement produit ────────────────────────────────────────────────────

  loadProduct(): void {
    this.loading = true;
    this.error   = '';
    this.product = null;
    this.cdr.markForCheck();

    this.productService.getProductById(this.productId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data: any) => {
          const raw = data?.Result ?? data;
          this.product = {
            Id_Produit:    raw?.id_Produit    ?? raw?.Id_Produit,
            Nom:           raw?.nom           ?? raw?.Nom,
            Description:   raw?.description   ?? raw?.Description,
            Prix_Vente:    raw?.prix_Vente    ?? raw?.Prix_Vente    ?? 0,
            Prix_Creation: raw?.prix_Creation ?? raw?.Prix_Creation ?? 0,
            TVA:           raw?.tva           ?? raw?.TVA           ?? 0,
            IsActive:      raw?.isActive      ?? raw?.IsActive      ?? true,
            IsArchived:    raw?.isArchived    ?? raw?.IsArchived    ?? false,
            ...raw,
          };
          this.loading = false;
          this.cdr.markForCheck();
          this.loadLots();
        },
        error: () => {
          this.error   = 'Impossible de charger le produit.';
          this.loading = false;
          this.toastService.showError('Erreur chargement produit');
          this.cdr.markForCheck();
        },
      });
  }

  private loadLots(): void {
    this.lotService.getLotsByProductId(Number(this.productId))
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (lots: any[]) => {
          this.lots  = lots;
          this.stock = lots.reduce((s, l) => s + (l.quantite ?? l.Quantite ?? 0), 0);
          this.cdr.markForCheck();
          this.loadSupports();
          this.loadInventoryStocks();
        },
        error: () => {
          this.lots  = [];
          this.stock = 0;
          this.cdr.markForCheck();
          this.loadSupports();
          this.loadInventoryStocks();
        },
      });
  }

  loadSupports(): void {
    this.marketingService.getSupportsByProductId(Number(this.productId))
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          // Gère : { Result: [...] } | { result: [...] } | [...] direct
          const raw = response?.Result ?? response?.result ?? response;
          let data: any[] = Array.isArray(raw) ? raw : [];
          // Cas double-wrap [[...]]
          if (data.length === 1 && Array.isArray(data[0])) data = data[0];

          this.supports = data.map(s => this.normalizeSupport(s));
          this.cdr.markForCheck();
          this.loadPromotions();
        },
        error: (err: any) => {
          // 404 est intercepté dans le service → retourne [] ; ici ce sont de vraies erreurs
          console.error('[ProductDetail] loadSupports error:', err);
          this.supports = [];
          this.cdr.markForCheck();
          this.loadPromotions();
        },
      });
  }

  private loadInventoryStocks(): void {
    this.loadingInventory = true;
    this.inventoryService.getStockByProduit(Number(this.productId))
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (stocks: StockDelegue[]) => {
          this.inventoryStocks  = stocks;
          this.stockDisponible  = stocks.reduce((s, st) => s + (st.qteDisponible ?? 0), 0);
          this.loadingInventory = false;
          this.cdr.markForCheck();
        },
        error: () => {
          this.inventoryStocks  = [];
          this.stockDisponible  = 0;
          this.loadingInventory = false;
          this.cdr.markForCheck();
        },
      });
  }

  private loadPromotions(): void {
    this.productAdvanced.getPromotionsByProduct(Number(this.productId))
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          const raw = response?.Result ?? response?.result ?? response;
          this.promotions = Array.isArray(raw) ? raw : [];
          this.cdr.markForCheck();
        },
        error: () => {
          this.promotions = [];
          this.cdr.markForCheck();
        },
      });
  }

  // ── SUPPORTS — Modal créer / modifier ────────────────────────────────────

  openCreateSupport(): void {
    this.editingSupportId = null;
    this.supportFormError = '';
    this.supportForm.reset({ type: '', campaignName: '', isActive: true });
    this.showSupportModal = true;
  }

  openEditSupport(support: any): void {
    this.editingSupportId = support.idSupportMarketting;
    this.supportFormError = '';
    this.supportForm.setValue({
      type:         support.type         ?? '',
      campaignName: support.campaignName ?? '',
      isActive:     support.isActive     ?? true,
    });
    this.showSupportModal = true;
  }

  closeSupportModal(): void {
    this.showSupportModal = false;
    this.editingSupportId = null;
    this.supportFormError = '';
  }

  submitSupportForm(): void {
    if (this.supportForm.invalid) {
      this.supportForm.markAllAsTouched();
      return;
    }
    this.supportSaving    = true;
    this.supportFormError = '';

    const v = this.supportForm.value;

    const payload: SupportMarketingDto = {
      // editingSupportId !== null → mode édition : inclure l'ID pour que le backend fasse un UPDATE
      ...(this.editingSupportId !== null ? { Id_SupportMarketting: this.editingSupportId } : {}),
      Type:         v.type,
      CampaignName: v.campaignName,
      IsActive:     v.isActive ?? true,
      Id_Produit:   Number(this.productId),
    };

    this.marketingService.createOrUpdateSupport(payload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.supportSaving = false;
          this.closeSupportModal();
          this.toastService.showSuccess(
            this.editingSupportId ? 'Support mis à jour.' : 'Support créé avec succès.'
          );
          this.loadSupports();
        },
        error: (err: any) => {
          this.supportFormError = err?.error?.message ?? 'Erreur lors de l\'enregistrement.';
          this.supportSaving    = false;
          this.cdr.markForCheck();
        },
      });
  }

  // ── SUPPORTS — Toggle statut ──────────────────────────────────────────────

  toggleSupportStatus(support: any): void {
    const id   = support.idSupportMarketting;
    const req$ = support.isActive
      ? this.marketingService.disableSupport(id)
      : this.marketingService.activateSupport(id);

    req$.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.showSuccess(
          support.isActive ? 'Support désactivé.' : 'Support activé.'
        );
        this.loadSupports();
      },
      error: () => this.toastService.showError('Erreur lors du changement de statut.'),
    });
  }

  // ── SUPPORTS — Expand / collapse fichiers ────────────────────────────────

  toggleExpand(support: any): void {
    const id = support.idSupportMarketting;
    if (this.expandedSupports.has(id)) {
      this.expandedSupports.delete(id);
    } else {
      this.expandedSupports.add(id);
      if (!this.supportFiles[id]) this.loadFilesForSupport(support);
    }
    this.cdr.markForCheck();
  }

  isExpanded(support: any): boolean {
    return this.expandedSupports.has(support.idSupportMarketting);
  }

  private loadFilesForSupport(support: any): void {
    const id = support.idSupportMarketting;
    this.loadingFilesFor.add(id);
    this.cdr.markForCheck();

    this.marketingService.getFilesBySupport(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          const raw = response?.Result ?? response?.result ?? response;
          this.supportFiles[id] = Array.isArray(raw)
            ? raw.map((f: any) => this.normalizeFichier(f))
            : [];
          this.loadingFilesFor.delete(id);
          this.cdr.markForCheck();
        },
        error: () => {
          this.supportFiles[id] = [];
          this.loadingFilesFor.delete(id);
          this.cdr.markForCheck();
        },
      });
  }

  getFilesFor(support: any): any[] {
    const id = support.idSupportMarketting;
    if (this.supportFiles[id]) return this.supportFiles[id];
    const embedded = support.fichiers;
    return Array.isArray(embedded) ? embedded.map((f: any) => this.normalizeFichier(f)) : [];
  }

  // ── FICHIERS — Modal ajouter ──────────────────────────────────────────────

  openAddFile(support: any): void {
    this.fileTargetSupport = support;
    this.fileFormError     = '';
    this.fileForm.reset({ nomFichier: '', url: '', extension: '', taille: 0 });
    this.showFileModal = true;
  }

  closeFileModal(): void {
    this.showFileModal     = false;
    this.fileTargetSupport = null;
    this.fileFormError     = '';
  }

  onFileUrlChange(url: string): void {
    if (!url) return;
    const ext = url.split('.').pop()?.split('?')[0]?.toLowerCase() ?? '';
    if (ext && ext.length <= 5) {
      this.fileForm.patchValue({ extension: ext }, { emitEvent: false });
    }
  }

  submitFileForm(): void {
    if (this.fileForm.invalid) {
      this.fileForm.markAllAsTouched();
      return;
    }
    this.fileSaving    = true;
    this.fileFormError = '';

    const v = this.fileForm.value;

    // PascalCase : correspond exactement au FichierDto C# attendu par AutoMapper
    const payload: FichierDto = {
      NomFichier: v.nomFichier,
      Url:        v.url,
      Extension:  v.extension || (v.url.split('.').pop()?.split('?')[0] ?? ''),
      Taille:     Number(v.taille) || 0,
      Id_Support: this.fileTargetSupport.idSupportMarketting
                    ?? this.fileTargetSupport.Id_SupportMarketting,
    };

    this.marketingService.addFileToSupport(payload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.fileSaving = false;
          this.closeFileModal();
          this.toastService.showSuccess('Fichier ajouté avec succès.');
          const id = payload.Id_Support;
          delete this.supportFiles[id];
          if (this.expandedSupports.has(id)) {
            this.loadFilesForSupport(this.fileTargetSupport ?? { idSupportMarketting: id });
          }
          this.loadSupports();
        },
        error: (err: any) => {
          this.fileFormError = err?.error?.message ?? 'Erreur lors de l\'ajout du fichier.';
          this.fileSaving    = false;
          this.cdr.markForCheck();
        },
      });
  }

  // ── FICHIERS — Supprimer ──────────────────────────────────────────────────

  deleteFile(file: any, support: any): void {
    const fileId    = file.idFichier ?? file.Id_Fichier;
    const supportId = support.idSupportMarketting;

    this.marketingService.deleteFile(fileId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toastService.showSuccess('Fichier supprimé.');
          delete this.supportFiles[supportId];
          if (this.expandedSupports.has(supportId)) this.loadFilesForSupport(support);
          this.loadSupports();
        },
        error: () => this.toastService.showError('Erreur lors de la suppression.'),
      });
  }

  // ── Normalisation (réponse backend PascalCase → camelCase pour le template) ──

  private normalizeSupport(s: any): any {
    return {
      ...s,
      // CamelCase policy: Id_SupportMarketting → id_SupportMarketting (bracket needed for underscore key)
      idSupportMarketting: s['id_SupportMarketting'] ?? s.Id_SupportMarketting ?? s.idSupportMarketting ?? null,
      type:                s.type         ?? s.Type         ?? '',
      idProduit:           s['id_Produit'] ?? s.Id_Produit  ?? s.idProduit    ?? 0,
      isActive:            s.isActive     ?? s.IsActive     ?? true,
      campaignName:        s.campaignName ?? s.CampaignName ?? '',
      fichiers: (s.fichiers ?? s.Fichiers ?? []).map((f: any) => this.normalizeFichier(f)),
    };
  }

  private normalizeFichier(f: any): any {
    return {
      ...f,
      // CamelCase policy: Id_Fichier → id_Fichier, etc.
      idFichier:  f['id_Fichier']  ?? f.Id_Fichier  ?? f.idFichier  ?? null,
      nomFichier: f.nomFichier     ?? f.NomFichier   ?? '',
      url:        f.url            ?? f.Url          ?? '',
      extension:  f.extension      ?? f.Extension    ?? '',
      taille:     f.taille         ?? f.Taille       ?? 0,
      idSupport:  f['id_Support']  ?? f.Id_Support   ?? f.idSupport  ?? 0,
    };
  }

  getSupportFileCount(support: any): number {
    const id = support.idSupportMarketting;
    if (this.supportFiles[id]) return this.supportFiles[id].length;
    return (support.fichiers ?? support.Fichiers ?? []).length;
  }

  formatFileSize(bytes: number): string {
    if (!bytes || bytes === 0) return '—';
    if (bytes < 1024)          return `${bytes} o`;
    if (bytes < 1048576)       return `${(bytes / 1024).toFixed(1)} Ko`;
    return `${(bytes / 1048576).toFixed(1)} Mo`;
  }

  // ── Helpers promotions ────────────────────────────────────────────────────

  isPromoExpired(promo: any): boolean {
    const exp = promo.dateExpiration ?? promo.DateExpiration;
    if (!exp) return false;
    return new Date(exp) < new Date();
  }

  // ── Navigation & actions produit ──────────────────────────────────────────

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
        error: () => this.toastService.showError('Erreur lors de l\'archivage.'),
      });
  }

  onDeactivate(): void {
    if (!this.product) return;
    this.productService.deleteProduct(String(this.productId))
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toastService.showSuccess('Produit désactivé avec succès.');
          this.loadProduct();
        },
        error: () => this.toastService.showError('Erreur lors de la désactivation.'),
      });
  }

  onActivate(): void {
    if (!this.product) return;
    this.productService.activateProduct(String(this.productId))
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toastService.showSuccess('Produit activé avec succès.');
          this.loadProduct();
        },
        error: () => this.toastService.showError('Erreur lors de l\'activation.'),
      });
  }

  goToLots(): void {
    this.router.navigate(['/lots'], {
      queryParams: { productId: this.productId, productName: this.product?.Nom },
    });
  }

  // ── Helpers template ──────────────────────────────────────────────────────

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

  isProductArchived(): boolean { return this.product?.IsArchived ?? false; }
  isProductActive():   boolean { return this.product?.IsActive   ?? false; }
  canEditProduct():    boolean { return !this.isProductArchived(); }

  getLotStatusDays(lot: any): number {
    const dateExp = lot.DateExpiration ?? lot.dateExpiration;
    if (!dateExp) return -1;
    return Math.ceil((new Date(dateExp).getTime() - Date.now()) / 86400000);
  }

  getLotExpirationWarning(lot: any): string {
    const days = this.getLotStatusDays(lot);
    if (days < 0)   return 'Expiré';
    if (days === 0) return 'Expire aujourd\'hui';
    if (days <= 7)  return `Expire dans ${days} jour(s)`;
    return '';
  }

  isSupportFieldInvalid(field: string): boolean {
    const c = this.supportForm.get(field);
    return !!(c?.invalid && c?.touched);
  }

  isFileFieldInvalid(field: string): boolean {
    const c = this.fileForm.get(field);
    return !!(c?.invalid && c?.touched);
  }
}