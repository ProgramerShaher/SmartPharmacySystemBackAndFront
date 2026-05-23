using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

public class DamagedGoodsRecord : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string DamageCode { get; set; } = string.Empty;

    [Required]
    public int SourceWarehouseId { get; set; }

    [Required]
    public int MedicineId { get; set; }

    [Required]
    [MaxLength(100)]
    public string BatchNumber { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiryDate { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public DamageType DamageType { get; set; } = DamageType.PhysicalDamage;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal DamageValue { get; set; }

    [Required]
    public DisposalMethod DisposalMethod { get; set; } = DisposalMethod.Destroyed;

    [Required]
    public RecordStatus Status { get; set; } = RecordStatus.PendingApproval;

    public int? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }

    [Required]
    public int RecordedByUserId { get; set; }

    // Navigation properties
    [ForeignKey("SourceWarehouseId")]
    public virtual Warehouse SourceWarehouse { get; set; } = null!;

    public virtual Medicine Medicine { get; set; } = null!;

    [ForeignKey("ApprovedByUserId")]
    public virtual User? ApprovedByUser { get; set; }

    [ForeignKey("RecordedByUserId")]
    public virtual User RecordedByUser { get; set; } = null!;
}
