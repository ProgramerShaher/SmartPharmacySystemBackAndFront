using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.Quotations;

/// <summary>
/// كائن إدخال بند في عرض السعر
/// </summary>
public class CreateQuotationDetailDto
{
    [Required(ErrorMessage = "يرجى تحديد الصنف")]
    public int MedicineId { get; set; }

    public int? SaleUnitId { get; set; }

    [Required(ErrorMessage = "يرجى إدخال الكمية")]
    [Range(0.0001, 1000000, ErrorMessage = "الكمية يجب أن تكون أكبر من الصفر")]
    public decimal Quantity { get; set; }

    [Required(ErrorMessage = "يرجى إدخال سعر الوحدة")]
    [Range(0, 10000000, ErrorMessage = "سعر الوحدة يجب ألا يكون بالسالب")]
    public decimal UnitPrice { get; set; }

    [Range(0, 100, ErrorMessage = "نسبة الخصم يجب أن تكون بين 0 و 100")]
    public decimal DiscountPercentage { get; set; } = 0;

    public decimal DiscountAmount { get; set; } = 0;

    [Range(0, 100, ErrorMessage = "نسبة الضريبة يجب أن تكون بين 0 و 100")]
    public decimal TaxRate { get; set; } = 0;

    public string? Notes { get; set; }
}
