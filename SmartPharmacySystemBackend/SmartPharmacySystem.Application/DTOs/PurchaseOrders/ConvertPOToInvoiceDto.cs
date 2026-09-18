using System;
using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.PurchaseOrders;

/// <summary>
/// كائن طلب تحويل أمر الشراء إلى فاتورة مشتريات رسمية
/// </summary>
public class ConvertPOToInvoiceDto
{
    [MaxLength(100)]
    public string? SupplierInvoiceNumber { get; set; }

    public DateTime? PurchaseDate { get; set; } = DateTime.UtcNow;

    public PaymentType PaymentMethod { get; set; } = PaymentType.Cash;

    public int? WarehouseId { get; set; }

    public decimal? PaidAmount { get; set; }

    public decimal? ActualShippingCost { get; set; }

    public string? Notes { get; set; }
}
