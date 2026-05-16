using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class JournalEntryRepository : IJournalEntryRepository
{
    private readonly ApplicationDbContext _context;

    public JournalEntryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<JournalEntry?> GetByIdAsync(int id)
    {
        return await _context.JournalEntries
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(j => j.Id == id && !j.IsDeleted);
    }

    public async Task<JournalEntry?> GetByVoucherNumberAsync(string voucherNumber)
    {
        return await _context.JournalEntries
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.VoucherNumber == voucherNumber && !j.IsDeleted);
    }

    public async Task AddAsync(JournalEntry entity)
    {
        await _context.JournalEntries.AddAsync(entity);
    }

    public Task UpdateAsync(JournalEntry entity)
    {
        _context.JournalEntries.Update(entity);
        return Task.CompletedTask;
    }

    public async Task SoftDeleteAsync(int id)
    {
        var entry = await _context.JournalEntries.FindAsync(id);
        if (entry != null)
        {
            entry.IsDeleted = true;
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.JournalEntries.AnyAsync(j => j.Id == id && !j.IsDeleted);
    }

    public async Task<(IEnumerable<JournalEntry> Items, int TotalCount)> GetPagedAsync(
        string? search, DateTime? fromDate, DateTime? toDate, VoucherType? type, int page, int pageSize)
    {
        var query = _context.JournalEntries
            .Include(j => j.Lines)
            .Where(j => !j.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(j => j.Description.Contains(search) || j.VoucherNumber.Contains(search));
        }

        if (fromDate.HasValue)
        {
            query = query.Where(j => j.EntryDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(j => j.EntryDate <= toDate.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(j => j.Type == type.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(j => j.EntryDate)
            .ThenByDescending(j => j.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task PostEntryAsync(int entryId)
    {
        var entry = await GetByIdAsync(entryId);
        if (entry == null || entry.IsPosted) return;

        // In a real accounting system, posting would involve updating balances
        // We'll update the IsPosted flag here, the actual balance update logic 
        // can be triggered from a service to ensure consistency.
        entry.IsPosted = true;
        entry.PostedAt = DateTime.UtcNow;
    }

    public async Task<IEnumerable<JournalEntryLine>> GetLinesByAccountIdAsync(int accountId, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.JournalEntryLines
            .Include(l => l.JournalEntry)
            .Where(l => l.AccountId == accountId && !l.JournalEntry.IsDeleted);

        if (startDate.HasValue)
        {
            query = query.Where(l => l.JournalEntry.EntryDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(l => l.JournalEntry.EntryDate <= endDate.Value);
        }

        return await query
            .OrderBy(l => l.JournalEntry.EntryDate)
            .ThenBy(l => l.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<JournalEntryLine>> GetAllLinesAsync(DateTime? upToDate)
    {
        var query = _context.JournalEntryLines
            .Include(l => l.JournalEntry)
            .Include(l => l.Account)
            .Where(l => !l.JournalEntry.IsDeleted);

        if (upToDate.HasValue)
        {
            query = query.Where(l => l.JournalEntry.EntryDate <= upToDate.Value);
        }

        return await query.ToListAsync();
    }
}
