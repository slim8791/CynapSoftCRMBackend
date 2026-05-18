import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { FactureService } from './facture.service';
import { ApiService } from '../../../../core/services/api.service';
import { of } from 'rxjs';

describe('FactureService', () => {
  let service: FactureService;
  let apiMock: { get: ReturnType<typeof vi.fn>; post: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    apiMock = { get: vi.fn(), post: vi.fn() };
    TestBed.configureTestingModule({
      providers: [FactureService, { provide: ApiService, useValue: apiMock }]
    });
    service = TestBed.inject(FactureService);
  });

  it('getAll should call GET /documents/factures with pagination', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of([]));
    service.getAll(1, 20).subscribe(() => resolve());
    expect(apiMock.get.mock.calls[0][0]).toBe('/documents/factures');
  }));

  it('getAll should unwrap Result', () => new Promise<void>((resolve) => {
    const items = [{ id: 1, id_Client: 1, montantHT: 100, montantTTC: 120, tva: 20 }];
    apiMock.get.mockReturnValue(of({ Result: items }));
    service.getAll().subscribe(data => {
      expect(data).toEqual(items);
      resolve();
    });
  }));

  it('getById should call /documents/factures/:id', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of({}));
    service.getById(3).subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/documents/factures/3');
  }));

  it('getByClient should call /documents/factures/client/:id', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of([]));
    service.getByClient(7).subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/documents/factures/client/7');
  }));

  it('createOrUpdate should POST to /documents/factures/createUpdate', () => new Promise<void>((resolve) => {
    const dto = { id_Client: 1, montantHT: 100, montantTTC: 120, tva: 20 };
    apiMock.post.mockReturnValue(of(dto));
    service.createOrUpdate(dto).subscribe(() => resolve());
    expect(apiMock.post).toHaveBeenCalledWith('/documents/factures/createUpdate', dto);
  }));
});
