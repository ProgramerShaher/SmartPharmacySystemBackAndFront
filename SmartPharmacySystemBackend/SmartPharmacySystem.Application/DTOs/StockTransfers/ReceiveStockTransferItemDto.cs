using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.StockTransfers;

/// <summary>
/// كائن نقل البيانات لبند استلام تحويل.
/// </summary>
public class ReceiveStockTransferItemDto
{
    [Required]
    public int StockTransferItemId { get; set; }

    [Required(ErrorMessage = "الكمية المستلمة مطلوبة")]
    [Range(0, double.MaxValue, ErrorMessage = "الكمية يجب أن تكون صفر أو أكثر")]
    public decimal QuantityReceived { get; set; }
}
