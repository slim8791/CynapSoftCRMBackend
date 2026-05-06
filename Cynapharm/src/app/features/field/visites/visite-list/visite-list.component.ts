import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { VisiteService, VisiteDto } from '../services/visite.service';
import { VisiteType } from '../../../../core/models/enums/index';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-visite-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, EmptyStateComponent],
  templateUrl: './visite-list.component.html',
  styleUrls: ['./visite-list.component.css']
})
export class VisiteListComponent implements OnInit, OnDestroy {
  visites: VisiteDto[] = [];
  loading = false;
  error = '';
  searched = false;
  delegueId: number | null = null;
  VisiteType = VisiteType;

  private destroy$ = new Subject<void>();

  constructor(private svc: VisiteService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {}

  load(): void {
    if (!this.delegueId) return;
    this.loading = true;
    this.error = '';
    this.searched = true;
    this.svc.getByDelegue(this.delegueId).pipe(takeUntil(this.destroy$)).subscribe({
      next: data => { this.visites = data; this.loading = false; this.cdr.markForCheck(); },
      error: () => { this.error = 'Impossible de charger les visites.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  visiteTypeLabel(t: VisiteType): string {
    switch (t) {
      case VisiteType.Medecin:    return 'Médecin';
      case VisiteType.Pharmacien: return 'Pharmacien';
      default:                    return 'Autre';
    }
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
