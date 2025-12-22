using System;
using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.Expense;

/// <summary>
/// DTO for updating an existing Expense.
/// </summary>
public class UpdateExpenseDto
{
    /// <summary>
    /// Unique identifier for the expense to update.
    /// </summary>
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// Type of expense.
    /// </summary>
    [Required]
    public string ExpenseType { get; set; } = string.Empty;

    /// <summary>
    /// Amount of the expense.
    /// </summary>
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Amount must be greater than or equal to 0")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Date of the expense.
    /// </summary>
    [Required]
    public DateTime ExpenseDate { get; set; }

    /// <summary>
    /// Payment method used.
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Additional notes about the expense.
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the expense was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
}
