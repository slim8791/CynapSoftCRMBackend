import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { PromotionService, PromotionDto } from '../services/promotion.service';
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

  private destroy$ = new Subject<void>();

  constructor(
    private svc:   PromotionService,
    private toast: ToastService,
    private cdr:   ChangeDetectorRef
  ) {}

  ngOnInit(): void { this.load(); }
  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  private load(): void {
    this.loading = true;
    this.svc.getAll().pipe(takeUntil(this.destroy$)).subscribe({
      next: data => { this.promotions = data.filter(p => p.pourcentage > 0); this.loading = false; this.cdr.markForCheck(); },
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
}
