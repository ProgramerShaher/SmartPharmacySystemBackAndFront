import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { 
  ApiResponse, 
  ExpenseCategoryDto, 
  CreateExpenseCategoryDto,
  UpdateExpenseCategoryDto
} from '../../../core/models';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ExpenseCategoryService {
  private readonly baseUrl = `${environment.apiUrl}/ExpenseCategories`;

  constructor(private http: HttpClient) { }

  /**
   * Get all expense categories
   */
  getAll(): Observable<ExpenseCategoryDto[]> {
    return this.http
      .get<ApiResponse<ExpenseCategoryDto[]>>(this.baseUrl)
      .pipe(map(res => res.data));
  }

  /**
   * Create new expense category
   */
  create(dto: CreateExpenseCategoryDto): Observable<ExpenseCategoryDto> {
    return this.http
      .post<ApiResponse<ExpenseCategoryDto>>(this.baseUrl, dto)
      .pipe(map(res => res.data));
  }

  /**
   * Update existing expense category
   */
  update(id: number, dto: UpdateExpenseCategoryDto): Observable<void> {
    return this.http
      .put<ApiResponse<void>>(`${this.baseUrl}/${id}`, dto)
      .pipe(map(res => res.data));
  }

  /**
   * Delete expense category (soft delete)
   */
  delete(id: number): Observable<void> {
    return this.http
      .delete<ApiResponse<void>>(`${this.baseUrl}/${id}`)
      .pipe(map(res => res.data));
  }
}
