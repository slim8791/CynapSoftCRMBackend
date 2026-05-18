import { vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { VisiteFormComponent } from './visite-form.component';
import { VisiteService } from '../services/visite.service';
import { Router, ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { VisiteType } from '../../../../core/models/enums/index';

describe('VisiteFormComponent', () => {
  let component: VisiteFormComponent;
  let svcMock: { getById: ReturnType<typeof vi.fn>; createOrUpdate: ReturnType<typeof vi.fn> };
  let routerMock: { navigate: ReturnType<typeof vi.fn> };

  async function setup(id: string | null = null) {
    svcMock = { getById: vi.fn().mockReturnValue(of(null)), createOrUpdate: vi.fn() };
    routerMock = { navigate: vi.fn() };
    if (id) {
      svcMock.getById.mockReturnValue(of({ idVisite: Number(id), id_User_Delegue: 1, date: '2024-01-15T00:00:00', type: VisiteType.Medecin }));
    }
    await TestBed.configureTestingModule({
      imports: [VisiteFormComponent, ReactiveFormsModule],
      providers: [
        { provide: VisiteService, useValue: svcMock },
        { provide: Router, useValue: routerMock },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => id } } } }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
    const fixture = TestBed.createComponent(VisiteFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  it('should create in create mode', async () => {
    await setup(null);
    expect(component.isEdit).toBe(false);
  });

  it('should enter edit mode and load visite when id provided', async () => {
    await setup('4');
    expect(component.isEdit).toBe(true);
    expect(svcMock.getById).toHaveBeenCalledWith(4);
    expect(component.form.value.date).toBe('2024-01-15');
  });

  it('submit should not call service when form is invalid', async () => {
    await setup(null);
    component.submit();
    expect(svcMock.createOrUpdate).not.toHaveBeenCalled();
  });

  it('submit should createOrUpdate and set successMsg on success', async () => {
    await setup(null);
    svcMock.createOrUpdate.mockReturnValue(of({} as any));
    component.form.patchValue({ id_User_Delegue: 1, date: '2024-01-01', type: VisiteType.Medecin, id_Medecin: null, id_Pharmacien: null });
    component.submit();
    expect(svcMock.createOrUpdate).toHaveBeenCalled();
    expect(component.successMsg).toBe('Visite créée.');
  });

  it('submit should set submitError on failure', async () => {
    await setup(null);
    svcMock.createOrUpdate.mockReturnValue(throwError(() => new Error()));
    component.form.patchValue({ id_User_Delegue: 1, date: '2024-01-01', type: VisiteType.Pharmacien, id_Medecin: null, id_Pharmacien: null });
    component.submit();
    expect(component.submitError).toBe("Erreur lors de l'enregistrement.");
  });
});
