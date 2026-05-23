using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.CustomerLedgers;

/// <summary>
/// كائن نقل البيانات لتسجيل حركة في دفتر العميل.
/// </summary>
public class CreateCustomerLedgerDto
{
    [Required(ErrorMessage = "العميل مطلوب")]
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "الفرع مطلوب")]
    public int BranchId { get; set; }

    [Required(ErrorMessage = "نوع العملية مطلوب")]
    public CustomerTransactionType TransactionType { get; set; }

    [Required(ErrorMessage = "معرف المرجع مطلوب")]
    public int ReferenceId { get; set; }

    [Required(ErrorMessage = "المبلغ مطلوب")]
    [Range(0, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون صفر أو أكثر")]
    public decimal Amount { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.Now;
}
