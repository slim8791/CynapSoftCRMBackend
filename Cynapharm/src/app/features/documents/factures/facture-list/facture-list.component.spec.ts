import { vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { FactureListComponent } from './facture-list.component';
import { FactureService } from '../services/facture.service';
import { ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { NO_ERRORS_SCHEMA } from '@angular/core';

const activatedRouteMock = { snapshot: { paramMap: { get: () => null } } };

describe('FactureListComponent', () => {
  let component: FactureListComponent;
  let svcMock: { getAll: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    svcMock = { getAll: vi.fn().mockReturnValue(of([])) };
    await TestBed.configureTestingModule({
      imports: [FactureListComponent],
      providers: [
        { provide: FactureService, useValue: svcMock },
        { provide: ActivatedRoute, useValue: activatedRouteMock }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
    const fixture = TestBed.createComponent(FactureListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create and load on init', () => {
    expect(component).toBeTruthy();
    expect(svcMock.getAll).toHaveBeenCalled();
  });

  it('load should populate factures on success', () => {
    const items = [{ id: 1, id_Client: 1, montantHT: 100, montantTTC: 120, tva: 20 }];
    svcMock.getAll.mockReturnValue(of(items));
    component.load();
    expect(component.factures).toEqual(items);
    expect(component.total).toBe(1);
    expect(component.loading).toBe(false);
  });

  it('load should set error on failure', () => {
    svcMock.getAll.mockReturnValue(throwError(() => new Error()));
    component.load();
    expect(component.error).toBe('Erreur chargement.');
  });

  it('onPage should update page and reload', () => {
    svcMock.getAll.mockReturnValue(of([]));
    component.onPage(4);
    expect(component.page).toBe(4);
  });
});
