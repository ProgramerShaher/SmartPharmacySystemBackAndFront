using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

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
    /// معرف فئة المصروف
    /// </summary>
    [Required]
    public int CategoryId { get; set; }

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
    public PaymentType PaymentMethod { get; set; } = PaymentType.Cash;

    /// <summary>
    /// Additional notes about the expense.
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// معرف الحساب المرتبط بهذا المصروف
    /// </summary>
    public int? AccountId { get; set; }
}
