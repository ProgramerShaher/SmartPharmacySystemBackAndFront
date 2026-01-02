using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;
using SmartPharmacySystem.Infrastructure.Extensions;

namespace SmartPharmacySystem.Infrastructure.Repositories;

/// <summary>
/// Implements the medicine repository for data access operations.
/// Optimized for performance with AsNoTracking and efficient search patterns.
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
        return await _context.Medicines
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Medicine>> GetAllAsync()
    {
        return await _context.Medicines
            .AsNoTracking()
            .Include(m => m.Category)
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.Name)
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
        return await _context.Medicines
            .AsNoTracking()
            .AnyAsync(m => m.Id == id && !m.IsDeleted);
    }

    public async Task<IEnumerable<Medicine>> GetAlternativesAsync(string activeIngredient, int excludeMedicineId)
    {
        if (string.IsNullOrWhiteSpace(activeIngredient))
            return new List<Medicine>();

        return await _context.Medicines
            .AsNoTracking()
            .Include(m => m.MedicineBatches.Where(b => !b.IsDeleted && b.RemainingQuantity > 0))
            .Where(m => m.ActiveIngredient == activeIngredient
                     && m.Id != excludeMedicineId
                     && !m.IsDeleted
                     && m.Status == "Active")
            .OrderByDescending(m => m.MedicineBatches.Sum(b => b.RemainingQuantity))
            .Take(3)
            .ToListAsync();
    }

    /// <summary>
    /// Optimized paged search with AsNoTracking and StartsWith for better performance
    /// محسّن: استخدام StartsWith بدلاً من Contains + AsNoTracking
    /// </summary>
    public async Task<(IEnumerable<Medicine> Items, int TotalCount)> GetPagedAsync(
        string search,
        int page,
        int pageSize,
        string sortBy,
        string sortDirection,
        int? categoryId,
        string manufacturer,
        string status)
    {
        var query = _context.Medicines
            .AsNoTracking()
            .Include(m => m.Category)
            .Where(m => !m.IsDeleted)
            .AsQueryable();

        // Optimized search: Use StartsWith for indexed search
        if (!string.IsNullOrWhiteSpace(search))
        {
            // Try StartsWith first (faster with index)
            query = query.Where(m =>
                m.Name.StartsWith(search) ||
                m.ScientificName.StartsWith(search) ||
                m.DefaultBarcode.StartsWith(search) ||
                // Fallback to Contains for other fields
                (m.Notes != null && m.Notes.Contains(search)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(m => m.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(manufacturer))
        {
            query = query.Where(m => m.Manufacturer.StartsWith(manufacturer));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(m => m.Status == status);
        }

        // Count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting with explicit OrderBy
        query = string.IsNullOrWhiteSpace(sortBy) || sortBy == "Name"
            ? (sortDirection?.ToLower() == "desc"
                ? query.OrderByDescending(m => m.Name)
                : query.OrderBy(m => m.Name))
            : query.ApplySorting(sortBy, sortDirection);

        // Apply pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<MedicineBatch>> GetBatchesByFEFOAsync(int medicineId)
    {
        return await _context.MedicineBatches
            .AsNoTracking()
            .Where(b => b.MedicineId == medicineId
                     && b.RemainingQuantity > 0
                     && b.ExpiryDate > DateTime.UtcNow
                     && !b.IsDeleted
                     && b.Status == "Active")
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Medicine>> GetReorderReadyMedicinesAsync()
    {
        return await _context.Medicines
            .AsNoTracking()
            .Include(m => m.MedicineBatches.Where(b => !b.IsDeleted && b.Status == "Active"))
            .Where(m => !m.IsDeleted
                     && m.Status == "Active"
                     && m.MedicineBatches.Sum(b => b.RemainingQuantity) <= m.ReorderLevel)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }
}