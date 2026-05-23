using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.DamagedGoods;

/// <summary>
/// كائن نقل البيانات لتسجيل تالف جديد.
/// </summary>
public class CreateDamagedGoodsRecordDto
{
    [Required(ErrorMessage = "المخزن مطلوب")]
    public int SourceWarehouseId { get; set; }

    [Required(ErrorMessage = "الدواء مطلوب")]
    public int MedicineId { get; set; }

    [Required(ErrorMessage = "رقم التشغيلة مطلوب")]
    [MaxLength(100)]
    public string BatchNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "تاريخ الانتهاء مطلوب")]
    public DateTime ExpiryDate { get; set; }

    [Required(ErrorMessage = "الكمية مطلوبة")]
    [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون 1 أو أكثر")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "نوع التلف مطلوب")]
    public DamageType DamageType { get; set; }

    [Required(ErrorMessage = "طريقة التخلص مطلوبة")]
    public DisposalMethod DisposalMethod { get; set; }
}
