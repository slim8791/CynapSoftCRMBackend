import { vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { ObjectifListComponent } from './objectif-list.component';
import { ObjectifService } from '../services/objectif.service';
import { ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { NO_ERRORS_SCHEMA } from '@angular/core';

const activatedRouteMock = { snapshot: { paramMap: { get: () => null } } };

describe('ObjectifListComponent', () => {
  let component: ObjectifListComponent;
  let svcMock: { getAll: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    svcMock = { getAll: vi.fn().mockReturnValue(of([])) };
    await TestBed.configureTestingModule({
      imports: [ObjectifListComponent],
      providers: [
        { provide: ObjectifService, useValue: svcMock },
        { provide: ActivatedRoute, useValue: activatedRouteMock }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
    const fixture = TestBed.createComponent(ObjectifListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create and load on init', () => {
    expect(component).toBeTruthy();
    expect(svcMock.getAll).toHaveBeenCalled();
  });

  it('load should populate objectifs on success', () => {
    const items = [{ idObjectif: 1, id_User_Delegue: 1, type: 'Visites' as any, periode: 1 as any, valeurCible: 10, valeurRealisee: 5, dateDebut: '', dateFin: '' }];
    svcMock.getAll.mockReturnValue(of(items));
    component.load();
    expect(component.objectifs).toEqual(items);
    expect(component.loading).toBe(false);
  });

  it('load should set error on failure', () => {
    svcMock.getAll.mockReturnValue(throwError(() => new Error()));
    component.load();
    expect(component.error).toBe('Impossible de charger les objectifs.');
  });

  it('progressPct should return 0 when cible is 0', () => {
    expect(component.progressPct(5, 0)).toBe(0);
  });

  it('progressPct should return correct percentage', () => {
    expect(component.progressPct(8, 10)).toBe(80);
  });

  it('progressPct should cap at 100', () => {
    expect(component.progressPct(15, 10)).toBe(100);
  });
});
