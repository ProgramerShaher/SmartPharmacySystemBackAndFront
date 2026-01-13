import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.interface';
import { SalesDashboardStats, PurchasesDashboardStats } from '../models/dashboard-stats.interface';

/**
 * Dashboard Statistics Service
 * Provides high-performance stats for KPI cards
 * All endpoints are optimized for <100ms response time
 */
@Injectable({
    providedIn: 'root'
})
export class DashboardStatsService {
    private readonly http = inject(HttpClient);
    private readonly salesUrl = `${environment.apiUrl}/SalesInvoices`;
    private readonly purchasesUrl = `${environment.apiUrl}/PurchaseInvoices`;

    /**
     * Get sales dashboard statistics
     * Returns: TodayTotalSales, TodayNetProfit, CustomerDebts, ReturnRate, Last7DaysSales
     */
    getSalesDashboardStats(): Observable<SalesDashboardStats> {
        return this.http.get<ApiResponse<SalesDashboardStats>>(`${this.salesUrl}/dashboard-stats`).pipe(
            map(response => response.data),
            catchError(this.handleError)
        );
    }

    /**
     * Get purchases dashboard statistics
     * Returns: MonthlyTotalPurchases, SupplierDebts, SupplierDistribution, Last7DaysPurchases
     */
    getPurchasesDashboardStats(): Observable<PurchasesDashboardStats> {
        return this.http.get<ApiResponse<PurchasesDashboardStats>>(`${this.purchasesUrl}/dashboard-stats`).pipe(
            map(response => response.data),
            catchError(this.handleError)
        );
    }

    private handleError(error: any): Observable<never> {
        console.error('DashboardStatsService Error:', error);
        throw error;
    }
}
