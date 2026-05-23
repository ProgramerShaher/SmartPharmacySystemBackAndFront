using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.StockTransfers;

/// <summary>
/// كائن نقل البيانات لتأكيد استلام سند تحويل.
/// </summary>
public class ReceiveStockTransferDto
{
    [Required(ErrorMessage = "يجب إدخال الكميات المستلمة")]
    public List<ReceiveStockTransferItemDto> Items { get; set; } = new();

    [MaxLength(500)]
    public string? Notes { get; set; }
}
