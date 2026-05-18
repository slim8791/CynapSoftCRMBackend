import { inject } from '@angular/core';
import { CanActivateFn, ActivatedRouteSnapshot, Router } from '@angular/router';
import { AuthService, UserRole } from '../services/auth.service';

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {

  const authService = inject(AuthService);
  const router = inject(Router);

  const allowedRoles: UserRole[] = route.data['roles'];

  // ✅ pas connecté → login
  if (!authService.isAuthenticated()) {
    router.navigate(['/login']);
    return false;
  }

  const userRole = authService.getUserRole();

  // ✅ rôle autorisé
  if (userRole && allowedRoles.includes(userRole)) {
    return true;
  }

  // ❌ rôle interdit → forbidden (redirection claire)
  router.navigate(['/forbidden']);
  return false;
};
