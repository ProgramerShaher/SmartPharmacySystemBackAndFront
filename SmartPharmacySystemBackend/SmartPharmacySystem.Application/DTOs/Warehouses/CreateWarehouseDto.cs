using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Warehouses;

/// <summary>
/// كائن نقل البيانات لإنشاء مخزن جديد.
/// </summary>
public class CreateWarehouseDto
{
    [Required(ErrorMessage = "الفرع مطلوب")]
    public int BranchId { get; set; }

    [Required(ErrorMessage = "نوع المخزن مطلوب")]
    public WarehouseType Type { get; set; }

    [Required(ErrorMessage = "اسم المخزن مطلوب")]
    [MaxLength(150, ErrorMessage = "اسم المخزن يجب أن لا يتجاوز 150 حرف")]
    public string Name { get; set; } = string.Empty;
}
