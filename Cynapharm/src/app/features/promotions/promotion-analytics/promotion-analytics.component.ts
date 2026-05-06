import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil, catchError, of } from 'rxjs';

import { PromotionService } from '../services/promotion.service';

@Component({
  selector: 'app-promotion-analytics',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './promotion-analytics.component.html',
  styleUrls: ['./promotion-analytics.component.css']
})
export class PromotionAnalyticsComponent implements OnInit, OnDestroy {
  activeCount  = 0;
  coverageRate = 0;
  loading      = true;
  private destroy$ = new Subject<void>();

  constructor(private svc: PromotionService) {}

  ngOnInit(): void {
    forkJoin({
      count:    this.svc.getActiveCount().pipe(catchError(() => of(0))),
      coverage: this.svc.getCoverageRate().pipe(catchError(() => of(0)))
    }).pipe(takeUntil(this.destroy$)).subscribe(({ count, coverage }) => {
      this.activeCount  = count;
      this.coverageRate = coverage;
      this.loading      = false;
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
