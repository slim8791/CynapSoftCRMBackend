import { Injectable } from '@angular/core';
import { ApiService } from '../../core/services/api.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  // ✅ MODIF : endpoint réel AuthAPI
  private baseUrl = '/auth';

  constructor(private apiService: ApiService) {}

  /**
   * ✅ GET ALL USERS (ADMIN)
   * GET /api/auth/users
   */
  getUsers(): Observable<any[]> {
    return this.apiService.get<any[]>(`${this.baseUrl}/users`);
  }

  /**
   * ❌ SUPPRIMÉ : endpoint inexistant
   * getUserById(id: string)
   */

  /**
   * ✅ CREATE USER
   * POST /api/auth/register
   */
  registerUser(payload: any): Observable<any> {
    return this.apiService.post<any>(`${this.baseUrl}/register`, payload);
  }

  /**
   * ❌ SUPPRIMÉ : createUser()
   * ❌ SUPPRIMÉ : updateUser()
   * (backend ne supporte PAS ces routes)
   */

  /**
   * ✅ CHANGE ROLE
   * PUT /api/auth/change-role
   */
  changeRole(payload: { email: string; newRole: string }): Observable<any> {
    return this.apiService.put<any>(`${this.baseUrl}/change-role`, payload);
  }

  /**
   * ✅ DISABLE USER (soft delete)
   * PUT /api/auth/delete-user/{email}
   */
  disableUser(email: string): Observable<any> {
    return this.apiService.put<any>(
      `${this.baseUrl}/delete-user/${email}`,
      {}
    );
  }

  /**
   * ✅ ENABLE USER
   * PUT /api/auth/enable-user/{email}
   */
  enableUser(email: string): Observable<any> {
    return this.apiService.put<any>(
      `${this.baseUrl}/enable-user/${email}`,
      {}
    );
  }
}