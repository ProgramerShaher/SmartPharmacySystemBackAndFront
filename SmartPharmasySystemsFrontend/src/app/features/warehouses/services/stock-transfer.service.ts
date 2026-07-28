import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiResponse } from '../../../core/models';
import { environment } from '../../../../environments/environment';

export interface StockTransferDto {
    id: number;
    transferCode: string;
    sourceWarehouseId: number;
    sourceWarehouseName: string;
    destinationWarehouseId: number;
    destinationWarehouseName: string;
    status: string | number;
    statusName: string;
    notes?: string;
    requestedByUserId: number;
    requestedByName: string;
    requestedAt: string;
    approvedByUserId?: number;
    approvedByName?: string;
    approvedAt?: string;
    dispatchedByUserId?: number;
    dispatchedByName?: string;
    dispatchedAt?: string;
    receivedByUserId?: number;
    receivedByName?: string;
    receivedAt?: string;
    items: StockTransferItemDto[];
}

export interface StockTransferItemDto {
    id: number;
    medicineId: number;
    medicineName: string;
    batchNumber: string;
    quantityRequested: number;
    quantityDispatched: number;
    quantityReceived?: number;
    expiryDate?: string | Date;
    differenceQuantity?: number;
    rejectionReason?: string;
}

export interface CreateStockTransferDto {
    sourceWarehouseId: number;
    destinationWarehouseId: number;
    notes?: string;
    items: CreateStockTransferItemDto[];
}

export interface CreateStockTransferItemDto {
    medicineId: number;
    batchNumber: string;
    quantityRequested: number;
    expiryDate?: string | Date;
}

export interface ReceiveStockTransferDto {
    items: ReceiveStockTransferItemDto[];
}

export interface ReceiveStockTransferItemDto {
    stockTransferItemId: number;
    quantityReceived: number;
    rejectionReason?: string;
}

@Injectable({ providedIn: 'root' })
export class StockTransferService {
    private readonly apiUrl = `${environment.apiUrl}/StockTransfers`;

    constructor(private http: HttpClient) { }

    getAll(paramsObj?: {
        sourceWarehouseId?: number;
        destinationWarehouseId?: number;
        status?: number;
        from?: string;
        to?: string;
    }): Observable<StockTransferDto[]> {
        let params = new HttpParams();
        if (paramsObj?.sourceWarehouseId) params = params.set('sourceWarehouseId', paramsObj.sourceWarehouseId.toString());
        if (paramsObj?.destinationWarehouseId) params = params.set('destinationWarehouseId', paramsObj.destinationWarehouseId.toString());
        if (paramsObj?.status !== undefined) params = params.set('status', paramsObj.status.toString());
        if (paramsObj?.from) params = params.set('from', paramsObj.from);
        if (paramsObj?.to) params = params.set('to', paramsObj.to);

        return this.http.get<ApiResponse<StockTransferDto[]>>(this.apiUrl, { params })
            .pipe(map(r => r.data || []));
    }

    getById(id: number): Observable<StockTransferDto> {
        return this.http.get<ApiResponse<StockTransferDto>>(`${this.apiUrl}/${id}`)
            .pipe(map(r => r.data));
    }

    getByCode(code: string): Observable<StockTransferDto> {
        return this.http.get<ApiResponse<StockTransferDto>>(`${this.apiUrl}/code/${code}`)
            .pipe(map(r => r.data));
    }

    getPendingApproval(): Observable<StockTransferDto[]> {
        return this.http.get<ApiResponse<StockTransferDto[]>>(`${this.apiUrl}/pending-approval`)
            .pipe(map(r => r.data || []));
    }

    getPendingDispatch(): Observable<StockTransferDto[]> {
        return this.http.get<ApiResponse<StockTransferDto[]>>(`${this.apiUrl}/pending-dispatch`)
            .pipe(map(r => r.data || []));
    }

    getPendingReceipt(warehouseId: number): Observable<StockTransferDto[]> {
        return this.http.get<ApiResponse<StockTransferDto[]>>(`${this.apiUrl}/pending-receipt/${warehouseId}`)
            .pipe(map(r => r.data || []));
    }

    create(dto: CreateStockTransferDto, userId: number): Observable<StockTransferDto> {
        return this.http.post<ApiResponse<StockTransferDto>>(`${this.apiUrl}?requestedByUserId=${userId}`, dto)
            .pipe(map(r => r.data));
    }

    approve(id: number, userId: number): Observable<StockTransferDto> {
        return this.http.put<ApiResponse<StockTransferDto>>(`${this.apiUrl}/${id}/approve?approvedByUserId=${userId}`, {})
            .pipe(map(r => r.data));
    }

    dispatch(id: number, userId: number): Observable<StockTransferDto> {
        return this.http.put<ApiResponse<StockTransferDto>>(`${this.apiUrl}/${id}/dispatch?dispatchedByUserId=${userId}`, {})
            .pipe(map(r => r.data));
    }

    receive(id: number, dto: ReceiveStockTransferDto, userId: number): Observable<StockTransferDto> {
        return this.http.put<ApiResponse<StockTransferDto>>(`${this.apiUrl}/${id}/receive?receivedByUserId=${userId}`, dto)
            .pipe(map(r => r.data));
    }

    delete(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`)
            .pipe(map(r => r.data));
    }
}
