import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Apartment, ApartmentDto } from '../models/settings.models';

@Injectable({
  providedIn: 'root'
})
export class ApartmentService {
  private apiUrl = `${environment.apiUrl}/apartments`;

  constructor(private http: HttpClient) {}

  getAll(buildingId?: number): Observable<Apartment[]> {
    let params = new HttpParams();
    if (buildingId) {
      params = params.set('buildingId', buildingId.toString());
    }
    return this.http.get<Apartment[]>(this.apiUrl, { params });
  }

  getById(id: number): Observable<Apartment> {
    return this.http.get<Apartment>(`${this.apiUrl}/${id}`);
  }

  getByBuilding(buildingId: number): Observable<Apartment[]> {
    return this.http.get<Apartment[]>(`${this.apiUrl}?buildingId=${buildingId}`);
  }

  create(apartment: ApartmentDto): Observable<Apartment> {
    return this.http.post<Apartment>(this.apiUrl, apartment);
  }

  update(id: number, apartment: ApartmentDto): Observable<Apartment> {
    return this.http.put<Apartment>(`${this.apiUrl}/${id}`, apartment);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getMemberName(apartmentId: number): Observable<{ apartmentId: number, memberName: string | null, hasMember: boolean }> {
    return this.http.get<{ apartmentId: number, memberName: string | null, hasMember: boolean }>(`${this.apiUrl}/${apartmentId}/member-name`);
  }
}
