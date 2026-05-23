using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Attendances;

public class UpdateAttendanceDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int WorkingBranchId { get; set; }
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public ShiftType Shift { get; set; }
    public AttendanceStatus AttendanceStatus { get; set; }
}
