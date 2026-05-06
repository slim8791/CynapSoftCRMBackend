import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../core/services/api.service';

export interface Region {
  id_Region: number;
  nomRegion: string;
  codePostal: string;
  id_User_Delegue: number;
}

export interface VisiteKpi {
  idDelegue: number;
  nomDelegue?: string;
  count: number;
  date?: string;
}

export interface PerformanceDelegue {
  idDelegue: number;
  nomDelegue?: string;
  objectifVisites: number;
  visiteRealisees: number;
  tauxPerformance: number;
}

@Injectable({ providedIn: 'root' })
export class FieldApiService {

  constructor(private api: ApiService) {}

  private unwrap<T>(r: any): T {
    if (r?.Result !== undefined) return r.Result;
    if (r?.result !== undefined) return r.result;
    return r;
  }

  /** Nombre de visites par délégué (optionnel : filtrer par date ISO) */
  getVisitesCount(date?: string): Observable<VisiteKpi[]> {
    const path = date
      ? `/fields/kpi/visites-count?date=${date}`
      : `/fields/kpi/visites-count`;
    return this.api.get<any>(path).pipe(
      map(r => {
        const data = this.unwrap<any>(r);
        return Array.isArray(data) ? data : [];
      })
    );
  }

  /** Taux de performance des délégués */
  getPerformance(): Observable<PerformanceDelegue[]> {
    return this.api.get<any>(`/fields/kpi/performance`).pipe(
      map(r => {
        const data = this.unwrap<any>(r);
        return Array.isArray(data) ? data : [];
      })
    );
  }

  /** Taux de performance global (nombre 0-100) */
  getPerformanceRate(): Observable<number> {
    return this.api.get<any>(`/fields/kpi/performance-rate`).pipe(
      map(r => {
        const data = this.unwrap<any>(r);
        return typeof data === 'number' ? data : 0;
      })
    );
  }

  /** Liste de toutes les régions */
  getRegions(): Observable<Region[]> {
    return this.api.get<any>(`/fields/regions`).pipe(
      map(r => {
        const data = this.unwrap<any>(r);
        return Array.isArray(data) ? data : [];
      })
    );
  }

  /** Historique des visites (tous délégués) */
  getHistoriqueVisites(): Observable<any[]> {
    return this.api.get<any>(`/fields/kpi/historique`).pipe(
      map(r => {
        const data = this.unwrap<any>(r);
        return Array.isArray(data) ? data : [];
      })
    );
  }
}
