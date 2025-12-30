using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.SalesInvoiceDetails;

/// <summary>
/// كائن نقل البيانات لإنشاء تفصيل فاتورة بيع جديد.
/// يحتوي على البيانات المطلوبة لإنشاء تفصيل فاتورة بيع.
/// </summary>
public class CreateSaleInvoiceDetailDto
{
    /// <summary>
    /// معرف فاتورة البيع
    /// </summary>
    [Required]
    public int SaleInvoiceId { get; set; }

    /// <summary>
    /// معرف الدواء
    /// </summary>
    [Required]
    public int MedicineId { get; set; }

    /// <summary>
    /// معرف دفعة الدواء (اختياري، يحدده النظام آلياً بناءً على FEFO إذا لم يرسل)
    /// </summary>
    public int? BatchId { get; set; }

    /// <summary>
    /// الكمية
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من صفر")]
    public int Quantity { get; set; }

    /// <summary>
    /// سعر البيع للوحدة
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "سعر البيع يجب أن يكون أكبر من صفر")]
    public decimal SalePrice { get; set; }

}
