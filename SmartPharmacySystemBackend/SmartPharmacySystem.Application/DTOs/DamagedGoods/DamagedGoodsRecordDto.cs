using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.DamagedGoods;

/// <summary>
/// كائن نقل البيانات لعرض سجل تالف.
/// </summary>
public class DamagedGoodsRecordDto
{
    public int Id { get; set; }
    public string DamageCode { get; set; } = string.Empty;
    public int SourceWarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int Quantity { get; set; }
    public DamageType DamageType { get; set; }
    public string DamageTypeName { get; set; } = string.Empty;
    public decimal DamageValue { get; set; }
    public DisposalMethod DisposalMethod { get; set; }
    public string DisposalMethodName { get; set; } = string.Empty;
    public RecordStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public int? ApprovedByUserId { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int RecordedByUserId { get; set; }
    public string RecordedByName { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
}
