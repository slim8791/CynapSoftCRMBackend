import { Injectable } from '@angular/core';
import { ApiService } from '../../core/services/api.service';
import { Observable } from 'rxjs';

export interface LotDto {
  numero: string;
  dateExpiration: string; // ISO 8601 format
  quantite: number;
  idProduit: number;
  isExpired?: boolean;
  isOutOfStock?: boolean;
  promotions?: any[];
}

@Injectable({
  providedIn: 'root'
})
export class LotService {

  private baseUrl = '/products/lots';

  constructor(private apiService: ApiService) {}

  /**
   * Get all lots (GetAllLotsAsync backend)
   */
  getAllLots(): Observable<any> {
    return this.apiService.get<any>(`${this.baseUrl}`);
  }

  /**
   * Get all lots by product ID
   */
  getLotsByProductId(productId: number): Observable<any> {
    return this.apiService.get<any>(`${this.baseUrl}/${productId}/lots`);
  }

  /**
   * Create or update a lot
   */
  createOrUpdateLot(payload: LotDto): Observable<any> {
    return this.apiService.post<any>(`${this.baseUrl}/lot`, payload);
  }

  /**
   * Delete a lot
   */
  deleteLot(numeroLot: string): Observable<any> {
    return this.apiService.delete<any>(`${this.baseUrl}/lot/${numeroLot}`);
  }

  /**
   * Get single lot by numero (new endpoint)
   */
  getLotByNumero(numeroLot: string): Observable<any> {
    return this.apiService.get<any>(`${this.baseUrl}/lot/${numeroLot}`);
  }
}
