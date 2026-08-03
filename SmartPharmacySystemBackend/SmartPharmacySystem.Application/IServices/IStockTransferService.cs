using SmartPharmacySystem.Application.DTOs.StockTransfers;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Interfaces;

public interface IStockTransferService
{
    Task<StockTransferDto> GetByIdAsync(int id);
    Task<StockTransferDto> GetByCodeAsync(string transferCode);
    Task<IEnumerable<StockTransferDto>> GetAllAsync(int? sourceWarehouseId = null, int? destinationWarehouseId = null, TransferStatus? status = null, DateTime? dateFrom = null, DateTime? dateTo = null, TransferType? transferType = null);
    Task<IEnumerable<StockTransferDto>> GetByBranchAsync(int branchId, TransferType? transferType = null);
    Task<IEnumerable<StockTransferDto>> GetByStatusAsync(TransferStatus status);
    Task<IEnumerable<StockTransferDto>> GetPendingForApprovalAsync();
    Task<IEnumerable<StockTransferDto>> GetPendingForDispatchAsync();
    Task<IEnumerable<StockTransferDto>> GetPendingForReceiptAsync(int destinationWarehouseId);
    Task<StockTransferDto> CreateAsync(CreateStockTransferDto dto, int requestedByUserId);
    Task<StockTransferDto> ApproveAsync(int id, int approvedByUserId);
    Task<StockTransferDto> DispatchAsync(int id, int dispatchedByUserId);
    Task<StockTransferDto> ReceiveAsync(int id, ReceiveStockTransferDto dto, int receivedByUserId);
    Task DeleteAsync(int id);
    Task<int> GetCountByStatusAsync(TransferStatus status);
    Task<decimal> GetTotalTransferredValueAsync(DateTime dateFrom, DateTime dateTo);
    Task<bool> CodeExistsAsync(string transferCode, int? excludeId = null);
}
