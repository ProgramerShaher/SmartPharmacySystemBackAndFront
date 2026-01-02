import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import {
    ApiResponse,
    PurchaseReturn
} from '../../../core/models';
import { environment } from '../../../../environments/environment';

export interface CreatePurchaseReturnDto {
    purchaseInvoiceId: number;
    supplierId: number;
    returnDate: string; // ISO
    reason: string;
    details: CreatePurchaseReturnDetailDto[];
}

export interface CreatePurchaseReturnDetailDto {
    medicineId: number;
    batchId: number;
    quantity: number;
    purchasePrice: number;
}

export interface UpdatePurchaseReturnDto {
    id: number;
    returnDate: string;
    reason: string;
    details: CreatePurchaseReturnDetailDto[];
}

@Injectable({ providedIn: 'root' })
export class PurchaseReturnService {
    private readonly apiUrl = `${environment.apiUrl}/PurchaseReturns`;

    constructor(private http: HttpClient) { }

    getAll(query?: any): Observable<PurchaseReturn[]> {
        let params = new HttpParams();
        if (query) {
            Object.keys(query).forEach(key => {
                if (query[key] !== null && query[key] !== undefined) {
                    params = params.set(key, query[key].toString());
                }
            });
        }

        return this.http.get<ApiResponse<PurchaseReturn[]>>(this.apiUrl, { params })
            .pipe(
                map(res => res.data || []),
                catchError(this.handleError)
            );
    }

    getById(id: number): Observable<PurchaseReturn> {
        return this.http.get<ApiResponse<PurchaseReturn>>(`${this.apiUrl}/${id}`)
            .pipe(
                map(res => res.data),
                catchError(this.handleError)
            );
    }

    create(dto: CreatePurchaseReturnDto): Observable<PurchaseReturn> {
        return this.http.post<ApiResponse<PurchaseReturn>>(this.apiUrl, dto)
            .pipe(
                map(res => res.data),
                catchError(this.handleError)
            );
    }

    update(id: number, dto: UpdatePurchaseReturnDto): Observable<void> {
        return this.http.put<ApiResponse<void>>(`${this.apiUrl}/${id}`, dto)
            .pipe(
                map(res => res.data),
                catchError(this.handleError)
            );
    }

    delete(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`)
            .pipe(
                map(res => res.data),
                catchError(this.handleError)
            );
    }

    approve(id: number): Observable<void> {
        return this.http.post<ApiResponse<void>>(`${this.apiUrl}/${id}/approve`, {})
            .pipe(
                map(res => res.data),
                catchError(this.handleError)
            );
    }

    cancel(id: number): Observable<void> {
        return this.http.post<ApiResponse<void>>(`${this.apiUrl}/${id}/cancel`, {})
            .pipe(
                map(res => res.data),
                catchError(this.handleError)
            );
    }

    private handleError(error: any): Observable<never> {
        console.error('Purchase Return Service Error:', error);
        throw error;
    }
}
