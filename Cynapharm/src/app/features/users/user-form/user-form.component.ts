import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { UserService } from '../user.service';
import { UserRole, UserType } from '../../../core/services/auth.service'; // ✅ MODIF
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
export class UserFormComponent implements OnInit {

  userForm: FormGroup;
  isEditMode = false;
  loading = false;
  error = '';
  success = false;

  // ✅ MODIF : backend enums
  roles = Object.values(UserRole);
  userTypes = Object.values(UserType);

  private userId!: number;
  private userEmail!: string; // ✅ backend utilise EMAIL

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private userService: UserService
  ) {
    // ✅ MODIF : formulaire ALIGNÉ backend
    this.userForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
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
    }
  }

  /**
   * ✅ MODIF : récupération via getUsers + filter
   */
  private loadUser(): void {
    this.loading = true;

    this.userService.getUsers().subscribe({
      next: users => {
        const user = users.find(u => u.id === this.userId);

        if (!user) {
          this.error = 'User not found';
          this.loading = false;
          return;
        }

        this.userEmail = user.email;

        this.userForm.patchValue({
          name: user.name,
          email: user.email,
          adresse: user.adresse,
          role: user.role
        });

        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.error = 'Failed to load user';
        console.error(err);
      }
    });
  }

  onSubmit(): void {
    if (this.userForm.invalid) return;

    this.loading = true;
    this.error = '';
    this.success = false;

    const form = this.userForm.value;

    if (this.isEditMode) {
      // ✅ MODIF : changement de rôle UNIQUEMENT
      this.userService.changeRole({
        email: this.userEmail,
        newRole: form.role
      }).subscribe({
        next: () => this.onSuccess(),
        error: err => this.onError(err)
      });

    } else {
      // ✅ MODIF : création via REGISTER
      const payload = {
        email: form.email,
        name: form.name,
        password: form.password,
        adresse: form.adresse,
        role: form.role,
        userType: form.userType
      };

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