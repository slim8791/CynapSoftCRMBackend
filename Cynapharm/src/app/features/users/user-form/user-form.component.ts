import { Component, OnInit, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { UserService } from '../user.service';
import { UserRole, UserType } from '../../../core/services/auth.service';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterLink,
    CardComponent,
    ButtonComponent
  ],
  templateUrl: './user-form.component.html',
  styleUrls: ['./user-form.component.css']
})
export class UserFormComponent implements OnInit, AfterViewInit {

  userForm: FormGroup;
  isEditMode = false;
  loading = false;
  error = '';
  success = false;

  // ✅ MODIF : backend enums
  roles = Object.values(UserRole).filter(r => isNaN(Number(r))); // ✅ Filtrer les indices numériques
  userTypes = Object.values(UserType).filter(r => isNaN(Number(r))); // ✅ Filtrer les indices numériques

  private userId!: number;
  private userEmail!: string; // ✅ backend utilise EMAIL

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private userService: UserService,
    private cdr: ChangeDetectorRef
  ) {
    // ✅ MODIF : formulaire ALIGNÉ backend
    this.userForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: [''], // ✅ Ajouter phoneNumber
      adresse: ['', Validators.required],
      role: ['', Validators.required],
      userType: [UserType.PHARMACIEN],
      password: [''] // ✅ requis UNIQUEMENT en création
    });
  }

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');

    if (idParam) {
      this.isEditMode = true;
      this.userId = Number(idParam);

      // ❌ MODIF : password inutile en édition
      this.userForm.get('password')?.clearValidators();
      this.userForm.get('password')?.updateValueAndValidity();

      this.loadUser();
    } else {
      // ✅ création → mot de passe obligatoire
      this.userForm.get('password')?.setValidators([Validators.required, Validators.minLength(6)]);
      this.userForm.get('password')?.updateValueAndValidity();
    }
  }

  /**
   * ✅ MODIF : récupération directe via getUserById (plus efficace)
   */
  private loadUser(): void {
    this.loading = true;

    this.userService.getUserById(this.userId).subscribe({
      next: (response: any) => {
        const user = response?.Result ?? response?.result ?? response;

        if (!user) {
          this.error = 'Utilisateur introuvable.';
          this.loading = false;
          this.cdr.detectChanges();
          return;
        }

        this.userEmail = user.email ?? user.Email ?? '';

        this.userForm.patchValue({
          name:        user.name        ?? user.Name        ?? '',
          email:       user.email       ?? user.Email       ?? '',
          phoneNumber: user.phoneNumber ?? user.PhoneNumber ?? '',
          adresse:     user.adresse     ?? user.Adresse     ?? '',
          role:        user.role        ?? user.Role        ?? '',
        });

        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.loading = false;
        this.error = err?.error?.message ?? 'Impossible de charger l\'utilisateur.';
        this.cdr.detectChanges();
      }
    });
  }

  ngAfterViewInit(): void {
    console.log('🔍 USER FORM LOADED:', {
      valid: this.userForm.valid,
      value: this.userForm.value,
      status: this.userForm.status,
      passwordValidators: this.userForm.get('password')?.hasValidator(Validators.required)
    });
  }

  onSubmit(): void {
    console.log('🚀 USER FORM SUBMIT:', {
      value: this.userForm.value,
      valid: this.userForm.valid,
      touched: this.userForm.touched
    });
    if (this.userForm.invalid) {
      console.log('❌ FORM INVALID. Errors:', Object.keys(this.userForm.controls).reduce((acc, k) => {
        const ctrl = this.userForm.get(k);
        if (ctrl?.invalid) acc[k] = ctrl.errors;
        return acc;
      }, {} as any));

      // Mark all touched to show errors
      Object.keys(this.userForm.controls).forEach(key => this.userForm.get(key)?.markAsTouched());
      return;
    }

    this.loading = true;
    this.error = '';
    this.success = false;

    const form = this.userForm.value;
    console.log('📤 SENDING:', form);

    if (this.isEditMode) {
      this.userService.changeRole({
        email: this.userEmail,
        newRole: form.role
      }).subscribe({
        next: () => this.onSuccess(),
        error: err => this.onError(err)
      });
    } else {
      const payload = {
        email: form.email,
        name: form.name,
        password: form.password,
        phoneNumber: form.phoneNumber,
        adresse: form.adresse,
        role: form.role,
        userType: form.userType
      };
      console.log('📤 REGISTER PAYLOAD:', payload);
      this.userService.registerUser(payload).subscribe({
        next: () => this.onSuccess(),
        error: err => this.onError(err)
      });
    }
  }

  private onSuccess(): void {
    this.success = true;
    this.loading = false;

    setTimeout(() => {
      this.router.navigate(['/users']);
    }, 1200);
  }

  private onError(err: any): void {
    this.loading = false;
    this.error = 'Operation failed';
    console.error(err);
  }
}