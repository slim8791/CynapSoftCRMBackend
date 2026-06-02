import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { StockService, StockDelegueDto } from '../services/stock.service';
import { UserService } from '../../../users/user.service';
import { ProductService } from '../../../products/product.service';
import { LotService } from '../../../lots/lot.service';
import { LotDto } from '../../../lots/lot.model';

@Component({
  selector: 'app-stock-form',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './stock-form.component.html',
  styleUrls: ['./stock-form.component.css']
})
export class StockFormComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  isEdit      = false;
  editId: number | null = null;
  loadingData = false;
  saving      = false;
  fetchError  = '';
  submitError = '';
  successMsg  = '';

  delegues:      any[]    = [];
  products:      any[]    = [];
  lots:          LotDto[] = [];
  loadingLots    = false;

  // displayed expiration (formatted dd/MM/yyyy)
  lotDateDisplay = '';

  private destroy$ = new Subject<void>();

  constructor(
    private fb:         FormBuilder,
    private route:      ActivatedRoute,
    private router:     Router,
    private svc:        StockService,
    private userSvc:    UserService,
    private productSvc: ProductService,
    private lotSvc:     LotService,
    private cdr:        ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      id_User_Delegue: [null, [Validators.required]],
      id_Produit:      [null, [Validators.required]],
      numeroLot:       ['',   [Validators.required]],
      dateExpiration:  ['',   [Validators.required]],
      qteDisponible:   [null, [Validators.required, Validators.min(1)]]
    });

    // Disable lot select until a product is chosen
    this.form.get('numeroLot')!.disable();

    // Load delegues and products in parallel
    this.userSvc.getUsersByRole('DELEGUE').pipe(takeUntil(this.destroy$))
      .subscribe({
        next: users => {
          // Normalize: handle both camelCase (id) and PascalCase (Id) from auth API
          this.delegues = users.map(u => ({
            ...u,
            id:   u.id   ?? u.Id,
            name: u.name ?? u.Name ?? u.fullName ?? u.FullName ?? u.email ?? u.Email
          })).filter(u => u.id != null);
          this.cdr.markForCheck();
        },
        error: () => {}
      });

    this.productSvc.getVisibleProducts().pipe(takeUntil(this.destroy$))
      .subscribe({
        next: prods => {
          this.products = prods.map((p: any) => ({
            ...p,
            Id_Produit: p.Id_Produit ?? p.id_Produit,
            Nom:        p.Nom        ?? p.nom ?? ''
          }));
          this.cdr.markForCheck();
        },
        error: () => {}
      });

    // When product changes → reload lots, clear lot + date
    this.form.get('id_Produit')!.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(id => {
      this.lots = [];
      this.lotDateDisplay = '';
      const lotCtrl = this.form.get('numeroLot')!;
      lotCtrl.setValue('', { emitEvent: false });
      lotCtrl.disable();
      this.form.patchValue({ dateExpiration: '' }, { emitEvent: false });
      if (id) this.loadLots(+id);
    });

    // When lot changes → auto-fill expiration date and set max quantity
    this.form.get('numeroLot')!.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(num => {
      const lot = this.lots.find(l => l.numero === num);
      
      // Dynamic max validator
      if (lot) {
        this.form.get('qteDisponible')!.setValidators([Validators.required, Validators.min(1), Validators.max(lot.quantite)]);
      } else {
        this.form.get('qteDisponible')!.setValidators([Validators.required, Validators.min(1)]);
      }
      this.form.get('qteDisponible')!.updateValueAndValidity({ emitEvent: false });

      if (lot?.dateExpiration) {
        const iso  = lot.dateExpiration.substring(0, 10);
        const [y, m, d] = iso.split('-');
        this.lotDateDisplay = `${d}/${m}/${y}`;
        this.form.patchValue({ dateExpiration: iso }, { emitEvent: false });
      } else {
        this.lotDateDisplay = '';
        this.form.patchValue({ dateExpiration: '' }, { emitEvent: false });
      }
      this.cdr.markForCheck();
    });

    // Edit mode
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.isEdit     = true;
      this.editId     = id;
      this.loadingData = true;
      this.svc.getById(id).pipe(takeUntil(this.destroy$)).subscribe({
        next: data => {
          this.form.patchValue({
            id_User_Delegue: data.id_User_Delegue,
            id_Produit:      data.id_Produit,
            numeroLot:       data.numeroLot,
            dateExpiration:  data.dateExpiration?.substring(0, 10) ?? '',
            qteDisponible:   data.qteDisponible
          }, { emitEvent: false });

          // Format display date
          const raw = data.dateExpiration?.substring(0, 10) ?? '';
          if (raw) {
            const [y, m, d] = raw.split('-');
            this.lotDateDisplay = `${d}/${m}/${y}`;
          }

          this.loadingData = false;
          this.cdr.markForCheck();

          // Load lots for the existing product (needed to show lot dropdown)
          if (data.id_Produit) {
            this.loadLots(data.id_Produit, data.numeroLot);
          }
        },
        error: () => { this.fetchError = 'Impossible de charger le stock.'; this.loadingData = false; this.cdr.markForCheck(); }
      });
    }
  }

  private loadLots(productId: number, preselectLot?: string): void {
    this.loadingLots = true;
    this.lotSvc.getLotsByProductId(productId).pipe(takeUntil(this.destroy$)).subscribe({
      next: lots => {
        this.lots = lots.filter(l => !l.isExpired);
        // In edit mode, keep the existing lot even if expired so selection isn't lost
        if (preselectLot && !this.lots.find(l => l.numero === preselectLot)) {
          const existing = lots.find(l => l.numero === preselectLot);
          if (existing) this.lots = [existing, ...this.lots];
        }
        this.loadingLots = false;
        // Enable the lot control now that options are loaded
        this.form.get('numeroLot')!.enable();
        this.cdr.markForCheck();
      },
      error: () => { this.loadingLots = false; this.cdr.markForCheck(); }
    });
  }

  userName(u: any): string {
    return u?.name ?? u?.Name ?? u?.fullName ?? u?.email ?? `#${u?.id}`;
  }

  productName(p: any): string {
    return p?.Nom ?? p?.nom ?? `#${p?.Id_Produit ?? p?.id_Produit}`;
  }

  get f() { return this.form.controls; }

  lotLabel(l: LotDto): string {
    const exp = l.dateExpiration
      ? new Date(l.dateExpiration).toLocaleDateString('en-GB', { day: '2-digit', month: '2-digit', year: '2-digit' })
      : '—';
    return `${l.numero}  —  qty ${l.quantite}  ·  exp. ${exp}`;
  }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    this.saving     = true;
    this.submitError = '';
    this.successMsg  = '';

    const v = this.form.getRawValue(); // includes disabled controls (numeroLot)
    const dto: StockDelegueDto = {
      id_User_Delegue: +v.id_User_Delegue,
      id_Produit:      +v.id_Produit,
      numeroLot:       v.numeroLot,
      dateExpiration:  v.dateExpiration,
      qteDisponible:   +v.qteDisponible,
      qteReservee:     0,          // managed by the system
      ...(this.isEdit && this.editId ? { id_stock: this.editId } : {})
    };

    this.svc.createOrUpdate(dto).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.saving     = false;
        this.successMsg = this.isEdit ? 'Stock mis à jour.' : 'Stock créé avec succès.';
        this.cdr.markForCheck();
        setTimeout(() => this.router.navigate(['/inventory/stocks']), 1200);
      },
      error: () => {
        this.submitError = 'Erreur lors de l\'enregistrement du stock.';
        this.saving      = false;
        this.cdr.markForCheck();
      }
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
