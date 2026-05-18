import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DocumentService, DocumentDto } from '../services/document.service';
import { PaginatorComponent } from '../../../../shared/components/paginator/paginator.component';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-document-list',
  standalone: true,
  imports: [CommonModule, RouterLink, PaginatorComponent, EmptyStateComponent],
  templateUrl: './document-list.component.html',
  styleUrls: ['./document-list.component.css']
})
export class DocumentListComponent implements OnInit, OnDestroy {
  docs: DocumentDto[] = [];
  loading = false; error = ''; page = 1; pageSize = 20; total = 0;
  typeFilter: '' | 'FACTURE' | 'BC' | 'BL' = '';
  private d$ = new Subject<void>();
  constructor(private svc: DocumentService, private cdr: ChangeDetectorRef) {}
  ngOnInit() { this.load(); }
  ngOnDestroy() { this.d$.next(); this.d$.complete(); }

  setTypeFilter(t: '' | 'FACTURE' | 'BC' | 'BL'): void {
    this.typeFilter = t;
    this.page = 1;
    this.load();
  }

  load() {
    this.loading = true;
    const req$ = this.typeFilter
      ? this.svc.getByType(this.typeFilter, this.page, this.pageSize)
      : this.svc.getAll(this.page, this.pageSize);
    req$.pipe(takeUntil(this.d$)).subscribe({
      next: data => { this.docs = data; this.total = data.length; this.loading = false; this.cdr.markForCheck(); },
      error: () => { this.error = 'Erreur chargement.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }
  onPage(p: number) { this.page = p; this.load(); }
}
