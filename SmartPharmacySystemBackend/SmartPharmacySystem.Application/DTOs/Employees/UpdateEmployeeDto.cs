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
}
