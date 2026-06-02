import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ProductListComponent } from './product-list/product-list.component';
import { ProductDetailComponent } from './product-detail/product-detail.component';
import { ProductFormComponent } from './product-form/product-form.component';
import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';

const routes: Routes = [
  {
    path: '',
    component: ProductListComponent
  },
  {
    path: 'new',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR'] },
    component: ProductFormComponent
  },
  {
    path: ':id/edit',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR'] },
    component: ProductFormComponent
  },
  {
    path: ':id',
    canActivate: [authGuard],
    component: ProductDetailComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ProductsRoutingModule { }
