using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacySystem.Core.Entities;

public enum StockCountFrequency
{
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Quarterly = 4,
    SemiAnnually = 5,
    Annually = 6
}

public class StockCountSchedule : BaseEntity
{
    [Required]
    public int WarehouseId { get; set; }

    [Required]
    public StockCountFrequency Frequency { get; set; } = StockCountFrequency.Monthly;

    [Required]
    public int BranchId { get; set; }

    [Required]
    public DateTime NextRunDate { get; set; }

    public DateTime? LastRunDate { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(200)]
    public string? Notes { get; set; }

    // Navigation property
    public virtual Warehouse Warehouse { get; set; } = null!;
    public virtual Branch Branch { get; set; } = null!;
}
