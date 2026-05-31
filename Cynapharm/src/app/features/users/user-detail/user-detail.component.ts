import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subject, of } from 'rxjs';
import { takeUntil, catchError } from 'rxjs/operators';

import { UserService } from '../user.service';
import { ToastService } from '../../../shared/services/toast.service';
import { RapportService, RapportDto } from '../../field/rapports/services/rapport.service';
import { InventoryBusinessService, StockSummaryDto } from '../../inventory/stocks/services/inventory-business.service';
import { StockMovementService, StockMovementDto } from '../../inventory/movements/services/stock-movement.service';
import { RegionService } from '../../field/regions/services/region.service';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';

@Component({
  selector: 'app-user-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, CardComponent, ButtonComponent],
  templateUrl: './user-detail.component.html',
  styleUrls: ['./user-detail.component.css']
})
export class UserDetailComponent implements OnInit, OnDestroy {

  user: any = null;
  loading = true;
  error   = '';
  regionName = '—';
  activeTab: 'info' | 'rapports' | 'mouvements' = 'info';
  rapports: RapportDto[] = [];
  loadingRapports = false;
  stockSummary: StockSummaryDto | null = null;
  loadingStockSummary = false;
  movements: StockMovementDto[] = [];
  loadingMovements = false;

  // Modal confirmation
  showActionModal  = false;
  modalAction: 'disable' | 'enable' | '' = '';
  isProcessing = false;

  private userId!: number;
  private readonly destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private userService: UserService,
    private rapportService: RapportService,
    private inventoryBizSvc: InventoryBusinessService,
    private movementSvc: StockMovementService,
    private toastService: ToastService,
    private regionSvc: RegionService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.userId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadUserDetails();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadUserDetails(): void {
    this.loading = true;
    this.error   = '';

    this.userService.getUserById(this.userId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          const raw = response?.result ?? response?.Result ?? response;
          if (!raw) {
            this.error   = 'Aucune donnée reçue du serveur.';
            this.loading = false;
            this.cdr.markForCheck();
            return;
          }
          this.user = {
            ...raw,
            id:          raw.id          ?? raw.Id,
            name:        raw.name        ?? raw.Name        ?? '',
            email:       raw.email       ?? raw.Email       ?? '',
            phoneNumber: raw.phoneNumber ?? raw.PhoneNumber ?? '',
            adresse:     raw.adresse     ?? raw.Adresse     ?? '',
            role:        raw.role        ?? raw.Role        ?? '',
            isDeleted:   raw.isDeleted   ?? raw.IsDeleted   ?? false
          };
          this.loading = false;
          const role = (this.user?.role ?? '').toUpperCase();
          if (role === 'DELEGUE') {
            this.loadRapports();
            this.loadStockSummary();
            this.loadMovements();
          }
          if ((role === 'DELEGUE' || role === 'SUPERVISEUR') && this.user?.idRegion) {
            this.loadRegionName(this.user.idRegion);
          }
          this.cdr.markForCheck();
        },
        error: (err: any) => {
          this.error   = this.resolveError(err);
          this.loading = false;
          this.cdr.markForCheck();
        }
      });
  }

  private loadRegionName(idRegion: number): void {
    this.regionSvc.getAll()
      .pipe(catchError(() => of([])), takeUntil(this.destroy$))
      .subscribe(regions => {
        const found = regions.find(r => {
          const id = (r as any).Id_Region ?? r.id_Region;
          return id === idRegion;
        });
        this.regionName = (found as any)?.NomRegion ?? found?.nomRegion ?? '—';
        this.cdr.markForCheck();
      });
  }

  private loadRapports(): void {
    this.loadingRapports = true;
    this.rapportService.getByDelegue(this.userId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: data => { this.rapports = data; this.loadingRapports = false; this.cdr.markForCheck(); },
        error: ()   => { this.rapports = []; this.loadingRapports = false; this.cdr.markForCheck(); }
      });
  }

  private loadStockSummary(): void {
    this.loadingStockSummary = true;
    this.inventoryBizSvc.getStockSummary(this.userId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: data => { this.stockSummary = data; this.loadingStockSummary = false; this.cdr.markForCheck(); },
        error: ()   => { this.stockSummary = null; this.loadingStockSummary = false; this.cdr.markForCheck(); }
      });
  }

  private loadMovements(): void {
    this.loadingMovements = true;
    this.movementSvc.getMovementsByDelegue(this.userId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: data => { this.movements = data; this.loadingMovements = false; this.cdr.markForCheck(); },
        error: ()   => { this.movements = []; this.loadingMovements = false; this.cdr.markForCheck(); }
      });
  }

  setTab(t: 'info' | 'rapports' | 'mouvements'): void { this.activeTab = t; }

  getMovementClass(type: string): string {
    switch ((type ?? '').toLowerCase()) {
      case 'increment':     return 'mv-green';
      case 'decrement':     return 'mv-orange';
      case 'transfer-in':   return 'mv-cyan';
      case 'transfer-out':  return 'mv-red';
      default:              return '';
    }
  }

  getResultatClass(r: string): string {
    switch ((r ?? '').toUpperCase()) {
      case 'POSITIF':    return 'chip-success';
      case 'NEGATIF':    return 'chip-danger';
      case 'EN_ATTENTE': return 'chip-warn';
      default:           return '';
    }
  }

  private resolveError(err: any): string {
    if (err.status === 401) return 'Session expirée — veuillez vous reconnecter.';
    if (err.status === 403) return 'Accès refusé — rôle ADMIN requis.';
    if (err.status === 404) return 'Utilisateur introuvable.';
    if (err.status === 0)   return 'Passerelle inaccessible — vérifiez localhost:5555.';
    return err.error?.message ?? `Erreur ${err.status}`;
  }

  // ── Helpers ──────────────────────────────────────────

  getInitials(name: string): string {
    if (!name) return '?';
    return name.split(' ').slice(0, 2).map(w => w[0]?.toUpperCase() ?? '').join('');
  }

  getRoleClass(role: string): string {
    switch ((role || '').toUpperCase()) {
      case 'ADMIN':       return 'role-admin';
      case 'SUPERVISEUR': return 'role-superviseur';
      case 'DELEGUE':     return 'role-delegue';
      case 'MEDECIN':     return 'role-medecin';
      case 'CLIENT':      return 'role-client';
      default:            return 'role-default';
    }
  }

  // ── Navigation ────────────────────────────────────────

  onEdit(): void {
    this.router.navigate(['/users', this.userId, 'edit']);
  }

  // ── Actions avec modal ────────────────────────────────

  openDisable(): void { this.modalAction = 'disable'; this.showActionModal = true; }
  openEnable():  void { this.modalAction = 'enable';  this.showActionModal = true; }
  cancelAction(): void { this.showActionModal = false; this.modalAction = ''; }

  confirmAction(): void {
    if (!this.user || !this.modalAction) return;
    const email = this.user.email;
    if (!email) return;

    this.isProcessing = true;

    const req$ = this.modalAction === 'disable'
      ? this.userService.disableUser(email)
      : this.userService.enableUser(email);

    req$.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.isProcessing   = false;
        this.showActionModal = false;
        const msg = this.modalAction === 'disable'
          ? 'Utilisateur désactivé avec succès.'
          : 'Utilisateur réactivé avec succès.';
        this.toastService.showSuccess(msg);
        this.loadUserDetails();
      },
      error: (err: any) => {
        this.isProcessing   = false;
        this.showActionModal = false;
        this.toastService.showError(this.resolveError(err));
        this.cdr.markForCheck();
      }
    });
  }
}
