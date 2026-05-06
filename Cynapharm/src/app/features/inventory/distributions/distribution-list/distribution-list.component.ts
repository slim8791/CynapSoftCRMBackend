import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DistributionService, EchantillonDto } from '../services/distribution.service';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';

type TabKey = 'by-delegue' | 'by-medecin' | 'by-pharmacien';

@Component({
  selector: 'app-distribution-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, EmptyStateComponent],
  templateUrl: './distribution-list.component.html',
  styleUrls: ['./distribution-list.component.css']
})
export class DistributionListComponent implements OnInit, OnDestroy {
  distributions: EchantillonDto[] = [];
  loading = false;
  error = '';
  searched = false;
  activeTab: TabKey = 'by-delegue';
  inputId: number | null = null;

  tabs = [
    { key: 'by-delegue' as TabKey,    label: 'Par délégué' },
    { key: 'by-medecin' as TabKey,    label: 'Par médecin' },
    { key: 'by-pharmacien' as TabKey, label: 'Par pharmacien' }
  ];

  get tabLabel(): string {
    return this.tabs.find(t => t.key === this.activeTab)?.label.replace('Par ', '') ?? '';
  }

  private destroy$ = new Subject<void>();

  constructor(private svc: DistributionService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {}

  switchTab(key: TabKey): void {
    this.activeTab = key;
    this.distributions = [];
    this.inputId = null;
    this.searched = false;
    this.error = '';
  }

  load(): void {
    if (!this.inputId) return;
    this.loading = true;
    this.error = '';
    this.searched = true;

    const id = this.inputId;
    const obs$ = this.activeTab === 'by-delegue'
      ? this.svc.getByDelegue(id)
      : this.activeTab === 'by-medecin'
        ? this.svc.getByMedecin(id)
        : this.svc.getByPharmacien(id);

    obs$.pipe(takeUntil(this.destroy$)).subscribe({
      next: data => { this.distributions = data; this.loading = false; this.cdr.markForCheck(); },
      error: () => { this.error = 'Impossible de charger les distributions.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
