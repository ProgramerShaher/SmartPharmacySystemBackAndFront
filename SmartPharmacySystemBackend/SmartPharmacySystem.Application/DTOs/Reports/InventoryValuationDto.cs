namespace SmartPharmacySystem.Application.DTOs.Reports;

/// <summary>
/// تقرير تقييم المخزون الذري
/// Atomic Inventory Valuation Report
/// </summary>
public class InventoryValuationDto
{
    /// <summary>
    /// تاريخ التقرير
    /// Report Date
    /// </summary>
    public DateTime ReportDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// إجمالي رأس المال السائل (الكمية × سعر الشراء)
    /// Total Capital (Quantity × Purchase Price)
    /// </summary>
    public decimal TotalCapital { get; set; }

    /// <summary>
    /// إجمالي القيمة البيعية (الكمية × سعر البيع)
    /// Total Retail Value (Quantity × Retail Price)
    /// </summary>
    public decimal TotalRetailValue { get; set; }

    /// <summary>
    /// الربح المحتمل = القيمة البيعية - رأس المال
    /// Potential Profit = Retail Value - Capital
    /// </summary>
    public decimal PotentialProfit => TotalRetailValue - TotalCapital;

    /// <summary>
    /// نسبة الربح المحتمل
    /// Potential Profit Margin
    /// </summary>
    public decimal PotentialProfitMargin => TotalCapital != 0 
        ? Math.Round((PotentialProfit / TotalCapital) * 100, 2) : 0;

    /// <summary>
    /// إجمالي عدد الدفعات
    /// Total Batch Count
    /// </summary>
    public int TotalBatches { get; set; }

    /// <summary>
    /// الدفعات النشطة
    /// Active Batches
    /// </summary>
    public int ActiveBatches { get; set; }

    /// <summary>
    /// الدفعات المنتهية الصلاحية
    /// Expired Batches
    /// </summary>
    public int ExpiredBatches { get; set; }

    /// <summary>
    /// الدفعات القريبة من الانتهاء (خلال 30 يوم)
    /// Expiring Soon Batches (within 30 days)
    /// </summary>
    public int ExpiringSoonBatches { get; set; }

    /// <summary>
    /// قيمة المخزون المنتهي
    /// Expired Stock Value
    /// </summary>
    public decimal ExpiredStockValue { get; set; }

    /// <summary>
    /// إجمالي الكمية المتوفرة
    /// Total Available Quantity
    /// </summary>
    public int TotalQuantity { get; set; }

    /// <summary>
    /// تفاصيل الدفعات
    /// Batch Details
    /// </summary>
    public List<BatchValuationDto> Batches { get; set; } = new();
}

/// <summary>
/// تقييم دفعة واحدة
/// Single Batch Valuation
/// </summary>
public class BatchValuationDto
{
    /// <summary>
    /// معرف الدفعة
    /// Batch ID
    /// </summary>
    public int BatchId { get; set; }

    /// <summary>
    /// معرف الدواء
    /// Medicine ID
    /// </summary>
    public int MedicineId { get; set; }

    /// <summary>
    /// اسم الدواء
    /// Medicine Name
    /// </summary>
    public string MedicineName { get; set; } = string.Empty;

    /// <summary>
    /// الاسم العلمي
    /// Scientific Name
    /// </summary>
    public string? ScientificName { get; set; }

    /// <summary>
    /// رقم دفعة الشركة
    /// Company Batch Number (Lot Number)
    /// </summary>
    public string CompanyBatchNumber { get; set; } = string.Empty;

    /// <summary>
    /// باركود الدفعة
    /// Batch Barcode
    /// </summary>
    public string? BatchBarcode { get; set; }

    /// <summary>
    /// تاريخ الانتهاء
    /// Expiry Date
    /// </summary>
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// الكمية المتبقية
    /// Remaining Quantity
    /// </summary>
    public int RemainingQuantity { get; set; }

    /// <summary>
    /// سعر الوحدة للشراء
    /// Unit Purchase Price
    /// </summary>
    public decimal UnitPurchasePrice { get; set; }

    /// <summary>
    /// سعر البيع
    /// Retail Price
    /// </summary>
    public decimal RetailPrice { get; set; }

    /// <summary>
    /// رأس مال الدفعة = الكمية × سعر الشراء
    /// Batch Capital = Quantity × Purchase Price
    /// </summary>
    public decimal BatchCapital => RemainingQuantity * UnitPurchasePrice;

    /// <summary>
    /// القيمة البيعية للدفعة = الكمية × سعر البيع
    /// Batch Retail Value = Quantity × Retail Price
    /// </summary>
    public decimal BatchRetailValue => RemainingQuantity * RetailPrice;

    /// <summary>
    /// الربح المحتمل للدفعة
    /// Batch Potential Profit
    /// </summary>
    public decimal BatchProfit => BatchRetailValue - BatchCapital;

    /// <summary>
    /// عدد الأيام حتى الانتهاء
    /// Days Until Expiry
    /// </summary>
    public int DaysUntilExpiry => (ExpiryDate.Date - DateTime.UtcNow.Date).Days;

    /// <summary>
    /// حالة الصلاحية
    /// Expiry Status
    /// </summary>
    public string ExpiryStatus => DaysUntilExpiry < 0 ? "منتهي" :
                                  DaysUntilExpiry <= 7 ? "حرج" :
                                  DaysUntilExpiry <= 30 ? "قريب" : "جيد";

    /// <summary>
    /// لون حالة الصلاحية
    /// Expiry Status Color
    /// </summary>
    public string ExpiryStatusColor => DaysUntilExpiry < 0 ? "danger" :
                                       DaysUntilExpiry <= 7 ? "danger" :
                                       DaysUntilExpiry <= 30 ? "warning" : "success";

    /// <summary>
    /// معرف فاتورة الشراء
    /// Purchase Invoice ID
    /// </summary>
    public int? PurchaseInvoiceId { get; set; }

    /// <summary>
    /// تاريخ الإدخال
    /// Entry Date
    /// </summary>
    public DateTime EntryDate { get; set; }

    /// <summary>
    /// موقع التخزين
    /// Storage Location
    /// </summary>
    public string? StorageLocation { get; set; }
}

/// <summary>
/// استعلام تقييم المخزون
/// Inventory Valuation Query Parameters
/// </summary>
public class InventoryValuationQueryDto
{
    /// <summary>
    /// فلترة حسب حالة الصلاحية: all, active, expired, expiring
    /// Filter by Expiry Status
    /// </summary>
    public string ExpiryFilter { get; set; } = "active";

    /// <summary>
    /// معرف الدواء (اختياري)
    /// Medicine ID (Optional)
    /// </summary>
    public int? MedicineId { get; set; }

    /// <summary>
    /// معرف الفئة (اختياري)
    /// Category ID (Optional)
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// البحث بالاسم أو الباركود
    /// Search by Name or Barcode
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// رقم الصفحة
    /// Page Number
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// حجم الصفحة
    /// Page Size
    /// </summary>
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// ترتيب حسب: expiryDate, capital, quantity
    /// Sort By
    /// </summary>
    public string SortBy { get; set; } = "expiryDate";

    /// <summary>
    /// اتجاه الترتيب: asc, desc
    /// Sort Direction
    /// </summary>
    public string SortDirection { get; set; } = "asc";
}
