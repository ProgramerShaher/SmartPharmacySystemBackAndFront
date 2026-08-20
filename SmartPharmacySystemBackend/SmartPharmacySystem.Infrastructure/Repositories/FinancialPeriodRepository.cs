using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class FinancialPeriodRepository : IFinancialPeriodRepository
{
    private readonly ApplicationDbContext _context;

    public FinancialPeriodRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FinancialPeriod> GetByIdAsync(int id)
    {
        return await _context.FinancialPeriods.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<FinancialPeriod>> GetAllAsync()
    {
        return await _context.FinancialPeriods.OrderByDescending(p => p.StartDate).ToListAsync();
    }

    public async Task<FinancialPeriod> GetByDateAsync(System.DateTime date)
    {
        return await _context.FinancialPeriods
            .FirstOrDefaultAsync(p => date >= p.StartDate && date <= p.EndDate);
    }

    public async Task AddAsync(FinancialPeriod entity)
    {
        await _context.FinancialPeriods.AddAsync(entity);
    }

    public Task UpdateAsync(FinancialPeriod entity)
    {
        _context.FinancialPeriods.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.FinancialPeriods.FindAsync(id);
        if (entity != null)
        {
            _context.FinancialPeriods.Remove(entity);
        }
    }
}
