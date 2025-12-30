using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Expense;

/// <summary>
/// DTO for Expense entity.
/// </summary>
public class ExpenseDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string ExpenseType => CategoryName; // Alias for backward compatibility
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public PaymentType PaymentMethod { get; set; } = PaymentType.Cash;
    public bool IsPaid { get; set; }
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Notes { get; set; } = string.Empty;
}
