using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.InterBranchSettlements;

/// <summary>
/// كائن نقل البيانات لإنشاء تسوية بين فروع.
/// </summary>
public class CreateInterBranchSettlementDto
{
    [Required(ErrorMessage = "الفرع المصدر مطلوب")]
    public int FromBranchId { get; set; }

    [Required(ErrorMessage = "الفرع الوجهة مطلوب")]
    public int ToBranchId { get; set; }

    [Required(ErrorMessage = "المبلغ مطلوب")]
    [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
    public decimal Amount { get; set; }

    public DateTime SettlementDate { get; set; } = DateTime.Now;
}
