using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents a price list (e.g., "Friends List", "Near-Expiry Discounts").
/// قائمة أسعار - يمكن ربطها بعميل أو تطبيقها يدوياً عند البيع.
/// </summary>
public class Pricelist : BaseEntity
{
    /// <summary>
    /// Name of the price list (e.g. "قائمة الأصدقاء").
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description for the price list.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Global discount percentage applied to all items (0-100).
    /// خصم عام يُطبق على جميع الأصناف عند استخدام هذه القائمة.
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    [Range(0, 100)]
    public decimal GlobalDiscountPercentage { get; set; } = 0;

    /// <summary>
    /// Whether this price list is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation Properties

    /// <summary>
    /// Customers linked to this price list.
    /// </summary>
    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    /// <summary>
    /// Specific per-medicine overrides within this pricelist.
    /// تجاوزات سعر خاصة لكل دواء ضمن هذه القائمة.
    /// </summary>
    public virtual ICollection<PricelistItem> Items { get; set; } = new List<PricelistItem>();
}
