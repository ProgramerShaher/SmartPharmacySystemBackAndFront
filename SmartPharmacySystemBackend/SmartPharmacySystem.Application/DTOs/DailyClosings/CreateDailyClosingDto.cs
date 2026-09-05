using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.DailyClosings;

/// <summary>
/// كائن نقل البيانات لإنشاء إغلاق يومي.
/// يحتوي على التاريخ فقط — جميع القيم المالية تُحسب تلقائياً من الـ backend.
/// </summary>
public class CreateDailyClosingDto
{
    public int BranchId { get; set; }

    [Required(ErrorMessage = "تاريخ الإغلاق مطلوب")]
    public DateTime ClosingDate { get; set; }
}
