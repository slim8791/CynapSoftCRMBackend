import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { RapportService, RapportDto } from '../services/rapport.service';
import { UserService } from '../../../../features/users/user.service';
import { VisiteService, VisiteDto } from '../../../field/visites/services/visite.service';

@Component({
  selector: 'app-rapport-form',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
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

  delegues: any[] = [];
  visites: VisiteDto[] = [];
  loadingVisites = false;

  readonly resultats = ['POSITIF', 'NEGATIF', 'EN_ATTENTE'];

  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private svc: RapportService,
    private userSvc: UserService,
    private visiteSvc: VisiteService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.form = this.fb.group({
      id_User_Delegue: [null, [Validators.required]],
      id_Visite: [null, [Validators.required]],
      commentaire: ['', [Validators.required]],
      resultat: ['', [Validators.required]]
    });

    // Load delegues
    this.userSvc.getUsersByRole('DELEGUE').pipe(takeUntil(this.destroy$))
      .subscribe({ next: u => { this.delegues = u; this.cdr.markForCheck(); }, error: () => { } });

    // When delegue changes, reload visites
    this.form.get('id_User_Delegue')!.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(id => {
      this.form.patchValue({ id_Visite: null });
      this.visites = [];
      if (id) this.loadVisites(+id);
    });

    // Edit mode
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.isEdit = true;
      this.editId = id;
      this.loadingData = true;
      this.svc.getById(id).pipe(takeUntil(this.destroy$)).subscribe({
        next: data => {
          this.form.patchValue(data);
          // Load visites for the loaded delegue
          if (data.id_User_Delegue) this.loadVisites(data.id_User_Delegue);
          this.loadingData = false;
          this.cdr.markForCheck();
        },
        error: () => { this.fetchError = 'Impossible de charger le rapport.'; this.loadingData = false; this.cdr.markForCheck(); }
      });
    }
  }

  private loadVisites(delegueId: number): void {
    this.loadingVisites = true;
    this.visiteSvc.getByDelegue(delegueId).pipe(takeUntil(this.destroy$)).subscribe({
      next: v => { this.visites = v; this.loadingVisites = false; this.cdr.markForCheck(); },
      error: () => { this.loadingVisites = false; this.cdr.markForCheck(); }
    });
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

    const dto: RapportDto = {
      ...this.form.value,
      id_User_Delegue: +this.form.value.id_User_Delegue,
      id_Visite: +this.form.value.id_Visite,
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
