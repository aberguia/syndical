import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ParkingStatus, IncrementDecrementDto, SetCurrentCarsDto } from '../models/parking.models';

@Injectable({
  providedIn: 'root'
})
export class ParkingStatusService {
  private apiUrl = `${environment.apiUrl}/parking`;

  constructor(private http: HttpClient) {}

  getStatus(): Observable<ParkingStatus> {
    return this.http.get<ParkingStatus>(`${this.apiUrl}/status`);
  }

  increment(count: number = 1): Observable<ParkingStatus> {
    const dto: IncrementDecrementDto = { count };
    return this.http.post<ParkingStatus>(`${this.apiUrl}/increment`, dto);
  }

  decrement(count: number = 1): Observable<ParkingStatus> {
    const dto: IncrementDecrementDto = { count };
    return this.http.post<ParkingStatus>(`${this.apiUrl}/decrement`, dto);
  }

  setCurrentCars(currentCars: number): Observable<ParkingStatus> {
    const dto: SetCurrentCarsDto = { currentCars };
    return this.http.put<ParkingStatus>(`${this.apiUrl}/status`, dto);
  }
}
