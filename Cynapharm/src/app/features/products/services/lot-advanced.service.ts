import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../core/services/api.service';

export interface LotStatus {
  numero: string;
  quantite: number;
  dateExpiration: Date;
  isExpired: boolean;
  isOutOfStock: boolean;
  daysUntilExpiration: number;
  status: 'available' | 'expired' | 'out-of-stock';
}

@Injectable({
  providedIn: 'root'
})
export class LotAdvancedService {

  private lotStatusCache$ = new BehaviorSubject<LotStatus[]>([]);

  constructor(private api: ApiService) {}

  /**
   * Récupère les lots disponibles pour un produit (non expirés, quantité > 0)
   */
  getAvailableLots(productId: number): Observable<any[]> {
    return this.api.get(`/product/${productId}/available-lots`).pipe(
      map((response: any) => response.result ?? [])
    );
  }

  /**
   * Récupère les lots expirés
   */
  getExpiredLots(): Observable<any[]> {
    return this.api.get('/product/expired-lots').pipe(
      map((response: any) => response.result ?? [])
    );
  }

  /**
   * Récupère les lots par produit
   */
  getLotsByProductId(productId: number): Observable<any[]> {
    return this.api.get(`/product/${productId}/lots`).pipe(
      map((response: any) => response.result ?? [])
    );
  }

  /**
   * Récupère un lot spécifique
   */
  getLotByNumero(numeroLot: string): Observable<any> {
    return this.api.get(`/product/lot/${numeroLot}`).pipe(
      map((response: any) => response.result)
    );
  }

  /**
   * Crée ou met à jour un lot
   */
  createOrUpdateLot(lotData: any): Observable<any> {
    return this.api.post('/product/lot', lotData).pipe(
      map((response: any) => response.result)
    );
  }

  /**
   * Supprime un lot
   */
  deleteLot(numeroLot: string): Observable<any> {
    return this.api.delete(`/product/lot/${numeroLot}`);
  }

  /**
   * Ajuste le stock d'un produit (logique FEFO appliquée au backend)
   * @param productId ID du produit
   * @param quantityChange Quantité à ajuster (positif pour augmenter, négatif pour diminuer)
   */
  adjustStock(productId: number, quantityChange: number): Observable<any> {
    return this.api.put(`/product/product/${productId}/adjust-stock?quantityChange=${quantityChange}`, {});
  }

  /**
   * Vérifie si un lot est expiré
   */
  isLotExpired(expirationDate: Date | string): boolean {
    const expDate = new Date(expirationDate);
    return expDate < new Date();
  }

  /**
   * Calcule les jours jusqu'à l'expiration
   */
  getDaysUntilExpiration(expirationDate: Date | string): number {
    const expDate = new Date(expirationDate);
    const now = new Date();
    const diffTime = expDate.getTime() - now.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays;
  }

  /**
   * Enrichit les données d'un lot avec le statut
   */
  enrichLotData(lot: any): LotStatus {
    const isExpired = this.isLotExpired(lot.dateExpiration || lot.DateExpiration);
    const isOutOfStock = (lot.quantite || lot.Quantite || 0) <= 0;
    const daysUntilExpiration = this.getDaysUntilExpiration(lot.dateExpiration || lot.DateExpiration);

    let status: LotStatus['status'] = 'available';
    if (isExpired) {
      status = 'expired';
    } else if (isOutOfStock) {
      status = 'out-of-stock';
    }

    return {
      numero: lot.numero || lot.Numero,
      quantite: lot.quantite || lot.Quantite,
      dateExpiration: new Date(lot.dateExpiration || lot.DateExpiration),
      isExpired,
      isOutOfStock,
      daysUntilExpiration,
      status
    };
  }

  /**
   * Trie les lots par date d'expiration (FEFO - First Expiration First Out)
   */
  sortByExpirationDateFEFO(lots: any[]): any[] {
    return lots.sort((a, b) => {
      const dateA = new Date(a.dateExpiration || a.DateExpiration);
      const dateB = new Date(b.dateExpiration || b.DateExpiration);
      return dateA.getTime() - dateB.getTime();
    });
  }

  /**
   * Filtre les lots disponibles
   */
  filterAvailableLots(lots: any[]): any[] {
    return lots.filter(lot => !this.isLotExpired(lot.dateExpiration || lot.DateExpiration) && (lot.quantite || lot.Quantite) > 0);
  }

  /**
   * Filtre les lots expirés
   */
  filterExpiredLots(lots: any[]): any[] {
    return lots.filter(lot => this.isLotExpired(lot.dateExpiration || lot.DateExpiration));
  }

  /**
   * Filtre les lots avec rupture de stock
   */
  filterOutOfStockLots(lots: any[]): any[] {
    return lots.filter(lot => (lot.quantite || lot.Quantite) <= 0);
  }

  /**
   * Filtre les lots proches de l'expiration (moins de X jours)
   */
  filterExpiringLots(lots: any[], daysThreshold: number = 7): any[] {
    return lots.filter(lot => {
      const daysRemaining = this.getDaysUntilExpiration(lot.dateExpiration || lot.DateExpiration);
      return daysRemaining > 0 && daysRemaining <= daysThreshold;
    });
  }

  /**
   * Calcule la quantité totale disponible pour un produit
   */
  calculateTotalAvailableQuantity(lots: any[]): number {
    return this.filterAvailableLots(lots).reduce((sum, lot) => sum + (lot.quantite || lot.Quantite || 0), 0);
  }

  /**
   * Calcule la quantité totale (tous les lots)
   */
  calculateTotalQuantity(lots: any[]): number {
    return lots.reduce((sum, lot) => sum + (lot.quantite || lot.Quantite || 0), 0);
  }

  /**
   * Estime la disponibilité futur après N jours
   */
  estimateFutureAvailability(lots: any[], daysFromNow: number): number {
    const futureDate = new Date();
    futureDate.setDate(futureDate.getDate() + daysFromNow);

    return lots.filter(lot => {
      const expDate = new Date(lot.dateExpiration || lot.DateExpiration);
      return expDate > futureDate && (lot.quantite || lot.Quantite) > 0;
    }).reduce((sum, lot) => sum + (lot.quantite || lot.Quantite || 0), 0);
  }

  /**
   * Récupère le cache du statut des lots
   */
  getCachedLotStatus(): LotStatus[] {
    return this.lotStatusCache$.value;
  }

  /**
   * Met à jour le cache du statut des lots
   */
  updateCachedLotStatus(status: LotStatus[]): void {
    this.lotStatusCache$.next(status);
  }
}
