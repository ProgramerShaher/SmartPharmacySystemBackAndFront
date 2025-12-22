using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

/// <summary>
/// Defines the contract for category repository operations.
/// This interface outlines the data access methods for managing categories.
/// </summary>
public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int id);
    Task<IEnumerable<Category>> GetAllAsync();
    Task AddAsync(Category entity);
    Task UpdateAsync(Category entity);
    Task SoftDeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<(IEnumerable<Category> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize, string sortBy, string sortDir);
}
