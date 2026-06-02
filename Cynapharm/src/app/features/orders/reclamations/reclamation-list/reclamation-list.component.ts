import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { Subject, forkJoin, of } from 'rxjs';
import { takeUntil, switchMap, catchError, map } from 'rxjs/operators';

import { ReclamationService, ReclamationDto, StatutReclamation } from '../../services/reclamation.service';
import { AuthService, UserRole } from '../../../../core/services/auth.service';
import { ToastService } from '../../../../shared/services/toast.service';
import { UserService } from '../../../users/user.service';
import { ProductService } from '../../../products/product.service';
import { OrderService } from '../../order.service';

@Component({
  selector: 'app-reclamation-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './reclamation-list.component.html',
  styleUrls: ['./reclamation-list.component.css'],
})
export class ReclamationListComponent implements OnInit, OnDestroy {

  reclamations: any[] = [];
  filterStatut: string = 'all';
  loading     = false;
  error       = '';
  orderId:    number | null = null;
  clientId:   number | null = null;

  isAdmin      = false;
  isSuperviseur = false;

  clientNames: Record<number, string> = {};
  productNames: Record<number, string> = {};
  ordersCache: Record<number, any> = {};

  // Status update inline
  updatingId:   number | null = null;
  readonly statuts = [
    { value: StatutReclamation.Ouverte, label: 'Ouverte' },
    { value: StatutReclamation.EnCours, label: 'En cours' },
    { value: StatutReclamation.Resolue, label: 'Résolue' },
  ];

  private destroy$ = new Subject<void>();

  constructor(
    readonly svc:   ReclamationService,
    private auth:   AuthService,
    private toast:  ToastService,
    private route:  ActivatedRoute,
    private router: Router,
    private cdr:    ChangeDetectorRef,
    private userSvc: UserService,
    private productSvc: ProductService,
    private orderSvc: OrderService
  ) {}

  ngOnInit(): void {
    const role      = this.auth.getUserRole();
    this.isAdmin      = role === UserRole.ADMIN;
    this.isSuperviseur = role === UserRole.SUPERVISEUR;

    const oid = this.route.snapshot.queryParamMap.get('orderId');
    const cid = this.route.snapshot.queryParamMap.get('clientId');
    this.orderId  = oid ? Number(oid)  : null;
    this.clientId = cid ? Number(cid)  : null;
    
    this.loadClients();
    this.loadProducts();
    this.load();
  }

  loadClients(): void {
    this.userSvc.getUsers().pipe(takeUntil(this.destroy$)).subscribe((res: any) => {
      const raw = Array.isArray(res) ? res : (res?.Result ?? res?.result ?? res?.data ?? []);
      raw.forEach((u: any) => {
        const id = u.id ?? u.Id;
        if (id) {
          this.clientNames[id] = u.fullName ?? u.FullName ?? u.name ?? u.Name ?? `Client #${id}`;
        }
      });
      this.cdr.markForCheck();
    });
  }

  loadProducts(): void {
    this.productSvc.getProductsAll().pipe(takeUntil(this.destroy$)).subscribe((res: any) => {
      res.forEach((p: any) => {
        const id = p.Id_Produit ?? p.id_Produit;
        if (id) this.productNames[id] = p.Nom ?? p.nom ?? `Produit #${id}`;
      });
      this.cdr.markForCheck();
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  load(): void {
    this.loading = true;
    const src$ = this.orderId  ? this.svc.getByOrder(this.orderId)
               : this.clientId ? this.svc.getByClient(this.clientId)
               : this.svc.getAll();

    src$.pipe(takeUntil(this.destroy$)).subscribe({
      next: (response: any) => {
        const raw: any[] = response?.result ?? response?.Result ?? response?.data ?? response?.Data ?? (Array.isArray(response) ? response : []);
        this.reclamations = raw.map((r: any) => this.svc.normalizeRec(r));
        
        // On force la mise à jour asynchrone pour éviter les conflits avec le Router Angular
        setTimeout(() => {
          this.loading = false;
          this.cdr.detectChanges();
        });
        
        const orderIds = [...new Set(this.reclamations.map(r => r.Id_Commande))].filter(id => id && !this.ordersCache[id]);
        
        if (orderIds.length > 0) {
          orderIds.forEach(id => {
            this.orderSvc.getOrderById(id).pipe(takeUntil(this.destroy$)).subscribe({
              next: (order) => {
                this.ordersCache[id] = order || { Lignes: [] };
                this.cdr.detectChanges();
              },
              error: () => {
                this.ordersCache[id] = { Lignes: [] };
                this.cdr.detectChanges();
              }
            });
          });
        }
      },
      error: (err: any) => {
        const s = err?.status;
        if (s === 403) this.error = 'Accès refusé — rôle ADMIN ou SUPERVISEUR requis.';
        else if (s === 401) this.error = 'Session expirée, veuillez vous reconnecter.';
        else if (s === 0)   this.error = 'Serveur inaccessible.';
        else this.error = err?.error?.Message ?? 'Erreur lors du chargement.';
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }

  onUpdateStatus(id: number, status: StatutReclamation): void {
    this.updatingId = id;
    this.svc.updateStatus(id, status).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => { this.toast.showSuccess('Statut mis à jour.'); this.updatingId = null; this.load(); },
      error: () => { this.toast.showError('Erreur mise à jour.'); this.updatingId = null; this.cdr.markForCheck(); },
    });
  }

  onDelete(id: number): void {
    if (!confirm('Supprimer cette réclamation ?')) return;
    this.svc.delete(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => { this.toast.showSuccess('Réclamation supprimée.'); this.load(); },
      error: () => this.toast.showError('Erreur lors de la suppression.'),
    });
  }

  onView(id: number):   void { this.router.navigate(['/orders/reclamations', id]); }

  formatOrderNum(id: number): string { return 'CMD-' + String(id).padStart(5, '0'); }

  getClientName(id: number): string { return this.clientNames[id] ?? `Client #${id}`; }

  getProductNameForLigne(idCmd: number, idLigne: number): string {
    const order = this.ordersCache[idCmd];
    if (!order) return 'Chargement...';
    const ligne = order.Lignes?.find((l: any) => l.Id_Ligne === idLigne);
    if (!ligne) return 'Ligne inconnue';
    return this.productNames[ligne.Id_Produit] ?? `Produit #${ligne.Id_Produit}`;
  }

  getStatutLabel(s?: string | number): string {
    const n = typeof s === 'number' ? s : Number(s);
    switch (n) { case 0: return 'Ouverte'; case 1: return 'En cours'; case 2: return 'Résolue'; default: return 'Inconnu'; }
  }
  getStatutClass(s?: string | number): string {
    const n = typeof s === 'number' ? s : Number(s);
    switch (n) { case 0: return 'chip-warning'; case 1: return 'chip-info'; case 2: return 'chip-success'; default: return 'chip-default'; }
  }
  get filteredReclamations(): any[] {
    if (this.filterStatut === 'all') return this.reclamations;
    const n = Number(this.filterStatut);
    return this.reclamations.filter(rec => Number(rec.Statut) === n);
  }

  canManageStatus(): boolean { return this.isAdmin || this.isSuperviseur; }
}
