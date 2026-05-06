import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../core/services/api.service';

// ── Enums mirroring C# EtatCommande ──────────────────────────────────────────
export enum EtatCommande {
  Brouillon  = 0,
  EnAttente  = 1,
  Validee    = 2,
  Expediee   = 3,
  Livree     = 4,
  Annulee    = 5,
}

export const ETAT_LABELS: Record<number, string> = {
  0: 'Brouillon',
  1: 'En attente',
  2: 'Validée',
  3: 'Expédiée',
  4: 'Livrée',
  5: 'Annulée',
};

export const ETAT_CSS: Record<number, string> = {
  0: 'chip-default',
  1: 'chip-warning',
  2: 'chip-info',
  3: 'chip-orange',
  4: 'chip-success',
  5: 'chip-danger',
};

// ── DTOs (PascalCase — OrderAPI has NO JsonNamingPolicy.CamelCase) ────────────
export interface LigneCommandeDto {
  Id_Ligne:     number;
  Id_Produit:   number;
  Id_Commande:  number;
  Quantite:     number;
  Remise:       number;
  NumeroLot:    string;
  PrixUnitaire: number;
}

export interface CommandeDto {
  Id_Commande:    number;
  DateCommande:   string;
  MontantTotalHT: number;
  MontantTTC:     number;
  Statut:         string;   // enum name as string: "Brouillon", "EnAttente" …
  Id_Client:      number;
  Lignes:         LigneCommandeDto[];
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
      Id_Commande:    o.Id_Commande    ?? o.id_Commande    ?? o.idCommande    ?? 0,
      DateCommande:   o.DateCommande   ?? o.dateCommande   ?? '',
      MontantTotalHT: o.MontantTotalHT ?? o.montantTotalHT ?? o.montantTotalHt ?? 0,
      MontantTTC:     o.MontantTTC     ?? o.montantTTC     ?? o.montantTtc    ?? 0,
      Statut:         o.Statut         ?? o.statut         ?? '',
      Id_Client:      o.Id_Client      ?? o.id_Client      ?? o.idClient      ?? 0,
      Lignes: (o.Lignes ?? o.lignes ?? []).map((l: any) => this.normalizeLigne(l)),
    };
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
    };
  }

  // Map enum string name → EtatCommande number
  statutToNumber(statut: string): number {
    const map: Record<string, number> = {
      Brouillon: 0, EnAttente: 1, Validee: 2,
      Expediee: 3, Livree: 4, Annulee: 5,
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

  // Valid next statuses for workflow
  getNextStatuses(current: string): { label: string; value: EtatCommande }[] {
    const n = this.statutToNumber(current);
    const transitions: Record<number, EtatCommande[]> = {
      0: [EtatCommande.EnAttente, EtatCommande.Annulee],
      1: [EtatCommande.Validee,   EtatCommande.Annulee],
      2: [EtatCommande.Expediee,  EtatCommande.Annulee],
      3: [EtatCommande.Livree,    EtatCommande.Annulee],
    };
    return (transitions[n] ?? []).map(v => ({ value: v, label: ETAT_LABELS[v] }));
  }

  // ── CRUD ───────────────────────────────────────────────────────────────────

  getOrders(page = 1, pageSize = 20): Observable<CommandeDto[]> {
    return this.api.get<any>(`${this.base}?page=${page}&pageSize=${pageSize}`).pipe(
      map(r => {
        const raw = this.unwrap<any[]>(r) ?? [];
        return Array.isArray(raw) ? raw.map(o => this.normalizeOrder(o)) : [];
      })
    );
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
