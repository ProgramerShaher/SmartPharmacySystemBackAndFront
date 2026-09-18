using System;
using System.Collections.Generic;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Quotations;

/// <summary>
/// كائن نقل البيانات الكامل لعرض السعر
/// </summary>
public class QuotationDto
{
    public int Id { get; set; }
    public int? BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string QuotationNumber { get; set; } = string.Empty;
    public DateTime QuotationDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public QuotationStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public bool IsTaxInclusive { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }
    public int? ConvertedSaleInvoiceId { get; set; }
    public string? ConvertedSaleInvoiceNumber { get; set; }
    public DateTime? ConvertedAt { get; set; }
    public int? ConvertedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<QuotationDetailDto> Details { get; set; } = new();
}
