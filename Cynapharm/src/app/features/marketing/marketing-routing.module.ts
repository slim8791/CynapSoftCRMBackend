import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { SupportListComponent } from './support-list/support-list.component';
import { SupportFormComponent } from './support-form/support-form.component';
import { SupportDetailComponent } from './support-detail/support-detail.component';

import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';
import { UserRole } from '../../core/services/auth.service';

const routes: Routes = [
  {
    path: 'supports',
    component: SupportListComponent,
    canActivate: [authGuard, roleGuard],
    data: {
      roles: [UserRole.ADMIN, UserRole.SUPERVISEUR, UserRole.DELEGUE]
    }
  },
  {
    path: 'supports/new',
    component: SupportFormComponent,
    canActivate: [authGuard, roleGuard],
    data: {
      roles: [UserRole.ADMIN, UserRole.SUPERVISEUR]
    }
  },
  {
    path: 'supports/:id',
    component: SupportDetailComponent,
    canActivate: [authGuard, roleGuard],
    data: {
      roles: [UserRole.ADMIN, UserRole.SUPERVISEUR, UserRole.DELEGUE]
    }
  },
  {
    path: 'supports/:id/edit',
    component: SupportFormComponent,
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
export class MarketingRoutingModule {}