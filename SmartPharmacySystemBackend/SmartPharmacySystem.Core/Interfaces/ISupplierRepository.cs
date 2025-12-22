using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

/// <summary>
/// Defines the contract for supplier repository operations.
/// This interface outlines the data access methods for managing suppliers.
/// </summary>
public interface ISupplierRepository
{
    Task<Supplier> GetByIdAsync(int id);
    Task<IEnumerable<Supplier>> GetAllAsync();
    Task AddAsync(Supplier entity);
    Task UpdateAsync(Supplier entity);
    Task DeleteAsync(int id);
    Task SoftDeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<(IEnumerable<Supplier> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize, string sortBy, string sortDir, bool? hasBalance);
}