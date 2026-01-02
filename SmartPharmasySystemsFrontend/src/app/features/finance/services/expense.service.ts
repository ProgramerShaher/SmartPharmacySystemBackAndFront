import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { 
  ApiResponse, 
  ExpenseDto, 
  CreateExpenseDto,
  UpdateExpenseDto,
  ExpenseQueryDto, 
  PagedResult 
} from '../../../core/models';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ExpenseService {
  private readonly baseUrl = `${environment.apiUrl}/Expenses`;

  constructor(private http: HttpClient) { }

  /**
   * Search and paginate expenses with filters
   */
  search(query: ExpenseQueryDto): Observable<PagedResult<ExpenseDto>> {
    return this.http
      .get<ApiResponse<PagedResult<ExpenseDto>>>(this.baseUrl, { 
        params: this.buildQueryParams(query) 
      })
      .pipe(map(res => res.data));
  }

  /**
   * Get expense by ID
   */
  getById(id: number): Observable<ExpenseDto> {
    return this.http
      .get<ApiResponse<ExpenseDto>>(`${this.baseUrl}/${id}`)
      .pipe(map(res => res.data));
  }

  /**
   * Create new expense
   */
  create(dto: CreateExpenseDto): Observable<ExpenseDto> {
    return this.http
      .post<ApiResponse<ExpenseDto>>(this.baseUrl, dto)
      .pipe(map(res => res.data));
  }

  /**
   * Update existing expense
   */
  update(id: number, dto: UpdateExpenseDto): Observable<ExpenseDto> {
    return this.http
      .put<ApiResponse<ExpenseDto>>(`${this.baseUrl}/${id}`, dto)
      .pipe(map(res => res.data));
  }

  /**
   * Delete expense (soft delete)
   */
  delete(id: number): Observable<void> {
    return this.http
      .delete<ApiResponse<void>>(`${this.baseUrl}/${id}`)
      .pipe(map(res => res.data));
  }

  /**
   * Build query parameters for HTTP request
   */
  private buildQueryParams(query: ExpenseQueryDto): any {
    const params: any = {};
    
    if (query.search) params.search = query.search;
    if (query.page) params.page = query.page.toString();
    if (query.pageSize) params.pageSize = query.pageSize.toString();
    if (query.sortBy) params.sortBy = query.sortBy;
    if (query.sortDirection) params.sortDirection = query.sortDirection;
    if (query.fromDate) params.fromDate = query.fromDate;
    if (query.toDate) params.toDate = query.toDate;
    if (query.categoryId) params.categoryId = query.categoryId.toString();
    if (query.paymentMethod !== undefined) params.paymentMethod = query.paymentMethod.toString();
    if (query.isPaid !== undefined) params.isPaid = query.isPaid.toString();
    
    return params;
  }
}
