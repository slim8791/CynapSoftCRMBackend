import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';

describe('authGuard', () => {
  let authServiceMock: { isAuthenticated: ReturnType<typeof vi.fn> };
  let routerMock: { navigate: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    authServiceMock = { isAuthenticated: vi.fn() };
    routerMock = { navigate: vi.fn() };

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authServiceMock },
        { provide: Router, useValue: routerMock }
      ]
    });
  });

  function runGuard(url = '/dashboard'): boolean | any {
    return TestBed.runInInjectionContext(() => {
      const route = {} as ActivatedRouteSnapshot;
      const state = { url } as RouterStateSnapshot;
      return authGuard(route, state);
    });
  }

  it('should return true when user is authenticated', () => {
    authServiceMock.isAuthenticated.mockReturnValue(true);
    expect(runGuard()).toBe(true);
    expect(routerMock.navigate).not.toHaveBeenCalled();
  });

  it('should return false and navigate to login when user is not authenticated', () => {
    authServiceMock.isAuthenticated.mockReturnValue(false);
    const result = runGuard('/protected');
    expect(result).toBe(false);
    expect(routerMock.navigate).toHaveBeenCalledWith(['/login'], { queryParams: { returnUrl: '/protected' } });
  });
});
