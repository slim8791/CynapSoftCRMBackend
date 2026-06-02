import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { Router, ActivatedRouteSnapshot } from '@angular/router';
import { roleGuard } from './role.guard';
import { AuthService, UserRole } from '../services/auth.service';

describe('roleGuard', () => {
  let authServiceMock: { isAuthenticated: ReturnType<typeof vi.fn>; getUserRole: ReturnType<typeof vi.fn> };
  let routerMock: { navigate: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    authServiceMock = { isAuthenticated: vi.fn(), getUserRole: vi.fn() };
    routerMock = { navigate: vi.fn() };

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authServiceMock },
        { provide: Router, useValue: routerMock }
      ]
    });
  });

  function buildRoute(roles: UserRole[]): ActivatedRouteSnapshot {
    return { data: { roles } } as any;
  }

  function runGuard(route: ActivatedRouteSnapshot): boolean | any {
    return TestBed.runInInjectionContext(() => roleGuard(route, null as any));
  }

  it('should return false and navigate to login when not authenticated', () => {
    authServiceMock.isAuthenticated.mockReturnValue(false);
    const result = runGuard(buildRoute([UserRole.ADMIN]));
    expect(result).toBe(false);
    expect(routerMock.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should return true when user has an allowed role', () => {
    authServiceMock.isAuthenticated.mockReturnValue(true);
    authServiceMock.getUserRole.mockReturnValue(UserRole.ADMIN);
    const result = runGuard(buildRoute([UserRole.ADMIN, UserRole.SUPERVISEUR]));
    expect(result).toBe(true);
  });

  it('should return false and navigate to forbidden when role is not allowed', () => {
    authServiceMock.isAuthenticated.mockReturnValue(true);
    authServiceMock.getUserRole.mockReturnValue(UserRole.CLIENT);
    const result = runGuard(buildRoute([UserRole.ADMIN]));
    expect(result).toBe(false);
    expect(routerMock.navigate).toHaveBeenCalledWith(['/forbidden']);
  });

  it('should return false and navigate to forbidden when getUserRole returns null', () => {
    authServiceMock.isAuthenticated.mockReturnValue(true);
    authServiceMock.getUserRole.mockReturnValue(null);
    const result = runGuard(buildRoute([UserRole.ADMIN]));
    expect(result).toBe(false);
    expect(routerMock.navigate).toHaveBeenCalledWith(['/forbidden']);
  });
});
