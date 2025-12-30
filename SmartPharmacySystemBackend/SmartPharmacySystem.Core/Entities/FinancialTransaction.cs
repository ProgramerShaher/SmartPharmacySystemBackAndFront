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
    /// معرف الحساب المرتبط بهذه الحركة
    /// Account ID associated with this transaction
    /// </summary>
    [Required]
    public int AccountId { get; set; }

    /// <summary>
    /// نوع الحركة (دخل أو مصروف)
    /// Type of the transaction (Income or Expense).
    /// </summary>
    [Required]
    public FinancialTransactionType Type { get; set; }

    /// <summary>
    /// Amount of the transaction.
    /// </summary>
    [Required]
    public decimal Amount { get; set; }

    /// <summary>
    /// نوع المرجع الذي أنشأ هذه الحركة
    /// Reference type that created this transaction
    /// </summary>
    [Required]
    public ReferenceType ReferenceType { get; set; }

    /// <summary>
    /// معرف المرجع (مثل: معرف الفاتورة)
    /// Reference ID (e.g., Invoice ID)
    /// </summary>
    [Required]
    public int ReferenceId { get; set; }

    /// <summary>
    /// Description explaining the reason for the transaction.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// تاريخ الحركة المالية (قد يختلف عن تاريخ الإنشاء)
    /// Transaction date (may differ from creation date)
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Date and time when the transaction was created in the system.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Navigation property to the pharmacy account.
    /// </summary>
    public PharmacyAccount Account { get; set; } = null!;
}
