import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./promotion-list/promotion-list.component').then(m => m.PromotionListComponent)
  },
  {
    path: 'new',
    loadComponent: () =>
      import('./promotion-form/promotion-form.component').then(m => m.PromotionFormComponent)
  },
  {
    path: 'analytics',
    loadComponent: () =>
      import('./promotion-analytics/promotion-analytics.component').then(m => m.PromotionAnalyticsComponent)
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./promotion-detail/promotion-detail.component').then(m => m.PromotionDetailComponent)
  },
  {
    path: ':id/edit',
    loadComponent: () =>
      import('./promotion-form/promotion-form.component').then(m => m.PromotionFormComponent)
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PromotionsRoutingModule {}
