import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';

const routes: Routes = [
  { path: '', redirectTo: 'stocks', pathMatch: 'full' },
  {
    path: 'stocks',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./stocks/stock-list/stock-list.component').then(m => m.StockListComponent)
  },
  {
    path: 'stocks/new',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./stocks/stock-form/stock-form.component').then(m => m.StockFormComponent)
  },
  {
    path: 'stocks/:id',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./stocks/stock-detail/stock-detail.component').then(m => m.StockDetailComponent)
  },
  {
    path: 'stocks/:id/edit',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./stocks/stock-form/stock-form.component').then(m => m.StockFormComponent)
  },
  {
    path: 'movements/new',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./movements/movement-form/movement-form.component').then(m => m.MovementFormComponent)
  },
  {
    path: 'movements',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./movements/movement-list/movement-list.component').then(m => m.MovementListComponent)
  },
  {
    path: 'distributions',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./distributions/distribution-list/distribution-list.component').then(m => m.DistributionListComponent)
  },
  {
    path: 'distributions/new',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./distributions/distribution-form/distribution-form.component').then(m => m.DistributionFormComponent)
  },
  {
    path: 'distributions/:id',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./distributions/distribution-detail/distribution-detail.component').then(m => m.DistributionDetailComponent)
  },
  {
    path: 'promo-stocks/new',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./promo-stocks/promo-stock-form/promo-stock-form.component').then(m => m.PromoStockFormComponent)
  },
  {
    path: 'promo-stocks',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./promo-stocks/promo-stock-detail/promo-stock-detail.component').then(m => m.PromoStockDetailComponent)
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class InventoryRoutingModule { }
