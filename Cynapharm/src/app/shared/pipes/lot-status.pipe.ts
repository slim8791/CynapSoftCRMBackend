import { Pipe, PipeTransform } from '@angular/core';

export type LotStatusType = 'active' | 'expired' | 'low-stock';

/**
 * Pipe to compute lot status based on expiration date and quantity
 * Returns: 'En stock', 'Expiré', or 'Faible'
 */
@Pipe({
  name: 'lotStatus',
  standalone: true
})
export class LotStatusPipe implements PipeTransform {
  transform(lot: any, threshold: number = 5): string {
    if (!lot) return '';

    const dateExp = lot.DateExpiration ?? lot.dateExpiration;
    if (!dateExp) return 'Inconnu';

    const isExpired = new Date(dateExp) < new Date();
    if (isExpired) return 'Expiré';

    const quantity = lot.Quantite ?? lot.quantite ?? 0;
    if (quantity <= threshold) return 'Faible';

    return 'En stock';
  }
}

/**
 * Pipe to get CSS class for lot status badge
 * Returns: 'badge-active', 'badge-expired', or 'badge-low-stock'
 */
@Pipe({
  name: 'lotStatusClass',
  standalone: true
})
export class LotStatusClassPipe implements PipeTransform {
  transform(lot: any, threshold: number = 5): string {
    if (!lot) return '';

    const dateExp = lot.DateExpiration ?? lot.dateExpiration;
    if (!dateExp) return 'badge-unknown';

    const isExpired = new Date(dateExp) < new Date();
    if (isExpired) return 'badge-expired';

    const quantity = lot.Quantite ?? lot.quantite ?? 0;
    if (quantity <= threshold) return 'badge-low-stock';

    return 'badge-active';
  }
}

/**
 * Pipe to get icon code for lot status
 * Returns: SVG icon class or code
 */
@Pipe({
  name: 'lotStatusIcon',
  standalone: true
})
export class LotStatusIconPipe implements PipeTransform {
  transform(lot: any, threshold: number = 5): string {
    if (!lot) return 'unknown';

    const dateExp = lot.DateExpiration ?? lot.dateExpiration;
    if (!dateExp) return 'unknown';

    const isExpired = new Date(dateExp) < new Date();
    if (isExpired) return 'expired';

    const quantity = lot.Quantite ?? lot.quantite ?? 0;
    if (quantity <= threshold) return 'low-stock';

    return 'active';
  }
}
