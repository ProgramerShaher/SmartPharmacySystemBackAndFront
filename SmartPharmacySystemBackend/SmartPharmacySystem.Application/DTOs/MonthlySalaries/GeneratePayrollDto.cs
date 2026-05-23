using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.MonthlySalaries;

/// <summary>
/// كائن نقل البيانات لتوليد رواتب شهر.
/// </summary>
public class GeneratePayrollDto
{
    [Required(ErrorMessage = "الشهر مطلوب")]
    [Range(1, 12, ErrorMessage = "الشهر يجب أن يكون بين 1 و 12")]
    public int Month { get; set; }

    [Required(ErrorMessage = "السنة مطلوبة")]
    [Range(2020, 2100, ErrorMessage = "السنة غير صالحة")]
    public int Year { get; set; }

    public int? BranchId { get; set; }
}
