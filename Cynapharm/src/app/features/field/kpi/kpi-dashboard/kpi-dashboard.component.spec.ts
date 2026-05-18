import { vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { KpiDashboardComponent } from './kpi-dashboard.component';
import { KpiService } from '../services/kpi.service';
import { ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { NO_ERRORS_SCHEMA } from '@angular/core';

const activatedRouteMock = { snapshot: { paramMap: { get: () => null } } };

describe('KpiDashboardComponent', () => {
  let component: KpiDashboardComponent;
  let svcMock: {
    getNombreVisites: ReturnType<typeof vi.fn>;
    getPerformanceRate: ReturnType<typeof vi.fn>;
    getHistorique: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    svcMock = { getNombreVisites: vi.fn(), getPerformanceRate: vi.fn(), getHistorique: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [KpiDashboardComponent],
      providers: [
        { provide: KpiService, useValue: svcMock },
        { provide: ActivatedRoute, useValue: activatedRouteMock }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
    const fixture = TestBed.createComponent(KpiDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('load should do nothing when idDelegue is null', () => {
    component.idDelegue = null;
    component.load();
    expect(svcMock.getNombreVisites).not.toHaveBeenCalled();
  });

  it('load should call all kpi methods when idDelegue is set', () => {
    svcMock.getNombreVisites.mockReturnValue(of(5));
    svcMock.getPerformanceRate.mockReturnValue(of(75));
    svcMock.getHistorique.mockReturnValue(of([{ date: '2024-01-01' }]));
    component.idDelegue = 1;
    component.load();
    expect(svcMock.getNombreVisites).toHaveBeenCalledWith(1, undefined, undefined);
    expect(svcMock.getPerformanceRate).toHaveBeenCalledWith(1);
    expect(svcMock.getHistorique).toHaveBeenCalledWith(1);
    expect(component.visitesCount).toBe(5);
    expect(component.performanceRate).toBe(75);
    expect(component.historique.length).toBe(1);
    expect(component.loaded).toBe(true);
    expect(component.loading).toBe(false);
  });

  it('load should handle errors gracefully', () => {
    svcMock.getNombreVisites.mockReturnValue(throwError(() => new Error()));
    svcMock.getPerformanceRate.mockReturnValue(throwError(() => new Error()));
    svcMock.getHistorique.mockReturnValue(throwError(() => new Error()));
    component.idDelegue = 1;
    component.load();
    expect(component.visitesCount).toBe(0);
    expect(component.performanceRate).toBe(0);
    expect(component.historique).toEqual([]);
  });
});
