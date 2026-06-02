import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject, forkJoin, of } from 'rxjs';
import { takeUntil, switchMap, map, catchError } from 'rxjs/operators';
import { RapportService, RapportDto } from '../services/rapport.service';
import { UserService } from '../../../../features/users/user.service';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { VisiteService, VisiteDto } from '../../visites/services/visite.service';
import { VisiteType } from '../../../../core/models/enums';
import { AuthService, UserRole } from '../../../../core/services/auth.service';

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
  successMsg = '';
  expandedRapportId: number | null = null;
  validating: Record<number, boolean> = {};
  validatedRapports: Record<number, boolean> = {};

  delegueNames: Record<number, string> = {};
  visiteDetails: Record<number, VisiteDto> = {};

  private destroy$ = new Subject<void>();

  constructor(
    private svc:     RapportService,
    private userSvc: UserService,
    private visiteSvc: VisiteService,
    private authSvc: AuthService,
    private cdr:     ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.load();
    this.userSvc.getUsersByRole('DELEGUE').pipe(takeUntil(this.destroy$)).subscribe({
      next: users => {
        users.forEach(u => {
          const id = this.userSvc.userId(u);
          if (id != null) this.delegueNames[id] = this.userName(u, id);
        });
        this.cdr.markForCheck();
      },
      error: () => {}
    });
  }

  load(): void {
    this.loading = true;
    this.error   = '';
    this.successMsg = '';
    const source$ = this.isDelegue
      ? this.svc.getByDelegue(this.authSvc.getUserId())
      : this.svc.getAll();
      
    source$.pipe(
      switchMap(data => {
        const delegueIds = [...new Set(data.map(r => r.id_User_Delegue).filter(id => id && !this.delegueNames[id]))];
        const visiteIds = [...new Set(data.map(r => r.id_Visite).filter(id => id > 0 && !this.visiteDetails[id]))];

        const delegueReq$ = delegueIds.length > 0 
          ? this.userSvc.getDisplayNamesByIds(delegueIds).pipe(catchError(() => of({})))
          : of({});

        const visiteReqs$ = visiteIds.length > 0
          ? forkJoin(visiteIds.map(id => this.visiteSvc.getById(id).pipe(
              map(v => ({ id, v })),
              catchError(() => of(null))
            )))
          : of([]);

        return forkJoin([of(data), delegueReq$, visiteReqs$]);
      }),
      takeUntil(this.destroy$)
    ).subscribe({
      next: ([data, newDelegues, newVisites]) => {
        this.delegueNames = { ...this.delegueNames, ...newDelegues };
        
        (newVisites as any[]).forEach(res => {
          if (res && res.v) {
            this.visiteDetails[res.id] = res.v;
          }
        });

        this.rapports = data as RapportDto[];
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => { 
        this.error = 'Impossible de charger les rapports.'; 
        this.loading = false; 
        this.cdr.markForCheck(); 
      }
    });
  }

  get isAdmin():       boolean { return this.authSvc.getUserRole()?.toUpperCase() === 'ADMIN'; }
  get isSuperviseur(): boolean { return this.authSvc.getUserRole()?.toUpperCase() === 'SUPERVISEUR'; }
  get isDelegue():     boolean { return this.authSvc.getUserRole()?.toUpperCase() === 'DELEGUE'; }

  get canWriteReports(): boolean {
    const role = this.authSvc.getUserRole();
    return role === UserRole.ADMIN || role === UserRole.DELEGUE;
  }

  getDelegrueName(id: number): string { return this.delegueName(id); }

  delegueName(id: number): string { return this.delegueNames[id] ?? `#${id}`; }

  toggleDetails(r: RapportDto): void {
    this.expandedRapportId = this.expandedRapportId === r.idRapport ? null : (r.idRapport ?? null);
  }

  isValidated(r: RapportDto): boolean {
    const id = r.idRapport;
    return !!(id && this.validatedRapports[id])
      || r.idSuperviseurValidateur != null
      || this.visiteDetails[r.id_Visite]?.isCompleted === true;
  }

  isValidating(r: RapportDto): boolean {
    return !!(r.idRapport && this.validating[r.idRapport]);
  }

  validateReport(r: RapportDto): void {
    const idRapport = r.idRapport;
    const currentUserId = this.authSvc.getCurrentUser()?.id;
    if (!idRapport || !currentUserId || this.validating[idRapport] || this.isValidated(r)) return;

    this.error = '';
    this.successMsg = '';
    this.validating[idRapport] = true;
    this.svc.validate(idRapport, currentUserId).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.validatedRapports[idRapport] = true;
        r.idSuperviseurValidateur = currentUserId;
        const visite = this.visiteDetails[r.id_Visite];
        if (visite) {
          this.visiteDetails[r.id_Visite] = { ...visite, isCompleted: true };
        }
        this.successMsg = 'Rapport validé et visite clôturée.';
        this.validating[idRapport] = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.error = 'Impossible de valider le rapport.';
        this.validating[idRapport] = false;
        this.cdr.markForCheck();
      }
    });
  }

  visiteDate(visite: VisiteDto): string {
    return visite.dateVisite;
  }

  visiteTypeLabel(type: VisiteType): string {
    switch (type) {
      case VisiteType.Medecin: return 'Médecin';
      case VisiteType.Pharmacien: return 'Pharmacien';
      default: return 'Autre';
    }
  }

  private userName(u: any, id: number): string {
    return this.userSvc.displayName(u, id);
  }



  // Modale de suppression
  showDeleteModal = false;
  deletingItem: any = null;
  deleting = false;

  deleteRapport(o: RapportDto): void {
    if (!o.idRapport || o.idRapport === 0) {
      alert('Impossible de trouver l\'ID du rapport.');
      return;
    }
    this.deletingItem = { ...o, __computedId: o.idRapport };
    this.showDeleteModal = true;
  }

  cancelDelete(): void {
    this.showDeleteModal = false;
    this.deletingItem = null;
  }

  confirmDelete(): void {
    if (!this.deletingItem || !this.deletingItem.__computedId) return;
    this.deleting = true;
    this.svc.delete(this.deletingItem.__computedId).subscribe({
      next: () => {
        this.deleting = false;
        this.showDeleteModal = false;
        this.deletingItem = null;
        this.load();
      },
      error: (err) => {
        this.deleting = false;
        this.showDeleteModal = false;
        alert('Erreur lors de la suppression. Code: ' + (err?.status || 'Inconnu'));
      }
    });
  }

  getGoogleMapsUrl(r: RapportDto): string {
    return `https://www.google.com/maps?q=${r.latitude},${r.longitude}`;
  }

  parseProduitsDiscutes(r: RapportDto): { id: number; nom: string }[] {
    if (!r.produitsDiscutes) return [];
    try {
      const parsed = JSON.parse(r.produitsDiscutes);
      return Array.isArray(parsed) ? parsed : [];
    } catch {
      return [];
    }
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
