using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.StockCounts;

/// <summary>
/// كائن نقل البيانات لإنشاء أمر جرد جديد.
/// </summary>
public class CreateStockCountHeaderDto
{
    [Required(ErrorMessage = "المخزن مطلوب")]
    public int WarehouseId { get; set; }

    [Required(ErrorMessage = "نوع الجرد مطلوب")]
    public StockCountType CountType { get; set; }

    [Required(ErrorMessage = "وقت اللقطة مطلوب")]
    public DateTime SnapshotAt { get; set; }

    [MaxLength(500, ErrorMessage = "الملاحظات يجب أن لا تتجاوز 500 حرف")]
    public string? Notes { get; set; }
}
