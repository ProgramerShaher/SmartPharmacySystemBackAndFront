using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;
using SmartPharmacySystem.Infrastructure.Extensions;

namespace SmartPharmacySystem.Infrastructure.Repositories;

/// <summary>
/// Implements the medicine repository for data access operations.
/// This class provides concrete implementations of medicine data operations.
/// </summary>
public class MedicineRepository : IMedicineRepository
{
    private readonly ApplicationDbContext _context;

    public MedicineRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Medicine> GetByIdAsync(int id)
    {
        return await _context.Medicines.FindAsync(id);
    }

    public async Task<IEnumerable<Medicine>> GetAllAsync()
    {
        return await _context.Medicines
            .Include(m => m.Category)
            .ToListAsync();
    }

    public async Task AddAsync(Medicine entity)
    {
        await _context.Medicines.AddAsync(entity);
    }

    public Task UpdateAsync(Medicine entity)
    {
        _context.Medicines.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Medicines.FindAsync(id);
        if (entity != null)
        {
            _context.Medicines.Remove(entity);
        }
    }

    public async Task SoftDeleteAsync(int id)
    {
        var entity = await _context.Medicines.FindAsync(id);
        if (entity != null)
        {
            entity.IsDeleted = true;
            _context.Medicines.Update(entity);
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Medicines.AnyAsync(m => m.Id == id);
    }

    public async Task<(IEnumerable<Medicine> Items, int TotalCount)> GetPagedAsync(string search, int page, int pageSize, string sortBy, string sortDirection, int? categoryId, string manufacturer, string status)
    {
        var query = _context.Medicines
            .Include(m => m.Category)
            .Where(m => !m.IsDeleted) // Typically exclude soft deleted items in search
            .AsQueryable();

        // Filtering
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(m => m.Name.Contains(search) || (m.Notes != null && m.Notes.Contains(search)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(m => m.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(manufacturer))
        {
            query = query.Where(m => m.Manufacturer.Contains(manufacturer));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(m => m.Status.Contains(status));
        }

        // Sorting
        query = query.ApplySorting(sortBy, sortDirection);

        // Count
        var totalCount = await query.CountAsync();

        // Paging
        query = query.ApplyPaging(page, pageSize);

        return (await query.ToListAsync(), totalCount);
    }
}