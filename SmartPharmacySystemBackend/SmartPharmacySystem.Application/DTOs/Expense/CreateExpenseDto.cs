using SmartPharmacySystem.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.Expense;

/// <summary>
/// كائن نقل البيانات لإنشاء مصروف جديد.
/// يحتوي على البيانات المطلوبة لإنشاء المصروف.
/// </summary>
public class CreateExpenseDto
{
    /// <summary>
    /// معرف الحساب المرتبط بهذا المصروف
    /// </summary>
    public int? AccountId { get; set; }

    /// <summary>
    /// معرف فئة المصروف
    /// </summary>
    [Required]
    public int CategoryId { get; set; }

    /// <summary>
    /// مبلغ المصروف
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    /// <summary>
    /// تاريخ المصروف
    /// </summary>
    [Required]
    public DateTime ExpenseDate { get; set; }

    /// <summary>
    /// طريقة الدفع
    /// </summary>
    public PaymentType PaymentMethod { get; set; } = PaymentType.Cash;

    /// <summary>
    /// ملاحظات إضافية
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    public int CreatedBy { get; set; }
}
