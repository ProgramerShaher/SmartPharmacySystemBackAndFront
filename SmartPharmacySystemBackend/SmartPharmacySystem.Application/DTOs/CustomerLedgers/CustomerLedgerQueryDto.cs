namespace SmartPharmacySystem.Application.DTOs.CustomerLedgers;

/// <summary>
/// كائن نقل البيانات للاستعلام عن دفتر العميل.
/// </summary>
public class CustomerLedgerQueryDto
{
    public int CustomerId { get; set; }
    public int? BranchId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
