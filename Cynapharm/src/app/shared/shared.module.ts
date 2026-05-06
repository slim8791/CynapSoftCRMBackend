import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

// Components
import { ButtonComponent } from './components/button/button.component';
import { CardComponent } from './components/card/card.component';
import { TableComponent } from './components/table/table.component';
import { ToastComponent } from './components/toast/toast.component';

// Directives
import { HighlightDirective } from './directives/highlight.directive';
import { DebounceClickDirective } from './directives/debounce-click.directive';

// Pipes
import { DateFormatPipe } from './pipes/date-format.pipe';
import { CurrencyFormatPipe } from './pipes/currency-format.pipe';
import { CurrencyTNDPipe } from './pipes/currency-tnd.pipe';

const COMPONENTS = [
  ButtonComponent,
  CardComponent,
  TableComponent,
  ToastComponent
];

const DIRECTIVES = [
  HighlightDirective,
  DebounceClickDirective
];

const PIPES = [
  DateFormatPipe,
  CurrencyFormatPipe,
  CurrencyTNDPipe
];

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    ...COMPONENTS,
    ...DIRECTIVES,
    ...PIPES
  ],
  exports: [
    ...COMPONENTS,
    ...DIRECTIVES,
    ...PIPES
  ]
})
export class SharedModule { }
