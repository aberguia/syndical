import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Building, BuildingDto } from '../models/settings.models';

@Injectable({
  providedIn: 'root'
})
export class BuildingService {
  private apiUrl = `${environment.apiUrl}/buildings`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Building[]> {
    return this.http.get<Building[]>(this.apiUrl);
  }

  getById(id: number): Observable<Building> {
    return this.http.get<Building>(`${this.apiUrl}/${id}`);
  }

  create(building: BuildingDto): Observable<Building> {
    return this.http.post<Building>(this.apiUrl, building);
  }

  update(id: number, building: BuildingDto): Observable<Building> {
    return this.http.put<Building>(`${this.apiUrl}/${id}`, building);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
