using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.Quotations;

/// <summary>
/// كائن إنشاء عرض سعر جديد
/// </summary>
public class CreateQuotationDto
{
    public int? BranchId { get; set; }

    public DateTime QuotationDate { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiryDate { get; set; }

    public int? CustomerId { get; set; }

    [MaxLength(200)]
    public string? CustomerName { get; set; }

    [MaxLength(50)]
    public string? CustomerPhone { get; set; }

    [MaxLength(100)]
    [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
    public string? CustomerEmail { get; set; }

    public decimal TaxRate { get; set; } = 15;

    public bool IsTaxInclusive { get; set; } = true;

    public decimal TotalDiscount { get; set; } = 0;

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [MaxLength(2000)]
    public string? TermsAndConditions { get; set; }

    [Required(ErrorMessage = "يجب إضافة بند واحد على الأقل في عرض السعر")]
    [MinLength(1, ErrorMessage = "يجب إضافة بند واحد على الأقل في عرض السعر")]
    public List<CreateQuotationDetailDto> Details { get; set; } = new();
}
