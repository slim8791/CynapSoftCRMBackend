import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { PlanningService, PlanningDto } from '../services/planning.service';
import { EtatPlanning, PLANNING_STATUS_LABELS } from '../../../../core/models/enums/index';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { UserService } from '../../../users/user.service';

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
  delegues: any[] = [];
  delegueNames: Record<number, string> = {};

  private destroy$ = new Subject<void>();

  constructor(private svc: PlanningService, private userSvc: UserService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.userSvc.getUsersByRole('DELEGUE').pipe(takeUntil(this.destroy$)).subscribe({
      next: users => {
        this.delegues = users;
        users.forEach(u => {
          const id = u?.id ?? u?.Id;
          if (id != null) this.delegueNames[id] = this.userName(u);
        });
        this.cdr.markForCheck();
      },
      error: () => {}
    });
  }

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

  userName(u: any): string {
    return u?.name ?? u?.Name ?? u?.fullName ?? u?.FullName ?? u?.email ?? u?.Email ?? `#${u?.id ?? u?.Id}`;
  }

  delegueName(id: number): string {
    return this.delegueNames[id] ?? `#${id}`;
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
