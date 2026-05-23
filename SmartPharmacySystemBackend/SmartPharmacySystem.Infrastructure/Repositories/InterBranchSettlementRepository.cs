using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class InterBranchSettlementRepository : IInterBranchSettlementRepository
{
    private readonly ApplicationDbContext _context;

    public InterBranchSettlementRepository(ApplicationDbContext context) => _context = context;

    public async Task<InterBranchSettlement?> GetByIdAsync(int id)
    {
        return await _context.InterBranchSettlements
            .AsNoTracking()
            .Include(s => s.FromBranch)
            .Include(s => s.ToBranch)
            .Include(s => s.SettledByUser)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<IEnumerable<InterBranchSettlement>> GetAllAsync(int? fromBranchId = null, int? toBranchId = null, SettlementStatus? status = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var query = _context.InterBranchSettlements.AsNoTracking().Where(s => !s.IsDeleted).AsQueryable();

        if (fromBranchId.HasValue)
            query = query.Where(s => s.FromBranchId == fromBranchId.Value);

        if (toBranchId.HasValue)
            query = query.Where(s => s.ToBranchId == toBranchId.Value);

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        if (dateFrom.HasValue)
            query = query.Where(s => s.SettlementDate >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(s => s.SettlementDate <= dateTo.Value);

        return await query
            .Include(s => s.FromBranch)
            .Include(s => s.ToBranch)
            .OrderByDescending(s => s.SettlementDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<InterBranchSettlement>> GetPendingAsync()
    {
        return await _context.InterBranchSettlements
            .AsNoTracking()
            .Include(s => s.FromBranch)
            .Include(s => s.ToBranch)
            .Where(s => s.Status == SettlementStatus.Pending && !s.IsDeleted)
            .OrderBy(s => s.SettlementDate)
            .ToListAsync();
    }

    public async Task<InterBranchSettlement> AddAsync(InterBranchSettlement settlement)
    {
        await _context.InterBranchSettlements.AddAsync(settlement);
        return settlement;
    }

    public async Task UpdateAsync(InterBranchSettlement settlement)
    {
        _context.InterBranchSettlements.Update(settlement);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var settlement = await _context.InterBranchSettlements.FindAsync(id);
        if (settlement != null)
        {
            settlement.IsDeleted = true;
            _context.InterBranchSettlements.Update(settlement);
        }
    }

    public async Task<decimal> GetTotalPendingAmountAsync(int branchId)
    {
        return await _context.InterBranchSettlements
            .AsNoTracking()
            .Where(s => (s.FromBranchId == branchId || s.ToBranchId == branchId)
                && s.Status == SettlementStatus.Pending
                && !s.IsDeleted)
            .SumAsync(s => s.Amount);
    }
}
