import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../../core/services/api.service';

export interface RegionDto {
  id_Region?:      number;
  nomRegion:       string;
  codePostal:      string;
  id_User_Delegue: number;
}

@Injectable({ providedIn: 'root' })
export class RegionService {
  private readonly base = '/fields/regions';
  constructor(private api: ApiService) {}
  private u<T>(r: any): T { return r?.Result ?? r?.result ?? r; }

  getAll()                { return this.api.get<any>(`${this.base}/all`).pipe(map(r => this.u<RegionDto[]>(r) ?? [])); }
  getById(id: number)     { return this.api.get<any>(`${this.base}/${id}`).pipe(map(r => this.u<RegionDto>(r))); }
  getByDelegue(id: number){ return this.api.get<any>(`${this.base}/by-delegue/${id}`).pipe(map(r => this.u<RegionDto[]>(r) ?? [])); }
  getCount(id: number): Observable<number> { return this.api.get<any>(`${this.base}/count/${id}`).pipe(map(r => this.u<number>(r) ?? 0)); }
  createOrUpdate(dto: RegionDto): Observable<RegionDto> { return this.api.post<any>(this.base, dto).pipe(map(r => this.u<RegionDto>(r))); }
  delete(id: number): Observable<void> { return this.api.delete<void>(`${this.base}/${id}`); }
}
