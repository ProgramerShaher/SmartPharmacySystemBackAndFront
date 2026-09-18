using System;
using System.Collections.Generic;

namespace SmartPharmacySystem.Application.DTOs.PurchaseOrders;

/// <summary>
/// كائن نقل البيانات لطباعة أمر الشراء بصيغة رسمية A4
/// </summary>
public class PurchaseOrderPrintDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string StatusName { get; set; } = string.Empty;

    // بيانات المنشأة
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyTaxNumber { get; set; }
    public string? CompanyCommercialRegister { get; set; }
    public string? CompanyAddress { get; set; }
    public string? CompanyPhone { get; set; }
    public string? CompanyEmail { get; set; }
    public string? CompanyLogoUrl { get; set; }
    public string BranchName { get; set; } = string.Empty;

    // بيانات المورد
    public string? SupplierName { get; set; }
    public string? SupplierContact { get; set; }
    public string? SupplierAddress { get; set; }
    public string? SupplierTaxNumber { get; set; }

    // المبالغ
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public bool IsTaxInclusive { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TotalAmount { get; set; }

    // الشروط والملاحظات
    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }

    // البنود
    public List<PurchaseOrderDetailDto> Details { get; set; } = new();
}
