import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { ReclamationService, ReclamationDto } from '../../services/reclamation.service';
import { AuthService, UserRole } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-reclamation-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './reclamation-detail.component.html',
  styleUrls: ['./reclamation-detail.component.css'],
})
export class ReclamationDetailComponent implements OnInit, OnDestroy {

  rec:     ReclamationDto | null = null;
  loading  = true;
  error    = '';
  isAdmin  = false;

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private svc:   ReclamationService,
    private auth:  AuthService,
    private cdr:   ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.isAdmin = this.auth.getUserRole() === UserRole.ADMIN;
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.svc.getById(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: r  => { this.rec = r; this.loading = false; this.cdr.markForCheck(); },
      error: () => { this.error = 'Réclamation introuvable.'; this.loading = false; this.cdr.markForCheck(); },
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  getStatutLabel = (s?: string) => this.svc.getStatutLabel(s);
  getStatutClass = (s?: string) => this.svc.getStatutClass(s);
}
