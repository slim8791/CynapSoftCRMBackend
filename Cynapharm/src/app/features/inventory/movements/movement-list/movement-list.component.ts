import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, forkJoin, of } from 'rxjs';
import { takeUntil, catchError, map, switchMap } from 'rxjs/operators';
import { StockMovementService, StockMovementDto } from '../../movements/services/stock-movement.service';
import { AuthService, UserRole } from '../../../../core/services/auth.service';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { UserService } from '../../../users/user.service';
import { ProductService } from '../../../products/product.service';
import { StockService, StockDelegueDto } from '../../stocks/services/stock.service';

/** Augmented DTO with resolved delegue/product IDs and display labels */
interface MovementRow extends StockMovementDto {
  resolvedDelegueId?: number;
  resolvedProduitId?: number;
  delegueLabel?: string;
  produitLabel?: string;
}

@Component({
  selector: 'app-movement-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, EmptyStateComponent],
  templateUrl: './movement-list.component.html',
  styleUrls: ['./movement-list.component.css']
})
export class MovementListComponent implements OnInit, OnDestroy {
  allMovements: MovementRow[] = [];
  movements: MovementRow[] = [];
  loading = false;
  error = '';

  /** Client-side filters */
  dateDebut    = '';
  dateFin      = '';
  typeMovement = '';
  selectedDelegueId: number | null = null;
  selectedProduitId: number | null = null;

  /** Dropdown options */
  delegues: { id: number; label: string }[] = [];
  produits: { id: number; label: string }[] = [];

  /** True if the connected user is ADMIN or SUPERVISEUR */
  isAdmin = false;

  /** Lookup: stockId → { delegueId, produitId } */
  private stockLookup = new Map<number, { delegueId: number; produitId: number }>();

  private destroy$ = new Subject<void>();

  constructor(
    private route:       ActivatedRoute,
    private svc:         StockMovementService,
    private auth:        AuthService,
    private cdr:         ChangeDetectorRef,
    private userService: UserService,
    private productSvc:  ProductService,
    private stockSvc:    StockService
  ) {}

  ngOnInit(): void {
    const role = this.auth.getUserRole();
    this.isAdmin = role === UserRole.ADMIN || role === UserRole.SUPERVISEUR;

    // Load reference data (delegues, produits, stocks) in parallel,
    // then load movements according to role
    this.loading = true;
    this.loadReferenceData();
  }

  /**
   * Step 1 — load delegues, produits, stocks in parallel.
   * Step 2 — once done, load movements based on role.
   */
  private loadReferenceData(): void {
    forkJoin({
      delegues: this.userService.getUsersByRole('DELEGUE').pipe(catchError(() => of([]))),
      produits: this.productSvc.getProducts().pipe(
        map(r => {
          // getProducts() returns unwrapped array OR wraps in Result
          const arr = Array.isArray(r) ? r : [];
          return arr;
        }),
        catchError(() => of([]))
      ),
      stocks: this.stockSvc.getAll(1, 10000).pipe(catchError(() => of([])))
    }).pipe(takeUntil(this.destroy$)).subscribe(({ delegues, produits, stocks }) => {

      // ── Build delegue dropdown ──
      this.delegues = (Array.isArray(delegues) ? delegues : [])
        .map((u: any) => ({
          id:    u?.id    ?? u?.Id    ?? 0,
          label: u?.name  ?? u?.Name  ?? u?.email ?? `#${u?.id ?? u?.Id}`
        }))
        .filter(d => d.id > 0);

      // ── Build produit dropdown ──
      // Backend fields: Id_Produit, Nom (ProduitDto)
      this.produits = (Array.isArray(produits) ? produits : [])
        .map((p: any) => ({
          id:    p?.Id_Produit ?? p?.id_Produit ?? p?.id ?? p?.Id ?? 0,
          label: p?.Nom        ?? p?.nom        ?? p?.name ?? p?.Name ?? `#${p?.Id_Produit ?? p?.id}`
        }))
        .filter(p => p.id > 0);

      // ── Build stock lookup: stockId → { delegueId, produitId } ──
      this.stockLookup.clear();
      (Array.isArray(stocks) ? stocks : []).forEach((s: any) => {
        const stockId = s?.id_stock ?? s?.id_Stock ?? s?.Id_stock ?? s?.Id_Stock;
        const delegueId = s?.id_User_Delegue ?? s?.Id_User_Delegue;
        const produitId = s?.id_Produit ?? s?.Id_Produit;

        if (stockId != null) {
          this.stockLookup.set(stockId, { delegueId, produitId });
        }
      });

      // ── Load movements based on role ──
      this.loadMovements();
    });
  }

