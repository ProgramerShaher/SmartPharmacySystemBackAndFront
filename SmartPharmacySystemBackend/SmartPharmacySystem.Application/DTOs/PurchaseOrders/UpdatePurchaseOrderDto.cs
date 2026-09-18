using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.PurchaseOrders;

/// <summary>
/// كائن تعديل أمر الشراء
/// </summary>
public class UpdatePurchaseOrderDto
{
    public DateTime? ExpectedDeliveryDate { get; set; }

    public int? SupplierId { get; set; }

    [MaxLength(200)]
    public string? SupplierName { get; set; }

    [MaxLength(100)]
    public string? SupplierContact { get; set; }

    public PurchaseOrderStatus? Status { get; set; }

    public decimal TaxRate { get; set; } = 0;

    public bool IsTaxInclusive { get; set; } = false;

    public decimal TotalDiscount { get; set; } = 0;

    public decimal ShippingCost { get; set; } = 0;

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [MaxLength(2000)]
    public string? TermsAndConditions { get; set; }

    [Required(ErrorMessage = "يجب إضافة صنف واحد على الأقل في أمر الشراء")]
    [MinLength(1, ErrorMessage = "يجب إضافة صنف واحد على الأقل في أمر الشراء")]
    public List<CreatePurchaseOrderDetailDto> Details { get; set; } = new();
}
