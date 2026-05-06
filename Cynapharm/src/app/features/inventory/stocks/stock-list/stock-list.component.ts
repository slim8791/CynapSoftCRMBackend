import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { StockService, StockDelegueDto } from '../services/stock.service';
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
  stocks: StockDelegueDto[] = [];
  loading = false;
  error   = '';
  page    = 1;
  pageSize = 20;
  total   = 0;

  private destroy$ = new Subject<void>();

  constructor(private svc: StockService, private toast: ToastService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void { this.load(); }
  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  load(): void {
    this.loading = true;
    this.svc.getAll(this.page, this.pageSize).pipe(takeUntil(this.destroy$)).subscribe({
      next: data => {
        this.stocks = data;
        this.total  = data.length;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => { this.error = 'Erreur lors du chargement.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  onPage(p: number): void { this.page = p; this.load(); }
}
