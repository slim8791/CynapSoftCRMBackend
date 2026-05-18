import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { VisiteService } from './visite.service';
import { ApiService } from '../../../../core/services/api.service';
import { VisiteType } from '../../../../core/models/enums';
import { of } from 'rxjs';

describe('VisiteService', () => {
  let service: VisiteService;
  let apiMock: { get: ReturnType<typeof vi.fn>; post: ReturnType<typeof vi.fn>; put: ReturnType<typeof vi.fn>; delete: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    apiMock = { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() };
    TestBed.configureTestingModule({
      providers: [VisiteService, { provide: ApiService, useValue: apiMock }]
    });
    service = TestBed.inject(VisiteService);
  });

  it('getById should call GET /fields/visites/:id', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of({}));
    service.getById(1).subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/fields/visites/1');
  }));

  it('getByDelegue should call /fields/visites/by-delegue/:id', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of([]));
    service.getByDelegue(2).subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/fields/visites/by-delegue/2');
  }));

  it('getByPlanning should call /fields/visites/by-planning/:id', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of([]));
    service.getByPlanning(3).subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/fields/visites/by-planning/3');
  }));

  it('createOrUpdate should POST to /fields/visites', () => new Promise<void>((resolve) => {
    const dto = { id_User_Delegue: 1, date: '2024-01-01', type: VisiteType.Medecin };
    apiMock.post.mockReturnValue(of(dto));
    service.createOrUpdate(dto).subscribe(() => resolve());
    expect(apiMock.post).toHaveBeenCalledWith('/fields/visites', dto);
  }));

  it('affectToPlanning should PUT to /fields/visites/:id/planning/:planId', () => new Promise<void>((resolve) => {
    apiMock.put.mockReturnValue(of(undefined));
    service.affectToPlanning(1, 2).subscribe(() => resolve());
    expect(apiMock.put).toHaveBeenCalledWith('/fields/visites/1/planning/2', {});
  }));

  it('complete should PUT to /fields/visites/:id/complete', () => new Promise<void>((resolve) => {
    apiMock.put.mockReturnValue(of(undefined));
    service.complete(5).subscribe(() => resolve());
    expect(apiMock.put).toHaveBeenCalledWith('/fields/visites/5/complete', {});
  }));

  it('delete should call DELETE /fields/visites/:id', () => new Promise<void>((resolve) => {
    apiMock.delete.mockReturnValue(of(undefined));
    service.delete(4).subscribe(() => resolve());
    expect(apiMock.delete).toHaveBeenCalledWith('/fields/visites/4');
  }));
});
