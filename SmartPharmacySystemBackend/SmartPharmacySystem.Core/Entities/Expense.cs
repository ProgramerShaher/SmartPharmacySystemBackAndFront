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
    /// Type of expense.
    /// </summary>
    public string ExpenseType { get; set; }

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
    public string PaymentMethod { get; set; }

    /// <summary>
    /// Additional notes about the expense.
    /// </summary>
    public string? Notes { get; set; }

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
}