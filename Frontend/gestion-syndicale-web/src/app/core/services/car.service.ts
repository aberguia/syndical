import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Car, CreateCarDto, UpdateCarDto, MemberLookup } from '../models/parking.models';

@Injectable({
  providedIn: 'root'
})
export class CarService {
  private apiUrl = `${environment.apiUrl}/cars`;

  constructor(private http: HttpClient) {}

  getAll(search?: string, buildingId?: number): Observable<Car[]> {
    let params = new HttpParams();
    if (search) {
      params = params.set('search', search);
    }
    if (buildingId) {
      params = params.set('buildingId', buildingId.toString());
    }
    return this.http.get<Car[]>(this.apiUrl, { params });
  }

  getById(id: number): Observable<Car> {
    return this.http.get<Car>(`${this.apiUrl}/${id}`);
  }

  create(car: CreateCarDto): Observable<Car> {
    return this.http.post<Car>(this.apiUrl, car);
  }

  update(id: number, car: UpdateCarDto): Observable<Car> {
    return this.http.put<Car>(`${this.apiUrl}/${id}`, car);
  }

  delete(id: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.apiUrl}/${id}`);
  }

  getMembersLookup(buildingId?: number): Observable<MemberLookup[]> {
    let params = new HttpParams();
    if (buildingId) {
      params = params.set('buildingId', buildingId.toString());
    }
    return this.http.get<MemberLookup[]>(`${this.apiUrl}/members/lookup`, { params });
  }
}
