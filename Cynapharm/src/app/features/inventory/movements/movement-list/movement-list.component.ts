import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { StockMovementService, StockMovementDto } from '../../movements/services/stock-movement.service';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-movement-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, EmptyStateComponent],
  templateUrl: './movement-list.component.html',
  styleUrls: ['./movement-list.component.css']
})
export class MovementListComponent implements OnInit, OnDestroy {
  movements: StockMovementDto[] = [];
  loading = false;
  error = '';
  filterStockId: number | null = null;
  activeStockId: number | null = null;

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private svc: StockMovementService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const idStock = this.route.snapshot.queryParamMap.get('idStock');
    if (idStock) {
      this.filterStockId = Number(idStock);
      this.applyFilter();
    }
  }

  applyFilter(): void {
    if (!this.filterStockId) return;
    this.activeStockId = this.filterStockId;
    this.loading = true;
    this.error = '';
    this.svc.getMovements(this.activeStockId).pipe(takeUntil(this.destroy$)).subscribe({
      next: data => { this.movements = data; this.loading = false; this.cdr.markForCheck(); },
      error: () => { this.error = 'Impossible de charger les mouvements.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  clearFilter(): void {
    this.filterStockId = null;
    this.activeStockId = null;
    this.movements = [];
    this.error = '';
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
