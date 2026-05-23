using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

public class Warehouse : BaseEntity
{
    [Required]
    public int BranchId { get; set; }

    [Required]
    public WarehouseType Type { get; set; } = WarehouseType.Branch;

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    // Navigation properties
    public virtual Branch Branch { get; set; } = null!;
    public virtual ICollection<MedicineBatch> MedicineBatches { get; set; } = new List<MedicineBatch>();
}
