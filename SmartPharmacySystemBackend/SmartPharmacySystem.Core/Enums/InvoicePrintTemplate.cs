namespace SmartPharmacySystem.Core.Enums;

/// <summary>
/// يحدد قالب الطباعة الافتراضي لفواتير المبيعات
/// </summary>
public enum InvoicePrintTemplate
{
    /// <summary>
    /// طابعة فواتير حرارية مقاس 80 مم (صيدليات، سوبرماركت، بقالة، كاشير سريع)
    /// </summary>
    Thermal80mm = 1,

    /// <summary>
    /// نموذج فاتورة رسمية قياس A4 (مواد بناء، جملة، إلكترونيات، شركات)
    /// </summary>
    A4Formal = 2,

    /// <summary>
    /// نموذج فاتورة مبسط قياس A4
    /// </summary>
    A4Simple = 3
}
