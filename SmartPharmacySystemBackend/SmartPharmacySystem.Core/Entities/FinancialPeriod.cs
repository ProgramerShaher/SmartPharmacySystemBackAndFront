using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents a financial period (e.g., month/year) for accounting closures.
/// When a period is closed, no financial transactions can be backdated into this period.
/// </summary>
public class FinancialPeriod : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string PeriodName { get; set; } = string.Empty; // e.g., "2026-01"

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public bool IsClosed { get; set; } = false;

    public DateTime? ClosedAt { get; set; }
    
    public int? ClosedByUserId { get; set; }

    [ForeignKey("ClosedByUserId")]
    public virtual User? ClosedByUser { get; set; }
}
