import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { RapportService, RapportDto } from '../services/rapport.service';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-rapport-form',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, EmptyStateComponent],
  templateUrl: './rapport-form.component.html',
  styleUrls: ['./rapport-form.component.css']
})
export class RapportFormComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  isEdit = false;
  editId: number | null = null;
  loadingData = false;
  saving = false;
  fetchError = '';
  submitError = '';
  successMsg = '';

  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private svc: RapportService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      id_Visite:       [null, [Validators.required]],
      id_User_Delegue: [null, [Validators.required]],
      commentaire:     ['',   [Validators.required]],
      resultat:        ['',   [Validators.required]]
    });

    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.isEdit = true;
      this.editId = id;
      this.loadingData = true;
      this.svc.getById(id).pipe(takeUntil(this.destroy$)).subscribe({
        next: data => { this.form.patchValue(data); this.loadingData = false; this.cdr.markForCheck(); },
        error: () => { this.fetchError = 'Impossible de charger le rapport.'; this.loadingData = false; this.cdr.markForCheck(); }
      });
    }
  }

  get f() { return this.form.controls; }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    this.saving = true;
    this.submitError = '';
    this.successMsg = '';

    const dto: RapportDto = {
      ...this.form.value,
      ...(this.isEdit && this.editId ? { idRapport: this.editId } : {})
    };

    this.svc.createOrUpdate(dto).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.saving = false;
        this.successMsg = this.isEdit ? 'Rapport mis à jour.' : 'Rapport créé.';
        this.cdr.markForCheck();
        setTimeout(() => this.router.navigate(['/field/rapports']), 1200);
      },
      error: () => { this.submitError = 'Erreur lors de l\'enregistrement.'; this.saving = false; this.cdr.markForCheck(); }
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
