namespace SmartPharmacySystem.Core.Enums;

/// <summary>
/// حالات دورة حياة عرض السعر (Price Quotation / Proforma Invoice)
/// </summary>
public enum QuotationStatus
{
    /// <summary>
    /// مسودة تحت الإعداد
    /// </summary>
    Draft = 1,

    /// <summary>
    /// تم إرسال العرض للعميل
    /// </summary>
    Sent = 2,

    /// <summary>
    /// تم قبول العرض من العميل
    /// </summary>
    Accepted = 3,

    /// <summary>
    /// تم رفض العرض من العميل
    /// </summary>
    Rejected = 4,

    /// <summary>
    /// انتهت صلاحية العرض ولم يعد سارياً
    /// </summary>
    Expired = 5,

    /// <summary>
    /// تم تحويل العرض إلى فاتورة مبيعات معتمدة
    /// </summary>
    ConvertedToInvoice = 6
}
