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

  getAll()                { return this.api.get<any>(this.base).pipe(map(r => this.u<ObjectifDto[]>(r) ?? [])); }
  getById(id: number)     { return this.api.get<any>(`${this.base}/${id}`).pipe(map(r => this.u<ObjectifDto>(r))); }
  getByDelegue(id: number){ return this.api.get<any>(`${this.base}/by-delegue/${id}`).pipe(map(r => this.u<ObjectifDto[]>(r) ?? [])); }
  createOrUpdate(dto: ObjectifDto): Observable<ObjectifDto> { return this.api.post<any>(this.base, dto).pipe(map(r => this.u<ObjectifDto>(r))); }
  updateValue(id: number, v: number): Observable<void> {
    return this.api.put<void>(`${this.base}/${id}/value?nouvelleValeur=${v}`, {});
  }
  delete(id: number): Observable<void> { return this.api.delete<void>(`${this.base}/${id}`); }
}
