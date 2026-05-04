import { Component, OnInit, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ToastService } from '../../../shared/services/toast.service';
import { LotService } from '../lot.service';
import { ProductService } from '../../products/product.service';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';

@Component({
  selector: 'app-lot-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    CardComponent,
    ButtonComponent
  ],
  templateUrl: './lot-form.component.html',
  styleUrls: ['./lot-form.component.css']
})

export class LotFormComponent implements OnInit {

  lotForm!: FormGroup;
  isEditMode = false;
  numeroLot = '';
  loading = false;
  error = '';
  success = false;

  products: any[] = [];
  productId: number = 0;

  private readonly destroyRef = inject(DestroyRef);
  private readonly toastService = inject(ToastService);

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private lotService: LotService,
    private productService: ProductService
  ) {}

  ngOnInit(): void {
    this.productId = Number(this.route.snapshot.queryParamMap.get('productId')) || 0;
    this.initForm();
    
    this.route.params
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(params => {
        if (params['numero']) {
          this.isEditMode = true;
          this.numeroLot = params['numero'];
          this.lotForm.patchValue({ numero: this.numeroLot });
          this.loadLotData();
        } else {
          // Create mode
          this.loadProducts();
        }
      });
  }

  private initForm(): void {
    this.lotForm = this.fb.group({
      numero: ['', [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(50),
        Validators.pattern(/^[a-zA-Z0-9\-_]+$/)
      ]],
      idProduit: [this.productId || '', Validators.required],
      dateExpiration: ['', [
        Validators.required,
        this.futureDateValidator.bind(this)
      ]],
      quantite: ['', [
        Validators.required,
        Validators.min(1),
        Validators.max(999999)
      ]],
      idPromotion: [null]
    });
  }

  /**
   * Validateur personnalisé : date >= aujourd'hui
   */
  private futureDateValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) return null;
    const date = new Date(control.value);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    date.setHours(0, 0, 0, 0);
    return date >= today ? null : { pastDate: true };
  }

  private loadProducts(): void {
    this.productService.getProducts().subscribe({
      next: (data: any) => {
        this.products = Array.isArray(data) ? data : data ? [data] : [];
        console.log('[LotForm] Loaded products:', this.products);
      },
      error: (err: any) => {
        console.error('[LotForm] Error loading products:', err);
        this.products = [];
      }
    });
  }

  private loadLotData(): void {
    this.loading = true;
    this.error = '';

    this.lotService.getLotByNumero(this.numeroLot)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data: any) => {
          const lot = data?.Result ?? data;
          this.lotForm.patchValue({
            numero: lot.numero ?? this.numeroLot,
            idProduit: lot.idProduit ?? 0,
            dateExpiration: lot.dateExpiration ?? '',
            quantite: lot.quantite ?? 0,
            idPromotion: lot.idPromotion ?? null
          });
          this.loading = false;
        },
        error: (err) => {
          this.error = `Impossible de charger le lot (${err.status ?? err.message}).`;
          this.loading = false;
        }
      });
  }

  // ── Helpers ──
  isInvalid(field: string): boolean {
    const ctrl = this.lotForm.get(field);
    return !!(ctrl?.invalid && ctrl?.touched);
  }

  ctrl(field: string): AbstractControl {
    return this.lotForm.get(field)!;
  }

  onSubmit(): void {
    if (this.lotForm.invalid) {
      this.lotForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.error = '';
    this.success = false;

    const payload = { ...this.lotForm.value };
    if (this.isEditMode) {
      payload.numero = this.numeroLot;
    }

    this.lotService.createOrUpdateLot(payload).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.success = true;
        this.loading = false;
        this.toastService.showSuccess(
          this.isEditMode ? 'Lot mis à jour avec succès.' : 'Lot créé avec succès.'
        );
        setTimeout(() => {
          if (this.isEditMode) {
            this.router.navigate(['/lots', this.numeroLot]);
          } else {
            this.router.navigate(['/lots'], { queryParams: { productId: this.productId } });
          }
        }, 1200);
      },
      error: (err) => {
        this.error = `Erreur lors de ${this.isEditMode ? 'la mise à jour' : 'la création'} du lot.`;
        this.loading = false;
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/lots'], {
      queryParams: { productId: this.productId }
    });
  }
}
