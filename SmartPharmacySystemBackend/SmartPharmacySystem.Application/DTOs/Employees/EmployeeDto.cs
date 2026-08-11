using System;

namespace SmartPharmacySystem.Application.DTOs.Employees;

public class EmployeeDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public decimal BasicSalary { get; set; }
    public bool IsActive { get; set; }

    public int? UserId { get; set; }
    public string? UserName { get; set; }

    public SmartPharmacySystem.Core.Enums.ShiftType Shift { get; set; } = SmartPharmacySystem.Core.Enums.ShiftType.Morning;
    public string ShiftName { get; set; } = string.Empty;
    public TimeSpan? ShiftStartTime { get; set; }
    public TimeSpan? ShiftEndTime { get; set; }
    public decimal WorkingHours { get; set; } = 8;
    
    public decimal TotalLoans { get; set; }
    public decimal RemainingLoans { get; set; }
}
