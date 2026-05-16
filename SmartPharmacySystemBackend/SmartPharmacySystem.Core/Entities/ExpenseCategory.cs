using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents a category for expenses (e.g., Salaries, Rent, Utilities).
/// </summary>
public class ExpenseCategory : BaseEntity
{

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Description { get; set; }

    /// <summary>
    /// معرف الحساب المرتبط بهذا التصنيف في شجرة الحسابات
    /// </summary>
    public int? AccountId { get; set; }

    // Navigation
    [ForeignKey(nameof(AccountId))]
    public virtual Account? Account { get; set; }

    /// <summary>
    /// Navigation property for expenses in this category.
    /// </summary>
    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
