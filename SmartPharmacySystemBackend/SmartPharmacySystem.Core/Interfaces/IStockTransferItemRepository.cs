using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IStockTransferItemRepository
{
    Task<StockTransferItem?> GetByIdAsync(int id);
    Task<IEnumerable<StockTransferItem>> GetByTransferIdAsync(int transferId);
    Task<StockTransferItem> AddAsync(StockTransferItem item);
    Task UpdateAsync(StockTransferItem item);
    Task DeleteAsync(int id);
}
