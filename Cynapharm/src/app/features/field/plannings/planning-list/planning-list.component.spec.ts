import { vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { PlanningListComponent } from './planning-list.component';
import { PlanningService } from '../services/planning.service';
import { ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { EtatPlanning } from '../../../../core/models/enums/index';

const activatedRouteMock = { snapshot: { paramMap: { get: () => null } } };

describe('PlanningListComponent', () => {
  let component: PlanningListComponent;
  let svcMock: { getByDelegue: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    svcMock = { getByDelegue: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [PlanningListComponent],
      providers: [
        { provide: PlanningService, useValue: svcMock },
        { provide: ActivatedRoute, useValue: activatedRouteMock }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
    const fixture = TestBed.createComponent(PlanningListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('load should do nothing when delegueId is null', () => {
    component.delegueId = null;
    component.load();
    expect(svcMock.getByDelegue).not.toHaveBeenCalled();
  });

  it('load should call getByDelegue and populate plannings on success', () => {
    const items = [{ idPlanning: 1, id_User_Delegue: 1, date: '2024-01-01', heureDebut: '09:00', heureFin: '10:00', etatPlanning: EtatPlanning.EnAttente }];
    svcMock.getByDelegue.mockReturnValue(of(items));
    component.delegueId = 1;
    component.load();
    expect(component.plannings).toEqual(items);
    expect(component.searched).toBe(true);
    expect(component.loading).toBe(false);
  });

  it('load should set error on failure', () => {
    svcMock.getByDelegue.mockReturnValue(throwError(() => new Error()));
    component.delegueId = 1;
    component.load();
    expect(component.error).toBe('Impossible de charger les plannings.');
  });

  it('statusLabel should return readable label for EtatPlanning', () => {
    expect(component.statusLabel(EtatPlanning.EnAttente)).toBe('En attente');
    expect(component.statusLabel(EtatPlanning.Confirme)).toBe('Confirmé');
    expect(component.statusLabel(EtatPlanning.Annule)).toBe('Annulé');
  });
});