  /**
   * Admin/Superviseur → load movements for ALL delegues via forkJoin.
   * Delegue → load only their own movements.
   */
  private loadMovements(): void {
    if (this.isAdmin) {
      // Load movements for every delegue in parallel, then merge
      if (this.delegues.length === 0) {
        this.allMovements = [];
        this.movements    = [];
        this.loading      = false;
        this.cdr.markForCheck();
        return;
      }

      const requests = this.delegues.map(d =>
        this.svc.getMovementsByDelegue(d.id).pipe(catchError(() => of([])))
      );

      forkJoin(requests).pipe(takeUntil(this.destroy$)).subscribe({
        next: results => {
          const merged: StockMovementDto[] = ([] as StockMovementDto[]).concat(...results);
          // Deduplicate by id_Movement
          const seen = new Set<number>();
          this.allMovements = merged
            .filter(m => {
              if (m.id_Movement == null || seen.has(m.id_Movement!)) return false;
              seen.add(m.id_Movement!);
              return true;
            }) as MovementRow[];

          this.resolveMovementLabels();
          this.applyClientFilters();
          this.loading = false;
          this.cdr.markForCheck();
        },
        error: () => {
          this.error   = 'Impossible de charger les mouvements.';
          this.loading = false;
          this.cdr.markForCheck();
        }
      });
    } else {
      // Delegue: load only their own movements
      const userId = this.auth.getCurrentUser()?.id;
      if (!userId) {
        this.loading = false;
        this.cdr.markForCheck();
        return;
      }
      this.svc.getMovementsByDelegue(userId).pipe(takeUntil(this.destroy$)).subscribe({
        next: data => {
          this.allMovements = data as MovementRow[];
          this.resolveMovementLabels();
          this.applyClientFilters();
          this.loading = false;
          this.cdr.markForCheck();
        },
        error: () => { this.loading = false; this.cdr.markForCheck(); }
      });
    }
  }

  /** Attach resolved delegue/product IDs and display labels to each movement row */
  private resolveMovementLabels(): void {
    this.allMovements = this.allMovements.map((m: any) => {
      const stockId = m?.id_Stock ?? m?.id_stock ?? m?.Id_Stock ?? m?.Id_stock;
      const entry      = this.stockLookup.get(stockId);

      const delegueId  = m?.id_User_Delegue ?? m?.Id_User_Delegue ?? entry?.delegueId;
      const produitId  = m?.id_Produit ?? m?.Id_Produit ?? entry?.produitId;

      const delegueLabel = delegueId != null && delegueId > 0
        ? (this.delegues.find(d => d.id === delegueId)?.label ?? `Délégué #${delegueId}`)
        : '—';
      const produitLabel = produitId != null && produitId > 0
        ? (this.produits.find(p => p.id === produitId)?.label ?? `Produit #${produitId}`)
        : '—';

      return { 
        ...m, 
        dateMovement: m?.dateMovement ?? m?.DateMovement,
        typeMovement: m?.typeMovement ?? m?.TypeMovement,
        quantite: m?.quantite ?? m?.Quantite ?? 0,
        resolvedDelegueId: delegueId, 
        resolvedProduitId: produitId, 
        delegueLabel, 
        produitLabel 
      };
    });
  }

  isSortie(type: string | undefined, quantite: number = 0): boolean {
    if (quantite < 0) return true;
    const t = type?.toLowerCase() ?? '';
    return t === 'decrement' || t === 'transfer-out' || t === 'distribution';
  }

  getAbsQuantite(q: number): number {
    return Math.abs(q || 0);
  }

  resetFilters(): void {
    this.selectedDelegueId = null;
    this.selectedProduitId = null;
    this.dateDebut         = '';
    this.dateFin           = '';
    this.typeMovement      = '';
    this.applyClientFilters();
  }

  hasActiveFilters(): boolean {
    return !!(this.selectedDelegueId || this.selectedProduitId ||
              this.dateDebut || this.dateFin || this.typeMovement);
  }

  applyClientFilters(): void {
    let result: MovementRow[] = this.allMovements;

    if (this.dateDebut) {
      const start = new Date(`${this.dateDebut}T00:00:00`);
      result = result.filter(m => {
        const d = (m as any).dateMovement ?? (m as any).DateMovement;
        return d && new Date(d) >= start;
      });
    }

    if (this.dateFin) {
      const end = new Date(`${this.dateFin}T23:59:59`);
      result = result.filter(m => {
        const d = (m as any).dateMovement ?? (m as any).DateMovement;
        return d && new Date(d) <= end;
      });
    }

    if (this.typeMovement) {
      result = result.filter(m => {
        const t = (m as any).typeMovement ?? (m as any).TypeMovement ?? '';
        return t.toLowerCase() === this.typeMovement.toLowerCase();
      });
    }

    if (this.selectedDelegueId != null) {
      result = result.filter(m => m.resolvedDelegueId === this.selectedDelegueId);
    }

    if (this.selectedProduitId != null) {
      result = result.filter(m => m.resolvedProduitId === this.selectedProduitId);
    }

    this.movements = result;
    this.cdr.markForCheck();
  }

  getDelegueLabel(id: number | null): string {
    if (id == null) return '';
    return this.delegues.find(d => d.id === id)?.label ?? `Délégué #${id}`;
  }

  getProduitLabel(id: number | null): string {
    if (id == null) return '';
    return this.produits.find(p => p.id === id)?.label ?? `Produit #${id}`;
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
