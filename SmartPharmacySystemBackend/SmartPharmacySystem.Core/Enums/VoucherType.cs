namespace SmartPharmacySystem.Core.Enums;

/// <summary>
/// أنواع السندات / القيود المحاسبية
/// </summary>
public enum VoucherType
{
    /// <summary>
    /// قيد يومية عام
    /// </summary>
    JournalEntry = 1,

    /// <summary>
    /// سند صرف (نقدي/بنكي)
    /// </summary>
    PaymentVoucher = 2,

    /// <summary>
    /// سند قبض (نقدي/بنكي)
    /// </summary>
    ReceiptVoucher = 3,

    /// <summary>
    /// قيد تسوية
    /// </summary>
    AdjustmentEntry = 4,

    /// <summary>
    /// قيد إقفال
    /// </summary>
    ClosingEntry = 5,

    /// <summary>
    /// فاتورة مبيعات
    /// </summary>
    SalesInvoice = 6,

    /// <summary>
    /// فاتورة مشتريات
    /// </summary>
    PurchaseInvoice = 7
}
