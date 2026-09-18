using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacySystem.Core.Entities;

public class StockCountItem : BaseEntity
{
    [Required]
    public int StockCountHeaderId { get; set; }

    [Required]
    public int MedicineId { get; set; }

    [Required]
    [MaxLength(100)]
    public string BatchNumber { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiryDate { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,4)")]
    public decimal SystemQuantity { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? PhysicalQuantity { get; set; }

    [NotMapped]
    public decimal Variance => (PhysicalQuantity ?? 0m) - SystemQuantity;

    [NotMapped]
    public decimal VarianceValue => Variance * PurchasePrice;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PurchasePrice { get; set; }

    [MaxLength(250)]
    public string? VarianceReason { get; set; }

    // Navigation properties
    public virtual StockCountHeader StockCountHeader { get; set; } = null!;
    public virtual Medicine Medicine { get; set; } = null!;
}
