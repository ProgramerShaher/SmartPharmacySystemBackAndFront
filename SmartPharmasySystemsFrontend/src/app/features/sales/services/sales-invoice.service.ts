import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { ApiResponse, SaleInvoice, SaleInvoiceCreateDto, SaleInvoiceUpdateDto, SaleInvoiceDetail } from '../../../core/models';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class SalesInvoiceService {
    constructor(private http: HttpClient) { }

    getAll(search?: string): Observable<SaleInvoice[]> {
        return this.http.get<ApiResponse<SaleInvoice[]>>(`${environment.apiUrl}/SalesInvoices`, { params: search ? { search } : {} }).pipe(map(res => res.data));
    }

    getById(id: number): Observable<SaleInvoice> {
        return this.http.get<ApiResponse<SaleInvoice>>(`${environment.apiUrl}/SalesInvoices/${id}`).pipe(map(res => res.data));
    }

    create(invoice: SaleInvoiceCreateDto): Observable<SaleInvoice> {
        return this.http.post<ApiResponse<SaleInvoice>>(`${environment.apiUrl}/SalesInvoices`, invoice).pipe(map(res => res.data));
    }

    update(id: number, invoice: SaleInvoiceUpdateDto): Observable<SaleInvoice> {
        return this.http.put<ApiResponse<SaleInvoice>>(`${environment.apiUrl}/SalesInvoices/${id}`, invoice).pipe(map(res => res.data));
    }

    delete(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${environment.apiUrl}/SalesInvoices/${id}`).pipe(map(res => res.data));
    }

    approve(id: number): Observable<void> {
        return this.http.post<ApiResponse<void>>(`${environment.apiUrl}/SalesInvoices/${id}/approve`, {}).pipe(map(res => res.data));
    }

    cancel(id: number): Observable<void> {
        return this.http.post<ApiResponse<void>>(`${environment.apiUrl}/SalesInvoices/${id}/cancel`, {}).pipe(map(res => res.data));
    }

    // --- Details ---
    getDetailsByInvoiceId(invoiceId: number): Observable<SaleInvoiceDetail[]> {
        return this.http.get<ApiResponse<SaleInvoiceDetail[]>>(`${environment.apiUrl}/SalesInvoiceDetails`, { params: { saleInvoiceId: invoiceId.toString() } }).pipe(map(res => res.data));
    }

    createDetail(detail: any): Observable<SaleInvoiceDetail> {
        return this.http.post<ApiResponse<SaleInvoiceDetail>>(`${environment.apiUrl}/SalesInvoiceDetails`, detail).pipe(map(res => res.data));
    }

    updateDetail(id: number, detail: any): Observable<void> {
        return this.http.put<ApiResponse<void>>(`${environment.apiUrl}/SalesInvoiceDetails/${id}`, detail).pipe(map(res => res.data));
    }

    deleteDetail(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${environment.apiUrl}/SalesInvoiceDetails/${id}`).pipe(map(res => res.data));
    }
}
