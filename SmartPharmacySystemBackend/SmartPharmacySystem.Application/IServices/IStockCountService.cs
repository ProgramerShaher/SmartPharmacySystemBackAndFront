using SmartPharmacySystem.Application.DTOs.StockCounts;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Interfaces;

public interface IStockCountService
{
    Task<StockCountHeaderDto> GetHeaderByIdAsync(int id);
    Task<StockCountHeaderDto> GetByCodeAsync(string countCode);
    Task<IEnumerable<StockCountHeaderDto>> GetAllHeadersAsync(int? warehouseId = null, StockCountStatus? status = null, DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<IEnumerable<StockCountHeaderDto>> GetPendingApprovalAsync();
    Task<StockCountHeaderDto> CreateHeaderAsync(CreateStockCountHeaderDto dto);
    Task<StockCountItemDto> AddItemAsync(int headerId, CreateStockCountItemDto dto);
    Task UpdateItemAsync(int headerId, UpdateStockCountItemDto dto);
    Task<StockCountHeaderDto> SubmitForApprovalAsync(int headerId);
    Task<StockCountHeaderDto> ApproveAsync(int headerId, int approvedByUserId);
    Task DeleteHeaderAsync(int id);
    Task DeleteItemAsync(int headerId, int medicineId, string batchNumber);
    Task<IEnumerable<StockCountItemDto>> GetItemsByHeaderIdAsync(int headerId);
    Task<bool> CodeExistsAsync(string countCode, int? excludeId = null);
}
