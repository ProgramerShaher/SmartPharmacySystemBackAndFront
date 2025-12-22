namespace SmartPharmacySystem.Application.DTOs.SalesReturnDetails;

/// <summary>
/// كائن نقل البيانات لتفصيل إرجاع البيع.
/// يحتوي على جميع بيانات تفصيل إرجاع البيع للعرض.
/// </summary>
public class SalesReturnDetailDto
{
    /// <summary>
    /// معرف التفصيل
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// معرف إرجاع البيع
    /// </summary>
    public int SalesReturnId { get; set; }

    /// <summary>
    /// تاريخ إرجاع البيع
    /// </summary>
    public DateTime ReturnDate { get; set; }

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
    /// سعر البيع للوحدة
    /// </summary>
    public decimal SalePrice { get; set; }

    /// <summary>
    /// تكلفة الوحدة
    /// </summary>
    public decimal UnitCost { get; set; }

    /// <summary>
    /// المبلغ الإجمالي للإرجاع
    /// </summary>
    public decimal TotalReturn { get; set; }

    /// <summary>
    /// هل محذوف
    /// </summary>
    public bool IsDeleted { get; set; }
}