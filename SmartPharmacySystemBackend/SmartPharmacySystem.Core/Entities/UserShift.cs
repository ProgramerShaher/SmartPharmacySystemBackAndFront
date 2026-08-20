using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacySystem.Core.Entities;

public class UserShift : BaseEntity
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public int BranchId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal OpeningCash { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? ActualClosingCash { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ExpectedClosingCash { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Difference { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Open"; // Open, Closed
    public string? Notes { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Branch Branch { get; set; } = null!;
}
