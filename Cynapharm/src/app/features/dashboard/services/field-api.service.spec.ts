import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { FieldApiService } from './field-api.service';
import { ApiService } from '../../../core/services/api.service';
import { of } from 'rxjs';

describe('FieldApiService', () => {
  let service: FieldApiService;
  let apiMock: { get: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    apiMock = { get: vi.fn() };
    TestBed.configureTestingModule({
      providers: [
        FieldApiService,
        { provide: ApiService, useValue: apiMock }
      ]
    });
    service = TestBed.inject(FieldApiService);
  });

  it('getVisitesCount should call correct endpoint without date', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of([{ idDelegue: 1, count: 5 }]));
    service.getVisitesCount().subscribe(data => {
      expect(data).toEqual([{ idDelegue: 1, count: 5 }]);
      resolve();
    });
    expect(apiMock.get).toHaveBeenCalledWith('/fields/kpi/visites-count');
  }));

  it('getVisitesCount should include date param when provided', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of([]));
    service.getVisitesCount('2024-01-01').subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/fields/kpi/visites-count?date=2024-01-01');
  }));

  it('getVisitesCount should return empty array when response is not array', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of(null));
    service.getVisitesCount().subscribe(data => {
      expect(data).toEqual([]);
      resolve();
    });
  }));

  it('getPerformance should call /fields/kpi/performance', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of({ Result: [] }));
    service.getPerformance().subscribe(data => {
      expect(data).toEqual([]);
      resolve();
    });
    expect(apiMock.get).toHaveBeenCalledWith('/fields/kpi/performance');
  }));

  it('getPerformanceRate should call /fields/kpi/performance-rate and return number', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of({ Result: 75 }));
    service.getPerformanceRate().subscribe(r => {
      expect(r).toBe(75);
      resolve();
    });
  }));

  it('getRegions should call /fields/regions and return array', () => new Promise<void>((resolve) => {
    const regions = [{ id_Region: 1, nomRegion: 'Tunis', codePostal: '1000', id_User_Delegue: 2 }];
    apiMock.get.mockReturnValue(of(regions));
    service.getRegions().subscribe(data => {
      expect(data).toEqual(regions);
      resolve();
    });
  }));

  it('getHistoriqueVisites should call /fields/kpi/historique', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of({ result: [1, 2] }));
    service.getHistoriqueVisites().subscribe(data => {
      expect(data).toEqual([1, 2]);
      resolve();
    });
  }));
});
