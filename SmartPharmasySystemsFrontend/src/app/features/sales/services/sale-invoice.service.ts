import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import {
  SaleInvoice,
  CreateSaleInvoiceDto,
  UpdateSaleInvoiceDto
} from '../../../core/models/sale-invoice.interface';
import { ApiResponse } from '../../../core/models/api-response.interface';

/**
 * خدمة فواتير المبيعات - SaleInvoiceService
 * توفر جميع عمليات CRUD والاعتماد والإلغاء لفواتير المبيعات
 * Built for Angular 17 with Standalone Components pattern
 */
@Injectable({
  providedIn: 'root'
})
export class SaleInvoiceService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/SalesInvoices`;

  /**
   * جلب جميع فواتير المبيعات مع إمكانية البحث
   * @param search نص البحث (اختياري)
   * @returns قائمة فواتير المبيعات
   */
  getAll(search?: string): Observable<SaleInvoice[]> {
    let params = new HttpParams();
    if (search) {
      params = params.set('search', search);
    }

    return this.http.get<ApiResponse<SaleInvoice[]>>(this.baseUrl, { params }).pipe(
      map(response => response.data || []),
      catchError(this.handleError)
    );
  }

  /**
   * جلب فاتورة مبيعات محددة بالمعرف
   * @param id معرف الفاتورة
   * @returns فاتورة المبيعات
   */
  getById(id: number): Observable<SaleInvoice> {
    return this.http.get<ApiResponse<SaleInvoice>>(`${this.baseUrl}/${id}`).pipe(
      map(response => {
        if (!response.data) {
          throw new Error('Invoicenot found');
        }
        return response.data;
      }),
      catchError(this.handleError)
    );
  }

  /**
   * إنشاء فاتورة مبيعات جديدة
   * @param dto بيانات الفاتورة الجديدة
   * @returns الفاتورة المنشأة
   */
  create(dto: CreateSaleInvoiceDto): Observable<SaleInvoice> {
    return this.http.post<ApiResponse<SaleInvoice>>(this.baseUrl, dto).pipe(
      map(response => {
        if (!response.data) {
          throw new Error('Failed to create invoice');
        }
        return response.data;
      }),
      catchError(this.handleError)
    );
  }

  /**
   * تحديث فاتورة مبيعات موجودة
   * @param id معرف الفاتورة
   * @param dto بيانات التحديث
   * @returns الفاتورة المحدثة
   */
  update(id: number, dto: UpdateSaleInvoiceDto): Observable<SaleInvoice> {
    return this.http.put<ApiResponse<SaleInvoice>>(`${this.baseUrl}/${id}`, dto).pipe(
      map(response => {
        if (!response.data) {
          throw new Error('Failed to update invoice');
        }
        return response.data;
      }),
      catchError(this.handleError)
    );
  }

  /**
   * اعتماد فاتورة مبيعات (Admin فقط)
   * يقوم بخصم الكميات من المخزون وإضافة المعاملة المالية
   * @param id معرف الفاتورة
   * @returns نتيجة العملية
   */
  approve(id: number): Observable<void> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/${id}/approve`, {}).pipe(
      map(() => undefined),
      catchError(this.handleError)
    );
  }

  /**
   * إلغاء اعتماد فاتورة مبيعات (Admin فقط)
   * يقوم بعكس حركات المخزون والمعاملات المالية
   * @param id معرف الفاتورة
   * @returns نتيجة العملية
   */
  unapprove(id: number): Observable<void> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/${id}/unapprove`, {}).pipe(
      map(() => undefined),
      catchError(this.handleError)
    );
  }

  /**
   * إلغاء فاتورة مبيعات (Admin فقط)
   * @param id معرف الفاتورة
   * @returns نتيجة العملية
   */
  cancel(id: number): Observable<void> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/${id}/cancel`, {}).pipe(
      map(() => undefined),
      catchError(this.handleError)
    );
  }

  /**
   * حذف فاتورة مبيعات (Admin فقط)
   * @param id معرف الفاتورة
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
        errorMessage = 'الفاتورة غير موجودة';
      } else if (error.status === 401) {
        errorMessage = 'غير مصرح لك بالدخول';
      } else if (error.status === 403) {
        errorMessage = 'ليس لديك صلاحية للقيام بهذه العملية';
      }
    }

    console.error('SaleInvoiceService Error:', error);
    return throwError(() => new Error(errorMessage));
  }
}
