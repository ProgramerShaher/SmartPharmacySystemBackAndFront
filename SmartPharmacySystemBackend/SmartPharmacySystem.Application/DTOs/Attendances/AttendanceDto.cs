using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Attendances;

/// <summary>
/// كائن نقل البيانات لعرض سجل حضور.
/// </summary>
public class AttendanceDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int WorkingBranchId { get; set; }
    public string WorkingBranchName { get; set; } = string.Empty;
    public DateTime CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public decimal? WorkedHours { get; set; }
    public ShiftType Shift { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public AttendanceStatus AttendanceStatus { get; set; }
    public string AttendanceStatusName { get; set; } = string.Empty;
}
