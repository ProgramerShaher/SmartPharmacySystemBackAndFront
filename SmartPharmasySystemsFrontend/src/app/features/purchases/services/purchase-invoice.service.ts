import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { ApiResponse, PurchaseInvoice, PurchaseInvoiceCreateDto, PurchaseInvoiceUpdateDto, PurchaseInvoiceDetail, PurchaseReturn } from '../../../core/models';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class PurchaseInvoiceService {
    constructor(private http: HttpClient) { }

    getAll(search?: string): Observable<PurchaseInvoice[]> {
        return this.http.get<ApiResponse<PurchaseInvoice[]>>(`${environment.apiUrl}/PurchaseInvoices`, { params: search ? { search } : {} }).pipe(map(res => res.data));
    }

    getById(id: number): Observable<PurchaseInvoice> {
        return this.http.get<ApiResponse<PurchaseInvoice>>(`${environment.apiUrl}/PurchaseInvoices/${id}`).pipe(map(res => res.data));
    }

    create(invoice: PurchaseInvoiceCreateDto): Observable<PurchaseInvoice> {
        return this.http.post<ApiResponse<PurchaseInvoice>>(`${environment.apiUrl}/PurchaseInvoices`, invoice).pipe(map(res => res.data));
    }

    update(id: number, invoice: any): Observable<PurchaseInvoice> {
        return this.http.put<ApiResponse<PurchaseInvoice>>(`${environment.apiUrl}/PurchaseInvoices/${id}`, invoice).pipe(map(res => res.data));
    }

    delete(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${environment.apiUrl}/PurchaseInvoices/${id}`).pipe(map(res => res.data));
    }

    approve(id: number): Observable<PurchaseInvoice> {
        return this.http.post<ApiResponse<PurchaseInvoice>>(`${environment.apiUrl}/PurchaseInvoices/${id}/approve`, {}).pipe(map(res => res.data));
    }

    cancel(id: number): Observable<PurchaseInvoice> {
        return this.http.post<ApiResponse<PurchaseInvoice>>(`${environment.apiUrl}/PurchaseInvoices/${id}/cancel`, {}).pipe(map(res => res.data));
    }

    // --- Detail Operations ---
    getAllDetails(): Observable<PurchaseInvoiceDetail[]> {
        return this.http.get<ApiResponse<PurchaseInvoiceDetail[]>>(`${environment.apiUrl}/PurchaseInvoiceDetails`).pipe(map(res => res.data));
    }

    createDetail(detail: any): Observable<PurchaseInvoiceDetail> {
        return this.http.post<ApiResponse<PurchaseInvoiceDetail>>(`${environment.apiUrl}/PurchaseInvoiceDetails`, detail).pipe(map(res => res.data));
    }

    updateDetail(id: number, detail: any): Observable<void> {
        return this.http.put<ApiResponse<void>>(`${environment.apiUrl}/PurchaseInvoiceDetails/${id}`, detail).pipe(map(res => res.data));
    }

    deleteDetail(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${environment.apiUrl}/PurchaseInvoiceDetails/${id}`).pipe(map(res => res.data));
    }

    // --- Return Operations ---
    getAllReturns(): Observable<PurchaseReturn[]> {
        return this.http.get<ApiResponse<PurchaseReturn[]>>(`${environment.apiUrl}/PurchaseReturns`).pipe(map(res => res.data));
    }

    getReturnById(id: number): Observable<PurchaseReturn> {
        return this.http.get<ApiResponse<PurchaseReturn>>(`${environment.apiUrl}/PurchaseReturns/${id}`).pipe(map(res => res.data));
    }

    createReturn(ret: any): Observable<PurchaseReturn> {
        return this.http.post<ApiResponse<PurchaseReturn>>(`${environment.apiUrl}/PurchaseReturns`, ret).pipe(map(res => res.data));
    }

    createReturnDetail(detail: any): Observable<any> {
        return this.http.post<ApiResponse<any>>(`${environment.apiUrl}/PurchaseReturnDetails`, detail).pipe(map(res => res.data));
    }
}
