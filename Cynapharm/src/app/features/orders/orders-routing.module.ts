import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { OrderListComponent } from './order-list/order-list.component';
import { OrderDetailComponent } from './order-detail/order-detail.component';
import { OrderFormComponent } from './order-form/order-form.component';

const routes: Routes = [
  { path: '',       component: OrderListComponent  },
  { path: 'new',    component: OrderFormComponent  },
  // Réclamations (sous orders)
  { path: 'reclamations',
    loadComponent: () => import('./reclamations/reclamation-list/reclamation-list.component').then(m => m.ReclamationListComponent) },
  { path: 'reclamations/new',
    loadComponent: () => import('./reclamations/reclamation-form/reclamation-form.component').then(m => m.ReclamationFormComponent) },
  { path: 'reclamations/:id',
    loadComponent: () => import('./reclamations/reclamation-detail/reclamation-detail.component').then(m => m.ReclamationDetailComponent) },
  { path: 'reclamations/:id/edit',
    loadComponent: () => import('./reclamations/reclamation-form/reclamation-form.component').then(m => m.ReclamationFormComponent) },
  { path: ':id',    component: OrderDetailComponent },
  { path: ':id/edit', component: OrderFormComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class OrdersRoutingModule { }
