namespace SmartPharmacySystem.Application.DTOs.Attendances;

/// <summary>
/// كائن نقل البيانات للاستعلام عن الحضور.
/// </summary>
public class AttendanceQueryDto
{
    public int? EmployeeId { get; set; }
    public int? BranchId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
