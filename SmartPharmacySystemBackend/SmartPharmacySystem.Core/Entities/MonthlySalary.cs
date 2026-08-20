using SmartPharmacySystem.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Metrics;

namespace SmartPharmacySystem.Core.Entities;

public class MonthlySalary : BaseEntity
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public int BranchId { get; set; }

    [Required]
    public int Month { get; set; }

    [Required]
    public int Year { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal BasicSalary { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAllowances { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalBonuses { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalDeductions { get; set; }

    [NotMapped]
    public decimal NetSalary => BasicSalary + TotalAllowances + TotalBonuses - TotalDeductions; 

    [Required]
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public DateTime? PaidAt { get; set; }

    public int? PaidFromAccountId { get; set; }

    public int? SalaryExpenseAccountId { get; set; }

    public int? JournalEntryId { get; set; }

    // Navigation properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual Branch Branch { get; set; } = null!;
    public virtual Account? PaidFromAccount { get; set; }
    public virtual Account? SalaryExpenseAccount { get; set; }
    public virtual JournalEntry? JournalEntry { get; set; }
    public virtual ICollection<SalaryDeductionItem> Deductions { get; set; } = new List<SalaryDeductionItem>();
}


//No backing field could be found for property 'MonthlySalary.NetSalary' and the property does not have a setter.
