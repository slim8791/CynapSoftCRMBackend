import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { BonLivraisonService, BonLivraisonDto } from '../services/bon-livraison.service';
import { PaginatorComponent } from '../../../../shared/components/paginator/paginator.component';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { PdfService } from '../../../../shared/services/pdf.service';

@Component({
  selector: 'app-bon-livraison-list',
  standalone: true,
  imports: [CommonModule, RouterLink, PaginatorComponent, EmptyStateComponent],
  templateUrl: './bon-livraison-list.component.html',
  styleUrls: ['./bon-livraison-list.component.css']
})
export class BonLivraisonListComponent implements OnInit, OnDestroy {
  bons: BonLivraisonDto[] = [];
  loading = false; error = ''; page = 1; pageSize = 20; total = 0;
  private d$ = new Subject<void>();
  constructor(private svc: BonLivraisonService, private cdr: ChangeDetectorRef, private pdf: PdfService) {}
  ngOnInit() { this.load(); }
  ngOnDestroy() { this.d$.next(); this.d$.complete(); }
  load() {
    this.loading = true;
    this.svc.getAll(this.page, this.pageSize).pipe(takeUntil(this.d$)).subscribe({
      next: d => { this.bons = d; this.total = d.length; this.loading = false; this.cdr.markForCheck(); },
      error: () => { this.error = 'Erreur chargement.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }
  onPage(p: number) { this.page = p; this.load(); }
  downloadPdf(bon: BonLivraisonDto) { this.pdf.downloadBonLivraison(bon); }
}
