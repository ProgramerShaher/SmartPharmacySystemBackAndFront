using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

/// <summary>
/// Defines the contract for medicine repository operations.
/// This interface outlines the data access methods for managing medicines.
/// </summary>
public interface IMedicineRepository
{
    Task<Medicine> GetByIdAsync(int id);
    Task<IEnumerable<Medicine>> GetAllAsync();
    Task AddAsync(Medicine entity);
    Task UpdateAsync(Medicine entity);
    Task DeleteAsync(int id);
    Task SoftDeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<(IEnumerable<Medicine> Items, int TotalCount)> GetPagedAsync(string search, int page, int pageSize, string sortBy, string sortDirection, int? categoryId, string manufacturer, string status);
}