import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ContributionsMatrix,
  OtherRevenuePagedResult,
  OtherRevenueDetail,
  CreateOtherRevenueDto,
  UpdateOtherRevenueDto,
  RevenuesTotal
} from '../models/revenues.models';

@Injectable({
  providedIn: 'root'
})
export class RevenuesService {
  private apiUrl = `${environment.apiUrl}/revenues`;

  constructor(private http: HttpClient) {}

  // Contributions Matrix
  getContributionsMatrix(year: number): Observable<ContributionsMatrix> {
    const params = new HttpParams().set('year', year.toString());
    return this.http.get<ContributionsMatrix>(`${this.apiUrl}/contributions/matrix`, { params });
  }

  // Other Revenues
  getOtherRevenues(year: number, search?: string, page: number = 1, pageSize: number = 25): Observable<OtherRevenuePagedResult> {
    let params = new HttpParams()
      .set('year', year.toString())
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (search) {
      params = params.set('search', search);
    }

    return this.http.get<OtherRevenuePagedResult>(`${this.apiUrl}/other`, { params });
  }

  getOtherRevenue(id: number): Observable<OtherRevenueDetail> {
    return this.http.get<OtherRevenueDetail>(`${this.apiUrl}/other/${id}`);
  }

  createOtherRevenue(dto: CreateOtherRevenueDto): Observable<OtherRevenueDetail> {
    return this.http.post<OtherRevenueDetail>(`${this.apiUrl}/other`, dto);
  }

  updateOtherRevenue(id: number, dto: UpdateOtherRevenueDto): Observable<OtherRevenueDetail> {
    return this.http.put<OtherRevenueDetail>(`${this.apiUrl}/other/${id}`, dto);
  }

  deleteOtherRevenue(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/other/${id}`);
  }

  // Attachments
  uploadAttachments(revenueId: number, files: File[]): Observable<any> {
    const formData = new FormData();
    files.forEach(file => {
      formData.append('files', file);
    });
    return this.http.post(`${this.apiUrl}/other/${revenueId}/attachments`, formData);
  }

  downloadAttachment(revenueId: number, documentId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/other/${revenueId}/attachments/${documentId}/download`, {
      responseType: 'blob'
    });
  }

  deleteAttachment(revenueId: number, documentId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/other/${revenueId}/attachments/${documentId}`);
  }

  // Totals
  getRevenuesTotals(year: number): Observable<RevenuesTotal> {
    const params = new HttpParams().set('year', year.toString());
    return this.http.get<RevenuesTotal>(`${this.apiUrl}/totals`, { params });
  }
}
