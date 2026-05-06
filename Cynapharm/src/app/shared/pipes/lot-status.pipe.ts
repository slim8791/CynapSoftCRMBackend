import { Pipe, PipeTransform } from '@angular/core';

export type LotStatusType = 'active' | 'expired' | 'low-stock' | 'out-of-stock';

/**
 * Calcule le statut d'un lot à partir de la date d'expiration et de la quantité.
 * Retourne : 'Expiré', 'Rupture', 'Faible' ou 'En stock'
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

    if (new Date(dateExp) < new Date()) return 'Expiré';

    const quantity = lot.Quantite ?? lot.quantite ?? 0;
    if (quantity === 0) return 'Rupture';
    if (quantity <= threshold) return 'Faible';

    return 'En stock';
  }
}

/**
 * Retourne la classe CSS du badge de statut d'un lot.
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

    if (new Date(dateExp) < new Date()) return 'badge-expired';

    const quantity = lot.Quantite ?? lot.quantite ?? 0;
    if (quantity === 0) return 'badge-out-of-stock';
    if (quantity <= threshold) return 'badge-low-stock';

    return 'badge-active';
  }
}

/**
 * Retourne le code icône du statut d'un lot.
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

    if (new Date(dateExp) < new Date()) return 'expired';

    const quantity = lot.Quantite ?? lot.quantite ?? 0;
    if (quantity === 0) return 'out-of-stock';
    if (quantity <= threshold) return 'low-stock';

    return 'active';
  }
}
