namespace SmartPharmacySystem.Application.DTOs.Dashboard;

/// <summary>
/// Master Dashboard Statistics - Single Source of Truth
/// Optimized for <100ms response time with all KPIs
/// </summary>
public class MasterDashboardStatsDto
{
    public SystemOverviewDto SystemOverview { get; set; } = new();

    // ============================================
    // 1. رادار الرخاء المالي (Financial Intelligence Hub)
    // ============================================

    /// <summary>
    /// صافي الربح الفعلي = (المبيعات المعتمدة - المرتجعات) - COGS - المصاريف
    /// Net Profit = (Approved Sales - Returns) - COGS - Expenses
    /// </summary>
    public FinancialIntelligenceDto FinancialIntelligence { get; set; } = new();

    // ============================================
    // 2. الاستخبارات المخزنية واللوجستية (Inventory & Logistics IQ)
    // ============================================

    /// <summary>
    /// تقييم رأس المال الساكن - Inventory valuation
    /// </summary>
    public InventoryIntelligenceDto InventoryIntelligence { get; set; } = new();

    // ============================================
    // 3. الرقابة التشغيلية وأداء الموظفين (Operational Pulse)
    // ============================================

    /// <summary>
    /// نبض النظام - System activity and performance
    /// </summary>
    public OperationalPulseDto OperationalPulse { get; set; } = new();
}

/// <summary>
/// Financial Intelligence Hub Data
/// </summary>
public class FinancialIntelligenceDto
{
    /// <summary>
    /// صافي الربح الفعلي اليوم
    /// Today's Net Profit
    /// </summary>
    public decimal TodayNetProfit { get; set; }

    /// <summary>
    /// إجمالي المبيعات المعتمدة اليوم
    /// </summary>
    public decimal TodayApprovedSales { get; set; }

    /// <summary>
    /// المرتجعات اليوم
    /// </summary>
    public decimal TodayReturns { get; set; }

    /// <summary>
    /// تكلفة البضاعة المباعة اليوم
    /// </summary>
    public decimal TodayCOGS { get; set; }

    /// <summary>
    /// المصاريف اليوم
    /// </summary>
    public decimal TodayExpenses { get; set; }

    public decimal TotalSalesLast30Days { get; set; }
    public decimal TotalPurchasesLast30Days { get; set; }
    public decimal TotalReturnsLast30Days { get; set; }
    public decimal TotalExpensesLast30Days { get; set; }
    public decimal NetProfitLast30Days { get; set; }

    /// <summary>
    /// السيولة النقدية (Cash + Bank)
    /// Total Liquidity
    /// </summary>
    public decimal TotalLiquidity { get; set; }

    /// <summary>
    /// رصيد الصندوق
    /// </summary>
    public decimal CashBalance { get; set; }

    /// <summary>
    /// رصيد البنك
    /// </summary>
    public decimal BankBalance { get; set; }

    /// <summary>
    /// ما لنا عند العملاء (Customer Receivables)
    /// </summary>
    public decimal CustomerReceivables { get; set; }

    /// <summary>
    /// ما علينا للموردين (Supplier Payables)
    /// </summary>
    public decimal SupplierPayables { get; set; }

    /// <summary>
    /// المديونية الصافية = Receivables - Payables
    /// Net Debt (positive = we are owed more, negative = we owe more)
    /// </summary>
    public decimal NetDebt { get; set; }

    /// <summary>
    /// تدفق السيولة الداخلة لآخر 30 يوم
    /// Daily cash inflow for last 30 days
    /// </summary>
    public List<DailyCashFlowDto> CashFlowInLast30Days { get; set; } = new();

    /// <summary>
    /// تدفق السيولة الخارجة لآخر 30 يوم
    /// Daily cash outflow for last 30 days
    /// </summary>
    public List<DailyCashFlowDto> CashFlowOutLast30Days { get; set; } = new();

    /// <summary>
    /// إيرادات الفروع (إذا كان النظام يدعم فروع متعددة)
    /// Revenue by branch
    /// </summary>
    public List<BranchRevenueDto> BranchRevenues { get; set; } = new();
}

/// <summary>
/// Daily Cash Flow Entry
/// </summary>
public class DailyCashFlowDto
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
}

/// <summary>
/// Branch Revenue Entry
/// </summary>
public class BranchRevenueDto
{
    public string BranchName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
}

/// <summary>
/// Inventory Intelligence Data
/// </summary>
public class InventoryIntelligenceDto
{
    /// <summary>
    /// إجمالي قيمة المخزون بسعر التكلفة
    /// Total inventory value at cost price
    /// </summary>
    public decimal TotalInventoryValue { get; set; }
    public int TotalMedicines { get; set; }
    public int TotalBatches { get; set; }
    public int ActiveBatches { get; set; }
    public int ExpiredBatches { get; set; }
    public int NearExpiryBatches { get; set; }
    public int TotalStockQuantity { get; set; }
    public int ActiveAlerts { get; set; }

