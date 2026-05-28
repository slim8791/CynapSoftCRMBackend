import { Component, OnDestroy, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Observable, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { FactureService } from '../factures/services/facture.service';
import { BonCommandeService } from '../bons-commandes/services/bon-commande.service';
import { BonLivraisonService } from '../bons-livraison/services/bon-livraison.service';
import { UserService } from '../../users/user.service';

type DocumentKind = 'facture' | 'bon-commande' | 'bon-livraison';

@Component({
  selector: 'app-document-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './document-detail.component.html',
  styleUrls: ['./document-detail.component.css']
})
export class DocumentDetailComponent implements OnInit, OnDestroy {
  doc: any = null;
  loading = false;
  error = '';
  clientName = '';
  kind: DocumentKind = 'facture';

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private factureSvc: FactureService,
    private bcSvc: BonCommandeService,
    private blSvc: BonLivraisonService,
    private userSvc: UserService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.kind = (this.route.snapshot.data['documentKind'] as DocumentKind) ?? 'facture';
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) {
      this.error = 'Identifiant de document invalide.';
      return;
    }

    this.loading = true;
    this.requestByKind(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: doc => {
        this.doc = doc;
        this.loading = false;
        this.loadClientName(this.clientId);
        this.cdr.markForCheck();
      },
      error: () => {
        this.error = 'Impossible de charger le document.';
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get title(): string {
    if (this.kind === 'facture') return 'Détail de la facture';
    if (this.kind === 'bon-commande') return 'Détail du bon de commande';
    return 'Détail du bon de livraison';
  }

  get backLink(): string {
    if (this.kind === 'facture') return '/documents/factures';
    if (this.kind === 'bon-commande') return '/documents/bons-commandes';
    return '/documents/bons-livraison';
  }

  get clientId(): number | undefined {
    return this.doc?.id_Client ?? this.doc?.Id_Client ?? this.doc?.idClient;
  }

  get documentDate(): string | undefined {
    return this.doc?.dateFacture ?? this.doc?.DateFacture ?? this.doc?.dateCreation ?? this.doc?.DateCreation;
  }

  print(): void {
    if (typeof window !== 'undefined') window.print();
  }

  private requestByKind(id: number): Observable<any> {
    if (this.kind === 'facture') return this.factureSvc.getById(id);
    if (this.kind === 'bon-commande') return this.bcSvc.getById(id);
    return this.blSvc.getById(id);
  }

  private loadClientName(id?: number): void {
    if (!id) {
      this.clientName = '-';
      return;
    }

    this.userSvc.getUserById(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: (res: any) => {
        const u = res?.Result ?? res?.result ?? res;
        this.clientName = u?.fullName ?? u?.FullName ?? u?.name ?? u?.Name ?? u?.email ?? u?.Email ?? `Client #${id}`;
        this.cdr.markForCheck();
      },
      error: () => {
        this.clientName = `Client #${id}`;
        this.cdr.markForCheck();
      }
    });
  }

}
