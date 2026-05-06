export enum OrderStatus {
  Brouillon  = 0,
  EnAttente  = 1,
  Validee    = 2,
  Expediee   = 3,
  Livree     = 4,
  Annulee    = 5,
}

export const ORDER_STATUS_LABELS: Record<OrderStatus, string> = {
  [OrderStatus.Brouillon]: 'Brouillon',
  [OrderStatus.EnAttente]: 'En attente',
  [OrderStatus.Validee]:   'Validée',
  [OrderStatus.Expediee]:  'Expédiée',
  [OrderStatus.Livree]:    'Livrée',
  [OrderStatus.Annulee]:   'Annulée',
};

export enum StatutReclamation {
  EnCours  = 0,
  Traitee  = 1,
  Rejetee  = 2,
}

export const RECLAMATION_STATUS_LABELS: Record<StatutReclamation, string> = {
  [StatutReclamation.EnCours]:  'En cours',
  [StatutReclamation.Traitee]:  'Traitée',
  [StatutReclamation.Rejetee]:  'Rejetée',
};

export enum VisiteType {
  Medecin    = 1,
  Pharmacien = 2,
  Autre      = 3,
}

export enum EtatPlanning {
  EnAttente = 0,
  Confirme  = 1,
  Annule    = 2,
}

export const PLANNING_STATUS_LABELS: Record<EtatPlanning, string> = {
  [EtatPlanning.EnAttente]: 'En attente',
  [EtatPlanning.Confirme]:  'Confirmé',
  [EtatPlanning.Annule]:    'Annulé',
};

export enum PeriodeObjectif {
  Mensuel      = 1,
  Trimestriel  = 2,
  Annuel       = 3,
}

export enum TypeObjectif {
  Visites        = 'Visites',
  ChiffreAffaires= 'ChiffreAffaires',
  NouveauxClients= 'NouveauxClients',
  Fidelisation   = 'Fidelisation',
}

export enum DocumentType {
  Facture        = 'Facture',
  BonCommande    = 'BonCommande',
  BonLivraison   = 'BonLivraison',
  Autre          = 'Autre',
}

export enum StockType {
  Delegue    = 'Delegue',
  Echantillon= 'Echantillon',
  Gratuite   = 'Gratuite',
}
