using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacySystem.Core.Entities;

public class InventoryStock : BaseEntity
{
    [Required]
    public int WarehouseId { get; set; }

    [Required]
    public int MedicineId { get; set; }

    [Required]
    [MaxLength(100)]
    public string BatchNumber { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiryDate { get; set; }

    [Required]
    public int Quantity { get; set; }

    // Navigation properties
    [ForeignKey("WarehouseId")]
    public virtual Warehouse Warehouse { get; set; } = null!;

    public virtual Medicine Medicine { get; set; } = null!;
}
