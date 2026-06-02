import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule, FormsModule,
  FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors
} from '@angular/forms';

import { AuthService, User } from '../../core/services/auth.service';
import { ToastService } from '../../shared/services/toast.service';
import { RegionService } from '../field/regions/services/region.service';
import { catchError } from 'rxjs/operators';
import { of } from 'rxjs';

type Tab = 'profile' | 'password';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.css']
})
export class SettingsComponent implements OnInit {

  activeTab: Tab = 'profile';
  currentUser: User | null = null;
  regionName = '—';

  // ── Formulaire édition du profil ───────────────────────
  profileForm!: FormGroup;
  isEditingProfile = false;
  profileLoading = false;
  profileError = '';
  profileSuccess = false;

  // ── Formulaire changement de mot de passe ──────────────
  pwForm!: FormGroup;
  pwLoading  = false;
  pwError    = '';
  pwSuccess  = false;

  // ── Affichage des mots de passe ───────────────────────
  showCurrent = false;
  showNew     = false;
  showConfirm = false;

  constructor(
    private authService: AuthService,
    private fb: FormBuilder,
    private toast: ToastService,
    private regionSvc: RegionService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();

    const role = (this.currentUser?.role ?? '').toUpperCase();
    const uid  = this.currentUser?.id;

    if (uid && role === 'SUPERVISEUR') {
      this.regionSvc.getBySuperviseur(uid)
        .pipe(catchError(() => of(null)))
        .subscribe(region => {
          this.regionName = (region as any)?.NomRegion ?? region?.nomRegion ?? '—';
          this.cdr.markForCheck();
        });
    } else if (uid && role === 'DELEGUE') {
      this.regionSvc.getByDelegue(uid)
        .pipe(catchError(() => of([])))
        .subscribe(regions => {
          const r = regions[0] as any;
          this.regionName = r?.NomRegion ?? r?.nomRegion ?? '—';
          this.cdr.markForCheck();
        });
    }

    this.profileForm = this.fb.group({
      name:        ['', Validators.required],
      phoneNumber: [''],
      adresse:     ['']
    });

    this.pwForm = this.fb.group(
      {
        currentPassword: ['', [Validators.required, Validators.minLength(6)]],
        newPassword:      ['', [Validators.required, Validators.minLength(6), Validators.pattern(/(?=.*[^a-zA-Z0-9])/)]],
        confirmPassword:  ['', Validators.required]
      },
      { validators: this.passwordsMatchValidator }
    );
  }

  // Vérifie que newPassword === confirmPassword
  private passwordsMatchValidator(group: AbstractControl): ValidationErrors | null {
    const np = group.get('newPassword')?.value;
    const cp = group.get('confirmPassword')?.value;
    return np && cp && np !== cp ? { passwordsMismatch: true } : null;
  }

  setTab(tab: Tab): void {
    this.activeTab = tab;
    this.pwError   = '';
    this.pwSuccess = false;
    if (tab === 'profile') {
      this.isEditingProfile = false;
      this.profileError = '';
      this.profileSuccess = false;
    }
  }

  onEditProfile(): void {
    this.isEditingProfile = true;
    this.profileError = '';
    this.profileSuccess = false;
    this.profileForm.patchValue({
      name:        this.currentUser?.name        ?? '',
      phoneNumber: this.currentUser?.phoneNumber ?? '',
      adresse:     this.currentUser?.adresse     ?? ''
    });
  }

  onCancelEditProfile(): void {
    this.isEditingProfile = false;
    this.profileError = '';
    this.profileSuccess = false;
    this.profileForm.reset();
  }

  onSaveProfile(): void {
    if (this.profileForm.invalid) { this.profileForm.markAllAsTouched(); return; }
    const email = this.currentUser?.email;
    if (!email) {
      this.profileError = 'Utilisateur non identifié. Veuillez vous reconnecter.';
      return;
    }
    this.profileLoading = true;
    this.profileError = '';
    this.profileSuccess = false;
    const { name, phoneNumber, adresse } = this.profileForm.value;
    this.authService.updateProfile({ email, name, phoneNumber, adresse }).subscribe({
      next: () => {
        this.profileLoading = false;
        this.profileSuccess = true;
        this.authService.updateCurrentUser({ name, phoneNumber, adresse });
        this.currentUser = this.authService.getCurrentUser();
        this.toast.showSuccess('Profil mis à jour avec succès.');
        this.cdr.markForCheck();
        setTimeout(() => {
          this.isEditingProfile = false;
          this.profileSuccess = false;
          this.cdr.markForCheck();
        }, 1200);
      },
      error: (err: any) => {
        this.profileLoading = false;
        this.profileError = err?.error?.message ?? err?.error?.Message ?? 'Une erreur est survenue.';
        this.cdr.markForCheck();
      }
    });
  }

  isProfileFieldInvalid(field: string): boolean {
    const c = this.profileForm.get(field);
    return !!(c?.invalid && c?.touched);
  }

  isInvalid(field: string): boolean {
    const c = this.pwForm.get(field);
    return !!(c?.invalid && c?.touched);
  }

  onChangePassword(): void {
    if (this.pwForm.invalid) { this.pwForm.markAllAsTouched(); return; }

    const email = this.currentUser?.email;
    if (!email) {
      this.pwError = 'Utilisateur non identifié. Veuillez vous reconnecter.';
      return;
    }

    this.pwLoading = true;
    this.pwError   = '';
    this.pwSuccess = false;

    const { currentPassword, newPassword } = this.pwForm.value;

    this.authService.changePassword(email, currentPassword, newPassword).subscribe({
      next: () => {
        this.pwLoading = false;
        this.pwSuccess = true;
        this.pwForm.reset();
        this.toast.showSuccess('Mot de passe changé avec succès.');
        this.cdr.markForCheck();
      },
      error: (err: any) => {
        this.pwLoading = false;
        this.pwError   =
          err?.error?.message ??
          err?.error?.Message ??
          'Mot de passe actuel incorrect ou erreur serveur.';
        this.cdr.markForCheck();
      }
    });
  }

  getRoleLabel(role: string): string {
    const labels: Record<string, string> = {
      ADMIN:       'Administrateur',
      SUPERVISEUR: 'Superviseur',
      DELEGUE:     'Délégué médical',
      MEDECIN:     'Médecin',
      CLIENT:      'Client',
    };
    return labels[role] ?? role;
  }

  getRoleCss(role: string): string {
    const map: Record<string, string> = {
      ADMIN:       'role-admin',
      SUPERVISEUR: 'role-superviseur',
      DELEGUE:     'role-delegue',
      MEDECIN:     'role-medecin',
      CLIENT:      'role-client',
    };
    return map[role] ?? 'role-default';
  }

  getInitials(name: string): string {
    if (!name) return '?';
    return name.split(' ').slice(0, 2).map(w => w[0]?.toUpperCase() ?? '').join('');
  }
}
