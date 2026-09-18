using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.StockTransfers;

/// <summary>
/// كائن نقل البيانات لإنشاء بند في سند تحويل.
/// </summary>
public class CreateStockTransferItemDto
{
    [Required(ErrorMessage = "الدواء مطلوب")]
    public int MedicineId { get; set; }

    [Required(ErrorMessage = "رقم التشغيلة مطلوب")]
    [MaxLength(100)]
    public string BatchNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "تاريخ الانتهاء مطلوب")]
    public DateTime ExpiryDate { get; set; }

    [Required(ErrorMessage = "الكمية المطلوبة مطلوبة")]
    [Range(0.0001, double.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من صفر")]
    public decimal QuantityRequested { get; set; }
}
