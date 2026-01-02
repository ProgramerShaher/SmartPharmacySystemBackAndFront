import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ApiResponse, PagedResult, Alert, AlertQueryDto, CreateAlertDto, UpdateAlertDto } from '../../../core/models';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SystemAlertsService {
  constructor(private http: HttpClient) { }

  private logSuccess(operation: string, details?: string): void {
    console.log(`✅ ${operation} نجاح`, {
      timestamp: new Date().toISOString(),
      details
    });
  }

  private logError(operation: string, error: any): void {
    console.error(`❌ ${operation} فشل:`, {
      timestamp: new Date().toISOString(),
      operation: operation,
      error: error.message || error,
      status: error.status,
      statusText: error.statusText,
      url: error.url,
      method: error.method,
      headers: error.headers,
      body: error.error,
      fullError: error
    });

    // إذا كان الخطأ من الـ backend، أظهر تفاصيل إضافية
    if (error.error) {
      console.error('📄 تفاصيل الخطأ من الـ backend:', error.error);
    }
  }

  // Get all alerts with optional filtering
  getAllAlerts(query?: AlertQueryDto): Observable<Alert[]> {
    console.log('🔍 Searching alerts with query:', query);

    let params = new HttpParams();
    if (query?.search) {
      params = params.set('search', query.search);
    }
    if (query?.status !== undefined && query?.status !== null) {
      params = params.set('status', query.status.toString());
    }
    // page and pageSize might be ignored by backend if it returns list, but keeping them safely
    if (query?.page) {
      params = params.set('page', query.page.toString());
    }
    if (query?.pageSize) {
      params = params.set('pageSize', query.pageSize.toString());
    }

    console.log('📤 Sending request to:', `${environment.apiUrl}/Alerts`);
    console.log('📤 With params:', params.toString());

    // The API documentation shows data is Alert[], not PagedResult
    return this.http.get<ApiResponse<Alert[]>>(
      `${environment.apiUrl}/Alerts`,
      { params }
    ).pipe(
      map(res => {
        console.log('✅ Raw API Response:', res);

        let items: any[] = [];
        if (Array.isArray(res.data)) {
          items = res.data;
        } else {
          // If it sends paged result structure
          items = (res.data as any)?.items || [];
        }

        // Transform API DTO to Frontend Model
        return items.map(item => ({
          ...item,
          // Map expiryDateSnapshot to expiryDate if missing
          expiryDate: item.expiryDateSnapshot || item.expiryDate,
          // Map isRead to status (1=Read, 0=Pending) if status is missing
          status: item.status !== undefined ? item.status : (item.isRead ? 1 : 0),
          // Ensure alertType is string if needed or handle it
          alertType: item.alertType?.toString() || ''
        }));
      }),
      catchError(error => {
        console.error('❌ Error in getAllAlerts:', error);
        this.logError('جلب التنبيهات', error);
        throw error;
      })
    );
  }

  // Get alert by ID
  getAlertById(id: number): Observable<Alert> {
    console.log(`📥 جلب بيانات التنبيه ${id}`);
    return this.http.get<ApiResponse<Alert>>(
      `${environment.apiUrl}/Alerts/${id}`
    ).pipe(
      map(res => {
        const item: any = res.data;
        if (!item) return item;

        // Transform DTO to Frontend Model
        return {
          ...item,
          expiryDate: item.expiryDateSnapshot || item.expiryDate,
          status: item.status !== undefined ? item.status : (item.isRead ? 1 : 0),
          alertType: item.alertType?.toString() || ''
        };
      }),
      catchError(error => {
        console.error(`❌ Error getting alert ${id}:`, error);
        this.logError(`جلب التنبيه ${id}`, error);
        throw error;
      })
    );
  }

  // Create new alert
  createAlert(alert: CreateAlertDto): Observable<Alert> {
    console.log('➕ إنشاء تنبيه جديد:', alert);

    return this.http.post<ApiResponse<Alert>>(
      `${environment.apiUrl}/Alerts`,
      alert
    ).pipe(
      map(res => res.data),
      catchError(error => {
        console.error('❌ Error creating alert:', error);
        this.logError('إنشاء تنبيه', error);
        throw error;
      })
    );
  }

  // Update alert
  updateAlert(id: number, alert: UpdateAlertDto): Observable<Alert> {
    console.log(`✏️ تحديث التنبيه ${id}:`, alert);

    return this.http.put<ApiResponse<Alert>>(
      `${environment.apiUrl}/Alerts/${id}`,
      alert
    ).pipe(
      map(res => res.data),
      catchError(error => {
        console.error(`❌ Error updating alert ${id}:`, error);
        this.logError(`تحديث التنبيه ${id}`, error);
        throw error;
      })
    );
  }

  // Delete alert
  deleteAlert(id: number): Observable<void> {
    console.log(`🗑️ حذف التنبيه ${id}`);
    return this.http.delete<ApiResponse<void>>(
      `${environment.apiUrl}/Alerts/${id}`
    ).pipe(
      map(res => res.data),
      catchError(error => {
        console.error(`❌ Error deleting alert ${id}:`, error);
        this.logError(`حذف التنبيه ${id}`, error);
        throw error;
      })
    );
  }

  // Get alerts by status
  getAlertsByStatus(status: string): Observable<Alert[]> {
    console.log(`📋 جلب التنبيهات بحالة: ${status}`);
    return this.http.get<ApiResponse<Alert[]>>(
      `${environment.apiUrl}/Alerts/status/${status}`
    ).pipe(
      map(res => res.data),
      catchError(error => {
        console.error(`❌ Error getting alerts by status ${status}:`, error);
        this.logError(`جلب التنبيهات بحالة ${status}`, error);
        throw error;
      })
    );
  }

  // Get alerts by batch ID
  getAlertsByBatchId(batchId: number): Observable<Alert[]> {
    console.log(`📦 جلب تنبيهات الدفعة: ${batchId}`);
    return this.http.get<ApiResponse<Alert[]>>(
      `${environment.apiUrl}/Alerts/batch/${batchId}`
    ).pipe(
      map(res => res.data),
      catchError(error => {
        console.error(`❌ Error getting alerts for batch ${batchId}:`, error);
        this.logError(`جلب تنبيهات الدفعة ${batchId}`, error);
        throw error;
      })
    );
  }

  // Mark alert as read
  markAsRead(id: number): Observable<void> {
    console.log(`👁️ تحديد التنبيه ${id} كمقروء`);
    return this.http.post<ApiResponse<void>>(
      `${environment.apiUrl}/Alerts/${id}/read`,
      {}
    ).pipe(
      map(res => res.data),
      catchError(error => {
        console.error(`❌ Error marking alert ${id} as read:`, error);
        this.logError(`تحديد التنبيه ${id} كمقروء`, error);
        throw error;
      })
    );
  }

  // Generate expiry alerts
  generateExpiryAlerts(): Observable<void> {
    console.log('🔄 إنشاء تنبيهات انتهاء الصلاحية');
    return this.http.post<ApiResponse<void>>(
      `${environment.apiUrl}/Alerts/generate-expiry`,
      {}
    ).pipe(
      map(res => res.data),
      catchError(error => {
        console.error('❌ Error generating expiry alerts:', error);
        this.logError('إنشاء تنبيهات انتهاء الصلاحية', error);
        throw error;
      })
    );
  }

  // Generate low stock alerts
  generateLowStockAlerts(): Observable<void> {
    console.log('🔄 إنشاء تنبيهات نقص المخزون');
    return this.http.post<ApiResponse<void>>(
      `${environment.apiUrl}/Alerts/generate-low-stock`,
      {}
    ).pipe(
      map(res => res.data),
      catchError(error => {
        console.error('❌ Error generating low stock alerts:', error);
        this.logError('إنشاء تنبيهات نقص المخزون', error);
        throw error;
      })
    );
  }

  // Alias methods for backward compatibility
  getAll(query?: AlertQueryDto): Observable<Alert[]> {
    return this.getAllAlerts(query);
  }

  getById(id: number): Observable<Alert> {
    return this.getAlertById(id);
  }

  create(alert: CreateAlertDto): Observable<Alert> {
    return this.createAlert(alert);
  }

  update(id: number, alert: UpdateAlertDto): Observable<Alert> {
    return this.updateAlert(id, alert);
  }

  delete(id: number): Observable<void> {
    return this.deleteAlert(id);
  }
}
