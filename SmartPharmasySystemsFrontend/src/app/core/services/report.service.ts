import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  UnifiedStatement,
  StatementLine,
  NetProfitReport,
  InventoryValuation,
  BatchValuation,
  ReportsSummary,
  StatementQuery,
  NetProfitQuery,
  InventoryValuationQuery,
  DailySalesReport,
  BestSellingMedicinesReport,
  CustomerDebtsReport,
  SupplierDebtsReport
} from '../models/reports.interface';

/**
 * خدمة التقارير المركزية
 * Central Reports Service
 * Performance Protocol: Optimized for instant response
 */
@Injectable({
  providedIn: 'root'
})
export class ReportService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/Reports`;

  // ===================== كشف الحساب الموحد - Unified Statement =====================

  /**
   * كشف الحساب الموحد للعملاء والموردين
   */
  getUnifiedStatement(
    entityType: string,
    entityId: number,
    fromDate?: Date,
    toDate?: Date
  ): Observable<UnifiedStatement> {
    let params = new HttpParams();
    if (fromDate) {
      params = params.set('fromDate', fromDate.toISOString().split('T')[0]);
    }
    if (toDate) {
      params = params.set('toDate', toDate.toISOString().split('T')[0]);
    }

    return this.http.get<{ data: UnifiedStatement }>(
      `${this.baseUrl}/statement/${entityType}/${entityId}`,
      { params }
    ).pipe(map(res => res.data));
  }

  /**
   * كشف الحساب مع التصفح
   */
  getUnifiedStatementPaged(
    entityType: string,
    entityId: number,
    fromDate?: Date,
    toDate?: Date,
    page = 1,
    pageSize = 50
  ): Observable<{ items: StatementLine[]; totalCount: number }> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (fromDate) {
      params = params.set('fromDate', fromDate.toISOString().split('T')[0]);
    }
    if (toDate) {
      params = params.set('toDate', toDate.toISOString().split('T')[0]);
    }

    return this.http.get<{ data: { items: StatementLine[]; totalCount: number } }>(
      `${this.baseUrl}/statement/${entityType}/${entityId}/paged`,
      { params }
    ).pipe(map(res => res.data));
  }

  // ===================== تقرير صافي الأرباح - Net Profit Report =====================

  /**
   * تقرير صافي الأرباح الدوري
   */
  getNetProfitReport(
    fromDate: Date,
    toDate: Date,
    includeExpenseDetails = true
  ): Observable<NetProfitReport> {
    const params = new HttpParams()
      .set('fromDate', fromDate.toISOString().split('T')[0])
      .set('toDate', toDate.toISOString().split('T')[0])
      .set('includeExpenseDetails', includeExpenseDetails.toString());

    return this.http.get<{ data: NetProfitReport }>(
      `${this.baseUrl}/net-profit`,
      { params }
    ).pipe(map(res => res.data));
  }

  // ===================== تقييم المخزون - Inventory Valuation =====================

  /**
   * تقرير تقييم المخزون الذري
   */
  getInventoryValuation(query: InventoryValuationQuery = {}): Observable<InventoryValuation> {
    let params = new HttpParams();

    if (query.expiryFilter) params = params.set('expiryFilter', query.expiryFilter);
    if (query.medicineId) params = params.set('medicineId', query.medicineId.toString());
    if (query.categoryId) params = params.set('categoryId', query.categoryId.toString());
    if (query.search) params = params.set('search', query.search);
    if (query.page) params = params.set('page', query.page.toString());
    if (query.pageSize) params = params.set('pageSize', query.pageSize.toString());
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDirection) params = params.set('sortDirection', query.sortDirection);

    return this.http.get<{ data: InventoryValuation }>(
      `${this.baseUrl}/inventory-valuation`,
      { params }
    ).pipe(map(res => res.data));
  }

  /**
   * تقييم المخزون مع التصفح
   */
  getInventoryValuationPaged(
    query: InventoryValuationQuery = {}
  ): Observable<{ items: BatchValuation[]; totalCount: number }> {
    let params = new HttpParams();

    if (query.expiryFilter) params = params.set('expiryFilter', query.expiryFilter);
    if (query.medicineId) params = params.set('medicineId', query.medicineId.toString());
    if (query.categoryId) params = params.set('categoryId', query.categoryId.toString());
    if (query.search) params = params.set('search', query.search);
    if (query.page) params = params.set('page', query.page.toString());
    if (query.pageSize) params = params.set('pageSize', query.pageSize.toString());
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDirection) params = params.set('sortDirection', query.sortDirection);

    return this.http.get<{ data: { items: BatchValuation[]; totalCount: number } }>(
      `${this.baseUrl}/inventory-valuation/paged`,
      { params }
    ).pipe(map(res => res.data));
  }

  // ===================== ملخص التقارير - Reports Summary =====================

  /**
   * ملخص سريع للتقارير (للوحة التحكم)
   */
  getReportsSummary(): Observable<ReportsSummary> {
    return this.http.get<{ data: ReportsSummary }>(
      `${this.baseUrl}/summary`
    ).pipe(map(res => res.data));
  }

  // ===================== تصدير التقارير - Export Reports =====================

  /**
   * تصدير كشف الحساب بصيغة CSV
   */
  exportStatementCsv(
    entityType: string,
    entityId: number,
    fromDate?: Date,
    toDate?: Date
  ): Observable<Blob> {
    let params = new HttpParams().set('format', 'csv');

    if (fromDate) {
      params = params.set('fromDate', fromDate.toISOString().split('T')[0]);
    }
    if (toDate) {
      params = params.set('toDate', toDate.toISOString().split('T')[0]);
    }

    return this.http.get(
      `${this.baseUrl}/statement/${entityType}/${entityId}/export`,
      { params, responseType: 'blob' }
    );
  }

  /**
   * تنزيل ملف التصدير
   */
  downloadExport(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    window.URL.revokeObjectURL(url);
  }

  // ===================== تقرير المبيعات اليومية - Daily Sales Report =====================

  /**
   * تقرير المبيعات اليومية مع رسم بياني بالساعات
   */
  getDailySalesReport(date?: Date): Observable<DailySalesReport> {
    let params = new HttpParams();
    if (date) {
      params = params.set('date', date.toISOString().split('T')[0]);
    }

    return this.http.get<{ data: DailySalesReport }>(
      `${this.baseUrl}/daily-sales`,
      { params }
    ).pipe(map(res => res.data));
  }

  // ===================== تقرير الأدوية الأكثر مبيعاً - Best Selling =====================

  /**
   * تقرير الأدوية الأكثر مبيعاً
   */
  getBestSellingMedicines(
    fromDate: Date,
    toDate: Date,
    top: number = 10
  ): Observable<BestSellingMedicinesReport> {
    const params = new HttpParams()
      .set('fromDate', fromDate.toISOString().split('T')[0])
      .set('toDate', toDate.toISOString().split('T')[0])
      .set('top', top.toString());

    return this.http.get<{ data: BestSellingMedicinesReport }>(
      `${this.baseUrl}/best-selling`,
      { params }
    ).pipe(map(res => res.data));
  }

  // ===================== تقرير ديون العملاء - Customer Debts =====================

  /**
   * تقرير ديون العملاء - كم لهم وكم عليهم
   */
  getCustomerDebtsReport(): Observable<CustomerDebtsReport> {
    return this.http.get<{ data: CustomerDebtsReport }>(
      `${this.baseUrl}/customer-debts`
    ).pipe(map(res => res.data));
  }

  // ===================== تقرير ديون الموردين - Supplier Debts =====================

  /**
   * تقرير ديون الموردين - كم لهم وكم عليهم
   */
  getSupplierDebtsReport(): Observable<SupplierDebtsReport> {
    return this.http.get<{ data: SupplierDebtsReport }>(
      `${this.baseUrl}/supplier-debts`
    ).pipe(map(res => res.data));
  }
}

