import { Pipe, PipeTransform } from '@angular/core';
import { VisiteType } from '../../core/models/enums';

const VISITE_LABELS: Record<VisiteType, string> = {
  [VisiteType.Medecin]:    'Médecin',
  [VisiteType.Pharmacien]: 'Pharmacien',
  [VisiteType.Autre]:      'Autre',
};

@Pipe({ name: 'visiteStatus', standalone: true })
export class VisiteStatusPipe implements PipeTransform {
  transform(type: VisiteType | number): string {
    return VISITE_LABELS[type as VisiteType] ?? 'Inconnu';
  }
}
