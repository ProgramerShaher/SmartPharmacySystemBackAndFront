import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { ApiResponse, Expense, ExpenseQueryDto, PagedResult, Payment, PaymentCreateDto, PaymentUpdateDto, RevenueReport, ExpenseReport, ProfitLossReport, CashFlowReport, SaleInvoice } from '../../../core/models';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class FinanceService {
    constructor(private http: HttpClient) { }

    search(query: ExpenseQueryDto): Observable<PagedResult<Expense>> {
        return this.http.get<ApiResponse<PagedResult<Expense>>>(`${environment.apiUrl}/Expenses`, { params: query as any }).pipe(map(res => res.data));
    }
    getById(id: number): Observable<Expense> {
        return this.http.get<ApiResponse<Expense>>(`${environment.apiUrl}/Expenses/${id}`).pipe(map(res => res.data));
    }
    create(expense: Partial<Expense>): Observable<Expense> {
        return this.http.post<ApiResponse<Expense>>(`${environment.apiUrl}/Expenses`, expense).pipe(map(res => res.data));
    }
    update(id: number, expense: Partial<Expense>): Observable<Expense> {
        return this.http.put<ApiResponse<Expense>>(`${environment.apiUrl}/Expenses/${id}`, expense).pipe(map(res => res.data));
    }
    delete(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${environment.apiUrl}/Expenses/${id}`).pipe(map(res => res.data));
    }
}
