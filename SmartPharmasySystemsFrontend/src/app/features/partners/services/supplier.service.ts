import { Injectable, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { ApiResponse, PagedResult } from '../../../core/models';
import {
    Supplier,
    SupplierPayment,
    CreateSupplierPaymentDto,
    SupplierStatement,
    SupplierQueryDto
} from '../../../core/models/supplier.models';

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
        if (query?.page) params = params.set('page', query.page.toString());
        if (query?.pageSize) params = params.set('pageSize', query.pageSize.toString());

        this.loading.set(true);
        return this.http.get<ApiResponse<PagedResult<Supplier>>>(this.apiUrl, { params })
            .pipe(
                map(res => res.data),
                tap(res => {
                    this.suppliers.set(res.items);
                    const total = res.items.reduce((acc, curr) => acc + (curr.balance || 0), 0);
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

        return this.http.get<ApiResponse<SupplierPayment[]>>(this.paymentsUrl, { params })
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
        return this.http.get<ApiResponse<SupplierStatement>>(`${this.paymentsUrl}/statement/${supplierId}`)
            .pipe(map(res => res.data));
    }

    // --- Smart Payment Helpers ---

    /**
     * Get unpaid credit invoices for a supplier to support "Smart Payment" distribution.
     * Uses PurchaseInvoiceService to fetch and filters client-side.
     */
    getUnpaidInvoices(supplierId: number): Observable<any[]> { // Using any[] to avoid circular dependency if model not exported here
        // Ideally we inject PurchaseInvoiceService, but to avoid circular deps if any, 
        // we might better make this call direct or use clean architecture.
        // For now, let's use a direct HTTP call to PurchaseInvoices with filters if possible, 
        // or inject the service if no cycle exists. 
        // Checking imports... PurchaseInvoiceService is not imported. Safe to inject?
        // Let's try direct HTTP to keep it decoupled from another service class if possible, 
        // OR better yet, let's inject HttpClient and call the endpoint manually to be safe.
        
        let params = new HttpParams()
            .set('supplierId', supplierId.toString())
            .set('status', '2') // Approved
            .set('paymentMethod', '1'); // Credit (Assuming Enum 1=Credit)
            
        return this.http.get<ApiResponse<any[]>>(`${environment.apiUrl}/PurchaseInvoices`, { params })
            .pipe(
                map(res => res.data || []),
                map(invoices => invoices.filter(inv => !inv.isPaid)) // Client-side filter for IsPaid if API doesn't support it
            );
    }

    /**
     * Check if the main vault (Account 1) has enough balance.
     */
    checkVaultBalance(amount: number): Observable<boolean> {
        return this.http.get<ApiResponse<any>>(`${environment.apiUrl}/Financial/accounts/1`)
            .pipe(map(res => (res.data.balance || 0) >= amount));
    }
}
