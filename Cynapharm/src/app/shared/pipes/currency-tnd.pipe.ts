import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'currencyTND',
  standalone: true
})
export class CurrencyTNDPipe implements PipeTransform {
  transform(value: number): string {
    return new Intl.NumberFormat('fr-FR', {
      style: 'currency',
      currency: 'TND',
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(value);
  }
}
