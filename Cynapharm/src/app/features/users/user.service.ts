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
    return this.apiService.put<any>(
      `${this.baseUrl}/delete-user/${email}`,
      {}
    );
  }

  enableUser(email: string): Observable<any> {
    return this.apiService.put<any>(
      `${this.baseUrl}/enable-user/${email}`,
      {}
    );
  }
}

