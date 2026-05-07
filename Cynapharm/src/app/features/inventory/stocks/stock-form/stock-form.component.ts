import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { StockService, StockDelegueDto } from '../services/stock.service';
import { ProductService } from '../../../products/product.service';
import { UserService } from '../../../users/user.service';
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

  form!:      FormGroup;
  isEdit      = false;
  editId:     number | null = null;

  loadingData = false;
  saving      = false;
  fetchError  = '';
  submitError = '';
  successMsg  = '';

  // Dropdown data
  delegates:       any[]     = [];
  products:        any[]     = [];
  lots:            LotDto[]  = [];
  loadingDelegates = false;
  loadingProducts  = false;
  loadingLots      = false;

  private destroy$ = new Subject<void>();

  constructor(
    private fb:         FormBuilder,
    private route:      ActivatedRoute,
    private router:     Router,
    private svc:        StockService,
    private productSvc: ProductService,
    private userSvc:    UserService,
    private lotSvc:     LotService,
    private cdr:        ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      id_User_Delegue: [null, Validators.required],
      id_Produit:      [null, Validators.required],
      numeroLot:       [null, Validators.required],
      dateExpiration:  ['',  Validators.required],
      qteDisponible:   [0,   [Validators.required, Validators.min(0)]],
      qteReservee:     [0,   Validators.min(0)]
    });

    // Auto-fill expiry when a lot is selected
    this.form.get('numeroLot')!.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(numero => {
        const lot = this.lots.find(l => l.numero === numero);
        if (lot?.dateExpiration) {
          this.form.patchValue({ dateExpiration: lot.dateExpiration.substring(0, 10) }, { emitEvent: false });
        }
      });

    // Reload lots when product changes
    this.form.get('id_Produit')!.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(productId => {
        if (productId) {
          this.form.patchValue({ numeroLot: null, dateExpiration: '' }, { emitEvent: false });
          this.loadLots(Number(productId));
        }
      });

    this.loadDropdowns();

    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.isEdit   = true;
      this.editId   = id;
      this.loadingData = true;
      this.svc.getById(id).pipe(takeUntil(this.destroy$)).subscribe({
        next: data => {
          // Load lots for the product first, then patch
          if (data.id_Produit) {
            this.loadLots(data.id_Produit, () => {
              this.form.patchValue({
                ...data,
                dateExpiration: data.dateExpiration ? data.dateExpiration.substring(0, 10) : ''
              });
              this.loadingData = false;
              this.cdr.markForCheck();
            });
          } else {
            this.form.patchValue({
              ...data,
              dateExpiration: data.dateExpiration ? data.dateExpiration.substring(0, 10) : ''
            });
            this.loadingData = false;
            this.cdr.markForCheck();
          }
        },
        error: () => {
          this.fetchError  = 'Cannot load stock entry.';
          this.loadingData = false;
          this.cdr.markForCheck();
        }
      });
    }
  }

  private loadDropdowns(): void {
    // Delegates
    this.loadingDelegates = true;
    this.userSvc.getUsers().pipe(takeUntil(this.destroy$)).subscribe({
      next: (r: any) => {
        const raw = r?.result ?? r?.Result ?? r;
        const list: any[] = Array.isArray(raw) ? raw : [];
        this.delegates = list.map((u: any) => ({
          id:   u.id   ?? u.Id   ?? u.userId ?? 0,
          name: u.name ?? u.Name ?? u.nom    ?? `User ${u.id ?? u.Id}`,
          role: u.role ?? u.Role ?? ''
        })).filter((u: any) => u.id > 0);
        this.loadingDelegates = false;
        this.cdr.markForCheck();
      },
      error: () => { this.loadingDelegates = false; this.cdr.markForCheck(); }
    });

    // Products
    this.loadingProducts = true;
    this.productSvc.getProducts().pipe(takeUntil(this.destroy$)).subscribe({
      next: (prods: any[]) => {
        this.products = (prods || []).map((p: any) => ({
          id:  p['id_Produit'] ?? p.Id_Produit ?? p.idProduit ?? 0,
          nom: p.nom ?? p.Nom ?? p.name ?? `Product ${p['id_Produit'] ?? p.Id_Produit}`
        })).filter((p: any) => p.id > 0);
        this.loadingProducts = false;
        this.cdr.markForCheck();
      },
      error: () => { this.loadingProducts = false; this.cdr.markForCheck(); }
    });
  }

  private loadLots(productId: number, onDone?: () => void): void {
    this.loadingLots = true;
    this.lots = [];
    this.lotSvc.getLotsByProductId(productId).pipe(takeUntil(this.destroy$)).subscribe({
      next: lots => {
        this.lots = lots.filter(l => !l.isExpired);
        this.loadingLots = false;
        this.cdr.markForCheck();
        onDone?.();
      },
      error: () => {
        this.lots = [];
        this.loadingLots = false;
        this.cdr.markForCheck();
        onDone?.();
      }
    });
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
    this.saving      = true;
    this.submitError = '';
    this.successMsg  = '';

    const dto: StockDelegueDto = {
      ...this.form.value,
      id_User_Delegue: Number(this.form.value.id_User_Delegue),
      id_Produit:      Number(this.form.value.id_Produit),
      ...(this.isEdit && this.editId ? { id_stock: this.editId } : {})
    };

    this.svc.createOrUpdate(dto).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.saving     = false;
        this.successMsg = this.isEdit ? 'Stock updated.' : 'Stock created.';
        this.cdr.markForCheck();
        setTimeout(() => this.router.navigate(['/inventory/stocks']), 1200);
      },
      error: () => {
        this.submitError = 'Error saving stock.';
        this.saving      = false;
        this.cdr.markForCheck();
      }
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
