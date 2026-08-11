using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.Employees;

/// <summary>
/// كائن نقل البيانات لإضافة موظف جديد.
/// </summary>
public class CreateEmployeeDto
{
    [Required(ErrorMessage = "كود الموظف مطلوب")]
    [MaxLength(50, ErrorMessage = "كود الموظف يجب أن لا يتجاوز 50 حرف")]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    [MaxLength(150, ErrorMessage = "الاسم الكامل يجب أن لا يتجاوز 150 حرف")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "رقم الهوية مطلوب")]
    [MaxLength(50, ErrorMessage = "رقم الهوية يجب أن لا يتجاوز 50 حرف")]
    public string NationalId { get; set; } = string.Empty;

    [Required(ErrorMessage = "الفرع مطلوب")]
    public int BranchId { get; set; }

    [Required(ErrorMessage = "القسم مطلوب")]
    public int DepartmentId { get; set; }

    [Required(ErrorMessage = "المسمى الوظيفي مطلوب")]
    [MaxLength(100, ErrorMessage = "المسمى الوظيفي يجب أن لا يتجاوز 100 حرف")]
    public string JobTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "تاريخ التعيين مطلوب")]
    public DateTime HireDate { get; set; }

    [Required(ErrorMessage = "الراتب الأساسي مطلوب")]
    [Range(0, double.MaxValue, ErrorMessage = "الراتب يجب أن يكون صفر أو أكثر")]
    public decimal BasicSalary { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// رابط حساب المستخدم بالنظام (اختياري)
    /// </summary>
    public int? UserId { get; set; }

    [Required(ErrorMessage = "تحديد الوردية مطلوب")]
    public SmartPharmacySystem.Core.Enums.ShiftType Shift { get; set; } = SmartPharmacySystem.Core.Enums.ShiftType.Morning;

    public TimeSpan? ShiftStartTime { get; set; }
    public TimeSpan? ShiftEndTime { get; set; }

    [Range(0, 24, ErrorMessage = "ساعات العمل يجب أن تكون بين 0 و 24")]
    public decimal WorkingHours { get; set; } = 8;
}
