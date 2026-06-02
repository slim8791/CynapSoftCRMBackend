import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { DocumentService } from './document.service';
import { ApiService } from '../../../../core/services/api.service';
import { of } from 'rxjs';

describe('DocumentService', () => {
  let service: DocumentService;
  let apiMock: { get: ReturnType<typeof vi.fn>; post: ReturnType<typeof vi.fn>; delete: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    apiMock = { get: vi.fn(), post: vi.fn(), delete: vi.fn() };
    TestBed.configureTestingModule({
      providers: [DocumentService, { provide: ApiService, useValue: apiMock }]
    });
    service = TestBed.inject(DocumentService);
  });

  it('getAll should call GET /documents with pagination', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of([]));
    service.getAll(1, 10).subscribe(() => resolve());
    expect(apiMock.get.mock.calls[0][0]).toBe('/documents');
  }));

  it('getAll should unwrap Result', () => new Promise<void>((resolve) => {
    const items = [{ type: 'Facture', id_Client: 1 }];
    apiMock.get.mockReturnValue(of({ Result: items }));
    service.getAll().subscribe(data => {
      expect(data).toEqual(items);
      resolve();
    });
  }));

  it('getById should call /documents/:numero', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of({}));
    service.getById('DOC001').subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/documents/DOC001');
  }));

  it('getByClient should call /documents/client/:id', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of([]));
    service.getByClient(3).subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/documents/client/3');
  }));

  it('getByCommande should call /documents/commande/:id', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of([]));
    service.getByCommande(9).subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/documents/commande/9');
  }));

  it('createOrUpdate should POST to /documents/document', () => new Promise<void>((resolve) => {
    const dto = { type: 'Facture', id_Client: 1 };
    apiMock.post.mockReturnValue(of(dto));
    service.createOrUpdate(dto).subscribe(() => resolve());
    expect(apiMock.post).toHaveBeenCalledWith('/documents/document', dto);
  }));

  it('delete should call DELETE /documents/:numero', () => new Promise<void>((resolve) => {
    apiMock.delete.mockReturnValue(of(undefined));
    service.delete('DOC001').subscribe(() => resolve());
    expect(apiMock.delete).toHaveBeenCalledWith('/documents/DOC001');
  }));
});
