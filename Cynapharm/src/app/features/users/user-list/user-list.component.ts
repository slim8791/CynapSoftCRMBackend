import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, NavigationEnd } from '@angular/router';
import { Subject, forkJoin, of } from 'rxjs';
import { takeUntil, filter, debounceTime, distinctUntilChanged, switchMap, catchError, map } from 'rxjs/operators';

import { UserService } from '../user.service';
import { AuthService, UserRole } from '../../../core/services/auth.service';
import { RegionService, RegionDto } from '../../field/regions/services/region.service';

type StatusFilter = 'all' | 'active' | 'disabled';
type RoleFilter   = 'all' | 'ADMIN' | 'SUPERVISEUR' | 'DELEGUE' | 'MEDECIN' | 'CLIENT';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.css']
})
export class UserListComponent implements OnInit, OnDestroy {

  // ── Données ──────────────────────────────────────────
  allUsers:      any[] = [];
  filteredUsers: any[] = [];
  loading  = false;
  error    = '';
  currentRegion: RegionDto | null = null;

  // ── Recherche & filtres ──────────────────────────────
  searchTerm:   string       = '';
  statusFilter: StatusFilter = 'active';
  roleFilter:   RoleFilter   = 'all';

  get ROLES(): RoleFilter[] {
    if (this.isSuperviseur) return ['DELEGUE', 'CLIENT'];
    return ['ADMIN', 'SUPERVISEUR', 'DELEGUE', 'MEDECIN', 'CLIENT'];
  }

  // ── Pagination ───────────────────────────────────────
  currentPage = 1;
  readonly pageSize = 10;

  // ── Modal de confirmation ────────────────────────────
  showConfirmModal = false;
  confirmAction: 'enable' | 'disable' | '' = '';
  confirmUser: any = null;
  isProcessing = false;

  private readonly destroy$      = new Subject<void>();
  private readonly searchSubject = new Subject<string>();

  constructor(
    private userService: UserService,
    private authSvc: AuthService,
    private regionSvc: RegionService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    this.router.events.pipe(
      filter(e => e instanceof NavigationEnd),
      filter((e: any) => e.url === '/users'),
      takeUntil(this.destroy$)
    ).subscribe(() => this.loadUsers());
  }

  // ── Role-based access ──────────────────────────────────

  get isAdmin(): boolean {
    return this.authSvc.getUserRole()?.toUpperCase() === 'ADMIN';
  }

  get isSuperviseur(): boolean {
    return this.authSvc.getUserRole()?.toUpperCase() === 'SUPERVISEUR';
  }

  get canCreate(): boolean {
    return this.isAdmin || this.isSuperviseur;
  }

  get canEdit(): boolean {
    return this.isAdmin || this.isSuperviseur;
  }

