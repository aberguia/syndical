import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Announcement, AnnouncementListDto, CreateAnnouncementDto, UpdateAnnouncementDto } from '../models/community.models';

@Injectable({
  providedIn: 'root'
})
export class AnnouncementsService {
  private adminUrl = `${environment.apiUrl}/admin/announcements`;
  private portalUrl = `${environment.apiUrl}/portal/announcements`;

  constructor(private http: HttpClient) {}

  // Admin endpoints
  getAdminList(status?: string, search?: string, page = 1, pageSize = 10): Observable<AnnouncementListDto> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    if (status) params = params.set('status', status);
    if (search) params = params.set('search', search);
    return this.http.get<AnnouncementListDto>(this.adminUrl, { params });
  }

  getAdminById(id: number): Observable<Announcement> {
    return this.http.get<Announcement>(`${this.adminUrl}/${id}`);
  }

  create(dto: CreateAnnouncementDto): Observable<Announcement> {
    return this.http.post<Announcement>(this.adminUrl, dto);
  }

  update(id: number, dto: UpdateAnnouncementDto): Observable<Announcement> {
    return this.http.put<Announcement>(`${this.adminUrl}/${id}`, dto);
  }

  publish(id: number): Observable<void> {
    return this.http.post<void>(`${this.adminUrl}/${id}/publish`, {});
  }

  archive(id: number): Observable<void> {
    return this.http.post<void>(`${this.adminUrl}/${id}/archive`, {});
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.adminUrl}/${id}`);
  }

  // Portal endpoints
  getPublished(page = 1, pageSize = 20): Observable<Announcement[]> {
    const params = new HttpParams().set('page', page.toString()).set('pageSize', pageSize.toString());
    return this.http.get<Announcement[]>(this.portalUrl, { params });
  }

  getPublishedById(id: number): Observable<Announcement> {
    return this.http.get<Announcement>(`${this.portalUrl}/${id}`);
  }
}
