import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../../core/services/api.service';

export interface RapportDto {
  idRapport?:       number;
  id_User_Delegue:  number;
  id_Visite:        number;
  commentaire:      string;
  resultat:         string;
  dateRapport?:     string;
  estValide?:       boolean;
}

@Injectable({ providedIn: 'root' })
export class RapportService {
  private readonly base = '/fields/rapports';
  constructor(private api: ApiService) {}
  private u<T>(r: any): T { return r?.Result ?? r?.result ?? r; }

  private normalize(r: any): RapportDto {
    return {
      idRapport:       r.idRapport       ?? r.IdRapport       ?? undefined,
      id_User_Delegue: r.id_User_Delegue ?? r.idUserDelegue   ?? 0,
      id_Visite:       r.id_Visite       ?? r.idVisite        ?? 0,
      commentaire:     r.commentaire     ?? r.Commentaire     ?? '',
      resultat:        r.resultat        ?? r.Resultat        ?? '',
      dateRapport:     r.dateRapport     ?? r.DateRapport     ?? r.date ?? r.Date ?? '',
      estValide:       r.estValide       ?? r.EstValide       ?? undefined,
    };
  }

  getAll()       { return this.api.get<any>(`${this.base}/all`).pipe(map(r => (this.u<any[]>(r) ?? []).map((x: any) => this.normalize(x)))); }
  getById(id: number) { return this.api.get<any>(`${this.base}/${id}`).pipe(map(r => this.normalize(this.u<any>(r)))); }
  getByVisite(id: number)        { return this.api.get<any>(`${this.base}/by-visite/${id}`).pipe(map(r => this.u<RapportDto[]>(r) ?? [])); }
  getByDelegue(id: number)       { return this.api.get<any>(`${this.base}/by-delegue/${id}`).pipe(map(r => this.u<RapportDto[]>(r) ?? [])); }
  canCreate(idVisite: number): Observable<boolean> { return this.api.get<any>(`${this.base}/can-create/${idVisite}`).pipe(map(r => this.u<boolean>(r) ?? false)); }
  hasRapport(idVisite: number): Observable<boolean>{ return this.api.get<any>(`${this.base}/has-rapport/${idVisite}`).pipe(map(r => this.u<boolean>(r) ?? false)); }
  createOrUpdate(dto: RapportDto): Observable<RapportDto> { return this.api.post<any>(`${this.base}/createUpdate`, dto).pipe(map(r => this.u<RapportDto>(r))); }
  validate(id: number, idSuperviseur: number): Observable<void> {
    return this.api.put<void>(`${this.base}/${id}/validate?idSuperviseur=${idSuperviseur}`, {});
  }
  delete(id: number): Observable<void> { return this.api.delete<void>(`${this.base}/${id}`); }
}
