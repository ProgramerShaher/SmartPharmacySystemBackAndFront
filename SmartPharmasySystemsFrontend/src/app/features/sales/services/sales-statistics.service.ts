import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { SaleInvoiceService } from './sale-invoice.service';

/**
 * 📊 Sales Statistics Service - Uses Backend API
 * ====================================================
 * خدمة الإحصائيات للمبيعات - تستدعي API الباك إند
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

// Backend API Response Interface
interface ApiResponse<T> {
  data: T;
  succeeded: boolean;
  message: string;
}

interface BackendSalesStats {
  todayTotalSales: number;
  todayNetProfit: number;
  customerDebts: number;
  todayReturnsAmount: number;
  returnRate: number;
  cashPercentage: number;
  last7DaysSales: number[];
}

@Injectable({
  providedIn: 'root'
})
export class SalesStatisticsService {
  private readonly http = inject(HttpClient);
  private readonly invoiceService = inject(SaleInvoiceService);
  private readonly apiUrl = `${environment.apiUrl}/SalesInvoices`;

  /**
   * 📈 Get Today's KPI Data from Backend API
   * جلب مؤشرات الأداء الرئيسية من الباك إند
   */
  getTodayKPIs(): Observable<SalesKPIData> {
    return this.http.get<ApiResponse<BackendSalesStats>>(`${this.apiUrl}/dashboard-stats`).pipe(
      map(response => {
        const stats = response.data;
        return {
          totalSalesToday: stats.todayTotalSales,
          totalProfitToday: stats.todayNetProfit,
          totalDebts: stats.customerDebts,
          totalReturnsToday: stats.todayReturnsAmount,
          cashSalesPercentage: stats.cashPercentage,
          creditSalesPercentage: 100 - stats.cashPercentage,
          cashSalesAmount: stats.todayTotalSales * (stats.cashPercentage / 100),
          creditSalesAmount: stats.todayTotalSales * ((100 - stats.cashPercentage) / 100)
        };
      }),
      catchError(err => {
        console.error('[SalesStatisticsService] API Error, falling back to local calculation:', err);
        // Fallback to local calculation if API fails
        return this.calculateLocalKPIs();
      })
    );
  }

  /**
   * Fallback: Calculate from local invoices if API fails
   */
  private calculateLocalKPIs(): Observable<SalesKPIData> {
    return this.invoiceService.getAll().pipe(
      map(invoices => {
        const today = new Date().toDateString();
        const todayInvoices = invoices.filter(i => new Date(i.invoiceDate).toDateString() === today);
        const approved = todayInvoices.filter(i => (i.status as any) === 2 || (i.status as any) === 'Approved');

        const totalSales = approved.reduce((sum, i) => sum + (i.totalAmount || 0), 0);
        const totalProfit = approved.reduce((sum, i) => sum + (i.totalProfit || 0), 0);

        const cashInvoices = approved.filter(i => (i.paymentMethod as any) === 1 || (i.paymentMethod as any) === 'Cash');
        const creditInvoices = approved.filter(i => (i.paymentMethod as any) === 2 || (i.paymentMethod as any) === 'Credit');

        const cashAmount = cashInvoices.reduce((sum, i) => sum + (i.totalAmount || 0), 0);
        const creditAmount = creditInvoices.reduce((sum, i) => sum + (i.totalAmount || 0), 0);
        const total = cashAmount + creditAmount || 1;

        return {
          totalSalesToday: totalSales,
          totalProfitToday: totalProfit,
          totalDebts: creditAmount,
          totalReturnsToday: 0,
          cashSalesPercentage: Math.round(cashAmount / total * 100),
          creditSalesPercentage: Math.round(creditAmount / total * 100),
          cashSalesAmount: cashAmount,
          creditSalesAmount: creditAmount
        };
      }),
      catchError(() => of({
        totalSalesToday: 0,
        totalProfitToday: 0,
        totalDebts: 0,
        totalReturnsToday: 0,
        cashSalesPercentage: 0,
        creditSalesPercentage: 0,
        cashSalesAmount: 0,
        creditSalesAmount: 0
      }))
    );
  }

  /**
   * 📊 Get Sales Flow (Last N Days - Calculated from local invoices)
   * جلب تدفق المبيعات لآخر N أيام
   */
  getSalesFlow(days: number = 7): Observable<SalesFlowData[]> {
    return this.invoiceService.getAll().pipe(
      map(invoices => {
        const result: SalesFlowData[] = [];

        for (let i = days - 1; i >= 0; i--) {
          const date = new Date();
          date.setDate(date.getDate() - i);
          const dateStr = date.toDateString();

          const dayInvoices = invoices.filter(inv =>
            new Date(inv.invoiceDate).toDateString() === dateStr &&
            ((inv.status as any) === 2 || (inv.status as any) === 'Approved')
          );

          result.push({
            date: date.toISOString(),
            sales: dayInvoices.reduce((sum, i) => sum + (i.totalAmount || 0), 0),
            profit: dayInvoices.reduce((sum, i) => sum + (i.totalProfit || 0), 0),
            returns: 0
          });
        }

        return result;
      }),
      catchError(() => {
        const result: SalesFlowData[] = [];
        for (let i = days - 1; i >= 0; i--) {
          const date = new Date();
          date.setDate(date.getDate() - i);
          result.push({ date: date.toISOString(), sales: 0, profit: 0, returns: 0 });
        }
        return of(result);
      })
    );
  }

  /**
   * 🥧 Get Payment Method Distribution (Calculated from local invoices)
   * جلب توزيع طرق الدفع (Cash vs Credit)
   */
  getPaymentDistribution(startDate?: string, endDate?: string): Observable<PaymentMethodDistribution[]> {
    return this.invoiceService.getAll().pipe(
      map(invoices => {
        const approved = invoices.filter(i => (i.status as any) === 2 || (i.status as any) === 'Approved');

        const cashInvoices = approved.filter(i => (i.paymentMethod as any) === 1 || (i.paymentMethod as any) === 'Cash');
        const creditInvoices = approved.filter(i => (i.paymentMethod as any) === 2 || (i.paymentMethod as any) === 'Credit');

        const cashAmount = cashInvoices.reduce((sum, i) => sum + (i.totalAmount || 0), 0);
        const creditAmount = creditInvoices.reduce((sum, i) => sum + (i.totalAmount || 0), 0);
        const total = cashAmount + creditAmount || 1;

        return [
          {
            method: 'نقدي',
            amount: cashAmount,
            percentage: Math.round(cashAmount / total * 100),
            count: cashInvoices.length
          },
          {
            method: 'آجل',
            amount: creditAmount,
            percentage: Math.round(creditAmount / total * 100),
            count: creditInvoices.length
          }
        ];
      }),
      catchError(() => of([
        { method: 'نقدي', amount: 0, percentage: 50, count: 0 },
        { method: 'آجل', amount: 0, percentage: 50, count: 0 }
      ]))
    );
  }

  /**
   * 🏆 Get Top Selling Products (Returns empty - requires backend endpoint)
   * جلب أكثر المنتجات مبيعاً - يتطلب endpoint خاص
   */
  getTopSellingProducts(limit: number = 10): Observable<TopSellingProduct[]> {
    // This would require aggregation at the backend level
    return of([]);
  }

  /**
   * 📋 Get Complete Dashboard Statistics
   * جلب كافة الإحصائيات للوحة التحكم
   */
  getDashboardStatistics(): Observable<SalesStatisticsResponse> {
    return this.getTodayKPIs().pipe(
      map(kpiData => ({
        kpiData,
        salesFlow: [],
        paymentDistribution: [],
        topProducts: []
      }))
    );
  }

  /**
   * 💰 Get Customer Debts Summary (Returns total credit from invoices)
   * جلب ملخص ديون العملاء
   */
  getCustomerDebtsSummary(): Observable<any> {
    return this.invoiceService.getAll().pipe(
      map(invoices => {
        const creditInvoices = invoices.filter(i =>
          ((i.status as any) === 2 || (i.status as any) === 'Approved') &&
          ((i.paymentMethod as any) === 2 || (i.paymentMethod as any) === 'Credit')
        );

        return {
          totalDebt: creditInvoices.reduce((sum, i) => sum + (i.totalAmount || 0), 0),
          customersCount: new Set(creditInvoices.map(i => i.customerName)).size
        };
      }),
      catchError(() => of({ totalDebt: 0, customersCount: 0 }))
    );
  }

  /**
   * 📉 Get Returns Analysis (Returns empty - requires backend endpoint)
   * تحليل المرتجعات
   */
  getReturnsAnalysis(startDate?: string, endDate?: string): Observable<any> {
    return of({ totalReturns: 0, returnRate: 0 });
  }
}
