using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.Departments;

/// <summary>
/// كائن نقل البيانات لإنشاء قسم جديد.
/// </summary>
public class CreateDepartmentDto
{
    [Required(ErrorMessage = "اسم القسم مطلوب")]
    [MaxLength(100, ErrorMessage = "اسم القسم يجب أن لا يتجاوز 100 حرف")]
    public string Name { get; set; } = string.Empty;
}
