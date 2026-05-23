using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.DamagedGoods;

/// <summary>
/// كائن نقل البيانات للاستعلام عن سجلات التالف.
/// </summary>
public class DamagedGoodsQueryDto
{
    public int? SourceWarehouseId { get; set; }
    public int? BranchId { get; set; }
    public int? MedicineId { get; set; }
    public DamageType? DamageType { get; set; }
    public RecordStatus? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
