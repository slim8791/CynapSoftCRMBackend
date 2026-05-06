import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { PlanningService, PlanningDto } from '../services/planning.service';
import { EtatPlanning, PLANNING_STATUS_LABELS } from '../../../../core/models/enums/index';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-planning-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, EmptyStateComponent],
  templateUrl: './planning-list.component.html',
  styleUrls: ['./planning-list.component.css']
})
export class PlanningListComponent implements OnInit, OnDestroy {
  plannings: PlanningDto[] = [];
  loading = false;
  error = '';
  searched = false;
  delegueId: number | null = null;
  EtatPlanning = EtatPlanning;

  private destroy$ = new Subject<void>();

  constructor(private svc: PlanningService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {}

  load(): void {
    if (!this.delegueId) return;
    this.loading = true;
    this.error = '';
    this.searched = true;
    this.svc.getByDelegue(this.delegueId).pipe(takeUntil(this.destroy$)).subscribe({
      next: data => { this.plannings = data; this.loading = false; this.cdr.markForCheck(); },
      error: () => { this.error = 'Impossible de charger les plannings.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  statusLabel(e: EtatPlanning): string {
    return PLANNING_STATUS_LABELS[e] ?? String(e);
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
