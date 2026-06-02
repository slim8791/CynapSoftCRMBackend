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
    return this.api.get(`/products/lots/available/${productId}`).pipe(
      map((response: any) => response.Result ?? response.result ?? [])
    );
  }

  getExpiredLots(): Observable<any[]> {
    return this.api.get('/products/lots/expiring').pipe(
      map((response: any) => response.Result ?? response.result ?? [])
    );
  }

  getLotsByProductId(productId: number): Observable<any[]> {
    return this.api.get(`/products/lots/product/${productId}`).pipe(
      map((response: any) => response.Result ?? response.result ?? [])
    );
  }

  getLotByNumero(numeroLot: string): Observable<any> {
    return this.api.get(`/products/lots/${numeroLot}`).pipe(
      map((response: any) => response.Result ?? response.result)
    );
  }

  createOrUpdateLot(lotData: any): Observable<any> {
    return this.api.post('/products/lots/lot', lotData).pipe(
      map((response: any) => response.Result ?? response.result)
    );
  }

  deleteLot(numeroLot: string): Observable<any> {
    return this.api.delete(`/products/lots/${numeroLot}`);
  }

  adjustStock(productId: number, quantityChange: number): Observable<any> {
    return this.api.put(`/products/${productId}/adjust-stock?quantityChange=${quantityChange}`, {});
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
