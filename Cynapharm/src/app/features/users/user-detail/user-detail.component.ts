import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { UserService } from '../user.service';
import { ToastService } from '../../../shared/services/toast.service';
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
    private toastService: ToastService,
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
          this.cdr.markForCheck();
        },
        error: (err: any) => {
          this.error   = this.resolveError(err);
          this.loading = false;
          this.cdr.markForCheck();
        }
      });
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
