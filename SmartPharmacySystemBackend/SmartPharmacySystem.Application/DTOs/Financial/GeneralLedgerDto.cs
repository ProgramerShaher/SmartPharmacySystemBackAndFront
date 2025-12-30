using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Financial;

public class GeneralLedgerDto
{
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Incoming { get; set; }
    public decimal Outgoing { get; set; }
    public decimal RunningBalance { get; set; }
    public int ReferenceId { get; set; }
    public ReferenceType ReferenceType { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
}
