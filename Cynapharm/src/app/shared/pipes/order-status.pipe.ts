import { Pipe, PipeTransform } from '@angular/core';
import { OrderStatus, ORDER_STATUS_LABELS } from '../../core/models/enums';

@Pipe({ name: 'orderStatus', standalone: true })
export class OrderStatusPipe implements PipeTransform {
  transform(status: OrderStatus | number): string {
    return ORDER_STATUS_LABELS[status as OrderStatus] ?? 'Inconnu';
  }
}

@Pipe({ name: 'orderStatusClass', standalone: true })
export class OrderStatusClassPipe implements PipeTransform {
  transform(status: OrderStatus | number): string {
    switch (status) {
      case OrderStatus.Validee:   return 'badge-success';
      case OrderStatus.Livree:    return 'badge-success';
      case OrderStatus.EnAttente: return 'badge-warning';
      case OrderStatus.Expediee:  return 'badge-info';
      case OrderStatus.Annulee:   return 'badge-danger';
      default:                    return 'badge-secondary';
    }
  }
}
