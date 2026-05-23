using SmartPharmacySystem.Application.DTOs.ExpenseCategories;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Application.Interfaces;

public interface IExpenseCategoryService
{
    Task<ExpenseCategoryDto> GetByIdAsync(int id);
    Task<IEnumerable<ExpenseCategoryDto>> GetAllAsync();
    Task<ExpenseCategoryDto> CreateAsync(CreateExpenseCategoryDto dto);
    Task UpdateAsync(UpdateExpenseCategoryDto dto);
    Task DeleteAsync(int id);
    Task<PagedResponse<ExpenseCategoryDto>> GetPagedAsync(string? search, int page, int pageSize, string sortBy = "Name", string sortDir = "asc");
    Task<bool> ExistsAsync(int id);
}
