using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

/// <summary>
/// Defines the contract for expense repository operations.
/// This interface outlines the data access methods for managing expenses.
/// </summary>
public interface IExpenseRepository
{
    Task<Expense> GetByIdAsync(int id);
    Task<IEnumerable<Expense>> GetAllAsync();
    Task AddAsync(Expense entity);
    Task UpdateAsync(Expense entity);
    Task DeleteAsync(int id);
    Task SoftDeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<(IEnumerable<Expense> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize, string sortBy, string sortDir, DateTime? fromDate, DateTime? toDate, string? expenseType);
}