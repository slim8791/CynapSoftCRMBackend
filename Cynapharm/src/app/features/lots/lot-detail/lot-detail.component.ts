// ── lot-detail.component.ts ──────────────────────────────────────────────────
// - lot typé LotDto (plus any)
// - getLotStatus() / getStatusText() / getStatusClass() importés du lot.model
//   (plus de duplication de la logique métier statut)
// - normalizeLot() supprimé : le service retourne déjà du LotDto propre
// - Syntaxe @if/@for Angular 17+ dans le template associé

import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { LotService }               from '../lot.service';
import { LotDto, getLotStatus, STATUS_LABEL, STATUS_CSS_CLASS } from '../lot.model';
import { InventoryService, StockDelegue } from '../../inventory/inventory.service';
import { PromotionAdvancedService } from '../../products/services/promotion-advanced.service';
import { AuthService, UserRole }    from '../../../core/services/auth.service';
import { ToastService }             from '../../../shared/services/toast.service';
import { CardComponent }            from '../../../shared/components/card/card.component';
import { ButtonComponent }          from '../../../shared/components/button/button.component';

@Component({
  selector: 'app-lot-detail',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, DatePipe, CardComponent, ButtonComponent],
  templateUrl: './lot-detail.component.html',
  styleUrls: ['./lot-detail.component.scss'],
})
export class LotDetailComponent implements OnInit, OnDestroy {

  // ── État principal ────────────────────────────────────────────────────────
  lot: LotDto | null = null;
  loading  = true;
  error    = '';
  productId: number | null = null;

  // ── Stock InventoryAPI ────────────────────────────────────────────────────
  inventoryStock: StockDelegue | null = null;
  loadingInventory = false;

  // ── Modal suppression ─────────────────────────────────────────────────────
  showDeleteModal = false;

  // ── Modal promotion ───────────────────────────────────────────────────────
  showPromoModal  = false;
  promoLoading    = false;
  promoError      = '';
  promoForm!: FormGroup;
  canManagePromo  = false;

