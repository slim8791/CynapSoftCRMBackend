import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { OrderService, EtatCommande } from './order.service';
import { ApiService } from '../../core/services/api.service';
import { of } from 'rxjs';

describe('OrderService', () => {
  let service: OrderService;
  let apiMock: { get: ReturnType<typeof vi.fn>; post: ReturnType<typeof vi.fn>; put: ReturnType<typeof vi.fn>; delete: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    apiMock = { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() };
    TestBed.configureTestingModule({
      providers: [OrderService, { provide: ApiService, useValue: apiMock }]
    });
    service = TestBed.inject(OrderService);
  });

  it('getOrders should call GET /orders with page params', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of([]));
    service.getOrders(1, 20).subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/orders?page=1&pageSize=20');
  }));

  it('getOrders should normalize order fields from camelCase', () => new Promise<void>((resolve) => {
    const raw = [{ id_Commande: 1, dateCommande: '2024-01-01', montantTotalHT: 100, montantTTC: 120, statut: 'EnAttente', id_Client: 5, lignes: [] }];
    apiMock.get.mockReturnValue(of(raw));
    service.getOrders().subscribe(orders => {
      expect(orders[0].Id_Commande).toBe(1);
      expect(orders[0].Statut).toBe('EnAttente');
      resolve();
    });
  }));

  it('getOrderById should call GET /orders/:id and normalize', () => new Promise<void>((resolve) => {
    const raw = { Id_Commande: 2, DateCommande: '', MontantTotalHT: 0, MontantTTC: 0, Statut: 'Brouillon', Id_Client: 1, Lignes: [] };
    apiMock.get.mockReturnValue(of(raw));
    service.getOrderById(2).subscribe(order => {
      expect(order!.Id_Commande).toBe(2);
      resolve();
    });
    expect(apiMock.get).toHaveBeenCalledWith('/orders/2');
  }));

  it('getOrdersByClient should call GET /orders/by-client/:id', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of([]));
    service.getOrdersByClient(10).subscribe(() => resolve());
    expect(apiMock.get).toHaveBeenCalledWith('/orders/by-client/10');
  }));

  it('createOrder should call POST /orders', () => new Promise<void>((resolve) => {
    const dto = { Id_Client: 1, Lignes: [], IsFinalValidation: false };
    apiMock.post.mockReturnValue(of({}));
    service.createOrder(dto).subscribe(() => resolve());
    expect(apiMock.post).toHaveBeenCalledWith('/orders', dto);
  }));

  it('updateOrderStatus should call PUT /orders/status', () => new Promise<void>((resolve) => {
    const dto = { Id_Commande: 1, NouveauStatut: EtatCommande.Validee };
    apiMock.put.mockReturnValue(of({}));
    service.updateOrderStatus(dto).subscribe(() => resolve());
    expect(apiMock.put).toHaveBeenCalledWith('/orders/status', dto);
  }));

  it('deleteOrder should call DELETE /orders/:id', () => new Promise<void>((resolve) => {
    apiMock.delete.mockReturnValue(of({}));
    service.deleteOrder(3).subscribe(() => resolve());
    expect(apiMock.delete).toHaveBeenCalledWith('/orders/3');
  }));

  it('statutToNumber should map string statut to enum number', () => {
    expect(service.statutToNumber('Brouillon')).toBe(0);
    expect(service.statutToNumber('EnAttente')).toBe(1);
    expect(service.statutToNumber('Livree')).toBe(4);
    expect(service.statutToNumber('Unknown')).toBe(-1);
  });

  it('getEtatLabel should return label for numeric statut', () => {
    expect(service.getEtatLabel(0)).toBe('Brouillon');
    expect(service.getEtatLabel(4)).toBe('Livrée');
  });

  it('getEtatLabel should return label for string statut', () => {
    expect(service.getEtatLabel('EnAttente')).toBe('En attente');
  });

  it('getEtatClass should return correct CSS class for numeric statut', () => {
    expect(service.getEtatClass(0)).toBe('chip-default');
    expect(service.getEtatClass(4)).toBe('chip-success');
  });

  it('getEtatClass should return correct CSS class for string statut', () => {
    expect(service.getEtatClass('Annulee')).toBe('chip-danger');
  });

  it('getNextStatuses should return correct transitions for Brouillon', () => {
    const next = service.getNextStatuses('Brouillon');
    const values = next.map(s => s.value);
    expect(values).toContain(EtatCommande.EnAttente);
    expect(values).toContain(EtatCommande.Annulee);
  });

  it('getNextStatuses should return empty array for terminal state Livree', () => {
    const next = service.getNextStatuses('Livree');
    expect(next).toEqual([]);
  });
});
