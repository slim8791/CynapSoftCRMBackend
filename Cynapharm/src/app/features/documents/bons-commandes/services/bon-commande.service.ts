import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../../core/services/api.service';

export interface BonCommandeDto {
  numero_Doc:    number;
  nom_Doc?:      string;
  id_Client?:    number;
  id_Commande?:  number;
  dateCreation?: string;
  typeDocument?: string;
  cloudinaryUrl?: string;
  url?:          string;
}

@Injectable({ providedIn: 'root' })
export class BonCommandeService {
  private readonly base = '/documents/bons-commandes';
  constructor(private api: ApiService) {}
  private u<T>(r: any): T { return r?.Result ?? r?.result ?? r; }

  getAll(pageNumber = 1, pageSize = 20): Observable<BonCommandeDto[]> {
    const p = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.api.get<any>(this.base, p).pipe(map(r => this.u<BonCommandeDto[]>(r) ?? []));
  }
  getById(id: number)        { return this.api.get<any>(`${this.base}/${id}`).pipe(map(r => this.u<BonCommandeDto>(r))); }
  getByClient(id: number)    { return this.api.get<any>(`${this.base}/client/${id}`).pipe(map(r => this.u<BonCommandeDto[]>(r) ?? [])); }
  getByDate(start: string, end: string): Observable<BonCommandeDto[]> {
    const p = new HttpParams().set('startDate', start).set('endDate', end);
    return this.api.get<any>(`${this.base}/by-date`, p).pipe(map(r => this.u<BonCommandeDto[]>(r) ?? []));
  }
  getByCommande(id: number): Observable<BonCommandeDto[]> {
    return this.api.get<any>(`${this.base}/commande/${id}`).pipe(map(r => this.u<BonCommandeDto[]>(r) ?? []));
  }
  createOrUpdate(dto: BonCommandeDto): Observable<BonCommandeDto> {
    return this.api.post<any>(`${this.base}/createUpdate`, dto).pipe(map(r => this.u<BonCommandeDto>(r)));
  }
  delete(id: number): Observable<void> { return this.api.delete<void>(`${this.base}/${id}`); }
}