  private numeroLot = '';
  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly route:        ActivatedRoute,
    private readonly router:       Router,
    private readonly fb:           FormBuilder,
    private readonly lotService:   LotService,
    private readonly inventoryService: InventoryService,
    private readonly promoService:    PromotionAdvancedService,
    private readonly authService:     AuthService,
    private readonly toastService:    ToastService,
    private readonly cdr:             ChangeDetectorRef,
  ) {}

  // ── Cycle de vie ──────────────────────────────────────────────────────────

  ngOnInit(): void {
    this.numeroLot = this.route.snapshot.paramMap.get('numero') ?? '';
    this.productId = Number(this.route.snapshot.queryParamMap.get('productId')) || null;

    const role = this.authService.getUserRole();
    this.canManagePromo = role === UserRole.ADMIN || role === UserRole.SUPERVISEUR;

    this.initPromoForm();

    if (!this.numeroLot) {
      this.error   = 'Numéro de lot manquant dans l\'URL.';
      this.loading = false;
      return;
    }

    this.loadLotDetails();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── Chargement du lot ─────────────────────────────────────────────────────

  private loadLotDetails(): void {
    this.loading = true;
    this.error   = '';

    this.lotService.getLotByNumero(this.numeroLot)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (lot: LotDto) => {
          console.log('Lot reçu :', lot); // Log pour débogage
          this.lot     = lot;    // LotDto typé, normalisé par le service
          this.loading = false;
          this.cdr.markForCheck();
          this.loadInventoryStock();
        },
        error: (err: any) => {
          console.error('Erreur lors du chargement du lot :', err); // Log pour débogage
          this.error   = err?.error?.message ?? 'Impossible de charger les détails du lot.';
          this.loading = false;
          this.cdr.markForCheck();
        },
      });
  }

  // ── Stock InventoryAPI ────────────────────────────────────────────────────

  private loadInventoryStock(): void {
    if (!this.lot?.numero) return;
    this.loadingInventory = true;

    this.inventoryService.getStockByLot(this.lot.numero)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (stock) => {
          this.inventoryStock   = stock;
          this.loadingInventory = false;
          this.cdr.markForCheck();
        },
        error: () => {
          this.inventoryStock   = null;
          this.loadingInventory = false;
          this.cdr.markForCheck();
        },
      });
  }

  // ── Helpers statut ────────────────────────────────────────────────────────
  // Délégués au lot.model : unicité de la logique métier

  getStatusText(): string {
    return this.lot ? STATUS_LABEL[getLotStatus(this.lot)] : '';
  }

  getStatusClass(): string {
    return this.lot ? STATUS_CSS_CLASS[getLotStatus(this.lot)] : '';
  }

  // ── Helpers statut stock InventoryAPI ─────────────────────────────────────

  getInvStatus(qte: number): 'ok' | 'faible' | 'rupture' {
    if (qte === 0) return 'rupture';
    if (qte <= 5)  return 'faible';
    return 'ok';
  }

  getInvStatusLabel(qte: number): string {
    const map: Record<'ok' | 'faible' | 'rupture', string> = {
      ok: 'En stock', faible: 'Faible', rupture: 'Rupture',
    };
    return map[this.getInvStatus(qte)];
  }

  // ── Modal suppression ─────────────────────────────────────────────────────

  onDelete(): void   { this.showDeleteModal = true; }
  cancelDelete(): void { this.showDeleteModal = false; }

  confirmDelete(): void {
    this.showDeleteModal = false;
    this.lotService.deleteLot(this.numeroLot)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toastService.showSuccess(`Lot ${this.numeroLot} supprimé avec succès.`);
          this.router.navigate(['/lots'], {
            queryParams: this.productId ? { productId: this.productId } : {},
          });
        },
        error: (err: any) => {
          this.toastService.showError(
            err?.error?.message ?? 'Erreur lors de la suppression du lot.'
          );
        },
      });
  }

  // ── Modal promotion ───────────────────────────────────────────────────────

  private initPromoForm(): void {
    const today = new Date().toISOString().slice(0, 10);
    this.promoForm = this.fb.group({
      codePromo:      ['', [Validators.required, Validators.maxLength(50)]],
      dateDebut:      [today, Validators.required],
      dateExpiration: ['', Validators.required],
      pourcentage:    ['', [Validators.required, Validators.min(1), Validators.max(100)]],
      estActive:      [true],
    });
  }

  openPromoModal(): void {
    this.promoError = '';
    this.promoForm.reset({
      codePromo: '', dateDebut: new Date().toISOString().slice(0, 10),
      dateExpiration: '', pourcentage: '', estActive: true,
    });
    this.showPromoModal = true;
  }

  cancelPromo(): void {
    this.showPromoModal = false;
    this.promoError     = '';
  }

  submitPromo(): void {
    if (this.promoForm.invalid) {
      this.promoForm.markAllAsTouched();
      return;
    }
    this.promoLoading = true;
    this.promoError   = '';

    const v = this.promoForm.value;
    const payload = {
      codePromo:      v.codePromo,
      dateDebut:      v.dateDebut,
      dateExpiration: v.dateExpiration,
      pourcentage:    Number(v.pourcentage),
      estActive:      v.estActive ?? true,
      numeroLot:      this.numeroLot,
    };

    this.promoService.createOrUpdatePromotion(payload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.promoLoading   = false;
          this.showPromoModal = false;
          this.toastService.showSuccess('Promotion ajoutée avec succès.');
          this.cdr.markForCheck();
        },
        error: (err: any) => {
          this.promoError   = err?.error?.message ?? 'Erreur lors de la création de la promotion.';
          this.promoLoading = false;
          this.cdr.markForCheck();
        },
      });
  }

  isPromoFieldInvalid(field: string): boolean {
    const ctrl = this.promoForm.get(field);
    return !!(ctrl?.invalid && ctrl?.touched);
  }

  // ── Helpers affichage promotions ──────────────────────────────────────────

  getFormattedPromotion(): string {
    const promos = this.lot?.promotions;
    if (!promos || promos.length === 0) return 'Aucune promotion';

    const active = promos.find(p => {
      const est = (p as any).estActive ?? (p as any).EstActive ?? true;
      return est;
    }) ?? promos[0];

    const code  = (active as any).codePromo  ?? (active as any).CodePromo  ?? '—';
    const pct   = (active as any).pourcentage ?? (active as any).Pourcentage ?? 0;
    const debut = (active as any).dateDebut   ?? (active as any).DateDebut;
    const exp   = (active as any).dateExpiration ?? (active as any).DateExpiration;

    const fmt = (iso: string) => {
      if (!iso) return '—';
      const d = new Date(iso);
      return isNaN(d.getTime()) ? '—'
        : `${String(d.getDate()).padStart(2,'0')}/${String(d.getMonth()+1).padStart(2,'0')}/${String(d.getFullYear()).slice(-2)}`;
    };

    const extra = promos.length > 1 ? ` (+${promos.length - 1} autre${promos.length > 2 ? 's' : ''})` : '';
    return `${code} — −${pct}%  ·  ${fmt(debut)} → ${fmt(exp)}${extra}`;
  }

  // ── Navigation ────────────────────────────────────────────────────────────

  onEdit(): void {
    this.router.navigate(['/lots', this.numeroLot, 'edit'], {
      queryParams: this.productId ? { productId: this.productId } : {},
    });
  }

  onBackToList(): void {
    this.router.navigate(['/lots'], {
      queryParams: this.productId ? { productId: this.productId } : {},
    });
  }
}