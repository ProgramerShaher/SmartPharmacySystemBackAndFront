using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.PurchaseReturnDetails;

/// <summary>
/// كائن نقل البيانات لإنشاء تفصيل إرجاع شراء جديد.
/// يحتوي على البيانات المطلوبة لإنشاء تفصيل إرجاع شراء.
/// </summary>
public class CreatePurchaseReturnDetailDto
{
    // PurchaseReturnId removed - inferred from parent


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