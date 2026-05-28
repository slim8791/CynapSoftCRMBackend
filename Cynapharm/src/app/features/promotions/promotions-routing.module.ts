import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';

const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./promotion-list/promotion-list.component').then(m => m.PromotionListComponent)
  },
  {
    path: 'new',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR'] },
    loadComponent: () =>
      import('./promotion-form/promotion-form.component').then(m => m.PromotionFormComponent)
  },
  {
    path: 'analytics',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () =>
      import('./promotion-analytics/promotion-analytics.component').then(m => m.PromotionAnalyticsComponent)
  },
  {
    path: ':id',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () =>
      import('./promotion-detail/promotion-detail.component').then(m => m.PromotionDetailComponent)
  },
  {
    path: ':id/edit',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR'] },
    loadComponent: () =>
      import('./promotion-form/promotion-form.component').then(m => m.PromotionFormComponent)
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PromotionsRoutingModule { }
