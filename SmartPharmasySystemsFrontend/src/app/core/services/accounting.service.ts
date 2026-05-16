import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedResult } from '../models';
import {
    AccountDto,
    CreateAccountDto,
    JournalEntryDto,
    TrialBalanceDto,
    IncomeStatementDto,
    BalanceSheetDto,
    LedgerReportDto
} from '../models/accounting.interface';

@Injectable({
    providedIn: 'root'
})
export class AccountingService {
    private readonly accountsApi = `${environment.apiUrl}/Accounts`;
    private readonly journalApi = `${environment.apiUrl}/JournalEntries`;
    private readonly reportsApi = `${environment.apiUrl}/Reports`;

    constructor(private http: HttpClient) { }

    // --- Chart of Accounts ---
    getAccountsTree(): Observable<AccountDto[]> {
        return this.http.get<ApiResponse<AccountDto[]>>(`${this.accountsApi}/tree`)
            .pipe(map(res => res.data));
    }

    getAccountById(id: number): Observable<AccountDto> {
        return this.http.get<ApiResponse<AccountDto>>(`${this.accountsApi}/${id}`)
            .pipe(map(res => res.data));
    }

    createAccount(dto: CreateAccountDto): Observable<AccountDto> {
        return this.http.post<ApiResponse<AccountDto>>(this.accountsApi, dto)
            .pipe(map(res => res.data));
    }

    toggleAccountStatus(id: number, isActive: boolean): Observable<ApiResponse<void>> {
        return this.http.patch<ApiResponse<void>>(`${this.accountsApi}/${id}/toggle-status`, isActive);
    }

    deleteAccount(id: number): Observable<ApiResponse<void>> {
        return this.http.delete<ApiResponse<void>>(`${this.accountsApi}/${id}`);
    }

    getAccountLedger(accountId: number, startDate: string, endDate: string): Observable<LedgerReportDto> {
        const params = new HttpParams()
            .set('startDate', startDate)
            .set('endDate', endDate);
        return this.http.get<ApiResponse<LedgerReportDto>>(`${this.accountsApi}/${accountId}/ledger`, { params })
            .pipe(map(res => res.data));
    }

    getAllLedgers(startDate: string, endDate: string): Observable<LedgerReportDto[]> {
        const params = new HttpParams()
            .set('startDate', startDate)
            .set('endDate', endDate);
        return this.http.get<ApiResponse<LedgerReportDto[]>>(`${this.accountsApi}/all-ledgers`, { params })
            .pipe(map(res => res.data));
    }

    // --- Journal Entries ---
    getJournalEntries(page: number = 1, pageSize: number = 10, startDate?: string, endDate?: string, status?: string): Observable<PagedResult<JournalEntryDto>> {
        let params = new HttpParams()
            .set('page', page.toString())
            .set('pageSize', pageSize.toString());

        if (startDate) params = params.set('startDate', startDate);
        if (endDate) params = params.set('endDate', endDate);
        if (status) params = params.set('status', status);

        return this.http.get<ApiResponse<PagedResult<JournalEntryDto>>>(this.journalApi, { params })
            .pipe(map(res => res.data));
    }

    createJournalEntry(dto: JournalEntryDto): Observable<JournalEntryDto> {
        return this.http.post<ApiResponse<JournalEntryDto>>(this.journalApi, dto)
            .pipe(map(res => res.data));
    }

    postJournalEntry(id: number): Observable<void> {
        return this.http.post<ApiResponse<void>>(`${this.journalApi}/${id}/post`, {})
            .pipe(map(() => void 0));
    }

    // --- Financial Reports ---
    getTrialBalance(fromDate: string, toDate: string): Observable<TrialBalanceDto> {
        const params = new HttpParams()
            .set('fromDate', fromDate)
            .set('toDate', toDate);
        return this.http.get<ApiResponse<TrialBalanceDto>>(`${this.reportsApi}/trial-balance`, { params })
            .pipe(map(res => res.data));
    }

    getIncomeStatement(fromDate: string, toDate: string): Observable<IncomeStatementDto> {
        const params = new HttpParams()
            .set('fromDate', fromDate)
            .set('toDate', toDate);
        return this.http.get<ApiResponse<IncomeStatementDto>>(`${this.reportsApi}/income-statement`, { params })
            .pipe(map(res => res.data));
    }

    getBalanceSheet(date: string): Observable<BalanceSheetDto> {
        const params = new HttpParams().set('date', date);
        return this.http.get<ApiResponse<BalanceSheetDto>>(`${this.reportsApi}/balance-sheet`, { params })
            .pipe(map(res => res.data));
    }
}
