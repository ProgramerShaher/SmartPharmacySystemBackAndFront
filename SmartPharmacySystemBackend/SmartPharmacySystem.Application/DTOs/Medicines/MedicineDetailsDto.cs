using SmartPharmacySystem.Application.DTOs.MedicineBatch;

namespace SmartPharmacySystem.Application.DTOs.Medicine;

/// <summary>
/// تفاصيل الدواء مع الدفعات المرتبطة.
/// </summary>
public class MedicineDetailsDto : MedicineDto
{
    /// <summary>
    /// قائمة الدفعات المرتبطة بالدواء.
    /// </summary>
    public List<MedicineBatchDetailDto> Batches { get; set; } = new();
}

/// <summary>
/// تفاصيل الدفعة للعرض في شاشة تفاصيل الدواء.
/// </summary>
public class MedicineBatchDetailDto
{
    /// <summary>
    /// رقم التشغيلة (رقم الدفعة)
    /// </summary>
    public string BatchNumber { get; set; } = string.Empty;

    /// <summary>
    /// تاريخ الانتهاء
    /// </summary>
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// الكمية المتبقية
    /// </summary>
    public int RemainingQuantity { get; set; }

    /// <summary>
    /// حالة التنبيه (مثلاً: "LowStock", "Critical", "Normal")
    /// </summary>
    public string AlertStatus { get; set; } = string.Empty;
    
    /// <summary>
    /// لون الحالة (للعرض في الواجهة)
    /// </summary>
    public string StatusColor { get; set; } = string.Empty;
}
