import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  { path: '', redirectTo: 'stocks', pathMatch: 'full' },
  {
    path: 'stocks',
    loadComponent: () => import('./stocks/stock-list/stock-list.component').then(m => m.StockListComponent)
  },
  {
    path: 'stocks/new',
    loadComponent: () => import('./stocks/stock-form/stock-form.component').then(m => m.StockFormComponent)
  },
  {
    path: 'stocks/:id',
    loadComponent: () => import('./stocks/stock-detail/stock-detail.component').then(m => m.StockDetailComponent)
  },
  {
    path: 'stocks/:id/edit',
    loadComponent: () => import('./stocks/stock-form/stock-form.component').then(m => m.StockFormComponent)
  },
  {
    path: 'movements',
    loadComponent: () => import('./movements/movement-list/movement-list.component').then(m => m.MovementListComponent)
  },
  {
    path: 'distributions',
    loadComponent: () => import('./distributions/distribution-list/distribution-list.component').then(m => m.DistributionListComponent)
  },
  {
    path: 'distributions/new',
    loadComponent: () => import('./distributions/distribution-form/distribution-form.component').then(m => m.DistributionFormComponent)
  },
  {
    path: 'distributions/:id',
    loadComponent: () => import('./distributions/distribution-detail/distribution-detail.component').then(m => m.DistributionDetailComponent)
  },
  {
    path: 'promo-stocks',
    loadComponent: () => import('./promo-stocks/promo-stock-detail/promo-stock-detail.component').then(m => m.PromoStockDetailComponent)
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class InventoryRoutingModule {}
