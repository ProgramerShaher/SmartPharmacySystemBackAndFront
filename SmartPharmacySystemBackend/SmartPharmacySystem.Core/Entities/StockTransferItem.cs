using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacySystem.Core.Entities;

public class StockTransferItem : BaseEntity
{
    [Required]
    public int StockTransferId { get; set; }

    [Required]
    public int MedicineId { get; set; }

    [Required]
    [MaxLength(100)]
    public string BatchNumber { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiryDate { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,4)")]
    public decimal QuantityRequested { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,4)")]
    public decimal QuantityDispatched { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? QuantityReceived { get; set; }

    // Navigation properties
    public virtual StockTransfer StockTransfer { get; set; } = null!;
    public virtual Medicine Medicine { get; set; } = null!;
}
