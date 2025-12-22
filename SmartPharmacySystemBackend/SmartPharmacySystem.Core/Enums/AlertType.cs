namespace SmartPharmacySystem.Core.Enums;

/// <summary>
/// Represents the type of alert in the pharmacy system.
/// Derived from ExpiryStatus for expiry-related alerts.
/// يمثل نوع التنبيه في نظام الصيدلية.
/// </summary>
public enum AlertType
{
    /// <summary>
    /// Batch expires within 1 week. | الدفعة تنتهي خلال أسبوع واحد.
    /// Maps to ExpiryStatus.OneWeek
    /// </summary>
    ExpiryOneWeek = 1,

    /// <summary>
    /// Batch expires within 2 weeks. | الدفعة تنتهي خلال أسبوعين.
    /// Maps to ExpiryStatus.TwoWeeks
    /// </summary>
    ExpiryTwoWeeks = 2,

    /// <summary>
    /// Batch expires within 1 month. | الدفعة تنتهي خلال شهر واحد.
    /// Maps to ExpiryStatus.OneMonth
    /// </summary>
    ExpiryOneMonth = 3,

    /// <summary>
    /// Batch expires within 2 months. | الدفعة تنتهي خلال شهرين.
    /// Maps to ExpiryStatus.TwoMonths
    /// </summary>
    ExpiryTwoMonths = 4,

    /// <summary>
    /// Batch has already expired (final alert). | الدفعة منتهية الصلاحية بالفعل (التنبيه النهائي).
    /// </summary>
    Expired = 5,

    /// <summary>
    /// Low stock warning (future use). | تحذير مخزون منخفض (للاستخدام المستقبلي).
    /// </summary>
    LowStock = 6,

    /// <summary>
    /// Damaged batch alert (future use). | تنبيه دفعة تالفة (للاستخدام المستقبلي).
    /// </summary>
    Damaged = 7
}
