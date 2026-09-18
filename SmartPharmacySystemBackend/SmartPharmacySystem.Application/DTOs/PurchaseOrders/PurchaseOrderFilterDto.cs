using System;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.PurchaseOrders;

/// <summary>
/// معايير تصفية والبحث في أوامر الشراء
/// </summary>
public class PurchaseOrderFilterDto
{
    public string? Search { get; set; }
    public int? BranchId { get; set; }
    public int? SupplierId { get; set; }
    public PurchaseOrderStatus? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
