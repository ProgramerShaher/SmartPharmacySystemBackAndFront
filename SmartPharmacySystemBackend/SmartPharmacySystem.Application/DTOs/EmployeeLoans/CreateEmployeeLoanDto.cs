using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.EmployeeLoans;

/// <summary>
/// كائن نقل البيانات لإنشاء سلفة موظف.
/// </summary>
public class CreateEmployeeLoanDto
{
    [Required(ErrorMessage = "الموظف مطلوب")]
    public int EmployeeId { get; set; }

    [Required(ErrorMessage = "مبلغ السلفة مطلوب")]
    [Range(1, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "القسط الشهري مطلوب")]
    [Range(1, double.MaxValue, ErrorMessage = "القسط يجب أن يكون أكبر من صفر")]
    public decimal MonthlyInstalment { get; set; }

    [Required(ErrorMessage = "شهر البدء مطلوب")]
    [Range(1, 12)]
    public int StartMonth { get; set; }

    [Required(ErrorMessage = "سنة البدء مطلوبة")]
    [Range(2020, 2100)]
    public int StartYear { get; set; }
}
