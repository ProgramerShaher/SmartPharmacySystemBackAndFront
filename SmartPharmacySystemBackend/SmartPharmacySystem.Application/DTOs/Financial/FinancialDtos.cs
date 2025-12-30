using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Application.DTOs.Shared;

namespace SmartPharmacySystem.Application.DTOs.Financial;

public class PharmacyAccountDto
{
    public int Id { get; set; }
    public decimal Balance { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class FinancialTransactionDto
{
    public int Id { get; set; }
    public FinancialTransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public int? RelatedInvoiceId { get; set; }
}

public class FinancialInvoiceDto
{
    public int Id { get; set; }
    public FinancialInvoiceType Type { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime Date { get; set; }
}

public class FinancialTransactionQueryDto : BaseQueryDto
{
    public FinancialTransactionType? Type { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class FinancialInvoiceQueryDto : BaseQueryDto
{
    public FinancialInvoiceType? Type { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class FinancialReportDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetProfit { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal CurrentBalance { get; set; }
}
