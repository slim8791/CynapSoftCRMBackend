import { vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { BonLivraisonListComponent } from './bon-livraison-list.component';
import { BonLivraisonService } from '../services/bon-livraison.service';
import { ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { NO_ERRORS_SCHEMA } from '@angular/core';

const activatedRouteMock = { snapshot: { paramMap: { get: () => null } } };

describe('BonLivraisonListComponent', () => {
  let component: BonLivraisonListComponent;
  let svcMock: { getAll: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    svcMock = { getAll: vi.fn().mockReturnValue(of([])) };
    await TestBed.configureTestingModule({
      imports: [BonLivraisonListComponent],
      providers: [
        { provide: BonLivraisonService, useValue: svcMock },
        { provide: ActivatedRoute, useValue: activatedRouteMock }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
    const fixture = TestBed.createComponent(BonLivraisonListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create and load on init', () => {
    expect(component).toBeTruthy();
    expect(svcMock.getAll).toHaveBeenCalled();
  });

  it('load should populate bons on success', () => {
    const items = [{ id: 1, id_Client: 2, adresseLivraison: 'Addr' }];
    svcMock.getAll.mockReturnValue(of(items));
    component.load();
    expect(component.bons).toEqual(items);
    expect(component.total).toBe(1);
    expect(component.loading).toBe(false);
  });

  it('load should set error on failure', () => {
    svcMock.getAll.mockReturnValue(throwError(() => new Error()));
    component.load();
    expect(component.error).toBe('Erreur chargement.');
    expect(component.loading).toBe(false);
  });

  it('onPage should update page and reload', () => {
    svcMock.getAll.mockReturnValue(of([]));
    component.onPage(2);
    expect(component.page).toBe(2);
    expect(svcMock.getAll).toHaveBeenCalledWith(2, component.pageSize);
  });
});
