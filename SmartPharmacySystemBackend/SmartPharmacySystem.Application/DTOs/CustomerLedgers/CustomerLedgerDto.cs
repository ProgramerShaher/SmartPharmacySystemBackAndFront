using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.CustomerLedgers;

/// <summary>
/// كائن نقل البيانات لعرض حركة في دفتر العميل.
/// </summary>
public class CustomerLedgerDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public CustomerTransactionType TransactionType { get; set; }
    public string TransactionTypeName { get; set; } = string.Empty;
    public int ReferenceId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningBalance { get; set; }
}
