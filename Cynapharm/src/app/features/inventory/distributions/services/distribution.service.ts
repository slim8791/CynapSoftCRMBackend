import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../../core/services/api.service';

export interface EchantillonDto {
  id_Distribution?: number;
  id_Delegue:       number;
  id_Medecin?:      number | null;
  id_Pharmacien?:   number | null;
  id_Stock:         number;
  qte:              number;
  numeroLot:        string;
  dateDistribution?: string;
}

@Injectable({ providedIn: 'root' })
export class DistributionService {
  private readonly base = '/inventory/distributions';
  constructor(private api: ApiService) {}
  private u<T>(r: any): T { return r?.Result ?? r?.result ?? r; }

  getById(id: number)              { return this.api.get<any>(`${this.base}/${id}`).pipe(map(r => this.u<EchantillonDto>(r))); }
  getByMedecin(id: number)         { return this.api.get<any>(`${this.base}/by-medecin/${id}`).pipe(map(r => this.u<EchantillonDto[]>(r) ?? [])); }
  getByDelegue(id: number)         { return this.api.get<any>(`${this.base}/by-delegue/${id}`).pipe(map(r => this.u<EchantillonDto[]>(r) ?? [])); }
  getByPharmacien(id: number)      { return this.api.get<any>(`${this.base}/by-pharmacien/${id}`).pipe(map(r => this.u<EchantillonDto[]>(r) ?? [])); }
  createOrUpdate(dto: EchantillonDto): Observable<EchantillonDto> {
    return this.api.post<any>(`${this.base}/distribution`, dto).pipe(map(r => this.u<EchantillonDto>(r)));
  }
  delete(id: number): Observable<void> { return this.api.delete<void>(`${this.base}/${id}`); }
}
