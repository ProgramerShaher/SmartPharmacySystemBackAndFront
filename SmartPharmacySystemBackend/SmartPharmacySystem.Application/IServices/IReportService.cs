using SmartPharmacySystem.Application.DTOs.Reports;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Application.Interfaces;

/// <summary>
/// خدمة التقارير المركزية
/// Central Reporting Service Interface
/// </summary>
public interface IReportService
{
    // ===================== كشف الحساب الموحد - Unified Statement =====================

    /// <summary>
    /// الحصول على كشف الحساب الكامل لعميل أو مورد
    /// Get complete account statement for Customer or Supplier
    /// </summary>
    /// <param name="entityType">نوع الكيان: Customer أو Supplier</param>
    /// <param name="entityId">معرف الكيان</param>
    /// <param name="fromDate">من تاريخ (اختياري)</param>
    /// <param name="toDate">إلى تاريخ (اختياري)</param>
    /// <returns>كشف الحساب مع الرصيد التراكمي</returns>
    Task<UnifiedStatementDto> GetUnifiedStatementAsync(
        string entityType,
        int entityId,
        DateTime? fromDate = null,
        DateTime? toDate = null);

    /// <summary>
    /// الحصول على كشف الحساب مع التصفح
    /// Get paginated account statement
    /// </summary>
    Task<PagedResponse<StatementLineDto>> GetUnifiedStatementPagedAsync(
        string entityType,
        int entityId,
        DateTime? fromDate,
        DateTime? toDate,
        int page = 1,
        int pageSize = 50);

    // ===================== تقرير صافي الأرباح - Net Profit Report =====================

    /// <summary>
    /// الحصول على تقرير صافي الأرباح الدوري
    /// Get periodic net profit report
    /// معادلة: صافي الربح = (إجمالي المبيعات - المرتجعات - الخصومات) - تكلفة البضاعة المباعة - المصروفات
    /// Formula: Net Profit = (Gross Sales - Returns - Discounts) - COGS - Expenses
    /// </summary>
    /// <param name="fromDate">من تاريخ</param>
    /// <param name="toDate">إلى تاريخ</param>
    /// <param name="includeExpenseDetails">تضمين تفاصيل المصروفات حسب الفئة</param>
    /// <returns>تقرير صافي الأرباح</returns>
    Task<NetProfitReportDto> GetNetProfitReportAsync(
        DateTime fromDate,
        DateTime toDate,
        bool includeExpenseDetails = true);

    // ===================== تقييم المخزون - Inventory Valuation =====================

    /// <summary>
    /// الحصول على تقرير تقييم المخزون الذري
    /// Get atomic inventory valuation report
    /// يربط كل حبة دواء بـ BatchId و ExpiryDate و PurchasePrice
    /// </summary>
    /// <param name="query">معاملات الاستعلام</param>
    /// <returns>تقرير تقييم المخزون</returns>
    Task<InventoryValuationDto> GetInventoryValuationAsync(InventoryValuationQueryDto query);

    /// <summary>
    /// الحصول على تقييم المخزون مع التصفح
    /// Get paginated inventory valuation
    /// </summary>
    Task<PagedResponse<BatchValuationDto>> GetInventoryValuationPagedAsync(InventoryValuationQueryDto query);

    // ===================== تقارير إضافية - Additional Reports =====================

    /// <summary>
    /// تقرير المبيعات اليومية
    /// Daily Sales Report
    /// </summary>
    /// <param name="date">تاريخ التقرير</param>
    /// <returns>تقرير المبيعات اليومية</returns>
    Task<DailySalesReportDto> GetDailySalesReportAsync(DateTime date);

    /// <summary>
    /// تقرير الأدوية الأكثر مبيعاً
    /// Best Selling Medicines Report
    /// </summary>
    /// <param name="fromDate">من تاريخ</param>
    /// <param name="toDate">إلى تاريخ</param>
    /// <param name="top">عدد الأدوية المطلوب</param>
    /// <returns>قائمة الأدوية الأكثر مبيعاً</returns>
    Task<BestSellingMedicinesReportDto> GetBestSellingMedicinesAsync(DateTime fromDate, DateTime toDate, int top = 10);

    /// <summary>
    /// تقرير ديون العملاء
    /// Customer Debts Report
    /// </summary>
    /// <returns>تقرير ديون العملاء</returns>
    Task<CustomerDebtsReportDto> GetCustomerDebtsReportAsync();

    /// <summary>
    /// تقرير ديون الموردين
    /// Supplier Debts Report
    /// </summary>
    /// <returns>تقرير ديون الموردين</returns>
    Task<SupplierDebtsReportDto> GetSupplierDebtsReportAsync();

    Task<EmployeePerformanceReportDto> GetEmployeePerformanceReportAsync(EmployeePerformanceReportQueryDto query);

    /// <summary>
    /// الحصول على ملخص سريع للتقارير
    /// Get quick reports summary (for dashboard)
    /// </summary>
    Task<ReportsSummaryDto> GetReportsSummaryAsync();

    // ===================== القوائم المالية النهائية - Financial Statements =====================
    Task<TrialBalanceDto> GetTrialBalanceAsync(DateTime fromDate, DateTime toDate);
    Task<IncomeStatementDto> GetIncomeStatementAsync(DateTime fromDate, DateTime toDate);
    Task<BalanceSheetDto> GetBalanceSheetAsync(DateTime date);
}

/// <summary>
/// ملخص سريع للتقارير
/// Quick Reports Summary for Dashboard
/// </summary>
public class ReportsSummaryDto
{
    /// <summary>
    /// إجمالي ديون العملاء
    /// Total Customer Debts
    /// </summary>
    public decimal TotalCustomerDebts { get; set; }

    /// <summary>
    /// إجمالي ديون الموردين
    /// Total Supplier Debts
    /// </summary>
    public decimal TotalSupplierDebts { get; set; }

    /// <summary>
    /// رأس المال في المخزون
    /// Inventory Capital
    /// </summary>
    public decimal InventoryCapital { get; set; }

    /// <summary>
    /// صافي الربح للشهر الحالي
    /// Current Month Net Profit
    /// </summary>
    public decimal CurrentMonthNetProfit { get; set; }

    /// <summary>
    /// عدد الدفعات القريبة من الانتهاء
    /// Expiring Soon Batch Count
    /// </summary>
    public int ExpiringSoonBatches { get; set; }
}
