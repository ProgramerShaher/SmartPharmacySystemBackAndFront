using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Core.Entities;

public class MedicineWarehouseConfig : BaseEntity
{
    [Required]
    public int WarehouseId { get; set; }

    [Required]
    public int MedicineId { get; set; }

    [Required]
    public int ReorderLevel { get; set; }

    [Required]
    public int ReorderQuantity { get; set; }

    // Navigation properties
    public virtual Warehouse Warehouse { get; set; } = null!;
    public virtual Medicine Medicine { get; set; } = null!;
}
