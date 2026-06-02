import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DocumentService, DocumentDto } from '../services/document.service';
import { PaginatorComponent } from '../../../../shared/components/paginator/paginator.component';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { UserService } from '../../../users/user.service';

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
  clientNames: Record<number, string> = {};
  private d$ = new Subject<void>();
  constructor(private svc: DocumentService, private cdr: ChangeDetectorRef, private userSvc: UserService) {}
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
      next: data => {
        this.docs = data;
        this.total = data.length;
        this.loadClientNames(data);
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => { this.error = 'Erreur chargement.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }
  onPage(p: number) { this.page = p; this.load(); }

  clientName(id?: number): string {
    if (!id) return '-';
    return this.clientNames[id] ?? `Client #${id}`;
  }

  private loadClientNames(docs: DocumentDto[]): void {
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
}
