import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { PlanningService, PlanningDto } from '../services/planning.service';
import { EtatPlanning, PLANNING_STATUS_LABELS } from '../../../../core/models/enums/index';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { UserService } from '../../../users/user.service';

@Component({
  selector: 'app-planning-form',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, EmptyStateComponent],
  templateUrl: './planning-form.component.html',
  styleUrls: ['./planning-form.component.css']
})
export class PlanningFormComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  isEdit = false;
  editId: number | null = null;
  loadingData = false;
  saving = false;
  fetchError = '';
  submitError = '';
  successMsg = '';
  delegues: any[] = [];

  etatOptions = [
    { value: EtatPlanning.EnAttente, label: PLANNING_STATUS_LABELS[EtatPlanning.EnAttente] },
    { value: EtatPlanning.Confirme, label: PLANNING_STATUS_LABELS[EtatPlanning.Confirme] },
    { value: EtatPlanning.Annule, label: PLANNING_STATUS_LABELS[EtatPlanning.Annule] }
  ];

  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private svc: PlanningService,
    private userSvc: UserService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.form = this.fb.group({
      id_User_Delegue: [null, [Validators.required]],
      date: ['', [Validators.required]],
      heureDebut: [''],
      heureFin: [''],
      etat: [EtatPlanning.EnAttente]
    });

    this.userSvc.getUsersByRole('DELEGUE').pipe(takeUntil(this.destroy$))
      .subscribe({ next: users => { this.delegues = users; this.cdr.markForCheck(); }, error: () => { } });

    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.isEdit = true;
      this.editId = id;
      this.loadingData = true;
      this.svc.getById(id).pipe(takeUntil(this.destroy$)).subscribe({
        next: data => {
          if (data) {
            this.form.patchValue({
              id_User_Delegue: data.id_User_Delegue,
              date: data.date?.substring(0, 10) ?? '',
              heureDebut: data.heureDebut?.substring(0, 5) ?? '',
              heureFin: data.heureFin?.substring(0, 5) ?? '',
              etat: data.etat ?? EtatPlanning.EnAttente
            });
          }
          this.loadingData = false;
          this.cdr.markForCheck();
        },
        error: () => { this.fetchError = 'Impossible de charger le planning.'; this.loadingData = false; this.cdr.markForCheck(); }
      });
    }
  }

  get f() { return this.form.controls; }

  userName(u: any): string {
    return this.userSvc.displayName(u, this.userSvc.userId(u) ?? undefined);
  }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    this.saving = true;
    this.submitError = '';
    this.successMsg = '';

    const v = this.form.value;
    const dto: PlanningDto = {
      ...v,
      id_User_Delegue: +v.id_User_Delegue,
      ...(this.isEdit && this.editId ? { idPlanning: this.editId } : {})
    };

    this.svc.createOrUpdate(dto).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.saving = false;
        this.successMsg = this.isEdit ? 'Planning mis à jour.' : 'Planning créé.';
        this.cdr.markForCheck();
        setTimeout(() => this.router.navigate(['/field/plannings']), 1200);
      },
      error: () => { this.submitError = 'Erreur lors de l\'enregistrement.'; this.saving = false; this.cdr.markForCheck(); }
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
