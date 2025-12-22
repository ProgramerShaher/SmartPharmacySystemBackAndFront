namespace SmartPharmacySystem.Core.Enums;

/// <summary>
/// Represents the status of a medicine batch in the pharmacy system.
/// يمثل حالة دفعة الدواء في نظام الصيدلية.
/// </summary>
public enum BatchStatus
{
    /// <summary>
    /// Batch is available for sale. | الدفعة متاحة للبيع.
    /// </summary>
    Available = 0,

    /// <summary>
    /// All units in the batch have been sold. | تم بيع جميع الوحدات في الدفعة.
    /// </summary>
    SoldOut = 1,

    /// <summary>
    /// Batch has expired and cannot be sold. | انتهت صلاحية الدفعة ولا يمكن بيعها.
    /// </summary>
    Expired = 2,

    /// <summary>
    /// Batch is damaged and cannot be sold. | الدفعة تالفة ولا يمكن بيعها.
    /// </summary>
    Damaged = 3,

    /// <summary>
    /// Batch is reserved/on hold. | الدفعة محجوزة/معلقة.
    /// </summary>
    Reserved = 4,

    /// <summary>
    /// Batch is quarantined pending quality check. | الدفعة في الحجر الصحي بانتظار فحص الجودة.
    /// </summary>
    Quarantined = 5
}
