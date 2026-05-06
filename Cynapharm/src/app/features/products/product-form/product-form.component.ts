import { Component, OnInit, inject, DestroyRef, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { ActivatedRoute, RouterLink, Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ToastService } from '../../../shared/services/toast.service';
import { ProductService } from '../product.service';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink],
  templateUrl: './product-form.component.html',
  styleUrls: ['./product-form.component.scss']
})
export class ProductFormComponent implements OnInit {

  productForm: FormGroup;
  isEditMode = false;
  loading = false;
  error = '';
  success = false;

  private productId = '';
  private loadedIsArchived = false;
  private readonly destroyRef  = inject(DestroyRef);
  private readonly cdr         = inject(ChangeDetectorRef);
  private readonly toastService   = inject(ToastService);
  private readonly fb             = inject(FormBuilder);
  private readonly route          = inject(ActivatedRoute);
  private readonly router         = inject(Router);
  private readonly productService = inject(ProductService);

  constructor() {
    this.productForm = this.fb.group({
      Nom:           ['', [Validators.required, Validators.maxLength(200)]],
      Description:   ['', [Validators.maxLength(1000)]],
      Prix_Vente:    ['', [Validators.required, Validators.min(0)]],
      Prix_Creation: ['', [Validators.required, Validators.min(0)]],
      TVA:           [19, [Validators.required, Validators.min(0), Validators.max(100)]],
      isActive:      [true]
    });
  }

  ngOnInit(): void {
    this.route.params
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(params => {
        if (params['id'] && params['id'] !== 'new') {
          this.isEditMode = true;
          this.productId  = params['id'];
          this.loadProductData();
        } else {
          this.isEditMode = false;
        }
        this.cdr.detectChanges(); // ← force le template à relire isEditMode
      });
  }

  // ── Helpers template ────────────────────────────────

  isInvalid(field: string): boolean {
    const ctrl = this.productForm.get(field);
    return !!(ctrl?.invalid && ctrl?.touched);
  }

  ctrl(field: string): AbstractControl {
    return this.productForm.get(field)!;
  }

  formatDecimal(event: Event, controlName: string): void {
    const input = event.target as HTMLInputElement;
    let value = input.value.replace(',', '.');
    value = value.replace(/[^\d.]/g, '');
    const parts = value.split('.');
    if (parts.length > 2) value = parts[0] + '.' + parts.slice(1).join('');
    if (parts[1]?.length > 3) value = parts[0] + '.' + parts[1].slice(0, 3);
    this.productForm.get(controlName)?.setValue(value, { emitEvent: false });
  }

  // ── Chargement (mode édition) ────────────────────────

  private loadProductData(): void {
    this.loading = true;
    this.error   = '';

    this.productService.getProductById(this.productId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data: any) => {
          const raw = data?.Result ?? data;

          this.loadedIsArchived = raw.IsArchived ?? raw.isArchived ?? false;
          this.productForm.patchValue({
            Nom:           raw.Nom           ?? raw.nom           ?? '',
            Description:   raw.Description   ?? raw.description   ?? '',
            Prix_Vente:    raw.Prix_Vente     ?? raw.prix_Vente    ?? 0,
            Prix_Creation: raw.Prix_Creation  ?? raw.prix_Creation ?? 0,
            TVA:           raw.TVA            ?? raw.tva           ?? 19,
            isActive:      raw.IsActive       ?? raw.isActive      ?? true
          });

          this.loading = false;
          this.cdr.detectChanges(); // ← le bouton affiche "Mettre à jour" immédiatement
        },
        error: (err) => {
          this.error   = `Impossible de charger le produit (${err.status ?? err.message}).`;
          this.loading = false;
          this.cdr.detectChanges();
        }
      });
  }

  // ── Soumission ───────────────────────────────────────

  onSubmit(): void {
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.error   = '';
    this.success = false;

    const v = this.productForm.value;

    const productData = {
      Id_Produit:    this.isEditMode ? parseInt(this.productId, 10) : 0,
      Nom:           v.Nom,
      Description:   v.Description ?? '',
      Prix_Vente:    parseFloat(String(v.Prix_Vente).replace(',', '.')),
      Prix_Creation: parseFloat(String(v.Prix_Creation).replace(',', '.')),
      TVA:           Number(v.TVA),
      IsActive:      v.isActive ?? true,
      IsArchived:    this.loadedIsArchived
    };

    const request = this.isEditMode
      ? this.productService.updateProduct(this.productId, productData)
      : this.productService.createProduct(productData);

    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.success = true;
        this.loading = false;
        this.toastService.showSuccess(
          this.isEditMode ? 'Produit mis à jour avec succès.' : 'Produit créé avec succès.'
        );
        setTimeout(() => this.router.navigate(['/products']), 1200);
      },
      error: () => {
        this.error   = `Erreur lors de ${this.isEditMode ? 'la mise à jour' : 'la création'} du produit.`;
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }
}