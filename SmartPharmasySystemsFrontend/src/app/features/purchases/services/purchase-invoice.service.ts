import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import {
    ApiResponse,
    PurchaseInvoice,
    CreatePurchaseInvoiceDto,
    UpdatePurchaseInvoiceDto,
    PurchaseInvoiceQueryDto
} from '../../../core/models';
import { environment } from '../../../../environments/environment';

/**
 * Purchase Invoice API Service
 * Handles all HTTP operations for Purchase Invoices
 * 100% Backend Aligned with strict typing
 */
@Injectable({ providedIn: 'root' })
export class PurchaseInvoiceService {
    private readonly apiUrl = `${environment.apiUrl}/PurchaseInvoices`;

    constructor(private http: HttpClient) { }

    /**
     * Get all purchase invoices with optional search/filter
     * @param query Optional query parameters for filtering
     * @returns Observable<PurchaseInvoice[]>
     */
    getAll(query?: PurchaseInvoiceQueryDto): Observable<PurchaseInvoice[]> {
        let params = new HttpParams();

        if (query?.search) {
            params = params.set('search', query.search);
        }
        if (query?.supplierId) {
            params = params.set('supplierId', query.supplierId.toString());
        }
        if (query?.status) {
            params = params.set('status', query.status.toString());
        }
        if (query?.paymentMethod) {
            params = params.set('paymentMethod', query.paymentMethod.toString());
        }
        if (query?.dateFrom) {
            params = params.set('dateFrom', query.dateFrom);
        }
        if (query?.dateTo) {
            params = params.set('dateTo', query.dateTo);
        }
        if (query?.page) {
            params = params.set('page', query.page.toString());
        }
        if (query?.pageSize) {
            params = params.set('pageSize', query.pageSize.toString());
        }

        return this.http.get<ApiResponse<PurchaseInvoice[]>>(this.apiUrl, { params })
            .pipe(
                map(response => response.data || []),
                catchError(this.handleError)
            );
    }

    /**
     * Get purchase invoice by ID with full details
     * @param id Invoice ID
     * @returns Observable<PurchaseInvoice>
     */
    getById(id: number): Observable<PurchaseInvoice> {
        return this.http.get<ApiResponse<PurchaseInvoice>>(`${this.apiUrl}/${id}`)
            .pipe(
                map(response => response.data),
                catchError(this.handleError)
            );
    }

    /**
     * Create new purchase invoice with items
     * @param dto CreatePurchaseInvoiceDto with items array
     * @returns Observable<PurchaseInvoice>
     */
    create(dto: CreatePurchaseInvoiceDto): Observable<PurchaseInvoice> {
        return this.http.post<ApiResponse<PurchaseInvoice>>(this.apiUrl, dto)
            .pipe(
                map(response => response.data),
                catchError(this.handleError)
            );
    }

    /**
     * Update existing purchase invoice
     * Note: Only allowed if status is Draft
     * @param id Invoice ID
     * @param dto UpdatePurchaseInvoiceDto with items array
     * @returns Observable<PurchaseInvoice>
     */
    update(id: number, dto: UpdatePurchaseInvoiceDto): Observable<any> {
        return this.http.put<ApiResponse<any>>(`${this.apiUrl}/${id}`, dto)
            .pipe(
                map(response => response.data),
                catchError(this.handleError)
            );
    }

    /**
     * Approve purchase invoice
     * Triggers: Stock increase, Supplier balance update, Financial transaction
     * @param id Invoice ID
     * @returns Observable<any>
     */
    approve(id: number): Observable<any> {
        return this.http.post<ApiResponse<any>>(`${this.apiUrl}/${id}/approve`, {})
            .pipe(
                map(response => response.data),
                catchError(this.handleError)
            );
    }

    /**
     * Unapprove purchase invoice
     * Reverses: Stock, Supplier balance, Financial transaction
     * @param id Invoice ID
     * @returns Observable<any>
     */
    unapprove(id: number): Observable<any> {
        return this.http.post<ApiResponse<any>>(`${this.apiUrl}/${id}/unapprove`, {})
            .pipe(
                map(response => response.data),
                catchError(this.handleError)
            );
    }

    /**
     * Cancel purchase invoice
     * Reverses stock and financial transactions
     * @param id Invoice ID
     * @returns Observable<any>
     */
    cancel(id: number): Observable<any> {
        return this.http.post<ApiResponse<any>>(`${this.apiUrl}/${id}/cancel`, {})
            .pipe(
                map(response => response.data),
                catchError(this.handleError)
            );
    }

    /**
     * Delete purchase invoice (soft delete)
     * @param id Invoice ID
     * @returns Observable<void>
     */
    delete(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`)
            .pipe(
                map(response => response.data),
                catchError(this.handleError)
            );
    }

    /**
     * Error handler for HTTP operations
     * @param error HTTP error
     */
    private handleError(error: any): Observable<never> {
        console.error('Purchase Invoice Service Error:', error);
        throw error;
    }
}
