using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class DamagedGoodsRepository : IDamagedGoodsRepository
{
    private readonly ApplicationDbContext _context;

    public DamagedGoodsRepository(ApplicationDbContext context) => _context = context;

    public async Task<DamagedGoodsRecord?> GetByIdAsync(int id)
    {
        return await _context.DamagedGoodsRecords
            .AsNoTracking()
            .Include(r => r.SourceWarehouse).ThenInclude(w => w.Branch)
            .Include(r => r.Medicine)
            .Include(r => r.ApprovedByUser)
            .Include(r => r.RecordedByUser)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
    }

    public async Task<DamagedGoodsRecord?> GetByCodeAsync(string damageCode)
    {
        return await _context.DamagedGoodsRecords
            .AsNoTracking()
            .Include(r => r.SourceWarehouse)
            .Include(r => r.Medicine)
            .FirstOrDefaultAsync(r => r.DamageCode == damageCode && !r.IsDeleted);
    }

    public async Task<IEnumerable<DamagedGoodsRecord>> GetAllAsync(int? warehouseId = null, int? medicineId = null, DamageType? damageType = null, RecordStatus? status = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var query = _context.DamagedGoodsRecords.AsNoTracking().Where(r => !r.IsDeleted).AsQueryable();

        if (warehouseId.HasValue)
            query = query.Where(r => r.SourceWarehouseId == warehouseId.Value);

        if (medicineId.HasValue)
            query = query.Where(r => r.MedicineId == medicineId.Value);

        if (damageType.HasValue)
            query = query.Where(r => r.DamageType == damageType.Value);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (dateFrom.HasValue)
            query = query.Where(r => r.CreatedAt >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(r => r.CreatedAt <= dateTo.Value);

        return await query
            .Include(r => r.SourceWarehouse)
            .Include(r => r.Medicine)
            .Include(r => r.RecordedByUser)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<DamagedGoodsRecord>> GetPendingApprovalAsync()
    {
        return await _context.DamagedGoodsRecords
            .AsNoTracking()
            .Include(r => r.SourceWarehouse)
            .Include(r => r.Medicine)
            .Where(r => r.Status == RecordStatus.PendingApproval && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<DamagedGoodsRecord>> GetApprovedAsync(DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var query = _context.DamagedGoodsRecords.AsNoTracking()
            .Include(r => r.SourceWarehouse)
            .Include(r => r.Medicine)
            .Where(r => r.Status == RecordStatus.Approved && !r.IsDeleted);

        if (dateFrom.HasValue)
            query = query.Where(r => r.ApprovedAt >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(r => r.ApprovedAt <= dateTo.Value);

        return await query.OrderByDescending(r => r.ApprovedAt).ToListAsync();
    }

    public async Task<DamagedGoodsRecord> AddAsync(DamagedGoodsRecord record)
    {
        await _context.DamagedGoodsRecords.AddAsync(record);
        return record;
    }

    public async Task UpdateAsync(DamagedGoodsRecord record)
    {
        _context.DamagedGoodsRecords.Update(record);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        // NOTE: `FindAsync` can bypass global query filters; use a filtered query instead.
        var record = await _context.DamagedGoodsRecords.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        if (record != null)
        {
            record.IsDeleted = true;
            _context.DamagedGoodsRecords.Update(record);
        }
    }

    public async Task<bool> CodeExistsAsync(string damageCode, int? excludeId = null)
    {
        var query = _context.DamagedGoodsRecords.AsNoTracking().Where(r => r.DamageCode == damageCode && !r.IsDeleted);
        if (excludeId.HasValue)
            query = query.Where(r => r.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<decimal> GetTotalDamageValueAsync(DateTime dateFrom, DateTime dateTo)
    {
        return await _context.DamagedGoodsRecords
            .AsNoTracking()
            .Where(r => r.Status == RecordStatus.Approved
                && r.ApprovedAt >= dateFrom
                && r.ApprovedAt <= dateTo
                && !r.IsDeleted)
            .SumAsync(r => r.DamageValue);
    }

    public async Task<int> GetCountByStatusAsync(RecordStatus status)
    {
        return await _context.DamagedGoodsRecords.AsNoTracking().CountAsync(r => r.Status == status && !r.IsDeleted);
    }
}
