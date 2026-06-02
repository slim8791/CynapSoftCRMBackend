import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { DistributionService, EchantillonDto } from '../services/distribution.service';
import { UserService } from '../../../users/user.service';
import { StockService, StockDelegueDto } from '../../stocks/services/stock.service';

function recipientRequired(form: AbstractControl): ValidationErrors | null {
  const med = form.get('id_Medecin')?.value;
  const pha = form.get('id_Pharmacien')?.value;
  return (!med && !pha) ? { recipientRequired: true } : null;
}

@Component({
  selector: 'app-distribution-form',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './distribution-form.component.html',
  styleUrls: ['./distribution-form.component.css']
})
export class DistributionFormComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  saving = false;
  submitError = '';
  successMsg = '';

  delegues: any[] = [];
  medecins: any[] = [];
  pharmaciens: any[] = [];
  stocks: StockDelegueDto[] = [];
  loadingStocks = false;

  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private svc: DistributionService,
    private userSvc: UserService,
    private stockSvc: StockService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.form = this.fb.group({
      id_Delegue: [null, [Validators.required]],
      id_Medecin: [null],
      id_Pharmacien: [null],
      id_Stock: [null, [Validators.required]],
      qte: [null, [Validators.required, Validators.min(1)]],
      numeroLot: ['', [Validators.required]]
    }, { validators: recipientRequired });

    this.form.get('id_Stock')?.disable();

    this.loadUsers();

    // When delegue changes, reload stocks
    this.form.get('id_Delegue')!.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(id => {
      this.form.patchValue({ id_Stock: null, numeroLot: '' });
      if (id) {
        this.loadStocks(+id);
      } else {
        this.form.get('id_Stock')?.disable();
        this.stocks = [];
      }
    });

    // When stock changes, auto-fill numeroLot
    this.form.get('id_Stock')!.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(id => {
      const stock = this.stocks.find(s => s.id_stock === +id);
      if (stock) this.form.patchValue({ numeroLot: stock.numeroLot }, { emitEvent: false });
    });
  }

  private loadUsers(): void {
    this.userSvc.getAllUsers().pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res: any) => {
          const users: any[] = res?.result ?? res?.Result ?? res ?? [];
          const active = (x: any) => !x.isDeleted && !x.IsDeleted;
          const role = (x: any) => (x.role ?? x.Role ?? '').toLowerCase();

          this.delegues = users.filter(x => active(x) && role(x) === 'delegue');
          this.medecins = users.filter(x => active(x) && role(x) === 'medecin');
          this.pharmaciens = users.filter(x => active(x) && role(x) === 'client');

          this.cdr.markForCheck();
        },
        error: () => { }
      });
  }

  private loadStocks(delegueId: number): void {
    this.loadingStocks = true;
    this.stockSvc.getByDelegue(delegueId).pipe(takeUntil(this.destroy$)).subscribe({
      next: s => {
        this.stocks = s;
        this.loadingStocks = false;
        if (s.length > 0) {
          this.form.get('id_Stock')?.enable();
        }
        this.cdr.markForCheck();
      },
      error: () => { this.loadingStocks = false; this.cdr.markForCheck(); }
    });
  }

  userName(u: any): string {
    const name = u?.fullName ?? u?.FullName ?? u?.name ?? u?.Name;
    if (!name || name.toLowerCase() === 'string') return u?.email ?? `#${u?.id}`;
    return name;
  }

  get f() { return this.form.controls; }
  get recipientError(): boolean {
    return !!(this.form.errors?.['recipientRequired'] && this.form.touched);
  }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    this.saving = true;
    this.submitError = '';
    this.successMsg = '';

    const v = this.form.getRawValue();
    const dto: EchantillonDto = {
      id_Delegue: +v.id_Delegue,
      id_Medecin: v.id_Medecin ? +v.id_Medecin : null,
      id_Pharmacien: v.id_Pharmacien ? +v.id_Pharmacien : null,
      id_Stock: +v.id_Stock,
      qte: +v.qte,
      numeroLot: v.numeroLot
    };

    this.svc.createOrUpdate(dto).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.saving = false;
        this.successMsg = 'Distribution créée avec succès.';
        this.cdr.markForCheck();
        setTimeout(() => this.router.navigate(['/inventory/distributions']), 1200);
      },
      error: () => { this.submitError = 'Erreur lors de la création.'; this.saving = false; this.cdr.markForCheck(); }
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
