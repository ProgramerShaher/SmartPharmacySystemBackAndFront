using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IStockTransferRepository
{
    Task<StockTransfer?> GetByIdAsync(int id);
    Task<StockTransfer?> GetByCodeAsync(string transferCode);
    Task<IEnumerable<StockTransfer>> GetAllAsync(int? sourceWarehouseId = null, int? destinationWarehouseId = null, TransferStatus? status = null, DateTime? dateFrom = null, DateTime? dateTo = null, TransferType? transferType = null);
    Task<IEnumerable<StockTransfer>> GetByStatusAsync(TransferStatus status);
    Task<IEnumerable<StockTransfer>> GetPendingForApprovalAsync();
    Task<IEnumerable<StockTransfer>> GetPendingForDispatchAsync();
    Task<IEnumerable<StockTransfer>> GetPendingForReceiptAsync(int destinationWarehouseId);
    Task<StockTransfer> AddAsync(StockTransfer transfer);
    Task UpdateAsync(StockTransfer transfer);
    Task DeleteAsync(int id);
    Task<bool> CodeExistsAsync(string transferCode, int? excludeId = null);
    Task<int> GetCountByStatusAsync(TransferStatus status);
    Task<decimal> GetTotalTransferredValueAsync(DateTime dateFrom, DateTime dateTo);
}
