import { Injectable } from '@angular/core';
import jsPDF from 'jspdf';
import { BonCommandeDto } from '../../features/documents/bons-commandes/services/bon-commande.service';
import { BonLivraisonDto } from '../../features/documents/bons-livraison/services/bon-livraison.service';
import { FactureDto } from '../../features/documents/factures/services/facture.service';

// Brand colors
const C_PRIMARY   = [0,   119, 182] as const;   // #0077b6
const C_DARK      = [10,  22,  40 ] as const;   // #0a1628
const C_GRAY      = [107, 114, 128] as const;   // #6b7280
const C_LIGHT_BG  = [248, 250, 252] as const;   // #f8fafc
const C_BORDER    = [229, 231, 235] as const;   // #e5e7eb
const C_WHITE     = [255, 255, 255] as const;
const C_SUCCESS   = [5,   150, 105] as const;   // #059669

const PAGE_W  = 210;
const PAGE_H  = 297;
const MARGIN  = 18;
const COL_W   = PAGE_W - MARGIN * 2;

@Injectable({ providedIn: 'root' })
export class PdfService {

  // ── Public API ────────────────────────────────────────────────────────

  downloadBonCommande(bon: BonCommandeDto): void {
    const doc = this._base();
    const ref = String(bon.numero_Doc ?? 0).padStart(5, '0');
    let y = this._header(doc, 'BON DE COMMANDE', `BC-${ref}`);
    y = this._section(doc, y, 'INFORMATIONS GÉNÉRALES', [
      ['Numéro',           `BC-${ref}`],
      ['Nom',              bon.nom_Doc ?? '—'],
      ['Date de création', this._date(bon.dateCreation)],
      ['Client',           `Client n° ${bon.id_Client ?? '—'}`],
    ]);
    this._footer(doc);
    doc.save(`BC-${ref}.pdf`);
  }

  downloadBonLivraison(bon: BonLivraisonDto): void {
    const doc = this._base();
    const ref = String(bon.numero_Doc ?? 0).padStart(5, '0');
    let y = this._header(doc, 'BON DE LIVRAISON', `BL-${ref}`);
    y = this._section(doc, y, 'INFORMATIONS GÉNÉRALES', [
      ['Numéro',           `BL-${ref}`],
      ['Nom',              bon.nom_Doc ?? '—'],
      ['Date de création', this._date(bon.dateCreation)],
      ['Client',           `Client n° ${bon.id_Client ?? '—'}`],
    ]);
    this._footer(doc);
    doc.save(`BL-${ref}.pdf`);
  }

  downloadFacture(facture: FactureDto): void {
    const doc = this._base();
    const ref = String(facture.numero_Doc ?? 0).padStart(5, '0');
    let y = this._header(doc, 'FACTURE', `FAC-${ref}`);
    y = this._section(doc, y, 'INFORMATIONS GÉNÉRALES', [
      ['Numéro',       `FAC-${ref}`],
      ['Nom',          facture.nom_Doc ?? '—'],
      ['Date facture', this._date(facture.dateFacture)],
      ['Client',       `Client n° ${facture.id_Client ?? '—'}`],
    ]);
    y = this._amountBlock(doc, y, facture);
    this._footer(doc);
    doc.save(`FAC-${ref}.pdf`);
  }

  // ── Layout builders ───────────────────────────────────────────────────

  private _base(): jsPDF {
    return new jsPDF({ orientation: 'p', unit: 'mm', format: 'a4' });
  }

  /** Renders the top banner + company block + doc title. Returns next Y. */
  private _header(doc: jsPDF, title: string, ref: string): number {
    // Top accent bar
    doc.setFillColor(...C_PRIMARY);
    doc.rect(0, 0, PAGE_W, 2, 'F');

    // Header background
    doc.setFillColor(...C_LIGHT_BG);
    doc.rect(0, 2, PAGE_W, 36, 'F');

    // Left accent stripe
    doc.setFillColor(...C_PRIMARY);
    doc.rect(0, 2, 4, 36, 'F');

    // Company name
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(18);
    doc.setTextColor(...C_DARK);
    doc.text('CYNAPHARM', MARGIN + 4, 18);

    // Company subtitle
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(8);
    doc.setTextColor(...C_GRAY);
    doc.text('Gestion pharmaceutique', MARGIN + 4, 24);

    // Document title (right-aligned)
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(16);
    doc.setTextColor(...C_PRIMARY);
    doc.text(title, PAGE_W - MARGIN, 17, { align: 'right' });

    // Document reference
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(9);
    doc.setTextColor(...C_GRAY);
    doc.text(`Réf. : ${ref}`, PAGE_W - MARGIN, 24, { align: 'right' });

    // Generated date
    const now = new Date();
    doc.text(`Généré le ${this._formatDate(now)}`, PAGE_W - MARGIN, 30, { align: 'right' });

    // Separator line
    doc.setDrawColor(...C_BORDER);
    doc.setLineWidth(0.3);
    doc.line(MARGIN, 42, PAGE_W - MARGIN, 42);

    return 52;
  }

