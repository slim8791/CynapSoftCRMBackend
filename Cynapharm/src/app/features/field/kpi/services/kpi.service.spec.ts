import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { KpiService } from './kpi.service';
import { ApiService } from '../../../../core/services/api.service';
import { of } from 'rxjs';

describe('KpiService', () => {
  let service: KpiService;
  let apiMock: { get: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    apiMock = { get: vi.fn() };
    TestBed.configureTestingModule({
      providers: [KpiService, { provide: ApiService, useValue: apiMock }]
    });
    service = TestBed.inject(KpiService);
  });

  it('getNombreVisites should call /fields/kpi/visites-count with idDelegue', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of(10));
    service.getNombreVisites(1).subscribe(() => resolve());
    expect(apiMock.get.mock.calls[0][0]).toBe('/fields/kpi/visites-count');
  }));

  it('getNombreVisites should include debut and fin params when provided', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of(5));
    service.getNombreVisites(1, '2024-01-01', '2024-01-31').subscribe(() => resolve());
    const params = apiMock.get.mock.calls[0][1];
    expect(params.get('debut')).toBe('2024-01-01');
    expect(params.get('fin')).toBe('2024-01-31');
  }));

  it('hasVisiteAtDate should call /fields/kpi/has-visite and unwrap boolean', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of({ Result: true }));
    service.hasVisiteAtDate(1, '2024-05-01').subscribe(r => {
      expect(r).toBe(true);
      resolve();
    });
  }));

  it('getHistorique should call /fields/kpi/historique/:id', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of([]));
    service.getHistorique(2).subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/fields/kpi/historique/2');
  }));

  it('getPerformance should call /fields/kpi/performance/:id', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of({}));
    service.getPerformance(3).subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/fields/kpi/performance/3');
  }));

  it('getPerformanceRate should call /fields/kpi/performance-rate/:id and return number', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of({ Result: 80 }));
    service.getPerformanceRate(1).subscribe(r => {
      expect(r).toBe(80);
      resolve();
    });
  }));

  it('getClientFidelite should call /fields/kpi/client-fidelite/:id', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of({}));
    service.getClientFidelite(5).subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/fields/kpi/client-fidelite/5');
  }));
});
