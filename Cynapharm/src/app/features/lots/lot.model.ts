// ── lot.model.ts ─────────────────────────────────────────────────────────────
// Miroir exact du LotDto C# (CynapCRM.Services.ProductAPI.Models.Dto)
// Les champs calculés (isExpired, isOutOfStock) viennent du backend ;
// le frontend ne les recalcule JAMAIS lui-même.

export interface PromotionDto {
  id_Promo?:       number;
  codePromo:       string;
  pourcentage:     number;
  dateDebut:       string;   // ISO 8601
  dateExpiration:  string;   // ISO 8601
  estActive:       boolean;
  numeroLot:       string;
  isValid?:        boolean;
}

export interface LotDto {
  /** Numéro unique du lot (clé métier) */
  numero: string;
  /** Date d'expiration ISO 8601 */
  dateExpiration: string;
  /** Quantité disponible */
  quantite: number;
  /** FK vers le produit */
  idProduit: number;       // sérialisé id_Produit vers le backend
  // ── Champs calculés par le backend ──────────────────────────
  /** Lot expiré (dateExpiration < today) */
  isExpired: boolean;
  /** Rupture de stock (quantite === 0) */
  isOutOfStock: boolean;
  /** Promotions attachées (optionnel) */
  promotions?: PromotionDto[];
}

/** Payload envoyé au backend lors d'un create/update */
export interface LotPayload {
  numero: string;
  dateExpiration: string;
  quantite: number;
  id_Produit: number;   // nom exact attendu par l'API
}

// ── Statut dérivé (pur utilitaire frontend, calculé UNE SEULE FOIS) ──────────

export type LotStatus = 'active' | 'low-stock' | 'out-of-stock' | 'expired';

/** Seuil en-dessous duquel le stock est jugé faible */
const LOW_STOCK_THRESHOLD = 5;

/**
 * Dérive le statut d'affichage depuis les flags métier du backend.
 * Ordre de priorité : expired > out-of-stock > low-stock > active
 */
export function getLotStatus(lot: LotDto): LotStatus {
  if (lot.isExpired)     return 'expired';
  if (lot.isOutOfStock)  return 'out-of-stock';
  if (lot.quantite <= LOW_STOCK_THRESHOLD) return 'low-stock';
  return 'active';
}

export const STATUS_LABEL: Record<LotStatus, string> = {
  'active':       'Actif',
  'low-stock':    'Stock faible',
  'out-of-stock': 'Rupture',
  'expired':      'Expiré',
};

export const STATUS_CSS_CLASS: Record<LotStatus, string> = {
  'active':       'status-active',
  'low-stock':    'status-low-stock',
  'out-of-stock': 'status-out-of-stock',
  'expired':      'status-expired',
};