import { Component, signal, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class App {
  protected readonly title = signal('Cynapharm');
  protected authService = inject(AuthService);
  protected router = inject(Router);

  // ✅ AJOUT : décider quand afficher le menu
  showNavbar(): boolean {
    const url = this.router.url;

    // ❌ NE PAS afficher le menu sur login / register
    if (url.startsWith('/login') || url.startsWith('/register')) {
      return false;
    }

    // ✅ afficher le menu seulement si connecté
    return this.authService.isAuthenticated();
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}


