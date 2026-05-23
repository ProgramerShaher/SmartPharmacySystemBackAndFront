using System;
using System.ComponentModel.DataAnnotations;

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
    public int QuantityRequested { get; set; }

    [Required]
    public int QuantityDispatched { get; set; }

    public int? QuantityReceived { get; set; }

    // Navigation properties
    public virtual StockTransfer StockTransfer { get; set; } = null!;
    public virtual Medicine Medicine { get; set; } = null!;
}
