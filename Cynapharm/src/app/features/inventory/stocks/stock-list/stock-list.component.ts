import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, forkJoin, of } from 'rxjs';
import { takeUntil, catchError } from 'rxjs/operators';

import { StockService, StockDelegueDto } from '../services/stock.service';
import { ProductService } from '../../../products/product.service';
import { UserService } from '../../../users/user.service';
import { StockType } from '../../../../core/models/enums';
import { PaginatorComponent } from '../../../../shared/components/paginator/paginator.component';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { ToastService } from '../../../../shared/services/toast.service';

@Component({
  selector: 'app-stock-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, PaginatorComponent, EmptyStateComponent],
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

  productMap:  Map<number, string> = new Map();
  delegateMap: Map<number, string> = new Map();

  // Delete confirmation
  showDeleteModal = false;
  deletingStock:  StockDelegueDto | null = null;
  deleting        = false;

  private destroy$ = new Subject<void>();

  constructor(
    private svc:        StockService,
    private productSvc: ProductService,
    private userSvc:    UserService,
    private toast:      ToastService,
    private cdr:        ChangeDetectorRef
  ) {}

  ngOnInit(): void  { this.loadAll(); }
  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  private unwrapArray(r: any): any[] {
    const raw = r?.result ?? r?.Result ?? r;
    return Array.isArray(raw) ? raw : [];
  }

  private loadAll(): void {
    this.loading = true;
    forkJoin({
      stocks:   this.svc.getAll(this.page, this.pageSize).pipe(catchError(() => of([]))),
      products: this.productSvc.getProducts().pipe(catchError(() => of([]))),
      users:    this.userSvc.getUsers().pipe(catchError(() => of([]))),
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: ({ stocks, products, users }) => {

        // Products: CamelCase API → id_Produit
        this.productMap.clear();
        this.unwrapArray(products).forEach((p: any) => {
          const id  = p['id_Produit'] ?? p.Id_Produit ?? p.idProduit ?? 0;
          const nom = p.nom ?? p.Nom ?? p.name ?? `Product ${id}`;
          if (id) this.productMap.set(Number(id), nom);
        });

        // Users (delegates): already normalized by UserService
        this.delegateMap.clear();
        this.unwrapArray(users).forEach((u: any) => {
          const id   = u.id ?? u.Id ?? u.userId ?? 0;
          const name = u.name ?? u.Name ?? u.nom ?? `User ${id}`;
          if (id) this.delegateMap.set(Number(id), name);
        });

        this.stocks  = stocks as StockDelegueDto[];
        this.total   = this.stocks.length;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.error   = 'Error loading data.';
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  load():            void { this.loadAll(); }
  onPage(p: number): void { this.page = p; this.load(); }

  productName(id: number):  string { return this.productMap.get(id)  ?? `Product #${id}`; }
  delegateName(id: number): string { return this.delegateMap.get(id) ?? `Delegate #${id}`; }

  // ── Delete ──────────────────────────────────────────
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
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toast.showSuccess('Stock entry deleted.');
          this.showDeleteModal = false;
          this.deletingStock   = null;
          this.deleting        = false;
          this.load();
        },
        error: () => {
          this.toast.showError('Error deleting stock entry.');
          this.deleting = false;
          this.cdr.markForCheck();
        }
      });
  }

  stockStatus(s: StockDelegueDto): string {
    if (s.qteDisponible === 0) return 'status-out';
    if (s.qteDisponible <= 5)  return 'status-low';
    return 'status-ok';
  }

  stockLabel(s: StockDelegueDto): string {
    if (s.qteDisponible === 0) return 'Out of stock';
    if (s.qteDisponible <= 5)  return 'Low';
    return 'In stock';
  }
}
