import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class PrintService {
    private apiUrl = `${environment.apiUrl}/Print`;

    constructor(private http: HttpClient) { }

    /**
     * Print a thermal receipt (80mm) for an invoice.
     * Endpoints: POST /api/Print/invoice/{invoiceId}
     */
    printInvoice(invoiceId: number): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/invoice/${invoiceId}`, {});
    }

    /**
     * Print a receipt for a sales return.
     * Endpoints: POST /api/Print/return/{returnId}
     */
    printReturn(returnId: number): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/return/${returnId}`, {});
    }
}
