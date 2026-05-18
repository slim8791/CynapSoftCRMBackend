import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../../core/services/api.service';
import { EtatPlanning } from '../../../../core/models/enums';

export interface PlanningDto {
  idPlanning?:       number;
  id_User_Delegue:   number;
  date:              string;
  heureDebut:        string;
  heureFin:          string;
  etatPlanning:      EtatPlanning;
}

@Injectable({ providedIn: 'root' })
export class PlanningService {
  private readonly base = '/fields/plannings';
  constructor(private api: ApiService) {}
  private u<T>(r: any): T { return r?.Result ?? r?.result ?? r; }

  getAll(startDate?: string, endDate?: string): Observable<PlanningDto[]> {
    let p = new HttpParams();
    if (startDate) p = p.set('startDate', startDate);
    if (endDate)   p = p.set('endDate', endDate);
    return this.api.get<any>(this.base, p).pipe(map(r => this.u<PlanningDto[]>(r) ?? []));
  }
  getById(id: number)     { return this.api.get<any>(`${this.base}/${id}`).pipe(map(r => this.u<PlanningDto>(r))); }
  getByDelegue(id: number){ return this.api.get<any>(`${this.base}/by-delegue/${id}`).pipe(map(r => this.u<PlanningDto[]>(r) ?? [])); }
  getByRange(idDelegue: number, start: string, end: string): Observable<PlanningDto[]> {
    const p = new HttpParams().set('idDelegue', idDelegue).set('startDate', start).set('endDate', end);
    return this.api.get<any>(`${this.base}/by-range`, p).pipe(map(r => this.u<PlanningDto[]>(r) ?? []));
  }
  getByDate(idDelegue: number, date: string): Observable<PlanningDto[]> {
    const p = new HttpParams().set('idDelegue', idDelegue).set('date', date);
    return this.api.get<any>(`${this.base}/by-date`, p).pipe(map(r => this.u<PlanningDto[]>(r) ?? []));
  }
  createOrUpdate(dto: PlanningDto): Observable<PlanningDto> { return this.api.post<any>(this.base, dto).pipe(map(r => this.u<PlanningDto>(r))); }
  validate(id: number): Observable<void> { return this.api.put<void>(`${this.base}/${id}/validate`, {}); }
  delete(id: number): Observable<void>   { return this.api.delete<void>(`${this.base}/${id}`); }
}
