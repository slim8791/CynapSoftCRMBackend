import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, of } from 'rxjs';
import { takeUntil, catchError, map } from 'rxjs/operators';
import { RegionService, RegionDto } from '../services/region.service';
import { UserService } from '../../../../features/users/user.service';

@Component({
  selector: 'app-region-form',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './region-form.component.html',
  styleUrls: ['./region-form.component.css']
})
export class RegionFormComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  isEdit      = false;
  editId: number | null = null;
  loadingData = false;
  saving      = false;
  fetchError  = '';
  submitError = '';
  successMsg  = '';

  superviseurs: any[] = [];

  private destroy$ = new Subject<void>();

  constructor(
    private fb:      FormBuilder,
    private route:   ActivatedRoute,
    private router:  Router,
    private svc:     RegionService,
    private userSvc: UserService,
    private cdr:     ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      nomRegion:       ['', [Validators.required]],
      codePostal:      ['', [Validators.required, Validators.pattern(/^\d{4,}$/)]],
      id_Superviseur: [null]
    });

    this.userSvc.getUsersByRole('SUPERVISEUR').pipe(takeUntil(this.destroy$))
      .subscribe({ next: u => { this.superviseurs = u; this.cdr.markForCheck(); }, error: () => {} });

    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.isEdit = true;
      this.editId = id;
      this.loadingData = true;
      this.svc.getById(id).pipe(takeUntil(this.destroy$)).subscribe({
        next: data => { this.form.patchValue(data); this.loadingData = false; this.cdr.markForCheck(); },
        error: () => { this.fetchError = 'Impossible de charger la région.'; this.loadingData = false; this.cdr.markForCheck(); }
      });
    }
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
    this.successMsg  = '';

    const dto: RegionDto = {
      ...this.form.value,
      id_Superviseur: this.form.value.id_Superviseur ? +this.form.value.id_Superviseur : undefined,
      ...(this.isEdit && this.editId ? { id_Region: this.editId } : {})
    };

    this.svc.createOrUpdate(dto).pipe(takeUntil(this.destroy$)).subscribe({
      next: (savedRegion) => {
        this.saving = false;
        this.successMsg = this.isEdit ? 'Région mise à jour.' : 'Région créée.';
        this.cdr.markForCheck();

        // Sync IdRegion to the assigned superviseur in AuthAPI
        const supId    = dto.id_Superviseur;
        const regionId = (savedRegion as any)?.Id_Region ?? savedRegion?.id_Region;
        if (supId && regionId) {
          this.userSvc.getUserById(supId).pipe(
            map(r => r?.Result ?? r?.result ?? r),
            catchError(() => of(null)),
            takeUntil(this.destroy$)
          ).subscribe(user => {
            if (user?.email) {
              this.userSvc.updateProfile({ email: user.email, idRegion: regionId })
                .pipe(catchError(() => of(null)))
                .subscribe();
            }
          });
        }

        setTimeout(() => this.router.navigate(['/field/regions']), 1200);
      },
      error: () => { this.submitError = 'Erreur lors de l\'enregistrement.'; this.saving = false; this.cdr.markForCheck(); }
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
