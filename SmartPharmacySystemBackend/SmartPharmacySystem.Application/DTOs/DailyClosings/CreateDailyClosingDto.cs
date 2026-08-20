using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.DailyClosings;

/// <summary>
/// كائن نقل البيانات لإنشاء مسودة إغلاق يومي.
/// </summary>
public class CreateDailyClosingDto
{
    public int BranchId { get; set; }

    [Required(ErrorMessage = "تاريخ الإغلاق مطلوب")]
    public DateTime ClosingDate { get; set; }

    [Required(ErrorMessage = "رصيد الصندوق الافتتاحي مطلوب")]
    [Range(0, double.MaxValue, ErrorMessage = "الرصيد يجب أن يكون صفر أو أكثر")]
    public decimal OpeningCash { get; set; }

    [Required(ErrorMessage = "الكاش الفعلي مطلوب")]
    [Range(0, double.MaxValue, ErrorMessage = "الكاش الفعلي يجب أن يكون صفر أو أكثر")]
    public decimal ActualCash { get; set; }

    public bool TransferToMainSafe { get; set; } = false;
}
