using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class StockCountRepository : IStockCountRepository
{
    private readonly ApplicationDbContext _context;

    public StockCountRepository(ApplicationDbContext context) => _context = context;

    public async Task<StockCountHeader?> GetHeaderByIdAsync(int id)
    {
        return await _context.StockCountHeaders
            .AsNoTracking()
            .Include(h => h.Warehouse).ThenInclude(w => w.Branch)
            .Include(h => h.Items).ThenInclude(i => i.Medicine)
            .Include(h => h.ApprovedByUser)
            .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);
    }

    public async Task<StockCountHeader?> GetByCodeAsync(string countCode)
    {
        return await _context.StockCountHeaders
            .AsNoTracking()
            .Include(h => h.Warehouse)
            .Include(h => h.Items)
            .FirstOrDefaultAsync(h => h.CountCode == countCode && !h.IsDeleted);
    }

    public async Task<IEnumerable<StockCountHeader>> GetAllHeadersAsync(int? warehouseId = null, StockCountStatus? status = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var query = _context.StockCountHeaders.AsNoTracking().Where(h => !h.IsDeleted).AsQueryable();

        if (warehouseId.HasValue)
            query = query.Where(h => h.WarehouseId == warehouseId.Value);

        if (status.HasValue)
            query = query.Where(h => h.Status == status.Value);

        if (dateFrom.HasValue)
            query = query.Where(h => h.StartedAt >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(h => h.StartedAt <= dateTo.Value);

        return await query
            .Include(h => h.Warehouse)
            .Include(h => h.Items)
            .OrderByDescending(h => h.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<StockCountHeader>> GetPendingApprovalAsync()
    {
        return await _context.StockCountHeaders
            .AsNoTracking()
            .Include(h => h.Warehouse)
            .Include(h => h.Items)
            .Where(h => h.Status == StockCountStatus.PendingApproval && !h.IsDeleted)
            .OrderByDescending(h => h.StartedAt)
            .ToListAsync();
    }

    public async Task<StockCountHeader> AddHeaderAsync(StockCountHeader header)
    {
        await _context.StockCountHeaders.AddAsync(header);
        return header;
    }

    public async Task UpdateHeaderAsync(StockCountHeader header)
    {
        // استخدام Entry().State بدلاً من Update() لتجنب تعارض الـ Tracking مع Navigation Properties
        _context.Entry(header).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await Task.CompletedTask;
    }
        
    public async Task DeleteHeaderAsync(int id)
    {
        // NOTE: `FindAsync` can bypass global query filters; use a filtered query instead.
        var header = await _context.StockCountHeaders.FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);
        if (header != null)
        {
            header.IsDeleted = true;
            _context.StockCountHeaders.Update(header);
        }
    }

    public async Task<bool> CodeExistsAsync(string countCode, int? excludeId = null)
    {
        var query = _context.StockCountHeaders.AsNoTracking().Where(h => h.CountCode == countCode && !h.IsDeleted);
        if (excludeId.HasValue)
            query = query.Where(h => h.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<StockCountItem?> GetItemByIdAsync(int headerId, int medicineId, string batchNumber)
    {
        return await _context.StockCountItems
            .AsNoTracking()
            .Include(i => i.Medicine)
            .FirstOrDefaultAsync(i => i.StockCountHeaderId == headerId
                && i.MedicineId == medicineId
                && i.BatchNumber == batchNumber
                && !i.IsDeleted);
    }

    public async Task<IEnumerable<StockCountItem>> GetItemsByHeaderIdAsync(int headerId)
    {
        return await _context.StockCountItems
            .AsNoTracking()
            .Include(i => i.Medicine)
            .Where(i => i.StockCountHeaderId == headerId && !i.IsDeleted)
            .OrderBy(i => i.Medicine.Name)
            .ToListAsync();
    }

    public async Task<StockCountItem> AddItemAsync(StockCountItem item)
    {
        await _context.StockCountItems.AddAsync(item);
        return item;
    }

    public async Task UpdateItemAsync(StockCountItem item)
    {
        _context.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await Task.CompletedTask;
    }

    public async Task DeleteItemAsync(int headerId, int medicineId, string batchNumber)
    {
        var item = await _context.StockCountItems
            .FirstOrDefaultAsync(i => i.StockCountHeaderId == headerId
                && i.MedicineId == medicineId
                && i.BatchNumber == batchNumber);
        if (item != null)
        {
            item.IsDeleted = true;
            _context.StockCountItems.Update(item);
        }
    }

    // ==========================================
    // Schedules
    // ==========================================

    public async Task<StockCountSchedule?> GetScheduleByIdAsync(int id)
    {
        return await _context.StockCountSchedules
            .AsNoTracking()
            .Include(s => s.Warehouse).ThenInclude(w => w.Branch)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<IEnumerable<StockCountSchedule>> GetAllSchedulesAsync(int? warehouseId = null)
    {
        var query = _context.StockCountSchedules.AsNoTracking().Where(s => !s.IsDeleted);
        if (warehouseId.HasValue)
            query = query.Where(s => s.WarehouseId == warehouseId.Value);

        return await query
            .Include(s => s.Warehouse).ThenInclude(w => w.Branch)
            .OrderBy(s => s.NextRunDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<StockCountSchedule>> GetDueSchedulesAsync()
    {
        var today = DateTime.Today;
        return await _context.StockCountSchedules
            .AsNoTracking()
            .Where(s => s.IsActive && !s.IsDeleted && s.NextRunDate.Date <= today)
            .ToListAsync();
    }

    public async Task<StockCountSchedule> AddScheduleAsync(StockCountSchedule schedule)
    {
        await _context.StockCountSchedules.AddAsync(schedule);
        return schedule;
    }

    public async Task UpdateScheduleAsync(StockCountSchedule schedule)
    {
        _context.Entry(schedule).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await Task.CompletedTask;
    }

    public async Task DeleteScheduleAsync(int id)
    {
        var schedule = await _context.StockCountSchedules.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        if (schedule != null)
        {
            schedule.IsDeleted = true;
            _context.StockCountSchedules.Update(schedule);
        }
    }
}
