import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { UserService } from '../user.service';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';

@Component({
  selector: 'app-user-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, CardComponent, ButtonComponent],
  templateUrl: './user-detail.component.html',
  styleUrls: ['./user-detail.component.css']
})
export class UserDetailComponent implements OnInit {

  user: any = null;
  loading = false;
  error = '';

  private userId!: number; // ✅ MODIF : ID numérique

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private userService: UserService
  ) {}

  ngOnInit(): void {
    this.userId = Number(this.route.snapshot.paramMap.get('id')); // ✅ MODIF
    this.loadUserDetails();
  }

  /**
   * ✅ MODIF MAJEURE
   * Backend ne fournit PAS getUserById
   * → on filtre depuis la liste
   */
  private loadUserDetails(): void {
    this.loading = true;
    this.error = '';

    this.userService.getUsers().subscribe({
      next: (users) => {
        this.user = users.find(u => u.id === this.userId);
        this.loading = false;

        if (!this.user) {
          this.error = 'User not found';
        }
      },
      error: (err) => {
        this.loading = false;
        this.error = 'Failed to load user details';
        console.error(err);
      }
    });
  }

  onEdit(): void {
    this.router.navigate(['/users', this.userId, 'edit']);
  }

  /**
   * ✅ MODIF : suppression via EMAIL (backend)
   */
  onDelete(): void {
    if (!this.user?.email) return;

    if (confirm('Are you sure you want to disable this user?')) {
      this.userService.disableUser(this.user.email).subscribe({
        next: () => {
          this.router.navigate(['/users']);
        },
        error: (err) => {
          console.error('Error disabling user:', err);
        }
      });
    }
  }

  /**
   * ✅ BONUS : réactiver utilisateur
   */
  onEnable(): void {
    if (!this.user?.email) return;

    this.userService.enableUser(this.user.email).subscribe({
      next: () => this.loadUserDetails(),
      error: err => console.error(err)
    });
  }
}