    /// <summary>
    /// توزيع المخزون حسب الموردين
    /// Inventory value by supplier
    /// </summary>
    public List<SupplierInventoryDto> InventoryBySupplier { get; set; } = new();

    /// <summary>
    /// رادار الصلاحية - نسبة الأدوية حسب قرب انتهاء الصلاحية
    /// Expiry Radar - Percentage by expiry proximity
    /// </summary>
    public ExpiryRadarDto ExpiryRadar { get; set; } = new();

    /// <summary>
    /// الأصناف الحرجة - وصلت لنقطة إعادة الطلب
    /// Critical stock items at reorder point
    /// </summary>
    public List<CriticalStockItemDto> CriticalStockItems { get; set; } = new();
}

/// <summary>
/// Supplier Inventory Distribution
/// </summary>
public class SupplierInventoryDto
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public decimal InventoryValue { get; set; }
    public int ItemCount { get; set; }
}

/// <summary>
/// Expiry Radar with color-coded percentages
/// </summary>
public class ExpiryRadarDto
{
    /// <summary>
    /// نسبة الأدوية التي تنتهي في أقل من 3 أشهر (أحمر)
    /// Percentage expiring in less than 3 months (Red)
    /// </summary>
    public decimal PercentageLessThan3Months { get; set; }

    /// <summary>
    /// نسبة الأدوية التي تنتهي في 3-6 أشهر (برتقالي)
    /// Percentage expiring in 3-6 months (Orange)
    /// </summary>
    public decimal Percentage3To6Months { get; set; }

    /// <summary>
    /// نسبة الأدوية التي تنتهي في 6-12 شهر (أصفر)
    /// Percentage expiring in 6-12 months (Yellow)
    /// </summary>
    public decimal Percentage6To12Months { get; set; }

    /// <summary>
    /// نسبة الأدوية التي تنتهي في أكثر من سنة (أخضر)
    /// Percentage expiring in more than 12 months (Green)
    /// </summary>
    public decimal PercentageMoreThan12Months { get; set; }
}

/// <summary>
/// Critical Stock Item - at or below reorder point
/// </summary>
public class CriticalStockItemDto
{
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int ReorderPoint { get; set; }
    public int SuggestedOrderQuantity { get; set; }
    public int? PreferredSupplierId { get; set; }
    public string? PreferredSupplierName { get; set; }
}

/// <summary>
/// Operational Pulse Data
/// </summary>
public class OperationalPulseDto
{
    /// <summary>
    /// نبض النظام - آخر 10 عمليات
    /// Activity stream - last 10 operations
    /// </summary>
    public List<ActivityStreamItemDto> ActivityStream { get; set; } = new();

    /// <summary>
    /// أداء الكاشيرات في الوردية الحالية
    /// Cashier performance in current shift
    /// </summary>
    public List<CashierPerformanceDto> CashierPerformance { get; set; } = new();

    /// <summary>
    /// الخريطة الحرارية - المبيعات حسب الساعة
    /// Heat map - sales by hour
    /// </summary>
    public List<HourlyHeatMapDto> HourlyHeatMap { get; set; } = new();
}

public class SystemOverviewDto
{
    public int SalesInvoicesCount { get; set; }
    public int PurchaseInvoicesCount { get; set; }
    public int SalesReturnsCount { get; set; }
    public int PurchaseReturnsCount { get; set; }
    public int MedicinesCount { get; set; }
    public int CustomersCount { get; set; }
    public int SuppliersCount { get; set; }
    public int UsersCount { get; set; }
    public int ActiveAlertsCount { get; set; }
    public int CriticalStockCount { get; set; }
    public int TodayDocumentsCount { get; set; }
}

/// <summary>
/// Activity Stream Item
/// </summary>
public class ActivityStreamItemDto
{
    public string OperationType { get; set; } = string.Empty; // "SaleInvoice", "PurchaseInvoice", "Return", "Payment"
    public string DocumentNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int ReferenceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string SourceRoute { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
}

/// <summary>
/// Cashier Performance in Current Shift
/// </summary>
public class CashierPerformanceDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }
    public int InvoiceCount { get; set; }
    public decimal AverageInvoiceValue { get; set; }
}

/// <summary>
/// Hourly Sales Heat Map
/// </summary>
public class HourlyHeatMapDto
{
    public int Hour { get; set; } // 0-23
    public decimal TotalSales { get; set; }
    public int TransactionCount { get; set; }
}
