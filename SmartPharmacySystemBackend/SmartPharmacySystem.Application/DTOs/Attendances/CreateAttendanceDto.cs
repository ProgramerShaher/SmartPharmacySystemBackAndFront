using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Attendances;

/// <summary>
/// كائن نقل البيانات لتسجيل حضور.
/// </summary>
public class CreateAttendanceDto
{
    [Required(ErrorMessage = "الموظف مطلوب")]
    public int EmployeeId { get; set; }

    [Required(ErrorMessage = "فرع العمل مطلوب")]
    public int WorkingBranchId { get; set; }

    [Required(ErrorMessage = "وقت الحضور مطلوب")]
    public DateTime CheckIn { get; set; }

    public DateTime? CheckOut { get; set; }

    public ShiftType Shift { get; set; } = ShiftType.Morning;

    public AttendanceStatus AttendanceStatus { get; set; } = AttendanceStatus.Present;
}
