using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacySystem.Core.Entities;

public class EmployeeLoan : BaseEntity
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MonthlyInstalment { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal RemainingAmount { get; set; }

    [Required]
    public int StartMonth { get; set; }

    [Required]
    public int StartYear { get; set; }

    [Required]
    public bool IsFullyPaid { get; set; } = false;

    // Navigation properties
    public virtual Employee Employee { get; set; } = null!;
}
