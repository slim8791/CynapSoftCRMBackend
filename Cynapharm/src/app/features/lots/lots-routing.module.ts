import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { LotListComponent } from './lot-list/lot-list.component';
import { LotDetailComponent } from './lot-detail/lot-detail.component';
import { LotFormComponent } from './lot-form/lot-form.component';

import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';
import { UserRole } from '../../core/services/auth.service';

const routes: Routes = [
  {
    path: '',
    component: LotListComponent,
    canActivate: [authGuard, roleGuard],
    data: {
      roles: [UserRole.ADMIN, UserRole.SUPERVISEUR, UserRole.DELEGUE]
    }
  },
  {
    path: 'new',
    component: LotFormComponent,
    canActivate: [authGuard, roleGuard],
    data: {
      roles: [UserRole.ADMIN, UserRole.SUPERVISEUR]
    }
  },
  {
    path: ':numero',
    component: LotDetailComponent,
    canActivate: [authGuard, roleGuard],
    data: {
      roles: [UserRole.ADMIN, UserRole.SUPERVISEUR, UserRole.DELEGUE]
    }
  },
  {
    path: ':numero/edit',
    component: LotFormComponent,
    canActivate: [authGuard, roleGuard],
    data: {
      roles: [UserRole.ADMIN, UserRole.SUPERVISEUR]
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class LotsRoutingModule {}