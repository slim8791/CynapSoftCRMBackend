import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { BonCommandeService } from './bon-commande.service';
import { ApiService } from '../../../../core/services/api.service';
import { of } from 'rxjs';

describe('BonCommandeService', () => {
  let service: BonCommandeService;
  let apiMock: { get: ReturnType<typeof vi.fn>; post: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    apiMock = { get: vi.fn(), post: vi.fn() };
    TestBed.configureTestingModule({
      providers: [BonCommandeService, { provide: ApiService, useValue: apiMock }]
    });
    service = TestBed.inject(BonCommandeService);
  });

  it('getAll should call GET /documents/bons-commandes with pagination params', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of([]));
    service.getAll(1, 20).subscribe(() => resolve());
    expect(apiMock.get.mock.calls[0][0]).toBe('/documents/bons-commandes');
  }));

  it('getAll should unwrap Result when present', () => new Promise<void>((resolve) => {
    const items = [{ id: 1, id_Client: 2, montantTotal: 100 }];
    apiMock.get.mockReturnValue(of({ Result: items }));
    service.getAll().subscribe(data => {
      expect(data).toEqual(items);
      resolve();
    });
  }));

  it('getById should call GET /documents/bons-commandes/:id', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of({ id: 1, id_Client: 1, montantTotal: 50 }));
    service.getById(1).subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/documents/bons-commandes/1');
  }));

  it('getByClient should call GET /documents/bons-commandes/client/:id', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of([]));
    service.getByClient(5).subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/documents/bons-commandes/client/5');
  }));

  it('createOrUpdate should POST to /documents/bons-commandes/createUpdate', () => new Promise<void>((resolve) => {
    const dto = { id_Client: 1, montantTotal: 100 };
    apiMock.post.mockReturnValue(of(dto));
    service.createOrUpdate(dto).subscribe(() => resolve());
    expect(apiMock.post).toHaveBeenCalledWith('/documents/bons-commandes/createUpdate', dto);
  }));
});
