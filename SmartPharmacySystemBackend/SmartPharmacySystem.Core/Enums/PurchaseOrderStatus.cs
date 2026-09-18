namespace SmartPharmacySystem.Core.Enums;

/// <summary>
/// حالات أمر الشراء (Purchase Order Status)
/// </summary>
public enum PurchaseOrderStatus
{
    /// <summary>
    /// مسودة - قيد الإعداد
    /// </summary>
    Draft = 0,

    /// <summary>
    /// تم إرسال أمر الشراء للمورد رسمياً
    /// </summary>
    SentToSupplier = 1,

    /// <summary>
    /// تم استلام جزء من البضاعة من المورد
    /// </summary>
    PartiallyReceived = 2,

    /// <summary>
    /// تم استلام البضاعة بالكامل وتحويل الأمر إلى فاتورة مشتريات
    /// </summary>
    Completed = 3,

    /// <summary>
    /// تم إلغاء أمر الشراء
    /// </summary>
    Cancelled = 4
}
