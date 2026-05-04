import { Pipe, PipeTransform } from '@angular/core';

export type ProductStatusType = 'active' | 'inactive' | 'archived';

/**
 * Pipe to compute product status from IsActive and IsArchived flags
 * Returns: 'Actif', 'Inactif', or 'Archivé'
 */
@Pipe({
  name: 'productStatus',
  standalone: true
})
export class ProductStatusPipe implements PipeTransform {
  transform(product: any): string {
    if (!product) return '';
    if (product.IsArchived) return 'Archivé';
    return product.IsActive ? 'Actif' : 'Inactif';
  }
}

/**
 * Pipe to get CSS class for product status badge
 * Returns: 'badge-active', 'badge-inactive', or 'badge-archived'
 */
@Pipe({
  name: 'productStatusClass',
  standalone: true
})
export class ProductStatusClassPipe implements PipeTransform {
  transform(product: any): string {
    if (!product) return '';
    if (product.IsArchived) return 'badge-archived';
    return product.IsActive ? 'badge-active' : 'badge-inactive';
  }
}

/**
 * Pipe to get status type for conditional UI logic
 */
@Pipe({
  name: 'productStatusType',
  standalone: true
})
export class ProductStatusTypePipe implements PipeTransform {
  transform(product: any): ProductStatusType {
    if (!product) return 'inactive';
    if (product.IsArchived) return 'archived';
    return product.IsActive ? 'active' : 'inactive';
  }
}
