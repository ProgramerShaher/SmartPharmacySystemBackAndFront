using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class InventoryStockRepository : IInventoryStockRepository
{
    private readonly ApplicationDbContext _context;

    public InventoryStockRepository(ApplicationDbContext context) => _context = context;

    public async Task<InventoryStock?> GetByIdAsync(int id)
    {
        return await _context.InventoryStocks
            .AsNoTracking()
            .Include(s => s.Warehouse).ThenInclude(w => w.Branch)
            .Include(s => s.Medicine)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<IEnumerable<InventoryStock>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await _context.InventoryStocks
            .AsNoTracking()
            .Include(s => s.Medicine)
            .Where(s => s.WarehouseId == warehouseId && !s.IsDeleted && s.Quantity > 0)
            .OrderBy(s => s.ExpiryDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryStock>> GetByMedicineIdAsync(int medicineId)
    {
        return await _context.InventoryStocks
            .AsNoTracking()
            .Include(s => s.Warehouse).ThenInclude(w => w.Branch)
            .Where(s => s.MedicineId == medicineId && !s.IsDeleted && s.Quantity > 0)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryStock>> GetByWarehouseAndMedicineAsync(int warehouseId, int medicineId)
    {
        return await _context.InventoryStocks
            .AsNoTracking()
            .Where(s => s.WarehouseId == warehouseId && s.MedicineId == medicineId && !s.IsDeleted && s.Quantity > 0)
            .OrderBy(s => s.ExpiryDate)
            .ToListAsync();
    }

    public async Task<InventoryStock?> GetByWarehouseMedicineBatchAsync(int warehouseId, int medicineId, string batchNumber)
    {
        var normalizedBatchNumber = batchNumber.Trim();

        return await _context.InventoryStocks
            .AsNoTracking()
            // Removed .Include(s => s.Warehouse) and .Include(s => s.Medicine) 
            // because this method is primarily used to fetch stock for updates,
            // and including navigation properties with AsNoTracking causes tracking conflicts when calling .Update() later.
            .Where(s => s.WarehouseId == warehouseId
                && s.MedicineId == medicineId
                && s.BatchNumber.Trim() == normalizedBatchNumber
                && !s.IsDeleted
                && s.Quantity > 0)
            .OrderByDescending(s => s.Quantity)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<InventoryStock>> GetExpiringSoonAsync(int daysThreshold)
    {
        var now = DateTime.UtcNow.Date;
        var threshold = now.AddDays(daysThreshold);

        return await _context.InventoryStocks
            .AsNoTracking()
            .Include(s => s.Medicine)
            .Include(s => s.Warehouse).ThenInclude(w => w.Branch)
            .Where(s => !s.IsDeleted
                && s.Quantity > 0
                && s.ExpiryDate.Date > now
                && s.ExpiryDate.Date <= threshold)
            .OrderBy(s => s.ExpiryDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryStock>> GetExpiredAsync()
    {
        var now = DateTime.UtcNow.Date;

        return await _context.InventoryStocks
            .AsNoTracking()
            .Include(s => s.Medicine)
            .Include(s => s.Warehouse).ThenInclude(w => w.Branch)
            .Where(s => !s.IsDeleted && s.Quantity > 0 && s.ExpiryDate.Date <= now)
            .OrderBy(s => s.ExpiryDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryStock>> GetBelowReorderLevelAsync()
    {
        return await _context.InventoryStocks
            .AsNoTracking()
            .Include(s => s.Medicine)
            .Include(s => s.Warehouse)
            .Where(s => !s.IsDeleted && s.Quantity > 0)
            .ToListAsync();
    }

    public async Task<int> GetTotalQuantityAsync(int warehouseId, int medicineId)
    {
        return await _context.InventoryStocks
            .AsNoTracking()
            .Where(s => s.WarehouseId == warehouseId && s.MedicineId == medicineId && !s.IsDeleted)
            .SumAsync(s => s.Quantity);
    }

    public async Task<InventoryStock> AddAsync(InventoryStock stock)
    {
        await _context.InventoryStocks.AddAsync(stock);
        return stock;
    }

    public async Task UpdateAsync(InventoryStock stock)
    {
        _context.InventoryStocks.Update(stock);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        // NOTE: `FindAsync` can bypass global query filters; use a filtered query instead.
        var stock = await _context.InventoryStocks.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        if (stock != null)
        {
            stock.IsDeleted = true;
            _context.InventoryStocks.Update(stock);
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.InventoryStocks.AsNoTracking().AnyAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<IEnumerable<InventoryStock>> SearchAsync(string? search = null, int? warehouseId = null, int? medicineId = null)
    {
        var query = _context.InventoryStocks.AsNoTracking().Where(s => !s.IsDeleted).AsQueryable();

        if (warehouseId.HasValue)
            query = query.Where(s => s.WarehouseId == warehouseId.Value);

        if (medicineId.HasValue)
            query = query.Where(s => s.MedicineId == medicineId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.BatchNumber.Contains(search)
                || (s.Medicine != null && s.Medicine.Name.Contains(search)));

        return await query
            .Include(s => s.Medicine)
            .Include(s => s.Warehouse).ThenInclude(w => w.Branch)
            .OrderBy(s => s.ExpiryDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryStock>> GetStocksForBatchesAsync(IEnumerable<int> medicineIds, IEnumerable<string> batchNumbers)
    {
        var medIds = medicineIds.ToList();
        var batches = batchNumbers.ToList();
        
        return await _context.InventoryStocks
            .AsNoTracking()
            .Include(s => s.Warehouse).ThenInclude(w => w.Branch)
            .Where(s => !s.IsDeleted && s.Quantity > 0 && medIds.Contains(s.MedicineId) && batches.Contains(s.BatchNumber))
            .ToListAsync();
    }
}
