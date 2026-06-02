import { Injectable } from '@angular/core';
import jsPDF from 'jspdf';

// ── Brand colors ──────────────────────────────────────────────────────────
const C_PRIMARY   = [0,   119, 182] as const;   // #0077b6
const C_DARK      = [10,  22,  40 ] as const;   // #0a1628
const C_GRAY      = [107, 114, 128] as const;   // #6b7280
const C_LIGHT_BG  = [248, 250, 252] as const;   // #f8fafc
const C_BORDER    = [229, 231, 235] as const;   // #e5e7eb
const C_WHITE     = [255, 255, 255] as const;

// ── Page geometry ─────────────────────────────────────────────────────────
const PAGE_W  = 210;
const PAGE_H  = 297;
const MARGIN  = 18;
const COL_W   = PAGE_W - MARGIN * 2;
const BODY_BOTTOM = PAGE_H - 26;   // leave room for footer / page number

// ── Company fiscal identity (CynaPharm) ─────────────────────────────────────
// Centralised so every document carries consistent legal information.
export const CYNAPHARM_COMPANY = {
  name:    'CYNAPHARM',
  legal:   'CynaPharm SARL — Gestion pharmaceutique',
  address: 'Rue de la Santé, Tunis 1002, Tunisie',
  phone:   '+216 71 000 000',
  email:   'contact@cynapharm.tn',
  mf:      '0000000A/M/000',          // Matricule Fiscal
  rib:     'TN59 0000 0000 0000 0000 0000',
} as const;

// ── View-models ─────────────────────────────────────────────────────────────
export interface PdfParty {
  name?:    string;
  address?: string;
  phone?:   string;
  email?:   string;
  mf?:      string;           // Matricule Fiscal (client)
}

export interface PdfLine {
  designation:    string;
  quantite:       number;
  prixUnitaireHT: number;
  remisePct?:     number;     // discount %
  tvaPct:         number;     // VAT rate %
}

export interface BonCommandePdfData {
  numeroDoc:  number;
  commandeId?: number;
  date?:      string;
  client:     PdfParty;
  lignes:     PdfLine[];
}

export interface BonLivraisonPdfData {
  numeroDoc:    number;
  commandeId?:  number;
  date?:        string;
  client:       PdfParty;
  lignes:       PdfLine[];
  transporteur?: string;
}

export interface FacturePdfData {
  numeroDoc:    number;
  commandeId?:  number;
  dateFacture?: string;
  echeance?:    string;
  client:       PdfParty;
  lignes:       PdfLine[];
  // Fallback totals when the order has no detailed lines.
  fallbackHT?:  number;
  fallbackTTC?: number;
}

interface Column {
  header: string;
  width:  number;                 // mm
  align:  'left' | 'right' | 'center';
  get:    (l: PdfLine) => string;
}

@Injectable({ providedIn: 'root' })
export class PdfService {

  // ── Public API — build (returns the jsPDF doc) ───────────────────────────

  buildBonCommande(data: BonCommandePdfData): jsPDF {
    const doc = this._base();
    const ref = this._ref('BC', data.numeroDoc);
    let y = this._header(doc, 'BON DE COMMANDE', ref, this._cmdRef(data.commandeId), data.date);
    y = this._parties(doc, y, data.client);
    y = this._productTable(doc, y, data.lignes, 'BC');
    y = this._totals(doc, y, data.lignes, { showTtc: true });
    this._signature(doc, y, 'Cachet et signature du fournisseur');
    this._footer(doc);
    return doc;
  }

  buildBonLivraison(data: BonLivraisonPdfData): jsPDF {
    const doc = this._base();
    const ref = this._ref('BL', data.numeroDoc);
    let y = this._header(doc, 'BON DE LIVRAISON', ref, this._cmdRef(data.commandeId), data.date);
    y = this._parties(doc, y, data.client, {
      'Bon de commande': this._cmdRef(data.commandeId),
      'Transporteur':    data.transporteur || 'Livraison directe CynaPharm',
    });
    y = this._productTable(doc, y, data.lignes, 'BL');
    this._signature(doc, y, 'Reçu par (nom, date et signature)');
    this._footer(doc);
    return doc;
  }

