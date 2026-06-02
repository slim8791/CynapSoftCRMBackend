import { Injectable } from '@angular/core';
import { forkJoin, Observable, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';

import {
  PdfService, PdfParty, PdfLine,
  BonCommandePdfData, BonLivraisonPdfData, FacturePdfData,
} from '../../../shared/services/pdf.service';
import { CloudinaryService } from '../../../core/services/cloudinary.service';
import { OrderService, CommandeDto, LigneCommandeDto } from '../../orders/order.service';
import { ProductService } from '../../products/product.service';
import { UserService } from '../../users/user.service';
import { BonCommandeService, BonCommandeDto } from '../bons-commandes/services/bon-commande.service';
import { BonLivraisonService, BonLivraisonDto } from '../bons-livraison/services/bon-livraison.service';
import { FactureService, FactureDto } from '../factures/services/facture.service';

/** Result of a document generation: the rendered PDF blob + the persisted record (null if persistence failed). */
export interface DocResult<T> {
  blob:     Blob;
  document: T | null;
  fileName: string;
}

/**
 * Orchestrates document PDF generation end-to-end:
 *   1. gather the order, its product lines and the client identity
 *   2. render a rich PDF (PdfService)
 *   3. upload it to Cloudinary (raw)
 *   4. persist the document metadata + Cloudinary URL (DocAPI)
 *
 * This is the single entry point used when a BC / BL / Facture is created,
 * so every stored document has a downloadable, fiscally-complete PDF.
 */
@Injectable({ providedIn: 'root' })
export class DocumentGeneratorService {

  private readonly DEFAULT_TVA = 19;   // fallback rate when a product carries none

  constructor(
    private pdf:        PdfService,
    private cloudinary: CloudinaryService,
    private orders:     OrderService,
    private products:   ProductService,
    private users:      UserService,
    private bcSvc:      BonCommandeService,
    private blSvc:      BonLivraisonService,
    private factureSvc: FactureService,
  ) {}

  // ── Public API ─────────────────────────────────────────────────────────────

  /**
   * @param numeroDoc 0 = create a new document; >0 = regenerate/backfill an existing one.
   * Always resolves with the rendered `blob` (so the caller can download it directly,
   * even if the Cloudinary upload / persistence step fails).
   */
  generateBonCommande(commandeId: number, clientId?: number, numeroDoc = 0): Observable<DocResult<BonCommandeDto>> {
    return this._context(commandeId, clientId).pipe(
      switchMap(ctx => {
        const data: BonCommandePdfData = {
          numeroDoc,
          commandeId,
          date:       ctx.order?.DateCommande,
          client:     ctx.client,
          lignes:     ctx.lignes,
        };
        const blob = this.pdf.toBlob(this.pdf.buildBonCommande(data));
        return this._upload(blob, `BC-${commandeId}.pdf`).pipe(
          switchMap(url => this.bcSvc.createOrUpdate({
            numero_Doc: numeroDoc, nom_Doc: `BC-${commandeId}`,
            id_Commande: commandeId, id_Client: clientId, cloudinaryUrl: url,
          })),
          map(document => ({ blob, document, fileName: `BC-${this._pad(document?.numero_Doc ?? commandeId)}.pdf` })),
          catchError(() => of({ blob, document: null, fileName: `BC-${this._pad(numeroDoc || commandeId)}.pdf` })),
        );
      }),
    );
  }

  generateBonLivraison(commandeId: number, clientId?: number, numeroDoc = 0): Observable<DocResult<BonLivraisonDto>> {
    return this._context(commandeId, clientId).pipe(
      switchMap(ctx => {
        const data: BonLivraisonPdfData = {
          numeroDoc,
          commandeId,
          date:       new Date().toISOString(),
          client:     ctx.client,
          lignes:     ctx.lignes,
        };
        const blob = this.pdf.toBlob(this.pdf.buildBonLivraison(data));
        return this._upload(blob, `BL-${commandeId}.pdf`).pipe(
          switchMap(url => this.blSvc.createOrUpdate({
            numero_Doc: numeroDoc, nom_Doc: `BL-${commandeId}`,
            id_Commande: commandeId, id_Client: clientId, cloudinaryUrl: url,
          })),
          map(document => ({ blob, document, fileName: `BL-${this._pad(document?.numero_Doc ?? commandeId)}.pdf` })),
          catchError(() => of({ blob, document: null, fileName: `BL-${this._pad(numeroDoc || commandeId)}.pdf` })),
        );
      }),
    );
  }

  generateFacture(commandeId: number, clientId?: number, numeroDoc = 0): Observable<DocResult<FactureDto>> {
    return this._context(commandeId, clientId).pipe(
      switchMap(ctx => {
        const data: FacturePdfData = {
          numeroDoc,
          commandeId,
          dateFacture: new Date().toISOString(),
          client:      ctx.client,
          lignes:      ctx.lignes,
          fallbackHT:  ctx.order?.MontantTotalHT ?? 0,
          fallbackTTC: ctx.order?.MontantTTC ?? 0,
        };
        const blob = this.pdf.toBlob(this.pdf.buildFacture(data));
        return this._upload(blob, `FAC-${commandeId}.pdf`).pipe(
          switchMap(url => this.factureSvc.createOrUpdate({
            numero_Doc: numeroDoc, nom_Doc: `FAC-${commandeId}`,
            id_Commande: commandeId, id_Client: clientId,
            montantHT:  ctx.order?.MontantTotalHT ?? 0,
            montantTTC: ctx.order?.MontantTTC ?? 0,
            dateFacture: data.dateFacture,
            cloudinaryUrl: url,
          })),
          map(document => ({ blob, document, fileName: `FAC-${this._pad(document?.numero_Doc ?? commandeId)}.pdf` })),
          catchError(() => of({ blob, document: null, fileName: `FAC-${this._pad(numeroDoc || commandeId)}.pdf` })),
        );
      }),
    );
  }

  private _pad(n: number): string {
    return String(n ?? 0).padStart(5, '0');
  }

  // ── Internals ────────────────────────────────────────────────────────────

  /** Loads order + product details + client identity into a render context. */
  private _context(commandeId: number, clientId?: number) {
    return forkJoin({
      order:  this.orders.getOrderById(commandeId).pipe(catchError(() => of(null))),
      client: clientId
        ? this.users.getUserById(clientId).pipe(catchError(() => of(null)))
        : of(null),
    }).pipe(
      switchMap(({ order, client }) => {
        const lignes = order?.Lignes ?? [];
        const ids = [...new Set(lignes.map(l => l.Id_Produit).filter(id => id > 0))];

        const products$ = ids.length
          ? forkJoin(ids.map(id =>
              this.products.getProductById(id).pipe(
                map(p => ({ id, raw: this._unwrap(p) })),
                catchError(() => of({ id, raw: null })),
              )))
          : of([] as { id: number; raw: any }[]);

        return products$.pipe(map(products => {
          const byId = new Map(products.map(p => [p.id, p.raw]));
          return {
            order,
            client: this._toParty(this._unwrap(client), clientId),
            lignes: lignes.map(l => this._toLine(l, byId.get(l.Id_Produit))),
          };
        }));
      }),
    );
  }

  private _toLine(l: LigneCommandeDto, product: any): PdfLine {
    const tva = product?.TVA ?? product?.tva ?? this.DEFAULT_TVA;
    return {
      designation:    product?.Nom ?? product?.nom ?? `Produit #${l.Id_Produit}`,
      quantite:       l.Quantite ?? 0,
      prixUnitaireHT: l.PrixUnitaire ?? product?.PrixVente ?? product?.prixVente ?? 0,
      remisePct:      l.Remise ?? 0,
      tvaPct:         Number(tva) || this.DEFAULT_TVA,
    };
  }

  private _toParty(u: any, fallbackId?: number): PdfParty {
    if (!u) return { name: fallbackId ? `Client #${fallbackId}` : '—' };
    return {
      name:    u.name ?? u.Name ?? u.fullName ?? u.FullName ?? u.email ?? u.Email ?? `Client #${fallbackId ?? ''}`,
      address: u.adresse ?? u.Adresse ?? u.address ?? u.Address ?? '',
      phone:   u.phoneNumber ?? u.PhoneNumber ?? u.telephone ?? u.Telephone ?? '',
      email:   u.email ?? u.Email ?? '',
      mf:      u.matriculeFiscal ?? u.MatriculeFiscal ?? u.mf ?? u.MF ?? '',
    };
  }

  private _unwrap(r: any): any {
    return r?.Result ?? r?.result ?? r?.Data ?? r?.data ?? r;
  }

  /** Wraps the blob as a File and uploads it to Cloudinary (raw resource). */
  private _upload(blob: Blob, fileName: string): Observable<string> {
    const file = new File([blob], fileName, { type: 'application/pdf' });
    return this.cloudinary.uploadFile(file);
  }
}
