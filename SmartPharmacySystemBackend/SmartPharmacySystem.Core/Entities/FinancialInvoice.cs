using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents a simplified internal invoice record for financial tracking.
/// </summary>
public class FinancialInvoice
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Type of the invoice (Purchase or Sale).
    /// </summary>
    public FinancialInvoiceType Type { get; set; }

    /// <summary>
    /// Total amount of the invoice.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Date when the invoice was issued.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Reference to the original operational invoice ID (Sales or Purchase).
    /// </summary>
    public int? OriginalInvoiceId { get; set; }
}
