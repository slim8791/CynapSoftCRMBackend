import { Component, OnInit, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { UserService } from '../user.service';
import { AuthService, UserRole, UserType } from '../../../core/services/auth.service';
import { RegionService, RegionDto } from '../../field/regions/services/region.service';
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

  regions:        RegionDto[] = [];
  selectedRegion: RegionDto | null = null;

  get availableRoles(): string[] {
    const role = this.authSvc.getUserRole()?.toUpperCase();
    if (role === 'SUPERVISEUR') return ['DELEGUE', 'CLIENT'];
    return Object.values(UserRole).filter(r => isNaN(Number(r))) as string[];
  }
  roles = Object.values(UserRole).filter(r => isNaN(Number(r)));
  userTypes = Object.values(UserType).filter(r => isNaN(Number(r)));

  get isAdmin(): boolean {
    return this.authSvc.getUserRole()?.toUpperCase() === 'ADMIN';
  }

  get isSuperviseur(): boolean {
    return this.authSvc.getUserRole()?.toUpperCase() === 'SUPERVISEUR';
  }

  get showRegionDropdown(): boolean {
    const selectedRole = this.userForm.get('role')?.value;
    return selectedRole === 'DELEGUE' || selectedRole === 'SUPERVISEUR';
  }

  private userId!: number;
  private userEmail!: string;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private userService: UserService,
    private authSvc: AuthService,
    private regionSvc: RegionService,
    private cdr: ChangeDetectorRef
  ) {
    this.userForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: [''],
      adresse: ['', Validators.required],
      role: ['', Validators.required],
      userType: [UserType.PHARMACIEN],
      password: [''],
      idRegion: [null]
    });
  }

  // FieldAPI returns PascalCase keys (Id_Region, NomRegion, …).
  // Normalize to the RegionDto interface casing so the rest of the component
  // can use id_Region / nomRegion without worrying about the API casing.
  private normalizeRegion(r: any): RegionDto | null {
    if (!r) return null;
    return {
      id_Region:      r.Id_Region      ?? r.id_Region,
      nomRegion:      r.NomRegion      ?? r.nomRegion      ?? '',
      codePostal:     r.CodePostal     ?? r.codePostal     ?? '',
      id_Superviseur: r.Id_Superviseur ?? r.id_Superviseur,
    };
  }

  ngOnInit(): void {
    // Load all regions for ADMIN dropdown (normalize PascalCase → camelCase)
    this.regionSvc.getAll()
      .pipe(catchError(() => of([])))
      .subscribe(r => {
        this.regions = r.map(region => this.normalizeRegion(region) as RegionDto);
        this.cdr.markForCheck();
      });

    // For SUPERVISEUR: auto-load his region before the form is used
    if (this.isSuperviseur) {
      const userId = this.authSvc.getUserId();
      this.regionSvc.getBySuperviseur(userId)
        .pipe(catchError(() => of(null)))
        .subscribe(region => {
          // getBySuperviseur already unwraps the array; guard against raw array just in case
          const raw = Array.isArray(region) ? (region as any)[0] : region;
          this.selectedRegion = this.normalizeRegion(raw);
          console.log('[FORM] Superviseur region:', this.selectedRegion);
          if (this.selectedRegion?.id_Region) {
            this.userForm.get('idRegion')?.setValue(this.selectedRegion.id_Region);
          }
          this.cdr.markForCheck();
        });
    }

    const idParam = this.route.snapshot.paramMap.get('id');

    if (idParam) {
      this.isEditMode = true;
      this.userId = Number(idParam);

      this.userForm.get('password')?.clearValidators();
      this.userForm.get('password')?.updateValueAndValidity();

      this.loadUser();
    } else {
      this.userForm.get('password')?.setValidators([Validators.required, Validators.minLength(6), Validators.pattern(/(?=.*[^a-zA-Z0-9])/)]);
      this.userForm.get('password')?.updateValueAndValidity();
    }
  }

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
          idRegion:    user.idRegion    ?? user.IdRegion    ?? null,
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
    if (this.userForm.invalid) {
      console.log('❌ FORM INVALID. Errors:', Object.keys(this.userForm.controls).reduce((acc, k) => {
        const ctrl = this.userForm.get(k);
        if (ctrl?.invalid) acc[k] = ctrl.errors;
        return acc;
      }, {} as any));
      Object.keys(this.userForm.controls).forEach(key => this.userForm.get(key)?.markAsTouched());
      return;
    }

    this.loading = true;
    this.error = '';
    this.success = false;

    const form = this.userForm.value;

    if (this.isEditMode) {
      const idRegion: number | null = form.idRegion ?? null;
      const role = (form.role ?? '').toUpperCase();

      this.userService.changeRole({ email: this.userEmail, newRole: form.role }).subscribe({
        next: () => {
          // Update idRegion for DELEGUE/SUPERVISEUR if set
          if ((role === 'DELEGUE' || role === 'SUPERVISEUR') && idRegion != null) {
            this.userService.updateProfile({ email: this.userEmail, idRegion })
              .pipe(catchError(() => of(null)))
              .subscribe();
          }
          this.onSuccess();
        },
        error: err => this.onError(err)
      });
    } else {
      // For SUPERVISEUR, use the pre-loaded selectedRegion (already normalized)
      const idRegion = this.isSuperviseur
        ? (this.selectedRegion?.id_Region ?? null)
        : (form.idRegion ?? null);

      const payload = {
        email:       form.email,
        name:        form.name,
        password:    form.password,
        phoneNumber: form.phoneNumber,
        adresse:     form.adresse,
        role:        form.role,
        userType:    form.userType,
        idRegion
      };
      console.log('[CREATE USER] payload:', JSON.stringify(payload));
      this.userService.registerUser(payload).subscribe({
        next: () => this.onSuccess(),
        error: err => this.onError(err)
      });
    }
  }

  private onSuccess(): void {
    this.success = true;
    this.loading = false;
    setTimeout(() => this.router.navigate(['/users']), 1200);
  }

  private onError(err: any): void {
    this.loading = false;
    this.error = err?.error?.message ?? err?.error?.Message ?? err?.message ?? 'Une erreur est survenue.';
    console.error(err);
    this.cdr.detectChanges();
  }
}
