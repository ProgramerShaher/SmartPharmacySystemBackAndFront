namespace SmartPharmacySystem.Application.DTOs.Departments;

/// <summary>
/// كائن نقل البيانات لعرض قسم.
/// </summary>
public class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
}
