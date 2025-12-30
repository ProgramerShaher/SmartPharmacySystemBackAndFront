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

    // Financial Transactions & General Ledger
    getBalance(): Observable<import('../../../core/models').PharmacyAccount> {
        return this.http.get<ApiResponse<import('../../../core/models').PharmacyAccount>>(`${environment.apiUrl}/Financial/balance`).pipe(map(res => res.data));
    }

    getGeneralLedger(start?: string, end?: string, page: number = 1, pageSize: number = 50): Observable<PagedResult<import('../../../core/models').GeneralLedger>> {
        const params: any = { page, pageSize };
        if (start) params.start = start;
        if (end) params.end = end;
        return this.http.get<ApiResponse<PagedResult<import('../../../core/models').GeneralLedger>>>(`${environment.apiUrl}/Financial/general-ledger`, { params }).pipe(map(res => res.data));
    }

    getFinancialTransactions(query: import('../../../core/models').FinancialTransactionQuery): Observable<PagedResult<import('../../../core/models').FinancialTransaction>> {
        return this.http.get<ApiResponse<PagedResult<import('../../../core/models').FinancialTransaction>>>(`${environment.apiUrl}/Financial/transactions`, { params: query as any }).pipe(map(res => res.data));
    }

    getFinancialReport(start?: string, end?: string): Observable<import('../../../core/models').FinancialReport> {
        const params: any = {};
        if (start) params.start = start;
        if (end) params.end = end;
        return this.http.get<ApiResponse<import('../../../core/models').FinancialReport>>(`${environment.apiUrl}/Financial/report`, { params }).pipe(map(res => res.data));
    }
}
