import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import {
    ApiResponse,
    PagedResult,
    StockMovementDto,
    StockMovementSummary,
    StockCardDto,
    StockMovementQueryDto
} from '../../../core/models';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class InventoryMovementService {
    private readonly apiUrl = `${environment.apiUrl}/Movements`;

    constructor(private http: HttpClient) { }

    getAll(query?: StockMovementQueryDto): Observable<PagedResult<StockMovementDto>> {
        let params = new HttpParams();

        if (query?.search) params = params.set('search', query.search);
        if (query?.medicineId) params = params.set('medicineId', query.medicineId.toString());
        if (query?.batchId) params = params.set('batchId', query.batchId.toString());
        if (query?.movementType !== undefined) params = params.set('movementType', query.movementType.toString());
        if (query?.referenceType !== undefined) params = params.set('referenceType', query.referenceType.toString());
        if (query?.createdBy) params = params.set('createdBy', query.createdBy.toString());
        if (query?.startDate) params = params.set('startDate', query.startDate);
        if (query?.endDate) params = params.set('endDate', query.endDate);
        if (query?.page) params = params.set('page', query.page.toString());
        if (query?.pageSize) params = params.set('pageSize', query.pageSize.toString());

        return this.http.get<ApiResponse<PagedResult<StockMovementDto>>>(this.apiUrl, { params })
            .pipe(map(response => response.data), catchError(this.handleError));
    }

    getById(id: number): Observable<StockMovementDto> {
        return this.http.get<ApiResponse<StockMovementDto>>(`${this.apiUrl}/${id}`)
            .pipe(map(response => response.data), catchError(this.handleError));
    }

    getSummary(): Observable<StockMovementSummary> {
        return this.http.get<ApiResponse<StockMovementSummary>>(`${this.apiUrl}/summary`)
            .pipe(map(response => response.data), catchError(this.handleError));
    }

    getStockCard(medicineId: number, batchId?: number): Observable<StockCardDto[]> {
        let params = new HttpParams().set('medicineId', medicineId.toString());
        if (batchId) params = params.set('batchId', batchId.toString());

        return this.http.get<ApiResponse<StockCardDto[]>>(`${this.apiUrl}/stock-card`, { params })
            .pipe(map(response => response.data), catchError(this.handleError));
    }

    getCurrentBalance(medicineId: number, batchId?: number): Observable<number> {
        let params = new HttpParams().set('medicineId', medicineId.toString());
        if (batchId) params = params.set('batchId', batchId.toString());

        return this.http.get<ApiResponse<number>>(`${this.apiUrl}/balance`, { params })
            .pipe(map(response => response.data), catchError(this.handleError));
    }

    createManualMovement(dto: {
        medicineId: number;
        batchId: number;
        quantity: number;
        type: number;
        reason: string;
        approvedBy: number;
    }): Observable<any> {
        return this.http.post<ApiResponse<any>>(`${this.apiUrl}/manual`, dto)
            .pipe(map(response => response.data), catchError(this.handleError));
    }

    getMovementTypeLabel(type: number | string): string {
        const typeNum = typeof type === 'string' ? parseInt(type) : type;
        switch (typeNum) {
            case 1: return 'توريد';
            case 2: return 'بيع';
            case 3: return 'مردود مشتريات';
            case 4: return 'مردود مبيعات';
            case 5: return 'تعديل مخزون';
            case 6: return 'تالف';
            case 7: return 'منتهي الصلاحية';
            default: return 'غير معروف';
        }
    }

    getMovementTypeSeverity(type: number | string): 'success' | 'info' | 'warning' | 'danger' | 'secondary' {
        const typeNum = typeof type === 'string' ? parseInt(type) : type;
        switch (typeNum) {
            case 1:
            case 4:
                return 'success';
            case 2:
            case 3:
                return 'info';
            case 5:
                return 'warning';
            case 6:
            case 7:
                return 'danger';
            default:
                return 'secondary';
        }
    }

    getReferenceTypeLabel(type: number | string): string {
        const typeNum = typeof type === 'string' ? parseInt(type) : type;
        switch (typeNum) {
            case 1: return 'فاتورة مشتريات';
            case 2: return 'فاتورة مبيعات';
            case 3: return 'مردود مشتريات';
            case 4: return 'مردود مبيعات';
            case 5: return 'يدوي';
            case 7: return 'رصيد افتتاحي';
            case 8: return 'تعديل يدوي';
            case 9: return 'سند صرف مورد';
            case 10: return 'سند قبض عميل';
            default: return 'غير معروف';
        }
    }

    private handleError(error: any): Observable<never> {
        console.error('Inventory Movement Service Error:', error);
        throw error;
    }
}
