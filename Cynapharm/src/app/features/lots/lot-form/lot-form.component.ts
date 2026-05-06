// ── lot-form.component.ts ────────────────────────────────────────────────────
// Respecte intégralement le LotDto C# :
//   - numero, dateExpiration, quantite, idProduit (→ id_Produit backend)
//   - isExpired / isOutOfStock sont des champs calculés backend : jamais envoyés
//   - idPromotion retiré : la relation Promotion est gérée séparément (lot-detail)

import { Component, OnInit, inject, DestroyRef, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormsModule, ReactiveFormsModule,
  FormBuilder, FormGroup, Validators,
  AbstractControl, ValidationErrors
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { ToastService }   from '../../../shared/services/toast.service';
import { LotService }     from '../lot.service';
import { ProductService } from '../../products/product.service';
import { LotDto }         from '../lot.model';
import { CardComponent }  from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';

@Component({
  selector: 'app-lot-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink, CardComponent, ButtonComponent],
  templateUrl: './lot-form.component.html',
  styleUrls: ['./lot-form.component.scss'],
})
export class LotFormComponent implements OnInit {

  // ── État ──────────────────────────────────────────────────────────────────
  lotForm!: FormGroup;
  isEditMode = false;
  numeroLot  = '';
  loading    = false;
  error      = '';
  success    = false;

  /** Contexte produit passé via queryParams */
  productId: number = 0;

  /** Conserve la date originale pour ne pas invalider un lot déjà expiré en édition */
  originalExpirationDate = '';

  // ── Injection ─────────────────────────────────────────────────────────────
  private readonly destroyRef    = inject(DestroyRef);
  private readonly toastService  = inject(ToastService);
  private readonly cdr           = inject(ChangeDetectorRef);

  constructor(
    private readonly fb:             FormBuilder,
    private readonly router:         Router,
    private readonly route:          ActivatedRoute,
    private readonly lotService:     LotService,
    private readonly productService: ProductService,
  ) {}

  // ── Cycle de vie ──────────────────────────────────────────────────────────

  ngOnInit(): void {
    this.productId = Number(this.route.snapshot.queryParamMap.get('productId')) || 0;
    this.initForm();

    this.route.params
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(params => {
        if (params['numero']) {
          this.isEditMode = true;
          this.numeroLot  = params['numero'];
          this.lotForm.patchValue({ numero: this.numeroLot });
          this.cdr.markForCheck(); // ✅ Force la mise à jour du bouton
          this.loadLotData();
        }
        // En création, le champ idProduit est pré-rempli depuis le queryParam
      });
  }

  // ── Initialisation du formulaire ──────────────────────────────────────────

  private initForm(): void {
    this.lotForm = this.fb.group({
      // Miroir exact des champs éditables du LotDto C#
      numero: ['', [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(50),
        Validators.pattern(/^[a-zA-Z0-9\-_]+$/),
      ]],
      idProduit: [this.productId || '', Validators.required],
      dateExpiration: ['', [
        Validators.required,
        this.futureDateValidator.bind(this),
      ]],
      quantite: ['', [
        Validators.required,
        Validators.min(1),
        Validators.max(999999),
      ]],
      // isExpired & isOutOfStock : champs calculés backend — absents du formulaire
    });
  }

  // ── Validateur de date ────────────────────────────────────────────────────

  /**
   * Autorise la date originale en mode édition (lot peut-être déjà expiré).
   * Bloque toute nouvelle date passée.
   */
  private futureDateValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value as string;
    if (!value) return null;
    if (this.isEditMode && value === this.originalExpirationDate) return null;

    const selected = new Date(value);
    const today    = new Date();
    selected.setHours(0, 0, 0, 0);
    today.setHours(0, 0, 0, 0);

    return selected >= today ? null : { pastDate: true };
  }

  // ── Chargement des données ────────────────────────────────────────────────

  private loadLotData(): void {
    this.loading = true;
    this.error   = '';

    this.lotService.getLotByNumero(this.numeroLot)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (lot: LotDto) => {
          // Conserve la date originale pour le validateur
          this.originalExpirationDate = lot.dateExpiration ?? '';

          this.lotForm.patchValue({
            numero:         lot.numero,
            idProduit:      lot.idProduit,
            dateExpiration: lot.dateExpiration,
            quantite:       lot.quantite,
          });

          // Le champ numéro est en lecture seule en édition
          this.lotForm.get('numero')?.disable();

          this.loading = false;
          this.cdr.markForCheck(); // ✅ Mise à jour après chargement
        },
        error: (err: any) => {
          this.error   = `Impossible de charger le lot (${err.status ?? err.message}).`;
          this.loading = false;
          this.cdr.markForCheck(); // ✅ Mise à jour même en cas d'erreur
        },
      });
  }

  // ── Helpers template ──────────────────────────────────────────────────────

  isInvalid(field: string): boolean {
    const ctrl = this.lotForm.get(field);
    return !!(ctrl?.invalid && ctrl?.touched);
  }

  ctrl(field: string): AbstractControl {
    return this.lotForm.get(field)!;
  }

  // ── Soumission ────────────────────────────────────────────────────────────

  onSubmit(): void {
    if (this.lotForm.invalid) {
      this.lotForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.error   = '';
    this.success = false;

    // Construit un LotDto valide (sans isExpired / isOutOfStock qui sont calculés backend)
    const formValue = this.lotForm.getRawValue(); // getRawValue inclut les champs disabled
    const payload: LotDto = {
      numero:         this.isEditMode ? this.numeroLot : formValue.numero,
      idProduit:      Number(formValue.idProduit),
      dateExpiration: formValue.dateExpiration,
      quantite:       Number(formValue.quantite),
      // Champs calculés — le backend les recalcule, on envoie des valeurs par défaut
      isExpired:      false,
      isOutOfStock:   false,
    };

    this.lotService.createOrUpdateLot(payload)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.success = true;
          this.loading = false;
          this.cdr.markForCheck(); // ✅ Mise à jour après succès
          this.toastService.showSuccess(
            this.isEditMode ? 'Lot mis à jour avec succès.' : 'Lot créé avec succès.'
          );
          setTimeout(() => {
            this.router.navigate(
              this.isEditMode ? ['/lots', this.numeroLot] : ['/lots'],
              { queryParams: this.productId ? { productId: this.productId } : {} }
            );
          }, 1200);
        },
        error: (err: any) => {
          this.error   = err?.error?.message
            ?? `Erreur lors de ${this.isEditMode ? 'la mise à jour' : 'la création'} du lot.`;
          this.loading = false;
          this.cdr.markForCheck(); // ✅ Mise à jour après erreur
        },
      });
  }

  onCancel(): void {
    this.router.navigate(['/lots'], {
      queryParams: this.productId ? { productId: this.productId } : {},
    });
  }
}