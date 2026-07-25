using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents a packaging/sale unit for a medicine (e.g., Box, Strip, Carton).
/// يمثل وحدة تعبئة/بيع مضافة للدواء (مثل علبة، شريط، كرتون).
/// </summary>
public class MedicineUnit : BaseEntity
{
    /// <summary>
    /// Foreign key to the medicine this unit belongs to.
    /// </summary>
    [Required]
    public int MedicineId { get; set; }

    /// <summary>
    /// Name of the unit (e.g., Box, Strip, Carton).
    /// اسم الوحدة (مثل علبة، شريط، كرتون).
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// How many base units (pills) are in this unit.
    /// معامل التحويل (كم حبة تحتويها هذه الوحدة).
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int ConversionFactor { get; set; } = 1;

    /// <summary>
    /// Default purchase price for this unit.
    /// سعر الشراء الافتراضي لهذه الوحدة.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal DefaultPurchasePrice { get; set; }

    /// <summary>
    /// Default sale price for this unit.
    /// سعر البيع الافتراضي لهذه الوحدة.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal DefaultSalePrice { get; set; }

    /// <summary>
    /// Barcode specific to this packaging unit.
    /// باركود خاص بهذه الوحدة فقط.
    /// </summary>
    [StringLength(100)]
    public string? Barcode { get; set; }

    // ===================== Navigation Properties =====================
    
    public Medicine Medicine { get; set; } = null!;
}
