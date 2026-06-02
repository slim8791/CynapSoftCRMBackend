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
  date?:            string;
  idSuperviseurValidateur?: number | null;
  latitude?:         number | null;
  longitude?:        number | null;
  produitsDiscutes?: string | null;
}

@Injectable({ providedIn: 'root' })
export class RapportService {
  private readonly base = '/fields/rapports';
  constructor(private api: ApiService) {}
  private u<T>(r: any): T { return r?.Result ?? r?.result ?? r; }

  private normalize(r: any): RapportDto {
    r = r ?? {};
    const idRapport = r.idRapport ?? r.IdRapport ?? r.id_Rapport ?? r.Id_Rapport ?? 0;
    return {
      idRapport:       idRapport > 0 ? idRapport : undefined,
      id_User_Delegue: r.id_User_Delegue ?? r.Id_User_Delegue ?? r.idUserDelegue    ?? r.IdUserDelegue    ?? 0,
      id_Visite:       r.id_Visite       ?? r.Id_Visite       ?? r.idVisite         ?? r.IdVisite         ?? 0,
      commentaire:     r.commentaire     ?? r.Commentaire     ?? '',
      resultat:        r.resultat        ?? r.Resultat        ?? '',
      date:            r.date            ?? r.Date            ?? r.dateRapport ?? r.DateRapport ?? '',
      idSuperviseurValidateur: r.idSuperviseurValidateur ?? r.IdSuperviseurValidateur ?? null,
      latitude:        r.latitude        ?? r.Latitude        ?? null,
      longitude:       r.longitude       ?? r.Longitude       ?? null,
      produitsDiscutes: r.produitsDiscutes ?? r.ProduitsDiscutes ?? null,
    };
  }

  getAll()       { return this.api.get<any>(`${this.base}/all`).pipe(map(r => (this.u<any[]>(r) ?? []).map((x: any) => this.normalize(x)))); }
  getById(id: number) { return this.api.get<any>(`${this.base}/${id}`).pipe(map(r => this.normalize(this.u<any>(r)))); }
  getByVisite(id: number) {
    return this.api.get<any>(`${this.base}/by-visite/${id}`).pipe(
      map(r => {
        const raw = this.u<any>(r);
        if (!raw) return [];
        return Array.isArray(raw)
          ? raw.map((x: any) => this.normalize(x))
          : [this.normalize(raw)];
      })
    );
  }
  getByDelegue(id: number) {
    return this.api.get<any>(`${this.base}/by-delegue/${id}`).pipe(
      map(r => (this.u<any[]>(r) ?? []).map((x: any) => this.normalize(x)))
    );
  }
  canCreate(idVisite: number): Observable<boolean> { return this.api.get<any>(`${this.base}/can-create/${idVisite}`).pipe(map(r => this.u<boolean>(r) ?? false)); }
  hasRapport(idVisite: number): Observable<boolean>{ return this.api.get<any>(`${this.base}/has-rapport/${idVisite}`).pipe(map(r => this.u<boolean>(r) ?? false)); }
  createOrUpdate(dto: RapportDto): Observable<RapportDto> {
    return this.api.post<any>(`${this.base}/createUpdate`, dto).pipe(map(r => this.normalize(this.u<any>(r))));
  }
  validate(id: number, idSuperviseur: number): Observable<void> {
    return this.api.put<void>(`${this.base}/${id}/validate?idSuperviseur=${idSuperviseur}`, {});
  }
  delete(id: number): Observable<void> { return this.api.delete<void>(`${this.base}/${id}`); }
}
