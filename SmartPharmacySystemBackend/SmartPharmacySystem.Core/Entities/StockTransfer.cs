using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

public class StockTransfer : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string TransferCode { get; set; } = string.Empty;

    [Required]
    public int SourceWarehouseId { get; set; }

    [Required]
    public int DestinationWarehouseId { get; set; }

    [Required]
    public TransferStatus Status { get; set; } = TransferStatus.Requested;

    [Required]
    public TransferType TransferType { get; set; } = TransferType.Manual;

    [Required]
    public int RequestedByUserId { get; set; }

    public int? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public int? DispatchedByUserId { get; set; }
    public DateTime? DispatchedAt { get; set; }

    public int? ReceivedByUserId { get; set; }
    public DateTime? ReceivedAt { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation properties
    [ForeignKey("SourceWarehouseId")]
    public virtual Warehouse SourceWarehouse { get; set; } = null!;

    [ForeignKey("DestinationWarehouseId")]
    public virtual Warehouse DestinationWarehouse { get; set; } = null!;

    [ForeignKey("RequestedByUserId")]
    public virtual User RequestedByUser { get; set; } = null!;

    [ForeignKey("ApprovedByUserId")]
    public virtual User? ApprovedByUser { get; set; }

    [ForeignKey("DispatchedByUserId")]
    public virtual User? DispatchedByUser { get; set; }

    [ForeignKey("ReceivedByUserId")]
    public virtual User? ReceivedByUser { get; set; }

    public virtual ICollection<StockTransferItem> Items { get; set; } = new List<StockTransferItem>();
}
