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
    [Range(0, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون صفر أو أكثر")]
    public int QuantityReceived { get; set; }
}
