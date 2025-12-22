namespace SmartPharmacySystem.Application.DTOs.Expense;

/// <summary>
/// DTO for Expense entity.
/// </summary>
public class ExpenseDto
{
    public int Id { get; set; }
    public string ExpenseType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public int CreatedBy { get; set; }
    public string Notes { get; set; } = string.Empty;
}
