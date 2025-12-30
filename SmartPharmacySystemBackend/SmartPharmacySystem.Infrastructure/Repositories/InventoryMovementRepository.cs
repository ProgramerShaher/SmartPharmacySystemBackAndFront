using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

/// <summary>
/// Implements the inventory movement repository for data access operations.
/// This class provides concrete implementations of inventory movement data operations.
/// </summary>
public class InventoryMovementRepository : IInventoryMovementRepository
{
    private readonly ApplicationDbContext _context;

    public InventoryMovementRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryMovement> GetByIdAsync(int id)
    {
        return await _context.InventoryMovements
            .Include(m => m.Medicine)
            .Include(m => m.Batch)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<InventoryMovement>> GetAllAsync()
    {
        return await _context.InventoryMovements
            .Include(m => m.Medicine)
            .Include(m => m.Batch)
            .ToListAsync();
    }

    public async Task AddAsync(InventoryMovement entity)
    {
        await _context.InventoryMovements.AddAsync(entity);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.InventoryMovements.AnyAsync(x => x.Id == id);
    }

    public async Task<(IEnumerable<InventoryMovement> Items, int TotalCount)> GetPagedAsync(string search, int page, int pageSize, string sortBy, string sortDirection)
    {
        var query = _context.InventoryMovements
            .Include(m => m.Medicine)
            .Include(m => m.Batch)
            .AsQueryable();

        // ... ãäØÞ ÇáÈÍË (Search) íÙá ßãÇ åæ ...

        var totalCount = await query.CountAsync();

        var results = await (from inv in query
                             join fin in _context.FinancialTransactions
                             on new { RefId = inv.ReferenceId, RefType = (int)inv.ReferenceType }
                             equals new { RefId = fin.ReferenceId, RefType = (int)fin.ReferenceType } into finJoin
                             from subFin in finJoin.DefaultIfEmpty()
                             select new
                             {
                                 Movement = inv,
                                 FinDesc = subFin != null ? subFin.Description : null
                             })
                    .OrderByDescending(x => x.Movement.Date)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

        // äÞæã ÈäÞá ÇáæÕÝ ÇáãÇáí Åáì ÍÞá ãÄÞÊ Ýí ÇáÜ Entity (Ãæ ÏãÌ ãÄÞÊ Ýí ÇáÜ Notes)
        foreach (var item in results)
        {
            // äÓÊÎÏã ÍÞá ÇáÜ Notes áäÞá ÇáÈíÇäÇÊ ááÜ Mapper ãÚ ÝÇÕá ããíÒ
            if (!string.IsNullOrEmpty(item.FinDesc))
                item.Movement.Notes = $"{item.Movement.Notes}[FIN_DESC]{item.FinDesc}";
        }

        return (results.Select(x => x.Movement), totalCount);
    }
    public async Task<int> GetCurrentBalanceAsync(int medicineId, int? batchId = null)
    {
        var query = _context.InventoryMovements
            .Where(m => m.MedicineId == medicineId);

        if (batchId.HasValue)
        {
            query = query.Where(m => m.BatchId == batchId.Value);
        }

        return await query.SumAsync(m => m.Quantity);
    }

    public async Task<IEnumerable<InventoryMovement>> GetStockCardMovementsAsync(int medicineId, int? batchId = null)
    {
        var query = _context.InventoryMovements
            .Where(m => m.MedicineId == medicineId);

        if (batchId.HasValue)
        {
            query = query.Where(m => m.BatchId == batchId.Value);
        }

        return await query
            .OrderBy(m => m.Date)
            .ThenBy(m => m.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryMovement>> GetMovementsByReferenceAsync(int referenceId, SmartPharmacySystem.Core.Enums.ReferenceType type)
    {
        return await _context.InventoryMovements
            .Where(m => m.ReferenceId == referenceId && m.ReferenceType == type)
            .ToListAsync();
    }
}
