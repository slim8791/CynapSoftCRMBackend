import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../core/services/api.service';

export interface PromotionWithStatus {
  id: number;
  code: string;
  percentage: number;
  startDate: Date;
  expirationDate: Date;
  isActive: boolean;
  isValid: boolean;
  daysRemaining: number;
  discountAmount?: number;
}

@Injectable({
  providedIn: 'root'
})
export class PromotionAdvancedService {

  constructor(private api: ApiService) {}

  /**
   * Récupère toutes les promotions
   */
  getAllPromotions(): Observable<any[]> {
    return this.api.get('/products/promos').pipe(
      map((response: any) => response.result ?? [])
    );
  }

  /**
   * Récupère une promotion par ID
   */
  getPromotionById(promotionId: number): Observable<any> {
    return this.api.get(`/products/promos/${promotionId}`).pipe(
      map((response: any) => response.result)
    );
  }

  /**
   * Récupère les promotions pour un produit
   */
  getPromotionsByProduct(productId: number): Observable<any[]> {
    return this.api.get(`/products/promos/product/${productId}`).pipe(
      map((response: any) => response.result ?? [])
    );
  }

  /**
   * Récupère les promotions pour un lot
   */
  getPromotionsByLot(numeroLot: string): Observable<any[]> {
    return this.api.get(`/products/promos/lot/${numeroLot}`).pipe(
      map((response: any) => response.result ?? [])
    );
  }

  /**
   * Applique la meilleure promotion pour un produit
   */
  applyBestPromotion(productId: number, initialPrice: number): Observable<number> {
    return this.api.get(`/products/promos/product/${productId}/apply?initialPrice=${initialPrice}`).pipe(
      map((response: any) => response.result ?? initialPrice)
    );
  }

  /**
   * Vérifie si un produit est en promotion
   */
  isProductInPromotion(productId: number): Observable<boolean> {
    return this.api.get(`/products/promos/product/${productId}/in-promotion`).pipe(
      map((response: any) => response.result ?? false)
    );
  }

  /**
   * Crée ou met à jour une promotion
   */
  createOrUpdatePromotion(promotionData: any): Observable<any> {
    return this.api.post('/products/promos', promotionData).pipe(
      map((response: any) => response.result)
    );
  }

  /**
   * Supprime une promotion
   */
  deletePromotion(promotionId: number): Observable<any> {
    return this.api.delete(`/products/promos/${promotionId}`);
  }

  /**
   * Calcule le prix réduit basé sur la promotion
   */
  calculateDiscountedPrice(originalPrice: number, promotionPercentage: number): number {
    const discount = originalPrice * (promotionPercentage / 100);
    return Math.round((originalPrice - discount) * 100) / 100;
  }

  /**
   * Calcule le montant de réduction
   */
  calculateDiscountAmount(originalPrice: number, promotionPercentage: number): number {
    return Math.round(originalPrice * (promotionPercentage / 100) * 100) / 100;
  }

  /**
   * Vérifie si une promotion est valide (actuelle et active)
   */
  isPromotionValid(promotion: any): boolean {
    const now = new Date();
    const startDate = new Date(promotion.dateDebut || promotion.DateDebut);
    const expirationDate = new Date(promotion.dateExpiration || promotion.DateExpiration);

    return promotion.estActive &&
           startDate <= now &&
           expirationDate >= now;
  }

  /**
   * Calcule les jours restants pour une promotion
   */
  getDaysRemaining(expirationDate: Date | string): number {
    const expDate = new Date(expirationDate);
    const now = new Date();
    const diffTime = expDate.getTime() - now.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return Math.max(diffDays, 0);
  }

  /**
   * Enrichit les données de promotion avec le statut
   */
  enrichPromotionData(promotion: any): PromotionWithStatus {
    const isValid = this.isPromotionValid(promotion);
    const expirationDate = new Date(promotion.dateExpiration || promotion.DateExpiration);

    return {
      id: promotion.idPromo || promotion.id || promotion.Id,
      code: promotion.codePromo || promotion.CodePromo,
      percentage: promotion.pourcentage || promotion.Pourcentage,
      startDate: new Date(promotion.dateDebut || promotion.DateDebut),
      expirationDate: expirationDate,
      isActive: promotion.estActive || promotion.EstActive,
      isValid,
      daysRemaining: this.getDaysRemaining(expirationDate)
    };
  }

  /**
   * Trie les promotions par meilleure réduction
   */
  sortByBestDiscount(promotions: any[]): any[] {
    return promotions.sort((a, b) => {
      const percentA = a.pourcentage || a.Pourcentage || 0;
      const percentB = b.pourcentage || b.Pourcentage || 0;
      return percentB - percentA;
    });
  }

  /**
   * Filtre les promotions actives
   */
  filterActivePromotions(promotions: any[]): any[] {
    return promotions.filter(p => this.isPromotionValid(p));
  }

  /**
   * Filtre les promotions expirées
   */
  filterExpiredPromotions(promotions: any[]): any[] {
    return promotions.filter(p => !this.isPromotionValid(p));
  }
}
