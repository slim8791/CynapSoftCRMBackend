import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { BonLivraisonService } from './bon-livraison.service';
import { ApiService } from '../../../../core/services/api.service';
import { of } from 'rxjs';

describe('BonLivraisonService', () => {
  let service: BonLivraisonService;
  let apiMock: { get: ReturnType<typeof vi.fn>; post: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    apiMock = { get: vi.fn(), post: vi.fn() };
    TestBed.configureTestingModule({
      providers: [BonLivraisonService, { provide: ApiService, useValue: apiMock }]
    });
    service = TestBed.inject(BonLivraisonService);
  });

  it('getAll should call GET /documents/bons-livraison with pagination', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of([]));
    service.getAll(1, 20).subscribe(() => resolve());
    expect(apiMock.get.mock.calls[0][0]).toBe('/documents/bons-livraison');
  }));

  it('getAll should unwrap Result from response', () => new Promise<void>((resolve) => {
    const items = [{ id: 1, id_Client: 2, adresseLivraison: 'Addr' }];
    apiMock.get.mockReturnValue(of({ Result: items }));
    service.getAll().subscribe(data => {
      expect(data).toEqual(items);
      resolve();
    });
  }));

  it('getById should call /documents/bons-livraison/:id', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of({}));
    service.getById(7).subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/documents/bons-livraison/7');
  }));

  it('getByClient should call /documents/bons-livraison/ByClient/:id', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of([]));
    service.getByClient(10).subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/documents/bons-livraison/ByClient/10');
  }));

  it('createOrUpdate should POST to /documents/bons-livraison/createUpdate', () => new Promise<void>((resolve) => {
    const dto = { id_Client: 1, adresseLivraison: 'Addr' };
    apiMock.post.mockReturnValue(of(dto));
    service.createOrUpdate(dto).subscribe(() => resolve());
    expect(apiMock.post).toHaveBeenCalledWith('/documents/bons-livraison/createUpdate', dto);
  }));
});
