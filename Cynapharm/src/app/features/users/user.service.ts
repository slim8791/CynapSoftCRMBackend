import { Injectable } from '@angular/core';
import { ApiService } from '../../core/services/api.service';
import { Observable } from 'rxjs';

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
    // ✅ Email encodé dans l'URL
    const encodedEmail = encodeURIComponent(email);
    return this.apiService.put<any>(
      `${this.baseUrl}/delete-user/${encodedEmail}`,
      {}
    );
  }

  enableUser(email: string): Observable<any> {
    const encodedEmail = encodeURIComponent(email);
    return this.apiService.put<any>(
      `${this.baseUrl}/enable-user/${encodedEmail}`,
      {}
    );
  }

  /** Recherche backend (keyword >= 3 chars). isActive=true→actifs, false→désactivés, undefined→tous */
  searchUsers(keyword: string, isActive?: boolean): Observable<any> {
    let url = `${this.baseUrl}/users/search?keyword=${encodeURIComponent(keyword)}`;
    if (isActive !== undefined) url += `&isActive=${isActive}`;
    return this.apiService.get<any>(url);
  }

  getDisabledUsers(): Observable<any> {
    return this.apiService.get<any>(`${this.baseUrl}/disabled-users`);
  }
}

