import { Injectable } from '@angular/core';
import { ApiService } from '../../core/services/api.service';
import { Observable, map } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private endpoint = '/products';

  constructor(private apiService: ApiService) { }

  /**
   * Unwrap the common response wrapper and return the raw result.
   */
  private unwrapResult<T>(response: any): T {
    if (response == null) {
      return response;
    }
    if (response.Result !== undefined) {
      return response.Result;
    }
    if (response.result !== undefined) {
      return response.result;
    }
    return response;
  }

  /**
   * Get all products
   * The backend returns: { IsSuccess, Result, Message }
   */
  getProducts(): Observable<any> {
    return this.apiService.get<any>(this.endpoint).pipe(
      map(response => this.unwrapResult<any>(response))
    );
  }

  /**
   * Get product by ID
   */
  getProductById(id: string): Observable<any> {
    return this.apiService.get<any>(`${this.endpoint}/${id}`).pipe(
      map(response => this.unwrapResult<any>(response))
    );
  }

  /**
   * Create a new product
   */
  createProduct(productData: any): Observable<any> {
    return this.apiService.post<any>(this.endpoint, productData);
  }

  /**
   * Update an existing product - Backend uses POST for both create/update
   */
  updateProduct(id: string, productData: any): Observable<any> {
    return this.apiService.post<any>(this.endpoint, productData);
  }

  /**
   * Delete (deactivate) a product
   * Note: The API uses PUT /:id/deactivate, not DELETE
   */
  deleteProduct(id: string): Observable<any> {
    // Using the deactivate endpoint instead of delete
    return this.apiService.put<any>(`${this.endpoint}/${id}/deactivate`, {});
  }

  /**
   * Activate a product
   */
  activateProduct(id: string): Observable<any> {
    return this.apiService.put<any>(`${this.endpoint}/${id}/activate`, {});
  }

  /**
   * Archive a product
   */
  archiveProduct(id: string): Observable<any> {
    return this.apiService.put<any>(`${this.endpoint}/${id}/archive`, {});
  }
}
