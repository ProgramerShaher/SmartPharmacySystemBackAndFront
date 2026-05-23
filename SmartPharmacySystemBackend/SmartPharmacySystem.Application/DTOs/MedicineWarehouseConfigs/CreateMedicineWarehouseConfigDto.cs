using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.MedicineWarehouseConfigs;

/// <summary>
/// كائن نقل البيانات لإنشاء إعدادات دواء بمخزن.
/// </summary>
public class CreateMedicineWarehouseConfigDto
{
    [Required(ErrorMessage = "المخزن مطلوب")]
    public int WarehouseId { get; set; }

    [Required(ErrorMessage = "الدواء مطلوب")]
    public int MedicineId { get; set; }

    [Required(ErrorMessage = "حد الأمان مطلوب")]
    [Range(0, int.MaxValue, ErrorMessage = "حد الأمان يجب أن يكون صفر أو أكثر")]
    public int ReorderLevel { get; set; }

    [Required(ErrorMessage = "كمية إعادة الطلب مطلوبة")]
    [Range(1, int.MaxValue, ErrorMessage = "كمية إعادة الطلب يجب أن تكون 1 أو أكثر")]
    public int ReorderQuantity { get; set; }
}
