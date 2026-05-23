using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

public class SalaryDeductionItem : BaseEntity
{
    [Required]
    public int MonthlySalaryId { get; set; }

    [Required]
    public DeductionType DeductionType { get; set; } = DeductionType.Other;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(250)]
    public string Description { get; set; } = string.Empty;

    // Navigation properties
    public virtual MonthlySalary MonthlySalary { get; set; } = null!;
}
