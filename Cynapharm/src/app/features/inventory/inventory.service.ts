import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../core/services/api.service';

export interface StockDelegue {
  id_stock: number;
  id_User_Delegue: number;
  id_Produit: number;
  numeroLot: string;
  dateExpiration: string;
  qteDisponible: number;
  qteReservee: number;
}

export interface StockEchantillon extends StockDelegue {
  qteEchantillon: number;
}

export interface StockGratuite extends StockDelegue {
  qteGratuite: number;
  typePromotion: string;
}

@Injectable({ providedIn: 'root' })
export class InventoryService {

  constructor(private api: ApiService) {}

  private unwrap<T>(r: any): T {
    if (r?.Result !== undefined) return r.Result;
    if (r?.result !== undefined) return r.result;
    return r;
  }

  // ── Stocks par produit ──────────────────────────────────

  /** Tous les stocks délégués d'un produit (ADMIN/SUPERVISEUR) */
  getStockByProduit(idProduit: number): Observable<StockDelegue[]> {
    return this.api.get<any>(`/inventory/stocks-delegue/by-produit/${idProduit}`).pipe(
      map(r => {
        const data = this.unwrap<any>(r);
        return Array.isArray(data) ? data : [];
      })
    );
  }

  // ── Stock par lot ──────────────────────────────────────

  /** Stock délégué lié à un numéro de lot */
  getStockByLot(numeroLot: string): Observable<StockDelegue | null> {
    return this.api.get<any>(`/inventory/stocks-delegue/by-lot/${numeroLot}`).pipe(
      map(r => {
        const data = this.unwrap<any>(r);
        return data ?? null;
      })
    );
  }

  // ── Vérification disponibilité ─────────────────────────

  /** Vérifie si un stock a la quantité suffisante */
  checkAvailability(idStock: number, qte: number): Observable<boolean> {
    const params = new HttpParams()
      .set('idStock', String(idStock))
      .set('qte', String(qte));
    return this.api.get<any>(`/inventory/inventory-business/check-availability`, params).pipe(
      map(r => {
        const data = this.unwrap<any>(r);
        return typeof data === 'boolean' ? data : false;
      })
    );
  }

  // ── Stock par ID ───────────────────────────────────────

  getStockById(idStock: number): Observable<StockDelegue | null> {
    return this.api.get<any>(`/inventory/stocks-delegue/${idStock}`).pipe(
      map(r => {
        const data = this.unwrap<any>(r);
        return data ?? null;
      })
    );
  }

  // ── Stock échantillon ─────────────────────────────────

  getStockEchantillon(idStock: number): Observable<StockEchantillon | null> {
    return this.api.get<any>(`/inventory/stocks-promotionnels/echantillon/${idStock}`).pipe(
      map(r => this.unwrap<StockEchantillon | null>(r))
    );
  }

  // ── Stock gratuit ─────────────────────────────────────

  getStockGratuite(idStock: number): Observable<StockGratuite | null> {
    return this.api.get<any>(`/inventory/stocks-promotionnels/gratuite/${idStock}`).pipe(
      map(r => this.unwrap<StockGratuite | null>(r))
    );
  }
}
