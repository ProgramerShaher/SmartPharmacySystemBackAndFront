namespace SmartPharmacySystem.Core.Enums;

/// <summary>
/// Represents the status of an alert in the pharmacy system.
/// يمثل حالة التنبيه في نظام الصيدلية.
/// </summary>
public enum AlertStatus
{
    /// <summary>
    /// Alert created but not yet acknowledged. | تنبيه تم إنشاؤه ولكن لم يتم الاطلاع عليه بعد.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Alert has been viewed/acknowledged. | تم عرض/الاطلاع على التنبيه.
    /// </summary>
    Read = 1,

    /// <summary>
    /// Alert explicitly dismissed by user. | تم رفض التنبيه صراحةً من قبل المستخدم.
    /// </summary>
    Dismissed = 2,

    /// <summary>
    /// Underlying issue resolved (e.g., batch sold/removed). | تم حل المشكلة الأساسية (مثل بيع/إزالة الدفعة).
    /// </summary>
    Resolved = 3
}
