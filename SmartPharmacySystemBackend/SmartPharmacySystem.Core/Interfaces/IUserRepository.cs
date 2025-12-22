using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

/// <summary>
/// Defines the contract for user repository operations.
/// This interface outlines the data access methods for managing users.
/// </summary>
public interface IUserRepository
{
    Task<User> GetByIdAsync(int id);
    Task<IEnumerable<User>> GetAllAsync();
    Task AddAsync(User entity);
    Task UpdateAsync(User entity);
    Task DeleteAsync(int id);
    Task SoftDeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<User> GetByUsernameAsync(string username);
    Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize, string sortBy, string sortDir, string? role, bool? isDeleted);
}