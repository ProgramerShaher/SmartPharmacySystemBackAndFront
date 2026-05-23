using AutoMapper;
using SmartPharmacySystem.Application.DTOs.ExpenseCategories;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

public class ExpenseCategoryService : IExpenseCategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ExpenseCategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ExpenseCategoryDto> GetByIdAsync(int id)
    {
        var category = await _unitOfWork.ExpenseCategories.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("تصنيف المصروف غير موجود");
        return _mapper.Map<ExpenseCategoryDto>(category);
    }

    public async Task<IEnumerable<ExpenseCategoryDto>> GetAllAsync()
    {
        var categories = await _unitOfWork.ExpenseCategories.GetAllAsync();
        return _mapper.Map<IEnumerable<ExpenseCategoryDto>>(categories);
    }

    public async Task<ExpenseCategoryDto> CreateAsync(CreateExpenseCategoryDto dto)
    {
        var category = _mapper.Map<ExpenseCategory>(dto);
        await _unitOfWork.ExpenseCategories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<ExpenseCategoryDto>(category);
    }

    public async Task UpdateAsync(UpdateExpenseCategoryDto dto)
    {
        var category = await _unitOfWork.ExpenseCategories.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException("تصنيف المصروف غير موجود");

        _mapper.Map(dto, category);
        await _unitOfWork.ExpenseCategories.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _unitOfWork.ExpenseCategories.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("تصنيف المصروف غير موجود");
        await _unitOfWork.ExpenseCategories.SoftDeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PagedResponse<ExpenseCategoryDto>> GetPagedAsync(string? search, int page, int pageSize, string sortBy = "Name", string sortDir = "asc")
    {
        var (items, totalCount) = await _unitOfWork.ExpenseCategories.GetPagedAsync(search, page, pageSize, sortBy, sortDir);
        var dtos = _mapper.Map<IEnumerable<ExpenseCategoryDto>>(items);
        return new PagedResponse<ExpenseCategoryDto>(dtos, totalCount, page, pageSize);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _unitOfWork.ExpenseCategories.ExistsAsync(id);
    }
}
