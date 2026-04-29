import { Pipe, PipeTransform } from '@angular/core';
import { CurrencyPipe } from '@angular/common';

@Pipe({
  name: 'currencyFormat',
  standalone: true
})
export class CurrencyFormatPipe implements PipeTransform {
  private currencyPipe = new CurrencyPipe('en-US');

  transform(value: any, currencyCode: string = 'USD', display: string = 'symbol', digits?: string): string | null {
    return this.currencyPipe.transform(value, currencyCode, display, digits);
  }
}
