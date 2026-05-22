import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterLink,
    CardComponent,
    ButtonComponent
  ],
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css']
})
export class ForgotPasswordComponent {

  forgotForm: FormGroup;
  loading = false;
  message = '';
  error = '';
  success = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.forgotForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  onSubmit(): void {
    if (this.forgotForm.invalid) return;

    this.loading = true;
    this.error = '';
    this.message = '';

    const { email } = this.forgotForm.value;

    this.authService.forgotPassword(email).subscribe({
      next: (response: any) => {
        this.loading = false;
        this.success = true;
        this.message = response?.message ?? 'Un e-mail de réinitialisation a été envoyé.';
      },
      error: (err: any) => {
        this.loading = false;
        this.error = err?.error?.message ?? 'Une erreur est survenue. Veuillez réessayer.';
      }
    });
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }
}
