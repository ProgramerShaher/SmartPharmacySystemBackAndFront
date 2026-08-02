using System.Collections.Generic;

namespace SmartPharmacySystem.Application.DTOs.Employees;

public class BranchEmployeesDashboardDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int TotalEmployees { get; set; }
    public decimal TotalBranchSalaries { get; set; }
    
    public List<DepartmentEmployeesDto> Departments { get; set; } = new List<DepartmentEmployeesDto>();
}
