import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { UserService, UserDto } from '../user.service';
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
  loading = true;  // ✅ MODIF : commence à true pour afficher "Loading..." au départ
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
   * Direct API call to getUserById
   */
  private loadUserDetails(): void {
    this.loading = true;
    this.error = '';

    this.userService.getUserById(this.userId).subscribe({
      next: (response) => {
        console.log('✅ User response:', response);
        const userData = response.result || response;
        
        if (!userData) {
          this.error = 'No user data received from server';
          this.loading = false;
          console.error('❌ No userData:', response);
          return;
        }
        
        // ✅ Normaliser les données du backend (camelCase → PascalCase)
        this.user = {
          ...userData,
          Id: userData.id ?? userData.Id,
          Name: userData.name ?? userData.Name,
          Email: userData.email ?? userData.Email,
          PhoneNumber: userData.phoneNumber ?? userData.PhoneNumber,
          Adresse: userData.adresse ?? userData.Adresse,
          Role: userData.role ?? userData.Role,
          IsDeleted: userData.isDeleted ?? userData.IsDeleted ?? false
        };
        
        console.log('✅ Normalized user:', this.user);
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        console.error('❌ API Error:', err);
        
        // Meilleur message d'erreur
        if (err.status === 401) {
          this.error = 'Unauthorized - Please login again';
        } else if (err.status === 403) {
          this.error = 'Access Denied - Admin role required';
        } else if (err.status === 404) {
          this.error = 'User not found';
        } else if (err.status === 0) {
          this.error = 'Server not reachable - Check if gateway is running on localhost:5555';
        } else {
          this.error = err.error?.message || `Failed to load user (Status: ${err.status})`;
        }
      }
    });
  }

  onEdit(): void {
    this.router.navigate(['/users', this.userId, 'edit']);
  }

  /**
   * Disable via EMAIL
   */
  onDelete(): void {
    const email = this.user?.Email || this.user?.email;
    if (!email) return;

    if (confirm('Are you sure you want to disable this user?')) {
      this.userService.disableUser(email).subscribe({
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
   * Enable user
   */
  onEnable(): void {
    const email = this.user?.Email || this.user?.email;
    if (!email) return;

    this.userService.enableUser(email).subscribe({
      next: () => this.loadUserDetails(),
      error: err => console.error(err)
    });
  }
}
