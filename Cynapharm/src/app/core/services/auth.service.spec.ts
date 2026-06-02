import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService, UserRole } from './auth.service';
import { PLATFORM_ID } from '@angular/core';
import { environment } from '../../../environments/environment';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiUrl}/auth`;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        AuthService,
        { provide: PLATFORM_ID, useValue: 'browser' }
      ]
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('login should POST credentials and store token and user on success', () => {
    const mockUser = { id: 1, name: 'Test', email: 't@t.com', role: UserRole.ADMIN, phoneNumber: '', adresse: '', isDeleted: false };
    const mockResponse = { result: { token: 'abc123', user: mockUser } };

    service.login('t@t.com', 'pass').subscribe();
    const req = httpMock.expectOne(`${apiUrl}/login`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ UserName: 't@t.com', password: 'pass' });
    req.flush(mockResponse);

    expect(localStorage.getItem('token')).toBe('abc123');
    expect(JSON.parse(localStorage.getItem('user')!)).toEqual(mockUser);
    expect(service.getCurrentUser()).toEqual(mockUser);
  });

  it('register should POST user data without storing tokens', () => {
    service.register({ name: 'A', email: 'a@a.com' }).subscribe();
    const req = httpMock.expectOne(`${apiUrl}/register`);
    expect(req.request.method).toBe('POST');
    req.flush({});
    expect(localStorage.getItem('token')).toBeNull();
  });

  it('logout should clear localStorage and nullify currentUser', () => {
    localStorage.setItem('token', 'tok');
    localStorage.setItem('user', '{"id":1}');
    service.logout();
    expect(localStorage.getItem('token')).toBeNull();
    expect(localStorage.getItem('user')).toBeNull();
    expect(service.getCurrentUser()).toBeNull();
  });

  it('getToken should return token from localStorage when browser', () => {
    localStorage.setItem('token', 'mytoken');
    expect(service.getToken()).toBe('mytoken');
  });

  it('getToken should return null when no token in localStorage', () => {
    expect(service.getToken()).toBeNull();
  });

  it('isAuthenticated should return true when token exists', () => {
    localStorage.setItem('token', 'tok');
    expect(service.isAuthenticated()).toBe(true);
  });

  it('isAuthenticated should return false when no token', () => {
    expect(service.isAuthenticated()).toBe(false);
  });

  it('getUserRole should return null when no user', () => {
    expect(service.getUserRole()).toBeNull();
  });

  it('hasRole should return false when no current user', () => {
    expect(service.hasRole([UserRole.ADMIN])).toBe(false);
  });

  it('forgotPassword should POST email to forgot-password endpoint', () => {
    service.forgotPassword('a@a.com').subscribe();
    const req = httpMock.expectOne(`${apiUrl}/forgot-password`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ Email: 'a@a.com' });
    req.flush({});
  });

  it('resetPassword should PUT reset data to reset-password endpoint', () => {
    service.resetPassword('a@a.com', 'tok', 'newpass').subscribe();
    const req = httpMock.expectOne(`${apiUrl}/reset-password`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ Email: 'a@a.com', Token: 'tok', NewPassword: 'newpass' });
    req.flush({});
  });

  it('changePassword should PUT change-password data', () => {
    service.changePassword('a@a.com', 'old', 'new').subscribe();
    const req = httpMock.expectOne(`${apiUrl}/change-password`);
    expect(req.request.method).toBe('PUT');
    req.flush({});
  });
});
