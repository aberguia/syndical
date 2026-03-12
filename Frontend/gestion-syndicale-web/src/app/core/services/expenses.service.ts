import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Expense,
  ExpenseDetail,
  CreateExpenseDto,
  UpdateExpenseDto,
  ExpensePagedResult,
  ExpenseAttachment,
  ExpenseSummary
} from '../models/expenses.models';

@Injectable({
  providedIn: 'root'
})
export class ExpensesService {
  private readonly apiUrl = `${environment.apiUrl}/expenses`;

  constructor(private http: HttpClient) {}

  getExpenses(
    year?: number,
    month?: number,
    categoryId?: number,
    search?: string,
    page: number = 1,
    pageSize: number = 25
  ): Observable<ExpensePagedResult> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (year) {
      params = params.set('year', year.toString());
    }

    if (month) {
      params = params.set('month', month.toString());
    }

    if (categoryId) {
      params = params.set('categoryId', categoryId.toString());
    }

    if (search && search.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http.get<ExpensePagedResult>(this.apiUrl, { params });
  }

  getExpenseById(id: number): Observable<ExpenseDetail> {
    return this.http.get<ExpenseDetail>(`${this.apiUrl}/${id}`);
  }

  create(dto: CreateExpenseDto): Observable<ExpenseDetail> {
    return this.http.post<ExpenseDetail>(this.apiUrl, dto);
  }

  update(id: number, dto: UpdateExpenseDto): Observable<ExpenseDetail> {
    return this.http.put<ExpenseDetail>(`${this.apiUrl}/${id}`, dto);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  getSummary(year: number): Observable<ExpenseSummary> {
    const params = new HttpParams().set('year', year.toString());
    return this.http.get<ExpenseSummary>(`${this.apiUrl}/summary`, { params });
  }

  // Attachments

  getAttachments(expenseId: number): Observable<ExpenseAttachment[]> {
    return this.http.get<ExpenseAttachment[]>(`${this.apiUrl}/${expenseId}/attachments`);
  }

  uploadAttachment(expenseId: number, file: File): Observable<ExpenseAttachment> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<ExpenseAttachment>(
      `${this.apiUrl}/${expenseId}/attachments`,
      formData
    );
  }

  downloadAttachment(attachmentId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/attachments/${attachmentId}/download`, {
      responseType: 'blob'
    });
  }

  deleteAttachment(expenseId: number, attachmentId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${expenseId}/attachments/${attachmentId}`);
  }
}
