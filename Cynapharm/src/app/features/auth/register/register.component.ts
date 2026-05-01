import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService, UserRole, UserType } from '../../../core/services/auth.service'; // ✅ MODIF
import { CardComponent } from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterLink,
    CardComponent,
    ButtonComponent
  ],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {

  registerForm: FormGroup;
  loading = false;
  error = '';
  success = false;

  // ✅ MODIF : enums exposés au template
  roles = Object.values(UserRole);
  userTypes = Object.values(UserType);

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    // ✅ MODIF : formulaire ALIGNÉ BACKEND
    this.registerForm = this.fb.group({
      name: ['', Validators.required],          // ✅ Name (pas first/last)
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      adresse: ['', Validators.required],

      role: [UserRole.CLIENT, Validators.required], // ✅ OBLIGATOIRE
      userType: [UserType.PHARMACIEN],              // ✅ requis pour CLIENT

      // ✅ Champs conditionnels
      nomOfficine: [''],
      typePharmacie: [''],
      raisonSociale: ['']
    });
  }

  onRegister(): void {
    if (this.registerForm.invalid) return;

    this.loading = true;
    this.error = '';
    this.success = false;

    const form = this.registerForm.value;

    // ✅ MODIF : construction DTO EXACT backend
    const payload = {
      email: form.email,
      name: form.name,
      password: form.password,
      adresse: form.adresse,
      role: form.role,
      userType: form.userType,
      nomOfficine: form.nomOfficine,
      typePharmacie: form.typePharmacie,
      raisonSociale: form.raisonSociale
    };

    this.authService.register(payload).subscribe({
      next: () => {
        this.loading = false;
        this.success = true;

        // ✅ MODIF : pas de login auto (backend ne renvoie pas de token)
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 1200);
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message || 'Registration failed.';
        console.error(err);
      }
    });
  }
}