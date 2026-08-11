using System;

namespace SmartPharmacySystem.Application.DTOs.Shifts;

public class ShiftSummaryDto
{
    public int ShiftId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public decimal OpeningCash { get; set; }
    
    // Sales
    public decimal TotalSalesCash { get; set; }
    public decimal TotalSalesNetwork { get; set; }
    public decimal TotalSalesCredit { get; set; }
    public int InvoicesCount { get; set; }

    // Returns
    public decimal TotalReturnsCash { get; set; }
    
    // Receipts & Payments
    public decimal TotalCustomerReceiptsCash { get; set; }
    public decimal TotalSupplierPaymentsCash { get; set; }
    
    // Expenses
    public decimal TotalExpensesCash { get; set; }

    // Reconciliation
    public decimal ExpectedClosingCash { get; set; }
    public decimal? ActualClosingCash { get; set; }
    public decimal Difference { get; set; }
}
