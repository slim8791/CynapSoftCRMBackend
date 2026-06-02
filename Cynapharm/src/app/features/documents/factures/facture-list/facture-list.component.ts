import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { FactureService, FactureDto } from '../services/facture.service';
import { CurrencyTNDPipe } from '../../../../shared/pipes/currency-tnd.pipe';
import { PaginatorComponent } from '../../../../shared/components/paginator/paginator.component';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { UserService } from '../../../users/user.service';
import { DocumentGeneratorService } from '../../services/document-generator.service';

@Component({
  selector: 'app-facture-list',
  standalone: true,
  imports: [CommonModule, RouterLink, CurrencyTNDPipe, PaginatorComponent, EmptyStateComponent],
  templateUrl: './facture-list.component.html',
  styleUrls: ['./facture-list.component.css']
})
export class FactureListComponent implements OnInit, OnDestroy {
  factures: FactureDto[] = [];
  loading = false; error = ''; page = 1; pageSize = 20; total = 0;
  regenerating: Record<number, boolean> = {};
  clientNames: Record<number, string> = {};
  private d$ = new Subject<void>();
  constructor(private svc: FactureService, private cdr: ChangeDetectorRef, private userSvc: UserService,
              private docGen: DocumentGeneratorService) {}
  ngOnInit() { this.load(); }
  ngOnDestroy() { this.d$.next(); this.d$.complete(); }
  load() {
    this.loading = true; this.error = '';
    this.svc.getAll(this.page, this.pageSize).pipe(takeUntil(this.d$)).subscribe({
      next: d => {
        this.factures = d;
        this.total = d.length;
        this.loadClientNames(d);
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => { this.error = 'Erreur chargement.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }
  onPage(p: number) { this.page = p; this.load(); }
  delete(id: number) {
    if (!confirm('Supprimer cette facture ?')) return;
    this.svc.delete(id).pipe(takeUntil(this.d$)).subscribe({
      next: () => this.load(),
      error: () => { this.error = 'Erreur lors de la suppression.'; this.cdr.markForCheck(); }
    });
  }

  clientName(id?: number): string {
    if (!id) return '-';
    return this.clientNames[id] ?? `Client #${id}`;
  }

  download(doc: FactureDto): void {
    const url = this.downloadUrl(doc);
    if (url) { this.openUrl(url); return; }

    // No Cloudinary file yet (document created before generation existed) — generate it now.
    if (!doc.id_Commande) {
      this.error = 'Impossible de générer : commande associée introuvable.';
      this.cdr.markForCheck();
      return;
    }
    this.error = '';
    this.regenerating[doc.numero_Doc] = true;
    this.cdr.markForCheck();
    this.docGen.generateFacture(doc.id_Commande, doc.id_Client, doc.numero_Doc)
      .pipe(takeUntil(this.d$)).subscribe({
        next: res => {
          this.regenerating[doc.numero_Doc] = false;
          this.saveBlob(res.blob, res.fileName);   // robust: never popup-blocked
          this.load();                              // refresh so the row gets its stored URL
        },
        error: () => {
          this.regenerating[doc.numero_Doc] = false;
          this.error = 'Erreur lors de la génération du document.';
          this.cdr.markForCheck();
        },
      });
  }

  /** Triggers a browser download from a Blob via a transient anchor (not popup-blocked). */
  private saveBlob(blob: Blob, fileName: string): void {
    if (typeof document === 'undefined') return;
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    setTimeout(() => URL.revokeObjectURL(url), 1000);
  }

  /** Opens an existing remote document URL in a new tab. */
  private openUrl(url: string): void {
    if (typeof window === 'undefined') return;
    const a = document.createElement('a');
    a.href = url;
    a.target = '_blank';
    a.rel = 'noopener';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
  }

  private loadClientNames(docs: FactureDto[]): void {
    [...new Set(docs.map(d => d.id_Client).filter((id): id is number => !!id))]
      .filter(id => !this.clientNames[id])
      .forEach(id => {
        this.userSvc.getUserById(id).pipe(takeUntil(this.d$)).subscribe({
          next: (res: any) => {
            const u = res?.Result ?? res?.result ?? res;
            this.clientNames[id] = u?.fullName ?? u?.FullName ?? u?.name ?? u?.Name ?? u?.email ?? u?.Email ?? `Client #${id}`;
            this.cdr.markForCheck();
          },
          error: () => { this.clientNames[id] = `Client #${id}`; this.cdr.markForCheck(); }
        });
      });
  }

  private downloadUrl(doc: any): string | null {
    const url = doc?.cloudinaryUrl ?? doc?.CloudinaryUrl ?? doc?.url ?? doc?.Url ?? doc?.documentUrl ?? doc?.DocumentUrl ?? doc?.fileUrl ?? doc?.FileUrl ?? doc?.pdfUrl ?? doc?.PdfUrl;
    return typeof url === 'string' && url.trim()
      ? url.replace('/raw/upload/', '/raw/upload/fl_attachment/')
      : null;
  }
}
