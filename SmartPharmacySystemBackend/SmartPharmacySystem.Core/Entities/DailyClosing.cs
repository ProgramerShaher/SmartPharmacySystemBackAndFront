using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

public class DailyClosing : BaseMultiBranchEntity
{
    [Required]
    public DateTime ClosingDate { get; set; }

    [Required]
    public ClosingStatus Status { get; set; } = ClosingStatus.Draft;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal OpeningCash { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCashSales { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCreditSales { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCardSales { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCollections { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCashReturns { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalExpenses { get; set; }

    [NotMapped]
    public decimal ExpectedCash => OpeningCash + TotalCashSales + TotalCollections - TotalCashReturns - TotalExpenses;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal ActualCash { get; set; }

    [NotMapped]
    public decimal CashVariance => ActualCash - ExpectedCash;

    [Required]
    public int SubmittedByUserId { get; set; }

    public int? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }

    [ForeignKey("SubmittedByUserId")]
    public virtual User SubmittedByUser { get; set; } = null!;

    [ForeignKey("ApprovedByUserId")]
    public virtual User? ApprovedByUser { get; set; }
}
