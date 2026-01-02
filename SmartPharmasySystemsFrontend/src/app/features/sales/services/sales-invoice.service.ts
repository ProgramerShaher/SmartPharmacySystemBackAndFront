import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import {
    ApiResponse,
    SaleInvoice,
    CreateSaleInvoiceDto,
    UpdateSaleInvoiceDto
} from '../../../core/models';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class SaleInvoiceService {
    private readonly apiUrl = `${environment.apiUrl}/SalesInvoices`;

    constructor(private http: HttpClient) { }

    getAll(search?: string): Observable<SaleInvoice[]> {
        let params = new HttpParams();
        if (search) params = params.set('search', search);

        return this.http.get<ApiResponse<SaleInvoice[]>>(this.apiUrl, { params }).pipe(
            map(res => res.data)
        );
    }

    getById(id: number): Observable<SaleInvoice> {
        return this.http.get<ApiResponse<SaleInvoice>>(`${this.apiUrl}/${id}`).pipe(
            map(res => res.data)
        );
    }

    create(invoice: CreateSaleInvoiceDto): Observable<SaleInvoice> {
        return this.http.post<ApiResponse<SaleInvoice>>(this.apiUrl, invoice).pipe(
            map(res => res.data)
        );
    }

    update(id: number, invoice: UpdateSaleInvoiceDto): Observable<SaleInvoice> {
        return this.http.put<ApiResponse<SaleInvoice>>(`${this.apiUrl}/${id}`, invoice).pipe(
            map(res => res.data)
        );
    }

    approve(id: number): Observable<void> {
        return this.http.post<ApiResponse<void>>(`${this.apiUrl}/${id}/approve`, {}).pipe(
            map(res => res.data)
        );
    }

    unapprove(id: number): Observable<void> {
        return this.http.post<ApiResponse<void>>(`${this.apiUrl}/${id}/unapprove`, {}).pipe(
            map(res => res.data)
        );
    }

    cancel(id: number): Observable<void> {
        return this.http.post<ApiResponse<void>>(`${this.apiUrl}/${id}/cancel`, {}).pipe(
            map(res => res.data)
        );
    }

    delete(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`).pipe(
            map(res => res.data)
        );
    }
}
