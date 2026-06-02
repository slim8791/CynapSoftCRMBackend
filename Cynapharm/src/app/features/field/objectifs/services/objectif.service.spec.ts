import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { ObjectifService } from './objectif.service';
import { ApiService } from '../../../../core/services/api.service';
import { TypeObjectif, PeriodeObjectif } from '../../../../core/models/enums';
import { of } from 'rxjs';

describe('ObjectifService', () => {
  let service: ObjectifService;
  let apiMock: { get: ReturnType<typeof vi.fn>; post: ReturnType<typeof vi.fn>; put: ReturnType<typeof vi.fn>; delete: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    apiMock = { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() };
    TestBed.configureTestingModule({
      providers: [ObjectifService, { provide: ApiService, useValue: apiMock }]
    });
    service = TestBed.inject(ObjectifService);
  });

  it('getAll should call GET /fields/objectifs', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of([]));
    service.getAll().subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/fields/objectifs');
  }));

  it('getById should call /fields/objectifs/:id', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of({}));
    service.getById(5).subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/fields/objectifs/5');
  }));

  it('getByDelegue should call /fields/objectifs/by-delegue/:id', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of([]));
    service.getByDelegue(2).subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/fields/objectifs/by-delegue/2');
  }));

  it('createOrUpdate should POST to /fields/objectifs', () => new Promise<void>((resolve) => {
    const dto = { id_User_Delegue: 1, type: TypeObjectif.Visites, periode: PeriodeObjectif.Mensuel, valeurCible: 10, valeurRealisee: 0, dateDebut: '2024-01-01', dateFin: '2024-01-31' };
    apiMock.post.mockReturnValue(of(dto));
    service.createOrUpdate(dto).subscribe(() => resolve());
    expect(apiMock.post).toHaveBeenCalledWith('/fields/objectifs', dto);
  }));

  it('updateValue should PUT to /fields/objectifs/:id/value with valeur param', () => new Promise<void>((resolve) => {
    apiMock.put.mockReturnValue(of(undefined));
    service.updateValue(3, 15).subscribe(() => resolve());
    expect(apiMock.put).toHaveBeenCalledWith('/fields/objectifs/3/value?nouvelleValeur=15', {});
  }));

  it('delete should call DELETE /fields/objectifs/:id', () => new Promise<void>((resolve) => {
    apiMock.delete.mockReturnValue(of(undefined));
    service.delete(4).subscribe(() => resolve());
    expect(apiMock.delete).toHaveBeenCalledWith('/fields/objectifs/4');
  }));
});
