using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class StockTransferItemRepository : IStockTransferItemRepository
{
    private readonly ApplicationDbContext _context;

    public StockTransferItemRepository(ApplicationDbContext context) => _context = context;

    public async Task<StockTransferItem?> GetByIdAsync(int id)
    {
        return await _context.StockTransferItems
            .AsNoTracking()
            .Include(i => i.Medicine)
            .Include(i => i.StockTransfer)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
    }

    public async Task<IEnumerable<StockTransferItem>> GetByTransferIdAsync(int transferId)
    {
        return await _context.StockTransferItems
            .AsNoTracking()
            .Include(i => i.Medicine)
            .Where(i => i.StockTransferId == transferId && !i.IsDeleted)
            .ToListAsync();
    }

    public async Task<StockTransferItem> AddAsync(StockTransferItem item)
    {
        await _context.StockTransferItems.AddAsync(item);
        return item;
    }

    public async Task UpdateAsync(StockTransferItem item)
    {
        _context.StockTransferItems.Update(item);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _context.StockTransferItems.FindAsync(id);
        if (item != null)
        {
            item.IsDeleted = true;
            _context.StockTransferItems.Update(item);
        }
    }
}
