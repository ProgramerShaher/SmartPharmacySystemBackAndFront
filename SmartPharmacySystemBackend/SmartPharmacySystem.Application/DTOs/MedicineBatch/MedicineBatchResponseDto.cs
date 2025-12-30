using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.MedicineBatch;

/// <summary>
/// Data Transfer Object for medicine batch response/display.
/// كائن نقل البيانات لاستجابة/عرض دفعة الدواء.
/// </summary>
public class MedicineBatchResponseDto
{
    /// <summary>
    /// Primary key - Unique identifier for the medicine batch.
    /// المفتاح الأساسي - المعرف الفريد لدفعة الدواء.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// Medicine ID to which this batch belongs.
    /// معرف الدواء الذي تنتمي إليه هذه الدفعة.
    /// </summary>
    /// <example>1</example>
    public int MedicineId { get; set; }

    /// <summary>
    /// Name of the medicine for display purposes.
    /// اسم الدواء لأغراض العرض.
    /// </summary>
    /// <example>Panadol Extra</example>
    public string MedicineName { get; set; } = string.Empty;

    /// <summary>
    /// Company batch/lot number provided by the manufacturer.
    /// رقم دفعة/لوط الشركة المقدم من الشركة المصنعة.
    /// </summary>
    /// <example>LOT-2024-001234</example>
    public string CompanyBatchNumber { get; set; } = string.Empty;

    /// <summary>
    /// Unique barcode for this batch.
    /// الباركود الفريد لهذه الدفعة.
    /// </summary>
    /// <example>6224001234567</example>
    public string BatchBarcode { get; set; } = string.Empty;

    /// <summary>
    /// Total quantity purchased in this batch.
    /// إجمالي الكمية المشتراة في هذه الدفعة.
    /// </summary>
    /// <example>100</example>
    public int Quantity { get; set; }

    /// <summary>
    /// Remaining quantity available for sale.
    /// الكمية المتبقية المتاحة للبيع.
    /// </summary>
    /// <example>75</example>
    public int RemainingQuantity { get; set; }

    /// <summary>
    /// Quantity that has been sold.
    /// الكمية التي تم بيعها.
    /// </summary>
    /// <example>25</example>
    public int SoldQuantity { get; set; }

    /// <summary>
    /// Expiry date of the batch.
    /// تاريخ انتهاء صلاحية الدفعة.
    /// </summary>
    /// <example>2025-12-31</example>
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// Date when the batch was entered/received.
    /// تاريخ إدخال/استلام الدفعة.
    /// </summary>
    /// <example>2024-01-15</example>
    public DateTime EntryDate { get; set; }

    /// <summary>
    /// Unit purchase price.
    /// سعر الشراء للوحدة.
    /// </summary>
    /// <example>10.50</example>
    public decimal UnitPurchasePrice { get; set; }

    /// <summary>
    /// Storage location within the pharmacy.
    /// موقع التخزين داخل الصيدلية.
    /// </summary>
    /// <example>Shelf A-3</example>
    public string? StorageLocation { get; set; }

    /// <summary>
    /// Current status of the batch.
    /// الحالة الحالية للدفعة.
    /// </summary>
    /// <example>Active</example>
    public string Status { get; set; } = string.Empty;

    // Status Tracking & Dynamic Colors
    public string StatusName { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public string StatusIcon { get; set; } = string.Empty;

    // Action Tracking (Last Action)
    public string ActionByName { get; set; } = string.Empty;
    public DateTime ActionDate { get; set; }

    /// <summary>
    /// Whether the batch is expired.
    /// هل الدفعة منتهية الصلاحية.
    /// </summary>
    /// <example>false</example>
    public bool IsExpired { get; set; }

    /// <summary>
    /// Whether the batch is expiring soon (within 60 days).
    /// هل الدفعة ستنتهي قريباً (خلال 60 يوماً).
    /// </summary>
    /// <example>true</example>
    public bool IsExpiringSoon { get; set; }

    /// <summary>
    /// Days until expiry. Negative means already expired.
    /// عدد الأيام حتى انتهاء الصلاحية. السالب يعني منتهية بالفعل.
    /// </summary>
    /// <example>180</example>
    public int DaysUntilExpiry { get; set; }

    /// <summary>
    /// Whether the batch can be sold (Available, has quantity, not expired).
    /// هل يمكن بيع الدفعة (متاحة، لديها كمية، غير منتهية).
    /// </summary>
    /// <example>true</example>
    public bool IsSellable { get; set; }

    /// <summary>
    /// ID of the user who created this batch.
    /// معرف المستخدم الذي أنشأ هذه الدفعة.
    /// </summary>
    /// <example>1</example>
    public int CreatedBy { get; set; }

    /// <summary>
    /// Name of the user who created this batch.
    /// اسم المستخدم الذي أنشأ هذه الدفعة.
    /// </summary>
    /// <example>Admin User</example>
    public string? CreatedByUserName { get; set; }

    /// <summary>
    /// Whether the batch is soft deleted.
    /// هل الدفعة محذوفة (حذف ناعم).
    /// </summary>
    /// <example>false</example>
    public bool IsDeleted { get; set; }
}
