import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { PromotionService, PromotionDto } from '../services/promotion.service';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-promotion-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './promotion-form.component.html',
  styleUrls: ['./promotion-form.component.css']
})
export class PromotionFormComponent implements OnInit, OnDestroy {

  form!: FormGroup;
  isEdit  = false;
  loading = false;
  error   = '';

  private promoId: number | null = null;
  private destroy$ = new Subject<void>();

  constructor(
    private fb:     FormBuilder,
    private route:  ActivatedRoute,
    private router: Router,
    private svc:    PromotionService,
    private toast:  ToastService
  ) {}

  ngOnInit(): void {
    const today = new Date().toISOString().slice(0, 10);
    this.form = this.fb.group({
      codePromo:      ['', [Validators.required, Validators.maxLength(50)]],
      pourcentage:    ['', [Validators.required, Validators.min(1), Validators.max(100)]],
      numeroLot:      ['', Validators.required],
      dateDebut:      [today, Validators.required],
      dateExpiration: ['', Validators.required],
      estActive:      [true]
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isEdit   = true;
      this.promoId  = Number(id);
      this.loadPromo();
    }
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

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
      },
      error: () => { this.error = 'Impossible de charger la promotion.'; this.loading = false; }
    });
  }

  onSubmit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;
    const dto: PromotionDto = {
      ...(this.promoId ? { id_Promo: this.promoId } : {}),
      ...this.form.value
    };
    this.svc.createOrUpdate(dto).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toast.showSuccess(this.isEdit ? 'Promotion mise à jour.' : 'Promotion créée.');
        this.router.navigate(['/promotions']);
      },
      error: () => { this.error = 'Erreur lors de l\'enregistrement.'; this.loading = false; }
    });
  }

  isInvalid(f: string): boolean {
    const c = this.form.get(f);
    return !!(c?.invalid && c?.touched);
  }
}
