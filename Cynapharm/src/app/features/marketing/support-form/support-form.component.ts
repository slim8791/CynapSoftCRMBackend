// ── support-form.component.ts ────────────────────────────────────────────────
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { MarketingService, SupportMarketingDto } from '../marketing.service';
import { CardComponent }   from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';

@Component({
  selector: 'app-support-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, CardComponent, ButtonComponent],
  templateUrl: './support-form.component.html',
  styleUrls: ['./support-form.component.css'],
})
export class SupportFormComponent implements OnInit {

  supportForm: FormGroup;
  isEditMode = false;
  loading    = false;
  error      = '';
  success    = false;

  productId  = 0;
  private supportId = 0;

  readonly supportTypes = [
    'Brochure', 'Flyer', 'Catalogue', 'Vidéo',
    'Présentation', 'Affiche', 'Document technique', 'Autre',
  ];

  constructor(
    private readonly fb:               FormBuilder,
    private readonly route:            ActivatedRoute,
    private readonly router:           Router,
    private readonly marketingService: MarketingService,
  ) {
    this.supportForm = this.fb.group({
      type:         ['', Validators.required],
      campaignName: ['', Validators.required],
      isActive:     [true],
    });
  }

  ngOnInit(): void {
    this.productId = Number(this.route.snapshot.queryParamMap.get('productId')) || 0;
    const idParam  = this.route.snapshot.paramMap.get('id');

    if (idParam) {
      this.isEditMode = true;
      this.supportId  = Number(idParam);
      this.loadSupport();
    }
  }

  // ── Chargement (mode édition) ─────────────────────────────────────────────

  private loadSupport(): void {
    this.loading = true;
    this.marketingService.getSupportById(this.supportId).subscribe({
      next: (response: any) => {
        const data = response?.Result ?? response?.result ?? response;
        this.supportForm.patchValue({
          type:         data.Type         ?? data.type         ?? '',
          campaignName: data.CampaignName ?? data.campaignName ?? '',
          isActive:     data.IsActive     ?? data.isActive     ?? true,
        });
        this.loading = false;
      },
      error: (err: any) => {
        this.error   = err?.error?.message ?? 'Impossible de charger le support.';
        this.loading = false;
      },
    });
  }

  // ── Soumission ────────────────────────────────────────────────────────────

  onSubmit(): void {
    if (this.supportForm.invalid) {
      this.supportForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.error   = '';
    this.success = false;

    const v = this.supportForm.value;

    // Envoie les deux casings pour être compatible avec ou sans JsonNamingPolicy.CamelCase
    const payload: SupportMarketingDto = {
  ...(this.isEditMode ? {
    Id_SupportMarketting: this.supportId,
  } : {}),
  Type: v.type,
  CampaignName: v.campaignName,
  IsActive: v.isActive ?? true,
  Id_Produit: this.productId,
};
    this.marketingService.createOrUpdateSupport(payload).subscribe({
      next: () => {
        this.success = true;
        this.loading = false;
        setTimeout(() => {
          this.router.navigate(['/marketing/supports'], {
            queryParams: { productId: this.productId },
          });
        }, 1200);
      },
      error: (err: any) => {
        this.error   = err?.error?.message ?? 'Erreur lors de l\'enregistrement.';
        this.loading = false;
      },
    });
  }

  // ── Annulation ────────────────────────────────────────────────────────────

  onCancel(): void {
    this.router.navigate(['/marketing/supports'], {
      queryParams: { productId: this.productId },
    });
  }

  // ── Helper template ───────────────────────────────────────────────────────

  isInvalid(field: string): boolean {
    const ctrl = this.supportForm.get(field);
    return !!(ctrl?.invalid && ctrl?.touched);
  }
}