using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.Categories;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CategoryService> _logger;
    private readonly IMapper _mapper;

    public CategoryService(IUnitOfWork unitOfWork, ILogger<CategoryService> logger, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        var category = _mapper.Map<Category>(dto);
        category.CreatedAt = DateTime.UtcNow;
        category.IsDeleted = false;

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"التصنيف برقم {id} غير موجود");

        _mapper.Map(dto, category);

        await _unitOfWork.Categories.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var exists = await _unitOfWork.Categories.ExistsAsync(id);
        if (!exists)
            throw new KeyNotFoundException($"التصنيف برقم {id} غير موجود");

        await _unitOfWork.Categories.SoftDeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null) return null;
        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }

    public async Task<PagedResult<CategoryDto>> SearchAsync(CategoryQueryDto query)
    {
        var (items, totalCount) = await _unitOfWork.Categories.GetPagedAsync(
            query.Search,
            query.Page,
            query.PageSize,
            query.SortBy,
            query.SortDirection);

        var dtos = _mapper.Map<IEnumerable<CategoryDto>>(items);
        return new PagedResult<CategoryDto>(dtos, totalCount, query.Page, query.PageSize);
    }
}
