namespace SmartPharmacySystem.Application.DTOs.PurchaseReturnDetails;

/// <summary>
/// كائن نقل البيانات لتفصيل إرجاع الشراء.
/// يحتوي على جميع بيانات تفصيل إرجاع الشراء للعرض.
/// </summary>
public class PurchaseReturnDetailDto
{
    /// <summary>
    /// معرف التفصيل
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// معرف إرجاع الشراء
    /// </summary>
    public int PurchaseReturnId { get; set; }

    /// <summary>
    /// رقم إرجاع الشراء
    /// </summary>
    public string PurchaseReturnNumber { get; set; } = string.Empty;

    /// <summary>
    /// معرف الدواء
    /// </summary>
    public int MedicineId { get; set; }

    /// <summary>
    /// اسم الدواء
    /// </summary>
    public string MedicineName { get; set; } = string.Empty;

    /// <summary>
    /// معرف دفعة الدواء
    /// </summary>
    public int BatchId { get; set; }

    /// <summary>
    /// رقم دفعة الشركة
    /// </summary>
    public string CompanyBatchNumber { get; set; } = string.Empty;

    /// <summary>
    /// الكمية
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// سعر الشراء للوحدة
    /// </summary>
    public decimal PurchasePrice { get; set; }

    /// <summary>
    /// المجموع الكلي للإرجاع
    /// </summary>
    public decimal TotalReturn { get; set; }

    /// <summary>
    /// هل محذوف
    /// </summary>
    public bool IsDeleted { get; set; }

    // --- Calculated / Extra Fields ---

    /// <summary>
    /// تاريخ الانتهاء (من الدفعة)
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// الأيام المتبقية للصلاحية
    /// </summary>
    public int DaysUntilExpiry { get; set; }

    /// <summary>
    /// هل يمكن بيعه (قابل للصرف)
    /// </summary>
    public bool CanSell { get; set; }

    /// <summary>
    /// حالة الصلاحية (صالح / قريب الانتهاء / منتهي)
    /// </summary>
    public string ExpiryStatus { get; set; } = string.Empty;
}