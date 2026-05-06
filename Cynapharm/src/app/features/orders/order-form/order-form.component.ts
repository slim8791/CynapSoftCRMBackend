import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { OrderService } from '../order.service';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-order-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './order-form.component.html',
  styleUrls: ['./order-form.component.css'],
})
export class OrderFormComponent implements OnInit, OnDestroy {

  form!:   FormGroup;
  loading  = false;
  error    = '';
  private destroy$ = new Subject<void>();

  constructor(
    private fb:     FormBuilder,
    private router: Router,
    private svc:    OrderService,
    private toast:  ToastService,
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      IsFinalValidation: [false],
      Lignes: this.fb.array([this.newLigne()]),
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  get lignes(): FormArray { return this.form.get('Lignes') as FormArray; }

  newLigne(): FormGroup {
    return this.fb.group({
      Id_Produit:   ['', [Validators.required, Validators.min(1)]],
      Quantite:     [1,  [Validators.required, Validators.min(1)]],
      PrixUnitaire: ['', [Validators.required, Validators.min(0)]],
      Remise:       [0,  [Validators.min(0), Validators.max(100)]],
    });
  }

  addLigne():       void { this.lignes.push(this.newLigne()); }
  removeLigne(i: number): void { if (this.lignes.length > 1) this.lignes.removeAt(i); }

  onSubmit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;
    const v = this.form.value;
    const dto = {
      Id_Client: 0,   // filled from JWT server-side
      IsFinalValidation: v.IsFinalValidation,
      Lignes: (v.Lignes as any[]).map((l: any) => ({
        Id_Commande:  0,
        Id_Ligne:     0,
        Id_Produit:   Number(l.Id_Produit),
        Quantite:     Number(l.Quantite),
        PrixUnitaire: Number(l.PrixUnitaire),
        Remise:       Number(l.Remise),
      })),
    };
    this.svc.createOrder(dto).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => { this.toast.showSuccess('Commande créée avec succès.'); this.router.navigate(['/orders']); },
      error: err => { this.error = err?.error?.Message ?? 'Erreur lors de la création.'; this.loading = false; },
    });
  }

  inv(g: FormGroup, f: string): boolean { const c = g.get(f); return !!(c?.invalid && c?.touched); }
}
