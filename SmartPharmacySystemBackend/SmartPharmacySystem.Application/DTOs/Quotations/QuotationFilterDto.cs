using System;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Quotations;

/// <summary>
/// معايير تصفية والبحث في عروض الأسعار
/// </summary>
public class QuotationFilterDto
{
    public string? Search { get; set; }
    public int? BranchId { get; set; }
    public int? CustomerId { get; set; }
    public QuotationStatus? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
