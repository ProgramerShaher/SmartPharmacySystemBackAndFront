using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Branches;

/// <summary>
/// كائن نقل البيانات لإنشاء فرع جديد.
/// </summary>
public class CreateBranchDto
{
    [Required(ErrorMessage = "كود الفرع مطلوب")]
    [MaxLength(50, ErrorMessage = "كود الفرع يجب أن لا يتجاوز 50 حرف")]
    public string BranchCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "اسم الفرع مطلوب")]
    [MaxLength(150, ErrorMessage = "اسم الفرع يجب أن لا يتجاوز 150 حرف")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(250, ErrorMessage = "الموقع يجب أن لا يتجاوز 250 حرف")]
    public string? Location { get; set; }

    [Required(ErrorMessage = "نوع الفرع مطلوب")]
    public BranchType BranchType { get; set; } = BranchType.Sub;

    public bool IsActive { get; set; } = true;
}
