import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { OrderListComponent } from './order-list/order-list.component';
import { OrderDetailComponent } from './order-detail/order-detail.component';
import { OrderFormComponent } from './order-form/order-form.component';

const routes: Routes = [
  {
    path: '',
    component: OrderListComponent
  },
  {
    path: 'new',
    component: OrderFormComponent
  },
  {
    path: ':id',
    component: OrderDetailComponent
  },
  {
    path: ':id/edit',
    component: OrderFormComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class OrdersRoutingModule { }
