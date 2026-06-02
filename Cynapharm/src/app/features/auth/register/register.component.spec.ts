import { vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { RegisterComponent } from './register.component';
import { AuthService, UserRole, UserType } from '../../../core/services/auth.service';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let authMock: { register: ReturnType<typeof vi.fn> };
  let routerMock: { navigate: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    authMock = { register: vi.fn() };
    routerMock = { navigate: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [RegisterComponent, ReactiveFormsModule],
      providers: [
        { provide: AuthService, useValue: authMock },
        { provide: Router, useValue: routerMock }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    const fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create with registerForm initialized', () => {
    expect(component).toBeTruthy();
    expect(component.registerForm).toBeDefined();
  });

  it('onRegister should do nothing when form is invalid', () => {
    component.onRegister();
    expect(authMock.register).not.toHaveBeenCalled();
  });

  it('onRegister should call authService.register with payload when form is valid', () => {
    authMock.register.mockReturnValue(of({}));
    component.registerForm.patchValue({
      name: 'Ahmed', email: 'a@a.com', password: 'secret123',
      adresse: '123 Rue', role: UserRole.CLIENT, userType: UserType.PHARMACIEN
    });
    component.onRegister();
    expect(authMock.register).toHaveBeenCalled();
    const payload = (authMock.register.mock.calls[0] as any[])[0];
    expect(payload.email).toBe('a@a.com');
    expect(payload.name).toBe('Ahmed');
  });

  it('onRegister should set success=true on successful registration', () => {
    authMock.register.mockReturnValue(of({}));
    component.registerForm.patchValue({
      name: 'Ahmed', email: 'a@a.com', password: 'secret123',
      adresse: '123 Rue', role: UserRole.CLIENT, userType: UserType.PHARMACIEN
    });
    component.onRegister();
    expect(component.success).toBe(true);
  });

  it('onRegister should set error on failure', () => {
    authMock.register.mockReturnValue(throwError(() => ({ error: { message: 'Email taken' } })));
    component.registerForm.patchValue({
      name: 'Ahmed', email: 'a@a.com', password: 'secret123',
      adresse: '123 Rue', role: UserRole.CLIENT, userType: UserType.PHARMACIEN
    });
    component.onRegister();
    expect(component.error).toBe('Email taken');
    expect(component.loading).toBe(false);
  });
});
