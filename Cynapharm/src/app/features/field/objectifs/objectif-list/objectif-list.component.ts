import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ObjectifService, ObjectifDto } from '../services/objectif.service';
import { UserService } from '../../../../features/users/user.service';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-objectif-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, EmptyStateComponent],
  templateUrl: './objectif-list.component.html',
  styleUrls: ['./objectif-list.component.css']
})
export class ObjectifListComponent implements OnInit, OnDestroy {
  objectifs: ObjectifDto[] = [];
  loading   = false;
  error     = '';

  delegueNames: Record<number, string> = {};
  delegues: any[] = [];
  selectedDelegueId: number | null = null;

  private destroy$ = new Subject<void>();

  constructor(
    private svc:     ObjectifService,
    private userSvc: UserService,
    private cdr:     ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.load();
    this.userSvc.getUsersByRole('DELEGUE').pipe(takeUntil(this.destroy$)).subscribe({
      next: users => {
        this.delegues = users;
        users.forEach(u => {
          const id = u?.id ?? u?.Id;
          if (id != null) this.delegueNames[id] = u?.name ?? u?.Name ?? u?.email ?? `#${id}`;
        });
        this.cdr.markForCheck();
      },
      error: () => {}
    });
  }

  load(): void {
    this.loading = true;
    this.error   = '';
    this.svc.getAll().pipe(takeUntil(this.destroy$)).subscribe({
      next: data => { this.objectifs = data; this.loading = false; this.cdr.markForCheck(); },
      error: () => { this.error = 'Impossible de charger les objectifs.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  getDelegrueName(id: number): string { return this.delegueNames[id] ?? `#${id}`; }

  get filteredObjectifs(): ObjectifDto[] {
    return this.selectedDelegueId
      ? this.objectifs.filter(o => o.id_User_Delegue === this.selectedDelegueId)
      : this.objectifs;
  }

  userName(u: any): string {
    return u?.name ?? u?.Name ?? u?.fullName ?? u?.FullName ?? u?.email ?? u?.Email ?? `#${u?.id ?? u?.Id}`;
  }

  progressPct(realise: number, cible: number): number {
    if (!cible || cible <= 0) return 0;
    return Math.min(100, Math.round((realise / cible) * 100));
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
