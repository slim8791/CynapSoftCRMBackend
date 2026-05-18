import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { RapportService } from './rapport.service';
import { ApiService } from '../../../../core/services/api.service';
import { of } from 'rxjs';

describe('RapportService', () => {
  let service: RapportService;
  let apiMock: { get: ReturnType<typeof vi.fn>; post: ReturnType<typeof vi.fn>; put: ReturnType<typeof vi.fn>; delete: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    apiMock = { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() };
    TestBed.configureTestingModule({
      providers: [RapportService, { provide: ApiService, useValue: apiMock }]
    });
    service = TestBed.inject(RapportService);
  });

  it('getAll should call GET /fields/rapports/all', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of([]));
    service.getAll().subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/fields/rapports/all');
  }));

  it('getById should call GET /fields/rapports/:id', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of({}));
    service.getById(1).subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/fields/rapports/1');
  }));

  it('getByVisite should call /fields/rapports/by-visite/:id', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of([]));
    service.getByVisite(3).subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/fields/rapports/by-visite/3');
  }));

  it('canCreate should call /fields/rapports/can-create/:idVisite and return boolean', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of({ Result: true }));
    service.canCreate(2).subscribe(r => { expect(r).toBe(true); resolve(); });
  }));

  it('createOrUpdate should POST to /fields/rapports/createUpdate', () => new Promise<void>((resolve) => {
    const dto = { id_User_Delegue: 1, id_Visite: 1, commentaire: 'OK', resultat: 'Bon' };
    apiMock.post.mockReturnValue(of(dto));
    service.createOrUpdate(dto).subscribe(() => resolve());
    expect(apiMock.post).toHaveBeenCalledWith('/fields/rapports/createUpdate', dto);
  }));

  it('validate should PUT to /fields/rapports/:id/validate with superviseur query', () => new Promise<void>((resolve) => {
    apiMock.put.mockReturnValue(of(undefined));
    service.validate(1, 5).subscribe(() => resolve());
    expect(apiMock.put).toHaveBeenCalledWith('/fields/rapports/1/validate?idSuperviseur=5', {});
  }));

  it('delete should call DELETE /fields/rapports/:id', () => new Promise<void>((resolve) => {
    apiMock.delete.mockReturnValue(of(undefined));
    service.delete(2).subscribe(() => resolve());
    expect(apiMock.delete).toHaveBeenCalledWith('/fields/rapports/2');
  }));
});
