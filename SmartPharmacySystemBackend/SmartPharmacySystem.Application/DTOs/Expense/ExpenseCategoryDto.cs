using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.Expense;

public class ExpenseCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateExpenseCategoryDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Description { get; set; }
}

public class UpdateExpenseCategoryDto : CreateExpenseCategoryDto
{
    public int Id { get; set; }
}
