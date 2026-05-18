import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../../core/services/api.service';

export interface DocumentDto {
  numeroDoc?:    string;
  type:          string;
  id_Client:     number;
  id_Commande?:  number | null;
  dateDocument?: string;
}

@Injectable({ providedIn: 'root' })
export class DocumentService {
  private readonly base = '/documents';
  constructor(private api: ApiService) {}
  private u<T>(r: any): T { return r?.Result ?? r?.result ?? r; }

  getAll(pageNumber = 1, pageSize = 20): Observable<DocumentDto[]> {
    const p = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.api.get<any>(this.base, p).pipe(map(r => this.u<DocumentDto[]>(r) ?? []));
  }
  getById(numero: string)    { return this.api.get<any>(`${this.base}/${numero}`).pipe(map(r => this.u<DocumentDto>(r))); }
  getByClient(id: number)    { return this.api.get<any>(`${this.base}/client/${id}`).pipe(map(r => this.u<DocumentDto[]>(r) ?? [])); }
  getByCommande(id: number)  { return this.api.get<any>(`${this.base}/commande/${id}`).pipe(map(r => this.u<DocumentDto[]>(r) ?? [])); }
  getByType(type: string, pageNumber = 1, pageSize = 20): Observable<DocumentDto[]> {
    const p = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.api.get<any>(`${this.base}/type/${type}`, p).pipe(map(r => this.u<DocumentDto[]>(r) ?? []));
  }
  getByClientAndType(idClient: number, type: string): Observable<DocumentDto[]> {
    return this.api.get<any>(`${this.base}/client/${idClient}/type/${type}`).pipe(map(r => this.u<DocumentDto[]>(r) ?? []));
  }
  createOrUpdate(dto: DocumentDto): Observable<DocumentDto> { return this.api.post<any>(`${this.base}/createUpdate`, dto).pipe(map(r => this.u<DocumentDto>(r))); }
  delete(numero: string): Observable<void> { return this.api.delete<void>(`${this.base}/${numero}`); }
}
