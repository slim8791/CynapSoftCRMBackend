import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
  AbstractControl,
  ValidationErrors
} from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { PromoStockService } from '../services/promo-stock.service';
import { StockService, StockDelegueDto } from '../../stocks/services/stock.service';
import { ToastService } from '../../../../shared/services/toast.service';
import { UserService } from '../../../users/user.service';
import { ProductService } from '../../../products/product.service';

function typeValidator(form: AbstractControl): ValidationErrors | null {
  const type = form.get('type')?.value;
  if (type === 'echantillon') {
    const qte = form.get('qteEchantillon')?.value;
    return qte > 0 ? null : { qteRequired: true };
  }
  if (type === 'gratuite') {
    const qa = form.get('quantiteAchat')?.value;
    const qg = form.get('quantiteGratuite')?.value;
    const qd = form.get('qteDisponible')?.value;
    if (!qa || !qg || !qd) return { gratuiteRequired: true };
    if (qg >= qa) return { gratuiteTooHigh: true };
  }
  return null;
}

@Component({
  selector: 'app-promo-stock-form',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, FormsModule],
  templateUrl: './promo-stock-form.component.html',
  styleUrls: ['./promo-stock-form.component.css']
})
export class PromoStockFormComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  allStocks: StockDelegueDto[] = [];
  loadingStocks = false;
  selectedStock: StockDelegueDto | null = null;
  submitting = false;
  submitError = '';

  selectedDelegueId: number | null = null;
  selectedProduitId: number | null = null;

  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private promoSvc: PromoStockService,
    private stockSvc: StockService,
    private toast: ToastService,
    private userSvc: UserService,
    private productSvc: ProductService,
    private cdr: ChangeDetectorRef
  ) {}

  deleguesData: any[] = [];
  productsData: any[] = [];

  ngOnInit(): void {
    this.form = this.fb.group({
      id_Stock:         [null, Validators.required],
      type:             ['echantillon', Validators.required],
      qteEchantillon:   [null],
      description:      [''],
      dateDebut:        [null],
      dateFin:          [null],
      quantiteAchat:    [null],
      quantiteGratuite: [null],
      qteDisponible:    [null]
    }, { validators: typeValidator });

    this.loadingStocks = true;
    this.stockSvc.getAll(1, 200).pipe(takeUntil(this.destroy$)).subscribe({
      next: data => { this.allStocks = data; this.loadingStocks = false; this.cdr.markForCheck(); },
      error: ()   => { this.loadingStocks = false; this.cdr.markForCheck(); }
    });

    this.userSvc.getUsersByRole('DELEGUE').pipe(takeUntil(this.destroy$)).subscribe({
      next: users => {
        this.deleguesData = users.map(u => ({
          ...u,
          id: u.id ?? u.Id,
          name: u.name ?? u.Name ?? u.fullName ?? u.FullName ?? u.email ?? u.Email
        })).filter(u => u.id != null);
        this.cdr.markForCheck();
      },
      error: () => {}
    });

    this.productSvc.getVisibleProducts().pipe(takeUntil(this.destroy$)).subscribe({
      next: prods => {
        this.productsData = prods.map((p: any) => ({
          ...p,
          Id_Produit: p.Id_Produit ?? p.id_Produit,
          Nom: p.Nom ?? p.nom ?? ''
        }));
        this.cdr.markForCheck();
      },
      error: () => {}
    });

    this.form.get('id_Stock')!.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(id => {
      this.onStockSelected(id);
    });

    this.form.get('type')!.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.cdr.markForCheck();
    });
  }

  get uniqueDelegues(): number[] {
    return [...new Set(this.allStocks.map(s => s.id_User_Delegue))].sort((a, b) => a - b);
  }

  get filteredProduits(): number[] {
    const stocks = this.selectedDelegueId
      ? this.allStocks.filter(s => s.id_User_Delegue === this.selectedDelegueId)
      : this.allStocks;
    return [...new Set(stocks.map(s => s.id_Produit))].sort((a, b) => a - b);
  }

  get filteredLots(): StockDelegueDto[] {
    return this.allStocks.filter(s =>
      (!this.selectedDelegueId || s.id_User_Delegue === this.selectedDelegueId) &&
      (!this.selectedProduitId || s.id_Produit === this.selectedProduitId)
    );
  }

  delegueName(id: number): string {
    const u = this.deleguesData.find(x => x.id === id);
    return u ? u.name : `Délégué #${id}`;
  }

  produitName(id: number): string {
    const p = this.productsData.find(x => x.Id_Produit === id);
    return p ? p.Nom : `Produit #${id}`;
  }

  onDelegueChange(id: any): void {
    this.selectedDelegueId = id ? +id : null;
    this.selectedProduitId = null;
    this.form.patchValue({ id_Stock: null });
    this.selectedStock = null;
    this.cdr.markForCheck();
  }

  onProduitChange(id: any): void {
    this.selectedProduitId = id ? +id : null;
    this.form.patchValue({ id_Stock: null });
    this.selectedStock = null;
    this.cdr.markForCheck();
  }

  onStockSelected(stockId: any): void {
    const stock = this.allStocks.find(s => s.id_stock === +stockId) ?? null;
    this.selectedStock = stock;
    if (stock) {
      this.form.get('qteEchantillon')!.setValidators([
        Validators.min(1),
        Validators.max(stock.qteDisponible)
      ]);
      this.form.get('qteEchantillon')!.updateValueAndValidity();
      this.form.get('qteDisponible')!.setValidators([
        Validators.min(1),
        Validators.max(stock.qteDisponible)
      ]);
      this.form.get('qteDisponible')!.updateValueAndValidity();
    }
    this.cdr.markForCheck();
  }

  get isEchantillon(): boolean { return this.form.get('type')?.value === 'echantillon'; }
  get isGratuite(): boolean    { return this.form.get('type')?.value === 'gratuite'; }
  get formErrors(): ValidationErrors | null { return this.form.errors; }

  get qteError(): boolean {
    const c = this.form.get('qteEchantillon');
    return !!(c?.invalid && c.touched && c.errors?.['max']);
  }

  fieldInvalid(name: string): boolean {
    const c = this.form.get(name);
    return !!(c?.invalid && c.touched);
  }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || !this.selectedStock) return;

    const v = this.form.value;
    const base = {
      id_stock:        0,
      id_User_Delegue: this.selectedStock.id_User_Delegue,
      id_Produit:      this.selectedStock.id_Produit,
      numeroLot:       this.selectedStock.numeroLot,
      dateExpiration:  this.selectedStock.dateExpiration,
      qteDisponible:   this.selectedStock.qteDisponible,
      qteReservee:     this.selectedStock.qteReservee
    };

    this.submitting = true;
    this.submitError = '';

    if (v.type === 'echantillon') {
      this.promoSvc.createOrUpdateEchantillon({
        ...base,
        qteEchantillon: v.qteEchantillon,
        description:    v.description  || null,
        dateDebut:      v.dateDebut    || null,
        dateFin:        v.dateFin      || null
      } as any)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.toast.showSuccess(`${v.qteEchantillon} échantillons alloués avec succès.`);
            setTimeout(() => this.router.navigate(['/inventory/promo-stocks']), 1200);
          },
          error: () => {
            this.submitError = 'Erreur lors de la création.';
            this.submitting = false;
            this.cdr.markForCheck();
          }
        });
    } else {
      this.promoSvc.createOrUpdateGratuite({
        ...base,
        qteGratuite:      v.qteDisponible,
        typePromotion:    'Gratuite',
        quantiteAchat:    v.quantiteAchat,
        quantiteGratuite: v.quantiteGratuite,
        dateDebut:        v.dateDebut  || null,
        dateFin:          v.dateFin    || null
      })
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.toast.showSuccess('Gratuité créée avec succès.');
            setTimeout(() => this.router.navigate(['/inventory/promo-stocks']), 1200);
          },
          error: () => {
            this.submitError = 'Erreur lors de la création.';
            this.submitting = false;
            this.cdr.markForCheck();
          }
        });
    }
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
