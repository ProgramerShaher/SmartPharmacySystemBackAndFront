import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { ApiResponse, PurchaseInvoice, PurchaseInvoiceDetail, PurchaseReturn, PurchaseReturnDetail, PurchaseInvoiceCreateDto, PurchaseInvoiceUpdateDto, PurchaseInvoiceDetailCreateDto, PurchaseInvoiceDetailUpdateDto } from '../../../core/models';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class PurchasesService {
    constructor(private http: HttpClient) { }

    // --- PurchaseInvoice Service Spec ---
    getAll(search?: string): Observable<PurchaseInvoice[]> {
        return this.http.get<ApiResponse<PurchaseInvoice[]>>(`${environment.apiUrl}/PurchaseInvoices`, { params: search ? { search } : {} }).pipe(map(res => res.data));
    }
    getById(id: number): Observable<PurchaseInvoice> {
        return this.http.get<ApiResponse<PurchaseInvoice>>(`${environment.apiUrl}/PurchaseInvoices/${id}`).pipe(map(res => res.data));
    }
    create(invoice: Partial<PurchaseInvoice>): Observable<PurchaseInvoice> {
        return this.http.post<ApiResponse<PurchaseInvoice>>(`${environment.apiUrl}/PurchaseInvoices`, invoice).pipe(map(res => res.data));
    }
    update(id: number, invoice: Partial<PurchaseInvoice>): Observable<PurchaseInvoice> {
        return this.http.put<ApiResponse<PurchaseInvoice>>(`${environment.apiUrl}/PurchaseInvoices/${id}`, invoice).pipe(map(res => res.data));
    }
    delete(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${environment.apiUrl}/PurchaseInvoices/${id}`).pipe(map(res => res.data));
    }
    approve(id: number): Observable<PurchaseInvoice> {
        return this.http.put<ApiResponse<PurchaseInvoice>>(`${environment.apiUrl}/PurchaseInvoices/${id}/approve`, {}).pipe(map(res => res.data));
    }
    cancel(id: number): Observable<PurchaseInvoice> {
        return this.http.put<ApiResponse<PurchaseInvoice>>(`${environment.apiUrl}/PurchaseInvoices/${id}/cancel`, {}).pipe(map(res => res.data));
    }

    // --- PurchaseInvoiceDetail Service Spec ---
    getAllDetails(): Observable<PurchaseInvoiceDetail[]> {
        return this.http.get<ApiResponse<PurchaseInvoiceDetail[]>>(`${environment.apiUrl}/PurchaseInvoiceDetails`).pipe(map(res => res.data));
    }
    getDetailById(id: number): Observable<PurchaseInvoiceDetail> {
        return this.http.get<ApiResponse<PurchaseInvoiceDetail>>(`${environment.apiUrl}/PurchaseInvoiceDetails/${id}`).pipe(map(res => res.data));
    }
    createDetail(detail: Partial<PurchaseInvoiceDetail>): Observable<PurchaseInvoiceDetail> {
        return this.http.post<ApiResponse<PurchaseInvoiceDetail>>(`${environment.apiUrl}/PurchaseInvoiceDetails`, detail).pipe(map(res => res.data));
    }
    updateDetail(id: number, detail: Partial<PurchaseInvoiceDetail>): Observable<PurchaseInvoiceDetail> {
        return this.http.put<ApiResponse<PurchaseInvoiceDetail>>(`${environment.apiUrl}/PurchaseInvoiceDetails/${id}`, detail).pipe(map(res => res.data));
    }
    deleteDetail(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${environment.apiUrl}/PurchaseInvoiceDetails/${id}`).pipe(map(res => res.data));
    }

    // --- PurchaseReturn Service Spec ---
    getAllReturns(search?: string): Observable<PurchaseReturn[]> {
        return this.http.get<ApiResponse<PurchaseReturn[]>>(`${environment.apiUrl}/PurchaseReturns`, { params: search ? { search } : {} }).pipe(map(res => res.data));
    }
    getReturnById(id: number): Observable<PurchaseReturn> {
        return this.http.get<ApiResponse<PurchaseReturn>>(`${environment.apiUrl}/PurchaseReturns/${id}`).pipe(map(res => res.data));
    }
    createReturn(returnItem: Partial<PurchaseReturn>): Observable<PurchaseReturn> {
        return this.http.post<ApiResponse<PurchaseReturn>>(`${environment.apiUrl}/PurchaseReturns`, returnItem).pipe(map(res => res.data));
    }
    updateReturn(id: number, returnItem: Partial<PurchaseReturn>): Observable<PurchaseReturn> {
        return this.http.put<ApiResponse<PurchaseReturn>>(`${environment.apiUrl}/PurchaseReturns/${id}`, returnItem).pipe(map(res => res.data));
    }
    deleteReturn(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${environment.apiUrl}/PurchaseReturns/${id}`).pipe(map(res => res.data));
    }

    // --- PurchaseReturnDetail Service Spec ---
    getAllReturnDetails(): Observable<PurchaseReturnDetail[]> {
        return this.http.get<ApiResponse<PurchaseReturnDetail[]>>(`${environment.apiUrl}/PurchaseReturnDetails`).pipe(map(res => res.data));
    }
    getReturnDetailById(id: number): Observable<PurchaseReturnDetail> {
        return this.http.get<ApiResponse<PurchaseReturnDetail>>(`${environment.apiUrl}/PurchaseReturnDetails/${id}`).pipe(map(res => res.data));
    }
    createReturnDetail(detail: Partial<PurchaseReturnDetail>): Observable<PurchaseReturnDetail> {
        return this.http.post<ApiResponse<PurchaseReturnDetail>>(`${environment.apiUrl}/PurchaseReturnDetails`, detail).pipe(map(res => res.data));
    }
    updateReturnDetail(id: number, detail: Partial<PurchaseReturnDetail>): Observable<PurchaseReturnDetail> {
        return this.http.put<ApiResponse<PurchaseReturnDetail>>(`${environment.apiUrl}/PurchaseReturnDetails/${id}`, detail).pipe(map(res => res.data));
    }
    deleteReturnDetail(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${environment.apiUrl}/PurchaseReturnDetails/${id}`).pipe(map(res => res.data));
    }
}
