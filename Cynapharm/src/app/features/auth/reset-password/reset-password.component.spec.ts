import { vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { ResetPasswordComponent } from './reset-password.component';
import { AuthService } from '../../../core/services/auth.service';
import { Router, ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('ResetPasswordComponent', () => {
  let component: ResetPasswordComponent;
  let authMock: { resetPassword: ReturnType<typeof vi.fn> };
  let routerMock: { navigate: ReturnType<typeof vi.fn> };

  function buildRoute(email: string, token: string) {
    return { snapshot: { queryParamMap: { get: (k: string) => k === 'email' ? email : k === 'token' ? token : null } } };
  }

  async function setup(email = 'a@a.com', token = 'tok123') {
    authMock = { resetPassword: vi.fn() };
    routerMock = { navigate: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [ResetPasswordComponent, ReactiveFormsModule],
      providers: [
        { provide: AuthService, useValue: authMock },
        { provide: Router, useValue: routerMock },
        { provide: ActivatedRoute, useValue: buildRoute(email, token) }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
    const fixture = TestBed.createComponent(ResetPasswordComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  it('should read email and token from query params', async () => {
    await setup('u@u.com', 'mytoken');
    expect(component.email).toBe('u@u.com');
    expect(component.token).toBe('mytoken');
  });

  it('ngOnInit should set error when email or token missing', async () => {
    await setup('', '');
    expect(component.error).toBe('Lien de réinitialisation invalide.');
  });

  it('onSubmit should do nothing when form is invalid', async () => {
    await setup();
    component.resetForm.setValue({ newPassword: '', confirmPassword: '' });
    component.onSubmit();
    expect(authMock.resetPassword).not.toHaveBeenCalled();
  });

  it('onSubmit should call resetPassword when form is valid', async () => {
    await setup();
    authMock.resetPassword.mockReturnValue(of({ IsSuccess: true, Message: 'Done' }));
    component.resetForm.setValue({ newPassword: 'newpass1', confirmPassword: 'newpass1' });
    component.onSubmit();
    expect(authMock.resetPassword).toHaveBeenCalledWith('a@a.com', 'tok123', 'newpass1');
  });

  it('onSubmit should set success on successful reset', async () => {
    await setup();
    authMock.resetPassword.mockReturnValue(of({ IsSuccess: true, Message: 'Password reset!' }));
    component.resetForm.setValue({ newPassword: 'newpass1', confirmPassword: 'newpass1' });
    component.onSubmit();
    expect(component.success).toBe(true);
    expect(component.message).toBe('Password reset!');
  });

  it('onSubmit should set error on failure', async () => {
    await setup();
    authMock.resetPassword.mockReturnValue(throwError(() => ({ error: { Message: 'Invalid token' } })));
    component.resetForm.setValue({ newPassword: 'newpass1', confirmPassword: 'newpass1' });
    component.onSubmit();
    expect(component.error).toBe('Invalid token');
  });

  it('goToLogin should navigate to /login', async () => {
    await setup();
    component.goToLogin();
    expect(routerMock.navigate).toHaveBeenCalledWith(['/login']);
  });
});
