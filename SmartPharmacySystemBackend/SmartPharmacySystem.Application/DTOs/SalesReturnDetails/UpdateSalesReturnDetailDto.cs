using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.SalesReturnDetails;

/// <summary>
/// كائن نقل البيانات لتحديث تفصيل إرجاع بيع.
/// يحتوي على البيانات القابلة للتحديث لتفصيل إرجاع البيع.
/// </summary>
public class UpdateSalesReturnDetailDto
{
    /// <summary>
    /// معرف إرجاع البيع
    /// </summary>
    [Required]
    public int SalesReturnId { get; set; }

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
    [Range(0.01, double.MaxValue, ErrorMessage = "سعر البيع يجب أن يكون أكبر من صفر")]
    public decimal SalePrice { get; set; }
}