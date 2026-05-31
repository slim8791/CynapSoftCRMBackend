import { Injectable } from '@angular/core';
import { ApiService } from '../../core/services/api.service';
import { forkJoin, Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

export interface UserDto {
  id: number;
  name: string;
  email: string;
  phoneNumber: string;
  adresse: string;
  role: string;
  isDeleted: boolean;
  idRegion?: number;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {

  private baseUrl = '/auth';

  constructor(private apiService: ApiService) { }

  getUsers(): Observable<any[]> {
    return this.apiService.get<any[]>(`${this.baseUrl}/users`);
  }

  getAllUsers(): Observable<any> {
    return this.apiService.get<any>(`${this.baseUrl}/users`);
  }

  getUsersByRole(role: string): Observable<any[]> {
    return this.apiService.get<any>(`${this.baseUrl}/users/by-role/${encodeURIComponent(role)}`).pipe(
      map(r => r?.Result ?? r?.result ?? r ?? [])
    );
  }

  getUserById(id: number): Observable<any> {
    return this.apiService.get<any>(`${this.baseUrl}/users/${id}`);
  }

  unwrapList(response: any): any[] {
    const raw = Array.isArray(response)
      ? response
      : (response?.Result ?? response?.result ?? response?.Data ?? response?.data ?? response?.Items ?? response?.items ?? []);
    return Array.isArray(raw) ? raw : [];
  }

  unwrapUser(response: any): any {
    return response?.Result ?? response?.result ?? response?.Data ?? response?.data ?? response;
  }

  userId(user: any): number | null {
    const raw = user?.id ?? user?.Id ?? user?.idUser ?? user?.IdUser ?? user?.userId ?? user?.UserId;
    const id = Number(raw);
    return Number.isFinite(id) ? id : null;
  }

  displayName(user: any, fallbackId?: number): string {
    const u = this.unwrapUser(user);
    const firstName = u?.prenom ?? u?.Prenom ?? u?.firstName ?? u?.FirstName ?? '';
    const lastName = u?.nom ?? u?.Nom ?? u?.lastName ?? u?.LastName ?? '';
    const fullName = `${firstName} ${lastName}`.trim();
    return fullName
      || u?.name
      || u?.Name
      || u?.fullName
      || u?.FullName
      || u?.email
      || u?.Email
      || (fallbackId != null ? `#${fallbackId}` : '');
  }

  toUserOption(user: any): { id: number; nom: string } | null {
    const id = this.userId(user);
    return id != null ? { id, nom: this.displayName(user, id) } : null;
  }

  getDisplayNamesByIds(ids: Array<number | null | undefined>): Observable<Record<number, string>> {
    const uniqueIds = [...new Set(
      ids
        .map(id => Number(id))
        .filter(id => Number.isFinite(id) && id > 0)
    )];

    if (!uniqueIds.length) return of({});

    return forkJoin(uniqueIds.map(id =>
      this.getUserById(id).pipe(
        map(response => [id, this.displayName(response, id)] as [number, string]),
        catchError(() => of([id, `#${id}`] as [number, string]))
      )
    )).pipe(
      map(entries => entries.reduce((acc, [id, name]) => {
        acc[id] = name;
        return acc;
      }, {} as Record<number, string>))
    );
  }

  registerUser(payload: any): Observable<any> {
    return this.apiService.post<any>(`${this.baseUrl}/register`, payload);
  }

  changeRole(payload: { email: string; newRole: string }): Observable<any> {
    return this.apiService.put<any>(`${this.baseUrl}/change-role`, payload);
  }


  disableUser(email: string): Observable<any> {
    return this.apiService.put<any>(`${this.baseUrl}/delete-user/${encodeURIComponent(email)}`, {});
  }

  enableUser(email: string): Observable<any> {
    return this.apiService.put<any>(`${this.baseUrl}/enable-user/${encodeURIComponent(email)}`, {});
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

  getUsersByRegion(idRegion: number): Observable<any[]> {
    return this.apiService.get<any>(`${this.baseUrl}/users/by-region/${idRegion}`).pipe(
      map(r => r?.Result ?? r?.result ?? r ?? []),
      catchError(() => of([]))
    );
  }

  updateProfile(payload: {
    email:        string;
    idRegion?:    number | null;
    name?:        string;
    phoneNumber?: string;
    adresse?:     string;
  }): Observable<any> {
    return this.apiService.put<any>(`${this.baseUrl}/update-profile`, payload);
  }

  updateUserById(id: number, payload: any): Observable<any> {
    return this.apiService.put<any>(`${this.baseUrl}/users/${id}`, payload);
  }
}

