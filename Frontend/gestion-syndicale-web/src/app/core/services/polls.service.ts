import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Poll, PollListDto, CreatePollDto, UpdatePollDto, PortalPoll, PollVoteDto, PollResult } from '../models/community.models';

@Injectable({
  providedIn: 'root'
})
export class PollsService {
  private adminUrl = `${environment.apiUrl}/admin/polls`;
  private portalUrl = `${environment.apiUrl}/portal/polls`;

  constructor(private http: HttpClient) {}

  // Admin endpoints
  getAdminList(status?: string, page = 1, pageSize = 10): Observable<PollListDto> {
    let params = new HttpParams().set('page', page.toString()).set('pageSize', pageSize.toString());
    if (status) params = params.set('status', status);
    return this.http.get<PollListDto>(this.adminUrl, { params });
  }

  getAdminById(id: number): Observable<Poll> {
    return this.http.get<Poll>(`${this.adminUrl}/${id}`);
  }

  create(dto: CreatePollDto): Observable<Poll> {
    return this.http.post<Poll>(this.adminUrl, dto);
  }

  update(id: number, dto: UpdatePollDto): Observable<Poll> {
    return this.http.put<Poll>(`${this.adminUrl}/${id}`, dto);
  }

  publish(id: number): Observable<void> {
    return this.http.post<void>(`${this.adminUrl}/${id}/publish`, {});
  }

  close(id: number): Observable<void> {
    return this.http.post<void>(`${this.adminUrl}/${id}/close`, {});
  }

  getResults(id: number): Observable<PollResult[]> {
    return this.http.get<PollResult[]>(`${this.adminUrl}/${id}/results`);
  }

  // Portal endpoints
  getPublished(): Observable<PortalPoll[]> {
    return this.http.get<PortalPoll[]>(this.portalUrl);
  }

  getPublishedById(id: number): Observable<PortalPoll> {
    return this.http.get<PortalPoll>(`${this.portalUrl}/${id}`);
  }

  vote(dto: PollVoteDto): Observable<any> {
    return this.http.post(`${this.portalUrl}/${dto.pollId}/vote`, dto);
  }
}
