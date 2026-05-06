import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LotsRoutingModule } from './lots-routing.module';
import { LotListComponent } from './lot-list/lot-list.component';
import { LotDetailComponent } from './lot-detail/lot-detail.component';
import { LotFormComponent } from './lot-form/lot-form.component';
import { LotService } from './lot.service';
import { SharedModule } from '../../shared/shared.module';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    LotsRoutingModule,
    LotListComponent,
    LotDetailComponent,
    LotFormComponent,
    SharedModule
  ],
  providers: [LotService]
})
export class LotsModule { }
