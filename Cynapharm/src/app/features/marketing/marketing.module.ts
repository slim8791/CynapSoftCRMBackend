import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MarketingRoutingModule } from './marketing-routing.module';
import { SupportListComponent } from './support-list/support-list.component';
import { SupportFormComponent } from './support-form/support-form.component';
import { SupportDetailComponent } from './support-detail/support-detail.component';
import { MarketingService } from './marketing.service';
import { SharedModule } from '../../shared/shared.module';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    MarketingRoutingModule,
    SupportListComponent,
    SupportFormComponent,
    SupportDetailComponent,
    SharedModule
  ],
  providers: [MarketingService]
})
export class MarketingModule { }
