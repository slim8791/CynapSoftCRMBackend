import { vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { DashboardComponent } from './dashboard.component';
import { OrderApiService, Commande, EtatCommande } from './services/order-api.service';
import { of, throwError } from 'rxjs';
import { NO_ERRORS_SCHEMA } from '@angular/core';

// ResizeObserver is not available in jsdom — mock it globally
(globalThis as any).ResizeObserver = class {
  observe() {}
  unobserve() {}
  disconnect() {}
};

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let orderApiMock: { getAllOrders: ReturnType<typeof vi.fn>; computeStats: ReturnType<typeof vi.fn> };

  const emptyStats = {
    countByStatus: {}, totalCA: 0, countEnAttente: 0,
    countLivrees: 0, countAnnulees: 0, countToday: 0, totalOrders: 0, last7Days: []
  };

  const mockOrders: Commande[] = [
    { id_Commande: 1, dateCommande: new Date().toISOString().slice(0, 10), montantHT: 100, montantTTC: 120, etatCommande: EtatCommande.EnAttente, id_Client: 1 },
  ];

  function setup() {
    orderApiMock = { getAllOrders: vi.fn(), computeStats: vi.fn() };
    TestBed.configureTestingModule({
      imports: [DashboardComponent],
      providers: [{ provide: OrderApiService, useValue: orderApiMock }],
      schemas: [NO_ERRORS_SCHEMA]
    });
  }

  it('should create', async () => {
    setup();
    orderApiMock.getAllOrders.mockReturnValue(of([]));
    orderApiMock.computeStats.mockReturnValue(emptyStats);
    const fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('ngOnInit should call getAllOrders and computeStats', async () => {
    setup();
    const stats = {
      countByStatus: { 'En attente': 1 }, totalCA: 360, countEnAttente: 1,
      countLivrees: 0, countAnnulees: 0, countToday: 1, totalOrders: 1,
      last7Days: Array(7).fill(null).map((_, i) => ({ date: `2024-01-0${i + 1}`, count: 0, ca: 0 }))
    };
    orderApiMock.getAllOrders.mockReturnValue(of(mockOrders));
    orderApiMock.computeStats.mockReturnValue(stats);
    const fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
    expect(orderApiMock.getAllOrders).toHaveBeenCalled();
    expect(orderApiMock.computeStats).toHaveBeenCalledWith(mockOrders);
    expect(component.caTotal).toBe(360);
    expect(component.loading).toBe(false);
  });

  it('should handle getAllOrders error gracefully via catchError', async () => {
    setup();
    orderApiMock.getAllOrders.mockReturnValue(throwError(() => new Error('fail')));
    orderApiMock.computeStats.mockReturnValue(emptyStats);
    const fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
    expect(component.loading).toBe(false);
  });

  it('reload should trigger loadAll again', async () => {
    setup();
    orderApiMock.getAllOrders.mockReturnValue(of([]));
    orderApiMock.computeStats.mockReturnValue(emptyStats);
    const fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
    const callsBefore = orderApiMock.getAllOrders.mock.calls.length;
    component.reload();
    expect(orderApiMock.getAllOrders.mock.calls.length).toBeGreaterThan(callsBefore);
  });

  it('tauxLivraison should be 0 when no orders', async () => {
    setup();
    orderApiMock.getAllOrders.mockReturnValue(of([]));
    orderApiMock.computeStats.mockReturnValue(emptyStats);
    const fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
    expect(component.tauxLivraison).toBe(0);
  });

  it('ngOnDestroy should not throw', async () => {
    setup();
    orderApiMock.getAllOrders.mockReturnValue(of([]));
    orderApiMock.computeStats.mockReturnValue(emptyStats);
    const fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
    expect(() => component.ngOnDestroy()).not.toThrow();
  });
});
