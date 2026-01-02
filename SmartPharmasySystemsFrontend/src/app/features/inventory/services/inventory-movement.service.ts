import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import {
    ApiResponse,
    PagedResult,
    StockMovementDto,
    StockCardDto,
    CreateManualMovementDto,
    StockMovementQueryDto
} from '../../../core/models';
import { environment } from '../../../../environments/environment';

/**
 * Inventory Movement Service
 * Handles all HTTP operations for stock movements
 */
@Injectable({ providedIn: 'root' })
export class InventoryMovementService {
    private readonly apiUrl = `${environment.apiUrl}/Movements`;

    constructor(private http: HttpClient) { }

    /**
     * Get all stock movements with optional filtering
     * @param query Query parameters for filtering
     * @returns Observable<PagedResult<StockMovementDto>>
     */
    getAll(query?: StockMovementQueryDto): Observable<PagedResult<StockMovementDto>> {
        let params = new HttpParams();

        if (query?.search) {
            params = params.set('search', query.search);
        }
        if (query?.medicineId) {
            params = params.set('medicineId', query.medicineId.toString());
        }
        if (query?.batchId) {
            params = params.set('batchId', query.batchId.toString());
        }
        if (query?.movementType !== undefined) {
            params = params.set('movementType', query.movementType.toString());
        }
        if (query?.referenceType !== undefined) {
            params = params.set('referenceType', query.referenceType.toString());
        }
        if (query?.startDate) {
            params = params.set('startDate', query.startDate);
        }
        if (query?.endDate) {
            params = params.set('endDate', query.endDate);
        }
        if (query?.page) {
            params = params.set('page', query.page.toString());
        }
        if (query?.pageSize) {
            params = params.set('pageSize', query.pageSize.toString());
        }

        return this.http.get<ApiResponse<PagedResult<StockMovementDto>>>(this.apiUrl, { params })
            .pipe(
                map(response => response.data),
                catchError(this.handleError)
            );
    }

    /**
     * Get stock movement by ID
     * @param id Movement ID
     * @returns Observable<StockMovementDto>
     */
    getById(id: number): Observable<StockMovementDto> {
        return this.http.get<ApiResponse<StockMovementDto>>(`${this.apiUrl}/${id}`)
            .pipe(
                map(response => response.data),
                catchError(this.handleError)
            );
    }

    /**
     * Get stock card (ledger) for a medicine
     * @param medicineId Medicine ID
     * @param batchId Optional batch ID
     * @returns Observable<StockCardDto[]>
     */
    getStockCard(medicineId: number, batchId?: number): Observable<StockCardDto[]> {
        let params = new HttpParams().set('medicineId', medicineId.toString());

        if (batchId) {
            params = params.set('batchId', batchId.toString());
        }

        return this.http.get<ApiResponse<StockCardDto[]>>(`${this.apiUrl}/stock-card`, { params })
            .pipe(
                map(response => response.data),
                catchError(this.handleError)
            );
    }

    /**
     * Get current balance for a medicine
     * @param medicineId Medicine ID
     * @param batchId Optional batch ID
     * @returns Observable<number>
     */
    getCurrentBalance(medicineId: number, batchId?: number): Observable<number> {
        let params = new HttpParams().set('medicineId', medicineId.toString());

        if (batchId) {
            params = params.set('batchId', batchId.toString());
        }

        return this.http.get<ApiResponse<number>>(`${this.apiUrl}/balance`, { params })
            .pipe(
                map(response => response.data),
                catchError(this.handleError)
            );
    }

    /**
     * Create manual stock movement (Adjustment/Damage/Expiry)
     * Requires Admin role
     * Backend expects: medicineId, batchId, quantity, type, reason, approvedBy
     * @param dto Manual movement data
     * @returns Observable<any>
     */
    createManualMovement(dto: {
        medicineId: number;
        batchId: number;
        quantity: number;
        type: number; // 5=Adjustment, 6=Damage, 7=Expiry
        reason: string;
        approvedBy: number;
    }): Observable<any> {
        return this.http.post<ApiResponse<any>>(`${this.apiUrl}/manual`, dto)
            .pipe(
                map(response => response.data),
                catchError(this.handleError)
            );
    }

    /**
     * Get movement type label in Arabic
     */
    getMovementTypeLabel(type: number | string): string {
        const typeNum = typeof type === 'string' ? parseInt(type) : type;
        switch (typeNum) {
            case 1: return 'توريد';
            case 2: return 'بيع';
            case 3: return 'مرتجع مشتريات';
            case 4: return 'مرتجع مبيعات';
            case 5: return 'تعديل';
            case 6: return 'تلف';
            case 7: return 'منتهي الصلاحية';
            default: return 'غير معروف';
        }
    }

    /**
     * Get movement type severity for PrimeNG tags
     */
    getMovementTypeSeverity(type: number | string): 'success' | 'info' | 'warning' | 'danger' | 'secondary' {
        const typeNum = typeof type === 'string' ? parseInt(type) : type;
        switch (typeNum) {
            case 1: // Purchase
            case 4: // Sales Return
                return 'success';
            case 2: // Sale
            case 3: // Purchase Return
                return 'info';
            case 5: // Adjustment
                return 'warning';
            case 6: // Damage
            case 7: // Expiry
                return 'danger';
            default:
                return 'secondary';
        }
    }

    /**
     * Get reference type label in Arabic
     */
    getReferenceTypeLabel(type: number | string): string {
        const typeNum = typeof type === 'string' ? parseInt(type) : type;
        switch (typeNum) {
            case 1: return 'فاتورة مشتريات';
            case 2: return 'فاتورة مبيعات';
            case 3: return 'مرتجع مشتريات';
            case 4: return 'مرتجع مبيعات';
            case 5: return 'يدوي';
            case 8: return 'تعديل يدوي';
            default: return 'غير معروف';
        }
    }

    /**
     * Error handler
     */
    private handleError(error: any): Observable<never> {
        console.error('Inventory Movement Service Error:', error);
        throw error;
    }
}
