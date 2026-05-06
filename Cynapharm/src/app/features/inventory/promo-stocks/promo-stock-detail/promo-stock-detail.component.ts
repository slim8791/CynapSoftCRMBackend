import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { PromoStockService, StockGratuiteDto, StockEchantillonDto } from '../services/promo-stock.service';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-promo-stock-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, FormsModule, EmptyStateComponent],
  templateUrl: './promo-stock-detail.component.html',
  styleUrls: ['./promo-stock-detail.component.css']
})
export class PromoStockDetailComponent implements OnInit, OnDestroy {
  stockId: number | null = null;
  loadingLookup = false;
  lookupError = '';
  searched = false;

  gratuiteData: StockGratuiteDto | null = null;
  echantillonData: StockEchantillonDto | null = null;

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
    private fb: FormBuilder,
    private svc: PromoStockService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.gratuiteForm = this.fb.group({
      id_User_Delegue: [null, Validators.required],
      id_Produit:      [null, Validators.required],
      numeroLot:       ['',   Validators.required],
      qteDisponible:   [0,    [Validators.required, Validators.min(0)]],
      qteReservee:     [0,    [Validators.required, Validators.min(0)]],
      qteGratuite:     [0,    [Validators.required, Validators.min(0)]],
      typePromotion:   ['',   Validators.required]
    });
    this.echantillonForm = this.fb.group({
      id_User_Delegue: [null, Validators.required],
      id_Produit:      [null, Validators.required],
      numeroLot:       ['',   Validators.required],
      qteDisponible:   [0,    [Validators.required, Validators.min(0)]],
      qteReservee:     [0,    [Validators.required, Validators.min(0)]],
      qteEchantillon:  [0,    [Validators.required, Validators.min(0)]]
    });
  }

  lookup(): void {
    if (!this.stockId) return;
    this.loadingLookup = true;
    this.lookupError = '';
    this.searched = true;
    this.gratuiteData = null;
    this.echantillonData = null;

    const id = this.stockId;

    this.svc.getGratuite(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: d => { this.gratuiteData = d; if (d) this.gratuiteForm.patchValue({ ...d, id_stock: undefined }); this.loadingLookup = false; this.cdr.markForCheck(); },
      error: () => { this.loadingLookup = false; this.cdr.markForCheck(); }
    });

    this.svc.getEchantillon(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: d => { this.echantillonData = d; if (d) this.echantillonForm.patchValue({ ...d, id_stock: undefined }); this.cdr.markForCheck(); },
      error: () => { this.cdr.markForCheck(); }
    });
  }

  saveGratuite(): void {
    this.gratuiteForm.markAllAsTouched();
    if (this.gratuiteForm.invalid || !this.stockId) return;
    this.savingGratuite = true;
    this.gratuiteError = '';
    this.gratuiteSuccess = '';
    const dto: StockGratuiteDto = { ...this.gratuiteForm.value, id_stock: this.stockId };
    this.svc.createOrUpdateGratuite(dto).pipe(takeUntil(this.destroy$)).subscribe({
      next: d => { this.gratuiteData = d; this.savingGratuite = false; this.gratuiteSuccess = 'Enregistré.'; this.cdr.markForCheck(); },
      error: () => { this.gratuiteError = 'Erreur lors de l\'enregistrement.'; this.savingGratuite = false; this.cdr.markForCheck(); }
    });
  }

  saveEchantillon(): void {
    this.echantillonForm.markAllAsTouched();
    if (this.echantillonForm.invalid || !this.stockId) return;
    this.savingEchantillon = true;
    this.echantillonError = '';
    this.echantillonSuccess = '';
    const dto: StockEchantillonDto = { ...this.echantillonForm.value, id_stock: this.stockId };
    this.svc.createOrUpdateEchantillon(dto).pipe(takeUntil(this.destroy$)).subscribe({
      next: d => { this.echantillonData = d; this.savingEchantillon = false; this.echantillonSuccess = 'Enregistré.'; this.cdr.markForCheck(); },
      error: () => { this.echantillonError = 'Erreur lors de l\'enregistrement.'; this.savingEchantillon = false; this.cdr.markForCheck(); }
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
