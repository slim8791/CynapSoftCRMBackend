import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../../core/services/api.service';
import { TypeObjectif, PeriodeObjectif } from '../../../../core/models/enums';

export interface ObjectifDto {
  idObjectif?:       number;
  id_User_Delegue:   number;
  type:              TypeObjectif;
  periode:           PeriodeObjectif;
  valeurCible:       number;
  valeurRealisee:    number;
  dateDebut:         string;
  dateFin:           string;
}

@Injectable({ providedIn: 'root' })
export class ObjectifService {
  private readonly base = '/fields/objectifs';
  constructor(private api: ApiService) {}
  private u<T>(r: any): T { return r?.Result ?? r?.result ?? r; }
  
  private normalize(o: any): ObjectifDto {
    if (!o) return o;
    return {
      ...o,
      idObjectif: o.idObjectif ?? o.IdObjectif ?? o.id_Objectif ?? o.Id_Objectif ?? o.id ?? o.Id ?? 0,
      id_User_Delegue: o.id_User_Delegue ?? o.Id_User_Delegue ?? o.idUserDelegue ?? o.IdUserDelegue ?? 0
    };
  }

  getAll()                { return this.api.get<any>(this.base).pipe(map(r => (this.u<any[]>(r) ?? []).map(o => this.normalize(o)))); }
  getById(id: number)     { return this.api.get<any>(`${this.base}/${id}`).pipe(map(r => this.normalize(this.u<any>(r)))); }
  getByDelegue(id: number){ return this.api.get<any>(`${this.base}/by-delegue/${id}`).pipe(map(r => (this.u<any[]>(r) ?? []).map(o => this.normalize(o)))); }
  createOrUpdate(dto: ObjectifDto): Observable<ObjectifDto> { return this.api.post<any>(this.base, dto).pipe(map(r => this.normalize(this.u<any>(r)))); }
  updateValue(id: number, v: number): Observable<void> {
    return this.api.put<void>(`${this.base}/${id}/value?nouvelleValeur=${v}`, {});
  }
  delete(id: number): Observable<void> { return this.api.delete<void>(`${this.base}/${id}`); }
}
