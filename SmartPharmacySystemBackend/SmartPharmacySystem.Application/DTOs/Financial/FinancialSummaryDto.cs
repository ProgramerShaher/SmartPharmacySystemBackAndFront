namespace SmartPharmacySystem.Application.DTOs.Financial;

public class FinancialSummaryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public double Percentage { get; set; }
}
