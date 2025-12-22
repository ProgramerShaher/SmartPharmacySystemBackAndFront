using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.PurchaseReturnDetails;

/// <summary>
/// كائن نقل البيانات لتحديث تفصيل إرجاع شراء.
/// يحتوي على البيانات القابلة للتحديث لتفصيل إرجاع الشراء.
/// </summary>
public class UpdatePurchaseReturnDetailDto
{
    /// <summary>
    /// معرف تفصيل إرجاع الشراء
    /// </summary>
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// معرف إرجاع الشراء
    /// </summary>
    [Required]
    public int PurchaseReturnId { get; set; }

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
    /// سعر الشراء للوحدة
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "سعر الشراء يجب أن يكون أكبر من صفر")]
    public decimal PurchasePrice { get; set; }
}
