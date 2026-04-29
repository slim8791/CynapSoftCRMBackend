import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink, Router } from '@angular/router';
import { UserService } from '../user.service';
import { CardComponent } from '../../shared/components/card/card.component';
import { ButtonComponent } from '../../shared/components/button/button.component';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink, CardComponent, ButtonComponent],
  templateUrl: './user-form.component.html',
  styleUrl: './user-form.component.css'
})
export class UserFormComponent implements OnInit {
  userForm: FormGroup;
  isEditMode: boolean = false;
  loading: boolean = false;
  error: string = '';
  success: boolean = false;
  private userId: string = '';

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private userService: UserService
  ) {
    this.userForm = this.fb.group({
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      role: ['', [Validators.required]],
      password: ['', [Validators.required]]
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      if (params['id']) {
        this.isEditMode = true;
        this.userId = params['id'];
        this.userForm.get('password')?.clearValidators();
        this.userForm.get('password')?.updateValueAndValidity();
        this.loadUserData();
      }
    });
  }

  private loadUserData(): void {
    this.userService.getUserById(this.userId).subscribe({
      next: (data) => {
        this.userForm.patchValue(data);
      },
      error: (err) => {
        this.error = 'Failed to load user data';
        console.error(err);
      }
    });
  }

  onSubmit(): void {
    if (!this.userForm.valid) return;

    this.loading = true;
    this.error = '';
    this.success = false;

    const formData = this.userForm.value;

    const request = this.isEditMode
      ? this.userService.updateUser(this.userId, formData)
      : this.userService.createUser(formData);

    request.subscribe({
      next: () => {
        this.success = true;
        this.loading = false;
        setTimeout(() => {
          this.router.navigate(['/users']);
        }, 1500);
      },
      error: (err) => {
        this.error = `Failed to ${this.isEditMode ? 'update' : 'create'} user`;
        this.loading = false;
        console.error(err);
      }
    });
  }
}
