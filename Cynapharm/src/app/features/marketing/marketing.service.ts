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
  getSupportsByProductId(productId: number): Observable<any> {
    return this.apiService.get<any>(`${this.baseUrl}/product/${productId}/supports`).pipe(
      catchError(err => {
        if (err.status === 404) return of({ Result: [], IsSuccess: true });
        throw err;
      })
    );
  }

  /** GET /products/marketting/support/{supportId} */
  getSupportById(supportId: number): Observable<any> {
    return this.apiService.get<any>(`${this.baseUrl}/support/${supportId}`);
  }

  /** POST /products/marketting/support
   *  Envoie un SupportMarketingDto avec les noms de champs PascalCase
   *  pour correspondre au modèle C# attendu par AutoMapper.
   */
  createOrUpdateSupport(payload: SupportMarketingDto): Observable<any> {
    return this.apiService.post<any>(`${this.baseUrl}/support`, payload);
  }

  /** DELETE /products/marketting/support/{supportId} */
  deleteSupport(supportId: number): Observable<any> {
    return this.apiService.delete<any>(`${this.baseUrl}/support/${supportId}`);
  }

  // ── Visibilité ────────────────────────────────────────────────────────────

  /** PUT /products/marketting/support/{supportId}/disable */
  disableSupport(supportId: number): Observable<any> {
    return this.apiService.put<any>(`${this.baseUrl}/support/${supportId}/disable`, {});
  }

  /** PUT /products/marketting/support/{supportId}/activate */
  activateSupport(supportId: number): Observable<any> {
    return this.apiService.put<any>(`${this.baseUrl}/support/${supportId}/activate`, {});
  }

  /** GET /products/marketting/product/{productId}/visible-supports */
  getVisibleSupportsByProductId(productId: number): Observable<any> {
    return this.apiService.get<any>(`${this.baseUrl}/product/${productId}/visible-supports`);
  }

  // ── Fichiers ──────────────────────────────────────────────────────────────

  /** GET /products/marketting/support/{supportId}/files */
  getFilesBySupport(supportId: number): Observable<any> {
    return this.apiService.get<any>(`${this.baseUrl}/support/${supportId}/files`);
  }

  /** POST /products/marketting/support/file
   *  FichierDto doit avoir les champs PascalCase : NomFichier, Url, Extension, Taille, Id_Support
   */
  addFileToSupport(fichierDto: FichierDto): Observable<any> {
    return this.apiService.post<any>(`${this.baseUrl}/support/file`, fichierDto);
  }

  /** DELETE /products/marketting/file/{fichierId} */
  deleteFile(fileId: number): Observable<any> {
    return this.apiService.delete<any>(`${this.baseUrl}/file/${fileId}`);
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