using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.InventoryStocks;

/// <summary>
/// كائن نقل البيانات لإنشاء أو تحديث رصيد مخزون.
/// </summary>
public class CreateInventoryStockDto
{
    [Required(ErrorMessage = "المخزن مطلوب")]
    public int WarehouseId { get; set; }

    [Required(ErrorMessage = "الدواء مطلوب")]
    public int MedicineId { get; set; }

    [Required(ErrorMessage = "رقم التشغيلة مطلوب")]
    [MaxLength(100)]
    public string BatchNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "تاريخ الانتهاء مطلوب")]
    public DateTime ExpiryDate { get; set; }

    [Required(ErrorMessage = "الكمية مطلوبة")]
    [Range(0, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون صفر أو أكثر")]
    public int Quantity { get; set; }
}