  buildFacture(data: FacturePdfData): jsPDF {
    const doc = this._base();
    const ref = this._ref('FAC', data.numeroDoc);
    let y = this._header(doc, 'FACTURE', ref, this._cmdRef(data.commandeId), data.dateFacture);
    y = this._parties(doc, y, data.client, {
      'Date facture':   this._date(data.dateFacture),
      'Échéance':       data.echeance ? this._date(data.echeance) : 'À réception',
      'Bon de commande': this._cmdRef(data.commandeId),
    });
    y = this._productTable(doc, y, data.lignes, 'FAC');
    y = this._tvaBreakdown(doc, y, data.lignes);
    y = this._totals(doc, y, data.lignes, {
      showTtc:     true,
      fallbackHT:  data.fallbackHT,
      fallbackTTC: data.fallbackTTC,
    });
    y = this._legalMention(doc, y, data.lignes, data.fallbackTTC);
    this._signature(doc, y, 'Cachet et signature');
    this._footer(doc);
    return doc;
  }

  // ── Public API — download (build + save) ─────────────────────────────────

  downloadBonCommande(data: BonCommandePdfData): void {
    this.buildBonCommande(data).save(`${this._ref('BC', data.numeroDoc)}.pdf`);
  }
  downloadBonLivraison(data: BonLivraisonPdfData): void {
    this.buildBonLivraison(data).save(`${this._ref('BL', data.numeroDoc)}.pdf`);
  }
  downloadFacture(data: FacturePdfData): void {
    this.buildFacture(data).save(`${this._ref('FAC', data.numeroDoc)}.pdf`);
  }

  /** Render a document to a Blob (used for Cloudinary upload). */
  toBlob(doc: jsPDF): Blob {
    return doc.output('blob');
  }

  // ── Layout builders ───────────────────────────────────────────────────────

  private _base(): jsPDF {
    return new jsPDF({ orientation: 'p', unit: 'mm', format: 'a4' });
  }

  /** Small vector brand mark (rounded square + white cross) used as a logo. */
  private _brandMark(doc: jsPDF, x: number, y: number, size: number): void {
    doc.setFillColor(...C_PRIMARY);
    doc.roundedRect(x, y, size, size, 1.5, 1.5, 'F');
    doc.setFillColor(...C_WHITE);
    const arm = size * 0.18;
    const cx = x + size / 2, cy = y + size / 2;
    doc.rect(cx - arm / 2, y + size * 0.22, arm, size * 0.56, 'F');   // vertical
    doc.rect(x + size * 0.22, cy - arm / 2, size * 0.56, arm, 'F');   // horizontal
  }

  /** Top banner: logo + company fiscal block (left) and doc title/ref (right). */
  private _header(
    doc: jsPDF,
    title: string,
    ref: string,
    cmdRef: string,
    date?: string,
  ): number {
    doc.setFillColor(...C_PRIMARY);
    doc.rect(0, 0, PAGE_W, 2, 'F');                 // accent bar
    doc.setFillColor(...C_LIGHT_BG);
    doc.rect(0, 2, PAGE_W, 44, 'F');                // header background
    doc.setFillColor(...C_PRIMARY);
    doc.rect(0, 2, 4, 44, 'F');                     // left stripe

    // Logo + company name
    this._brandMark(doc, MARGIN + 2, 7, 12);
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(17);
    doc.setTextColor(...C_DARK);
    doc.text(CYNAPHARM_COMPANY.name, MARGIN + 18, 14);

    // Company fiscal identity
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(7.5);
    doc.setTextColor(...C_GRAY);
    const info = [
      CYNAPHARM_COMPANY.address,
      `Tél : ${CYNAPHARM_COMPANY.phone}   ·   ${CYNAPHARM_COMPANY.email}`,
      `MF : ${CYNAPHARM_COMPANY.mf}`,
    ];
    info.forEach((line, i) => doc.text(line, MARGIN + 18, 20 + i * 4));

    // Document title + reference (right)
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(16);
    doc.setTextColor(...C_PRIMARY);
    doc.text(title, PAGE_W - MARGIN, 13, { align: 'right' });

    doc.setFont('helvetica', 'normal');
    doc.setFontSize(8.5);
    doc.setTextColor(...C_GRAY);
    const right = [
      `Réf. : ${ref}`,
      `Commande : ${cmdRef}`,
      date ? `Date : ${this._date(date)}` : `Généré le ${this._fmtDate(new Date())}`,
    ];
    right.forEach((line, i) => doc.text(line, PAGE_W - MARGIN, 20 + i * 4.5, { align: 'right' }));

    doc.setDrawColor(...C_BORDER);
    doc.setLineWidth(0.3);
    doc.line(MARGIN, 50, PAGE_W - MARGIN, 50);
    return 58;
  }

