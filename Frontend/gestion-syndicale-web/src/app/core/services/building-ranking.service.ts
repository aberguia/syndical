import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BuildingRanking } from '../models/building-ranking.model';

@Injectable({
  providedIn: 'root'
})
export class BuildingRankingService {
  private apiUrl = `${environment.apiUrl}/dashboard`;

  constructor(private http: HttpClient) {}

  getBuildingRanking(): Observable<BuildingRanking> {
    return this.http.get<BuildingRanking>(`${this.apiUrl}/building-ranking`);
  }
}
