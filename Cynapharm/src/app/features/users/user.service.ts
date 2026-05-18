import { Injectable } from '@angular/core';
import { ApiService } from '../../core/services/api.service';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface UserDto {
  id: number;
  name: string;
  email: string;
  phoneNumber: string;
  adresse: string;
  role: string;
  isDeleted: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {

  private baseUrl = '/auth';

  constructor(private apiService: ApiService) {}

  getUsers(): Observable<any[]> {
    return this.apiService.get<any[]>(`${this.baseUrl}/users`);
  }

  getUsersByRole(role: string): Observable<any[]> {
    return this.apiService.get<any>(`${this.baseUrl}/users/role/${encodeURIComponent(role)}`).pipe(
      map((r: any) => {
        const raw = r?.Result ?? r?.result ?? r;
        return Array.isArray(raw) ? raw : [];
      })
    );
  }

  getUserById(id: number): Observable<any> {
    return this.apiService.get<any>(`${this.baseUrl}/users/${id}`);
  }

  registerUser(payload: any): Observable<any> {
    return this.apiService.post<any>(`${this.baseUrl}/register`, payload);
  }

  changeRole(payload: { email: string; newRole: string }): Observable<any> {
    return this.apiService.put<any>(`${this.baseUrl}/change-role`, payload);
  }

  
  disableUser(email: string): Observable<any> {
    return this.apiService.put<any>(`${this.baseUrl}/disable`, { Email: email });
  }

  enableUser(email: string): Observable<any> {
    return this.apiService.put<any>(`${this.baseUrl}/enable`, { Email: email });
  }

  /** Recherche backend (keyword >= 3 chars). isActive=true→actifs, false→désactivés, undefined→tous */
  searchUsers(keyword: string, isActive?: boolean): Observable<any> {
    let url = `${this.baseUrl}/users/search?keyword=${encodeURIComponent(keyword)}`;
    if (isActive !== undefined) url += `&isActive=${isActive}`;
    return this.apiService.get<any>(url);
  }

  getDisabledUsers(): Observable<any> {
    return this.apiService.get<any>(`${this.baseUrl}/users/disabled`);
  }
}

