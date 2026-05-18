import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { PlanningService } from './planning.service';
import { ApiService } from '../../../../core/services/api.service';
import { EtatPlanning } from '../../../../core/models/enums';
import { of } from 'rxjs';

describe('PlanningService', () => {
  let service: PlanningService;
  let apiMock: { get: ReturnType<typeof vi.fn>; post: ReturnType<typeof vi.fn>; put: ReturnType<typeof vi.fn>; delete: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    apiMock = { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() };
    TestBed.configureTestingModule({
      providers: [PlanningService, { provide: ApiService, useValue: apiMock }]
    });
    service = TestBed.inject(PlanningService);
  });

  it('getById should call GET /fields/plannings/:id', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of({}));
    service.getById(1).subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/fields/plannings/1');
  }));

  it('getByDelegue should call GET /fields/plannings/by-delegue/:id', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of([]));
    service.getByDelegue(2).subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/fields/plannings/by-delegue/2');
  }));

  it('getByRange should call GET /fields/plannings/by-range with params', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of([]));
    service.getByRange(1, '2024-01-01', '2024-01-31').subscribe(() => resolve());
    expect(apiMock.get.mock.calls[0][0]).toBe('/fields/plannings/by-range');
  }));

  it('createOrUpdate should POST to /fields/plannings', () => new Promise<void>((resolve) => {
    const dto = { id_User_Delegue: 1, date: '2024-01-01', heureDebut: '09:00', heureFin: '10:00', etatPlanning: EtatPlanning.EnAttente };
    apiMock.post.mockReturnValue(of(dto));
    service.createOrUpdate(dto).subscribe(() => resolve());
    expect(apiMock.post).toHaveBeenCalledWith('/fields/plannings', dto);
  }));

  it('validate should PUT to /fields/plannings/:id/validate', () => new Promise<void>((resolve) => {
    apiMock.put.mockReturnValue(of(undefined));
    service.validate(3).subscribe(() => resolve());
    expect(apiMock.put).toHaveBeenCalledWith('/fields/plannings/3/validate', {});
  }));

  it('delete should call DELETE /fields/plannings/:id', () => new Promise<void>((resolve) => {
    apiMock.delete.mockReturnValue(of(undefined));
    service.delete(4).subscribe(() => resolve());
    expect(apiMock.delete).toHaveBeenCalledWith('/fields/plannings/4');
  }));
});
