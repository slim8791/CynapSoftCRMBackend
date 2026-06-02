import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, forkJoin, of } from 'rxjs';
import { takeUntil, catchError, map } from 'rxjs/operators';
import { StockMovementService } from '../services/stock-movement.service';
import { ToastService } from '../../../../shared/services/toast.service';
import { UserService } from '../../../users/user.service';
import { ProductService } from '../../../products/product.service';
import { StockService, StockDelegueDto } from '../../stocks/services/stock.service';

@Component({
  selector: 'app-movement-form',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './movement-form.component.html',
  styleUrls: ['./movement-form.component.css']
})
export class MovementFormComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  submitting = false;
  submitError = '';
  private destroy$ = new Subject<void>();

  stocks: { id: number; label: string }[] = [];
  loadingData = true;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private svc: StockMovementService,
    private toast: ToastService,
    private cdr: ChangeDetectorRef,
    private userService: UserService,
    private productSvc: ProductService,
    private stockSvc: StockService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      type:        ['Decrement', Validators.required],
      idStock:     [null, [Validators.required]],
      idStockDest: [null],
      qte:         [null, [Validators.required, Validators.min(1)]]
    });

    this.form.get('type')!.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.updateDestValidation();
      this.cdr.markForCheck();
    });
    this.loadReferenceData();
  }

  private loadReferenceData(): void {
    forkJoin({
      delegues: this.userService.getUsersByRole('DELEGUE').pipe(catchError(() => of([]))),
      produits: this.productSvc.getProducts().pipe(
        map(r => Array.isArray(r) ? r : []),
        catchError(() => of([]))
      ),
      stocksList: this.stockSvc.getAll(1, 10000).pipe(catchError(() => of([])))
    }).pipe(takeUntil(this.destroy$)).subscribe(({ delegues, produits, stocksList }) => {

      const deleguesMap = new Map((Array.isArray(delegues) ? delegues : []).map((u: any) => [
        u?.id ?? u?.Id ?? 0,
        u?.name ?? u?.Name ?? u?.email ?? `#${u?.id ?? u?.Id}`
      ]));

      const produitsMap = new Map((Array.isArray(produits) ? produits : []).map((p: any) => [
        p?.Id_Produit ?? p?.id_Produit ?? p?.id ?? p?.Id ?? 0,
        p?.Nom ?? p?.nom ?? p?.name ?? p?.Name ?? `#${p?.Id_Produit ?? p?.id}`
      ]));

      this.stocks = (Array.isArray(stocksList) ? stocksList : [])
        .filter((s: StockDelegueDto) => s.id_stock != null)
        .map((s: StockDelegueDto) => {
          const delegueLabel = deleguesMap.get(s.id_User_Delegue) ?? `Délégué #${s.id_User_Delegue}`;
          const produitLabel = produitsMap.get(s.id_Produit) ?? `Produit #${s.id_Produit}`;
          return {
            id: s.id_stock!,
            label: `${delegueLabel} — ${produitLabel} (Lot: ${s.numeroLot || 'N/A'}) [Dispo: ${s.qteDisponible}]`
          };
        });

      this.loadingData = false;
      this.cdr.markForCheck();
    });
  }

  get isTransfer(): boolean { return this.form.get('type')?.value === 'Transfer'; }

  private updateDestValidation(): void {
    const destCtrl = this.form.get('idStockDest')!;
    if (this.isTransfer) {
      destCtrl.setValidators([Validators.required]);
    } else {
      destCtrl.clearValidators();
      destCtrl.setValue(null);
    }
    destCtrl.updateValueAndValidity();
  }

  fieldInvalid(name: string): boolean {
    const c = this.form.get(name);
    return !!(c?.invalid && c.touched);
  }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    const { type, idStock, idStockDest, qte } = this.form.value;
    this.submitting = true;
    this.submitError = '';

    let action$;
    if (type === 'Decrement') {
      action$ = this.svc.decrement(idStock, qte);
    } else if (type === 'Increment') {
      action$ = this.svc.increment(idStock, qte);
    } else {
      action$ = this.svc.transfer(idStock, idStockDest, qte);
    }

    action$.pipe(takeUntil(this.destroy$)).subscribe({
      next: result => {
        if (!result) {
          this.submitError = 'Stock insuffisant ou introuvable.';
          this.submitting = false;
          this.cdr.markForCheck();
          return;
        }
        this.toast.showSuccess('Mouvement enregistré.');
        this.router.navigate(['/inventory/movements']);
      },
      error: () => {
        this.submitError = 'Erreur lors de l\'enregistrement.';
        this.submitting = false;
        this.cdr.markForCheck();
      }
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
