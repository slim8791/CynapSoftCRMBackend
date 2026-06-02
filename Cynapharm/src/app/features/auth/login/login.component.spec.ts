import { vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { LoginComponent } from './login.component';
import { AuthService, UserRole } from '../../../core/services/auth.service';
import { Router, ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let authMock: { login: ReturnType<typeof vi.fn>; getCurrentUser: ReturnType<typeof vi.fn> };
  let routerMock: { navigate: ReturnType<typeof vi.fn>; navigateByUrl: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    authMock = { login: vi.fn(), getCurrentUser: vi.fn() };
    routerMock = { navigate: vi.fn(), navigateByUrl: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [LoginComponent, ReactiveFormsModule],
      providers: [
        { provide: AuthService, useValue: authMock },
        { provide: Router, useValue: routerMock },
        { provide: ActivatedRoute, useValue: { snapshot: { queryParamMap: { get: () => null } } } }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    const fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create with loginForm initialized', () => {
    expect(component).toBeTruthy();
    expect(component.loginForm).toBeDefined();
  });

  it('onLogin should do nothing when form is invalid', () => {
    component.loginForm.setValue({ email: '', password: '' });
    component.onLogin();
    expect(authMock.login).not.toHaveBeenCalled();
  });

  it('onLogin should call authService.login with credentials when form is valid', () => {
    authMock.login.mockReturnValue(of({}));
    authMock.getCurrentUser.mockReturnValue({ id: 1, name: 'T', email: 'e@e.com', role: UserRole.ADMIN, phoneNumber: '', adresse: '', isDeleted: false });
    component.loginForm.setValue({ email: 'test@example.com', password: 'password123' });
    component.onLogin();
    expect(authMock.login).toHaveBeenCalledWith('test@example.com', 'password123');
  });

  it('onLogin should navigate to /users for ADMIN on success', () => {
    authMock.login.mockReturnValue(of({}));
    authMock.getCurrentUser.mockReturnValue({ id: 1, name: 'T', email: 'e@e.com', role: UserRole.ADMIN, phoneNumber: '', adresse: '', isDeleted: false });
    component.loginForm.setValue({ email: 'test@example.com', password: 'password123' });
    component.onLogin();
    expect(component.loading).toBe(false);
    expect(routerMock.navigateByUrl).toHaveBeenCalledWith('/users');
  });

  it('onLogin should set error message on failure', () => {
    authMock.login.mockReturnValue(throwError(() => new Error('fail')));
    component.loginForm.setValue({ email: 'test@example.com', password: 'password123' });
    component.onLogin();
    expect(component.loading).toBe(false);
    expect(component.error).toBe('Login failed');
  });

  it('onLogin should navigate to /dashboard for SUPERVISEUR', () => {
    authMock.login.mockReturnValue(of({}));
    authMock.getCurrentUser.mockReturnValue({ id: 1, name: 'T', email: 'e@e.com', role: UserRole.SUPERVISEUR, phoneNumber: '', adresse: '', isDeleted: false });
    component.loginForm.setValue({ email: 'test@example.com', password: 'password123' });
    component.onLogin();
    expect(routerMock.navigateByUrl).toHaveBeenCalledWith('/dashboard');
  });

  it('onLogin should navigate to /home for CLIENT', () => {
    authMock.login.mockReturnValue(of({}));
    authMock.getCurrentUser.mockReturnValue({ id: 1, name: 'T', email: 'e@e.com', role: UserRole.CLIENT, phoneNumber: '', adresse: '', isDeleted: false });
    component.loginForm.setValue({ email: 'test@example.com', password: 'password123' });
    component.onLogin();
    expect(routerMock.navigateByUrl).toHaveBeenCalledWith('/home');
  });
});
