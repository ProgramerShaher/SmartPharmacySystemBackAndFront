using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents a category for expenses (e.g., Salaries, Rent, Utilities).
/// </summary>
public class ExpenseCategory
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Navigation property for expenses in this category.
    /// </summary>
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
