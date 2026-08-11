using System.Collections.Generic;

namespace SmartPharmacySystem.Application.DTOs.Shifts;

public class ShiftDetailsDto
{
    public ShiftSummaryDto Summary { get; set; } = null!;
    
    public IEnumerable<ShiftInvoiceDto> Invoices { get; set; } = new List<ShiftInvoiceDto>();
    public IEnumerable<ShiftExpenseDto> Expenses { get; set; } = new List<ShiftExpenseDto>();
    public IEnumerable<ShiftReturnDto> Returns { get; set; } = new List<ShiftReturnDto>();
}

public class ShiftInvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}

public class ShiftExpenseDto
{
    public int Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}

public class ShiftReturnDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}
