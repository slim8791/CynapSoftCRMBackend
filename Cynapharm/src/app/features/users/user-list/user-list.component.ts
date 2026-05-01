import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { UserService } from '../user.service';
import { TableComponent } from '../../../shared/components/table/table.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { CardComponent } from '../../../shared/components/card/card.component';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    TableComponent,
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

  // ✅ MODIF : colonnes ALIGNÉES backend
  columns = ['id', 'name', 'email', 'role', 'isDeleted'];

  constructor(
    private userService: UserService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  private loadUsers(): void {
    this.loading = true;
    this.error = '';

    this.userService.getUsers().subscribe({
      next: users => {
        this.users = users;
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.error = 'Failed to load users';
        console.error(err);
      }
    });
  }

  // ✅ MODIF : suppression = DISABLE via email
  onDelete(user: any): void {
    if (!user?.email) return;

    if (confirm(`Disable user ${user.email}?`)) {
      this.userService.disableUser(user.email).subscribe({
        next: () => this.loadUsers(),
        error: err => console.error(err)
      });
    }
  }

  onEdit(id: number): void {
    this.router.navigate(['/users', id, 'edit']);
  }
}
