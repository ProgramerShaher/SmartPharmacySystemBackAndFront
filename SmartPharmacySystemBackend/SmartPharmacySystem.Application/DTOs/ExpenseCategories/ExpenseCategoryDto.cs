namespace SmartPharmacySystem.Application.DTOs.ExpenseCategories;

public class ExpenseCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? AccountId { get; set; }
    public string? AccountName { get; set; }
    public int ExpenseCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateExpenseCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? AccountId { get; set; }
}

public class UpdateExpenseCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? AccountId { get; set; }
}
