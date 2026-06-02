import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import { ProductAdvancedService } from './product-advanced.service';

@Injectable({ providedIn: 'root' })
export class ProductDetailService {

  private readonly api             = inject(ApiService);
  private readonly productAdvanced = inject(ProductAdvancedService);

  getProductDashboard(productId: number): Observable<any> {
    return this.api.get<any>(`/products/${productId}/dashboard`);
  }

  getStock(productId: number): Observable<number> {
    return this.productAdvanced.getTotalStock(productId);
  }

  getLots(productId: number): Observable<any[]> {
    return this.api.get(`/products/lots/available/${productId}`);
  }

  getPromotions(productId: number): Observable<any[]> {
    return this.productAdvanced.getPromotionsByProduct(productId);
  }

  archiveProduct(productId: number): Observable<any> {
    return this.productAdvanced.archiveProduct(productId);
  }
}