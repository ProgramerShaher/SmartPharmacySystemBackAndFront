namespace SmartPharmacySystem.Application.DTOs.Reports;

/// <summary>
/// تقرير صافي الأرباح الدوري
/// Periodic Net Profit Report
/// </summary>
public class NetProfitReportDto
{
    /// <summary>
    /// من تاريخ
    /// From Date
    /// </summary>
    public DateTime FromDate { get; set; }

    /// <summary>
    /// إلى تاريخ
    /// To Date
    /// </summary>
    public DateTime ToDate { get; set; }

    /// <summary>
    /// تاريخ إنشاء التقرير
    /// Report Generation Date
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    // ===================== الإيرادات - Revenue =====================

    /// <summary>
    /// إجمالي المبيعات
    /// Gross Sales
    /// </summary>
    public decimal GrossSales { get; set; }

    /// <summary>
    /// عدد فواتير المبيعات
    /// Sales Invoice Count
    /// </summary>
    public int SalesInvoiceCount { get; set; }

    /// <summary>
    /// مرتجعات المبيعات
    /// Sales Returns
    /// </summary>
    public decimal SalesReturns { get; set; }

    /// <summary>
    /// خصومات المبيعات
    /// Sales Discounts
    /// </summary>
    public decimal SalesDiscounts { get; set; }

    /// <summary>
    /// صافي المبيعات = إجمالي المبيعات - المرتجعات - الخصومات
    /// Net Sales = Gross Sales - Returns - Discounts
    /// </summary>
    public decimal NetSales => GrossSales - SalesReturns - SalesDiscounts;

    // ===================== تكلفة البضاعة المباعة - COGS =====================

    /// <summary>
    /// تكلفة البضاعة المباعة (من سعر الشراء)
    /// Cost of Goods Sold (from Purchase Price)
    /// </summary>
    public decimal CostOfGoodsSold { get; set; }

    /// <summary>
    /// إجمالي الربح = صافي المبيعات - تكلفة البضاعة المباعة
    /// Gross Profit = Net Sales - COGS
    /// </summary>
    public decimal GrossProfit => NetSales - CostOfGoodsSold;

    /// <summary>
    /// نسبة هامش الربح الإجمالي
    /// Gross Profit Margin Percentage
    /// </summary>
    public decimal GrossProfitMargin => NetSales != 0 ? Math.Round((GrossProfit / NetSales) * 100, 2) : 0;

    // ===================== المصروفات التشغيلية - Operating Expenses =====================

    /// <summary>
    /// إجمالي المصروفات التشغيلية
    /// Total Operating Expenses
    /// </summary>
    public decimal TotalExpenses { get; set; }

    /// <summary>
    /// تفصيل المصروفات حسب الفئة
    /// Expenses Breakdown by Category
    /// </summary>
    public List<ExpenseBreakdownDto> ExpensesByCategory { get; set; } = new();

    // ===================== صافي الربح - Net Profit =====================

    /// <summary>
    /// صافي الربح = إجمالي الربح - المصروفات التشغيلية
    /// Net Profit = Gross Profit - Operating Expenses
    /// </summary>
    public decimal NetProfit => GrossProfit - TotalExpenses;

    /// <summary>
    /// نسبة هامش صافي الربح
    /// Net Profit Margin Percentage
    /// </summary>
    public decimal NetProfitMargin => NetSales != 0 ? Math.Round((NetProfit / NetSales) * 100, 2) : 0;

    /// <summary>
    /// حالة الربحية: ربح أو خسارة
    /// Profitability Status: Profit or Loss
    /// </summary>
    public string ProfitStatus => NetProfit >= 0 ? "ربح" : "خسارة";

    /// <summary>
    /// لون الحالة للعرض
    /// Status Color for Display
    /// </summary>
    public string StatusColor => NetProfit >= 0 ? "success" : "danger";

    // ===================== ملخص إضافي - Additional Summary =====================

    /// <summary>
    /// إجمالي المشتريات (للمقارنة)
    /// Total Purchases (for comparison)
    /// </summary>
    public decimal TotalPurchases { get; set; }

    /// <summary>
    /// مرتجعات المشتريات
    /// Purchase Returns
    /// </summary>
    public decimal PurchaseReturns { get; set; }

    /// <summary>
    /// صافي المشتريات
    /// Net Purchases
    /// </summary>
    public decimal NetPurchases => TotalPurchases - PurchaseReturns;
}

/// <summary>
/// تفصيل المصروفات حسب الفئة
/// Expense Breakdown by Category
/// </summary>
public class ExpenseBreakdownDto
{
    /// <summary>
    /// معرف الفئة
    /// Category ID
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// اسم الفئة
    /// Category Name
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// المبلغ
    /// Amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// النسبة المئوية من إجمالي المصروفات
    /// Percentage of Total Expenses
    /// </summary>
    public double Percentage { get; set; }

    /// <summary>
    /// لون الفئة للعرض
    /// Category Color for Display
    /// </summary>
    public string Color { get; set; } = "primary";
}

/// <summary>
/// استعلام تقرير صافي الأرباح
/// Net Profit Report Query Parameters
/// </summary>
public class NetProfitQueryDto
{
    /// <summary>
    /// من تاريخ (مطلوب)
    /// From Date (Required)
    /// </summary>
    public DateTime FromDate { get; set; }

    /// <summary>
    /// إلى تاريخ (مطلوب)
    /// To Date (Required)
    /// </summary>
    public DateTime ToDate { get; set; }

    /// <summary>
    /// تضمين تفاصيل المصروفات
    /// Include Expense Details
    /// </summary>
    public bool IncludeExpenseDetails { get; set; } = true;
}
