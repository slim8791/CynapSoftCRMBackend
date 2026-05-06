import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ObjectifService, ObjectifDto } from '../services/objectif.service';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-objectif-list',
  standalone: true,
  imports: [CommonModule, RouterLink, EmptyStateComponent],
  templateUrl: './objectif-list.component.html',
  styleUrls: ['./objectif-list.component.css']
})
export class ObjectifListComponent implements OnInit, OnDestroy {
  objectifs: ObjectifDto[] = [];
  loading = false;
  error = '';

  private destroy$ = new Subject<void>();

  constructor(private svc: ObjectifService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.error = '';
    this.svc.getAll().pipe(takeUntil(this.destroy$)).subscribe({
      next: data => { this.objectifs = data; this.loading = false; this.cdr.markForCheck(); },
      error: () => { this.error = 'Impossible de charger les objectifs.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  progressPct(realise: number, cible: number): number {
    if (!cible || cible <= 0) return 0;
    return Math.min(100, Math.round((realise / cible) * 100));
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
