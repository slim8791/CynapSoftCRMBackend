import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { StockMovementService } from '../services/stock-movement.service';
import { ToastService } from '../../../../shared/services/toast.service';

@Component({
  selector: 'app-movement-form',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './movement-form.component.html',
  styleUrls: ['./movement-form.component.css']
})
export class MovementFormComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  submitting = false;
  submitError = '';
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private svc: StockMovementService,
    private toast: ToastService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      type:        ['Decrement', Validators.required],
      idStock:     [null, [Validators.required, Validators.min(1)]],
      idStockDest: [null],
      qte:         [null, [Validators.required, Validators.min(1)]]
    });

    this.form.get('type')!.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.updateDestValidation();
      this.cdr.markForCheck();
    });
  }

  get isTransfer(): boolean { return this.form.get('type')?.value === 'Transfer'; }

  private updateDestValidation(): void {
    const destCtrl = this.form.get('idStockDest')!;
    if (this.isTransfer) {
      destCtrl.setValidators([Validators.required, Validators.min(1)]);
    } else {
      destCtrl.clearValidators();
      destCtrl.setValue(null);
    }
    destCtrl.updateValueAndValidity();
  }

  fieldInvalid(name: string): boolean {
    const c = this.form.get(name);
    return !!(c?.invalid && c.touched);
  }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    const { type, idStock, idStockDest, qte } = this.form.value;
    this.submitting = true;
    this.submitError = '';

    let action$;
    if (type === 'Decrement') {
      action$ = this.svc.decrement(idStock, qte);
    } else if (type === 'Increment') {
      action$ = this.svc.increment(idStock, qte);
    } else {
      action$ = this.svc.transfer(idStock, idStockDest, qte);
    }

    action$.pipe(takeUntil(this.destroy$)).subscribe({
      next: result => {
        if (!result) {
          this.submitError = 'Stock insuffisant ou introuvable.';
          this.submitting = false;
          this.cdr.markForCheck();
          return;
        }
        this.toast.showSuccess('Mouvement enregistré.');
        this.router.navigate(['/inventory/movements']);
      },
      error: () => {
        this.submitError = 'Erreur lors de l\'enregistrement.';
        this.submitting = false;
        this.cdr.markForCheck();
      }
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