  /** Client identity block (left) + optional document meta box (right). */
  private _parties(
    doc: jsPDF,
    startY: number,
    client: PdfParty,
    meta?: Record<string, string>,
  ): number {
    const boxW = meta ? (COL_W - 6) / 2 : COL_W;
    const boxH = 34;

    // Client box
    doc.setFillColor(...C_LIGHT_BG);
    doc.roundedRect(MARGIN, startY, boxW, boxH, 2, 2, 'F');
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(8);
    doc.setTextColor(...C_PRIMARY);
    doc.text('CLIENT', MARGIN + 4, startY + 6);

    doc.setFontSize(9);
    doc.setTextColor(...C_DARK);
    doc.setFont('helvetica', 'bold');
    doc.text(client.name || '—', MARGIN + 4, startY + 12);

    doc.setFont('helvetica', 'normal');
    doc.setFontSize(8);
    doc.setTextColor(...C_GRAY);
    const lines = [
      client.address || 'Adresse non renseignée',
      client.phone ? `Tél : ${client.phone}` : '',
      client.email || '',
      `MF : ${client.mf || '—'}`,
    ].filter(Boolean);
    lines.forEach((l, i) => doc.text(this._truncate(doc, l, boxW - 8), MARGIN + 4, startY + 18 + i * 4));

    // Meta box (right)
    if (meta) {
      const mx = MARGIN + boxW + 6;
      doc.setFillColor(...C_LIGHT_BG);
      doc.roundedRect(mx, startY, boxW, boxH, 2, 2, 'F');
      doc.setFont('helvetica', 'bold');
      doc.setFontSize(8);
      doc.setTextColor(...C_PRIMARY);
      doc.text('INFORMATIONS', mx + 4, startY + 6);

      doc.setFontSize(8);
      let my = startY + 12;
      Object.entries(meta).forEach(([k, v]) => {
        doc.setFont('helvetica', 'normal');
        doc.setTextColor(...C_GRAY);
        doc.text(k, mx + 4, my);
        doc.setFont('helvetica', 'bold');
        doc.setTextColor(...C_DARK);
        doc.text(v || '—', mx + boxW - 4, my, { align: 'right' });
        my += 5.5;
      });
    }

    return startY + boxH + 8;
  }

