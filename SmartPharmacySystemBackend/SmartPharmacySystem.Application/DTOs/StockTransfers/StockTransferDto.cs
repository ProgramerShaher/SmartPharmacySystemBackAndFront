using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.StockTransfers;

/// <summary>
/// كائن نقل البيانات لعرض سند تحويل مخزني.
/// </summary>
public class StockTransferDto
{
    public int Id { get; set; }
    public string TransferCode { get; set; } = string.Empty;
    public int SourceWarehouseId { get; set; }
    public string SourceWarehouseName { get; set; } = string.Empty;
    public string SourceBranchName { get; set; } = string.Empty;
    public int DestinationWarehouseId { get; set; }
    public string DestinationWarehouseName { get; set; } = string.Empty;
    public string DestinationBranchName { get; set; } = string.Empty;
    public TransferStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public TransferType TransferType { get; set; }
    public string TransferTypeName { get; set; } = string.Empty;
    public int RequestedByUserId { get; set; }
    public string RequestedByName { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public int? ApprovedByUserId { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? DispatchedByUserId { get; set; }
    public string? DispatchedByName { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public int? ReceivedByUserId { get; set; }
    public string? ReceivedByName { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public string? Notes { get; set; }
    public int ItemsCount { get; set; }
    public int TotalQuantityRequested { get; set; }
    public int TotalQuantityReceived { get; set; }
    public List<StockTransferItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