  /**
   * Renders a labeled info grid section.
   * highlight = true draws the row values in primary color (used for totals).
   */
  private _section(
    doc: jsPDF,
    startY: number,
    sectionTitle: string,
    rows: [string, string][],
    highlight = false
  ): number {
    let y = startY;

    // Section title
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(8);
    doc.setTextColor(...C_PRIMARY);
    doc.text(sectionTitle, MARGIN, y);
    y += 3;

    // Title underline
    doc.setDrawColor(...C_PRIMARY);
    doc.setLineWidth(0.5);
    doc.line(MARGIN, y, MARGIN + 40, y);
    y += 4;

    const ROW_H   = 9;
    const LABEL_W = 60;

    rows.forEach(([ label, value ], i) => {
      const rowY = y + i * ROW_H;

      // Alternating row background
      if (i % 2 === 0) {
        doc.setFillColor(...C_LIGHT_BG);
        doc.roundedRect(MARGIN, rowY - 5, COL_W, ROW_H, 1, 1, 'F');
      }

      // Label
      doc.setFont('helvetica', 'normal');
      doc.setFontSize(9);
      doc.setTextColor(...C_GRAY);
      doc.text(label, MARGIN + 4, rowY);

      // Separator dot
      doc.setTextColor(...C_BORDER);
      doc.text('·', MARGIN + LABEL_W - 6, rowY);

      // Value
      doc.setFont('helvetica', highlight ? 'bold' : 'normal');
      doc.setFontSize(highlight ? 11 : 9);
      doc.setTextColor(...(highlight ? C_DARK : C_DARK));
      doc.text(value, MARGIN + LABEL_W, rowY);
    });

    return y + rows.length * ROW_H + 8;
  }

  /** Facture-specific amount block with HT / TVA / TTC breakdown. */
  private _amountBlock(doc: jsPDF, startY: number, f: FactureDto): number {
    let y = startY;

    // Section title
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(8);
    doc.setTextColor(...C_PRIMARY);
    doc.text('DÉTAIL DES MONTANTS', MARGIN, y);
    y += 3;
    doc.setDrawColor(...C_PRIMARY);
    doc.setLineWidth(0.5);
    doc.line(MARGIN, y, MARGIN + 50, y);
    y += 6;

    const ROW_H   = 9;
    const LABEL_W = 60;

    // HT row
    doc.setFillColor(...C_LIGHT_BG);
    doc.roundedRect(MARGIN, y - 5, COL_W, ROW_H, 1, 1, 'F');
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(9);
    doc.setTextColor(...C_GRAY);
    doc.text('Montant HT', MARGIN + 4, y);
    doc.setTextColor(...C_DARK);
    doc.text(this._currency(f.montantHT ?? 0), MARGIN + LABEL_W, y);
    y += ROW_H;

    // TVA row (computed, since TVA % is not in FactureDto)
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(9);
    doc.setTextColor(...C_GRAY);
    doc.text('TVA', MARGIN + 4, y);
    doc.setTextColor(...C_DARK);
    const tvaAmount = (f.montantTTC ?? 0) - (f.montantHT ?? 0);
    doc.text(this._currency(tvaAmount), MARGIN + LABEL_W, y);
    y += ROW_H + 3;

    // TTC total highlight box
    doc.setFillColor(...C_PRIMARY);
    doc.roundedRect(MARGIN, y - 5, COL_W, 12, 2, 2, 'F');
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(10);
    doc.setTextColor(...C_WHITE);
    doc.text('MONTANT TTC', MARGIN + 4, y + 2);
    doc.setFontSize(12);
    doc.text(this._currency(f.montantTTC ?? 0), PAGE_W - MARGIN - 4, y + 2, { align: 'right' });
    y += 18;

    return y;
  }

  /** Footer: thin line + metadata centered. */
  private _footer(doc: jsPDF): void {
    const y = PAGE_H - 14;
    doc.setDrawColor(...C_BORDER);
    doc.setLineWidth(0.3);
    doc.line(MARGIN, y, PAGE_W - MARGIN, y);

    doc.setFont('helvetica', 'normal');
    doc.setFontSize(7.5);
    doc.setTextColor(...C_GRAY);
    const now = new Date();
    doc.text(
      `Cynapharm · Document généré automatiquement le ${this._formatDate(now)} à ${this._formatTime(now)}`,
      PAGE_W / 2, y + 5,
      { align: 'center' }
    );
  }

  // ── Helpers ───────────────────────────────────────────────────────────

  private _date(d?: string): string {
    if (!d) return '—';
    return new Date(d).toLocaleDateString('fr-TN', { day: '2-digit', month: '2-digit', year: 'numeric' });
  }

  private _formatDate(d: Date): string {
    return d.toLocaleDateString('fr-TN', { day: '2-digit', month: '2-digit', year: 'numeric' });
  }

  private _formatTime(d: Date): string {
    return d.toLocaleTimeString('fr-TN', { hour: '2-digit', minute: '2-digit' });
  }

  private _currency(amount: number): string {
    return new Intl.NumberFormat('fr-TN', {
      minimumFractionDigits: 3,
      maximumFractionDigits: 3,
    }).format(amount ?? 0) + ' TND';
  }
}
