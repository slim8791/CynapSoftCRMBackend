import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ObjectifService, ObjectifDto } from '../services/objectif.service';
import { TypeObjectif, PeriodeObjectif } from '../../../../core/models/enums/index';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-objectif-form',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, EmptyStateComponent],
  templateUrl: './objectif-form.component.html',
  styleUrls: ['./objectif-form.component.css']
})
export class ObjectifFormComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  isEdit = false;
  editId: number | null = null;
  loadingData = false;
  saving = false;
  fetchError = '';
  submitError = '';
  successMsg = '';

  typeOptions = [
    { value: TypeObjectif.Visites,         label: 'Visites' },
    { value: TypeObjectif.ChiffreAffaires, label: 'Chiffre d\'affaires' },
    { value: TypeObjectif.NouveauxClients, label: 'Nouveaux clients' },
    { value: TypeObjectif.Fidelisation,    label: 'Fidélisation' }
  ];

  periodeOptions = [
    { value: PeriodeObjectif.Mensuel,     label: 'Mensuel' },
    { value: PeriodeObjectif.Trimestriel, label: 'Trimestriel' },
    { value: PeriodeObjectif.Annuel,      label: 'Annuel' }
  ];

  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private svc: ObjectifService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      id_User_Delegue: [null, [Validators.required]],
      type:            ['',   [Validators.required]],
      periode:         ['',   [Validators.required]],
      valeurCible:     [null, [Validators.required, Validators.min(0)]],
      dateDebut:       ['',   [Validators.required]],
      dateFin:         ['',   [Validators.required]]
    });

    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.isEdit = true;
      this.editId = id;
      this.loadingData = true;
      this.svc.getById(id).pipe(takeUntil(this.destroy$)).subscribe({
        next: data => {
          this.form.patchValue({
            ...data,
            dateDebut: data.dateDebut?.substring(0, 10) ?? '',
            dateFin:   data.dateFin?.substring(0, 10) ?? ''
          });
          this.loadingData = false;
          this.cdr.markForCheck();
        },
        error: () => { this.fetchError = 'Impossible de charger l\'objectif.'; this.loadingData = false; this.cdr.markForCheck(); }
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

    const dto: ObjectifDto = {
      ...this.form.value,
      valeurRealisee: 0,
      ...(this.isEdit && this.editId ? { idObjectif: this.editId } : {})
    };

    this.svc.createOrUpdate(dto).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.saving = false;
        this.successMsg = this.isEdit ? 'Objectif mis à jour.' : 'Objectif créé.';
        this.cdr.markForCheck();
        setTimeout(() => this.router.navigate(['/field/objectifs']), 1200);
      },
      error: () => { this.submitError = 'Erreur lors de l\'enregistrement.'; this.saving = false; this.cdr.markForCheck(); }
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