  /** Columns per document type. */
  private _columns(kind: 'BC' | 'BL' | 'FAC'): Column[] {
    const money = (n: number) => this._money(n);
    if (kind === 'BL') {
      return [
        { header: 'Désignation', width: COL_W - 30, align: 'left',  get: l => l.designation },
        { header: 'Qté livrée',  width: 30,         align: 'right', get: l => String(l.quantite) },
      ];
    }
    if (kind === 'FAC') {
      return [
        { header: 'Désignation',  width: COL_W - 95, align: 'left',   get: l => l.designation },
        { header: 'Qté',          width: 13,         align: 'right',  get: l => String(l.quantite) },
        { header: 'P.U. HT',      width: 24,         align: 'right',  get: l => money(l.prixUnitaireHT) },
        { header: 'TVA %',        width: 14,         align: 'right',  get: l => `${l.tvaPct}%` },
        { header: 'Total TTC',    width: 28,         align: 'right',  get: l => money(this._lineTTC(l)) },
      ];
    }
    // BC
    return [
      { header: 'Désignation', width: COL_W - 86, align: 'left',  get: l => l.designation },
      { header: 'Qté',         width: 14,         align: 'right', get: l => String(l.quantite) },
      { header: 'P.U. HT',     width: 26,         align: 'right', get: l => money(l.prixUnitaireHT) },
      { header: 'Remise',      width: 16,         align: 'right', get: l => `${l.remisePct ?? 0}%` },
      { header: 'Total HT',    width: 30,         align: 'right', get: l => money(this._lineHT(l)) },
    ];
  }

  /** Renders the products table with header repetition + page breaks. */
  private _productTable(
    doc: jsPDF,
    startY: number,
    lignes: PdfLine[],
    kind: 'BC' | 'BL' | 'FAC',
  ): number {
    const cols = this._columns(kind);
    const HEAD_H = 8;
    const ROW_H  = 7;
    let y = startY;

    const drawHead = () => {
      doc.setFillColor(...C_PRIMARY);
      doc.rect(MARGIN, y, COL_W, HEAD_H, 'F');
      doc.setFont('helvetica', 'bold');
      doc.setFontSize(8);
      doc.setTextColor(...C_WHITE);
      let x = MARGIN;
      cols.forEach(c => {
        const tx = c.align === 'right' ? x + c.width - 2 : c.align === 'center' ? x + c.width / 2 : x + 2;
        doc.text(c.header, tx, y + 5.5, { align: c.align });
        x += c.width;
      });
      y += HEAD_H;
    };

    drawHead();

    if (lignes.length === 0) {
      doc.setFont('helvetica', 'italic');
      doc.setFontSize(8.5);
      doc.setTextColor(...C_GRAY);
      doc.text('Aucune ligne de produit pour cette commande.', MARGIN + 2, y + 5);
      return y + ROW_H + 4;
    }

    doc.setFont('helvetica', 'normal');
    doc.setFontSize(8.5);
    lignes.forEach((l, i) => {
      if (y + ROW_H > BODY_BOTTOM) {           // page break
        doc.addPage();
        y = MARGIN;
        drawHead();
        doc.setFont('helvetica', 'normal');
        doc.setFontSize(8.5);
      }
      if (i % 2 === 0) {
        doc.setFillColor(...C_LIGHT_BG);
        doc.rect(MARGIN, y, COL_W, ROW_H, 'F');
      }
      let x = MARGIN;
      doc.setTextColor(...C_DARK);
      cols.forEach(c => {
        const tx = c.align === 'right' ? x + c.width - 2 : c.align === 'center' ? x + c.width / 2 : x + 2;
        doc.text(this._truncate(doc, c.get(l), c.width - 3), tx, y + 4.8, { align: c.align });
        x += c.width;
      });
      y += ROW_H;
    });

    doc.setDrawColor(...C_BORDER);
    doc.setLineWidth(0.2);
    doc.rect(MARGIN, startY, COL_W, y - startY);
    return y + 6;
  }

