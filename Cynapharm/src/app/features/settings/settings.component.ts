import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule, FormsModule,
  FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors
} from '@angular/forms';

import { AuthService, User } from '../../core/services/auth.service';
import { ToastService } from '../../shared/services/toast.service';

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
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();

    this.pwForm = this.fb.group(
      {
        currentPassword: ['', [Validators.required, Validators.minLength(6)]],
        newPassword:      ['', [Validators.required, Validators.minLength(6)]],
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
