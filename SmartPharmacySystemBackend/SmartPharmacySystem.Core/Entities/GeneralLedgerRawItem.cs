using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

public class GeneralLedgerRawItem
{
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public FinancialTransactionType Type { get; set; }
    public int ReferenceId { get; set; }
    public ReferenceType ReferenceType { get; set; }
    public string? ExpenseCategory { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
}
