import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { UserService } from '../user.service';
import { TableComponent } from '../../shared/components/table/table.component';
import { ButtonComponent } from '../../shared/components/button/button.component';
import { CardComponent } from '../../shared/components/card/card.component';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, RouterLink, TableComponent, ButtonComponent, CardComponent],
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.css'
})
export class UserListComponent implements OnInit {
  users: any[] = [];
  loading: boolean = false;
  error: string = '';
  columns = ['id', 'firstName', 'lastName', 'email', 'role'];

  constructor(private userService: UserService, private router: Router) { }

  ngOnInit(): void {
    this.loadUsers();
  }

  private loadUsers(): void {
    this.loading = true;
    this.error = '';

    this.userService.getUsers().subscribe({
      next: (data) => {
        this.users = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load users';
        this.loading = false;
        console.error(err);
      }
    });
  }

  onView(id: string): void {
    console.log('View user:', id);
  }

  onEdit(id: string): void {
    this.router.navigate(['/users', id, 'edit']);
  }

  onDelete(id: string): void {
    if (confirm('Are you sure you want to delete this user?')) {
      this.userService.deleteUser(id).subscribe({
        next: () => {
          this.loadUsers();
        },
        error: (err) => {
          console.error('Error deleting user:', err);
        }
      });
    }
  }
}
