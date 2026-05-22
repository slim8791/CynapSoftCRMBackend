import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { passwordMatchValidator } from '../../../core/validators/password-match.validator';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterLink,
  ],
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.css']
})
export class ResetPasswordComponent implements OnInit, OnDestroy {

  resetForm: FormGroup;
  loading = false;
  message = '';
  error = '';
  success = false;
  email = '';
  token = '';
  showNewPassword     = false;
  showConfirmPassword = false;

  private redirectTimeout: any;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef,
  ) {
    this.resetForm = this.fb.group({
      newPassword: ['', [
        Validators.required,
        Validators.minLength(6),
        Validators.pattern(/(?=.*[0-9])(?=.*[A-Z])(?=.*[a-z])/)
      ]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: passwordMatchValidator('newPassword', 'confirmPassword') });
  }

  ngOnInit(): void {
    const urlParams = new URLSearchParams(window.location.search);
    this.token = urlParams.get('token') ?? '';
    this.email = urlParams.get('email') ?? '';

    if (!this.email || !this.token) {
      this.error = 'Lien de réinitialisation invalide.';
    }
  }

  onSubmit(): void {
    if (this.resetForm.invalid) return;

    const { newPassword } = this.resetForm.value;
    this.loading = true;
    this.error   = '';
    this.message = '';

    this.authService.resetPassword(this.email, this.token, newPassword).subscribe({
      next: (res: any) => {
        this.loading = false;
        if (res?.isSuccess === true || res?.IsSuccess === true) {
          this.success = true;
          this.message = res?.message ?? res?.Message ?? 'Mot de passe réinitialisé avec succès.';
          this.error   = '';
          this.redirectTimeout = setTimeout(() => this.router.navigate(['/login']), 3000);
        } else {
          // 200 OK but isSuccess = false — show the backend error message
          this.error = res?.message ?? res?.Message ?? 'Une erreur est survenue.';
        }
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.loading = false;
        this.error   = err?.error?.message ?? err?.error?.Message ?? 'Une erreur est survenue.';
        this.cdr.detectChanges();
      }
    });
  }

  get passwordErrors(): string[] {
    const ctrl = this.resetForm.get('newPassword');
    if (!ctrl || !ctrl.touched || !ctrl.errors) return [];
    const msgs: string[] = [];
    if (ctrl.errors['required'])   msgs.push('Le mot de passe est requis.');
    if (ctrl.errors['minlength'])  msgs.push('Minimum 6 caractères.');
    if (ctrl.errors['pattern'])    msgs.push('Doit contenir une majuscule, une minuscule et un chiffre.');
    return msgs;
  }

  get mismatchError(): boolean {
    const ctrl = this.resetForm.get('confirmPassword');
    return !!(ctrl?.touched && this.resetForm.errors?.['passwordMismatch']);
  }

  ngOnDestroy(): void {
    clearTimeout(this.redirectTimeout);
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }
}
