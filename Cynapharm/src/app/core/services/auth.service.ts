import { Inject, Injectable, PLATFORM_ID, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { toObservable } from '@angular/core/rxjs-interop';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

// ✅ MODIF : modèles alignés backend
export enum UserRole {
  ADMIN = 'ADMIN',
  SUPERVISEUR = 'SUPERVISEUR',
  DELEGUE = 'DELEGUE',
  MEDECIN = 'MEDECIN',
  CLIENT = 'CLIENT'
}

export enum UserType {
  PHARMACIEN = 'PHARMACIEN',
  GROSSISTE = 'GROSSISTE'
}

export interface User {
  id: number;
  name: string;
  email: string;
  phoneNumber: string;
  adresse: string;
  role: UserRole;
  isDeleted: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private apiUrl = `${environment.apiUrl}/auth`;

  // ✅ MODIF : typer le user (via Signal)
  private currentUserSignal = signal<User | null>(null);
  public currentUser$: Observable<User | null>;

  private isBrowser: boolean;

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);

    // ✅ MODIF : récupération propre du user
    if (this.isBrowser) {
      this.currentUserSignal.set(this.getUserFromStorage());
    }

    this.currentUser$ = toObservable(this.currentUserSignal);
  }

  /**
   * ✅ LOGIN (OK – aligné backend)
   */
  login(email: string, password: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, {
      UserName: email,
      password: password
    }).pipe(
      tap(response => {
        const data = response?.result;

        if (data?.token && data?.user) {
          if (this.isBrowser) {
            localStorage.setItem('token', data.token);
            localStorage.setItem('user', JSON.stringify(data.user));
          }
          this.currentUserSignal.set(data.user);
        }
      })
    );
  }

  /**
   * ✅ MODIF MAJEURE : REGISTER
   * Le backend NE renvoie PAS de token ici
   * L’utilisateur doit se connecter après création
   */
  register(userData: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/register`, userData);
    // ❌ SUPPRIMÉ : stockage token/user (inexistant côté backend)
  }

  /**
   * ❌ SUPPRIMÉ : endpoint inexistant dans AuthAPI
   */
  // getProfile(): Observable<any> {
  //   return this.http.get<any>(`${this.apiUrl}/profile`);
  // }

  /**
   * ✅ LOGOUT
   */
  logout(): void {
    if (this.isBrowser) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
    }
    this.currentUserSignal.set(null);
  }

  /**
   * ✅ TOKEN
   */
  getToken(): string | null {
    return this.isBrowser ? localStorage.getItem('token') : null;
  }

  /**
   * ✅ AUTH CHECK
   */
  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  /**
   * ✅ ROLE UTILS (NOUVEAU)
   */
  getUserRole(): UserRole | null {
    return this.currentUserSignal()?.role ?? null;
  }

  hasRole(roles: UserRole[]): boolean {
    const role = this.getUserRole();
    return role ? roles.includes(role) : false;
  }

  /**
   * ✅ STORAGE
   */
  private getUserFromStorage(): User | null {
    if (!this.isBrowser) return null;

    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }

/**
   * ✅ CURRENT USER
   */
  getCurrentUser(): User | null {
    return this.currentUserSignal();
  }

/**
   * ✅ NOUVEAU : FORGOT PASSWORD
   * Demande un email de réinitialisation de mot de passe
   */
  forgotPassword(email: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/forgot-password`, {
      Email: email
    });
  }

  /**
   * ✅ NOUVEAU : RESET PASSWORD
   * Réinitialise le mot de passe avec le token
   */
  resetPassword(email: string, token: string, newPassword: string): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/reset-password`, {
      Email: email,
      Token: token,
      NewPassword: newPassword
    });
  }

  /**
   * Changement de mot de passe — l'utilisateur doit être connecté.
   * PUT /auth/change-password
   */
  changePassword(email: string, currentPassword: string, newPassword: string): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/change-password`, {
      Email: email,
      CurrentPassword: currentPassword,
      NewPassword: newPassword
    });
  }
}
