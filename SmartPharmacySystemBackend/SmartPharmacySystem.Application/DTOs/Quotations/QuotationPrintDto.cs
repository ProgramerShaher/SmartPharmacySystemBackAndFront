using System;
using System.Collections.Generic;

namespace SmartPharmacySystem.Application.DTOs.Quotations;

/// <summary>
/// كائن نقل البيانات لطباعة عرض السعر بصيغة A4 رسمية
/// </summary>
public class QuotationPrintDto
{
    public int Id { get; set; }
    public string QuotationNumber { get; set; } = string.Empty;
    public DateTime QuotationDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string StatusName { get; set; } = string.Empty;

    // بيانات المنشأة / الفرع
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyTaxNumber { get; set; }
    public string? CompanyCommercialRegister { get; set; }
    public string? CompanyAddress { get; set; }
    public string? CompanyPhone { get; set; }
    public string? CompanyEmail { get; set; }
    public string? CompanyLogoUrl { get; set; }
    public string BranchName { get; set; } = string.Empty;

    // بيانات العميل
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerTaxNumber { get; set; }
    public string? CustomerAddress { get; set; }

    // المبالغ المالية
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public bool IsTaxInclusive { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalAmount { get; set; }

    // الشروط والملاحظات
    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }

    // البنود
    public List<QuotationDetailDto> Details { get; set; } = new();
}
