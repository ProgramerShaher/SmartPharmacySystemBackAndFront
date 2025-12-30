using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IExpenseCategoryRepository
{
    Task<ExpenseCategory?> GetByIdAsync(int id);
    Task<IEnumerable<ExpenseCategory>> GetAllAsync();
    Task AddAsync(ExpenseCategory entity);
    Task UpdateAsync(ExpenseCategory entity);
    Task SoftDeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<(IEnumerable<ExpenseCategory> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize, string sortBy, string sortDir);
}
