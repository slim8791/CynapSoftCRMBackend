import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { PromotionService, PromotionDto } from '../services/promotion.service';
import { LotService } from '../../lots/lot.service';
import { LotDto } from '../../lots/lot.model';
import { ProductService } from '../../products/product.service';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-promotion-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './promotion-form.component.html',
  styleUrls: ['./promotion-form.component.css']
})
export class PromotionFormComponent implements OnInit, OnDestroy {

  form!:          FormGroup;
  isEdit          = false;
  loading         = false;
  loadingLots     = false;
  loadingProducts = false;
  error           = '';

  lots:     LotDto[] = [];
  products: any[]    = [];

  private promoId: number | null = null;
  private destroy$ = new Subject<void>();

  constructor(
    private fb:         FormBuilder,
    private route:      ActivatedRoute,
    private router:     Router,
    private svc:        PromotionService,
    private lotSvc:     LotService,
    private productSvc: ProductService,
    private toast:      ToastService,
    private cdr:        ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const today = new Date().toISOString().slice(0, 10);
    this.form = this.fb.group({
      codePromo:           ['', [Validators.required, Validators.maxLength(50)]],
      typePromotion:       ['Pourcentage', Validators.required],
      // Percentage fields
      pourcentage:         [null],
      // Freebie fields
      seuilAchat:          [null],
      quantiteGratuite:    [null],
      // Scope
      porteeSurTousLesLots: [false, Validators.required],
      numeroLot:           [null],
      id_Produit:          [null],
      // Common
      dateDebut:           [today, Validators.required],
      dateExpiration:      ['',    Validators.required],
      estActive:           [true]
    });

    // Recompute validators when type or scope changes
    this.form.get('typePromotion')!.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => { this.updateValidators(); this.cdr.markForCheck(); });

    this.form.get('porteeSurTousLesLots')!.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => { this.updateValidators(); this.cdr.markForCheck(); });

    this.loadLots();
    this.loadProducts();

    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isEdit  = true;
      this.promoId = Number(id);
      this.loadPromo();
    } else {
      this.updateValidators();
    }
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  // ── Validators ─────────────────────────────────────

  private updateValidators(): void {
    const type  = this.form.get('typePromotion')!.value;
    const scope = this.form.get('porteeSurTousLesLots')!.value;

    const pct  = this.form.get('pourcentage')!;
    const seuil = this.form.get('seuilAchat')!;
    const qty   = this.form.get('quantiteGratuite')!;
    const lot   = this.form.get('numeroLot')!;
    const prod  = this.form.get('id_Produit')!;

    if (type === 'Pourcentage') {
      pct.setValidators([Validators.required, Validators.min(1), Validators.max(100)]);
      seuil.clearValidators();
      qty.clearValidators();
    } else {
      pct.clearValidators();
      seuil.setValidators([Validators.required, Validators.min(1)]);
      qty.setValidators([Validators.required, Validators.min(1)]);
    }

    if (scope) {
      prod.setValidators(Validators.required);
      lot.clearValidators();
    } else {
      lot.setValidators(Validators.required);
      prod.clearValidators();
    }

    [pct, seuil, qty, lot, prod].forEach(c => c.updateValueAndValidity({ emitEvent: false }));
  }

  // ── Data loading ────────────────────────────────────

  private loadLots(): void {
    this.loadingLots = true;
    this.lotSvc.getAllLots().pipe(takeUntil(this.destroy$)).subscribe({
      next: lots => {
        this.lots = lots.filter(l => !l.isExpired && !l.isOutOfStock);
        this.loadingLots = false;
        this.cdr.markForCheck();
      },
      error: () => { this.lots = []; this.loadingLots = false; this.cdr.markForCheck(); }
    });
  }

  private loadProducts(): void {
    this.loadingProducts = true;
    this.productSvc.getProducts().pipe(takeUntil(this.destroy$)).subscribe({
      next: (prods: any[]) => {
        this.products = (prods || []).map((p: any) => ({
          ...p,
          id:  p['id_Produit'] ?? p.Id_Produit ?? p.idProduit ?? p.id ?? 0,
          nom: p.nom ?? p.Nom ?? p.name ?? `Product ${p['id_Produit'] ?? p.Id_Produit}`,
        })).filter((p: any) => p.id > 0);
        this.loadingProducts = false;
        this.cdr.markForCheck();
      },
      error: () => { this.products = []; this.loadingProducts = false; this.cdr.markForCheck(); }
    });
  }

  private loadPromo(): void {
    this.loading = true;
    this.svc.getById(this.promoId!).pipe(takeUntil(this.destroy$)).subscribe({
      next: p => {
        this.form.patchValue({
          codePromo:            p.codePromo,
          typePromotion:        p.typePromotion ?? 'Pourcentage',
          pourcentage:          p.pourcentage ?? null,
          seuilAchat:           p.seuilAchat ?? null,
          quantiteGratuite:     p.quantiteGratuite ?? null,
          porteeSurTousLesLots: p.porteeSurTousLesLots ?? false,
          numeroLot:            p.numeroLot ?? null,
          id_Produit:           p.id_Produit ?? null,
          dateDebut:            p.dateDebut?.slice(0, 10),
          dateExpiration:       p.dateExpiration?.slice(0, 10),
          estActive:            p.estActive
        });
        this.updateValidators();
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => { this.error = 'Cannot load promotion.'; this.loading = false; }
    });
  }

  // ── Submit ─────────────────────────────────────────

  onSubmit(): void {
    this.updateValidators();
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;

    const v = this.form.value;
    const dto: PromotionDto = {
      ...(this.promoId ? { id_Promo: this.promoId } : {}),
      codePromo:            v.codePromo,
      typePromotion:        v.typePromotion,
      pourcentage:          v.typePromotion === 'Pourcentage' ? Number(v.pourcentage) : undefined,
      seuilAchat:           v.typePromotion === 'Gratuite'    ? Number(v.seuilAchat)    : undefined,
      quantiteGratuite:     v.typePromotion === 'Gratuite'    ? Number(v.quantiteGratuite) : undefined,
      porteeSurTousLesLots: v.porteeSurTousLesLots,
      numeroLot:            v.porteeSurTousLesLots ? undefined : v.numeroLot,
      id_Produit:           v.porteeSurTousLesLots ? Number(v.id_Produit) : undefined,
      dateDebut:            v.dateDebut,
      dateExpiration:       v.dateExpiration,
      estActive:            v.estActive
    };

    this.svc.createOrUpdate(dto).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toast.showSuccess(this.isEdit ? 'Promotion updated.' : 'Promotion created.');
        this.router.navigate(['/promotions']);
      },
      error: () => { this.error = 'Error saving promotion.'; this.loading = false; }
    });
  }

  // ── Helpers ────────────────────────────────────────

  get isPercentage(): boolean { return this.form.get('typePromotion')!.value === 'Pourcentage'; }
  get isFreebie():    boolean { return this.form.get('typePromotion')!.value === 'Gratuite'; }
  get allLots():      boolean { return this.form.get('porteeSurTousLesLots')!.value === true
                                    || this.form.get('porteeSurTousLesLots')!.value === 'true'; }

  isInvalid(f: string): boolean {
    const c = this.form.get(f);
    return !!(c?.invalid && c?.touched);
  }

  lotLabel(l: LotDto): string {
    const exp = l.dateExpiration
      ? new Date(l.dateExpiration).toLocaleDateString('en-GB', { day: '2-digit', month: '2-digit', year: '2-digit' })
      : '—';
    return `${l.numero}  —  qty ${l.quantite}  ·  exp. ${exp}`;
  }
}
