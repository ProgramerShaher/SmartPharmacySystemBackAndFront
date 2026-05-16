import { Injectable, signal, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, interval, of } from 'rxjs';
import { map, tap, catchError, retry, switchMap, shareReplay } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
    Alert,
    AlertQueryDto,
    CreateAlertDto,
    UpdateAlertDto,
    AlertStatus
} from '../models/alert.interface';
import { ApiResponse, PagedResult } from '../models';
import { MessageService } from 'primeng/api';

@Injectable({
    providedIn: 'root'
})
export class AlertService {
    private readonly apiUrl = `${environment.apiUrl}/Alerts`;

    // Reactive state for unread alerts
    private unreadAlertsSubject = new BehaviorSubject<Alert[]>([]);
    public unreadAlerts$ = this.unreadAlertsSubject.asObservable();

    // Signal for unread count
    public unreadCount = signal(0);

    // Polling interval (60 seconds)
    private pollingInterval = 60000;

    // Track if polling is active
    private isPollingActive = false;

    constructor(
        private http: HttpClient,
        private messageService: MessageService,
        @Inject(PLATFORM_ID) private platformId: Object
    ) {
        if (isPlatformBrowser(this.platformId)) {
            this.startPolling();
        }
    }

    // ─────────────────────────────────────────────────────────
    // Polling
    // ─────────────────────────────────────────────────────────

    /**
     * Start polling for new alerts every 60 seconds
     */
    private startPolling(): void {
        if (this.isPollingActive) return;
        this.isPollingActive = true;

        // Initial fetch
        this.fetchUnreadAlerts().subscribe();

        // Then poll every 60 seconds
        interval(this.pollingInterval)
            .pipe(
                switchMap(() => this.fetchUnreadAlerts()),
                retry({ count: 3, delay: 5000 }),
                catchError(() => {
                    console.warn('⚠️ Alert polling failed, will retry on next interval');
                    return of([] as Alert[]);
                })
            )
            .subscribe();
    }

    /**
     * Fetch unread (Pending) alerts and update the shared state
     */
    private fetchUnreadAlerts(): Observable<Alert[]> {
        const query: AlertQueryDto = {
            status: AlertStatus.Pending,
            pageSize: 10,
            page: 1
        };

        return this.getAllAlerts(query).pipe(
            tap(alerts => {
                const previousCount = this.unreadCount();
                this.unreadAlertsSubject.next(alerts);
                this.unreadCount.set(alerts.length);

                // Show toast for new alerts
                if (alerts.length > previousCount && previousCount >= 0) {
                    const newAlert = alerts[0];
                    this.messageService.add({
                        severity: this.getSeverity(newAlert.alertType),
                        summary: 'تنبيه جديد',
                        detail: newAlert.message,
                        life: 6000
                    });
                }
            }),
            catchError(() => of([] as Alert[]))
        );
    }

    private getSeverity(type: any): string {
        const t = type?.toString() || '';
        if (t.includes('Critical') || t.includes('OneWeek')) return 'error';
        if (t.includes('Warning') || t.includes('TwoWeeks') || t.includes('LowStock')) return 'warn';
        return 'info';
    }

    /**
     * Force refresh unread alerts (call after any state change)
     */
    refreshUnreadAlerts(): void {
        this.fetchUnreadAlerts().subscribe();
    }

    // ─────────────────────────────────────────────────────────
    // CRUD — Alerts
    // ─────────────────────────────────────────────────────────

    /**
     * Get all alerts with optional filters – returns flat Alert[]
     */
    getAllAlerts(query?: AlertQueryDto): Observable<Alert[]> {
        let params = new HttpParams();

        if (query?.search) params = params.set('search', query.search);
        if (query?.status !== undefined) params = params.set('status', query.status.toString());
        if (query?.alertType) params = params.set('alertType', query.alertType);
        if (query?.batchNumber) params = params.set('batchNumber', query.batchNumber);
        if (query?.medicineName) params = params.set('medicineName', query.medicineName);
        if (query?.page) params = params.set('page', query.page.toString());
        if (query?.pageSize) params = params.set('pageSize', query.pageSize.toString());

        return this.http.get<ApiResponse<Alert[] | PagedResult<Alert>>>(`${this.apiUrl}`, { params })
            .pipe(
                map(res => {
                    if (Array.isArray(res.data)) return res.data;
                    if (res.data && 'items' in res.data) return (res.data as PagedResult<Alert>).items;
                    return [];
                }),
                shareReplay(1)
            );
    }

    /**
     * Get alerts with pagination support – returns PagedResult<Alert>
     */
    getAlerts(query: AlertQueryDto): Observable<PagedResult<Alert>> {
        let params = new HttpParams();

        if (query.search) params = params.set('search', query.search);
        if (query.status !== undefined) params = params.set('status', query.status.toString());
        if (query.alertType) params = params.set('alertType', query.alertType);
        if (query.batchNumber) params = params.set('batchNumber', query.batchNumber);
        if (query.medicineName) params = params.set('medicineName', query.medicineName);
        if (query.page) params = params.set('page', query.page.toString());
        if (query.pageSize) params = params.set('pageSize', query.pageSize.toString());

        return this.http.get<ApiResponse<Alert[] | PagedResult<Alert>>>(`${this.apiUrl}`, { params })
            .pipe(
                map(res => {
                    if (Array.isArray(res.data)) {
                        return {
                            items: res.data,
                            totalCount: res.data.length,
                            pageNumber: query.page || 1,
                            pageSize: query.pageSize || res.data.length
                        } as PagedResult<Alert>;
                    }
                    return res.data as PagedResult<Alert>;
                })
            );
    }

