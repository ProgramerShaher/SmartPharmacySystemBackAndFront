using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.StockTransfers;

/// <summary>
/// كائن نقل البيانات لإنشاء طلب تحويل مخزني جديد.
/// </summary>
public class CreateStockTransferDto
{
    [Required(ErrorMessage = "المخزن المصدر مطلوب")]
    public int SourceWarehouseId { get; set; }

    public int? DestinationWarehouseId { get; set; }

    public int? DestinationBranchId { get; set; }

    public TransferType TransferType { get; set; } = TransferType.Manual;

    [MaxLength(500, ErrorMessage = "الملاحظات يجب أن لا تتجاوز 500 حرف")]
    public string? Notes { get; set; }

    [Required(ErrorMessage = "يجب إضافة بند واحد على الأقل")]
    [MinLength(1, ErrorMessage = "يجب إضافة بند واحد على الأقل")]
    public List<CreateStockTransferItemDto> Items { get; set; } = new();
}
