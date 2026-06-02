import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { OrderListComponent } from './order-list/order-list.component';
import { OrderDetailComponent } from './order-detail/order-detail.component';
import { OrderFormComponent } from './order-form/order-form.component';
import { authGuard } from '../../core/guards/auth.guard';

const routes: Routes = [
  { path: '', component: OrderListComponent },
  { path: 'new', canActivate: [authGuard], component: OrderFormComponent },
  // Réclamations (sous orders)
  {
    path: 'reclamations',
    canActivate: [authGuard],
    loadComponent: () => import('./reclamations/reclamation-list/reclamation-list.component').then(m => m.ReclamationListComponent)
  },
  {
    path: 'reclamations/new',
    canActivate: [authGuard],
    loadComponent: () => import('./reclamations/reclamation-form/reclamation-form.component').then(m => m.ReclamationFormComponent)
  },
  {
    path: 'reclamations/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./reclamations/reclamation-detail/reclamation-detail.component').then(m => m.ReclamationDetailComponent)
  },
  {
    path: 'reclamations/:id/edit',
    canActivate: [authGuard],
    loadComponent: () => import('./reclamations/reclamation-form/reclamation-form.component').then(m => m.ReclamationFormComponent)
  },
  { path: ':id', canActivate: [authGuard], component: OrderDetailComponent },
  { path: ':id/edit', canActivate: [authGuard], component: OrderFormComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class OrdersRoutingModule { }
