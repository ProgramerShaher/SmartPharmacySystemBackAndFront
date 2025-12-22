using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.PurchaseReturns;

/// <summary>
/// كائن نقل البيانات لإنشاء إرجاع شراء جديد.
/// يحتوي على البيانات المطلوبة لإنشاء إرجاع شراء.
/// </summary>
public class CreatePurchaseReturnDto
{
    /// <summary>
    /// معرف فاتورة الشراء
    /// </summary>
    [Required]
    public int PurchaseInvoiceId { get; set; }

    /// <summary>
    /// معرف المورد
    /// </summary>
    [Required]
    public int SupplierId { get; set; }

    /// <summary>
    /// تاريخ الإرجاع
    /// </summary>
    [Required]
    public DateTime ReturnDate { get; set; }

    /// <summary>
    /// سبب الإرجاع
    /// </summary>
    [Required]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "سبب الإرجاع يجب أن يكون بين 1 و 500 حرف")]
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// معرف المستخدم الذي أنشأ المرتجع
    /// ID of the user creating the return
    /// </summary>
    public int CreatedBy { get; set; }
}