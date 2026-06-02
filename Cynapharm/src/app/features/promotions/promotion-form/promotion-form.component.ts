import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { PromotionService, PromotionDto } from '../services/promotion.service';
import { LotService } from '../../lots/lot.service';
import { LotDto } from '../../lots/lot.model';
import { ToastService } from '../../../shared/services/toast.service';
import { dateRangeValidator } from '../../../core/validators/date-range.validator';
import { noWhitespaceValidator } from '../../../core/validators/no-whitespace.validator';

@Component({
  selector: 'app-promotion-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './promotion-form.component.html',
  styleUrls: ['./promotion-form.component.css']
})
export class PromotionFormComponent implements OnInit, OnDestroy {

  form!: FormGroup;
  isEdit      = false;
  loading     = false;
  error       = '';

  availableLots: LotDto[] = [];
  loadingLots   = false;

  private promoId: number | null = null;
  private destroy$ = new Subject<void>();

  constructor(
    private fb:         FormBuilder,
    private route:      ActivatedRoute,
    private router:     Router,
    private svc:        PromotionService,
    private lotService: LotService,
    private toast:      ToastService,
    private cdr:        ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const today = new Date().toISOString().slice(0, 10);
    this.form = this.fb.group({
      codePromo:      ['', [Validators.required, Validators.maxLength(50), noWhitespaceValidator]],
      pourcentage:    [null, [Validators.required, Validators.min(1), Validators.max(100)]],
      numeroLot:      ['', Validators.required],
      dateDebut:      [today, Validators.required],
      dateExpiration: ['', Validators.required],
      estActive:      [true]
    }, { validators: dateRangeValidator('dateDebut', 'dateExpiration') });

    this.loadLots();

    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isEdit  = true;
      this.promoId = Number(id);
      this.loadPromo();
    }
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  private loadLots(): void {
    this.loadingLots = true;
    this.lotService.getAllLots().pipe(takeUntil(this.destroy$)).subscribe({
      next: lots => {
        this.availableLots = lots.filter(l => !l.isExpired && !l.isOutOfStock);
        this.loadingLots   = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.loadingLots = false;
        this.error = 'Impossible de charger la liste des lots.';
        this.cdr.markForCheck();
      }
    });
  }

  private loadPromo(): void {
    this.loading = true;
    this.svc.getById(this.promoId!).pipe(takeUntil(this.destroy$)).subscribe({
      next: p => {
        this.form.patchValue({
          codePromo:      p.codePromo,
          pourcentage:    p.pourcentage,
          numeroLot:      p.numeroLot,
          dateDebut:      p.dateDebut?.slice(0, 10),
          dateExpiration: p.dateExpiration?.slice(0, 10),
          estActive:      p.estActive
        });
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => { this.error = 'Impossible de charger la promotion.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.error = 'Veuillez corriger les erreurs du formulaire avant de soumettre.';
      return;
    }

    this.loading = true;
    this.error   = '';

    const v = this.form.value;
    const dto: PromotionDto = {
      ...(this.promoId ? { id_Promo: this.promoId } : {}),
      codePromo:            v.codePromo,
      typePromotion:        'Pourcentage',
      pourcentage:          Number(v.pourcentage),
      porteeSurTousLesLots: false,
      numeroLot:            v.numeroLot,
      dateDebut:            v.dateDebut,
      dateExpiration:       v.dateExpiration,
      estActive:            v.estActive
    };

    this.svc.createOrUpdate(dto).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toast.showSuccess(this.isEdit ? 'Promotion mise à jour.' : 'Promotion créée.');
        this.router.navigate(['/promotions']);
      },
      error: () => { this.error = 'Erreur lors de l\'enregistrement.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  isInvalid(f: string): boolean {
    const c = this.form.get(f);
    return !!(c?.invalid && c?.touched);
  }

  get dateRangeInvalid(): boolean {
    const ctrl = this.form.get('dateExpiration');
    return !!(ctrl?.errors?.['dateRange'] && ctrl?.touched);
  }
}
