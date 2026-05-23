using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.InterBranchSettlements;

/// <summary>
/// كائن نقل البيانات لعرض تسوية بين فروع.
/// </summary>
public class InterBranchSettlementDto
{
    public int Id { get; set; }
    public int FromBranchId { get; set; }
    public string FromBranchName { get; set; } = string.Empty;
    public int ToBranchId { get; set; }
    public string ToBranchName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime SettlementDate { get; set; }
    public SettlementStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public int? SettledByUserId { get; set; }
    public string? SettledByUserName { get; set; }
}
