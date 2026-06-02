import { vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { ForgotPasswordComponent } from './forgot-password.component';
import { AuthService } from '../../../core/services/auth.service';
import { Router, ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('ForgotPasswordComponent', () => {
  let component: ForgotPasswordComponent;
  let authMock: { forgotPassword: ReturnType<typeof vi.fn> };
  let routerMock: { navigate: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    authMock = { forgotPassword: vi.fn() };
    routerMock = { navigate: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [ForgotPasswordComponent, ReactiveFormsModule],
      providers: [
        { provide: AuthService, useValue: authMock },
        { provide: Router, useValue: routerMock },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => null } } } }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
    const fixture = TestBed.createComponent(ForgotPasswordComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create with forgotForm initialized', () => {
    expect(component).toBeTruthy();
    expect(component.forgotForm).toBeDefined();
  });

  it('onSubmit should do nothing when form is invalid', () => {
    component.forgotForm.setValue({ email: '' });
    component.onSubmit();
    expect(authMock.forgotPassword).not.toHaveBeenCalled();
  });

  it('onSubmit should call forgotPassword with email when form is valid', () => {
    authMock.forgotPassword.mockReturnValue(of({ IsSuccess: true, Message: 'Email sent' }));
    component.forgotForm.setValue({ email: 'a@a.com' });
    component.onSubmit();
    expect(authMock.forgotPassword).toHaveBeenCalledWith('a@a.com');
  });

  it('onSubmit should set success and message when IsSuccess is true', () => {
    authMock.forgotPassword.mockReturnValue(of({ IsSuccess: true, Message: 'Sent!' }));
    component.forgotForm.setValue({ email: 'a@a.com' });
    component.onSubmit();
    expect(component.success).toBe(true);
    expect(component.message).toBe('Sent!');
    expect(component.loading).toBe(false);
  });

  it('onSubmit should set error when IsSuccess is false', () => {
    authMock.forgotPassword.mockReturnValue(of({ IsSuccess: false, Message: 'Not found' }));
    component.forgotForm.setValue({ email: 'a@a.com' });
    component.onSubmit();
    expect(component.error).toBe('Not found');
  });

  it('onSubmit should set error on HTTP failure', () => {
    authMock.forgotPassword.mockReturnValue(throwError(() => ({ error: { Message: 'Server err' } })));
    component.forgotForm.setValue({ email: 'a@a.com' });
    component.onSubmit();
    expect(component.error).toBe('Server err');
  });

  it('goToLogin should navigate to /login', () => {
    component.goToLogin();
    expect(routerMock.navigate).toHaveBeenCalledWith(['/login']);
  });
});
