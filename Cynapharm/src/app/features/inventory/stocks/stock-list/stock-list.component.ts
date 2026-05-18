import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { StockService, StockDelegueDto } from '../services/stock.service';
import { UserService } from '../../../users/user.service';
import { ProductService } from '../../../products/product.service';
import { LotService } from '../../../lots/lot.service';
import { ToastService } from '../../../../shared/services/toast.service';
import { StockType } from '../../../../core/models/enums';
import { PaginatorComponent } from '../../../../shared/components/paginator/paginator.component';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-stock-list',
  standalone: true,
  imports: [CommonModule, RouterLink, PaginatorComponent, EmptyStateComponent],
  templateUrl: './stock-list.component.html',
  styleUrls: ['./stock-list.component.css']
})
export class StockListComponent implements OnInit, OnDestroy {
  stocks:   StockDelegueDto[] = [];
  loading   = false;
  error     = '';
  page      = 1;
  pageSize  = 20;
  total     = 0;

  // resolved display names / dates
  delegueNames: Record<number, string> = {};
  productNames: Record<number, string> = {};
  lotDates:     Record<string, string> = {};

  // delete modal
  showDeleteModal  = false;
  deletingStock:   StockDelegueDto | null = null;
  deleting         = false;

  private destroy$ = new Subject<void>();

  constructor(
    private svc:        StockService,
    private toast:      ToastService,
    private userSvc:    UserService,
    private productSvc: ProductService,
    private lotSvc:     LotService,
    private cdr:        ChangeDetectorRef
  ) {}

  ngOnInit():    void { this.load(); }
  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  load(): void {
    this.loading = true;
    this.svc.getAll(this.page, this.pageSize).pipe(takeUntil(this.destroy$)).subscribe({
      next: data => {
        this.stocks  = data;
        this.total   = data.length;
        this.loading = false;
        this.loadRelatedData(data);
        this.cdr.markForCheck();
      },
      error: () => { this.error = 'Erreur lors du chargement.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  onPage(p: number): void { this.page = p; this.load(); }

  // ── Delete ────────────────────────────────────────────

  openDelete(s: StockDelegueDto): void {
    this.deletingStock   = s;
    this.showDeleteModal = true;
  }

  cancelDelete(): void {
    this.showDeleteModal = false;
    this.deletingStock   = null;
  }

  confirmDelete(): void {
    if (!this.deletingStock?.id_stock) return;
    this.deleting = true;
    this.svc.delete(this.deletingStock.id_stock, StockType.Delegue)
      .pipe(takeUntil(this.destroy$)).subscribe({
        next: () => {
          this.toast.showSuccess('Stock supprimé.');
          this.showDeleteModal = false;
          this.deletingStock   = null;
          this.deleting        = false;
          this.load();
        },
        error: () => {
          this.toast.showError('Erreur lors de la suppression.');
          this.deleting = false;
          this.cdr.markForCheck();
        }
      });
  }

  // ── Related data ──────────────────────────────────────

  private loadRelatedData(stocks: StockDelegueDto[]): void {
    const delegueIds = [...new Set(stocks.map(s => s.id_User_Delegue).filter(id => id > 0))];
    delegueIds.forEach(id => {
      if (this.delegueNames[id]) return;
      this.userSvc.getUserById(id).pipe(takeUntil(this.destroy$)).subscribe({
        next: (res: any) => {
          const u = res?.Result ?? res?.result ?? res;
          this.delegueNames[id] =
            u?.fullName ?? u?.FullName ?? u?.name ?? u?.Name ?? u?.email ?? `#${id}`;
          this.cdr.markForCheck();
        },
        error: () => { this.delegueNames[id] = `#${id}`; }
      });
    });

    const productIds = [...new Set(stocks.map(s => s.id_Produit).filter(id => id > 0))];
    productIds.forEach(id => {
      if (this.productNames[id]) return;
      this.productSvc.getProductById(id).pipe(takeUntil(this.destroy$)).subscribe({
        next: (data: any) => {
          const raw = data?.Result ?? data?.result ?? data;
          this.productNames[id] = raw?.Nom ?? raw?.nom ?? `#${id}`;
          this.cdr.markForCheck();
        },
        error: () => { this.productNames[id] = `#${id}`; }
      });
    });

    const lots = [...new Set(stocks.map(s => s.numeroLot).filter(n => !!n))];
    lots.forEach(num => {
      if (this.lotDates[num]) return;
      this.lotSvc.getLotByNumero(num).pipe(takeUntil(this.destroy$)).subscribe({
        next: lot => { this.lotDates[num] = lot.dateExpiration ?? ''; this.cdr.markForCheck(); },
        error: () => {}
      });
    });
  }

  getDelegrueName(id: number): string { return this.delegueNames[id] ?? `#${id}`; }
  getProductName(id: number):  string { return this.productNames[id] ?? `#${id}`; }
  getLotDate(num: string, fallback: string): string { return this.lotDates[num] ?? fallback; }
}