  /** TVA recap by rate (Facture only). */
  private _tvaBreakdown(doc: jsPDF, startY: number, lignes: PdfLine[]): number {
    if (lignes.length === 0) return startY;
    const buckets = this._tvaBuckets(lignes);
    let y = this._sectionTitle(doc, startY, 'RÉCAPITULATIF TVA');

    const cols = [
      { h: 'Taux',         w: 30, a: 'left'  as const },
      { h: 'Base HT',      w: 50, a: 'right' as const },
      { h: 'Montant TVA',  w: 50, a: 'right' as const },
    ];
    const tableW = cols.reduce((s, c) => s + c.w, 0);
    const HEAD_H = 7, ROW_H = 6;

    doc.setFillColor(...C_DARK);
    doc.rect(MARGIN, y, tableW, HEAD_H, 'F');
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(7.5);
    doc.setTextColor(...C_WHITE);
    let x = MARGIN;
    cols.forEach(c => {
      doc.text(c.h, c.a === 'right' ? x + c.w - 2 : x + 2, y + 5, { align: c.a });
      x += c.w;
    });
    y += HEAD_H;

    doc.setFont('helvetica', 'normal');
    doc.setFontSize(8);
    buckets.forEach((b, i) => {
      if (i % 2 === 0) { doc.setFillColor(...C_LIGHT_BG); doc.rect(MARGIN, y, tableW, ROW_H, 'F'); }
      doc.setTextColor(...C_DARK);
      doc.text(`TVA ${b.rate}%`, MARGIN + 2, y + 4);
      doc.text(this._money(b.base), MARGIN + cols[0].w + cols[1].w - 2, y + 4, { align: 'right' });
      doc.text(this._money(b.tva),  MARGIN + tableW - 2, y + 4, { align: 'right' });
      y += ROW_H;
    });

    doc.setDrawColor(...C_BORDER);
    doc.setLineWidth(0.2);
    doc.rect(MARGIN, startY + 7, tableW, y - (startY + 7));
    return y + 6;
  }

  /** HT / TVA / TTC totals block (right-aligned). */
  private _totals(
    doc: jsPDF,
    startY: number,
    lignes: PdfLine[],
    opts: { showTtc: boolean; fallbackHT?: number; fallbackTTC?: number },
  ): number {
    const hasLines = lignes.length > 0;
    const ht  = hasLines ? lignes.reduce((s, l) => s + this._lineHT(l), 0)  : (opts.fallbackHT ?? 0);
    const ttc = hasLines ? lignes.reduce((s, l) => s + this._lineTTC(l), 0) : (opts.fallbackTTC ?? 0);
    const tva = ttc - ht;

    const boxW = 80;
    const boxX = PAGE_W - MARGIN - boxW;
    let y = startY;

    const row = (label: string, value: string) => {
      doc.setFillColor(...C_LIGHT_BG);
      doc.rect(boxX, y, boxW, 8, 'F');
      doc.setFont('helvetica', 'normal');
      doc.setFontSize(9);
      doc.setTextColor(...C_GRAY);
      doc.text(label, boxX + 3, y + 5.5);
      doc.setTextColor(...C_DARK);
      doc.text(value, boxX + boxW - 3, y + 5.5, { align: 'right' });
      y += 8;
    };

    row('Total HT', this._money(ht));
    row('Total TVA', this._money(tva));

    if (opts.showTtc) {
      doc.setFillColor(...C_PRIMARY);
      doc.roundedRect(boxX, y, boxW, 11, 1.5, 1.5, 'F');
      doc.setFont('helvetica', 'bold');
      doc.setFontSize(10);
      doc.setTextColor(...C_WHITE);
      doc.text('TOTAL TTC', boxX + 3, y + 7);
      doc.setFontSize(11);
      doc.text(this._money(ttc), boxX + boxW - 3, y + 7, { align: 'right' });
      y += 11;
    }
    return y + 8;
  }

  /** Legal mention required on Tunisian invoices. */
  private _legalMention(doc: jsPDF, startY: number, lignes: PdfLine[], fallbackTTC?: number): number {
    const ttc = lignes.length > 0
      ? lignes.reduce((s, l) => s + this._lineTTC(l), 0)
      : (fallbackTTC ?? 0);

    let y = startY;
    doc.setFillColor(...C_LIGHT_BG);
    doc.roundedRect(MARGIN, y, COL_W, 16, 1.5, 1.5, 'F');
    doc.setFont('helvetica', 'italic');
    doc.setFontSize(8);
    doc.setTextColor(...C_DARK);
    const text = `Arrêtée la présente facture à la somme de : ${this._montantEnLettres(ttc)}.`;
    doc.text(doc.splitTextToSize(text, COL_W - 8), MARGIN + 4, y + 5);
    y += 16;

    doc.setFont('helvetica', 'normal');
    doc.setFontSize(7.5);
    doc.setTextColor(...C_GRAY);
    doc.text(
      `Conditions de règlement : à réception.  RIB : ${CYNAPHARM_COMPANY.rib}.  Timbre fiscal en sus selon législation en vigueur.`,
      MARGIN, y + 4,
    );
    return y + 10;
  }

