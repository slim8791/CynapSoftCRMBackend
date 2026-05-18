import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { RegionService } from './region.service';
import { ApiService } from '../../../../core/services/api.service';
import { of } from 'rxjs';

describe('RegionService', () => {
  let service: RegionService;
  let apiMock: { get: ReturnType<typeof vi.fn>; post: ReturnType<typeof vi.fn>; delete: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    apiMock = { get: vi.fn(), post: vi.fn(), delete: vi.fn() };
    TestBed.configureTestingModule({
      providers: [RegionService, { provide: ApiService, useValue: apiMock }]
    });
    service = TestBed.inject(RegionService);
  });

  it('getAll should call GET /fields/regions/all', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of([]));
    service.getAll().subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/fields/regions/all');
  }));

  it('getById should call GET /fields/regions/:id', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of({}));
    service.getById(1).subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/fields/regions/1');
  }));

  it('getByDelegue should call /fields/regions/by-delegue/:id', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of([]));
    service.getByDelegue(2).subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/fields/regions/by-delegue/2');
  }));

  it('getCount should call /fields/regions/count/:id and return number', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of({ Result: 5 }));
    service.getCount(1).subscribe(n => { expect(n).toBe(5); resolve(); });
  }));

  it('createOrUpdate should POST to /fields/regions', () => new Promise<void>((resolve) => {
    const dto = { nomRegion: 'Tunis', codePostal: '1000', id_User_Delegue: 1 };
    apiMock.post.mockReturnValue(of(dto));
    service.createOrUpdate(dto).subscribe(() => resolve());
    expect(apiMock.post).toHaveBeenCalledWith('/fields/regions', dto);
  }));

  it('delete should call DELETE /fields/regions/:id', () => new Promise<void>((resolve) => {
    apiMock.delete.mockReturnValue(of(undefined));
    service.delete(3).subscribe(() => resolve());
    expect(apiMock.delete).toHaveBeenCalledWith('/fields/regions/3');
  }));
});
