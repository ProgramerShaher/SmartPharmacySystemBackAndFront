namespace SmartPharmacySystem.Application.DTOs.Employees;

public class UpdateEmployeeDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public int DepartmentId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public decimal BasicSalary { get; set; }
    public bool IsActive { get; set; }

    /// <summary>
    /// رابط حساب المستخدم بالنظام (اختياري)
    /// </summary>
    public int? UserId { get; set; }

    public SmartPharmacySystem.Core.Enums.ShiftType Shift { get; set; } = SmartPharmacySystem.Core.Enums.ShiftType.Morning;
    public TimeSpan? ShiftStartTime { get; set; }
    public TimeSpan? ShiftEndTime { get; set; }
    public decimal WorkingHours { get; set; } = 8;
}
