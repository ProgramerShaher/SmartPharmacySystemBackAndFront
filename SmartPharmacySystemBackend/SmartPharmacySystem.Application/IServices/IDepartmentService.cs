using SmartPharmacySystem.Application.DTOs.Departments;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Application.Interfaces;

public interface IDepartmentService
{
    Task<DepartmentDto> GetByIdAsync(int id);
    Task<DepartmentDto> GetByNameAsync(string name);
    Task<IEnumerable<DepartmentDto>> GetAllAsync(string? search = null);
    Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto);
    Task UpdateAsync(UpdateDepartmentDto dto);
    Task DeleteAsync(int id);
    Task<bool> NameExistsAsync(string name, int? excludeId = null);
}
