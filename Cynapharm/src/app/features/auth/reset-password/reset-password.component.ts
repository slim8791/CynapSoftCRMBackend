import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { passwordMatchValidator } from '../../../core/validators/password-match.validator';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterLink,
    CardComponent,
    ButtonComponent
  ],
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.css']
})
export class ResetPasswordComponent implements OnInit {

  resetForm: FormGroup;
  loading = false;
  message = '';
  error = '';
  success = false;
  email = '';
  token = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.resetForm = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required, Validators.minLength(6)]]
    }, { validators: passwordMatchValidator('newPassword', 'confirmPassword') });
  }

  ngOnInit(): void {
    // Get email and token from query params
    this.email = this.route.snapshot.queryParamMap.get('email') || '';
    this.token = this.route.snapshot.queryParamMap.get('token') || '';

    if (!this.email || !this.token) {
      this.error = 'Lien de réinitialisation invalide.';
    }
  }

  onSubmit(): void {
    if (this.resetForm.invalid) return;

    const { newPassword } = this.resetForm.value;

    this.loading = true;
    this.error = '';
    this.message = '';

    this.authService.resetPassword(this.email, this.token, newPassword).subscribe({
      next: (response) => {
        this.loading = false;
        if (response.IsSuccess) {
          this.success = true;
          this.message = response.Message || 'Mot de passe réinitialisé avec succès.';
        } else {
          this.error = response.Message || 'Erreur lors de la réinitialisation.';
        }
      },
      error: (err) => {
        this.loading = false;
        this.error = err.error?.Message || 'Erreur lors de la réinitialisation.';
      }
    });
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }
}
