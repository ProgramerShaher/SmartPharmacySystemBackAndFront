using System.Collections.Generic;

namespace SmartPharmacySystem.Application.DTOs.Employees;

public class DepartmentEmployeesDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public decimal TotalBasicSalary { get; set; }
    public List<EmployeeDto> Employees { get; set; } = new List<EmployeeDto>();
}
