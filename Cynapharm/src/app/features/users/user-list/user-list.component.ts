import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { UserService } from '../user.service';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { CardComponent } from '../../../shared/components/card/card.component';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ButtonComponent,
    CardComponent
  ],
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.css']
})
export class UserListComponent implements OnInit {

  users: any[] = [];
  loading = false;
  error = '';

  columns = [
    { key: 'id', label: 'ID' },
    { key: 'name', label: 'Name' },
    { key: 'email', label: 'Email' },
    { key: 'role', label: 'Role' },
    { key: 'isDeleted', label: 'Status' }
  ];

  constructor(
    private userService: UserService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  getStatusText(isDeleted: boolean): string {
    return isDeleted ? 'Disabled' : 'Active';
  }

  getStatusClass(isDeleted: boolean): string {
    return isDeleted ? 'status-disabled' : 'status-active';
  }

  getValue(user: any, key: string): any {
    if (key === 'isDeleted') {
      return this.getStatusText(user.isDeleted);
    }
    return user[key] || '';
  }

  private loadUsers(): void {
    this.loading = true;
    this.error = '';

    this.userService.getUsers().subscribe({
      next: (response: any) => {
        console.log('Users API response:', response);
        let usersData = [];
        if (Array.isArray(response)) {
          usersData = response;
        } else if (response && (response.result || response.Result)) {
          usersData = response.result || response.Result;
        }
        console.log('Parsed users:', usersData);
        this.users = usersData.map((u: any) => this.normalizeUser(u));
        console.log('Normalized users:', this.users);
        this.loading = false;
      },
      error: (err: any) => {
        console.error('Users load error:', err);
        this.loading = false;
        let errorMsg = 'Failed to load users';
        if (err.status === 515) {
          errorMsg = 'Backend error (515) - Service crash. Check AuthAPI logs/DB.';
        } else if (err.status === 403) {
          errorMsg = 'Access denied - ADMIN role required.';
        } else if (err.status === 0) {
          errorMsg = 'Gateway not running - check localhost:5555';
        }
        this.error = errorMsg;
      }
    });
  }

  private normalizeUser(user: any): any {
    return {
      ...user,
      id: user.id ?? user.Id ?? user.userId,
      name: user.name ?? user.Name ?? user.nom,
      email: user.email ?? user.Email,
      role: user.role ?? user.Role,
      isDeleted: user.isDeleted ?? user.IsDeleted ?? false
    };
  }

  onDelete(user: any): void {
    if (!user?.email) return;

    if (confirm(`Disable user ${user.email}?`)) {
      this.userService.disableUser(user.email).subscribe({
        next: () => this.loadUsers(),
        error: err => console.error(err)
      });
    }
  }

  onView(id: number): void {
    this.router.navigate(['/users', id]);
  }

  onEdit(id: number): void {
    this.router.navigate(['/users', id, 'edit']);
  }

  onEnable(user: any): void {
    if (confirm(`Enable user ${user.email}?`)) {
      this.userService.enableUser(user.email).subscribe({
        next: () => this.loadUsers(),
        error: err => console.error(err)
      });
    }
  }
}
