import { vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { RapportListComponent } from './rapport-list.component';
import { RapportService } from '../services/rapport.service';
import { ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { NO_ERRORS_SCHEMA } from '@angular/core';

const activatedRouteMock = { snapshot: { paramMap: { get: () => null } } };

describe('RapportListComponent', () => {
  let component: RapportListComponent;
  let svcMock: { getAll: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    svcMock = { getAll: vi.fn().mockReturnValue(of([])) };
    await TestBed.configureTestingModule({
      imports: [RapportListComponent],
      providers: [
        { provide: RapportService, useValue: svcMock },
        { provide: ActivatedRoute, useValue: activatedRouteMock }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
    const fixture = TestBed.createComponent(RapportListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create and load on init', () => {
    expect(component).toBeTruthy();
    expect(svcMock.getAll).toHaveBeenCalled();
  });

  it('load should populate rapports on success', () => {
    const items = [{ idRapport: 1, id_User_Delegue: 1, id_Visite: 1, commentaire: 'OK', resultat: 'Bon' }];
    svcMock.getAll.mockReturnValue(of(items));
    component.load();
    expect(component.rapports).toEqual(items);
    expect(component.loading).toBe(false);
  });

  it('load should set error on failure', () => {
    svcMock.getAll.mockReturnValue(throwError(() => new Error()));
    component.load();
    expect(component.error).toBe('Impossible de charger les rapports.');
  });
});
