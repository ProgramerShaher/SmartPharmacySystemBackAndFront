import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../environments/environment';

/**
 * 📊 Sales Statistics Service - Live Data Integration
 * ====================================================
 * خدمة الإحصائيات الحية للمبيعات - تجلب البيانات المجمعة من الباك إند
 * NO STATIC DATA ALLOWED - كل البيانات من قاعدة البيانات
 */

export interface SalesKPIData {
  totalSalesToday: number;
  totalProfitToday: number;
  totalDebts: number;
  totalReturnsToday: number;
  cashSalesPercentage: number;
  creditSalesPercentage: number;
  cashSalesAmount: number;
  creditSalesAmount: number;
}

export interface SalesFlowData {
  date: string;
  sales: number;
  profit: number;
  returns: number;
}

export interface PaymentMethodDistribution {
  method: string;
  amount: number;
  percentage: number;
  count: number;
}

export interface TopSellingProduct {
  medicineName: string;
  quantity: number;
  revenue: number;
  profit: number;
}

export interface SalesStatisticsResponse {
  kpiData: SalesKPIData;
  salesFlow: SalesFlowData[];
  paymentDistribution: PaymentMethodDistribution[];
  topProducts: TopSellingProduct[];
}

@Injectable({
  providedIn: 'root'
})
export class SalesStatisticsService {
  private apiUrl = `${environment.apiUrl}/Sales`;

  constructor(private http: HttpClient) {}

  /**
   * 📈 Get Today's KPI Data
   * جلب مؤشرات الأداء الرئيسية لليوم الحالي
   */
  getTodayKPIs(): Observable<SalesKPIData> {
    return this.http.get<any>(`${this.apiUrl}/kpi/today`).pipe(
      map(response => ({
        totalSalesToday: response.totalSales || 0,
        totalProfitToday: response.totalProfit || 0,
        totalDebts: response.totalDebts || 0,
        totalReturnsToday: response.totalReturns || 0,
        cashSalesPercentage: response.cashPercentage || 0,
        creditSalesPercentage: response.creditPercentage || 0,
        cashSalesAmount: response.cashAmount || 0,
        creditSalesAmount: response.creditAmount || 0
      }))
    );
  }

  /**
   * 📊 Get Sales Flow (Last 7 Days)
   * جلب تدفق المبيعات لآخر 7 أيام
   */
  getSalesFlow(days: number = 7): Observable<SalesFlowData[]> {
    const params = new HttpParams().set('days', days.toString());
    return this.http.get<SalesFlowData[]>(`${this.apiUrl}/flow`, { params });
  }

  /**
   * 🥧 Get Payment Method Distribution
   * جلب توزيع طرق الدفع (Cash vs Credit)
   */
  getPaymentDistribution(startDate?: string, endDate?: string): Observable<PaymentMethodDistribution[]> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    
    return this.http.get<PaymentMethodDistribution[]>(`${this.apiUrl}/payment-distribution`, { params });
  }

  /**
   * 🏆 Get Top Selling Products
   * جلب أكثر المنتجات مبيعاً
   */
  getTopSellingProducts(limit: number = 10): Observable<TopSellingProduct[]> {
    const params = new HttpParams().set('limit', limit.toString());
    return this.http.get<TopSellingProduct[]>(`${this.apiUrl}/top-products`, { params });
  }

  /**
   * 📋 Get Complete Dashboard Statistics
   * جلب كافة الإحصائيات للوحة التحكم
   */
  getDashboardStatistics(): Observable<SalesStatisticsResponse> {
    return this.http.get<SalesStatisticsResponse>(`${this.apiUrl}/dashboard-stats`);
  }

  /**
   * 💰 Get Customer Debts Summary
   * جلب ملخص ديون العملاء
   */
  getCustomerDebtsSummary(): Observable<any> {
    return this.http.get(`${this.apiUrl}/debts/summary`);
  }

  /**
   * 📉 Get Returns Analysis
   * تحليل المرتجعات
   */
  getReturnsAnalysis(startDate?: string, endDate?: string): Observable<any> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    
    return this.http.get(`${this.apiUrl}/returns/analysis`, { params });
  }
}
