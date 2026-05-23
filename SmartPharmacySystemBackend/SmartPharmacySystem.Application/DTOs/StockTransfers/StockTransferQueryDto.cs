using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.StockTransfers;

/// <summary>
/// كائن نقل البيانات للاستعلام عن سندات التحويل.
/// </summary>
public class StockTransferQueryDto
{
    public int? SourceWarehouseId { get; set; }
    public int? DestinationWarehouseId { get; set; }
    public int? SourceBranchId { get; set; }
    public int? DestinationBranchId { get; set; }
    public TransferStatus? Status { get; set; }
    public TransferType? TransferType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
