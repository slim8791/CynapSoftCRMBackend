import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import {
  ReactiveFormsModule, FormBuilder, FormGroup, Validators,
  AbstractControl, ValidationErrors
} from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { Subject, of } from 'rxjs';
import { takeUntil, catchError } from 'rxjs/operators';
import { PromoStockService, StockGratuiteDto, StockEchantillonDto } from '../services/promo-stock.service';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { UserService } from '../../../users/user.service';
import { ProductService } from '../../../products/product.service';

function dateRangeValidator(form: AbstractControl): ValidationErrors | null {
  const debut = form.get('dateDebut')?.value;
  const fin   = form.get('dateFin')?.value;
  if (debut && fin && fin < debut) return { dateRange: true };
  return null;
}

function gratuiteRuleValidator(form: AbstractControl): ValidationErrors | null {
  const achat    = +(form.get('quantiteAchat')?.value  ?? 0);
  const gratuite = +(form.get('quantiteGratuite')?.value ?? 0);
  if (achat > 0 && gratuite >= achat) return { gratuiteTooHigh: true };
  return null;
}

@Component({
  selector: 'app-promo-stock-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, FormsModule, EmptyStateComponent],
  templateUrl: './promo-stock-detail.component.html',
  styleUrls: ['./promo-stock-detail.component.css']
})
export class PromoStockDetailComponent implements OnInit, OnDestroy {
  activeTab: 'gratuite' | 'echantillon' = 'gratuite';

  allGratuite: StockGratuiteDto[] = [];
  allEchantillon: StockEchantillonDto[] = [];
  selectedGratuiteId: number | null = null;
  selectedEchantillonId: number | null = null;
  loadingGratuite = false;
  loadingEchantillon = false;

  gratuiteData: StockGratuiteDto | null = null;
  echantillonData: StockEchantillonDto | null = null;

  // Resolved display names
  delegueNames:  Record<number, string> = {};
  productNames:  Record<number, string> = {};

  gratuiteForm!: FormGroup;
  echantillonForm!: FormGroup;

  savingGratuite = false;
  savingEchantillon = false;
  gratuiteSuccess = '';
  gratuiteError = '';
  echantillonSuccess = '';
  echantillonError = '';

  private destroy$ = new Subject<void>();

  constructor(
    private fb:         FormBuilder,
    private svc:        PromoStockService,
    private userSvc:    UserService,
    private productSvc: ProductService,
    private cdr:        ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadingGratuite = true;
    this.svc.getAllGratuite().pipe(takeUntil(this.destroy$)).subscribe({
      next: data => {
        this.allGratuite = data;
        this.loadingGratuite = false;
        this.resolveNames();
        this.cdr.markForCheck();
      },
      error: () => { this.loadingGratuite = false; this.cdr.markForCheck(); }
    });

    this.loadingEchantillon = true;
    this.svc.getAllEchantillon().pipe(takeUntil(this.destroy$)).subscribe({
      next: data => {
        this.allEchantillon = data;
        this.loadingEchantillon = false;
        this.resolveNames();
        this.cdr.markForCheck();
      },
      error: () => { this.loadingEchantillon = false; this.cdr.markForCheck(); }
    });

    this.gratuiteForm = this.fb.group({
      id_User_Delegue:  [null, Validators.required],
      id_Produit:       [null, Validators.required],
      numeroLot:        ['',   Validators.required],
      qteDisponible:    [0,    [Validators.required, Validators.min(0)]],
      qteReservee:      [0,    [Validators.required, Validators.min(0)]],
      qteGratuite:      [0,    [Validators.required, Validators.min(1)]],
      typePromotion:    ['Gratuite'],
      quantiteAchat:    [0,    Validators.min(1)],
      quantiteGratuite: [0,    Validators.min(1)],
      dateDebut:        [null],
      dateFin:          [null]
    }, { validators: [dateRangeValidator, gratuiteRuleValidator] });

    this.echantillonForm = this.fb.group({
      id_User_Delegue: [null, Validators.required],
      id_Produit:      [null, Validators.required],
      numeroLot:       ['',   Validators.required],
      qteDisponible:   [0,    [Validators.required, Validators.min(0)]],
      qteReservee:     [0,    [Validators.required, Validators.min(0)]],
      qteEchantillon:  [0,    [Validators.required, Validators.min(1)]],
      description:     [null],
      dateDebut:       [null],
      dateFin:         [null]
    }, { validators: dateRangeValidator });
  }

