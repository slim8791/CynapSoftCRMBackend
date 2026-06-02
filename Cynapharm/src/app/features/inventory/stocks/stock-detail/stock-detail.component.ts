import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { StockService, StockDelegueDto } from '../services/stock.service';
import { UserService } from '../../../users/user.service';
import { ProductService } from '../../../products/product.service';
import { LotService } from '../../../lots/lot.service';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-stock-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, EmptyStateComponent],
  templateUrl: './stock-detail.component.html',
  styleUrls: ['./stock-detail.component.css']
})
export class StockDetailComponent implements OnInit, OnDestroy {
  stock:   StockDelegueDto | null = null;
  loading  = false;
  error    = '';

  delegeName   = '';
  productName  = '';
  lotDate      = '';

  private destroy$ = new Subject<void>();

  constructor(
    private route:      ActivatedRoute,
    private svc:        StockService,
    private userSvc:    UserService,
    private productSvc: ProductService,
    private lotSvc:     LotService,
    private cdr:        ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) { this.error = 'Identifiant invalide.'; return; }
    this.loading = true;
    this.svc.getById(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: data => {
        this.stock   = data;
        this.loading = false;
        if (data) this.loadRelated(data);
        this.cdr.markForCheck();
      },
      error: () => { this.error = 'Impossible de charger le stock.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  private loadRelated(s: StockDelegueDto): void {
    // Delegue name
    if (s.id_User_Delegue > 0) {
      this.userSvc.getUserById(s.id_User_Delegue).pipe(takeUntil(this.destroy$)).subscribe({
        next: (res: any) => {
          const u = res?.Result ?? res?.result ?? res;
          this.delegeName =
            u?.fullName ?? u?.FullName ??
            u?.name     ?? u?.Name     ??
            u?.userName ?? u?.UserName ??
            u?.email    ?? u?.Email    ?? `#${s.id_User_Delegue}`;
          this.cdr.markForCheck();
        },
        error: () => { this.delegeName = `#${s.id_User_Delegue}`; }
      });
    }

    // Product name
    if (s.id_Produit > 0) {
      this.productSvc.getProductById(s.id_Produit).pipe(takeUntil(this.destroy$)).subscribe({
        next: (data: any) => {
          const raw = data?.Result ?? data?.result ?? data;
          this.productName = raw?.Nom ?? raw?.nom ?? `#${s.id_Produit}`;
          this.cdr.markForCheck();
        },
        error: () => { this.productName = `#${s.id_Produit}`; }
      });
    }

    // Lot expiration date
    if (s.numeroLot) {
      this.lotSvc.getLotByNumero(s.numeroLot).pipe(takeUntil(this.destroy$)).subscribe({
        next: lot => { this.lotDate = lot.dateExpiration ?? s.dateExpiration; this.cdr.markForCheck(); },
        error: ()  => { this.lotDate = s.dateExpiration; }
      });
    } else {
      this.lotDate = s.dateExpiration;
    }
  }
}
