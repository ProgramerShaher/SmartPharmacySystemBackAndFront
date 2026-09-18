using System;
using System.Collections.Generic;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.PurchaseOrders;

/// <summary>
/// كائن نقل البيانات الكامل لأمر الشراء
/// </summary>
public class PurchaseOrderDto
{
    public int Id { get; set; }
    public int? BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string? SupplierContact { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public bool IsTaxInclusive { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }
    public int? ConvertedPurchaseInvoiceId { get; set; }
    public string? ConvertedPurchaseInvoiceNumber { get; set; }
    public DateTime? ConvertedAt { get; set; }
    public int? ConvertedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<PurchaseOrderDetailDto> Details { get; set; } = new();
}
