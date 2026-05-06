import { Pipe, PipeTransform } from '@angular/core';

const DOC_LABELS: Record<string, string> = {
  Facture:      'Facture',
  BonCommande:  'Bon de commande',
  BonLivraison: 'Bon de livraison',
  Autre:        'Autre',
};

@Pipe({ name: 'documentType', standalone: true })
export class DocumentTypePipe implements PipeTransform {
  transform(type: string): string {
    return DOC_LABELS[type] ?? type ?? 'Inconnu';
  }
}
