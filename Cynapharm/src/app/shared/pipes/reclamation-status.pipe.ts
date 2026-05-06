import { Pipe, PipeTransform } from '@angular/core';
import { StatutReclamation, RECLAMATION_STATUS_LABELS } from '../../core/models/enums';

@Pipe({ name: 'reclamationStatus', standalone: true })
export class ReclamationStatusPipe implements PipeTransform {
  transform(status: StatutReclamation | number | undefined | null): string {
    if (status == null) return '—';
    return RECLAMATION_STATUS_LABELS[status as StatutReclamation] ?? 'Inconnu';
  }
}

@Pipe({ name: 'reclamationStatusClass', standalone: true })
export class ReclamationStatusClassPipe implements PipeTransform {
  transform(status: StatutReclamation | number | undefined | null): string {
    switch (status) {
      case StatutReclamation.Traitee:  return 'badge-success';
      case StatutReclamation.Rejetee:  return 'badge-danger';
      default:                         return 'badge-warning';
    }
  }
}
