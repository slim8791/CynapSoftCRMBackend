import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { RapportService, RapportDto } from '../services/rapport.service';
import { UserService } from '../../../../features/users/user.service';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { VisiteService, VisiteDto } from '../../visites/services/visite.service';
import { VisiteType } from '../../../../core/models/enums';

@Component({
  selector: 'app-rapport-list',
  standalone: true,
  imports: [CommonModule, RouterLink, EmptyStateComponent],
  templateUrl: './rapport-list.component.html',
  styleUrls: ['./rapport-list.component.css']
})
export class RapportListComponent implements OnInit, OnDestroy {
  rapports: RapportDto[] = [];
  loading   = false;
  error     = '';

  delegueNames: Record<number, string> = {};
  visiteDetails: Record<number, VisiteDto> = {};

  private destroy$ = new Subject<void>();

  constructor(
    private svc:     RapportService,
    private userSvc: UserService,
    private visiteSvc: VisiteService,
    private cdr:     ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.load();
    this.userSvc.getUsersByRole('DELEGUE').pipe(takeUntil(this.destroy$)).subscribe({
      next: users => {
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
      next: data => {
        this.rapports = data;
        this.loadVisiteDetails(data);
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => { this.error = 'Impossible de charger les rapports.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  getDelegrueName(id: number): string { return this.delegueNames[id] ?? `#${id}`; }

  visiteDate(visite: VisiteDto): string {
    return (visite as any).dateVisite ?? visite.date;
  }

  visiteTypeLabel(type: VisiteType): string {
    switch (type) {
      case VisiteType.Medecin: return 'Médecin';
      case VisiteType.Pharmacien: return 'Pharmacien';
      default: return 'Autre';
    }
  }

  private loadVisiteDetails(rapports: RapportDto[]): void {
    [...new Set(rapports.map(r => r.id_Visite).filter(id => id > 0))]
      .filter(id => !this.visiteDetails[id])
      .forEach(id => {
        this.visiteSvc.getById(id).pipe(takeUntil(this.destroy$)).subscribe({
          next: visite => { this.visiteDetails[id] = visite; this.cdr.markForCheck(); },
          error: () => {}
        });
      });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
