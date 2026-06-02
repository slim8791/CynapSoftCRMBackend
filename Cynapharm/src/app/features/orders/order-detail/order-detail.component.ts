import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { FormsModule } from '@angular/forms';
import { OrderService, CommandeDto, EtatCommande } from '../order.service';
import { ReclamationService, ReclamationDto } from '../services/reclamation.service';
import { FactureService, FactureDto } from '../../documents/factures/services/facture.service';
import { BonCommandeService, BonCommandeDto } from '../../documents/bons-commandes/services/bon-commande.service';
import { BonLivraisonService, BonLivraisonDto } from '../../documents/bons-livraison/services/bon-livraison.service';
import { AuthService, UserRole } from '../../../core/services/auth.service';
import { UserService } from '../../users/user.service';
import { ProductService } from '../../products/product.service';
import { ToastService } from '../../../shared/services/toast.service';
import { DocumentGeneratorService } from '../../documents/services/document-generator.service';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './order-detail.component.html',
  styleUrls: ['./order-detail.component.css'],
})
export class OrderDetailComponent implements OnInit, OnDestroy {

  order:         CommandeDto | null = null;
  reclamations:  any[]              = [];
  factures:      FactureDto[]       = [];
  bonsCommandes: BonCommandeDto[]   = [];
  bonsLivraison: BonLivraisonDto[]  = [];

  loading      = true;
  loadingRec   = false;
  loadingDocs  = false;
  error        = '';

  activeTab: 'info' | 'lignes' | 'reclamations' | 'documents' = 'info';

  isAdmin = false;

  // Fix 2 — resolved client name
  clientName = '';

  // Fix — product names cache
  productNames: Record<number, string> = {};

  // Modals
  showStatusModal = false;
  showDeleteModal = false;
  showCancelModal  = false;
  cancelMotif      = '';
  cancelMotifError = '';
  deleting        = false;
  statusOptions: { label: string; value: EtatCommande }[] = [];

  // Fix 4 — document creation loaders
  creatingBC      = false;
  creatingBL      = false;
  creatingFacture = false;

  // Lot assignment modal
  showLotModal    = false;
  lotInput        = '';
  lotInputError   = '';
  assigningLot    = false;
  availableLots:  any[] = [];
  loadingLots     = false;
  private lotLigne: import('../order.service').LigneCommandeDto | null = null;

  private orderId = 0;
  private destroy$ = new Subject<void>();

