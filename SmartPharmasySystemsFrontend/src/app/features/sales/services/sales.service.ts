import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { ApiResponse, SaleInvoice, SaleInvoiceDetail, SalesReturn, SalesReturnDetail } from '../../../core/models';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class SalesService {
    constructor(private http: HttpClient) { }

    // --- SaleInvoice Service Spec ---
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

    // --- SaleInvoiceDetail Service Spec ---
    getAllDetails(): Observable<SaleInvoiceDetail[]> {
        return this.http.get<ApiResponse<SaleInvoiceDetail[]>>(`${environment.apiUrl}/SalesInvoiceDetails`).pipe(map(res => res.data));
    }
    getDetailById(id: number): Observable<SaleInvoiceDetail> {
        return this.http.get<ApiResponse<SaleInvoiceDetail>>(`${environment.apiUrl}/SalesInvoiceDetails/${id}`).pipe(map(res => res.data));
    }
    createDetail(detail: Partial<SaleInvoiceDetail>): Observable<SaleInvoiceDetail> {
        return this.http.post<ApiResponse<SaleInvoiceDetail>>(`${environment.apiUrl}/SalesInvoiceDetails`, detail).pipe(map(res => res.data));
    }
    updateDetail(id: number, detail: Partial<SaleInvoiceDetail>): Observable<SaleInvoiceDetail> {
        return this.http.put<ApiResponse<SaleInvoiceDetail>>(`${environment.apiUrl}/SalesInvoiceDetails/${id}`, detail).pipe(map(res => res.data));
    }
    getDetailsByInvoiceId(invoiceId: number): Observable<SaleInvoiceDetail[]> {
        return this.http.get<ApiResponse<SaleInvoiceDetail[]>>(`${environment.apiUrl}/SalesInvoiceDetails`, { params: { saleInvoiceId: invoiceId.toString() } }).pipe(map(res => res.data));
    }
    deleteDetail(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${environment.apiUrl}/SalesInvoiceDetails/${id}`).pipe(map(res => res.data));
    }

    // --- SalesReturn Service Spec ---
    getAllReturns(search?: string): Observable<SalesReturn[]> {
        return this.http.get<ApiResponse<SalesReturn[]>>(`${environment.apiUrl}/SalesReturns`, { params: search ? { search } : {} }).pipe(map(res => res.data));
    }
    getReturnById(id: number): Observable<SalesReturn> {
        return this.http.get<ApiResponse<SalesReturn>>(`${environment.apiUrl}/SalesReturns/${id}`).pipe(map(res => res.data));
    }
    createReturn(returnItem: Partial<SalesReturn>): Observable<SalesReturn> {
        return this.http.post<ApiResponse<SalesReturn>>(`${environment.apiUrl}/SalesReturns`, returnItem).pipe(map(res => res.data));
    }
    updateReturn(id: number, returnItem: Partial<SalesReturn>): Observable<SalesReturn> {
        return this.http.put<ApiResponse<SalesReturn>>(`${environment.apiUrl}/SalesReturns/${id}`, returnItem).pipe(map(res => res.data));
    }
    deleteReturn(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${environment.apiUrl}/SalesReturns/${id}`).pipe(map(res => res.data));
    }

    // --- SalesReturnDetail Service Spec ---
    getAllReturnDetails(): Observable<SalesReturnDetail[]> {
        return this.http.get<ApiResponse<SalesReturnDetail[]>>(`${environment.apiUrl}/SalesReturnDetails`).pipe(map(res => res.data));
    }
    getReturnDetailById(id: number): Observable<SalesReturnDetail> {
        return this.http.get<ApiResponse<SalesReturnDetail>>(`${environment.apiUrl}/SalesReturnDetails/${id}`).pipe(map(res => res.data));
    }
    createReturnDetail(detail: Partial<SalesReturnDetail>): Observable<SalesReturnDetail> {
        return this.http.post<ApiResponse<SalesReturnDetail>>(`${environment.apiUrl}/SalesReturnDetails`, detail).pipe(map(res => res.data));
    }
    updateReturnDetail(id: number, detail: Partial<SalesReturnDetail>): Observable<SalesReturnDetail> {
        return this.http.put<ApiResponse<SalesReturnDetail>>(`${environment.apiUrl}/SalesReturnDetails/${id}`, detail).pipe(map(res => res.data));
    }
    deleteReturnDetail(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${environment.apiUrl}/SalesReturnDetails/${id}`).pipe(map(res => res.data));
    }
}
