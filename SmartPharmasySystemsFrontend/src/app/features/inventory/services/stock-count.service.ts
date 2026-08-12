import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/models';
import { 
    StockCountHeaderDto, 
    StockCountItemDto, 
    CreateStockCountHeaderDto, 
    CreateStockCountItemDto, 
    UpdateStockCountItemDto,
    StockCountStatus
} from '../../../core/models/stock-count.interface';

@Injectable({
    providedIn: 'root'
})
export class StockCountService {
    private readonly http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/StockCounts`;

    getAllHeaders(warehouseId?: number, status?: StockCountStatus, from?: string, to?: string): Observable<StockCountHeaderDto[]> {
        let params = new HttpParams();
        if (warehouseId) params = params.set('warehouseId', warehouseId);
        if (status) params = params.set('status', status);
        if (from) params = params.set('from', from);
        if (to) params = params.set('to', to);

        return this.http.get<ApiResponse<StockCountHeaderDto[]>>(this.apiUrl, { params }).pipe(
            map(response => response.data || [])
        );
    }

    getHeaderById(id: number): Observable<StockCountHeaderDto> {
        return this.http.get<ApiResponse<StockCountHeaderDto>>(`${this.apiUrl}/${id}`).pipe(
            map(response => response.data!)
        );
    }

    getPendingApproval(): Observable<StockCountHeaderDto[]> {
        return this.http.get<ApiResponse<StockCountHeaderDto[]>>(`${this.apiUrl}/pending`).pipe(
            map(response => response.data || [])
        );
    }

    getItems(headerId: number): Observable<StockCountItemDto[]> {
        return this.http.get<ApiResponse<StockCountItemDto[]>>(`${this.apiUrl}/${headerId}/items`).pipe(
            map(response => response.data || [])
        );
    }

    createHeader(dto: CreateStockCountHeaderDto): Observable<StockCountHeaderDto> {
        return this.http.post<ApiResponse<StockCountHeaderDto>>(this.apiUrl, dto).pipe(
            map(response => response.data!)
        );
    }

    addItem(headerId: number, dto: CreateStockCountItemDto): Observable<StockCountItemDto> {
        return this.http.post<ApiResponse<StockCountItemDto>>(`${this.apiUrl}/${headerId}/items`, dto).pipe(
            map(response => response.data!)
        );
    }

    updateItem(headerId: number, dto: UpdateStockCountItemDto): Observable<void> {
        return this.http.put<ApiResponse<void>>(`${this.apiUrl}/${headerId}/items`, dto).pipe(
            map(() => undefined)
        );
    }

    submitForApproval(headerId: number): Observable<StockCountHeaderDto> {
        return this.http.put<ApiResponse<StockCountHeaderDto>>(`${this.apiUrl}/${headerId}/submit`, {}).pipe(
            map(response => response.data!)
        );
    }

    approve(headerId: number, approvedByUserId: number): Observable<StockCountHeaderDto> {
        let params = new HttpParams().set('approvedByUserId', approvedByUserId);
        return this.http.put<ApiResponse<StockCountHeaderDto>>(`${this.apiUrl}/${headerId}/approve`, {}, { params }).pipe(
            map(response => response.data!)
        );
    }

    deleteHeader(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`).pipe(
            map(() => undefined)
        );
    }

    deleteItem(headerId: number, medicineId: number, batchNumber: string): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${headerId}/items/${medicineId}/${batchNumber}`).pipe(
            map(() => undefined)
        );
    }

    // ==========================================
    // Schedules
    // ==========================================

    getAllSchedules(warehouseId?: number): Observable<import('../../../core/models/stock-count.interface').StockCountScheduleDto[]> {
        let params = new HttpParams();
        if (warehouseId) params = params.set('warehouseId', warehouseId);

        return this.http.get<ApiResponse<import('../../../core/models/stock-count.interface').StockCountScheduleDto[]>>(`${this.apiUrl}/schedules`, { params }).pipe(
            map(response => response.data || [])
        );
    }

    createSchedule(dto: import('../../../core/models/stock-count.interface').CreateStockCountScheduleDto): Observable<import('../../../core/models/stock-count.interface').StockCountScheduleDto> {
        return this.http.post<ApiResponse<import('../../../core/models/stock-count.interface').StockCountScheduleDto>>(`${this.apiUrl}/schedules`, dto).pipe(
            map(response => response.data!)
        );
    }

    updateSchedule(id: number, dto: import('../../../core/models/stock-count.interface').UpdateStockCountScheduleDto): Observable<void> {
        return this.http.put<ApiResponse<void>>(`${this.apiUrl}/schedules/${id}`, dto).pipe(
            map(() => undefined)
        );
    }

    deleteSchedule(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/schedules/${id}`).pipe(
            map(() => undefined)
        );
    }
}
