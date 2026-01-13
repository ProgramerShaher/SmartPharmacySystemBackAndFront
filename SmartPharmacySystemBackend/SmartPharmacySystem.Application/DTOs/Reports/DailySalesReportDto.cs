namespace SmartPharmacySystem.Application.DTOs.Reports;

/// <summary>
/// تقرير المبيعات اليومية
/// Daily Sales Report DTO
/// </summary>
public class DailySalesReportDto
{
    /// <summary>تاريخ التقرير</summary>
    public DateTime Date { get; set; }

    /// <summary>إجمالي المبيعات</summary>
    public decimal TotalSales { get; set; }

    /// <summary>إجمالي التكلفة</summary>
    public decimal TotalCost { get; set; }

    /// <summary>إجمالي الربح</summary>
    public decimal GrossProfit => TotalSales - TotalCost;

    /// <summary>نسبة الربح</summary>
    public decimal ProfitMargin => TotalSales > 0 ? Math.Round((GrossProfit / TotalSales) * 100, 2) : 0;

    /// <summary>عدد الفواتير</summary>
    public int InvoiceCount { get; set; }

    /// <summary>عدد الأصناف المباعة</summary>
    public int ItemsSold { get; set; }

    /// <summary>متوسط قيمة الفاتورة</summary>
    public decimal AverageInvoiceValue => InvoiceCount > 0 ? Math.Round(TotalSales / InvoiceCount, 2) : 0;

    /// <summary>المبيعات نقداً</summary>
    public decimal CashSales { get; set; }

    /// <summary>المبيعات آجل</summary>
    public decimal CreditSales { get; set; }

    /// <summary>المبيعات حسب الساعة</summary>
    public List<HourlySalesDto> SalesByHour { get; set; } = new();

    /// <summary>أكثر 5 أصناف مبيعاً</summary>
    public List<TopSellingItemDto> TopSellingItems { get; set; } = new();

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>المبيعات حسب الساعة</summary>
public class HourlySalesDto
{
    public int Hour { get; set; }
    public decimal Amount { get; set; }
    public int InvoiceCount { get; set; }
}

/// <summary>أكثر الأصناف مبيعاً</summary>
public class TopSellingItemDto
{
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}
