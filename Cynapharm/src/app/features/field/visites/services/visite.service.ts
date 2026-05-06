import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../../core/services/api.service';
import { VisiteType } from '../../../../core/models/enums';

export interface VisiteDto {
  idVisite?:       number;
  id_User_Delegue: number;
  date:            string;
  type:            VisiteType;
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

  getById(id: number)           { return this.api.get<any>(`${this.base}/${id}`).pipe(map(r => this.u<VisiteDto>(r))); }
  getByDelegue(id: number)      { return this.api.get<any>(`${this.base}/by-delegue/${id}`).pipe(map(r => this.u<VisiteDto[]>(r) ?? [])); }
  getByPlanning(id: number)     { return this.api.get<any>(`${this.base}/by-planning/${id}`).pipe(map(r => this.u<VisiteDto[]>(r) ?? [])); }
  createOrUpdate(dto: VisiteDto): Observable<VisiteDto> { return this.api.post<any>(this.base, dto).pipe(map(r => this.u<VisiteDto>(r))); }
  affectToPlanning(idVisite: number, idPlanning: number): Observable<void> {
    return this.api.put<void>(`${this.base}/${idVisite}/planning/${idPlanning}`, {});
  }
  complete(id: number): Observable<void> { return this.api.put<void>(`${this.base}/${id}/complete`, {}); }
  delete(id: number): Observable<void>   { return this.api.delete<void>(`${this.base}/${id}`); }
}