  /** Signature / cachet zone (bottom-right). */
  private _signature(doc: jsPDF, startY: number, label: string): void {
    const y = Math.min(startY + 4, BODY_BOTTOM - 24);
    const boxW = 70;
    const boxX = PAGE_W - MARGIN - boxW;
    doc.setDrawColor(...C_BORDER);
    doc.setLineWidth(0.3);
    doc.roundedRect(boxX, y, boxW, 22, 1.5, 1.5, 'S');
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(7.5);
    doc.setTextColor(...C_GRAY);
    doc.text(label, boxX + boxW / 2, y + 5, { align: 'center', maxWidth: boxW - 4 });
  }

  /** Footer line + centered metadata + page numbers on every page. */
  private _footer(doc: jsPDF): void {
    const total = doc.getNumberOfPages();
    const now = new Date();
    for (let p = 1; p <= total; p++) {
      doc.setPage(p);
      const y = PAGE_H - 14;
      doc.setDrawColor(...C_BORDER);
      doc.setLineWidth(0.3);
      doc.line(MARGIN, y, PAGE_W - MARGIN, y);
      doc.setFont('helvetica', 'normal');
      doc.setFontSize(7.5);
      doc.setTextColor(...C_GRAY);
      doc.text(
        `${CYNAPHARM_COMPANY.legal} · MF ${CYNAPHARM_COMPANY.mf}`,
        MARGIN, y + 5,
      );
      doc.text(`Page ${p} / ${total}`, PAGE_W - MARGIN, y + 5, { align: 'right' });
      doc.text(`Généré le ${this._fmtDate(now)} à ${this._fmtTime(now)}`, PAGE_W / 2, y + 5, { align: 'center' });
    }
  }

  private _sectionTitle(doc: jsPDF, startY: number, title: string): number {
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(8);
    doc.setTextColor(...C_PRIMARY);
    doc.text(title, MARGIN, startY);
    doc.setDrawColor(...C_PRIMARY);
    doc.setLineWidth(0.5);
    doc.line(MARGIN, startY + 1.5, MARGIN + 45, startY + 1.5);
    return startY + 7;
  }

  // ── Math helpers ───────────────────────────────────────────────────────────

  private _lineHT(l: PdfLine): number {
    return l.quantite * l.prixUnitaireHT * (1 - (l.remisePct ?? 0) / 100);
  }
  private _lineTTC(l: PdfLine): number {
    return this._lineHT(l) * (1 + l.tvaPct / 100);
  }
  private _tvaBuckets(lignes: PdfLine[]): { rate: number; base: number; tva: number }[] {
    const map = new Map<number, { base: number; tva: number }>();
    lignes.forEach(l => {
      const ht = this._lineHT(l);
      const cur = map.get(l.tvaPct) ?? { base: 0, tva: 0 };
      cur.base += ht;
      cur.tva  += ht * l.tvaPct / 100;
      map.set(l.tvaPct, cur);
    });
    return [...map.entries()]
      .sort((a, b) => a[0] - b[0])
      .map(([rate, v]) => ({ rate, ...v }));
  }

  // ── Formatting helpers ───────────────────────────────────────────────────

