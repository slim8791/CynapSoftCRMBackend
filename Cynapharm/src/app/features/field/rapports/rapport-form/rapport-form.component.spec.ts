import { vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { RapportFormComponent } from './rapport-form.component';
import { RapportService } from '../services/rapport.service';
import { Router, ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('RapportFormComponent', () => {
  let component: RapportFormComponent;
  let svcMock: { getById: ReturnType<typeof vi.fn>; createOrUpdate: ReturnType<typeof vi.fn> };
  let routerMock: { navigate: ReturnType<typeof vi.fn> };

  async function setup(id: string | null = null) {
    svcMock = { getById: vi.fn().mockReturnValue(of(null)), createOrUpdate: vi.fn() };
    routerMock = { navigate: vi.fn() };
    if (id) {
      svcMock.getById.mockReturnValue(of({ idRapport: Number(id), id_User_Delegue: 1, id_Visite: 1, commentaire: 'C', resultat: 'R' }));
    }
    await TestBed.configureTestingModule({
      imports: [RapportFormComponent, ReactiveFormsModule],
      providers: [
        { provide: RapportService, useValue: svcMock },
        { provide: Router, useValue: routerMock },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => id } } } }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
    const fixture = TestBed.createComponent(RapportFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  it('should create in create mode', async () => {
    await setup(null);
    expect(component.isEdit).toBe(false);
  });

  it('should enter edit mode and load rapport when id provided', async () => {
    await setup('3');
    expect(component.isEdit).toBe(true);
    expect(svcMock.getById).toHaveBeenCalledWith(3);
  });

  it('submit should not call service when form is invalid', async () => {
    await setup(null);
    component.submit();
    expect(svcMock.createOrUpdate).not.toHaveBeenCalled();
  });

  it('submit should createOrUpdate and set successMsg on success', async () => {
    await setup(null);
    svcMock.createOrUpdate.mockReturnValue(of({} as any));
    component.form.patchValue({ id_Visite: 1, id_User_Delegue: 1, commentaire: 'Bon', resultat: 'OK' });
    component.submit();
    expect(svcMock.createOrUpdate).toHaveBeenCalled();
    expect(component.successMsg).toBe('Rapport créé.');
  });

  it('submit should set submitError on failure', async () => {
    await setup(null);
    svcMock.createOrUpdate.mockReturnValue(throwError(() => new Error()));
    component.form.patchValue({ id_Visite: 1, id_User_Delegue: 1, commentaire: 'C', resultat: 'R' });
    component.submit();
    expect(component.submitError).toBe("Erreur lors de l'enregistrement.");
  });
});
