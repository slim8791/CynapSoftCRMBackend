import { Injectable } from '@angular/core';
import { ApiService } from '../../core/services/api.service';
import { Observable } from 'rxjs';

export interface FichierDto {
  idFichier?: number;
  nomFichier: string;
  url: string;
  extension: string;
  taille: number;
  idSupport: number;
}

export interface SupportMarketingDto {
  idSupportMarketting?: number;
  type: string;
  idProduit: number;
  isActive?: boolean;
  campaignName?: string;
  fichiers?: FichierDto[];
}

@Injectable({
  providedIn: 'root'
})
export class MarketingService {

  private baseUrl = '/products/marketting';

  constructor(private apiService: ApiService) {}

  /**
   * Get all marketing supports by product ID
   */
  getSupportsByProductId(productId: number): Observable<any> {
    return this.apiService.get<any>(`${this.baseUrl}/product/${productId}/supports`);
  }

  /**
   * Get marketing support by ID
   */
  getSupportById(supportId: number): Observable<any> {
    return this.apiService.get<any>(`${this.baseUrl}/support/${supportId}`);
  }

  /**
   * Create or update marketing support
   */
  createOrUpdateSupport(payload: SupportMarketingDto): Observable<any> {
    return this.apiService.post<any>(`${this.baseUrl}/support`, payload);
  }

  /**
   * Delete marketing support
   */
  deleteSupport(supportId: number): Observable<any> {
    return this.apiService.delete<any>(`${this.baseUrl}/support/${supportId}`);
  }

  /**
   * Add file to marketing support
   */
  addFileToSupport(fichierDto: FichierDto): Observable<any> {
    return this.apiService.post<any>(`${this.baseUrl}/support/file`, fichierDto);
  }

  /**
   * Delete file from marketing support
   */
  deleteFile(fileId: number): Observable<any> {
    return this.apiService.delete<any>(`${this.baseUrl}/file/${fileId}`);
  }

  /**
   * Get files by support ID
   */
  getFilesBySupport(supportId: number): Observable<any> {
    return this.apiService.get<any>(`${this.baseUrl}/support/${supportId}/files`);
  }
}
