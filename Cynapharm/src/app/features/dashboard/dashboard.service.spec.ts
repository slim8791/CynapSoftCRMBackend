import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { DashboardService } from './dashboard.service';
import { ApiService } from '../../core/services/api.service';
import { of } from 'rxjs';

describe('DashboardService', () => {
  let service: DashboardService;
  let apiMock: { get: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    apiMock = { get: vi.fn() };
    TestBed.configureTestingModule({
      providers: [
        DashboardService,
        { provide: ApiService, useValue: apiMock }
      ]
    });
    service = TestBed.inject(DashboardService);
  });

  it('getDashboardData should call apiService.get with /dashboard', () => {
    apiMock.get.mockReturnValue(of({}));
    service.getDashboardData().subscribe();
    expect(apiMock.get).toHaveBeenCalledWith('/dashboard');
  });

  it('getMetrics should call apiService.get with /dashboard/metrics', () => {
    apiMock.get.mockReturnValue(of({}));
    service.getMetrics().subscribe();
    expect(apiMock.get).toHaveBeenCalledWith('/dashboard/metrics');
  });

  it('getRecentActivity should call apiService.get with /dashboard/recent-activity', () => {
    apiMock.get.mockReturnValue(of({}));
    service.getRecentActivity().subscribe();
    expect(apiMock.get).toHaveBeenCalledWith('/dashboard/recent-activity');
  });
});
