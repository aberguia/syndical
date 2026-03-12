import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AppConfig {
  residenceName: string;
  syndicLegalName: string;
  residenceStartYear: number;
}

@Injectable({
  providedIn: 'root'
})
export class ConfigService {
  private config: AppConfig | null = null;
  private apiUrl = `${environment.apiUrl}/config`;

  constructor(private http: HttpClient) {}

  loadConfig(): Observable<AppConfig> {
    return this.http.get<AppConfig>(`${this.apiUrl}/app-settings`).pipe(
      tap(config => {
        this.config = config;
      })
    );
  }

  getConfig(): AppConfig | null {
    return this.config;
  }

  getResidenceName(): string {
    return this.config?.residenceName || 'Résidence';
  }

  getSyndicLegalName(): string {
    return this.config?.syndicLegalName || 'Syndic';
  }

  getResidenceStartYear(): number {
    return this.config?.residenceStartYear || new Date().getFullYear();
  }
}