  private _ref(prefix: string, n?: number): string {
    return `${prefix}-${String(n ?? 0).padStart(5, '0')}`;
  }
  private _cmdRef(id?: number): string {
    return id ? `CMD-${String(id).padStart(5, '0')}` : '—';
  }
  private _date(d?: string): string {
    if (!d) return '—';
    const parsed = new Date(d);
    return isNaN(parsed.getTime()) ? '—' : this._fmtDate(parsed);
  }
  private _fmtDate(d: Date): string {
    const p = (n: number) => String(n).padStart(2, '0');
    return `${p(d.getDate())}/${p(d.getMonth() + 1)}/${d.getFullYear()}`;
  }
  private _fmtTime(d: Date): string {
    const p = (n: number) => String(n).padStart(2, '0');
    return `${p(d.getHours())}:${p(d.getMinutes())}`;
  }

  /** Tunisian money format: thousands grouped by space, dot decimal, 3 digits. e.g. 1 250.000 TND */
  private _money(amount: number): string {
    const v = Math.round((amount ?? 0) * 1000) / 1000;
    const [int, dec] = v.toFixed(3).split('.');
    const grouped = int.replace(/\B(?=(\d{3})+(?!\d))/g, ' ');
    return `${grouped}.${dec} TND`;
  }

  /** Truncate a string to fit a column width (mm) at the current font size. */
  private _truncate(doc: jsPDF, text: string, maxW: number): string {
    if (doc.getTextWidth(text) <= maxW) return text;
    let t = text;
    while (t.length > 1 && doc.getTextWidth(t + '…') > maxW) t = t.slice(0, -1);
    return t + '…';
  }

  // ── Amount in French words (dinars + millimes) ──────────────────────────────

  private _montantEnLettres(amount: number): string {
    const v = Math.round((amount ?? 0) * 1000);
    const dinars  = Math.floor(v / 1000);
    const millimes = v % 1000;
    const dinPart = `${this._frenchWords(dinars)} dinar${dinars > 1 ? 's' : ''}`;
    const milPart = millimes > 0
      ? ` et ${this._frenchWords(millimes)} millime${millimes > 1 ? 's' : ''}`
      : '';
    return (dinPart + milPart).replace(/\s+/g, ' ').trim();
  }

  private _frenchWords(n: number): string {
    if (n === 0) return 'zéro';
    const units = ['', 'un', 'deux', 'trois', 'quatre', 'cinq', 'six', 'sept', 'huit', 'neuf',
      'dix', 'onze', 'douze', 'treize', 'quatorze', 'quinze', 'seize', 'dix-sept', 'dix-huit', 'dix-neuf'];
    const tens = ['', '', 'vingt', 'trente', 'quarante', 'cinquante', 'soixante', 'soixante', 'quatre-vingt', 'quatre-vingt'];

    const below100 = (x: number): string => {
      if (x < 20) return units[x];
      const t = Math.floor(x / 10), u = x % 10;
      if (t === 7 || t === 9) {
        const base = tens[t];
        const rem = below100(10 + u);
        return u === 1 && t === 7 ? `${base} et ${rem}` : `${base}-${rem}`;
      }
      let w = tens[t];
      if (u === 0) return t === 8 ? w + 's' : w;
      if (u === 1 && t < 8) return `${w} et un`;
      return `${w}-${units[u]}`;
    };

    const below1000 = (x: number): string => {
      const h = Math.floor(x / 100), r = x % 100;
      let w = '';
      if (h > 0) w = h === 1 ? 'cent' : `${units[h]} cent`;
      if (h > 1 && r === 0) w += 's';
      if (r > 0) w += (w ? ' ' : '') + below100(r);
      return w;
    };

    const parts: string[] = [];
    const millions = Math.floor(n / 1_000_000);
    const thousands = Math.floor((n % 1_000_000) / 1000);
    const rest = n % 1000;
    if (millions > 0) parts.push(millions === 1 ? 'un million' : `${below1000(millions)} millions`);
    if (thousands > 0) parts.push(thousands === 1 ? 'mille' : `${below1000(thousands)} mille`);
    if (rest > 0) parts.push(below1000(rest));
    return parts.join(' ').trim();
  }
}
