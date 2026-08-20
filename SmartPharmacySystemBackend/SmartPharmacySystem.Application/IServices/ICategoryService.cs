using SmartPharmacySystem.Application.DTOs.Categories;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Application.DTOs.Medicine;
using Microsoft.AspNetCore.Http;

public interface ICategoryService
{
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
    Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteBulkAsync(IEnumerable<int> ids);
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<IEnumerable<CategoryDto>> GetAllAsync();
    Task<PagedResult<CategoryDto>> SearchAsync(CategoryQueryDto query);
    Task<byte[]> GenerateExcelTemplateAsync();
    Task<ImportResultDto> ImportFromExcelAsync(IFormFile file);
}
