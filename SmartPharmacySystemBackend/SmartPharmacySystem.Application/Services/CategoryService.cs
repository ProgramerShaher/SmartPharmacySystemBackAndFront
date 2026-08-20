using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.Categories;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Application.DTOs.Medicine;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using ExcelDataReader;

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

        var hasMedicines = await _unitOfWork.Categories.HasMedicinesAsync(id);
        if (hasMedicines)
            throw new InvalidOperationException($"لا يمكن حذف التصنيف برقم {id} لوجود أدوية مرتبطة به");

        await _unitOfWork.Categories.SoftDeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteBulkAsync(IEnumerable<int> ids)
    {
        var errors = new List<string>();
        foreach (var id in ids)
        {
            try
            {
                await DeleteAsync(id);
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
            }
        }
        
        if (errors.Any())
        {
            throw new InvalidOperationException("بعض الأصناف لم يتم حذفها:\n" + string.Join("\n", errors));
        }

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

    public async Task<byte[]> GenerateExcelTemplateAsync()
    {
        var sb = new System.Text.StringBuilder();
        // Add UTF-8 BOM so Excel opens it with correct Arabic encoding
        sb.AppendLine("اسم الصنف (مطلوب),الوصف");
        sb.AppendLine("مسكنات,أدوية لتخفيف الألم");
        
        var preamble = System.Text.Encoding.UTF8.GetPreamble();
        var data = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        return preamble.Concat(data).ToArray();
    }

    public async Task<ImportResultDto> ImportFromExcelAsync(IFormFile file)
    {
        var result = new ImportResultDto();
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        using var stream = file.OpenReadStream();
        using var reader = ExcelReaderFactory.CreateReader(stream);
        var conf = new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
        };
        var dataSet = reader.AsDataSet(conf);
        var dataTable = dataSet.Tables[0];

        var allCategories = (await _unitOfWork.Categories.GetAllAsync()).ToList();

        int rowNumber = 1;
        foreach (System.Data.DataRow row in dataTable.Rows)
        {
            rowNumber++;
            result.TotalProcessed++;
            try
            {
                var name = row[0]?.ToString()?.Trim();
                if (string.IsNullOrEmpty(name))
                {
                    result.FailedCount++;
                    result.Errors.Add($"الصف {rowNumber}: اسم الصنف مطلوب.");
                    continue;
                }

                var description = row.ItemArray.Length > 1 ? row[1]?.ToString()?.Trim() : null;

                var existingCategory = allCategories.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (existingCategory != null)
                {
                    existingCategory.Description = description;
                    existingCategory.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.Categories.UpdateAsync(existingCategory);
                    result.UpdatedCount++;
                }
                else
                {
                    var newCategory = new Category
                    {
                        Name = name,
                        Description = description,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };
                    await _unitOfWork.Categories.AddAsync(newCategory);
                    allCategories.Add(newCategory); // Add to local list to prevent duplicates in same file
                    result.SuccessCount++;
                }
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                result.Errors.Add($"الصف {rowNumber}: {ex.Message}");
            }
        }
        return result;
    }
}
