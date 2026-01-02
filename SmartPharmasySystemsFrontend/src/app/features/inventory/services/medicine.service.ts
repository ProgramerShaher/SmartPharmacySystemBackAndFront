import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import {
    ApiResponse,
    PagedResult,
    Medicine,
    MedicineDto,
    CreateMedicineDto,
    UpdateMedicineDto,
    MedicineQueryDto,
    MedicineBatchResponseDto
} from '../../../core/models';
import { environment } from '../../../../environments/environment';

/**
 * Medicine Service - Complete implementation matching backend 100%
 */
@Injectable({ providedIn: 'root' })
export class MedicineService {
    private readonly apiUrl = `${environment.apiUrl}/Medicines`;

    constructor(private http: HttpClient) { }

    /**
     * Search medicines with pagination and filters
     * GET /api/Medicines
     */
    search(query?: MedicineQueryDto): Observable<PagedResult<MedicineDto>> {
        let params = new HttpParams();

        if (query?.search) {
            params = params.set('search', query.search);
        }
        if (query?.categoryId) {
            params = params.set('categoryId', query.categoryId.toString());
        }
        if (query?.manufacturer) {
            params = params.set('manufacturer', query.manufacturer);
        }
        if (query?.status) {
            params = params.set('status', query.status);
        }
        if (query?.page) {
            params = params.set('page', query.page.toString());
        }
        if (query?.pageSize) {
            params = params.set('pageSize', query.pageSize.toString());
        }

        return this.http.get<ApiResponse<PagedResult<MedicineDto>>>(this.apiUrl, { params })
            .pipe(
                map(response => response.data),
                catchError(this.handleError)
            );
    }

    /**
     * Get all medicines without pagination
     * GET /api/Medicines/all
     */
    getAll(): Observable<MedicineDto[]> {
        return this.http.get<ApiResponse<MedicineDto[]>>(`${this.apiUrl}/all`)
            .pipe(
                map(response => response.data),
                catchError(this.handleError)
            );
    }

    /**
     * Get medicine by ID
     * GET /api/Medicines/{id}
     */
    getById(id: number): Observable<MedicineDto> {
        return this.http.get<ApiResponse<MedicineDto>>(`${this.apiUrl}/${id}`)
            .pipe(
                map(response => response.data),
                catchError(this.handleError)
            );
    }

    /**
     * Create new medicine
     * POST /api/Medicines
     */
    create(dto: CreateMedicineDto): Observable<MedicineDto> {
        return this.http.post<ApiResponse<MedicineDto>>(this.apiUrl, dto)
            .pipe(
                map(response => response.data),
                catchError(this.handleError)
            );
    }

    /**
     * Update existing medicine
     * PUT /api/Medicines/{id}
     */
    update(id: number, dto: UpdateMedicineDto): Observable<MedicineDto> {
        return this.http.put<ApiResponse<MedicineDto>>(`${this.apiUrl}/${id}`, dto)
            .pipe(
                map(response => response.data),
                catchError(this.handleError)
            );
    }

    /**
     * Delete medicine (soft delete) - Admin only
     * DELETE /api/Medicines/{id}
     */
    delete(id: number): Observable<any> {
        return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/${id}`)
            .pipe(
                map(response => response.data),
                catchError(this.handleError)
            );
    }

    /**
     * Get FEFO (First Expired First Out) batches for a medicine
     * GET /api/Medicines/{id}/fefo-batches
     */
    getFEFOBatches(medicineId: number): Observable<MedicineBatchResponseDto[]> {
        return this.http.get<ApiResponse<MedicineBatchResponseDto[]>>(`${this.apiUrl}/${medicineId}/fefo-batches`)
            .pipe(
                map(response => response.data),
                catchError(this.handleError)
            );
    }

    /**
     * Get reorder report (medicines at or below reorder level)
     * GET /api/Medicines/reorder-report
     */
    getReorderReport(): Observable<MedicineDto[]> {
        return this.http.get<ApiResponse<MedicineDto[]>>(`${this.apiUrl}/reorder-report`)
            .pipe(
                map(response => response.data),
                catchError(this.handleError)
            );
    }

    /**
     * Helper: Get status badge class
     */
    getStatusClass(status: string): string {
        return status === 'Active' ? 'success' : 'danger';
    }

    /**
     * Helper: Get status label
     */
    getStatusLabel(status: string): string {
        return status === 'Active' ? 'نشط' : 'غير نشط';
    }

    /**
     * Helper: Format price
     */
    formatPrice(price: number): string {
        return new Intl.NumberFormat('ar-YE', {
            style: 'currency',
            currency: 'YER',
            minimumFractionDigits: 0
        }).format(price);
    }

    /**
     * Error handler
     */
    private handleError(error: any): Observable<never> {
        console.error('Medicine Service Error:', error);
        throw error;
    }
}
