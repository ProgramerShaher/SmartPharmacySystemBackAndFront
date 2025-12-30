using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

/// <summary>
/// Implements the expense repository for data access operations.
/// This class provides concrete implementations of expense data operations.
/// </summary>
public class ExpenseRepository : IExpenseRepository
{
    private readonly ApplicationDbContext _context;

    public ExpenseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Expense> GetByIdAsync(int id)
    {
        return await _context.Expenses
            .Include(e => e.Category)
            .Include(e => e.Account)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    public async Task<IEnumerable<Expense>> GetAllAsync()
    {
        return await _context.Expenses
            .Include(e => e.Category)
            .Include(e => e.Account)
            .Where(e => !e.IsDeleted)
            .ToListAsync();
    }

    public async Task AddAsync(Expense entity)
    {
        await _context.Expenses.AddAsync(entity);
    }

    public Task UpdateAsync(Expense entity)
    {
        _context.Expenses.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Expenses.FindAsync(id);
        if (entity != null)
        {
            _context.Expenses.Remove(entity);
        }
    }

    public async Task SoftDeleteAsync(int id)
    {
        var entity = await _context.Expenses.FindAsync(id);
        if (entity != null)
        {
            entity.IsDeleted = true;
            _context.Expenses.Update(entity);
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Expenses.AnyAsync(e => e.Id == id);
    }

    public async Task<(IEnumerable<Expense> Items, int TotalCount)> GetPagedAsync(
        string? search, int page, int pageSize, string sortBy, string sortDir,
        DateTime? fromDate, DateTime? toDate, int? categoryId)
    {
        var query = _context.Expenses
            .Include(e => e.Category)
            .Include(e => e.Account)
            .Where(e => !e.IsDeleted);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(e => (e.Category != null && e.Category.Name.ToLower().Contains(search)) ||
                                     (e.Notes != null && e.Notes.ToLower().Contains(search)));
        }

        // Apply date range filter
        if (fromDate.HasValue)
            query = query.Where(e => e.ExpenseDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(e => e.ExpenseDate <= toDate.Value);

        // Apply category filter
        if (categoryId.HasValue)
            query = query.Where(e => e.CategoryId == categoryId.Value);

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
            : query.OrderBy(e => EF.Property<object>(e, sortBy));

        // Apply pagination
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return (items, totalCount);
    }
}