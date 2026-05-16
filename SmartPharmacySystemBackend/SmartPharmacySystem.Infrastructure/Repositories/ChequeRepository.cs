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

public class ChequeRepository : IChequeRepository
{
    private readonly ApplicationDbContext _context;

    public ChequeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Cheque?> GetByIdAsync(int id)
    {
        return await _context.Cheques
            .Include(c => c.BankAccount)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<Cheque?> GetByNumberAsync(string chequeNumber)
    {
        return await _context.Cheques
            .FirstOrDefaultAsync(c => c.ChequeNumber == chequeNumber && !c.IsDeleted);
    }

    public async Task AddAsync(Cheque entity)
    {
        await _context.Cheques.AddAsync(entity);
    }

    public Task UpdateAsync(Cheque entity)
    {
        _context.Cheques.Update(entity);
        return Task.CompletedTask;
    }

    public async Task SoftDeleteAsync(int id)
    {
        var cheque = await _context.Cheques.FindAsync(id);
        if (cheque != null)
        {
            cheque.IsDeleted = true;
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Cheques.AnyAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<(IEnumerable<Cheque> Items, int TotalCount)> GetPagedAsync(
        string? search, ChequeStatus? status, ChequeType? type, DateTime? fromDueDate, DateTime? toDueDate, int page, int pageSize)
    {
        var query = _context.Cheques
            .Include(c => c.BankAccount)
            .Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.ChequeNumber.Contains(search) || 
                                     c.BankName.Contains(search) || 
                                     (c.PersonName != null && c.PersonName.Contains(search)));
        }

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(c => c.Type == type.Value);
        }

        if (fromDueDate.HasValue)
        {
            query = query.Where(c => c.DueDate >= fromDueDate.Value);
        }

        if (toDueDate.HasValue)
        {
            query = query.Where(c => c.DueDate <= toDueDate.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.DueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task UpdateStatusAsync(int chequeId, ChequeStatus status)
    {
        var cheque = await _context.Cheques.FindAsync(chequeId);
        if (cheque != null)
        {
            cheque.Status = status;
            cheque.UpdatedAt = DateTime.UtcNow;
        }
    }
}
