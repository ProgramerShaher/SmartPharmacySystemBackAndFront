import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, tap } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
    ApiResponse,
    PagedResult
} from '../models';
import {
    PharmacyAccount,
    FinancialTransaction,
    FinancialTransactionQueryDto,
    FinancialReport,
    CreateManualAdjustmentRequest,
    GeneralLedger,
    AnnualFinancialReport,
    FinancialSummary,
    GeneralLedgerQueryDto
} from '../models/financial.models';

@Injectable({ providedIn: 'root' })
export class FinancialService {
    private readonly apiUrl = `${environment.apiUrl}/Financial`;

    // Reactive Balance State
    private balanceSubject = new BehaviorSubject<number>(0);
    public balance$ = this.balanceSubject.asObservable();

    constructor(private http: HttpClient) {
        // Initial fetch of balance
        this.refreshBalance();
    }

    /**
     * Fetch latest balance and update the reactive stream
     */
    refreshBalance() {
        this.getBalance().subscribe();
    }

    getBalance(): Observable<PharmacyAccount> {
        return this.http.get<ApiResponse<PharmacyAccount>>(`${this.apiUrl}/balance`)
            .pipe(
                map(res => {
                    if (res.data) {
                        this.balanceSubject.next(res.data.balance);
                    }
                    return res.data;
                }),
                catchError(this.handleError)
            );
    }

    getTransactions(query: FinancialTransactionQueryDto): Observable<PagedResult<FinancialTransaction>> {
        let params = new HttpParams();
        if (query.type) params = params.set('type', query.type.toString());
        if (query.startDate) params = params.set('startDate', query.startDate);
        if (query.endDate) params = params.set('endDate', query.endDate);
        if (query.page) params = params.set('page', query.page.toString());
        if (query.pageSize) params = params.set('pageSize', query.pageSize.toString());
        if (query.search) params = params.set('search', query.search);

        return this.http.get<ApiResponse<PagedResult<FinancialTransaction>>>(`${this.apiUrl}/transactions`, { params })
            .pipe(map(res => res.data));
    }

    getReport(start?: string, end?: string): Observable<FinancialReport> {
        let params = new HttpParams();
        if (start) params = params.set('start', start);
        if (end) params = params.set('end', end);

        return this.http.get<ApiResponse<FinancialReport>>(`${this.apiUrl}/report`, { params })
            .pipe(map(res => res.data));
    }

    recordManualAdjustment(request: CreateManualAdjustmentRequest): Observable<FinancialTransaction> {
        return this.http.post<ApiResponse<FinancialTransaction>>(`${this.apiUrl}/manual-adjustment`, request)
            .pipe(
                map(res => res.data),
                tap(() => this.refreshBalance()), // Auto-update balance
                catchError(this.handleError)
            );
    }

    getGeneralLedger(query: GeneralLedgerQueryDto): Observable<PagedResult<GeneralLedger>> {
        let params = new HttpParams();
        if (query.start) params = params.set('start', query.start);
        if (query.end) params = params.set('end', query.end);
        if (query.page) params = params.set('page', query.page.toString());
        if (query.pageSize) params = params.set('pageSize', query.pageSize.toString());

        return this.http.get<ApiResponse<PagedResult<GeneralLedger>>>(`${this.apiUrl}/general-ledger`, { params })
            .pipe(map(res => res.data));
    }

    getAnnualReport(year: number): Observable<AnnualFinancialReport[]> {
        return this.http.get<ApiResponse<AnnualFinancialReport[]>>(`${this.apiUrl}/annual-report/${year}`)
            .pipe(map(res => res.data));
    }

    getAnnualSummary(year: number): Observable<FinancialSummary[]> {
        return this.http.get<ApiResponse<FinancialSummary[]>>(`${this.apiUrl}/annual-summary/${year}`)
            .pipe(map(res => res.data));
    }

    private handleError(error: any): Observable<never> {
        console.error('Financial Service Error:', error);
        throw error;
    }
}
