import { Injectable } from '@angular/core';
import { Observable, of, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { ApiService } from '../../../core/services/api.service';

export enum StatutReclamation { Ouverte = 0, EnCours = 1, Resolue = 2 }

export const STATUT_REC_LABELS: Record<number, string> = {
  0: 'Ouverte', 1: 'En cours', 2: 'Résolue',
};

export const STATUT_REC_CSS: Record<number, string> = {
  0: 'chip-warning', 1: 'chip-info', 2: 'chip-success',
};

// PascalCase — OrderAPI has no JsonNamingPolicy.CamelCase
export interface ReclamationDto {
  Id_Rec:           number;
  Message:          string;
  DateReclamation:  string;
  Statut?:          string | number;   // 0/Ouverte, 1/EnCours, 2/Resolue
  Id_Commande:      number;
  Id_Ligne:         number;
  Id_Client:        number;
}

@Injectable({ providedIn: 'root' })
export class ReclamationService {

  private readonly base = '/orders/reclamations';

  constructor(private api: ApiService) {}

  private unwrap<T>(r: any): T {
    if (r?.Result !== undefined) return r.Result;
    if (r?.result !== undefined) return r.result;
    return r;
  }

  // Normalize: covers Id_Rec / id_Rec / idRec, Id_Commande / id_Commande / idCommande …
  private normalizeRec(r: any): ReclamationDto {
    return {
      Id_Rec:          r.Id_Rec          ?? r.id_Rec          ?? r.idRec          ?? 0,
      Message:         r.Message         ?? r.message         ?? '',
      DateReclamation: r.DateReclamation ?? r.dateReclamation ?? '',
      Statut:          r.Statut          ?? r.statut          ?? 'Ouverte',
      Id_Commande:     r.Id_Commande     ?? r.id_Commande     ?? r.idCommande     ?? 0,
      Id_Ligne:        r.Id_Ligne        ?? r.id_Ligne        ?? r.idLigne        ?? 0,
      Id_Client:       r.Id_Client       ?? r.id_Client       ?? r.idClient       ?? 0,
    };
  }

  statutToNumber(statut?: string | number): number {
    if (typeof statut === 'number') return statut;
    const map: Record<string, number> = { Ouverte: 0, EnCours: 1, Resolue: 2, '0': 0, '1': 1, '2': 2 };
    return statut != null ? (map[statut] ?? 0) : 0;
  }

  getStatutLabel(statut?: string | number): string {
    return STATUT_REC_LABELS[this.statutToNumber(statut)] ?? statut ?? '—';
  }

  getStatutClass(statut?: string | number): string {
    return STATUT_REC_CSS[this.statutToNumber(statut)] ?? 'chip-default';
  }

  private toArray(r: any): ReclamationDto[] {
    const raw = Array.isArray(r) ? r : (this.unwrap<any[]>(r) ?? []);
    return Array.isArray(raw) ? raw.map(x => this.normalizeRec(x)) : [];
  }

  // GET /orders/reclamations → returns direct array (no ResponseDto wrapper)
  getAll(): Observable<ReclamationDto[]> {
    return this.api.get<any>(this.base).pipe(map(r => this.toArray(r)));
  }

  getById(id: number): Observable<ReclamationDto | null> {
    return this.api.get<any>(`${this.base}/${id}`).pipe(
      map(r => { const raw = this.unwrap<any>(r); return raw ? this.normalizeRec(raw) : null; })
    );
  }

  // 404 = aucune réclamation pour cette commande → retourne [] sans erreur
  getByOrder(orderId: number): Observable<ReclamationDto[]> {
    return this.api.get<any>(`${this.base}/by-commande/${orderId}`).pipe(
      map(r => this.toArray(r)),
      catchError((err: HttpErrorResponse) => err.status === 404 ? of([] as ReclamationDto[]) : throwError(() => err))
    );
  }

  getByClient(clientId: number): Observable<ReclamationDto[]> {
    return this.api.get<any>(`${this.base}/by-client/${clientId}`).pipe(
      map(r => this.toArray(r)),
      catchError((err: HttpErrorResponse) => err.status === 404 ? of([] as ReclamationDto[]) : throwError(() => err))
    );
  }

  // POST — CLIENT only; Id_Client is injected from JWT server-side
  createOrUpdate(dto: ReclamationDto): Observable<any> {
    return this.api.post<any>(this.base, dto);
  }

  // PUT /orders/reclamations/{id}/status — ADMIN, SUPERVISEUR
  updateStatus(id: number, status: StatutReclamation): Observable<any> {
    return this.api.put<any>(`${this.base}/${id}/status`, status);
  }

  delete(id: number): Observable<any> {
    return this.api.delete<any>(`${this.base}/${id}`);
  }
}
