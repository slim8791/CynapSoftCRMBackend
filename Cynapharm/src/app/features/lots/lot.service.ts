// ── lot.service.ts ───────────────────────────────────────────────────────────
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { ApiService } from '../../core/services/api.service';
import { LotDto, LotPayload } from './lot.model';

/** Wrapper générique retourné par l'API */
interface ApiResponse<T> {
  Result?: T;
  result?: T;
}

@Injectable({ providedIn: 'root' })
export class LotService {

  private readonly baseUrl = '/products/lots';

  constructor(private apiService: ApiService) {}

  // ── Lecture ────────────────────────────────────────────────────────────────

  /** Récupère tous les lots (GetAllLotsAsync) */
  getAllLots(): Observable<LotDto[]> {
    return this.apiService
      .get<ApiResponse<LotDto[]>>(this.baseUrl)
      .pipe(map(res => this.unwrap(res)));
  }

  /** Récupère les lots d'un produit donné */
  getLotsByProductId(productId: number): Observable<LotDto[]> {
    return this.apiService
      .get<ApiResponse<LotDto[]>>(`${this.baseUrl}/product/${productId}`)
      .pipe(map(res => this.unwrap(res)));
  }

  /** Récupère un lot par son numéro (normalise PascalCase → camelCase) */
  getLotByNumero(numeroLot: string): Observable<LotDto> {
    return this.apiService
      .get<ApiResponse<LotDto>>(`${this.baseUrl}/${numeroLot}`)
      .pipe(map(res => {
        const raw: any = res?.Result ?? res?.result ?? res;
        return {
          numero:         raw.numero         ?? raw.Numero         ?? '',
          quantite:       raw.quantite       ?? raw.Quantite       ?? 0,
          dateExpiration: raw.dateExpiration ?? raw.DateExpiration ?? null,
          idProduit:      raw.idProduit      ?? raw.IdProduit      ?? (raw as any).id_Produit ?? 0,
          isExpired:      raw.isExpired      ?? raw.IsExpired      ?? false,
          isOutOfStock:   raw.isOutOfStock   ?? raw.IsOutOfStock   ?? false,
          promotions:     raw.promotions     ?? raw.Promotions     ?? [],
        } as LotDto;
      }));
  }

  // ── Écriture ───────────────────────────────────────────────────────────────

  /**
   * Crée ou met à jour un lot.
   * Transforme le LotDto (camelCase) en LotPayload (id_Produit) pour le backend.
   * Exclut les champs calculés (isExpired, isOutOfStock) qui sont recalculés côté backend.
   */
  createOrUpdateLot(lot: LotDto): Observable<LotDto> {
    const payload: LotPayload = {
      numero:          (lot.numero || '').trim(),
      dateExpiration:  lot.dateExpiration || '',
      quantite:        Number(lot.quantite) || 0,
      id_Produit:      Number(lot.idProduit) || 0,   // S'assurer que c'est un nombre
    };
    return this.apiService.post<LotDto>(`${this.baseUrl}/lot`, payload);
  }

  /** Supprime un lot par son numéro */
  deleteLot(numeroLot: string): Observable<void> {
    return this.apiService.delete<void>(`${this.baseUrl}/${numeroLot}`);
  }

  // ── Helpers privés ─────────────────────────────────────────────────────────

  /**
   * Désemballe la réponse API (gère Result / result / tableau direct).
   * Normalise également le casing Pascal→camel pour idProduit.
   */
  private unwrap(res: ApiResponse<LotDto[]> | LotDto[]): LotDto[] {
    const raw = (res as ApiResponse<LotDto[]>)?.Result
             ?? (res as ApiResponse<LotDto[]>)?.result
             ?? res as LotDto[];

    if (!Array.isArray(raw)) return [];

    return raw.map(lot => ({
      ...lot,
      // CamelCase policy: Id_Produit → id_Produit, Numero → numero, etc.
      numero:         lot.numero         ?? (lot as any).Numero         ?? '',
      dateExpiration: lot.dateExpiration ?? (lot as any).DateExpiration ?? '',
      quantite:       lot.quantite       ?? (lot as any).Quantite       ?? 0,
      idProduit:      (lot as any)['id_Produit'] ?? lot.idProduit ?? (lot as any).Id_Produit ?? 0,
      isExpired:      lot.isExpired      ?? (lot as any).IsExpired      ?? false,
      isOutOfStock:   lot.isOutOfStock   ?? (lot as any).IsOutOfStock   ?? false,
    }));
  }
}