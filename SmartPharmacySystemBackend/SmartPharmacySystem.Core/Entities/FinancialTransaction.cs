using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents a financial transaction (Income or Expense) affecting the pharmacy balance.
/// </summary>
public class FinancialTransaction
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Type of the transaction (Income or Expense).
    /// </summary>
    [Required]
    public FinancialTransactionType Type { get; set; }

    /// <summary>
    /// Amount of the transaction.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Description explaining the reason for the transaction.
    /// </summary>
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the transaction occurred.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Optional reference to a FinancialInvoice Id.
    /// </summary>
    public int? RelatedInvoiceId { get; set; }
}
