import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService, UserRole } from '../../../core/services/auth.service'; // ✅ MODIF : import UserRole
import { CardComponent } from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterLink,
    CardComponent,
    ButtonComponent
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {

  loginForm: FormGroup;
  loading = false;
  error = '';

  // ✅ MODIF : fallback si pas de returnUrl
  private defaultRoute = '/dashboard';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onLogin(): void {
    if (this.loginForm.invalid) return;

    this.loading = true;
    this.error = '';

    const { email, password } = this.loginForm.value;

    this.authService.login(email, password).subscribe({
      next: () => {
        this.loading = false;

        // ✅ MODIF : récupération user + rôle
        const user = this.authService.getCurrentUser();
        const returnUrl =
          this.route.snapshot.queryParamMap.get('returnUrl') ||
          this.getRedirectByRole(user?.role);

        this.router.navigateByUrl(returnUrl);
      },
      error: () => {
        this.loading = false;
        this.error = 'Email ou mot de passe incorrect.';
      }
    });
  }

  /**
   * ✅ MODIF : redirection selon rôle backend
   */
  private getRedirectByRole(role?: UserRole): string {
    switch (role) {
      case UserRole.ADMIN:
        return '/users';

      case UserRole.SUPERVISEUR:
        return '/dashboard';

      case UserRole.DELEGUE:
        return '/dashboard';

      case UserRole.MEDECIN:
        return '/dashboard';

      case UserRole.CLIENT:
        return '/home';

      default:
        return this.defaultRoute;
    }
  }
}