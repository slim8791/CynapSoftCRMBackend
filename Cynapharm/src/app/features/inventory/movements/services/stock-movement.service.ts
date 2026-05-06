import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../../core/services/api.service';

export interface StockMovementDto {
  id_Movement?: number;
  id_Stock:     number;
  quantite:     number;
  typeMovement: string;
  dateMovement?: string;
  description?: string;
}

@Injectable({ providedIn: 'root' })
export class StockMovementService {
  private readonly base = '/inventory/stock-movements';
  constructor(private api: ApiService) {}
  private u<T>(r: any): T { return r?.Result ?? r?.result ?? r; }

  getMovements(idStock: number): Observable<StockMovementDto[]> {
    return this.api.get<any>(`${this.base}/${idStock}`).pipe(map(r => this.u<StockMovementDto[]>(r) ?? []));
  }
  decrement(idStock: number, qte: number): Observable<boolean> {
    return this.api.post<any>(`${this.base}/decrement?idStock=${idStock}&qte=${qte}`, {}).pipe(map(r => this.u<boolean>(r)));
  }
  increment(idStock: number, qte: number): Observable<boolean> {
    return this.api.post<any>(`${this.base}/increment?idStock=${idStock}&qte=${qte}`, {}).pipe(map(r => this.u<boolean>(r)));
  }
  transfer(idSource: number, idDest: number, qte: number): Observable<boolean> {
    return this.api.post<any>(`${this.base}/transfer?idStockSource=${idSource}&idStockDestination=${idDest}&qte=${qte}`, {}).pipe(map(r => this.u<boolean>(r)));
  }
}
