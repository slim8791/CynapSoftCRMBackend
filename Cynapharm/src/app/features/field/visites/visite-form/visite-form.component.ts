import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { VisiteService, VisiteDto } from '../services/visite.service';
import { VisiteType } from '../../../../core/models/enums/index';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { UserService } from '../../../users/user.service';

@Component({
  selector: 'app-visite-form',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, EmptyStateComponent],
  templateUrl: './visite-form.component.html',
  styleUrls: ['./visite-form.component.css']
})
export class VisiteFormComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  isEdit = false;
  editId: number | null = null;
  loadingData = false;
  saving = false;
  fetchError = '';
  submitError = '';
  successMsg = '';
  delegues: any[] = [];
  medecins: any[] = [];
  pharmaciens: any[] = [];

  typeOptions = [
    { value: VisiteType.Medecin,    label: 'Médecin' },
    { value: VisiteType.Pharmacien, label: 'Pharmacien' },
    { value: VisiteType.Autre,      label: 'Autre' }
  ];

  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private svc: VisiteService,
    private userSvc: UserService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      id_User_Delegue: [null, [Validators.required]],
      date:            ['',   [Validators.required]],
      type:            ['',   [Validators.required]],
      id_Medecin:      [null],
      id_Pharmacien:   [null]
    });

    this.loadUsers();

    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.isEdit = true;
      this.editId = id;
      this.loadingData = true;
      this.svc.getById(id).pipe(takeUntil(this.destroy$)).subscribe({
        next: data => {
          this.form.patchValue({ ...data, date: data.date?.substring(0, 10) ?? '' });
          this.loadingData = false;
          this.cdr.markForCheck();
        },
        error: () => { this.fetchError = 'Impossible de charger la visite.'; this.loadingData = false; this.cdr.markForCheck(); }
      });
    }
  }

  get f() { return this.form.controls; }

  userName(u: any): string {
    return u?.name ?? u?.Name ?? u?.fullName ?? u?.FullName ?? u?.email ?? u?.Email ?? `#${u?.id ?? u?.Id}`;
  }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    this.saving = true;
    this.submitError = '';
    this.successMsg = '';

    const v = this.form.value;
    const dto: VisiteDto = {
      ...v,
      id_User_Delegue: +v.id_User_Delegue,
      id_Medecin: v.id_Medecin ? +v.id_Medecin : null,
      id_Pharmacien: v.id_Pharmacien ? +v.id_Pharmacien : null,
      ...(this.isEdit && this.editId ? { idVisite: this.editId } : {})
    };

    this.svc.createOrUpdate(dto).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.saving = false;
        this.successMsg = this.isEdit ? 'Visite mise à jour.' : 'Visite créée.';
        this.cdr.markForCheck();
        setTimeout(() => this.router.navigate(['/field/visites']), 1200);
      },
      error: () => { this.submitError = 'Erreur lors de l\'enregistrement.'; this.saving = false; this.cdr.markForCheck(); }
    });
  }

  private loadUsers(): void {
    this.userSvc.getUsersByRole('DELEGUE').pipe(takeUntil(this.destroy$))
      .subscribe({ next: users => { this.delegues = users; this.cdr.markForCheck(); }, error: () => {} });
    this.userSvc.getUsersByRole('MEDECIN').pipe(takeUntil(this.destroy$))
      .subscribe({ next: users => { this.medecins = users; this.cdr.markForCheck(); }, error: () => {} });
    this.userSvc.getUsersByRole('PHARMACIEN').pipe(takeUntil(this.destroy$))
      .subscribe({ next: users => { this.pharmaciens = users; this.cdr.markForCheck(); }, error: () => {} });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
