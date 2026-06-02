import { Component, OnInit, inject, DestroyRef, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
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

  // ── Catégories ───────────────────────────────────────
  categories: string[] = [];
  loadingCategories = false;
  catSelectValue = '';   // drives the <select>; '__new__' triggers the text input

  private productId = '';
  private loadedIsArchived = false;
  private readonly destroyRef     = inject(DestroyRef);
  private readonly cdr            = inject(ChangeDetectorRef);
  private readonly toastService   = inject(ToastService);
  private readonly fb             = inject(FormBuilder);
  private readonly route          = inject(ActivatedRoute);
  private readonly router         = inject(Router);
  private readonly productService = inject(ProductService);

  constructor() {
    this.productForm = this.fb.group({
      Nom:           ['', [Validators.required, Validators.maxLength(200)]],
      Description:   ['', [Validators.maxLength(1000)]],
      Categorie:     ['', [Validators.required]],
      Prix_Vente:    ['', [Validators.required, Validators.min(0)]],
      Prix_Creation: ['', [Validators.required, Validators.min(0)]],
      TVA:           [19, [Validators.required, Validators.min(0), Validators.max(100)]],
      isActive:      [true]
    }, { validators: this.priceOrderValidator });
  }

  ngOnInit(): void {
    this.loadCategories();

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
        this.cdr.detectChanges();
      });
  }

  // ── Catégories ───────────────────────────────────────

  private loadCategories(): void {
    this.loadingCategories = true;
    this.productService.getCategories()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: cats => {
          this.categories = cats;
          this.loadingCategories = false;
          // Re-sync select if the form already has a value (edit mode loaded first)
          const current = this.productForm.get('Categorie')?.value as string;
          if (current) {
            this.catSelectValue = cats.includes(current) ? current : '__new__';
          }
          this.cdr.detectChanges();
        },
        error: () => {
          this.loadingCategories = false;
          this.cdr.detectChanges();
        }
      });
  }

  get showNewCategoryInput(): boolean {
    return this.catSelectValue === '__new__';
  }

  onCategorySelectChange(value: string): void {
    this.catSelectValue = value;
    if (value !== '__new__') {
      this.productForm.get('Categorie')?.setValue(value);
      this.productForm.get('Categorie')?.markAsTouched();
    } else {
      this.productForm.get('Categorie')?.setValue('');
    }
    this.cdr.detectChanges();
  }

  // ── Helpers template ─────────────────────────────────

  isInvalid(field: string): boolean {
    const ctrl = this.productForm.get(field);
    return !!(ctrl?.invalid && ctrl?.touched);
  }

  ctrl(field: string): AbstractControl {
    return this.productForm.get(field)!;
  }

  get hasPriceOrderError(): boolean {
    const touched = !!(this.ctrl('Prix_Vente').touched || this.ctrl('Prix_Creation').touched);
    return touched && !!this.productForm.errors?.['priceBelowCreation'];
  }

  private priceOrderValidator(group: AbstractControl): ValidationErrors | null {
    const venteRaw = group.get('Prix_Vente')?.value;
    const creationRaw = group.get('Prix_Creation')?.value;
    const prixVente = parseFloat(String(venteRaw ?? '').replace(',', '.'));
    const prixCreation = parseFloat(String(creationRaw ?? '').replace(',', '.'));

    if (Number.isNaN(prixVente) || Number.isNaN(prixCreation)) return null;
    return prixVente < prixCreation ? { priceBelowCreation: true } : null;
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

          const cat = raw.Categorie ?? raw.categorie ?? '';
          this.productForm.patchValue({
            Nom:           raw.Nom           ?? raw.nom           ?? '',
            Description:   raw.Description   ?? raw.description   ?? '',
            Categorie:     cat,
            Prix_Vente:    raw.PrixVente      ?? raw.prixVente     ?? 0,
            Prix_Creation: raw.Prix_Creation  ?? raw.prix_Creation ?? 0,
            TVA:           raw.TVA            ?? raw.tva           ?? 19,
            isActive:      raw.IsActive       ?? raw.isActive      ?? true
          });

          // Sync the select: existing category or switch to text input
          if (cat) {
            this.catSelectValue = this.categories.includes(cat) ? cat : '__new__';
          }

          this.loading = false;
          this.cdr.detectChanges();
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
      Categorie:     v.Categorie ?? '',
      PrixVente:     parseFloat(String(v.Prix_Vente).replace(',', '.')),
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
        this.loadCategories();
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
