using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacySystem.Core.Entities;

public class Employee : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string NationalId { get; set; } = string.Empty;

    [Required]
    public int BranchId { get; set; }

    [Required]
    public int DepartmentId { get; set; }

    [Required]
    [MaxLength(100)]
    public string JobTitle { get; set; } = string.Empty;

    [Required]
    public DateTime HireDate { get; set; }

    public DateTime? TerminationDate { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal BasicSalary { get; set; }

    public bool IsActive { get; set; } = true;

    [Required]
    public SmartPharmacySystem.Core.Enums.ShiftType Shift { get; set; } = SmartPharmacySystem.Core.Enums.ShiftType.Morning;

    public TimeSpan? ShiftStartTime { get; set; }

    public TimeSpan? ShiftEndTime { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal WorkingHours { get; set; } = 8;

    // Navigation properties
    public virtual Branch Branch { get; set; } = null!;
    public virtual Department Department { get; set; } = null!;
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public virtual ICollection<MonthlySalary> Salaries { get; set; } = new List<MonthlySalary>();
    public virtual ICollection<EmployeeLoan> Loans { get; set; } = new List<EmployeeLoan>();
    
    /// <summary>
    /// رابط اختياري بحساب النظام (User Account)
    /// Optional link to a system user account for this employee.
    /// مثال: محمد = موظف وحسابه هو mohammed.ali
    /// </summary>
    public int? UserId { get; set; }
    public virtual User? User { get; set; }
}
