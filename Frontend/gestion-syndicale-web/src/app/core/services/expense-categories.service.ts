import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ExpenseCategory,
  CreateExpenseCategoryDto,
  UpdateExpenseCategoryDto
} from '../models/expenses.models';

@Injectable({
  providedIn: 'root'
})
export class ExpenseCategoriesService {
  private readonly apiUrl = `${environment.apiUrl}/expense-categories`;

  constructor(private http: HttpClient) {}

  getAll(includeInactive: boolean = false): Observable<ExpenseCategory[]> {
    const params = new HttpParams().set('includeInactive', includeInactive.toString());
    return this.http.get<ExpenseCategory[]>(this.apiUrl, { params });
  }

  getById(id: number): Observable<ExpenseCategory> {
    return this.http.get<ExpenseCategory>(`${this.apiUrl}/${id}`);
  }

  create(dto: CreateExpenseCategoryDto): Observable<ExpenseCategory> {
    return this.http.post<ExpenseCategory>(this.apiUrl, dto);
  }

  update(id: number, dto: UpdateExpenseCategoryDto): Observable<ExpenseCategory> {
    return this.http.put<ExpenseCategory>(`${this.apiUrl}/${id}`, dto);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
