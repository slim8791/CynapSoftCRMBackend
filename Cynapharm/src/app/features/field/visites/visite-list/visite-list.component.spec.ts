import { vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { VisiteListComponent } from './visite-list.component';
import { VisiteService } from '../services/visite.service';
import { ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { VisiteType } from '../../../../core/models/enums/index';

const activatedRouteMock = { snapshot: { paramMap: { get: () => null } } };

describe('VisiteListComponent', () => {
  let component: VisiteListComponent;
  let svcMock: { getByDelegue: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    svcMock = { getByDelegue: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [VisiteListComponent],
      providers: [
        { provide: VisiteService, useValue: svcMock },
        { provide: ActivatedRoute, useValue: activatedRouteMock }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
    const fixture = TestBed.createComponent(VisiteListComponent);
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

  it('load should call getByDelegue and populate visites', () => {
    const items = [{ idVisite: 1, id_User_Delegue: 1, dateVisite: '2024-01-01', type: VisiteType.Medecin }];
    svcMock.getByDelegue.mockReturnValue(of(items));
    component.delegueId = 1;
    component.load();
    expect(component.visites).toEqual(items);
    expect(component.searched).toBe(true);
    expect(component.loading).toBe(false);
  });

  it('load should set error on failure', () => {
    svcMock.getByDelegue.mockReturnValue(throwError(() => new Error()));
    component.delegueId = 1;
    component.load();
    expect(component.error).toBe('Impossible de charger les visites.');
  });

  it('visiteTypeLabel should return correct label for each VisiteType', () => {
    expect(component.visiteTypeLabel(VisiteType.Medecin)).toBe('Médecin');
    expect(component.visiteTypeLabel(VisiteType.Pharmacien)).toBe('Pharmacien');
    expect(component.visiteTypeLabel(VisiteType.Autre)).toBe('Autre');
  });
});
