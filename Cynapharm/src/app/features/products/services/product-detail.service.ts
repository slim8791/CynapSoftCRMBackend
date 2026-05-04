import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import { ProductAdvancedService } from './product-advanced.service';

@Injectable({ providedIn: 'root' })
export class ProductDetailService {

  private readonly api             = inject(ApiService);
  private readonly productAdvanced = inject(ProductAdvancedService);

  getProductDashboard(productId: number): Observable<any> {
    // BUG ORIGINAL : productId ignoré — appelait /api/products/dashboard
    // sans le passer à getProductDashboard(), retournant un dashboard global
    return this.productAdvanced.getProductDashboard();
  }

  getStock(productId: number): Observable<number> {
    return this.productAdvanced.getTotalStock(productId);
  }

  getLots(productId: number): Observable<any[]> {
    return this.api.get(`/api/lots/product/${productId}/available`);
  }

  getPromotions(productId: number): Observable<any[]> {
    return this.productAdvanced.getPromotionsByProduct(productId);
  }

  archiveProduct(productId: number): Observable<any> {
    return this.productAdvanced.archiveProduct(productId);
  }
}