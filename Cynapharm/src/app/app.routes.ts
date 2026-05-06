import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { UserRole } from './core/services/auth.service';

import { LoginComponent }          from './features/auth/login/login.component';
import { RegisterComponent }       from './features/auth/register/register.component';
import { ForgotPasswordComponent } from './features/auth/forgot-password/forgot-password.component';
import { ResetPasswordComponent }  from './features/auth/reset-password/reset-password.component';

export const routes: Routes = [

  // ── Public ────────────────────────────────────────────
  { path: 'login',          component: LoginComponent },
  { path: 'register',       component: RegisterComponent },
  { path: 'forgot-password',component: ForgotPasswordComponent },
  { path: 'reset-password', component: ResetPasswordComponent },
  { path: 'forbidden',
    loadComponent: () => import('./features/shared/forbidden.component').then(m => m.ForbiddenComponent)
  },

  // ── Default ───────────────────────────────────────────
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },

  // ── Dashboard ─────────────────────────────────────────
  {
    path: 'dashboard',
    loadChildren: () => import('./features/dashboard/dashboard.module').then(m => m.DashboardModule),
    canActivate: [authGuard]
  },

  // ── Products & Lots ────────────────────────────────────
  {
    path: 'products',
    loadChildren: () => import('./features/products/products.module').then(m => m.ProductsModule),
    canActivate: [authGuard]
  },
  {
    path: 'lots',
    loadChildren: () => import('./features/lots/lots.module').then(m => m.LotsModule),
    canActivate: [authGuard, roleGuard],
    data: { roles: [UserRole.ADMIN, UserRole.SUPERVISEUR, UserRole.DELEGUE] }
  },

  // ── Promotions ────────────────────────────────────────
  {
    path: 'promotions',
    loadChildren: () => import('./features/promotions/promotions.module').then(m => m.PromotionsModule),
    canActivate: [authGuard, roleGuard],
    data: { roles: [UserRole.ADMIN, UserRole.SUPERVISEUR] }
  },

  // ── Marketing (gardé pour compatibilité routes directes)
  {
    path: 'marketing',
    loadChildren: () => import('./features/marketing/marketing.module').then(m => m.MarketingModule),
    canActivate: [authGuard, roleGuard],
    data: { roles: [UserRole.ADMIN, UserRole.SUPERVISEUR, UserRole.DELEGUE] }
  },

  // ── Orders + Réclamations ─────────────────────────────
  {
    path: 'orders',
    loadChildren: () => import('./features/orders/orders.module').then(m => m.OrdersModule),
    canActivate: [authGuard]
  },

  // ── Inventory (Stocks, Mouvements, Distributions, Promo-stocks) ─
  {
    path: 'inventory',
    loadChildren: () => import('./features/inventory/inventory.module').then(m => m.InventoryModule),
    canActivate: [authGuard, roleGuard],
    data: { roles: [UserRole.ADMIN, UserRole.SUPERVISEUR, UserRole.DELEGUE] }
  },

  // ── Field (Visites, Plannings, Rapports, Objectifs, Régions, KPI) ─
  {
    path: 'field',
    loadChildren: () => import('./features/field/field.module').then(m => m.FieldModule),
    canActivate: [authGuard, roleGuard],
    data: { roles: [UserRole.ADMIN, UserRole.SUPERVISEUR, UserRole.DELEGUE] }
  },

  // ── Documents (Documents, BCs, BLs, Factures) ─────────
  {
    path: 'documents',
    loadChildren: () => import('./features/documents/documents.module').then(m => m.DocumentsModule),
    canActivate: [authGuard, roleGuard],
    data: { roles: [UserRole.ADMIN, UserRole.SUPERVISEUR] }
  },

  // ── Users ─────────────────────────────────────────────
  {
    path: 'users',
    loadChildren: () => import('./features/users/users.module').then(m => m.UsersModule),
    canActivate: [authGuard, roleGuard],
    data: { roles: [UserRole.ADMIN, UserRole.SUPERVISEUR] }
  },

  // ── Settings (profil + changement de mot de passe) ────
  {
    path: 'settings',
    loadComponent: () => import('./features/settings/settings.component').then(m => m.SettingsComponent),
    canActivate: [authGuard]
  },

  // ── Fallback ──────────────────────────────────────────
  { path: '**', redirectTo: 'dashboard' }
];
