using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// A specific medicine override within a Pricelist.
/// تجاوز سعر خاص لدواء معين ضمن قائمة أسعار.
/// If a medicine has a PricelistItem, it overrides the global discount.
/// </summary>
public class PricelistItem : BaseEntity
{
    /// <summary>
    /// Foreign key to the parent Pricelist.
    /// </summary>
    [Required]
    public int PricelistId { get; set; }

    /// <summary>
    /// Foreign key to the Medicine.
    /// </summary>
    [Required]
    public int MedicineId { get; set; }

    /// <summary>
    /// Fixed sale price for this medicine under this pricelist.
    /// If set, overrides the batch retail price completely.
    /// السعر الثابت لهذا الدواء ضمن القائمة (يلغي السعر الافتراضي).
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? FixedPrice { get; set; }

    /// <summary>
    /// Discount percentage for this specific medicine (0-100).
    /// نسبة الخصم الخاصة بهذا الدواء (تلغي الخصم العام للقائمة).
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    [Range(0, 100)]
    public decimal? DiscountPercentage { get; set; }

    // Navigation Properties

    /// <summary>
    /// The parent pricelist.
    /// </summary>
    [ForeignKey(nameof(PricelistId))]
    public virtual Pricelist Pricelist { get; set; } = null!;

    /// <summary>
    /// The medicine this override applies to.
    /// </summary>
    [ForeignKey(nameof(MedicineId))]
    public virtual Medicine Medicine { get; set; } = null!;
}
