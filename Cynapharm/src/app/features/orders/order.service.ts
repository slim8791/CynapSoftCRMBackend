import { Injectable } from '@angular/core';
import { ApiService } from '../../core/services/api.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private endpoint = '/orders';

  constructor(private apiService: ApiService) { }

  getOrders(): Observable<any[]> {
    return this.apiService.get<any[]>(this.endpoint);
  }

  getOrderById(id: string): Observable<any> {
    return this.apiService.get<any>(`${this.endpoint}/${id}`);
  }

  createOrder(orderData: any): Observable<any> {
    return this.apiService.post<any>(this.endpoint, orderData);
  }

  updateOrder(id: string, orderData: any): Observable<any> {
    return this.apiService.put<any>(`${this.endpoint}/${id}`, orderData);
  }

  deleteOrder(id: string): Observable<any> {
    return this.apiService.delete<any>(`${this.endpoint}/${id}`);
  }
}
