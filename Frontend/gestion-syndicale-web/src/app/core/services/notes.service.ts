import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MemberNote, CreateMemberNoteDto, UpdateMemberNoteDto, MemberLookupForNotes } from '../models/notes.models';

@Injectable({
  providedIn: 'root'
})
export class NotesService {
  private apiUrl = `${environment.apiUrl}/notes`;

  constructor(private http: HttpClient) {}

  getAll(search?: string, buildingId?: number, apartmentId?: number): Observable<MemberNote[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (buildingId) params = params.set('buildingId', buildingId.toString());
    if (apartmentId) params = params.set('apartmentId', apartmentId.toString());
    
    return this.http.get<MemberNote[]>(this.apiUrl, { params });
  }

  getById(id: number): Observable<MemberNote> {
    return this.http.get<MemberNote>(`${this.apiUrl}/${id}`);
  }

  create(dto: CreateMemberNoteDto): Observable<MemberNote> {
    return this.http.post<MemberNote>(this.apiUrl, dto);
  }

  update(id: number, dto: UpdateMemberNoteDto): Observable<MemberNote> {
    return this.http.put<MemberNote>(`${this.apiUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getMembersLookup(buildingId?: number, apartmentId?: number): Observable<MemberLookupForNotes[]> {
    let params = new HttpParams();
    if (buildingId) params = params.set('buildingId', buildingId.toString());
    if (apartmentId) params = params.set('apartmentId', apartmentId.toString());
    
    return this.http.get<MemberLookupForNotes[]>(`${this.apiUrl}/members/lookup`, { params });
  }
}