  ngOnInit(): void {
    // Debounce recherche : déclenche à partir du 3ème caractère
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(term => {
      if (term.length >= 3) {
        this.performSearch(term);
      } else {
        this.applyStatusFilter();
      }
    });

    this.loadUsers();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── Chargement initial ───────────────────────────────

  private loadUsers(): void {
    this.loading = true;
    this.error   = '';
    this.cdr.markForCheck();

    if (this.isSuperviseur) {
      this.loadUsersByRole();
      return;
    }

    this.userService.getUsers()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          const raw = Array.isArray(response)
            ? response
            : response?.result ?? response?.Result ?? [];
          this.allUsers = raw.map((u: any) => this.normalizeUser(u));
          this.applyStatusFilter();
          this.loading = false;
          this.cdr.markForCheck();
        },
        error: (err: any) => {
          this.error   = this.resolveError(err);
          this.loading = false;
          this.cdr.markForCheck();
        }
      });
  }

  private loadUsersByRole(): void {
    const userId = this.authSvc.getUserId();

    this.regionSvc.getBySuperviseur(userId).pipe(
      switchMap(region => {
        this.currentRegion = region;
        if (region?.id_Region) {
          return this.userService.getUsersByRegion(region.id_Region);
        }
        return forkJoin([
          this.userService.getUsersByRole('DELEGUE'),
          this.userService.getUsersByRole('CLIENT')
        ]).pipe(map(([d, c]: [any[], any[]]) => [...d, ...c]));
      }),
      catchError(() => of([])),
      takeUntil(this.destroy$)
    ).subscribe({
      next: (users: any[]) => {
        this.allUsers = users.map((u: any) => this.normalizeUser(u));
        this.applyStatusFilter();
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err: any) => {
        this.error   = this.resolveError(err);
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  // ── Filtres combinés : statut + rôle (frontend) ──────

  applyStatusFilter(): void {
    let result = [...this.allUsers];

    // Filtre par statut
    if (this.statusFilter === 'active')   result = result.filter(u => !u.isDeleted);
    if (this.statusFilter === 'disabled') result = result.filter(u =>  u.isDeleted);

    // Filtre par rôle
    if (this.roleFilter !== 'all') {
      result = result.filter(u => (u.role ?? '').toUpperCase() === this.roleFilter);
    }

    this.filteredUsers = result;
    this.currentPage   = 1;
    this.cdr.markForCheck();
  }

  onRoleFilterChange(): void {
    if (this.searchTerm.length >= 3) {
      this.applyRoleFilterOnSearchResults();
    } else {
      this.applyStatusFilter();
    }
  }

  // Applique le filtre rôle sur les résultats de recherche déjà affichés
  private applyRoleFilterOnSearchResults(): void {
    if (this.roleFilter === 'all') return;
    this.filteredUsers = this.filteredUsers.filter(
      u => (u.role ?? '').toUpperCase() === this.roleFilter
    );
    this.currentPage = 1;
    this.cdr.markForCheck();
  }

  // ── Pagination ────────────────────────────────────────

  get totalPages(): number { return Math.ceil(this.filteredUsers.length / this.pageSize) || 1; }

  get paginatedUsers(): any[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredUsers.slice(start, start + this.pageSize);
  }

  get pageNumbers(): number[] {
    const maxVisible = 5;
    let start = Math.max(1, this.currentPage - Math.floor(maxVisible / 2));
    const end = Math.min(this.totalPages, start + maxVisible - 1);
    start = Math.max(1, end - maxVisible + 1);
    return Array.from({ length: end - start + 1 }, (_, i) => start + i);
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) { this.currentPage = page; this.cdr.markForCheck(); }
  }

  onStatusFilterChange(): void {
    if (this.searchTerm.length >= 3) {
      this.performSearch(this.searchTerm);
    } else {
      this.applyStatusFilter();
    }
  }

  // ── Recherche backend (>= 3 chars) ───────────────────

  onSearchInput(term: string): void {
    this.searchSubject.next(term);
  }

  private performSearch(keyword: string): void {
    this.loading = true;
    this.cdr.markForCheck();

    const isActive = this.statusFilter === 'active'
      ? true
      : this.statusFilter === 'disabled'
        ? false
        : undefined;

    this.userService.searchUsers(keyword, isActive)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          const raw = Array.isArray(response)
            ? response
            : response?.result ?? response?.Result ?? [];
          let results = raw.map((u: any) => this.normalizeUser(u));
          // Filtre rôle côté client sur les résultats backend
          if (this.roleFilter !== 'all') {
            results = results.filter((u: any) => (u.role ?? '').toUpperCase() === this.roleFilter);
          }
          this.filteredUsers = results;
          this.currentPage   = 1;
          this.loading       = false;
          this.cdr.markForCheck();
        },
        error: () => {
          this.applyStatusFilter();
          this.loading = false;
          this.cdr.markForCheck();
        }
      });
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.applyStatusFilter();  // roleFilter remains active
  }

  // ── Helpers ──────────────────────────────────────────

  private normalizeUser(u: any): any {
    return {
      ...u,
      id:          u.id          ?? u.Id          ?? u.userId,
      name:        u.name        ?? u.Name        ?? u.nom      ?? '',
      email:       u.email       ?? u.Email       ?? '',
      role:        u.role        ?? u.Role        ?? '',
      phoneNumber: u.phoneNumber ?? u.PhoneNumber ?? '',
      adresse:     u.adresse     ?? u.Adresse     ?? '',
      isDeleted:   u.isDeleted   ?? u.IsDeleted   ?? false,
      typeClient:  u.typeClient  ?? u.TypeClient  ?? null,
      idRegion:    u.idRegion    ?? u.IdRegion     ?? null
    };
  }

  getInitials(name: string): string {
    if (!name) return '?';
    return name.split(' ')
      .slice(0, 2)
      .map(w => w[0]?.toUpperCase() ?? '')
      .join('');
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

  getRoleBadgeClass(user: any): string {
    if ((user.role ?? '').toUpperCase() === 'CLIENT') {
      if ((user.typeClient ?? '').toUpperCase() === 'GROSSISTE') return 'role-grossiste';
      return 'role-client';
    }
    return this.getRoleClass(user.role);
  }

  getRoleBadgeLabel(user: any): string {
    if ((user.role ?? '').toUpperCase() === 'CLIENT' && user.typeClient) {
      return user.typeClient;
    }
    return user.role || '—';
  }

  private resolveError(err: any): string {
    if (err.status === 515 || err.status === 500) return 'Erreur serveur — vérifiez l\'AuthAPI.';
    if (err.status === 403) return 'Accès refusé — rôle ADMIN requis.';
    if (err.status === 0)   return 'Passerelle inaccessible — vérifiez localhost:5555.';
    return err.error?.message ?? 'Erreur lors du chargement des utilisateurs.';
  }

  // ── Compteurs ─────────────────────────────────────────

  get totalActive():   number { return this.allUsers.filter(u => !u.isDeleted).length; }
  get totalDisabled(): number { return this.allUsers.filter(u =>  u.isDeleted).length; }

  countByRole(role: string): number {
    return this.allUsers.filter(u => (u.role ?? '').toUpperCase() === role).length;
  }

  // ── Navigation ────────────────────────────────────────

  onView(id: number):  void { this.router.navigate(['/users', id]); }
  onEdit(id: number):  void { this.router.navigate(['/users', id, 'edit']); }

  // ── Actions avec confirmation ─────────────────────────

  onDisable(user: any): void {
    this.confirmUser   = user;
    this.confirmAction = 'disable';
    this.showConfirmModal = true;
  }

  onEnable(user: any): void {
    this.confirmUser   = user;
    this.confirmAction = 'enable';
    this.showConfirmModal = true;
  }

  onConfirmAction(): void {
    if (!this.confirmUser || !this.confirmAction) return;
    this.isProcessing = true;

    const req$ = this.confirmAction === 'disable'
      ? this.userService.disableUser(this.confirmUser.email)
      : this.userService.enableUser(this.confirmUser.email);

    req$.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.isProcessing = false;
        this.closeModal();
        this.loadUsers();
      },
      error: (err: any) => {
        this.error        = this.resolveError(err);
        this.isProcessing = false;
        this.closeModal();
        this.cdr.markForCheck();
      }
    });
  }

  onCancelAction(): void { this.closeModal(); }

  private closeModal(): void {
    this.showConfirmModal = false;
    this.confirmAction    = '';
    this.confirmUser      = null;
  }
}
