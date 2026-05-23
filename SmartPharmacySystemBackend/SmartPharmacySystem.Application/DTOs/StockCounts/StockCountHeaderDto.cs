using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.StockCounts;

/// <summary>
/// كائن نقل البيانات لعرض أمر جرد مخزني.
/// </summary>
public class StockCountHeaderDto
{
    public int Id { get; set; }
    public string CountCode { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public StockCountType CountType { get; set; }
    public string CountTypeName { get; set; } = string.Empty;
    public DateTime SnapshotAt { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public StockCountStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public int? ApprovedByUserId { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Notes { get; set; }
    public int ItemsCount { get; set; }
    public int TotalVarianceItems { get; set; }
    public decimal TotalVarianceValue { get; set; }
    public DateTime CreatedAt { get; set; }
}
