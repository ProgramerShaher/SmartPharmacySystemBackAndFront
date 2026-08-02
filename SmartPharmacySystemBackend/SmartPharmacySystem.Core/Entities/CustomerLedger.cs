using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

public class CustomerLedger : BaseMultiBranchEntity
{
    [Required]
    public int CustomerId { get; set; }


    [Required]
    public DateTime TransactionDate { get; set; }

    [Required]
    public CustomerTransactionType TransactionType { get; set; } = CustomerTransactionType.Invoice;

    [Required]
    public int ReferenceId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Debit { get; set; } = 0;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Credit { get; set; } = 0;

    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
}
