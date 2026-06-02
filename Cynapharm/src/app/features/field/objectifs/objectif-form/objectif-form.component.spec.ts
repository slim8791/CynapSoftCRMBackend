import { vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { ObjectifFormComponent } from './objectif-form.component';
import { ObjectifService } from '../services/objectif.service';
import { Router, ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { TypeObjectif, PeriodeObjectif } from '../../../../core/models/enums/index';

describe('ObjectifFormComponent', () => {
  let component: ObjectifFormComponent;
  let svcMock: { getById: ReturnType<typeof vi.fn>; createOrUpdate: ReturnType<typeof vi.fn> };
  let routerMock: { navigate: ReturnType<typeof vi.fn> };

  async function setup(id: string | null = null) {
    svcMock = { getById: vi.fn().mockReturnValue(of(null)), createOrUpdate: vi.fn() };
    routerMock = { navigate: vi.fn() };
    if (id) {
      svcMock.getById.mockReturnValue(of({
        idObjectif: Number(id), id_User_Delegue: 1, type: TypeObjectif.Visites,
        periode: PeriodeObjectif.Mensuel, valeurCible: 10, valeurRealisee: 0,
        dateDebut: '2024-01-01T00:00:00', dateFin: '2024-01-31T00:00:00'
      }));
    }
    await TestBed.configureTestingModule({
      imports: [ObjectifFormComponent, ReactiveFormsModule],
      providers: [
        { provide: ObjectifService, useValue: svcMock },
        { provide: Router, useValue: routerMock },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => id } } } }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
    const fixture = TestBed.createComponent(ObjectifFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  it('should create in create mode when no id', async () => {
    await setup(null);
    expect(component).toBeTruthy();
    expect(component.isEdit).toBe(false);
  });

  it('should enter edit mode and load objectif when id is in route', async () => {
    await setup('5');
    expect(component.isEdit).toBe(true);
    expect(svcMock.getById).toHaveBeenCalledWith(5);
  });

  it('submit should not call service when form is invalid', async () => {
    await setup(null);
    component.submit();
    expect(svcMock.createOrUpdate).not.toHaveBeenCalled();
  });

  it('submit should call createOrUpdate on success', async () => {
    await setup(null);
    svcMock.createOrUpdate.mockReturnValue(of({} as any));
    component.form.patchValue({
      id_User_Delegue: 1, type: TypeObjectif.Visites,
      periode: PeriodeObjectif.Mensuel, valeurCible: 10,
      dateDebut: '2024-01-01', dateFin: '2024-01-31'
    });
    component.submit();
    expect(svcMock.createOrUpdate).toHaveBeenCalled();
    expect(component.successMsg).toBe('Objectif créé.');
  });

  it('submit should set submitError on failure', async () => {
    await setup(null);
    svcMock.createOrUpdate.mockReturnValue(throwError(() => new Error()));
    component.form.patchValue({
      id_User_Delegue: 1, type: TypeObjectif.Visites,
      periode: PeriodeObjectif.Mensuel, valeurCible: 10,
      dateDebut: '2024-01-01', dateFin: '2024-01-31'
    });
    component.submit();
    expect(component.submitError).toBe("Erreur lors de l'enregistrement.");
    expect(component.saving).toBe(false);
  });
});
