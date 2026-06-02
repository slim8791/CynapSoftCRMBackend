import { vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { RegionFormComponent } from './region-form.component';
import { RegionService } from '../services/region.service';
import { Router, ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('RegionFormComponent', () => {
  let component: RegionFormComponent;
  let svcMock: { getById: ReturnType<typeof vi.fn>; createOrUpdate: ReturnType<typeof vi.fn> };
  let routerMock: { navigate: ReturnType<typeof vi.fn> };

  async function setup(id: string | null = null) {
    svcMock = { getById: vi.fn().mockReturnValue(of(null)), createOrUpdate: vi.fn() };
    routerMock = { navigate: vi.fn() };
    if (id) {
      svcMock.getById.mockReturnValue(of({ id_Region: Number(id), nomRegion: 'Sousse', codePostal: '4000', id_User_Delegue: 1 }));
    }
    await TestBed.configureTestingModule({
      imports: [RegionFormComponent, ReactiveFormsModule],
      providers: [
        { provide: RegionService, useValue: svcMock },
        { provide: Router, useValue: routerMock },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => id } } } }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
    const fixture = TestBed.createComponent(RegionFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  it('should create in create mode', async () => {
    await setup(null);
    expect(component.isEdit).toBe(false);
  });

  it('should enter edit mode and load region when id provided', async () => {
    await setup('2');
    expect(component.isEdit).toBe(true);
    expect(svcMock.getById).toHaveBeenCalledWith(2);
  });

  it('submit should not call service when form is invalid', async () => {
    await setup(null);
    component.submit();
    expect(svcMock.createOrUpdate).not.toHaveBeenCalled();
  });

  it('submit should createOrUpdate and set successMsg on success', async () => {
    await setup(null);
    svcMock.createOrUpdate.mockReturnValue(of({} as any));
    component.form.patchValue({ nomRegion: 'Tunis', codePostal: '1000', id_User_Delegue: 1 });
    component.submit();
    expect(svcMock.createOrUpdate).toHaveBeenCalled();
    expect(component.successMsg).toBe('Région créée.');
  });

  it('submit should set submitError on failure', async () => {
    await setup(null);
    svcMock.createOrUpdate.mockReturnValue(throwError(() => new Error()));
    component.form.patchValue({ nomRegion: 'Tunis', codePostal: '1000', id_User_Delegue: 1 });
    component.submit();
    expect(component.submitError).toBe("Erreur lors de l'enregistrement.");
  });
});
