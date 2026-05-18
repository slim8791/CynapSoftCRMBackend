// ── marketing.service.ts ─────────────────────────────────────────────────────
// Route backend C# : [Route("api/marketting")]  ← deux 't'
// L'API Gateway préfixe avec '/products' → URL finale : /products/marketting/*
//
// CORRECTION : baseUrl = '/products/marketting'  ✅  (était déjà correct)
// Le vrai problème était que l'API Gateway routait '/products/marketting'
// vers un mauvais microservice (promos au lieu de ProductAPI).
// Vérifiez votre configuration Ocelot / YARP et assurez-vous que :
//   /products/marketting/** → ProductAPI (port 5xxx)
//   /products/promos/**     → PromotionAPI (ou ProductAPI selon votre archi)

import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ApiService } from '../../core/services/api.service';

// ── DTOs (miroir EXACT des modèles C# — PascalCase) ─────────────────────────
// Les noms de propriétés correspondent au JSON sérialisé par ASP.NET Core.
// Si vous avez JsonNamingPolicy.CamelCase dans Program.cs, utilisez la version
// camelCase commentée ci-dessous — sinon gardez PascalCase.

export interface FichierDto {
  Id_Fichier?:  number;
  NomFichier:   string;
  Url:          string;
  Extension:    string;
  Taille:       number;
  Id_Support:   number;     // FK → Support_Marketting.Id_SupportMarketting
}

export interface SupportMarketingDto {
  // PascalCase (ASP.NET default)
  Id_SupportMarketting?: number;
  Type?:                 string;
  Id_Produit?:           number;
  IsActive?:             boolean;
  CampaignName?:         string;
  Fichiers?:             FichierDto[];
  // camelCase (si JsonNamingPolicy.CamelCase activé)
  id_SupportMarketting?: number;
  type?:                 string;
  idProduit?:            number;
  isActive?:             boolean;
  campaignName?:         string;
  fichiers?:             FichierDto[];
}

// ── Version camelCase (si JsonNamingPolicy.CamelCase est activé) ──────────────
// export interface FichierDto {
//   idFichier?: number; nomFichier: string; url: string;
//   extension: string; taille: number; idSupport: number;
// }
// export interface SupportMarketingDto {
//   idSupportMarketting?: number; type: string; idProduit: number;
//   isActive?: boolean; campaignName?: string; fichiers?: FichierDto[];
// }

// ── Service ───────────────────────────────────────────────────────────────────

@Injectable({ providedIn: 'root' })
export class MarketingService {

  /**
   * Préfixe qui correspond à la route C# [Route("api/marketting")]
   * après passage par l'API Gateway (/products → ProductAPI).
   *
   * ⚠️  Le contrôleur C# s'appelle "Marketting" (deux 't') —
   *     conservez cette orthographe pour correspondre aux routes backend.
   */
  private readonly baseUrl = '/products/marketting';

  constructor(private readonly apiService: ApiService) {}

  // ── Supports ──────────────────────────────────────────────────────────────

  /** GET /products/marketting/product/{productId}/supports
   *  Le backend retourne 404 si aucun support — on normalise en tableau vide.
   */
  /** GET /products/marketting/product/{productId} */
  getSupportsByProductId(productId: number): Observable<any> {
    return this.apiService.get<any>(`${this.baseUrl}/product/${productId}`).pipe(
      catchError(err => {
        if (err.status === 404) return of({ Result: [], IsSuccess: true });
        throw err;
      })
    );
  }

  /** GET /products/marketting/{supportId} */
  getSupportById(supportId: number): Observable<any> {
    return this.apiService.get<any>(`${this.baseUrl}/${supportId}`);
  }

  /** POST /products/marketting */
  createOrUpdateSupport(payload: SupportMarketingDto): Observable<any> {
    return this.apiService.post<any>(this.baseUrl, payload);
  }

  /** DELETE /products/marketting/{supportId} */
  deleteSupport(supportId: number): Observable<any> {
    return this.apiService.delete<any>(`${this.baseUrl}/${supportId}`);
  }

  // ── Visibilité ────────────────────────────────────────────────────────────

  /** PUT /products/marketting/{supportId}/disable */
  disableSupport(supportId: number): Observable<any> {
    return this.apiService.put<any>(`${this.baseUrl}/${supportId}/disable`, {});
  }

  /** PUT /products/marketting/{supportId}/activate */
  activateSupport(supportId: number): Observable<any> {
    return this.apiService.put<any>(`${this.baseUrl}/${supportId}/activate`, {});
  }

  /** GET /products/marketting/visible/{productId} */
  getVisibleSupportsByProductId(productId: number): Observable<any> {
    return this.apiService.get<any>(`${this.baseUrl}/visible/${productId}`);
  }

  // ── Fichiers ──────────────────────────────────────────────────────────────

  /** GET /products/marketting/{supportId} (fichiers inclus dans la réponse) */
  getFilesBySupport(supportId: number): Observable<any> {
    return this.apiService.get<any>(`${this.baseUrl}/${supportId}`);
  }

  /** POST /products/fichiers */
  addFileToSupport(fichierDto: FichierDto): Observable<any> {
    return this.apiService.post<any>('/products/fichiers', fichierDto);
  }

  /** DELETE /products/fichiers/{fichierId} */
  deleteFile(fileId: number): Observable<any> {
    return this.apiService.delete<any>(`/products/fichiers/${fileId}`);
  }

  // ── Campagnes ─────────────────────────────────────────────────────────────

  /** GET /products/marketting/campaigns */
  getCampaigns(): Observable<any> {
    return this.apiService.get<any>(`${this.baseUrl}/campaigns`);
  }

  /** GET /products/marketting/campaign/{campaignName} */
  getSupportsByCampaign(campaignName: string): Observable<any> {
    return this.apiService.get<any>(`${this.baseUrl}/campaign/${encodeURIComponent(campaignName)}`);
  }
}