  constructor(
    private route:        ActivatedRoute,
    private router:       Router,
    private svc:          OrderService,
    private recSvc:       ReclamationService,
    private factureSvc:   FactureService,
    private bcSvc:        BonCommandeService,
    private blSvc:        BonLivraisonService,
    private auth:         AuthService,
    private userSvc:      UserService,
    private productSvc:   ProductService,
    private toast:        ToastService,
    private docGen:       DocumentGeneratorService,
    private cdr:          ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.isAdmin = this.auth.getUserRole() === UserRole.ADMIN;
    this.orderId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadOrder();
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  // ── Order ─────────────────────────────────────────────

  private loadOrder(): void {
    this.loading = true;
    this.svc.getOrderById(this.orderId).pipe(takeUntil(this.destroy$)).subscribe({
      next: order => {
        this.order   = order;
        this.loading = false;
        if (order) {
          this.statusOptions = this.svc.getNextStatuses(order.Statut);
          this.loadClientName(order.Id_Client);
          this.loadProductNames(order.Lignes ?? []);
          this.loadReclamations();
          this.loadDocuments();
        }
        this.cdr.markForCheck();
      },
      error: () => {
        this.error   = 'Commande introuvable.';
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }

  // Fix — load product names for all unique product IDs in the lignes
  private loadProductNames(lignes: { Id_Produit: number }[]): void {
    const ids = [...new Set(lignes.map(l => l.Id_Produit).filter(id => id > 0))];
    ids.forEach(id => {
      this.productSvc.getProductById(id).pipe(takeUntil(this.destroy$)).subscribe({
        next: (data: any) => {
          const raw = data?.Result ?? data?.result ?? data;
          this.productNames[id] = raw?.Nom ?? raw?.nom ?? `Produit #${id}`;
          this.cdr.markForCheck();
        },
        error: () => { this.productNames[id] = `Produit #${id}`; },
      });
    });
  }

  getProductName(id: number): string {
    return this.productNames[id] ?? `Produit #${id}`;
  }


  // Fix 2 — load single user name by id
  private loadClientName(id: number): void {
    if (!id || id === 0) { this.clientName = 'Client inconnu'; return; }
    this.userSvc.getUserById(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: (res: any) => {
        const u = res?.Result ?? res?.result ?? res;
        this.clientName =
          u?.fullName ?? u?.FullName ??
          u?.name     ?? u?.Name     ??
          u?.userName ?? u?.UserName ??
          u?.email    ?? u?.Email    ?? `Client #${id}`;
        this.cdr.markForCheck();
      },
      error: () => { this.clientName = `Client #${id}`; this.cdr.markForCheck(); },
    });
  }

  // ── Statut helpers ─────────────────────────────────────

  /** Current statut as a number for comparisons */
  get statutNumber(): number {
    return this.svc.statutToNumber(this.order?.Statut ?? '');
  }

  /** Only Brouillon(0) or Annulee(6) can be deleted */
  canDelete(): boolean {
    const n = this.statutNumber;
    return n === EtatCommande.Brouillon || n === EtatCommande.Annulee;
  }

  /** Brouillon(0) → EnPreparation(3) can be cancelled */
  canCancel(): boolean {
    const n = this.statutNumber;
    return n >= 0 && n <= 3;
  }

  openCancelModal(): void {
    this.cancelMotif     = '';
    this.showCancelModal = true;
  }

  /**
   * Fix 3 — single labelled forward-transition button.
   * Annulee is excluded here (handled by the cancel modal).
   */
  get directTransition(): { label: string; value: EtatCommande } | null {
    const map: Partial<Record<number, { label: string; value: EtatCommande }>> = {
      [EtatCommande.EnAttente]:     { label: 'Confirmer',             value: EtatCommande.Confirmee     },
      [EtatCommande.Confirmee]:     { label: 'Mettre en préparation', value: EtatCommande.EnPreparation },
      [EtatCommande.EnPreparation]: { label: 'Expédier',              value: EtatCommande.Expediee      },
      [EtatCommande.Expediee]:      { label: 'Marquer comme livrée',  value: EtatCommande.Livree        },
    };
    return map[this.statutNumber] ?? null;
  }

  canChangeStatus(): boolean { return this.statusOptions.length > 0; }

  // Status workflow
  openStatusModal(): void {
    if (!this.order) return;
    this.statusOptions   = this.svc.getNextStatuses(this.order.Statut);
    this.showStatusModal = true;
  }

  applyStatus(newStatus: EtatCommande): void {
    if (!this.order) return;
    if (newStatus === EtatCommande.Annulee) {
      this.showStatusModal = false;
      this.cancelMotif     = '';
      this.showCancelModal = true;
      return;
    }

    if (newStatus === EtatCommande.Expediee) {
      const lignesSansLot = (this.order.Lignes ?? []).filter((l: any) => {
        const lot = l.NumeroLot ?? l.numeroLot ?? l.numero_lot;
        return !lot || String(lot).trim() === '';
      });
      if (lignesSansLot.length > 0) {
        this.toast.showError(
          `Impossible d'expédier : ${lignesSansLot.length} ligne(s) sans numéro de lot assigné.`
        );
        this.showStatusModal = false;
        return;
      }
    }

    this.showStatusModal = false;
    this.svc.updateOrderStatus({ Id_Commande: this.order.Id_Commande, NouveauStatut: newStatus })
      .pipe(takeUntil(this.destroy$)).subscribe({
        next: () => {
          this.toast.showSuccess('Statut mis à jour.');
          // Automatic document creation on status transitions
          if (newStatus === EtatCommande.Confirmee)     this.autoCreateBC();
          else if (newStatus === EtatCommande.Expediee) this.autoCreateBL();
          else if (newStatus === EtatCommande.Livree)   this.autoCreateFacture();
          this.loadOrder();
        },
        error: () => this.toast.showError('Erreur lors de la mise à jour du statut.'),
      });
  }

  private autoCreateBC(): void {
    if (!this.order) return;
    if (this.bonsCommandes.length > 0) return;
    this.docGen.generateBonCommande(this.orderId, this.order.Id_Client)
      .pipe(takeUntil(this.destroy$)).subscribe({
        next: () => { this.toast.showSuccess('Bon de commande créé automatiquement.'); this.loadDocuments(); },
        error: () => this.toast.showError('Erreur lors de la création automatique du BC.'),
      });
  }

  private autoCreateBL(): void {
    if (!this.order) return;
    if (this.bonsLivraison.length > 0) return;
    this.docGen.generateBonLivraison(this.orderId, this.order.Id_Client)
      .pipe(takeUntil(this.destroy$)).subscribe({
        next: () => { this.toast.showSuccess('Bon de livraison créé automatiquement.'); this.loadDocuments(); },
        error: () => this.toast.showError('Erreur lors de la création automatique du BL.'),
      });
  }

  private autoCreateFacture(): void {
    if (!this.order) return;
    if (this.factures.length > 0) return;
    this.docGen.generateFacture(this.orderId, this.order.Id_Client)
      .pipe(takeUntil(this.destroy$)).subscribe({
        next: () => { this.toast.showSuccess('Facture créée automatiquement.'); this.loadDocuments(); },
        error: () => this.toast.showError('Erreur lors de la création automatique de la facture.'),
      });
  }

  closeCancelModal(): void { this.showCancelModal = false; this.cancelMotif = ''; this.cancelMotifError = ''; }

  confirmCancel(): void {
    if (!this.order) return;
    const motif = this.cancelMotif.trim();
    if (motif.length < 10) {
      this.cancelMotifError = 'Le motif doit contenir au moins 10 caractères.';
      this.cdr.markForCheck();
      return;
    }
    this.cancelMotifError = '';
    this.svc.cancelOrder(this.order.Id_Commande, motif)
      .pipe(takeUntil(this.destroy$)).subscribe({
        next: () => { this.showCancelModal = false; this.toast.showSuccess('Commande annulée.'); this.loadOrder(); },
        error: () => this.toast.showError('Erreur lors de l\'annulation.'),
      });
  }

  // Delete order
  openDeleteModal(): void   { this.showDeleteModal = true; }
  cancelDeleteOrder(): void { this.showDeleteModal = false; }

  confirmDeleteOrder(): void {
    if (!this.order) return;
    this.deleting = true;
    this.svc.deleteOrder(this.order.Id_Commande).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => { this.toast.showSuccess('Commande supprimée.'); this.router.navigate(['/orders']); },
      error: () => { this.toast.showError('Erreur lors de la suppression.'); this.deleting = false; this.cdr.markForCheck(); },
    });
  }

  // ── Réclamations ───────────────────────────────────────

  private loadReclamations(): void {
    this.loadingRec = true;
    this.recSvc.getByOrder(this.orderId).pipe(takeUntil(this.destroy$)).subscribe({
      next: (response: any) => {
        this.reclamations = response?.result ?? response?.Result ?? (Array.isArray(response) ? response : []);
        this.loadingRec = false;
        this.cdr.markForCheck();
      },
      error: () => { this.reclamations = []; this.loadingRec = false; this.cdr.markForCheck(); },
    });
  }

  onDeleteRec(id: number): void {
    if (!confirm('Supprimer cette réclamation ?')) return;
    this.recSvc.delete(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => { this.toast.showSuccess('Réclamation supprimée.'); this.loadReclamations(); },
      error: () => this.toast.showError('Erreur lors de la suppression.'),
    });
  }

