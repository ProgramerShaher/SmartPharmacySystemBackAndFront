namespace SmartPharmacySystem.Application.DTOs.Reports;

/// <summary>
/// تقرير الأدوية الأكثر مبيعاً
/// Best Selling Medicines Report DTO
/// </summary>
public class BestSellingMedicinesReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    /// <summary>إجمالي الأدوية المباعة</summary>
    public int TotalMedicinesSold { get; set; }

    /// <summary>إجمالي الإيرادات</summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>قائمة الأدوية الأكثر مبيعاً</summary>
    public List<BestSellingMedicineDto> Medicines { get; set; } = new();

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>تفاصيل الدواء الأكثر مبيعاً</summary>
public class BestSellingMedicineDto
{
    /// <summary>الترتيب</summary>
    public int Rank { get; set; }

    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string? ScientificName { get; set; }
    public string? CategoryName { get; set; }

    /// <summary>الكمية المباعة</summary>
    public int QuantitySold { get; set; }

    /// <summary>إجمالي الإيرادات</summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>إجمالي الربح</summary>
    public decimal TotalProfit { get; set; }

    /// <summary>نسبة الربح</summary>
    public decimal ProfitMargin => TotalRevenue > 0 ? Math.Round((TotalProfit / TotalRevenue) * 100, 2) : 0;

    /// <summary>متوسط سعر البيع</summary>
    public decimal AverageSellingPrice => QuantitySold > 0 ? Math.Round(TotalRevenue / QuantitySold, 2) : 0;

    /// <summary>عدد الفواتير</summary>
    public int InvoiceCount { get; set; }

    /// <summary>تاريخ آخر بيع</summary>
    public DateTime? LastSaleDate { get; set; }
}
