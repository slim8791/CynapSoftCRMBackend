import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../core/services/api.service';

@Injectable({
  providedIn: 'root'
})
export class ProductAdvancedService {

  constructor(private api: ApiService) {}

  getProductDashboard(): Observable<any> {
    return this.api.get('/api/products/dashboard');
  }

  getPromotionsByProduct(productId: number): Observable<any[]> {
    return this.api.get(`/api/promos/product/${productId}`);
  }

  getTotalStock(productId: number): Observable<number> {
    return this.api.get(`/api/products/${productId}/stock`).pipe(
      map((response: any) => response.Result ?? 0)
    );
  }

  archiveProduct(productId: number): Observable<any> {
    return this.api.put(`/api/products/${productId}/archive`, {});
  }

  getAvailableProducts(): Observable<any[]> {
    return this.api.get('/api/products/available');
  }

  getLowStockProducts(seuil: number): Observable<any[]> {
    return this.api.get(`/api/products/low-stock?seuil=${seuil}`);
  }

  searchProducts(keyword: string): Observable<any[]> {
    return this.api.get(`/api/products/search?keyword=${keyword}`);
  }

  getCategories(): Observable<string[]> {
    return this.api.get('/api/products/categories');
  }
}

