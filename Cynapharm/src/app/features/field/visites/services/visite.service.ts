import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../../core/services/api.service';
import { VisiteType } from '../../../../core/models/enums';

export interface VisiteDto {
  idVisite?:       number;
  id_User_Delegue: number;
  dateVisite:      string;
  type:            VisiteType;
  isCompleted?:    boolean;
  id_Medecin?:     number | null;
  id_Pharmacien?:  number | null;
  id_Planning?:    number | null;
  id_Region?:      number | null;
}

@Injectable({ providedIn: 'root' })
export class VisiteService {
  private readonly base = '/fields/visites';
  constructor(private api: ApiService) {}
  private u<T>(r: any): T { return r?.Result ?? r?.result ?? r; }

  private normalize(r: any): VisiteDto {
    return {
      idVisite:        r.idVisite        ?? r.IdVisite        ?? undefined,
      id_User_Delegue: r.id_User_Delegue ?? r.Id_User_Delegue ?? r.idUserDelegue ?? 0,
      dateVisite:      r.date            ?? r.dateVisite      ?? r.DateVisite     ?? r.Date ?? '',
      type:            r.type            ?? r.Type            ?? 0,
      isCompleted:     r.isCompleted     ?? r.IsCompleted     ?? false,
      id_Medecin:      r.id_Medecin      ?? r.Id_Medecin      ?? r.idMedecin      ?? null,
      id_Pharmacien:   r.id_Pharmacien   ?? r.Id_Pharmacien   ?? r.idPharmacien   ?? null,
      id_Planning:     r.id_Planning     ?? r.Id_Planning     ?? r.idPlanning     ?? null,
      id_Region:       r.id_Region       ?? r.Id_Region       ?? r.idRegion       ?? null,
    };
  }

  getAll(startDate?: string, endDate?: string): Observable<VisiteDto[]> {
    let p = new HttpParams();
    if (startDate) p = p.set('startDate', startDate);
    if (endDate)   p = p.set('endDate',   endDate);
    return this.api.get<any>(this.base, p).pipe(
      map(r => (this.u<any[]>(r) ?? []).map((x: any) => this.normalize(x)))
    );
  }

  getById(id: number): Observable<VisiteDto> {
    return this.api.get<any>(`${this.base}/${id}`).pipe(
      map(r => {
        const unwrapped = this.u<any>(r);
        console.log('API Response for getById:', r, 'Unwrapped:', unwrapped);
        const normalized = this.normalize(unwrapped);
        console.log('Normalized Visite:', normalized);
        return normalized;
      })
    );
  }

  getByDelegue(id: number): Observable<VisiteDto[]> {
    return this.api.get<any>(`${this.base}/by-delegue/${id}`).pipe(
      map(r => (this.u<any[]>(r) ?? []).map((x: any) => this.normalize(x)))
    );
  }

  getByPlanning(id: number): Observable<VisiteDto[]> {
    return this.api.get<any>(`${this.base}/by-planning/${id}`).pipe(
      map(r => (this.u<any[]>(r) ?? []).map((x: any) => this.normalize(x)))
    );
  }

  createOrUpdate(dto: VisiteDto): Observable<VisiteDto> {
    return this.api.post<any>(this.base, dto).pipe(
      map(r => this.normalize(this.u<any>(r)))
    );
  }

  affectToPlanning(idVisite: number, idPlanning: number): Observable<void> {
    return this.api.put<void>(`${this.base}/${idVisite}/planning/${idPlanning}`, {});
  }

  complete(id: number): Observable<void> {
    return this.api.put<void>(`${this.base}/${id}/complete`, {});
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`${this.base}/${id}`);
  }
}
