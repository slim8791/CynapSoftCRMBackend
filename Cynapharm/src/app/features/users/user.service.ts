import { Injectable } from '@angular/core';
import { ApiService } from '../../core/services/api.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private endpoint = '/users';

  constructor(private apiService: ApiService) { }

  getUsers(): Observable<any[]> {
    return this.apiService.get<any[]>(this.endpoint);
  }

  getUserById(id: string): Observable<any> {
    return this.apiService.get<any>(`${this.endpoint}/${id}`);
  }

  createUser(userData: any): Observable<any> {
    return this.apiService.post<any>(this.endpoint, userData);
  }

  updateUser(id: string, userData: any): Observable<any> {
    return this.apiService.put<any>(`${this.endpoint}/${id}`, userData);
  }

  deleteUser(id: string): Observable<any> {
    return this.apiService.delete<any>(`${this.endpoint}/${id}`);
  }
}
