import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DistributionService, EchantillonDto } from '../services/distribution.service';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-distribution-form',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, EmptyStateComponent],
  templateUrl: './distribution-form.component.html',
  styleUrls: ['./distribution-form.component.css']
})
export class DistributionFormComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  saving = false;
  submitError = '';
  successMsg = '';

  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private svc: DistributionService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      id_Delegue:    [null, [Validators.required]],
      id_Medecin:    [null],
      id_Pharmacien: [null],
      id_Stock:      [null, [Validators.required]],
      qte:           [null, [Validators.required, Validators.min(1)]],
      numeroLot:     ['',   [Validators.required]]
    });
  }

  get f() { return this.form.controls; }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    this.saving = true;
    this.submitError = '';
    this.successMsg = '';

    const dto: EchantillonDto = this.form.value;

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
