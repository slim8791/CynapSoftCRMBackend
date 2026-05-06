import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'currencyTND'
})
export class CurrencyTNDPipe implements PipeTransform {
  transform(value: number): string {
    if (value == null) return '0,000 TND';
    return new Intl.NumberFormat('fr-TN', {
      style: 'currency',
      currency: 'TND',
      minimumFractionDigits: 3,
      maximumFractionDigits: 3
    }).format(value);
  }
}

