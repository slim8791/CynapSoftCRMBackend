import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { StockMovementService, StockMovementDto } from '../../movements/services/stock-movement.service';
import { AuthService } from '../../../../core/services/auth.service';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-movement-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, EmptyStateComponent],
  templateUrl: './movement-list.component.html',
  styleUrls: ['./movement-list.component.css']
})
export class MovementListComponent implements OnInit, OnDestroy {
  allMovements: StockMovementDto[] = [];
  movements: StockMovementDto[] = [];
  loading = false;
  error = '';
  filterStockId: number | null = null;
  activeStockId: number | null = null;
  dateDebut = '';
  dateFin = '';
  typeMovement = '';

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private svc:   StockMovementService,
    private auth:  AuthService,
    private cdr:   ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const idStock = this.route.snapshot.queryParamMap.get('idStock');
    if (idStock) {
      this.filterStockId = Number(idStock);
      this.applyFilter();
    } else {
      const userId = this.auth.getCurrentUser()?.id;
      if (userId) {
        this.loading = true;
        this.svc.getMovementsByDelegue(userId).pipe(takeUntil(this.destroy$)).subscribe({
          next: data => {
            this.allMovements = data;
            this.applyClientFilters();
            this.loading = false;
            this.cdr.markForCheck();
          },
          error: () => { this.loading = false; this.cdr.markForCheck(); }
        });
      }
    }
  }

  applyFilter(): void {
    if (!this.filterStockId) return;
    this.activeStockId = this.filterStockId;
    this.loading = true;
    this.error = '';
    this.svc.getMovements(this.activeStockId).pipe(takeUntil(this.destroy$)).subscribe({
      next: data => {
        this.allMovements = data;
        this.applyClientFilters();
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => { this.error = 'Impossible de charger les mouvements.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  clearFilter(): void {
    this.filterStockId = null;
    this.activeStockId = null;
    this.dateDebut = '';
    this.dateFin = '';
    this.typeMovement = '';
    this.allMovements = [];
    this.movements = [];
    this.error = '';
  }

  applyClientFilters(): void {
    let result = this.allMovements;

    if (this.dateDebut) {
      const start = new Date(`${this.dateDebut}T00:00:00`);
      result = result.filter(m => m.dateMovement && new Date(m.dateMovement) >= start);
    }

    if (this.dateFin) {
      const end = new Date(`${this.dateFin}T23:59:59`);
      result = result.filter(m => m.dateMovement && new Date(m.dateMovement) <= end);
    }

    if (this.typeMovement) {
      result = result.filter(m => m.typeMovement?.toLowerCase() === this.typeMovement.toLowerCase());
    }

    this.movements = result;
    this.cdr.markForCheck();
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
