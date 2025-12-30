using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.SalesInvoiceDetails;

/// <summary>
/// كائن نقل البيانات لتحديث تفصيل فاتورة بيع.
/// يحتوي على البيانات القابلة للتحديث لتفصيل فاتورة البيع.
/// </summary>
public class UpdateSaleInvoiceDetailDto
{
    /// <summary>
    /// معرف تفصيل فاتورة البيع
    /// </summary>
    [Required]
    public int Id { get; set; }

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
    /// معرف دفعة الدواء
    /// </summary>
    [Required]
    public int BatchId { get; set; }

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
    public decimal SalePrice { get; set; }
}
