import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../../core/services/api.service';

export interface StockGratuiteDto {
  id_stock?:          number;
  id_User_Delegue:    number;
  id_Produit:         number;
  numeroLot:          string;
  qteDisponible:      number;
  qteReservee:        number;
  qteGratuite:        number;
  typePromotion:      string;
  quantiteAchat?:     number;
  quantiteGratuite?:  number;
  dateDebut?:         string | null;
  dateFin?:           string | null;
}

export interface StockEchantillonDto {
  id_stock?:       number;
  id_User_Delegue: number;
  id_Produit:      number;
  numeroLot:       string;
  qteDisponible:   number;
  qteReservee:     number;
  qteEchantillon:  number;
  description?:    string | null;
  dateDebut?:      string | null;
  dateFin?:        string | null;
}

@Injectable({ providedIn: 'root' })
export class PromoStockService {
  private readonly base = '/inventory/stocks-promotionnels';
  constructor(private api: ApiService) {}
  private u<T>(r: any): T { return r?.Result ?? r?.result ?? r; }

  createOrUpdateGratuite(dto: StockGratuiteDto): Observable<StockGratuiteDto> {
    return this.api.post<any>(`${this.base}/Gratuite`, dto).pipe(map(r => this.u<StockGratuiteDto>(r)));
  }
  getGratuite(idStock: number): Observable<StockGratuiteDto> {
    return this.api.get<any>(`${this.base}/gratuite/${idStock}`).pipe(map(r => this.u<StockGratuiteDto>(r)));
  }
  createOrUpdateEchantillon(dto: StockEchantillonDto): Observable<StockEchantillonDto> {
    return this.api.post<any>(`${this.base}/echantillon`, dto).pipe(map(r => this.u<StockEchantillonDto>(r)));
  }
  getEchantillon(idStock: number): Observable<StockEchantillonDto> {
    return this.api.get<any>(`${this.base}/echantillon/${idStock}`).pipe(map(r => this.u<StockEchantillonDto>(r)));
  }
  getAllGratuite(page = 1, size = 50): Observable<StockGratuiteDto[]> {
    return this.api.get<any>(`${this.base}/gratuite?pageNumber=${page}&pageSize=${size}`)
      .pipe(map(r => this.u<StockGratuiteDto[]>(r) ?? []));
  }
  getAllEchantillon(page = 1, size = 50): Observable<StockEchantillonDto[]> {
    return this.api.get<any>(`${this.base}/echantillon?pageNumber=${page}&pageSize=${size}`)
      .pipe(map(r => this.u<StockEchantillonDto[]>(r) ?? []));
  }
}
