import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient, withInterceptors, HttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { errorInterceptor } from './error.interceptor';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { ToastService } from '../../shared/services/toast.service';

describe('errorInterceptor', () => {
  let httpMock: HttpTestingController;
  let http: HttpClient;
  let authServiceMock: { logout: ReturnType<typeof vi.fn> };
  let routerMock: { navigate: ReturnType<typeof vi.fn> };
  let toastMock: { showError: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    authServiceMock = { logout: vi.fn() };
    routerMock = { navigate: vi.fn() };
    toastMock = { showError: vi.fn() };

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: authServiceMock },
        { provide: Router, useValue: routerMock },
        { provide: ToastService, useValue: toastMock }
      ]
    });

    httpMock = TestBed.inject(HttpTestingController);
    http = TestBed.inject(HttpClient);
  });

  afterEach(() => httpMock.verify());

  it('should call logout and navigate to login on 401', () => {
    http.get('/api/test').subscribe({ error: () => {} });
    const req = httpMock.expectOne('/api/test');
    req.flush(null, { status: 401, statusText: 'Unauthorized' });
    expect(authServiceMock.logout).toHaveBeenCalled();
    expect(routerMock.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should show error toast on 403', () => {
    http.get('/api/test').subscribe({ error: () => {} });
    const req = httpMock.expectOne('/api/test');
    req.flush(null, { status: 403, statusText: 'Forbidden' });
    expect(toastMock.showError).toHaveBeenCalled();
  });

  it('should show server error toast on 500', () => {
    http.get('/api/test').subscribe({ error: () => {} });
    const req = httpMock.expectOne('/api/test');
    req.flush(null, { status: 500, statusText: 'Server Error' });
    expect(toastMock.showError).toHaveBeenCalled();
  });

  it('should propagate the error as an observable error', () => new Promise<void>((resolve) => {
    http.get('/api/test').subscribe({
      error: (err) => {
        expect(err.status).toBe(404);
        resolve();
      }
    });
    const req = httpMock.expectOne('/api/test');
    req.flush(null, { status: 404, statusText: 'Not Found' });
  }));
});
