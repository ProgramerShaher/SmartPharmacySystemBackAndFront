using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacySystem.Core.Entities;

public class MedicineWarehouseConfig : BaseEntity
{
    [Required]
    public int WarehouseId { get; set; }

    [Required]
    public int MedicineId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,4)")]
    public decimal ReorderLevel { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,4)")]
    public decimal ReorderQuantity { get; set; }

    // Navigation properties
    public virtual Warehouse Warehouse { get; set; } = null!;
    public virtual Medicine Medicine { get; set; } = null!;
}
