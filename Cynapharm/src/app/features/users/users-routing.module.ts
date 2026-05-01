import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { UserListComponent } from './user-list/user-list.component';
import { UserDetailComponent } from './user-detail/user-detail.component';
import { UserFormComponent } from './user-form/user-form.component';

import { authGuard } from '../../core/guards/auth.guard';          // ✅ AJOUT
import { roleGuard } from '../../core/guards/role.guard';          // ✅ AJOUT
import { UserRole } from '../../core/services/auth.service';       // ✅ AJOUT

const routes: Routes = [
  {
    path: '',
    component: UserListComponent,
    canActivate: [authGuard, roleGuard],                           // ✅ AJOUT
    data: {
      roles: [UserRole.ADMIN, UserRole.SUPERVISEUR]                // ✅ BACKEND ROLES
    }
  },
  {
    path: 'new',
    component: UserFormComponent,
    canActivate: [authGuard, roleGuard],
    data: {
      roles: [UserRole.ADMIN, UserRole.SUPERVISEUR]
    }
  },
  {
    path: ':id',
    component: UserDetailComponent,
    canActivate: [authGuard, roleGuard],
    data: {
      roles: [UserRole.ADMIN, UserRole.SUPERVISEUR]
    }
  },
  {
    path: ':id/edit',
    component: UserFormComponent,
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
export class UsersRoutingModule {}