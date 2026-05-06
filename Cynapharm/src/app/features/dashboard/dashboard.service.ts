import { Injectable } from '@angular/core';
import { ApiService } from '../../core/services/api.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private endpoint = '/dashboard';

  constructor(private apiService: ApiService) { }

  getDashboardData(): Observable<any> {
    return this.apiService.get<any>(this.endpoint);
  }

  getMetrics(): Observable<any> {
    return this.apiService.get<any>(`${this.endpoint}/metrics`);
  }

  getRecentActivity(): Observable<any> {
    return this.apiService.get<any>(`${this.endpoint}/recent-activity`);
  }
}
