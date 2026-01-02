import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import {
    SalesReturn,
    CreateSalesReturnDto,
    UpdateSalesReturnDto
} from '../../../core/models/sales-return.interface';
import { ApiResponse } from '../../../core/models/api-response.interface';

/**
 * خدمة مرتجعات المبيعات - SalesReturnService
 * توفر جميع عمليات CRUD والاعتماد والإلغاء لمرتجعات المبيعات
 * Built for Angular 17 with Standalone Components pattern
 */
@Injectable({
    providedIn: 'root'
})
export class SalesReturnService {
    private readonly http = inject(HttpClient);
    private readonly baseUrl = `${environment.apiUrl}/SalesReturns`;

    /**
     * جلب جميع مرتجعات المبيعات مع إمكانية البحث
     * @param search نص البحث (اختياري)
     * @returns قائمة مرتجعات المبيعات
     */
    getAll(search?: string): Observable<SalesReturn[]> {
        let params = new HttpParams();
        if (search) {
            params = params.set('search', search);
        }

        return this.http.get<ApiResponse<SalesReturn[]>>(this.baseUrl, { params }).pipe(
            map(response => response.data || []),
            catchError(this.handleError)
        );
    }

    /**
     * جلب مرتجع مبيعات محدد بالمعرف
     * @param id معرف المرتجع
     * @returns مرتجع المبيعات
     */
    getById(id: number): Observable<SalesReturn> {
        return this.http.get<ApiResponse<SalesReturn>>(`${this.baseUrl}/${id}`).pipe(
            map(response => {
                if (!response.data) {
                    throw new Error('Return not found');
                }
                return response.data;
            }),
            catchError(this.handleError)
        );
    }

    /**
     * إنشاء مرتجع مبيعات جديد
     * @param dto بيانات المرتجع الجديد
     * @returns المرتجع المنشأ
     */
    create(dto: CreateSalesReturnDto): Observable<SalesReturn> {
        return this.http.post<ApiResponse<SalesReturn>>(this.baseUrl, dto).pipe(
            map(response => {
                if (!response.data) {
                    throw new Error('Failed to create return');
                }
                return response.data;
            }),
            catchError(this.handleError)
        );
    }

    /**
     * تحديث مرتجع مبيعات موجود
     * @param id معرف المرتجع
     * @param dto بيانات التحديث
     * @returns المرتجع المحدث
     */
    update(id: number, dto: UpdateSalesReturnDto): Observable<SalesReturn> {
        return this.http.put<ApiResponse<SalesReturn>>(`${this.baseUrl}/${id}`, dto).pipe(
            map(response => {
                if (!response.data) {
                    throw new Error('Failed to update return');
                }
                return response.data;
            }),
            catchError(this.handleError)
        );
    }

    /**
     * اعتماد مرتجع مبيعات (Admin فقط)
     * يقوم بإضافة الكميات للمخزون وتسجيل المعاملة المالية
     * @param id معرف المرتجع
     * @returns نتيجة العملية
     */
    approve(id: number): Observable<void> {
        return this.http.post<ApiResponse<any>>(`${this.baseUrl}/${id}/approve`, {}).pipe(
            map(() => undefined),
            catchError(this.handleError)
        );
    }

    /**
     * إلغاء مرتجع مبيعات (Admin فقط)
     * @param id معرف المرتجع
     * @returns نتيجة العملية
     */
    cancel(id: number): Observable<void> {
        return this.http.post<ApiResponse<any>>(`${this.baseUrl}/${id}/cancel`, {}).pipe(
            map(() => undefined),
            catchError(this.handleError)
        );
    }

    /**
     * حذف مرتجع مبيعات (Admin فقط)
     * @param id معرف المرتجع
     * @returns نتيجة العملية
     */
    delete(id: number): Observable<void> {
        return this.http.delete<ApiResponse<any>>(`${this.baseUrl}/${id}`).pipe(
            map(() => undefined),
            catchError(this.handleError)
        );
    }

    /**
     * معالجة الأخطاء القادمة من الباك إند
     * @param error خطأ HTTP
     * @returns Observable يحتوي على رسالة الخطأ
     */
    private handleError(error: any): Observable<never> {
        let errorMessage = 'حدث خطأ غير متوقع';

        if (error.error instanceof ErrorEvent) {
            // خطأ من جهة العميل
            errorMessage = `خطأ: ${error.error.message}`;
        } else {
            // خطأ من جهة الخادم
            if (error.error?.message) {
                errorMessage = error.error.message;
            } else if (error.error?.errors) {
                // Validation errors from backend
                const validationErrors = Object.values(error.error.errors).flat();
                errorMessage = validationErrors.join(', ');
            } else if (error.status === 404) {
                errorMessage = 'المرتجع غير موجود';
            } else if (error.status === 401) {
                errorMessage = 'غير مصرح لك بالدخول';
            } else if (error.status === 403) {
                errorMessage = 'ليس لديك صلاحية للقيام بهذه العملية';
            }
        }

        console.error('SalesReturnService Error:', error);
        return throwError(() => new Error(errorMessage));
    }
}
