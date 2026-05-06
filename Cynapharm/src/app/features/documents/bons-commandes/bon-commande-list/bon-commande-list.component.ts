import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { BonCommandeService, BonCommandeDto } from '../services/bon-commande.service';
import { CurrencyTNDPipe } from '../../../../shared/pipes/currency-tnd.pipe';
import { PaginatorComponent } from '../../../../shared/components/paginator/paginator.component';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-bon-commande-list',
  standalone: true,
  imports: [CommonModule, RouterLink, CurrencyTNDPipe, PaginatorComponent, EmptyStateComponent],
  templateUrl: './bon-commande-list.component.html',
  styleUrls: ['./bon-commande-list.component.css']
})
export class BonCommandeListComponent implements OnInit, OnDestroy {
  bons: BonCommandeDto[] = [];
  loading = false; error = ''; page = 1; pageSize = 20; total = 0;
  private d$ = new Subject<void>();
  constructor(private svc: BonCommandeService, private cdr: ChangeDetectorRef) {}
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
}
