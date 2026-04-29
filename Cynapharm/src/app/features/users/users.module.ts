import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UsersRoutingModule } from './users-routing.module';
import { UserListComponent } from './user-list/user-list.component';
import { UserDetailComponent } from './user-detail/user-detail.component';
import { UserFormComponent } from './user-form/user-form.component';
import { UserService } from './user.service';
import { SharedModule } from '../../shared/shared.module';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    UsersRoutingModule,
    UserListComponent,
    UserDetailComponent,
    UserFormComponent,
    SharedModule
  ],
  providers: [UserService]
})
export class UsersModule { }
