import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../../core/services/api.service';
import { StockType } from '../../../../core/models/enums';

export interface StockDelegueDto {
  id_stock?:       number;
  id_User_Delegue: number;
  id_Produit:      number;
  numeroLot:       string;
  dateExpiration:  string;
  qteDisponible:   number;
  qteReservee:     number;
}

@Injectable({ providedIn: 'root' })
export class StockService {
  private readonly base = '/inventory/stocks-delegue';
  constructor(private api: ApiService) {}
  private u<T>(r: any): T { return r?.Result ?? r?.result ?? r; }

  getAll(pageNumber = 1, pageSize = 20): Observable<StockDelegueDto[]> {
    const p = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.api.get<any>(this.base, p).pipe(map(r => this.u<StockDelegueDto[]>(r) ?? []));
  }
  getById(id: number)             { return this.api.get<any>(`${this.base}/${id}`).pipe(map(r => this.u<StockDelegueDto>(r))); }
  getByDelegue(id: number)        { return this.api.get<any>(`${this.base}/by-delegue/${id}`).pipe(map(r => this.u<StockDelegueDto[]>(r) ?? [])); }
  getByProduit(id: number)        { return this.api.get<any>(`${this.base}/by-produit/${id}`).pipe(map(r => this.u<StockDelegueDto[]>(r) ?? [])); }
  getByLot(numero: string)        { return this.api.get<any>(`${this.base}/by-lot/${numero}`).pipe(map(r => this.u<StockDelegueDto>(r))); }
  createOrUpdate(dto: StockDelegueDto): Observable<StockDelegueDto> {
    return this.api.post<any>(this.base, dto).pipe(map(r => this.u<StockDelegueDto>(r)));
  }
  delete(id: number, type: StockType): Observable<void> {
    return this.api.delete<void>(`${this.base}/${id}?type=${type}`);
  }
}
