namespace SmartPharmacySystem.Core.Enums;

/// <summary>
/// حالة الشيك في النظام
/// </summary>
public enum ChequeStatus
{
    /// <summary>
    /// بانتظار التحصيل / تحت التحصيل
    /// </summary>
    Pending = 1,

    /// <summary>
    /// تم التحصيل / صرف
    /// </summary>
    Collected = 2,

    /// <summary>
    /// شيك مرتجع
    /// </summary>
    Returned = 3,

    /// <summary>
    /// ملغي
    /// </summary>
    Cancelled = 4
}

/// <summary>
/// نوع الشيك (صادر من الصيدلية أم وارد إليها)
/// </summary>
public enum ChequeType
{
    /// <summary>
    /// شيك وارد (من عميل)
    /// </summary>
    Incoming = 1,

    /// <summary>
    /// شيك صادر (لمورد)
    /// </summary>
    Outgoing = 2
}
