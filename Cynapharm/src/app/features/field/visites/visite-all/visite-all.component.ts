import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { VisiteService, VisiteDto } from '../services/visite.service';
import { VisiteType } from '../../../../core/models/enums';

@Component({
  selector: 'app-visite-all',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './visite-all.component.html',
  styles: ['']
})
export class VisiteAllComponent implements OnInit, OnDestroy {
  visites:  VisiteDto[] = [];
  filtered: VisiteDto[] = [];
  loading    = false;
  error      = '';
  startDate  = '';
  endDate    = '';
  delegueId: number | null = null;
  VisiteType = VisiteType;

  private destroy$ = new Subject<void>();

  constructor(
    private svc: VisiteService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void { this.load(); }
  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  load(): void {
    this.loading = true;
    this.error   = '';
    this.svc.getAll(this.startDate || undefined, this.endDate || undefined)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: data => {
          this.visites = data;
          this.applyDelegueFilter();
          this.loading = false;
          this.cdr.markForCheck();
        },
        error: () => {
          this.error   = 'Impossible de charger les visites.';
          this.loading = false;
          this.cdr.markForCheck();
        }
      });
  }

  applyDelegueFilter(): void {
    this.filtered = this.delegueId
      ? this.visites.filter(v => v.id_User_Delegue === this.delegueId)
      : [...this.visites];
  }

  onDelegueFilter(): void { this.applyDelegueFilter(); }

  onRow(v: VisiteDto): void {
    if (v.idVisite) this.router.navigate(['/field/visites', v.idVisite, 'edit']);
  }

  typeLabel(t: VisiteType): string {
    switch (t) {
      case VisiteType.Medecin:    return 'Médecin';
      case VisiteType.Pharmacien: return 'Pharmacien';
      default:                    return String(t);
    }
  }
}