  canDeleteReclamation(rec: any): boolean {
    const statut = (rec as any).Statut ?? (rec as any).statut;
    return statut === 0 || statut === '0' || statut === 'Ouverte';
  }

  // ── Documents ──────────────────────────────────────────

  private loadDocuments(): void {
    this.loadingDocs = true;
    this.factureSvc.getByCommande(this.orderId).pipe(takeUntil(this.destroy$))
      .subscribe({ next: d => { this.factures = d; this.cdr.markForCheck(); }, error: () => {} });
    this.bcSvc.getByCommande(this.orderId).pipe(takeUntil(this.destroy$))
      .subscribe({ next: d => { this.bonsCommandes = d; this.cdr.markForCheck(); }, error: () => {} });
    this.blSvc.getByCommande(this.orderId).pipe(takeUntil(this.destroy$))
      .subscribe({
        next: d => { this.bonsLivraison = d; this.loadingDocs = false; this.cdr.markForCheck(); },
        error: ()  => { this.loadingDocs = false; this.cdr.markForCheck(); },
      });
  }

  // Fix 4 — create document buttons (generate PDF + upload + persist)
  createBC(): void {
    if (!this.order) return;
    this.creatingBC = true;
    this.docGen.generateBonCommande(this.orderId, this.order.Id_Client)
      .pipe(takeUntil(this.destroy$)).subscribe({
        next: () => { this.toast.showSuccess('Bon de commande créé.'); this.creatingBC = false; this.loadDocuments(); },
        error: () => { this.toast.showError('Erreur lors de la création du BC.'); this.creatingBC = false; this.cdr.markForCheck(); },
      });
  }

