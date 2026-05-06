import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { ReclamationService, ReclamationDto } from '../services/reclamation.service';
import { ToastService } from '../../../../shared/services/toast.service';

@Component({
  selector: 'app-reclamation-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './reclamation-form.component.html',
  styleUrls: ['./reclamation-form.component.css'],
})
export class ReclamationFormComponent implements OnInit, OnDestroy {

  form!:   FormGroup;
  isEdit   = false;
  loading  = false;
  error    = '';
  private recId = 0;
  private destroy$ = new Subject<void>();

  constructor(
    private fb:     FormBuilder,
    private route:  ActivatedRoute,
    private router: Router,
    private svc:    ReclamationService,
    private toast:  ToastService,
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      Id_Commande: ['', [Validators.required, Validators.min(1)]],
      Id_Ligne:    ['', [Validators.required, Validators.min(1)]],
      Message:     ['', [Validators.required, Validators.minLength(5)]],
    });

    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id > 0) {
      this.isEdit = true;
      this.recId  = id;
      this.loading = true;
      this.svc.getById(id).pipe(takeUntil(this.destroy$)).subscribe({
        next: r => {
          if (r) this.form.patchValue({ Id_Commande: r.Id_Commande, Id_Ligne: r.Id_Ligne, Message: r.Message });
          this.loading = false;
        },
        error: () => { this.error = 'Réclamation introuvable.'; this.loading = false; },
      });
    }

    // Pré-remplir depuis queryParam orderId
    const oid = this.route.snapshot.queryParamMap.get('orderId');
    if (oid) this.form.patchValue({ Id_Commande: Number(oid) });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  onSubmit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;
    const v = this.form.value;
    // Id_Client injected server-side from JWT; Id_Rec = 0 for create
    const dto: ReclamationDto = {
      Id_Rec:       this.recId,
      Id_Commande:  Number(v.Id_Commande),
      Id_Ligne:     Number(v.Id_Ligne),
      Message:      v.Message,
      Id_Client:    0,   // server fills from JWT
      DateReclamation: new Date().toISOString(),
    };
    this.svc.createOrUpdate(dto).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toast.showSuccess(this.isEdit ? 'Réclamation mise à jour.' : 'Réclamation créée.');
        this.router.navigate(['/orders/reclamations']);
      },
      error: err => { this.error = err?.error?.Message ?? 'Erreur lors de l\'enregistrement.'; this.loading = false; },
    });
  }

  inv(f: string): boolean { const c = this.form.get(f); return !!(c?.invalid && c?.touched); }
}
