import { Pipe, PipeTransform } from '@angular/core';
import { EtatPlanning, PLANNING_STATUS_LABELS } from '../../core/models/enums';

@Pipe({ name: 'planningStatus', standalone: true })
export class PlanningStatusPipe implements PipeTransform {
  transform(etat: EtatPlanning | number): string {
    return PLANNING_STATUS_LABELS[etat as EtatPlanning] ?? 'Inconnu';
  }
}

@Pipe({ name: 'planningStatusClass', standalone: true })
export class PlanningStatusClassPipe implements PipeTransform {
  transform(etat: EtatPlanning | number): string {
    switch (etat) {
      case EtatPlanning.Confirme:  return 'badge-success';
      case EtatPlanning.Annule:    return 'badge-danger';
      default:                     return 'badge-warning';
    }
  }
}
