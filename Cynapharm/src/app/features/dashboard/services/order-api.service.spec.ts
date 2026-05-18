import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { OrderApiService, EtatCommande, Commande } from './order-api.service';
import { ApiService } from '../../../core/services/api.service';
import { of } from 'rxjs';

describe('OrderApiService', () => {
  let service: OrderApiService;
  let apiMock: { get: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    apiMock = { get: vi.fn() };
    TestBed.configureTestingModule({
      providers: [
        OrderApiService,
        { provide: ApiService, useValue: apiMock }
      ]
    });
    service = TestBed.inject(OrderApiService);
  });

  it('getAllOrders should call GET /orders and return array', () => new Promise<void>((resolve) => {
    const mockOrders: Commande[] = [
      { id_Commande: 1, dateCommande: '2024-01-01', montantHT: 100, montantTTC: 120, etatCommande: EtatCommande.EnAttente, id_Client: 5 }
    ];
    apiMock.get.mockReturnValue(of(mockOrders));
    service.getAllOrders().subscribe(orders => {
      expect(orders).toEqual(mockOrders);
      resolve();
    });
    expect(apiMock.get).toHaveBeenCalledWith('/orders');
  }));

  it('getAllOrders should return empty array when response is not an array', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of(null));
    service.getAllOrders().subscribe(orders => {
      expect(orders).toEqual([]);
      resolve();
    });
  }));

  it('getOrdersByClient should call GET /orders/by-client/:id', () => new Promise<void>((resolve) => {
    apiMock.get.mockReturnValue(of({ Result: [] }));
    service.getOrdersByClient(42).subscribe(orders => {
      expect(orders).toEqual([]);
      resolve();
    });
    expect(apiMock.get).toHaveBeenCalledWith('/orders/by-client/42');
  }));

  it('computeStats should return correct totals for a list of orders', () => {
    const today = new Date().toISOString().slice(0, 10);
    const orders: Commande[] = [
      { id_Commande: 1, dateCommande: today, montantHT: 100, montantTTC: 120, etatCommande: EtatCommande.EnAttente, id_Client: 1 },
      { id_Commande: 2, dateCommande: today, montantHT: 200, montantTTC: 240, etatCommande: EtatCommande.Livree, id_Client: 2 },
      { id_Commande: 3, dateCommande: today, montantHT: 50, montantTTC: 60, etatCommande: EtatCommande.Annulee, id_Client: 3 },
    ];
    const stats = service.computeStats(orders);
    expect(stats.totalOrders).toBe(3);
    expect(stats.totalCA).toBe(420);
    expect(stats.countEnAttente).toBe(1);
    expect(stats.countLivrees).toBe(1);
    expect(stats.countAnnulees).toBe(1);
    expect(stats.countToday).toBe(3);
  });

  it('computeStats should compute last7Days buckets', () => {
    const stats = service.computeStats([]);
    expect(stats.last7Days.length).toBe(7);
  });

  it('computeStats should return 0 livrees when no orders', () => {
    const stats = service.computeStats([]);
    expect(stats.countLivrees).toBe(0);
  });
});
