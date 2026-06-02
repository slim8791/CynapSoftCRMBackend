import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { ObjectifService, ObjectifDto } from '../services/objectif.service';
import { UserService } from '../../../../features/users/user.service';
import { TypeObjectif, PeriodeObjectif } from '../../../../core/models/enums/index';

@Component({
  selector: 'app-objectif-form',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
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

  delegues: any[] = [];

  typeOptions = [
    { value: TypeObjectif.Visites, label: 'Visites' },
    { value: TypeObjectif.ChiffreAffaires, label: 'Chiffre d\'affaires' },
    { value: TypeObjectif.NouveauxClients, label: 'Nouveaux clients' },
    { value: TypeObjectif.Fidelisation, label: 'Fidélisation' }
  ];

  periodeOptions = [
    { value: PeriodeObjectif.Mensuel, label: 'Mensuel' },
    { value: PeriodeObjectif.Trimestriel, label: 'Trimestriel' },
    { value: PeriodeObjectif.Annuel, label: 'Annuel' }
  ];

  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private svc: ObjectifService,
    private userSvc: UserService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.form = this.fb.group({
      id_User_Delegue: [null, [Validators.required]],
      type: ['', [Validators.required]],
      periode: ['', [Validators.required]],
      valeurCible: [null, [Validators.required, Validators.min(1)]],
      dateDebut: ['', [Validators.required]],
      dateFin: ['', [Validators.required]]
    });

    // Load delegues
    this.userSvc.getUsersByRole('DELEGUE').pipe(takeUntil(this.destroy$))
      .subscribe({ next: u => { this.delegues = u; this.cdr.markForCheck(); }, error: () => { } });

    // Auto-calculate dates on periode change
    this.form.get('periode')!.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(val => {
      this.applyPeriodeDates(+val);
    });

    // Edit mode
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
            dateFin: data.dateFin?.substring(0, 10) ?? ''
          });
          this.loadingData = false;
          this.cdr.markForCheck();
        },
        error: () => { this.fetchError = 'Impossible de charger l\'objectif.'; this.loadingData = false; this.cdr.markForCheck(); }
      });
    }
  }

  private applyPeriodeDates(periode: number): void {
    const now = new Date();
    let dateDebut: Date, dateFin: Date;

    switch (periode) {
      case PeriodeObjectif.Mensuel:
        dateDebut = new Date(now.getFullYear(), now.getMonth(), 1);
        dateFin = new Date(now.getFullYear(), now.getMonth() + 1, 0);
        break;
      case PeriodeObjectif.Trimestriel:
        const q = Math.floor(now.getMonth() / 3);
        dateDebut = new Date(now.getFullYear(), q * 3, 1);
        dateFin = new Date(now.getFullYear(), q * 3 + 3, 0);
        break;
      case PeriodeObjectif.Annuel:
        dateDebut = new Date(now.getFullYear(), 0, 1);
        dateFin = new Date(now.getFullYear(), 11, 31);
        break;
      default:
        return;
    }

    this.form.patchValue({
      dateDebut: dateDebut.toISOString().slice(0, 10),
      dateFin: dateFin.toISOString().slice(0, 10)
    }, { emitEvent: false });
    this.cdr.markForCheck();
  }

  userName(u: any): string {
    return this.userSvc.displayName(u, this.userSvc.userId(u) ?? undefined);
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
      id_User_Delegue: +this.form.value.id_User_Delegue,
      type: +this.form.value.type,
      periode: +this.form.value.periode,
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
