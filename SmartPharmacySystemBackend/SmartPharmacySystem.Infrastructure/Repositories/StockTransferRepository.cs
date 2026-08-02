using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class StockTransferRepository : IStockTransferRepository
{
    private readonly ApplicationDbContext _context;

    public StockTransferRepository(ApplicationDbContext context) => _context = context;

    public async Task<StockTransfer?> GetByIdAsync(int id)
    {
        return await _context.StockTransfers
            .AsNoTracking()
            .Include(t => t.SourceWarehouse).ThenInclude(w => w.Branch)
            .Include(t => t.DestinationWarehouse).ThenInclude(w => w.Branch)
            .Include(t => t.DestinationBranch)
            .Include(t => t.Items).ThenInclude(i => i.Medicine)
            .Include(t => t.RequestedByUser)
            .Include(t => t.ApprovedByUser)
            .Include(t => t.DispatchedByUser)
            .Include(t => t.ReceivedByUser)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }

    public async Task<StockTransfer?> GetByCodeAsync(string transferCode)
    {
        return await _context.StockTransfers
            .AsNoTracking()
            .Include(t => t.SourceWarehouse).ThenInclude(w => w.Branch)
            .Include(t => t.DestinationWarehouse).ThenInclude(w => w.Branch)
            .Include(t => t.DestinationBranch)
            .Include(t => t.Items).ThenInclude(i => i.Medicine)
            .FirstOrDefaultAsync(t => t.TransferCode == transferCode && !t.IsDeleted);
    }

    public async Task<IEnumerable<StockTransfer>> GetAllAsync(int? sourceWarehouseId = null, int? destinationWarehouseId = null, TransferStatus? status = null, DateTime? dateFrom = null, DateTime? dateTo = null, TransferType? transferType = null)
    {
        var query = _context.StockTransfers.AsNoTracking().Where(t => !t.IsDeleted).AsQueryable();

        if (sourceWarehouseId.HasValue)
            query = query.Where(t => t.SourceWarehouseId == sourceWarehouseId.Value);

        if (destinationWarehouseId.HasValue)
            query = query.Where(t => t.DestinationWarehouseId == destinationWarehouseId.Value);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (dateFrom.HasValue)
            query = query.Where(t => t.CreatedAt >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(t => t.CreatedAt <= dateTo.Value);

        if (transferType.HasValue)
            query = query.Where(t => t.TransferType == transferType.Value);

        return await query
            .Include(t => t.SourceWarehouse).ThenInclude(w => w.Branch)
            .Include(t => t.DestinationWarehouse).ThenInclude(w => w.Branch)
            .Include(t => t.DestinationBranch)
            .Include(t => t.Items)
            .Include(t => t.RequestedByUser)
            .OrderByDescending(t => t.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<StockTransfer>> GetByStatusAsync(TransferStatus status)
    {
        return await _context.StockTransfers
            .AsNoTracking()
            .Include(t => t.SourceWarehouse).ThenInclude(w => w.Branch)
            .Include(t => t.DestinationWarehouse).ThenInclude(w => w.Branch)
            .Include(t => t.DestinationBranch)
            .Include(t => t.Items)
            .Where(t => t.Status == status && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<StockTransfer>> GetPendingForApprovalAsync()
    {
        return await _context.StockTransfers
            .AsNoTracking()
            .Include(t => t.SourceWarehouse)
            .Include(t => t.DestinationWarehouse)
            .Include(t => t.DestinationBranch)
            .Where(t => (t.Status == TransferStatus.AutoRequested || t.Status == TransferStatus.Requested) && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<StockTransfer>> GetPendingForDispatchAsync()
    {
        return await _context.StockTransfers
            .AsNoTracking()
            .Include(t => t.SourceWarehouse).ThenInclude(w => w.Branch)
            .Include(t => t.Items)
            .Where(t => t.Status == TransferStatus.Approved && !t.IsDeleted)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<StockTransfer>> GetPendingForReceiptAsync(int destinationWarehouseId)
    {
        return await _context.StockTransfers
            .AsNoTracking()
            .Include(t => t.SourceWarehouse).ThenInclude(w => w.Branch)
            .Include(t => t.Items)
            .Where(t => t.Status == TransferStatus.Dispatched && t.DestinationWarehouseId == destinationWarehouseId && !t.IsDeleted)
            .OrderBy(t => t.DispatchedAt)
            .ToListAsync();
    }

    public async Task<StockTransfer> AddAsync(StockTransfer transfer)
    {
        await _context.StockTransfers.AddAsync(transfer);
        return transfer;
    }

    public async Task UpdateAsync(StockTransfer transfer)
    {
        _context.StockTransfers.Update(transfer);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        // NOTE: `FindAsync` can bypass global query filters; use a filtered query instead.
        var transfer = await _context.StockTransfers.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        if (transfer != null)
        {
            transfer.IsDeleted = true;
            _context.StockTransfers.Update(transfer);
        }
    }

    public async Task<bool> CodeExistsAsync(string transferCode, int? excludeId = null)
    {
        var query = _context.StockTransfers.AsNoTracking().Where(t => t.TransferCode == transferCode && !t.IsDeleted);
        if (excludeId.HasValue)
            query = query.Where(t => t.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<int> GetCountByStatusAsync(TransferStatus status)
    {
        return await _context.StockTransfers.AsNoTracking().CountAsync(t => t.Status == status && !t.IsDeleted);
    }

    public async Task<decimal> GetTotalTransferredValueAsync(DateTime dateFrom, DateTime dateTo)
    {
        var transfers = await _context.StockTransfers
            .AsNoTracking()
            .Include(t => t.Items)
            .Where(t => t.Status == TransferStatus.Received
                && t.CreatedAt >= dateFrom
                && t.CreatedAt <= dateTo
                && !t.IsDeleted)
            .ToListAsync();

        return 0m;
    }
}
