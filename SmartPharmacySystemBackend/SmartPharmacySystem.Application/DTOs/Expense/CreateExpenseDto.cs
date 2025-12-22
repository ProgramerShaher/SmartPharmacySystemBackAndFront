namespace SmartPharmacySystem.Application.DTOs.Expense;

/// <summary>
/// كائن نقل البيانات لإنشاء مصروف جديد.
/// يحتوي على البيانات المطلوبة لإنشاء المصروف.
/// </summary>
public class CreateExpenseDto
{
    /// <summary>
    /// نوع المصروف
    /// </summary>
    public string ExpenseType { get; set; } = string.Empty;

    /// <summary>
    /// مبلغ المصروف
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// تاريخ المصروف
    /// </summary>
    public DateTime ExpenseDate { get; set; }

    /// <summary>
    /// طريقة الدفع
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// ملاحظات إضافية
    /// </summary>
    public string Notes { get; set; } = string.Empty;
    public int CreatedBy { get; set; } 
}