  onSelectGratuite(id: any): void {
    const numId = +id;
    if (!numId) { this.gratuiteData = null; return; }
    this.gratuiteData = this.allGratuite.find(g => g.id_stock === numId) ?? null;
    if (this.gratuiteData) this.gratuiteForm.patchValue({ ...this.gratuiteData });
    this.gratuiteSuccess = '';
    this.gratuiteError = '';
    this.cdr.markForCheck();
  }

  onSelectEchantillon(id: any): void {
    const numId = +id;
    if (!numId) { this.echantillonData = null; return; }
    this.echantillonData = this.allEchantillon.find(e => e.id_stock === numId) ?? null;
    if (this.echantillonData) this.echantillonForm.patchValue({ ...this.echantillonData });
    this.echantillonSuccess = '';
    this.echantillonError = '';
    this.cdr.markForCheck();
  }

  saveGratuite(): void {
    this.gratuiteForm.markAllAsTouched();
    if (this.gratuiteForm.invalid || !this.selectedGratuiteId) return;
    this.savingGratuite = true;
    this.gratuiteError = '';
    this.gratuiteSuccess = '';
    const dto: StockGratuiteDto = { ...this.gratuiteForm.value, id_stock: this.selectedGratuiteId };
    this.svc.createOrUpdateGratuite(dto).pipe(takeUntil(this.destroy$)).subscribe({
      next: d => {
        this.gratuiteData = d;
        const idx = this.allGratuite.findIndex(g => g.id_stock === d.id_stock);
        if (idx >= 0) this.allGratuite[idx] = d;
        this.savingGratuite = false;
        this.gratuiteSuccess = 'Modifications enregistrées.';
        this.cdr.markForCheck();
      },
      error: () => { this.gratuiteError = 'Erreur lors de l\'enregistrement.'; this.savingGratuite = false; this.cdr.markForCheck(); }
    });
  }

  saveEchantillon(): void {
    this.echantillonForm.markAllAsTouched();
    if (this.echantillonForm.invalid || !this.selectedEchantillonId) return;
    this.savingEchantillon = true;
    this.echantillonError = '';
    this.echantillonSuccess = '';
    const dto: StockEchantillonDto = { ...this.echantillonForm.value, id_stock: this.selectedEchantillonId };
    this.svc.createOrUpdateEchantillon(dto).pipe(takeUntil(this.destroy$)).subscribe({
      next: d => {
        this.echantillonData = d;
        const idx = this.allEchantillon.findIndex(e => e.id_stock === d.id_stock);
        if (idx >= 0) this.allEchantillon[idx] = d;
        this.savingEchantillon = false;
        this.echantillonSuccess = 'Modifications enregistrées.';
        this.cdr.markForCheck();
      },
      error: () => { this.echantillonError = 'Erreur lors de l\'enregistrement.'; this.savingEchantillon = false; this.cdr.markForCheck(); }
    });
  }

  // ── Name resolution ───────────────────────────────────

  private resolveNames(): void {
    const delegueIds = [...new Set([
      ...this.allGratuite.map(g => g.id_User_Delegue),
      ...this.allEchantillon.map(e => e.id_User_Delegue)
    ])].filter(id => id > 0);

    const produitIds = [...new Set([
      ...this.allGratuite.map(g => g.id_Produit),
      ...this.allEchantillon.map(e => e.id_Produit)
    ])].filter(id => id > 0);

    if (delegueIds.length > 0) {
      this.userSvc.getDisplayNamesByIds(delegueIds)
        .pipe(takeUntil(this.destroy$))
        .subscribe(names => { Object.assign(this.delegueNames, names); this.cdr.markForCheck(); });
    }

    produitIds.forEach(id => {
      if (this.productNames[id]) return;
      this.productSvc.getProductById(id)
        .pipe(takeUntil(this.destroy$), catchError(() => of(null)))
        .subscribe(r => {
          const p = r?.Result ?? r?.result ?? r;
          this.productNames[id] = p?.nom ?? p?.Nom ?? p?.name ?? p?.Name ?? `Produit #${id}`;
          this.cdr.markForCheck();
        });
    });
  }

  getDeleagueName(id: number): string { return this.delegueNames[+id] ?? `#${id}`; }
  getProduitName(id: number):  string { return this.productNames[+id]  ?? `Produit #${id}`; }

  getDistributed(item: any): number {
    if (!item?.qteEchantillon) return 0;
    return Math.max(0, item.qteEchantillon - (item.qteDisponible ?? 0));
  }

  getUsagePercent(item: any): number {
    if (!item?.qteEchantillon) return 0;
    return Math.min(100, (this.getDistributed(item) / item.qteEchantillon) * 100);
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
