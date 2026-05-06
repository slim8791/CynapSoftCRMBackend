import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../../core/services/api.service';

export interface BonLivraisonDto {
  id?:            number;
  id_Client:      number;
  dateLivraison?: string;
  adresseLivraison: string;
}

@Injectable({ providedIn: 'root' })
export class BonLivraisonService {
  private readonly base = '/documents/bons-livraison';
  constructor(private api: ApiService) {}
  private u<T>(r: any): T { return r?.Result ?? r?.result ?? r; }

  getAll(pageNumber = 1, pageSize = 20): Observable<BonLivraisonDto[]> {
    const p = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.api.get<any>(this.base, p).pipe(map(r => this.u<BonLivraisonDto[]>(r) ?? []));
  }
  getById(id: number)     { return this.api.get<any>(`${this.base}/${id}`).pipe(map(r => this.u<BonLivraisonDto>(r))); }
  getByClient(id: number) { return this.api.get<any>(`${this.base}/ByClient/${id}`).pipe(map(r => this.u<BonLivraisonDto[]>(r) ?? [])); }
  getByDate(start: string, end: string): Observable<BonLivraisonDto[]> {
    const p = new HttpParams().set('startDate', start).set('endDate', end);
    return this.api.get<any>(`${this.base}/by-date`, p).pipe(map(r => this.u<BonLivraisonDto[]>(r) ?? []));
  }
  createOrUpdate(dto: BonLivraisonDto): Observable<BonLivraisonDto> {
    return this.api.post<any>(`${this.base}/createUpdate`, dto).pipe(map(r => this.u<BonLivraisonDto>(r)));
  }
}
