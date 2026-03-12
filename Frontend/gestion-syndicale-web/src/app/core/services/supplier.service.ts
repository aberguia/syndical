import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  SupplierListDto,
  SupplierDetailDto,
  CreateSupplierDto,
  UpdateSupplierDto,
  SupplierLookupDto
} from '../models/supplier.models';

@Injectable({
  providedIn: 'root'
})
export class SupplierService {
  private apiUrl = `${environment.apiUrl}/admin/suppliers`;
  private lookupUrl = `${environment.apiUrl}/lookups/suppliers`;

  constructor(private http: HttpClient) {}

  getAll(category?: string, search?: string): Observable<SupplierListDto[]> {
    let params = new HttpParams();
    if (category) {
      params = params.set('category', category);
    }
    if (search) {
      params = params.set('q', search);
    }
    return this.http.get<SupplierListDto[]>(this.apiUrl, { params });
  }

  getById(id: number): Observable<SupplierDetailDto> {
    return this.http.get<SupplierDetailDto>(`${this.apiUrl}/${id}`);
  }

  create(dto: CreateSupplierDto): Observable<SupplierDetailDto> {
    return this.http.post<SupplierDetailDto>(this.apiUrl, dto);
  }

  update(id: number, dto: UpdateSupplierDto): Observable<SupplierDetailDto> {
    return this.http.put<SupplierDetailDto>(`${this.apiUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getLookups(category?: string): Observable<SupplierLookupDto[]> {
    let params = new HttpParams();
    if (category) {
      params = params.set('category', category);
    }
    return this.http.get<SupplierLookupDto[]>(this.lookupUrl, { params });
  }
}
