import { Injectable } from '@angular/core';
import { ApiService } from '../../core/services/api.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private endpoint = '/products';

  constructor(private apiService: ApiService) { }

  getProducts(): Observable<any[]> {
    return this.apiService.get<any[]>(this.endpoint);
  }

  getProductById(id: string): Observable<any> {
    return this.apiService.get<any>(`${this.endpoint}/${id}`);
  }

  createProduct(productData: any): Observable<any> {
    return this.apiService.post<any>(this.endpoint, productData);
  }

  updateProduct(id: string, productData: any): Observable<any> {
    return this.apiService.put<any>(`${this.endpoint}/${id}`, productData);
  }

  deleteProduct(id: string): Observable<any> {
    return this.apiService.delete<any>(`${this.endpoint}/${id}`);
  }
}
