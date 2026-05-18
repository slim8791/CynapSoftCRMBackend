import { vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { BonCommandeListComponent } from './bon-commande-list.component';
import { BonCommandeService } from '../services/bon-commande.service';
import { ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { NO_ERRORS_SCHEMA } from '@angular/core';

const activatedRouteMock = { snapshot: { paramMap: { get: () => null } } };

describe('BonCommandeListComponent', () => {
  let component: BonCommandeListComponent;
  let svcMock: { getAll: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    svcMock = { getAll: vi.fn().mockReturnValue(of([])) };
    await TestBed.configureTestingModule({
      imports: [BonCommandeListComponent],
      providers: [
        { provide: BonCommandeService, useValue: svcMock },
        { provide: ActivatedRoute, useValue: activatedRouteMock }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
    const fixture = TestBed.createComponent(BonCommandeListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create and load on init', () => {
    expect(component).toBeTruthy();
    expect(svcMock.getAll).toHaveBeenCalled();
  });

  it('load should set bons and total on success', () => {
    const items = [{ id: 1, id_Client: 2, montantTotal: 100 }];
    svcMock.getAll.mockReturnValue(of(items));
    component.load();
    expect(component.bons).toEqual(items);
    expect(component.total).toBe(1);
    expect(component.loading).toBe(false);
  });

  it('load should set error message on failure', () => {
    svcMock.getAll.mockReturnValue(throwError(() => new Error('fail')));
    component.load();
    expect(component.error).toBe('Erreur chargement.');
    expect(component.loading).toBe(false);
  });

  it('onPage should update page and reload', () => {
    svcMock.getAll.mockReturnValue(of([]));
    component.onPage(3);
    expect(component.page).toBe(3);
    expect(svcMock.getAll).toHaveBeenCalledWith(3, component.pageSize);
  });
});
