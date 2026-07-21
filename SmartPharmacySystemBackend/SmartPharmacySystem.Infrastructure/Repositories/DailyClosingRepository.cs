using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class DailyClosingRepository : IDailyClosingRepository
{
    private readonly ApplicationDbContext _context;

    public DailyClosingRepository(ApplicationDbContext context) => _context = context;

    public async Task<DailyClosing?> GetByIdAsync(int id)
    {
        return await _context.DailyClosings
            .AsNoTracking()
            .Include(c => c.Branch)
            .Include(c => c.SubmittedByUser)
            .Include(c => c.ApprovedByUser)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<DailyClosing?> GetByBranchDateAsync(int branchId, DateTime date)
    {
        var targetDate = date.Date;
        return await _context.DailyClosings
            .AsNoTracking()
            .Include(c => c.Branch)
            .Include(c => c.SubmittedByUser)
            .FirstOrDefaultAsync(c => c.BranchId == branchId
                && c.ClosingDate.Date == targetDate
                && !c.IsDeleted);
    }

    public async Task<IEnumerable<DailyClosing>> GetByBranchIdAsync(int branchId, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var query = _context.DailyClosings.AsNoTracking()
            .Include(c => c.Branch)
            .Where(c => c.BranchId == branchId && !c.IsDeleted);

        if (dateFrom.HasValue)
            query = query.Where(c => c.ClosingDate >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(c => c.ClosingDate <= dateTo.Value);

        return await query.OrderByDescending(c => c.ClosingDate).ToListAsync();
    }

    public async Task<IEnumerable<DailyClosing>> GetByDateAsync(DateTime date)
    {
        var targetDate = date.Date;
        return await _context.DailyClosings
            .AsNoTracking()
            .Include(c => c.Branch)
            .Include(c => c.SubmittedByUser)
            .Where(c => c.ClosingDate.Date == targetDate && !c.IsDeleted)
            .OrderBy(c => c.BranchId)
            .ToListAsync();
    }

    public async Task<IEnumerable<DailyClosing>> GetPendingApprovalAsync(DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var query = _context.DailyClosings.AsNoTracking()
            .Include(c => c.Branch)
            .Where(c => c.Status == ClosingStatus.PendingApproval && !c.IsDeleted);

        if (dateFrom.HasValue)
            query = query.Where(c => c.ClosingDate >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(c => c.ClosingDate <= dateTo.Value);

        return await query.OrderByDescending(c => c.ClosingDate).ToListAsync();
    }

    public async Task<DailyClosing> AddAsync(DailyClosing closing)
    {
        await _context.DailyClosings.AddAsync(closing);
        return closing;
    }

    public async Task UpdateAsync(DailyClosing closing)
    {
        _context.DailyClosings.Update(closing);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        // NOTE: `FindAsync` can bypass global query filters; use a filtered query instead.
        var closing = await _context.DailyClosings.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (closing != null)
        {
            closing.IsDeleted = true;
            _context.DailyClosings.Update(closing);
        }
    }

    public async Task<bool> ExistsAsync(int branchId, DateTime date)
    {
        var targetDate = date.Date;
        return await _context.DailyClosings
            .AsNoTracking()
            .AnyAsync(c => c.BranchId == branchId && c.ClosingDate.Date == targetDate && !c.IsDeleted);
    }

    public async Task<IEnumerable<DailyClosing>> GetApprovedAsync(DateTime dateFrom, DateTime dateTo, int? branchId = null)
    {
        var query = _context.DailyClosings.AsNoTracking()
            .Include(c => c.Branch)
            .Where(c => c.Status == ClosingStatus.Approved
                && c.ClosingDate >= dateFrom
                && c.ClosingDate <= dateTo
                && !c.IsDeleted);

        if (branchId.HasValue)
            query = query.Where(c => c.BranchId == branchId.Value);

        return await query.OrderByDescending(c => c.ClosingDate).ToListAsync();
    }
}
