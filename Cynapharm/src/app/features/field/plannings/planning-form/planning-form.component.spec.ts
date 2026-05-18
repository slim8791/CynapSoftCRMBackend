import { vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { PlanningFormComponent } from './planning-form.component';
import { PlanningService } from '../services/planning.service';
import { Router, ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { EtatPlanning } from '../../../../core/models/enums/index';

describe('PlanningFormComponent', () => {
  let component: PlanningFormComponent;
  let svcMock: { getById: ReturnType<typeof vi.fn>; createOrUpdate: ReturnType<typeof vi.fn> };
  let routerMock: { navigate: ReturnType<typeof vi.fn> };

  async function setup(id: string | null = null) {
    svcMock = { getById: vi.fn().mockReturnValue(of(null)), createOrUpdate: vi.fn() };
    routerMock = { navigate: vi.fn() };
    if (id) {
      svcMock.getById.mockReturnValue(of({ idPlanning: Number(id), id_User_Delegue: 1, date: '2024-01-01T00:00:00', heureDebut: '09:00', heureFin: '10:00', etatPlanning: EtatPlanning.EnAttente }));
    }
    await TestBed.configureTestingModule({
      imports: [PlanningFormComponent, ReactiveFormsModule],
      providers: [
        { provide: PlanningService, useValue: svcMock },
        { provide: Router, useValue: routerMock },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => id } } } }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
    const fixture = TestBed.createComponent(PlanningFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  it('should create in create mode', async () => {
    await setup(null);
    expect(component.isEdit).toBe(false);
  });

  it('should enter edit mode and load data when id provided', async () => {
    await setup('2');
    expect(component.isEdit).toBe(true);
    expect(svcMock.getById).toHaveBeenCalledWith(2);
  });

  it('submit should not call service when form is invalid', async () => {
    await setup(null);
    component.submit();
    expect(svcMock.createOrUpdate).not.toHaveBeenCalled();
  });

  it('submit should call createOrUpdate and set successMsg on success', async () => {
    await setup(null);
    svcMock.createOrUpdate.mockReturnValue(of({} as any));
    component.form.patchValue({ id_User_Delegue: 1, date: '2024-01-01', heureDebut: '', heureFin: '', etatPlanning: EtatPlanning.EnAttente });
    component.submit();
    expect(svcMock.createOrUpdate).toHaveBeenCalled();
    expect(component.successMsg).toBe('Planning créé.');
  });

  it('submit should set submitError on failure', async () => {
    await setup(null);
    svcMock.createOrUpdate.mockReturnValue(throwError(() => new Error()));
    component.form.patchValue({ id_User_Delegue: 1, date: '2024-01-01', heureDebut: '', heureFin: '', etatPlanning: EtatPlanning.EnAttente });
    component.submit();
    expect(component.submitError).toBe("Erreur lors de l'enregistrement.");
  });
});
