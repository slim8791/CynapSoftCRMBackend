import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { UserService } from '../user.service';
import { CardComponent } from '../../shared/components/card/card.component';
import { ButtonComponent } from '../../shared/components/button/button.component';

@Component({
  selector: 'app-user-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, CardComponent, ButtonComponent],
  templateUrl: './user-detail.component.html',
  styleUrl: './user-detail.component.css'
})
export class UserDetailComponent implements OnInit {
  user: any;
  loading: boolean = false;
  error: string = '';
  private userId: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private userService: UserService
  ) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.userId = params['id'];
      this.loadUserDetails();
    });
  }

  private loadUserDetails(): void {
    this.loading = true;
    this.error = '';

    this.userService.getUserById(this.userId).subscribe({
      next: (data) => {
        this.user = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load user details';
        this.loading = false;
        console.error(err);
      }
    });
  }

  onEdit(): void {
    this.router.navigate(['/users', this.userId, 'edit']);
  }

  onDelete(): void {
    if (confirm('Are you sure you want to delete this user?')) {
      this.userService.deleteUser(this.userId).subscribe({
        next: () => {
          this.router.navigate(['/users']);
        },
        error: (err) => {
          console.error('Error deleting user:', err);
        }
      });
    }
  }
}