    /**
     * Get alert by ID
     */
    getAlertById(id: number): Observable<Alert> {
        return this.http.get<ApiResponse<Alert>>(`${this.apiUrl}/${id}`)
            .pipe(map(res => res.data));
    }

    /**
     * Create new alert
     */
    createAlert(dto: CreateAlertDto): Observable<Alert> {
        return this.http.post<ApiResponse<Alert>>(`${this.apiUrl}`, dto)
            .pipe(
                map(res => res.data),
                tap(() => this.refreshUnreadAlerts())
            );
    }

    /**
     * Update alert
     */
    updateAlert(id: number, dto: UpdateAlertDto): Observable<Alert> {
        return this.http.put<ApiResponse<Alert>>(`${this.apiUrl}/${id}`, dto)
            .pipe(
                map(res => res.data),
                tap(() => this.refreshUnreadAlerts())
            );
    }

    /**
     * Mark alert as read
     */
    markAsRead(id: number): Observable<void> {
        return this.http.post<ApiResponse<void>>(`${this.apiUrl}/${id}/read`, {})
            .pipe(
                map(res => res.data),
                tap(() => this.refreshUnreadAlerts())
            );
    }

    /**
     * Mark multiple alerts as read
     */
    markMultipleAsRead(ids: number[]): Observable<void> {
        const requests = ids.map(id => this.markAsRead(id));
        return new Observable(observer => {
            Promise.all(requests.map(req => req.toPromise()))
                .then(() => {
                    this.refreshUnreadAlerts();
                    observer.next();
                    observer.complete();
                })
                .catch(err => observer.error(err));
        });
    }

    /**
     * Dismiss an alert
     */
    dismissAlert(id: number): Observable<Alert> {
        return this.updateAlert(id, { id, status: AlertStatus.Dismissed });
    }

    /**
     * Dismiss multiple alerts
     */
    dismissMultipleAlerts(ids: number[]): Observable<void> {
        const requests = ids.map(id => this.dismissAlert(id));
        return new Observable(observer => {
            Promise.all(requests.map(req => req.toPromise()))
                .then(() => {
                    this.refreshUnreadAlerts();
                    observer.next();
                    observer.complete();
                })
                .catch(err => observer.error(err));
        });
    }

    /**
     * Delete alert
     */
    deleteAlert(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`)
            .pipe(
                map(res => res.data),
                tap(() => this.refreshUnreadAlerts())
            );
    }

    // ─────────────────────────────────────────────────────────
    // Alert Generation
    // ─────────────────────────────────────────────────────────

    generateExpiryAlerts(): Observable<void> {
        return this.http.post<ApiResponse<void>>(`${this.apiUrl}/generate-expiry`, {})
            .pipe(map(res => res.data), tap(() => this.refreshUnreadAlerts()));
    }

    generateLowStockAlerts(): Observable<void> {
        return this.http.post<ApiResponse<void>>(`${this.apiUrl}/generate-low-stock`, {})
            .pipe(map(res => res.data), tap(() => this.refreshUnreadAlerts()));
    }

    syncMedicineAlerts(medicineId: number): Observable<void> {
        return this.http.post<ApiResponse<void>>(`${this.apiUrl}/sync/${medicineId}`, {})
            .pipe(map(res => res.data), tap(() => this.refreshUnreadAlerts()));
    }

    // ─────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────

    getAlertsByStatus(status: number): Observable<Alert[]> {
        return this.getAllAlerts({ status });
    }

    getAlertsByBatchId(batchId: number): Observable<Alert[]> {
        return this.http.get<ApiResponse<Alert[]>>(`${this.apiUrl}/batch/${batchId}`)
            .pipe(map(res => res.data));
    }

    getStatistics(): Observable<{
        totalAlerts: number;
        pendingAlerts: number;
        readAlerts: number;
        expiryAlerts: number;
        lowStockAlerts: number;
        criticalAlerts: number;
        recentAlerts: Alert[];
    }> {
        return this.getAllAlerts().pipe(
            map(alerts => {
                const yesterday = new Date(Date.now() - 24 * 60 * 60 * 1000);
                return {
                    totalAlerts: alerts.length,
                    pendingAlerts: alerts.filter(a => a.status === AlertStatus.Pending).length,
                    readAlerts: alerts.filter(a => a.status === AlertStatus.Read).length,
                    expiryAlerts: alerts.filter(a => a.alertType?.includes('Expiry')).length,
                    lowStockAlerts: alerts.filter(a => a.alertType?.includes('LowStock')).length,
                    criticalAlerts: alerts.filter(a =>
                        a.alertType?.includes('OneWeek') || a.alertType?.includes('TwoWeeks')
                    ).length,
                    recentAlerts: alerts.filter(a => new Date(a.createdAt) > yesterday).slice(0, 10)
                };
            })
        );
    }

    getAlertUrgency(alert: Alert): 'critical' | 'high' | 'medium' | 'low' {
        if (alert.alertType?.includes('OneWeek')) return 'critical';
        if (alert.alertType?.includes('TwoWeeks')) return 'high';
        if (alert.alertType?.includes('OneMonth') || alert.alertType?.includes('LowStock')) return 'medium';
        return 'low';
    }
}
