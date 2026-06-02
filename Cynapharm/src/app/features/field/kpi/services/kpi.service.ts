import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../../core/services/api.service';

@Injectable({ providedIn: 'root' })
export class KpiService {
  private readonly base = '/fields/kpi';
  constructor(private api: ApiService) {}
  private u<T>(r: any): T { return r?.Result ?? r?.result ?? r; }

  getNombreVisites(idDelegue: number, debut?: string, fin?: string): Observable<any> {
    let p = new HttpParams().set('idDelegue', idDelegue);
    if (debut) p = p.set('debut', debut);
    if (fin)   p = p.set('fin', fin);
    return this.api.get<any>(`${this.base}/visites-count`, p).pipe(map(r => this.u<any>(r)));
  }

  hasVisiteAtDate(idDelegue: number, date: string): Observable<boolean> {
    const p = new HttpParams().set('idDelegue', idDelegue).set('date', date);
    return this.api.get<any>(`${this.base}/has-visite`, p).pipe(map(r => this.u<boolean>(r) ?? false));
  }

  getHistorique(idDelegue: number): Observable<any[]> {
    return this.api.get<any>(`${this.base}/historique/${idDelegue}`).pipe(map(r => this.u<any[]>(r) ?? []));
  }

  getClientFidelite(idClient: number): Observable<any> {
    return this.api.get<any>(`${this.base}/client-fidelite/${idClient}`).pipe(map(r => this.u<any>(r)));
  }

  getPerformance(idDelegue: number): Observable<any> {
    return this.api.get<any>(`${this.base}/performance/${idDelegue}`).pipe(map(r => this.u<any>(r)));
  }

  getPerformanceRate(idDelegue: number): Observable<number> {
    return this.api.get<any>(`${this.base}/performance-rate/${idDelegue}`).pipe(map(r => this.u<number>(r) ?? 0));
  }

  getTauxConversion(idDelegue: number, debut: string, fin: string): Observable<number> {
    const p = new HttpParams().set('debut', debut).set('fin', fin);
    return this.api.get<any>(`${this.base}/taux-conversion/${idDelegue}`, p).pipe(map(r => this.u<number>(r) ?? 0));
  }
}
