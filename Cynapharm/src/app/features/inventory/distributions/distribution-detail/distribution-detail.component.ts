import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DistributionService, EchantillonDto } from '../services/distribution.service';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-distribution-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, EmptyStateComponent],
  templateUrl: './distribution-detail.component.html',
  styleUrls: ['./distribution-detail.component.css']
})
export class DistributionDetailComponent implements OnInit, OnDestroy {
  item: EchantillonDto | null = null;
  loading = false;
  error = '';

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private svc: DistributionService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) { this.error = 'Identifiant invalide.'; return; }
    this.loading = true;
    this.svc.getById(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: data => { this.item = data; this.loading = false; this.cdr.markForCheck(); },
      error: () => { this.error = 'Impossible de charger la distribution.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
