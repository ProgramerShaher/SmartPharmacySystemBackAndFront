using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

public class StockCountHeader : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string CountCode { get; set; } = string.Empty;

    [Required]
    public int WarehouseId { get; set; }

    [Required]
    public StockCountType CountType { get; set; } = StockCountType.Monthly;

    [Required]
    public DateTime SnapshotAt { get; set; }

    [Required]
    public DateTime StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    [Required]
    public StockCountStatus Status { get; set; } = StockCountStatus.Draft;

    public int? ApprovedByUserId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Warehouse Warehouse { get; set; } = null!;

    [ForeignKey("ApprovedByUserId")]
    public virtual User? ApprovedByUser { get; set; }

    public virtual ICollection<StockCountItem> Items { get; set; } = new List<StockCountItem>();
}
