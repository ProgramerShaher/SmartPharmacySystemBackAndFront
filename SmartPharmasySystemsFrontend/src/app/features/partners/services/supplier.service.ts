import { Injectable, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, tap, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
    ApiResponse,
    PagedResult,
    Supplier,
    SupplierPayment,
    CreateSupplierPaymentDto,
    SupplierStatement,
    SupplierQueryDto,
    PurchaseInvoice
} from '../../../core/models';

@Injectable({
    providedIn: 'root'
})
export class SupplierService {
    private readonly apiUrl = `${environment.apiUrl}/Suppliers`;
    private readonly paymentsUrl = `${environment.apiUrl}/SupplierPayments`;

    // Signals for state management
    public suppliers = signal<Supplier[]>([]);
    public totalSupplierDebt = signal(0);
    public loading = signal(false);

    constructor(private http: HttpClient) { }

    getAll(query?: SupplierQueryDto): Observable<PagedResult<Supplier>> {
        let params = new HttpParams();
        if (query?.search) params = params.set('search', query.search);

        // Pagination & Sorting (Critical for Performance)
        if (query?.page) params = params.set('page', query.page.toString());
        if (query?.pageSize) params = params.set('pageSize', query.pageSize.toString());
        if (query?.sortBy) params = params.set('sortBy', query.sortBy);
        if (query?.sortDir) params = params.set('sortDir', query.sortDir);

        this.loading.set(true);
        return this.http.get<ApiResponse<PagedResult<Supplier>>>(this.apiUrl, { params })
            .pipe(
                map(res => res.data),
                tap(res => {
                    this.suppliers.set(res.items);
                    // Use totalCount from backend if available, or calc local (less accurate for paged)
                    const total = res.items.reduce((acc, curr) => acc + (curr.Balance || 0), 0);
                    this.totalSupplierDebt.set(total);
                    this.loading.set(false);
                })
            );
    }

    getById(id: number): Observable<Supplier> {
        return this.http.get<ApiResponse<Supplier>>(`${this.apiUrl}/${id}`)
            .pipe(map(res => res.data));
    }

    create(supplier: Partial<Supplier>): Observable<Supplier> {
        return this.http.post<ApiResponse<Supplier>>(this.apiUrl, supplier)
            .pipe(map(res => res.data));
    }

    update(id: number, supplier: Partial<Supplier>): Observable<Supplier> {
        return this.http.put<ApiResponse<Supplier>>(`${this.apiUrl}/${id}`, supplier)
            .pipe(map(res => res.data));
    }

    delete(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`)
            .pipe(map(res => res.data));
    }

    // --- Supplier Payments ---

    getPayments(supplierId?: number): Observable<SupplierPayment[]> {
        let params = new HttpParams();
        if (supplierId) params = params.set('supplierId', supplierId.toString());

        return this.http.get<ApiResponse<SupplierPayment[]>>(`${this.paymentsUrl}/all`, { params })
            .pipe(map(res => res.data));
    }

    createPayment(dto: CreateSupplierPaymentDto): Observable<SupplierPayment> {
        return this.http.post<ApiResponse<SupplierPayment>>(this.paymentsUrl, dto)
            .pipe(map(res => res.data));
    }

    cancelPayment(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${this.paymentsUrl}/${id}/cancel`)
            .pipe(map(res => res.data));
    }

    // --- Supplier Statement ---

    getStatement(supplierId: number): Observable<SupplierStatement> {
        return this.http.get<ApiResponse<SupplierStatement>>(`${this.paymentsUrl}/supplier/${supplierId}/statement`)
            .pipe(map(res => res.data));
    }

    // https://localhost:7107/api/SupplierPayments/supplier/3/statement

    // --- Smart Payment Helpers ---

    /**
     * Get unpaid credit invoices for a supplier to support "Smart Payment" distribution.
     * Uses PurchaseInvoiceService to fetch and filters client-side.
     */
    getUnpaidInvoices(supplierId: number): Observable<PurchaseInvoice[]> {
        let params = new HttpParams()
            .set('supplierId', supplierId.toString())
            .set('status', '2') // Approved
            .set('paymentMethod', '2'); // Credit

        return this.http.get<ApiResponse<any>>(`${environment.apiUrl}/PurchaseInvoices`, { params })
            .pipe(
                map(res => {
                    const data = res.data;
                    const invoices: PurchaseInvoice[] = Array.isArray(data) ? data : (data?.items || []);
                    return invoices;
                }),
                map(invoices => invoices.filter(inv => {
                    const isCredit = inv.paymentMethod === 2 || inv.paymentMethod === 'Credit' as any;
                    const isApproved = inv.status === 2 || inv.status === 'Approved' as any;
                    return inv.supplierId === supplierId && isCredit && !inv.isPaid && isApproved;
                }))
            );
    }

    /**
     * Check if the main vault (Account 1) has enough balance.
     */
    checkVaultBalance(amount: number): Observable<boolean> {
        return this.http.get<ApiResponse<any>>(`${environment.apiUrl}/Financial/accounts/1`)
            .pipe(map(res => (res.data.balance || 0) >= amount));
    }

    /**
     * Get the total outstanding debt across ALL suppliers (not just the current page).
     * Fetches suppliers with a large pageSize to compute accurate totals.
     */
    getTotalDebt(): Observable<number> {
        const params = new HttpParams()
            .set('pageSize', '1000')
            .set('page', '1');
        return this.http.get<ApiResponse<PagedResult<Supplier>>>(this.apiUrl, { params })
            .pipe(
                map(res => {
                    const items = res.data?.items || [];
                    return items.reduce((sum, s) => sum + (s.Balance || 0), 0);
                }),
                catchError(() => of(0))
            );
    }
}
