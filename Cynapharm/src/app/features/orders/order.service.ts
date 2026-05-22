import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../core/services/api.service';

// ── Enums mirroring C# EtatCommande (7 states) ───────────────────────────────
export enum EtatCommande {
  Brouillon     = 0,
  EnAttente     = 1,
  Confirmee     = 2,
  EnPreparation = 3,
  Expediee      = 4,
  Livree        = 5,
  Annulee       = 6,
}

export const ETAT_LABELS: Record<number, string> = {
  0: 'Brouillon',
  1: 'En attente',
  2: 'Confirmée',
  3: 'En préparation',
  4: 'Expédiée',
  5: 'Livrée',
  6: 'Annulée',
};

export const ETAT_CSS: Record<number, string> = {
  0: 'chip-default',   // Grey
  1: 'chip-warning',   // Orange
  2: 'chip-info',      // Cyan
  3: 'chip-primary',   // Blue
  4: 'chip-purple',    // Purple
  5: 'chip-success',   // Green
  6: 'chip-danger',    // Red
};

// ── Dashboard DTO ─────────────────────────────────────────────────────────────
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

// ── DTOs (PascalCase — OrderAPI has NO JsonNamingPolicy.CamelCase) ────────────
export interface LigneCommandeDto {
  Id_Ligne:     number;
  Id_Produit:   number;
  Id_Commande:  number;
  Quantite:     number;
  Remise:       number;
  NumeroLot:    string;
  PrixUnitaire: number;
  SousTotal?:   number;
}

export interface CommandeDto {
  Id_Commande:      number;
  DateCommande:     string;
  MontantTotalHT:   number;
  MontantTTC:       number;
  Statut:           string;
  Id_Client:        number;
  Lignes:           LigneCommandeDto[];
  MotifAnnulation?: string | null;
  IsDeleted?:       boolean;
  Reclamations?:    any[];
}

export interface CreateLigneDto {
  Id_Commande:  number;
  Id_Produit:   number;
  Id_Ligne:     number;   // 0 = create, >0 = update
  Quantite:     number;
  Remise:       number;
  PrixUnitaire: number;
}

export interface CreateOrderDto {
  Id_Client:         number;
  Lignes:            CreateLigneDto[];
  IsFinalValidation: boolean;
}

export interface UpdateOrderStatusDto {
  Id_Commande:   number;
  NouveauStatut: EtatCommande;
}

// ── Service ───────────────────────────────────────────────────────────────────
@Injectable({ providedIn: 'root' })
export class OrderService {

  private readonly base = '/orders';

  constructor(private api: ApiService) {}

  // OrderAPI ResponseDto — handles both PascalCase and camelCase
  private unwrap<T>(r: any): T {
    if (r?.Result !== undefined) return r.Result;
    if (r?.result !== undefined) return r.result;
    return r;
  }

  // Normalize order: covers ASP.NET default (id_Commande) AND explicit PascalCase (Id_Commande)
  private normalizeOrder(o: any): CommandeDto {
    return {
      Id_Commande:      o.Id_Commande    ?? o.id_Commande    ?? o.idCommande    ?? 0,
      DateCommande:     o.DateCommande   ?? o.dateCommande   ?? '',
      MontantTotalHT:   o.MontantTotalHT ?? o.montantTotalHT ?? o.montantTotalHt ?? 0,
      MontantTTC:       o.MontantTTC     ?? o.montantTTC     ?? o.montantTtc    ?? 0,
      Statut:           this.toStatutString(o.Statut ?? o.statut ?? ''),
      Id_Client:        o.Id_Client      ?? o.id_Client      ?? o.idClient      ?? 0,
      Lignes:           (o.Lignes ?? o.lignes ?? []).map((l: any) => this.normalizeLigne(l)),
      MotifAnnulation:  o.MotifAnnulation ?? o.motifAnnulation ?? null,
      IsDeleted:        o.IsDeleted ?? o.isDeleted ?? false,
      Reclamations:     o.Reclamations ?? o.reclamations ?? [],
    };
  }

  private toStatutString(s: any): string {
    if (typeof s === 'number') {
      const names: Record<number, string> = {
        0: 'Brouillon', 1: 'EnAttente', 2: 'Confirmee',
        3: 'EnPreparation', 4: 'Expediee', 5: 'Livree', 6: 'Annulee',
      };
      return names[s] ?? '';
    }
    return String(s ?? '');
  }

  private normalizeLigne(l: any): LigneCommandeDto {
    return {
      Id_Ligne:     l.Id_Ligne     ?? l.id_Ligne     ?? l.idLigne     ?? 0,
      Id_Produit:   l.Id_Produit   ?? l.id_Produit   ?? l.idProduit   ?? 0,
      Id_Commande:  l.Id_Commande  ?? l.id_Commande  ?? l.idCommande  ?? 0,
      Quantite:     l.Quantite     ?? l.quantite     ?? 0,
      Remise:       l.Remise       ?? l.remise       ?? 0,
      NumeroLot:    l.NumeroLot    ?? l.numeroLot    ?? '',
      PrixUnitaire: l.PrixUnitaire ?? l.prixUnitaire ?? 0,
      SousTotal:    l.SousTotal    ?? l.sousTotal    ?? undefined,
    };
  }

