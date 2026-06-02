import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../../core/services/api.service';

export interface StockSummaryDto {
  TotalProduits:      number;
  TotalQteDisponible: number;
  StocksVides:        number;
  StocksFaibles:      number;
  TotalDistributions: number;
  TotalQteDistribuee: number;
  DernierMouvement:   string;
}

@Injectable({ providedIn: 'root' })
export class InventoryBusinessService {
  private readonly base = '/inventory/inventory-business';
  constructor(private api: ApiService) {}
  private u<T>(r: any): T { return r?.Result ?? r?.result ?? r; }

  getStockSummary(idDelegue: number): Observable<StockSummaryDto> {
    return this.api.get<any>(`${this.base}/summary/${idDelegue}`).pipe(map(r => this.u<StockSummaryDto>(r)));
  }

  checkAvailability(idStock: number, qte: number): Observable<boolean> {
    return this.api.get<any>(`${this.base}/check-availability?idStock=${idStock}&qte=${qte}`).pipe(map(r => this.u<boolean>(r) ?? false));
  }

  distributeEchantillon(idDelegue: number, idPharmacien: number | null, idMedecin: number | null, idStock: number, qte: number): Observable<boolean> {
    let url = `${this.base}/distribute-echantillon?idDelegue=${idDelegue}&idStock=${idStock}&qte=${qte}`;
    if (idPharmacien) url += `&idPharmacien=${idPharmacien}`;
    if (idMedecin)    url += `&idMedecin=${idMedecin}`;
    return this.api.post<any>(url, {}).pipe(map(r => this.u<boolean>(r) ?? false));
  }

  applyGratuite(idStock: number, quantiteAchetee: number, seuilPromo: number): Observable<boolean> {
    const url = `${this.base}/apply-gratuite?idStock=${idStock}&quantiteAchetee=${quantiteAchetee}&seuilPromo=${seuilPromo}`;
    return this.api.post<any>(url, {}).pipe(map(r => this.u<boolean>(r) ?? false));
  }

  reserveStock(idStock: number, quantite: number): Observable<boolean> {
    const url = `${this.base}/reserve-stock?idStock=${idStock}&quantite=${quantite}`;
    return this.api.post<any>(url, {}).pipe(map(r => this.u<boolean>(r) ?? false));
  }
}
