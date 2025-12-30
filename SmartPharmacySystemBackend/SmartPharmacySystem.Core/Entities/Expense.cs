using SmartPharmacySystem.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents expenses in the pharmacy system.
/// Tracks all non-medicine related expenditures.
/// </summary>
public class Expense
{
    /// <summary>
    /// Unique identifier for the expense.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// معرف فئة المصروف
    /// Category ID for the expense
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// معرف الحساب المرتبط بهذا المصروف
    /// Account ID associated with this expense
    /// </summary>
    public int AccountId { get; set; } = 1;

    /// <summary>
    /// Amount of the expense.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Date of the expense.
    /// </summary>
    public DateTime ExpenseDate { get; set; }

    /// <summary>
    /// Payment method used.
    /// </summary>
    [Required]
    public PaymentType PaymentMethod { get; set; } = PaymentType.Cash;

    /// <summary>
    /// Additional notes about the expense.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// هل تم دفع المصروف (للمصروفات الآجلة)
    /// Has the expense been paid (for credit expenses)
    /// </summary>
    public bool IsPaid { get; set; } = false;

    /// <summary>
    /// Date and time when the expense was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// User ID who created the expense.
    /// </summary>
    public int CreatedBy { get; set; }

    /// <summary>
    /// Navigation property for the category.
    /// </summary>
    public ExpenseCategory Category { get; set; } = null!;

    /// <summary>
    /// Navigation property to the pharmacy account.
    /// </summary>
    public PharmacyAccount Account { get; set; } = null!;
}