  createBL(): void {
    if (!this.order) return;
    this.creatingBL = true;
    this.docGen.generateBonLivraison(this.orderId, this.order.Id_Client)
      .pipe(takeUntil(this.destroy$)).subscribe({
        next: () => { this.toast.showSuccess('Bon de livraison créé.'); this.creatingBL = false; this.loadDocuments(); },
        error: () => { this.toast.showError('Erreur lors de la création du BL.'); this.creatingBL = false; this.cdr.markForCheck(); },
      });
  }

  createFacture(): void {
    if (!this.order) return;
    this.creatingFacture = true;
    this.docGen.generateFacture(this.orderId, this.order.Id_Client)
      .pipe(takeUntil(this.destroy$)).subscribe({
        next: () => { this.toast.showSuccess('Facture créée.'); this.creatingFacture = false; this.loadDocuments(); },
        error: () => { this.toast.showError('Erreur lors de la création de la facture.'); this.creatingFacture = false; this.cdr.markForCheck(); },
      });
  }

  onDeleteFacture(id: number): void {
    if (!confirm('Supprimer cette facture ?')) return;
    this.factureSvc.delete(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => { this.toast.showSuccess('Facture supprimée.'); this.loadDocuments(); },
      error: () => this.toast.showError('Erreur lors de la suppression.'),
    });
  }

  onDeleteBC(id: number): void {
    if (!confirm('Supprimer ce bon de commande ?')) return;
    this.bcSvc.delete(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => { this.toast.showSuccess('Bon de commande supprimé.'); this.loadDocuments(); },
      error: () => this.toast.showError('Erreur lors de la suppression.'),
    });
  }

  onDeleteBL(id: number): void {
    if (!confirm('Supprimer ce bon de livraison ?')) return;
    this.blSvc.delete(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => { this.toast.showSuccess('Bon de livraison supprimé.'); this.loadDocuments(); },
      error: () => this.toast.showError('Erreur lors de la suppression.'),
    });
  }

  // ── Lot assignment ────────────────────────────────────

  canAssignLot(ligne: import('../order.service').LigneCommandeDto): boolean {
    return this.isAdmin && !ligne.NumeroLot && this.statutNumber === 3;
  }

  async openLotModal(ligne: import('../order.service').LigneCommandeDto): Promise<void> {
    this.lotLigne      = ligne;
    this.lotInput      = '';
    this.lotInputError = '';
    this.availableLots = [];
    this.showLotModal  = true;
    this.loadingLots   = true;
    this.cdr.markForCheck();
    try {
      const lots = await this.productSvc.getLotsByProduct(ligne.Id_Produit).toPromise();
      this.availableLots = (lots ?? []).filter((l: any) =>
        !l.isExpired && !l.isOutOfStock && (l.quantite ?? 0) > 0
      );
    } catch {
      this.availableLots = [];
    } finally {
      this.loadingLots = false;
      this.cdr.markForCheck();
    }
  }

  closeLotModal(): void {
    this.showLotModal  = false;
    this.lotLigne      = null;
    this.lotInput      = '';
    this.lotInputError = '';
    this.availableLots = [];
  }

  confirmAssignLot(): void {
    const lot = this.lotInput?.trim();
    if (!lot) { this.lotInputError = 'Veuillez sélectionner un lot.'; return; }
    if (!this.lotLigne) return;

    this.assigningLot = true;
    this.svc.assignLot(this.lotLigne, lot).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.assigningLot = false;
        this.showLotModal = false;
        this.toast.showSuccess('Numéro de lot assigné avec succès.');
        this.loadOrder();
      },
      error: (err: any) => {
        this.assigningLot  = false;
        this.lotInputError = err?.error?.Message ?? 'Erreur lors de l\'assignation du lot.';
        this.cdr.markForCheck();
      },
    });
  }

  // ── Helpers ───────────────────────────────────────────

  viewDocument(type: string, id: number): void {
    const routeMap: Record<string, string> = {
      'BC':      '/documents/bons-commandes',
      'BL':      '/documents/bons-livraison',
      'FACTURE': '/documents/factures',
    };
    const base = routeMap[type];
    if (base) this.router.navigate([base, id]);
  }

  get totalDocs(): number { return this.factures.length + this.bonsCommandes.length + this.bonsLivraison.length; }
  setTab(t: 'info' | 'lignes' | 'reclamations' | 'documents'): void { this.activeTab = t; }
  getTotalLignes(): number { return this.order?.Lignes?.length ?? 0; }

  getEtatLabel  = (s: string | number) => this.svc.getEtatLabel(s);
  getEtatClass  = (s: string | number) => this.svc.getEtatClass(s);
  getRecLabel   = (s?: string | number) => this.recSvc.getStatutLabel(s);
  getRecClass   = (s?: string | number) => this.recSvc.getStatutClass(s);
}
