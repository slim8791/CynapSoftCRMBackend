// ── lot-list.component.ts ────────────────────────────────────────────────────
import {
  Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef
} from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { LotService } from '../lot.service';
import { ProductService } from '../../products/product.service';
import { ToastService } from '../../../shared/services/toast.service';
import {
  LotDto, LotStatus,
  getLotStatus, STATUS_LABEL, STATUS_CSS_CLASS
} from '../lot.model';

interface Column {
  key: keyof LotDto | 'status';
  label: string;
}

type StatusFilter = 'all' | LotStatus;

@Component({
  selector: 'app-lot-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, FormsModule, RouterLink, DatePipe],
  templateUrl: './lot-list.component.html',
  styleUrls: ['./lot-list.component.scss'],
})
export class LotListComponent implements OnInit, OnDestroy {

  // ── État ──────────────────────────────────────────────────────────────────
  lots: LotDto[]         = [];
  filteredLots: LotDto[] = [];
  loading  = true;
  error    = '';

  // ── Contexte produit (optionnel) ──────────────────────────────────────────
  productId:   number | null = null;
  productName  = '';

  // ── Filtres ───────────────────────────────────────────────────────────────
  searchTerm:   string       = '';
  statusFilter: StatusFilter = 'all';

  // ── KPI (calculés une seule fois après chaque chargement) ─────────────────
  totalLots     = 0;
  totalActive   = 0;
  totalLowStock = 0;
  totalExpired  = 0;

  // ── Modal suppression ─────────────────────────────────────────────────────
  showDeleteModal = false;
  lotToDelete: LotDto | null = null;

  // ── Configuration colonnes ────────────────────────────────────────────────
  readonly columns: Column[] = [
    { key: 'numero',         label: 'N° Lot'     },
    { key: 'quantite',       label: 'Quantité'   },
    { key: 'dateExpiration', label: 'Expiration' },
    { key: 'status',         label: 'Statut'     },
  ];

  // ── Map pour stocker les noms des produits ────────────────────────────────
  productNameMap: Map<number, string> = new Map();

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly lotService:   LotService,
    private readonly productService: ProductService,
    private readonly router:       Router,
    private readonly route:        ActivatedRoute,
    private readonly cdr:          ChangeDetectorRef,
    private readonly toastService: ToastService,
  ) {}

  // ── Cycle de vie ──────────────────────────────────────────────────────────

  ngOnInit(): void {
    // Charger les produits en premier
    this.loadProductNames();

    this.route.queryParams
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        this.productId   = params['productId'] ? Number(params['productId']) : null;
        this.productName = params['productName'] ?? '';
        this.loadLots();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── Chargement des noms de produits ───────────────────────────────────────

  private loadProductNames(): void {
    this.productService.getProducts()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (products: any[]) => {
          this.productNameMap.clear();
          products.forEach((p: any) => {
            // ProductAPI uses CamelCase policy: Id_Produit → id_Produit
            const id: number = p['id_Produit'] ?? p.Id_Produit ?? p.idProduit ?? p.id ?? 0;
            const name: string = p.nom ?? p.Nom ?? p.name ?? `Product ${id}`;
            if (id) this.productNameMap.set(id, name);
          });
          this.cdr.markForCheck();
        },
        error: () => { /* silently ignore — product names degrade to ID */ },
      });
  }

  // ── Chargement ────────────────────────────────────────────────────────────

  private loadLots(): void {
    this.loading = true;
    this.error   = '';

    const source$ = this.productId
      ? this.lotService.getLotsByProductId(this.productId)
      : this.lotService.getAllLots();

    source$.pipe(takeUntil(this.destroy$)).subscribe({
      next: (lots: LotDto[]) => {
        this.lots    = lots;
        this.loading = false;
        this.updateKPIs();
        this.applyFilters();
        this.cdr.markForCheck();
      },
      error: (err: any) => {
        this.error   = err?.error?.message ?? 'Erreur lors du chargement des lots.';
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }

  // ── KPI ───────────────────────────────────────────────────────────────────

  private updateKPIs(): void {
    this.totalLots     = this.lots.length;
    this.totalActive   = this.lots.filter(l => getLotStatus(l) === 'active').length;
    this.totalLowStock = this.lots.filter(l => getLotStatus(l) === 'low-stock').length;
    this.totalExpired  = this.lots.filter(l => getLotStatus(l) === 'expired').length;
  }

  // ── Filtres ───────────────────────────────────────────────────────────────

  applyFilters(): void {
    let result = this.lots;

    if (this.statusFilter !== 'all') {
      result = result.filter(l => getLotStatus(l) === this.statusFilter);
    }

    const term = this.searchTerm.trim().toLowerCase();
    if (term.length >= 2) {
      result = result.filter(l => l.numero.toLowerCase().includes(term));
    }

    this.filteredLots = result;
  }

  onSearch(): void          { this.applyFilters(); }
  onStatusFilterChange(): void { this.applyFilters(); }

  onKPIClick(filter: StatusFilter): void {
    this.statusFilter = filter;
    this.applyFilters();
    this.cdr.markForCheck();
  }

  // ── Helpers template ──────────────────────────────────────────────────────

  /** Retourne le nom du produit pour un id donné */
  getProductName(productId: number | undefined): string {
    if (!productId) return 'Unknown Product';
    return this.productNameMap.get(productId) || 'Unknown Product';
  }

  /** Renvoie la valeur affichable d'une cellule (hors colonne 'status') */
  getCellValue(lot: LotDto, key: keyof LotDto | 'status'): string {
    if (key === 'status')         return ''; // géré séparément
    if (key === 'dateExpiration') {
      return lot.dateExpiration
        ? new Date(lot.dateExpiration).toLocaleDateString('fr-FR')
        : 'N/A';
    }
    return String(lot[key as keyof LotDto] ?? '');
  }

  getStatusText(lot: LotDto): string {
    return STATUS_LABEL[getLotStatus(lot)];
  }

  getStatusClass(lot: LotDto): string {
    return STATUS_CSS_CLASS[getLotStatus(lot)];
  }

  // ── Navigation ────────────────────────────────────────────────────────────

  onView(lot: LotDto): void {
    this.router.navigate(['/lots', lot.numero], {
      queryParams: this.productId ? { productId: this.productId } : {},
    });
  }

  onEdit(lot: LotDto): void {
    this.router.navigate(['/lots', lot.numero, 'edit'], {
      queryParams: this.productId ? { productId: this.productId } : {},
    });
  }

  // ── Suppression ───────────────────────────────────────────────────────────

  onDelete(lot: LotDto): void {
    this.lotToDelete    = lot;
    this.showDeleteModal = true;
  }

  cancelDelete(): void {
    this.showDeleteModal = false;
    this.lotToDelete    = null;
  }

  confirmDelete(): void {
    if (!this.lotToDelete) return;
    const { numero } = this.lotToDelete;
    this.showDeleteModal = false;

    this.lotService.deleteLot(numero)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toastService.showSuccess(`Lot ${numero} supprimé avec succès.`);
          this.lotToDelete = null;
          this.loadLots();
        },
        error: (err: any) => {
          this.toastService.showError(
            err?.error?.message ?? 'Erreur lors de la suppression du lot.'
          );
          this.lotToDelete = null;
        },
      });
  }
}