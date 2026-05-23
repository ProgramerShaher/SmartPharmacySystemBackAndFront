namespace SmartPharmacySystem.Application.DTOs.Employees;

/// <summary>
/// كائن نقل البيانات للاستعلام عن الموظفين.
/// </summary>
public class EmployeeQueryDto
{
    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public bool? IsActive { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
