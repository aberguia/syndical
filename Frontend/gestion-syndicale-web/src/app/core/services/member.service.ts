import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { 
  MemberListDto, 
  CreateMemberDto, 
  UpdateMemberDto, 
  ContactMemberDto 
} from '../models/member.models';

@Injectable({
  providedIn: 'root'
})
export class MemberService {
  private apiUrl = `${environment.apiUrl}/members`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<MemberListDto[]> {
    return this.http.get<MemberListDto[]>(this.apiUrl);
  }

  getById(id: number): Observable<MemberListDto> {
    return this.http.get<MemberListDto>(`${this.apiUrl}/${id}`);
  }

  create(member: CreateMemberDto): Observable<MemberListDto> {
    return this.http.post<MemberListDto>(this.apiUrl, member);
  }

  update(id: number, member: UpdateMemberDto): Observable<MemberListDto> {
    return this.http.put<MemberListDto>(`${this.apiUrl}/${id}`, member);
  }

  delete(id: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.apiUrl}/${id}`);
  }

  contact(id: number, contactDto: ContactMemberDto): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/${id}/contact`, contactDto);
  }
}
