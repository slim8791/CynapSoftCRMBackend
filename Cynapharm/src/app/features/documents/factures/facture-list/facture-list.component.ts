import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { FactureService, FactureDto } from '../services/facture.service';
import { CurrencyTNDPipe } from '../../../../shared/pipes/currency-tnd.pipe';
import { PaginatorComponent } from '../../../../shared/components/paginator/paginator.component';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { PdfService } from '../../../../shared/services/pdf.service';

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
  private d$ = new Subject<void>();
  constructor(private svc: FactureService, private cdr: ChangeDetectorRef, private pdf: PdfService) {}
  ngOnInit() { this.load(); }
  ngOnDestroy() { this.d$.next(); this.d$.complete(); }
  load() {
    this.loading = true; this.error = '';
    this.svc.getAll(this.page, this.pageSize).pipe(takeUntil(this.d$)).subscribe({
      next: d => { this.factures = d; this.total = d.length; this.loading = false; this.cdr.markForCheck(); },
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
}
