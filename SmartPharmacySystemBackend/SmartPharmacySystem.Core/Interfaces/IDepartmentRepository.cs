using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(int id);
    Task<Department?> GetByNameAsync(string name);
    Task<IEnumerable<Department>> GetAllAsync(string? search = null);
    Task<Department> AddAsync(Department department);
    Task UpdateAsync(Department department);
    Task DeleteAsync(int id);
    Task<bool> NameExistsAsync(string name, int? excludeId = null);
}
