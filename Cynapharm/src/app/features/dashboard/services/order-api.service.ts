import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../../core/services/api.service';

/** Correspond à EtatCommande enum backend (7 états) */
export enum EtatCommande {
  Brouillon     = 0,
  EnAttente     = 1,
  Confirmee     = 2,
  EnPreparation = 3,
  Expediee      = 4,
  Livree        = 5,
  Annulee       = 6,
}

export const ETAT_LABELS: Record<EtatCommande, string> = {
  [EtatCommande.Brouillon]:     'Brouillon',
  [EtatCommande.EnAttente]:     'En attente',
  [EtatCommande.Confirmee]:     'Confirmée',
  [EtatCommande.EnPreparation]: 'En préparation',
  [EtatCommande.Expediee]:      'Expédiée',
  [EtatCommande.Livree]:        'Livrée',
  [EtatCommande.Annulee]:       'Annulée',
};

export interface OrderDashboardDto {
  TotalCommandes:       number;
  EnAttente:            number;
  Confirmees:           number;
  EnPreparation:        number;
  Expediees:            number;
  Livrees:              number;
  Annulees:             number;
  MontantTotalHT:       number;
  MontantTotalTTC:      number;
  ReclamationsOuvertes: number;
  ReclamationsEnCours:  number;
  ReclamationsResolues: number;
  CommandesAujourdHui:  number;
  CommandesCeMois:      number;
}

export interface Commande {
  id_Commande: number;
  dateCommande: string;
  montantHT: number;
  montantTTC: number;
  etatCommande: EtatCommande;
  id_Client: number;
}

export interface OrderStats {
  countByStatus:  Record<string, number>;
  totalCA:        number;
  countEnAttente: number;
  countLivrees:   number;
  countAnnulees:  number;
  countToday:     number;
  totalOrders:    number;
  last7Days:      { date: string; count: number; ca: number }[];
}

@Injectable({ providedIn: 'root' })
export class OrderApiService {

  constructor(private api: ApiService) {}

  private unwrap<T>(r: any): T {
    if (r?.Result !== undefined) return r.Result;
    if (r?.result !== undefined) return r.result;
    return r;
  }

  /** Toutes les commandes (ADMIN/SUPERVISEUR) */
  getAllOrders(): Observable<Commande[]> {
    return this.api.get<any>(`/orders`).pipe(
      map(r => {
        const data = this.unwrap<any>(r);
        return Array.isArray(data) ? data : [];
      })
    );
  }

  /** Tableau de bord commandes */
  getOrdersDashboard(): Observable<OrderDashboardDto> {
    return this.api.get<any>(`/orders/dashboard`).pipe(
      map(r => this.unwrap<OrderDashboardDto>(r))
    );
  }

  /** Commandes d'un client */
  getOrdersByClient(idClient: number): Observable<Commande[]> {
    return this.api.get<any>(`/orders/by-client/${idClient}`).pipe(
      map(r => {
        const data = this.unwrap<any>(r);
        return Array.isArray(data) ? data : [];
      })
    );
  }

  /**
   * Calcule les statistiques côté front à partir de la liste brute.
   * Évite un aller-retour supplémentaire au backend.
   */
  computeStats(orders: Commande[]): OrderStats {
    const countByStatus: Record<string, number> = {};
    let totalCA = 0;
    let countEnAttente = 0;
    let countLivrees = 0;

    // Données des 7 derniers jours
    const now   = new Date();
    const todayStr = now.toISOString().slice(0, 10);
    const last7: { date: string; count: number; ca: number }[] = [];
    for (let i = 6; i >= 0; i--) {
      const d = new Date(now);
      d.setDate(now.getDate() - i);
      last7.push({ date: d.toISOString().slice(0, 10), count: 0, ca: 0 });
    }

    let countAnnulees = 0;
    let countToday    = 0;

    for (const o of orders) {
      // Normalise le champ statut (PascalCase ou camelCase selon backend)
      const etat: any = (o as any).Statut ?? (o as any).statut
                     ?? (o as any).etatCommande ?? (o as any).EtatCommande;
      const label = typeof etat === 'string'
        ? etat
        : (ETAT_LABELS[etat as EtatCommande] ?? 'Inconnu');

      countByStatus[label] = (countByStatus[label] ?? 0) + 1;

      const ttc = (o as any).MontantTTC ?? (o as any).montantTTC ?? o.montantTTC ?? 0;
      totalCA += ttc;

      const etatNum = typeof etat === 'number' ? etat : undefined;
      if (etat === 'EnAttente'    || etatNum === EtatCommande.EnAttente)  countEnAttente++;
      if (etat === 'Livree'       || etatNum === EtatCommande.Livree)     countLivrees++;
      if (etat === 'Annulee'      || etatNum === EtatCommande.Annulee)    countAnnulees++;

      const dateStr = ((o as any).DateCommande ?? (o as any).dateCommande ?? o.dateCommande ?? '')
                        .slice(0, 10);
      if (dateStr === todayStr) countToday++;

      const bucket = last7.find(b => b.date === dateStr);
      if (bucket) { bucket.count++; bucket.ca += ttc; }
    }

    return {
      countByStatus, totalCA, countEnAttente, countLivrees,
      countAnnulees, countToday, totalOrders: orders.length,
      last7Days: last7,
    };
  }
}
