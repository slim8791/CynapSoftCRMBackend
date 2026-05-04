import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { UserRole } from './core/services/auth.service';

import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { ForgotPasswordComponent } from './features/auth/forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './features/auth/reset-password/reset-password.component';

export const routes: Routes = [

  // ✅ PUBLIC
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'forgot-password', component: ForgotPasswordComponent },
  { path: 'reset-password', component: ResetPasswordComponent },

  // ✅ FORBIDDEN (utilisé par RoleGuard)
  {
    path: 'forbidden',
    loadComponent: () =>
      import('./features/shared/forbidden.component')
        .then(m => m.ForbiddenComponent)
  },

  // ✅ DEFAULT
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },

  // ✅ DASHBOARD (auth seulement)
  {
    path: 'dashboard',
    loadChildren: () =>
      import('./features/dashboard/dashboard.module')
        .then(m => m.DashboardModule),
    canActivate: [authGuard]
  },

  // ✅ USERS (AUTH + ROLE ✅✅✅)
  {
    path: 'users',
    loadChildren: () =>
      import('./features/users/users.module')
        .then(m => m.UsersModule),
    canActivate: [authGuard, roleGuard],
    data: {
      roles: [UserRole.ADMIN, UserRole.SUPERVISEUR]
    }
  },

  // ✅ AUTRES
  {
    path: 'products',
    loadChildren: () =>
      import('./features/products/products.module')
        .then(m => m.ProductsModule),
    canActivate: [authGuard]
  },

  {
    path: 'lots',
    loadChildren: () =>
      import('./features/lots/lots.module')
        .then(m => m.LotsModule),
    canActivate: [authGuard, roleGuard],
    data: {
      roles: [UserRole.ADMIN, UserRole.SUPERVISEUR, UserRole.DELEGUE]
    }
  },

  {
    path: 'marketing',
    loadChildren: () =>
      import('./features/marketing/marketing.module')
        .then(m => m.MarketingModule),
    canActivate: [authGuard, roleGuard],
    data: {
      roles: [UserRole.ADMIN, UserRole.SUPERVISEUR, UserRole.DELEGUE]
    }
  },

  {
    path: 'orders',
    loadChildren: () =>
      import('./features/orders/orders.module')
        .then(m => m.OrdersModule),
    canActivate: [authGuard]
  },

  // ✅ FALLBACK
  { path: '**', redirectTo: 'dashboard' }
];
