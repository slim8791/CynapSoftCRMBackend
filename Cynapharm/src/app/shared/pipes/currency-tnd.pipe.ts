import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'currencyTND'
})
export class CurrencyTNDPipe implements PipeTransform {
  transform(value: number): string {
    if (value == null) return '0.000 TND';
    return value.toFixed(3) + ' TND';
  }
}

