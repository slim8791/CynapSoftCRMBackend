import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'currencyTND'
})
export class CurrencyTNDPipe implements PipeTransform {
  transform(value: number): string {
    if (value == null || isNaN(value)) return '—';
    return new Intl.NumberFormat('fr-TN', {
      minimumFractionDigits: 3,
      maximumFractionDigits: 3
    }).format(value) + ' TND';
  }
}

