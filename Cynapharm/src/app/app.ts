import { Component, signal, inject, HostListener, ElementRef } from '@angular/core';
import { Router, RouterOutlet, RouterLink, RouterLinkActive, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService, User, UserRole } from './core/services/auth.service';
import { filter } from 'rxjs/operators';
import { ToastComponent } from './shared/components/toast/toast.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    ToastComponent
  ],
  templateUrl: './app.html',
  styleUrls: ['./app.scss']
})
export class App {
  protected readonly title = signal('Cynapharm');
  protected showNavbar = signal(false);
  showUserMenu = false;

  private router = inject(Router);
  private authService = inject(AuthService);
  private el = inject(ElementRef);

  get currentUser(): User | null {
    return this.authService.getCurrentUser();
  }

  get userInitials(): string {
    const name = this.currentUser?.name;
    if (!name) return '??';
    return name.substring(0, 2).toUpperCase();
  }

  get isAdmin(): boolean {
    return this.authService.getUserRole() === UserRole.ADMIN;
  }

  get isSuperviseur(): boolean {
    return this.authService.getUserRole() === UserRole.SUPERVISEUR;
  }

  get isDelegue(): boolean {
    return this.authService.getUserRole() === UserRole.DELEGUE;
  }

  constructor() {
    this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe((e: NavigationEnd) => {
        const url = e.urlAfterRedirects;
        const hide = url.startsWith('/login') || url.startsWith('/register');
        this.showNavbar.set(!hide && this.authService.isAuthenticated());
      });
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(e: Event): void {
    if (!this.el.nativeElement.contains(e.target)) {
      this.showUserMenu = false;
    }
  }

  toggleUserMenu(): void {
    this.showUserMenu = !this.showUserMenu;
  }

  closeUserMenu(): void {
    this.showUserMenu = false;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}