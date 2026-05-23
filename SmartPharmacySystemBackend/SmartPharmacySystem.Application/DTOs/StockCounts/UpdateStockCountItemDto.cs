using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.StockCounts;

/// <summary>
/// كائن نقل البيانات لإدخال نتيجة جرد فعلية.
/// </summary>
public class UpdateStockCountItemDto
{
    [Required(ErrorMessage = "الكمية الفعلية مطلوبة")]
    [Range(0, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون صفر أو أكثر")]
    public int PhysicalQuantity { get; set; }

    [MaxLength(250)]
    public string? VarianceReason { get; set; }

    public string BatchNumber { get; set; } = string.Empty;

    public int MedicineId { get; set; } 
}
