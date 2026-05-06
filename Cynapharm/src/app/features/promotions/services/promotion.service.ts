import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../core/services/api.service';
import { ResponseDto } from '../../../core/models/response-dto.model';

export interface PromotionDto {
  id_Promo?:      number;
  codePromo:      string;
  pourcentage:    number;
  dateDebut:      string;
  dateExpiration: string;
  estActive:      boolean;
  numeroLot:      string;
  isValid?:       boolean;
}

@Injectable({ providedIn: 'root' })
export class PromotionService {

  private readonly base = '/products/promos';

  constructor(private api: ApiService) {}

  private unwrap<T>(r: any): T {
    if (r?.Result !== undefined) return r.Result;
    if (r?.result !== undefined) return r.result;
    return r;
  }

  private normalize(r: any): PromotionDto {
    return {
      id_Promo:       r.id_Promo       ?? r.Id_Promo       ?? r.idPromo       ?? undefined,
      codePromo:      r.codePromo      ?? r.CodePromo      ?? '',
      pourcentage:    r.pourcentage    ?? r.Pourcentage    ?? 0,
      dateDebut:      r.dateDebut      ?? r.DateDebut      ?? '',
      dateExpiration: r.dateExpiration ?? r.DateExpiration ?? '',
      estActive:      r.estActive      ?? r.EstActive      ?? false,
      numeroLot:      r.numeroLot      ?? r.NumeroLot      ?? '',
      isValid:        r.isValid        ?? r.IsValid        ?? false,
    };
  }

  getAll(): Observable<PromotionDto[]> {
    return this.api.get<any>(this.base).pipe(
      map(r => {
        const raw = this.unwrap<any[]>(r) ?? [];
        return Array.isArray(raw) ? raw.map(p => this.normalize(p)) : [];
      })
    );
  }

  getById(id: number): Observable<PromotionDto> {
    return this.api.get<any>(`${this.base}/${id}`).pipe(
      map(r => this.normalize(this.unwrap<any>(r)))
    );
  }

  getActiveCount(): Observable<number> {
    return this.api.get<any>(`${this.base}/active-count`).pipe(map(r => this.unwrap<number>(r) ?? 0));
  }

  getCoverageRate(): Observable<number> {
    return this.api.get<any>(`${this.base}/coverage-rate`).pipe(map(r => this.unwrap<number>(r) ?? 0));
  }

  isValid(id: number): Observable<boolean> {
    return this.api.get<any>(`${this.base}/${id}/valid`).pipe(map(r => this.unwrap<boolean>(r) ?? false));
  }

  isApplicable(id: number, referenceDate: string): Observable<boolean> {
    const params = new HttpParams().set('referenceDate', referenceDate);
    return this.api.get<any>(`${this.base}/${id}/applicable`, params).pipe(map(r => this.unwrap<boolean>(r) ?? false));
  }

  getByProduct(productId: number): Observable<PromotionDto[]> {
    return this.api.get<any>(`${this.base}/product/${productId}`).pipe(map(r => this.unwrap<PromotionDto[]>(r) ?? []));
  }

  isProductInPromotion(productId: number): Observable<boolean> {
    return this.api.get<any>(`${this.base}/product/${productId}/in-promotion`).pipe(map(r => this.unwrap<boolean>(r) ?? false));
  }

  applyBest(productId: number, initialPrice: number): Observable<number> {
    const params = new HttpParams().set('initialPrice', String(initialPrice));
    return this.api.get<any>(`${this.base}/product/${productId}/apply`, params).pipe(map(r => this.unwrap<number>(r) ?? initialPrice));
  }

  getByLot(numeroLot: string): Observable<PromotionDto[]> {
    return this.api.get<any>(`${this.base}/lot/${numeroLot}`).pipe(map(r => this.unwrap<PromotionDto[]>(r) ?? []));
  }

  createOrUpdate(dto: PromotionDto): Observable<PromotionDto> {
    return this.api.post<any>(this.base, dto).pipe(map(r => this.unwrap<PromotionDto>(r)));
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`${this.base}/${id}`);
  }
}
