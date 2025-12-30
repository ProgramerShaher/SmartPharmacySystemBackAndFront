using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class ExpenseCategoryRepository : IExpenseCategoryRepository
{
    private readonly ApplicationDbContext _context;

    public ExpenseCategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExpenseCategory?> GetByIdAsync(int id)
    {
        return await _context.ExpenseCategories.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
    }

    public async Task<IEnumerable<ExpenseCategory>> GetAllAsync()
    {
        return await _context.ExpenseCategories.Where(x => !x.IsDeleted).ToListAsync();
    }

    public async Task AddAsync(ExpenseCategory entity)
    {
        await _context.ExpenseCategories.AddAsync(entity);
    }

    public async Task UpdateAsync(ExpenseCategory entity)
    {
        _context.ExpenseCategories.Update(entity);
        await Task.CompletedTask;
    }

    public async Task SoftDeleteAsync(int id)
    {
        var entity = await _context.ExpenseCategories.FindAsync(id);
        if (entity != null)
        {
            entity.IsDeleted = true;
            _context.ExpenseCategories.Update(entity);
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.ExpenseCategories.AnyAsync(x => x.Id == id && !x.IsDeleted);
    }

    public async Task<(IEnumerable<ExpenseCategory> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize, string sortBy, string sortDir)
    {
        var query = _context.ExpenseCategories.Where(x => !x.IsDeleted).AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(x => x.Name.Contains(search) || (x.Description != null && x.Description.Contains(search)));
        }

        int totalCount = await query.CountAsync();

        if (sortDir.ToLower() == "desc")
        {
            query = sortBy.ToLower() switch
            {
                "name" => query.OrderByDescending(x => x.Name),
                "id" => query.OrderByDescending(x => x.Id),
                _ => query.OrderByDescending(x => x.CreatedAt)
            };
        }
        else
        {
            query = sortBy.ToLower() switch
            {
                "name" => query.OrderBy(x => x.Name),
                "id" => query.OrderBy(x => x.Id),
                _ => query.OrderBy(x => x.CreatedAt)
            };
        }

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return (items, totalCount);
    }
}
