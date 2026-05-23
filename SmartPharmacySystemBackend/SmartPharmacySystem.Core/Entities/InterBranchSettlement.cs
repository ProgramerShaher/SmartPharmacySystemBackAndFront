using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

public class InterBranchSettlement : BaseEntity
{
    [Required]
    public int FromBranchId { get; set; } // Branch that collected money

    [Required]
    public int ToBranchId { get; set; } // Original creditor branch

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime SettlementDate { get; set; }

    [Required]
    public SettlementStatus Status { get; set; } = SettlementStatus.Pending;

    public int? SettledByUserId { get; set; }

    // Navigation properties
    [ForeignKey("FromBranchId")]
    public virtual Branch FromBranch { get; set; } = null!;

    [ForeignKey("ToBranchId")]
    public virtual Branch ToBranch { get; set; } = null!;

    [ForeignKey("SettledByUserId")]
    public virtual User? SettledByUser { get; set; }
}
