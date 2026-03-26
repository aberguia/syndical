import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { FinancialSummary, GeneratePdfRequest } from '../models/reports.models';
import { LanguageService } from './language.service';

@Injectable({
  providedIn: 'root'
})
export class ReportsService {
  private apiUrl = `${environment.apiUrl}/reports`;

  constructor(private http: HttpClient, private languageService: LanguageService) {}

  getFinancialSummary(from: string, to: string): Observable<FinancialSummary> {
    const params = new HttpParams()
      .set('from', from)
      .set('to', to);

    return this.http.get<FinancialSummary>(`${this.apiUrl}/financial-summary`, { params });
  }

  downloadFinancialSummaryPdf(from: string, to: string): Observable<Blob> {
    const request: GeneratePdfRequest = { from, to, lang: this.languageService.getCurrentLang() };

    return this.http.post(`${this.apiUrl}/financial-summary/pdf`, request, {
      responseType: 'blob'
    });
  }
}