  // Map enum string name → EtatCommande number (keep Validee for backward compat)
  statutToNumber(statut: string): number {
    const map: Record<string, number> = {
      Brouillon: 0, EnAttente: 1,
      Confirmee: 2, Validee: 2,
      EnPreparation: 3,
      Expediee: 4, Livree: 5, Annulee: 6,
    };
    return map[statut] ?? -1;
  }

  getEtatLabel(statut: string | number): string {
    if (typeof statut === 'number') return ETAT_LABELS[statut] ?? '—';
    return ETAT_LABELS[this.statutToNumber(statut)] ?? statut;
  }

  getEtatClass(statut: string | number): string {
    const n = typeof statut === 'number' ? statut : this.statutToNumber(statut);
    return ETAT_CSS[n] ?? 'chip-default';
  }

  // Valid next statuses — strict state machine
  getNextStatuses(current: string): { label: string; value: EtatCommande }[] {
    const n = this.statutToNumber(current);
    const transitions: Record<number, EtatCommande[]> = {
      0: [EtatCommande.EnAttente,     EtatCommande.Annulee],
      1: [EtatCommande.Confirmee,     EtatCommande.Annulee],
      2: [EtatCommande.EnPreparation, EtatCommande.Annulee],
      3: [EtatCommande.Expediee,      EtatCommande.Annulee],
      4: [EtatCommande.Livree],
    };
    return (transitions[n] ?? []).map(v => ({ value: v, label: ETAT_LABELS[v] }));
  }

  // ── CRUD ───────────────────────────────────────────────────────────────────

  getOrders(page = 1, pageSize = 20, statut?: string, startDate?: string, endDate?: string): Observable<CommandeDto[]> {
    let p = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (statut)    p = p.set('statut', statut);
    if (startDate) p = p.set('startDate', startDate);
    if (endDate)   p = p.set('endDate', endDate);
    return this.api.get<any>(this.base, p).pipe(
      map(r => {
        const raw = this.unwrap<any[]>(r) ?? [];
        return Array.isArray(raw) ? raw.map(o => this.normalizeOrder(o)) : [];
      })
    );
  }

  getOrdersByStatus(statut: string, page = 1, pageSize = 20): Observable<CommandeDto[]> {
    const p = new HttpParams().set('statut', statut).set('page', page).set('pageSize', pageSize);
    return this.api.get<any>(`${this.base}/by-status`, p).pipe(
      map(r => (this.unwrap<any[]>(r) ?? []).map(o => this.normalizeOrder(o)))
    );
  }

  getOrdersByDateRange(startDate: string, endDate: string, page = 1, pageSize = 20): Observable<CommandeDto[]> {
    const p = new HttpParams().set('startDate', startDate).set('endDate', endDate).set('page', page).set('pageSize', pageSize);
    return this.api.get<any>(`${this.base}/by-date`, p).pipe(
      map(r => (this.unwrap<any[]>(r) ?? []).map(o => this.normalizeOrder(o)))
    );
  }

  getOrdersDashboard(): Observable<OrderDashboardDto> {
    return this.api.get<any>(`${this.base}/dashboard`).pipe(
      map(r => this.unwrap<OrderDashboardDto>(r))
    );
  }

  cancelOrder(id: number, motif: string): Observable<any> {
    return this.api.put<any>(`${this.base}/${id}/cancel`, { Motif: motif });
  }

  getOrderById(id: number): Observable<CommandeDto | null> {
    return this.api.get<any>(`${this.base}/${id}`).pipe(
      map(r => {
        const raw = this.unwrap<any>(r);
        return raw ? this.normalizeOrder(raw) : null;
      })
    );
  }

  getOrdersByClient(clientId: number): Observable<CommandeDto[]> {
    return this.api.get<any>(`${this.base}/by-client/${clientId}`).pipe(
      map(r => {
        const raw = this.unwrap<any[]>(r) ?? [];
        return Array.isArray(raw) ? raw.map(o => this.normalizeOrder(o)) : [];
      })
    );
  }

  createOrder(dto: CreateOrderDto): Observable<any> {
    return this.api.post<any>(this.base, dto);
  }

  updateOrderStatus(dto: UpdateOrderStatusDto): Observable<any> {
    return this.api.put<any>(`${this.base}/status`, dto);
  }

  deleteOrder(id: number): Observable<any> {
    return this.api.delete<any>(`${this.base}/${id}`);
  }
}
