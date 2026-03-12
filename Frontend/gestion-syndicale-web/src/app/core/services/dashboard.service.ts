import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AdminDashboard, AdherentDashboard } from '../models/community.models';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private apiUrl = `${environment.apiUrl}/dashboard`;

  constructor(private http: HttpClient) {}

  getDashboard(): Observable<AdminDashboard | AdherentDashboard> {
    return this.http.get<AdminDashboard | AdherentDashboard>(this.apiUrl);
  }
}
