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
  etat:              EtatPlanning;
}

@Injectable({ providedIn: 'root' })
export class PlanningService {
  private readonly base = '/fields/plannings';
  constructor(private api: ApiService) {}
  private u<T>(r: any): T { return r?.Result ?? r?.result ?? r; }

  private normalize(p: any): PlanningDto {
    return {
      idPlanning:      p.idPlanning      ?? p.Id_Planning      ?? p.id_Planning      ?? undefined,
      id_User_Delegue: p.id_User_Delegue ?? p.Id_User_Delegue  ?? p.idUserDelegue    ?? 0,
      date:            p.date            ?? p.Date                                    ?? '',
      heureDebut:      p.heureDebut      ?? p.HeureDebut                              ?? '',
      heureFin:        p.heureFin        ?? p.HeureFin                                ?? '',
      etat:            p.etat            ?? p.Etat                                    ?? 0,
    };
  }

  getAll(startDate?: string, endDate?: string): Observable<PlanningDto[]> {
    let p = new HttpParams();
    if (startDate) p = p.set('startDate', startDate);
    if (endDate)   p = p.set('endDate', endDate);
    return this.api.get<any>(this.base, p).pipe(map(r => (this.u<any[]>(r) ?? []).map((x: any) => this.normalize(x))));
  }
  getById(id: number): Observable<PlanningDto | null> {
    return this.api.get<any>(`${this.base}/${id}`).pipe(map(r => {
      const raw = this.u<any>(r);
      return raw ? this.normalize(raw) : null;
    }));
  }
  getByDelegue(id: number): Observable<PlanningDto[]> {
    return this.api.get<any>(`${this.base}/by-delegue/${id}`).pipe(map(r => (this.u<any[]>(r) ?? []).map((x: any) => this.normalize(x))));
  }
  getByRange(idDelegue: number, start: string, end: string): Observable<PlanningDto[]> {
    const p = new HttpParams().set('idDelegue', idDelegue).set('startDate', start).set('endDate', end);
    return this.api.get<any>(`${this.base}/by-range`, p).pipe(map(r => (this.u<any[]>(r) ?? []).map((x: any) => this.normalize(x))));
  }
  getByDate(idDelegue: number, date: string): Observable<PlanningDto[]> {
    const p = new HttpParams().set('idDelegue', idDelegue).set('date', date);
    return this.api.get<any>(`${this.base}/by-date`, p).pipe(map(r => (this.u<any[]>(r) ?? []).map((x: any) => this.normalize(x))));
  }
  createOrUpdate(dto: PlanningDto): Observable<PlanningDto> { 
    return this.api.post<any>(this.base, dto).pipe(map(r => this.normalize(this.u<any>(r)))); 
  }
  validate(id: number): Observable<void> { return this.api.put<void>(`${this.base}/${id}/validate`, {}); }
  delete(id: number): Observable<void>   { return this.api.delete<void>(`${this.base}/${id}`); }
}
