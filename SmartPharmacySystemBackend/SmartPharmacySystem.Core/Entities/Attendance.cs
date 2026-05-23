using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

public class Attendance : BaseEntity
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public int WorkingBranchId { get; set; }

    [Required]
    public DateTime CheckIn { get; set; }

    public DateTime? CheckOut { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? WorkedHours { get; set; }

    [Required]
    public ShiftType Shift { get; set; } = ShiftType.Morning;

    [Required]
    public AttendanceStatus AttendanceStatus { get; set; } = AttendanceStatus.Present;

    // Navigation properties
    public virtual Employee Employee { get; set; } = null!;
    
    [ForeignKey("WorkingBranchId")]
    public virtual Branch WorkingBranch { get; set; } = null!;
}
