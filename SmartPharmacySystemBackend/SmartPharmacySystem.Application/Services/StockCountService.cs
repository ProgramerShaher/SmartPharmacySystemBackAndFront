using AutoMapper;
using SmartPharmacySystem.Application.DTOs.StockCounts;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

public class StockCountService : IStockCountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public StockCountService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<StockCountHeaderDto> GetHeaderByIdAsync(int id)
    {
        var header = await _unitOfWork.StockCounts.GetHeaderByIdAsync(id)
            ?? throw new KeyNotFoundException("جرد المخزون غير موجود");
        return _mapper.Map<StockCountHeaderDto>(header);
    }

    public async Task<StockCountHeaderDto> GetByCodeAsync(string countCode)
    {
        var header = await _unitOfWork.StockCounts.GetByCodeAsync(countCode)
            ?? throw new KeyNotFoundException("جرد المخزون غير موجود");
        return _mapper.Map<StockCountHeaderDto>(header);
    }

    public async Task<IEnumerable<StockCountHeaderDto>> GetAllHeadersAsync(int? warehouseId = null, StockCountStatus? status = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var headers = await _unitOfWork.StockCounts.GetAllHeadersAsync(warehouseId, status, dateFrom, dateTo);
        return _mapper.Map<IEnumerable<StockCountHeaderDto>>(headers);
    }

    public async Task<IEnumerable<StockCountHeaderDto>> GetPendingApprovalAsync()
    {
        var headers = await _unitOfWork.StockCounts.GetPendingApprovalAsync();
        return _mapper.Map<IEnumerable<StockCountHeaderDto>>(headers);
    }

    public async Task<StockCountHeaderDto> CreateHeaderAsync(CreateStockCountHeaderDto dto)
    {
        var header = _mapper.Map<StockCountHeader>(dto);
        header.Status = StockCountStatus.InProgress;
        
        if (string.IsNullOrWhiteSpace(header.CountCode))
        {
            // STC-yyyyMMdd-XXXX
            header.CountCode = $"STC-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        }

        if (header.StartedAt == default)
        {
            header.StartedAt = DateTime.UtcNow;
        }

        if (header.SnapshotAt == default)
        {
            header.SnapshotAt = DateTime.UtcNow;
        }

        await _unitOfWork.StockCounts.AddHeaderAsync(header);
        await _unitOfWork.SaveChangesAsync();

        // ═══════════════════════════════════════════════════════
        // Auto-populate items from current warehouse inventory
        // ═══════════════════════════════════════════════════════
        var inventoryStocks = await _unitOfWork.InventoryStocks.GetByWarehouseIdAsync(header.WarehouseId);

        foreach (var stock in inventoryStocks.Where(s => s.Quantity > 0))
        {
            var item = new StockCountItem
            {
                StockCountHeaderId = header.Id,
                MedicineId = stock.MedicineId,
                BatchNumber = stock.BatchNumber,
                SystemQuantity = stock.Quantity,
                ExpiryDate = stock.ExpiryDate,
                PurchasePrice = 0,
                // الكمية الفعلية = الكمية النظامية (جرد تلقائي 100%)
                // يمكن للمستخدم تعديلها لاحقاً إن وُجد فارق
                PhysicalQuantity = stock.Quantity
            };
            await _unitOfWork.StockCounts.AddItemAsync(item);
        }

        if (inventoryStocks.Any(s => s.Quantity > 0))
        {
            await _unitOfWork.SaveChangesAsync();
        }

        return _mapper.Map<StockCountHeaderDto>(header);
    }

    public async Task<StockCountItemDto> AddItemAsync(int headerId, CreateStockCountItemDto dto)
    {
        var header = await _unitOfWork.StockCounts.GetHeaderByIdAsync(headerId)
            ?? throw new KeyNotFoundException("جرد المخزون غير موجود");

        var item = new StockCountItem
        {
            StockCountHeaderId = headerId,
            MedicineId = dto.MedicineId,
            BatchNumber = dto.BatchNumber,
            SystemQuantity = dto.SystemQuantity,
            PhysicalQuantity = dto.CountedQuantity
        };

        await _unitOfWork.StockCounts.AddItemAsync(item);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<StockCountItemDto>(item);
    }

    public async Task UpdateItemAsync(int headerId, UpdateStockCountItemDto dto)
    {
        var item = await _unitOfWork.StockCounts.GetItemByIdAsync(headerId, dto.MedicineId, dto.BatchNumber)
            ?? throw new KeyNotFoundException("عنصر الجرد غير موجود");

        item.PhysicalQuantity = dto.PhysicalQuantity;
        item.VarianceReason = dto.VarianceReason;
        await _unitOfWork.StockCounts.UpdateItemAsync(item);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<StockCountHeaderDto> SubmitForApprovalAsync(int headerId)
    {
        var header = await _unitOfWork.StockCounts.GetHeaderByIdAsync(headerId)
            ?? throw new KeyNotFoundException("جرد المخزون غير موجود");

        if (header.Status != StockCountStatus.InProgress)
            throw new InvalidOperationException("يمكن فقط إرسال الجرد قيد التنفيذ");

        header.Status = StockCountStatus.PendingApproval;
        await _unitOfWork.StockCounts.UpdateHeaderAsync(header);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<StockCountHeaderDto>(header);
    }

    public async Task<StockCountHeaderDto> ApproveAsync(int headerId, int approvedByUserId)
    {
        var header = await _unitOfWork.StockCounts.GetHeaderByIdAsync(headerId)
            ?? throw new KeyNotFoundException("جرد المخزون غير موجود");

        if (header.Status != StockCountStatus.PendingApproval)
            throw new InvalidOperationException("يمكن فقط اعتماد الجرد المعلق");

        header.Status = StockCountStatus.Approved;
        header.ApprovedByUserId = approvedByUserId;
        header.FinishedAt = DateTime.UtcNow;
        await _unitOfWork.StockCounts.UpdateHeaderAsync(header);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<StockCountHeaderDto>(header);
    }

    public async Task DeleteHeaderAsync(int id)
    {
        var header = await _unitOfWork.StockCounts.GetHeaderByIdAsync(id)
            ?? throw new KeyNotFoundException("جرد المخزون غير موجود");
        await _unitOfWork.StockCounts.DeleteHeaderAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteItemAsync(int headerId, int medicineId, string batchNumber)
    {
        await _unitOfWork.StockCounts.DeleteItemAsync(headerId, medicineId, batchNumber);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<StockCountItemDto>> GetItemsByHeaderIdAsync(int headerId)
    {
        var items = await _unitOfWork.StockCounts.GetItemsByHeaderIdAsync(headerId);
        return _mapper.Map<IEnumerable<StockCountItemDto>>(items);
    }

    public async Task<bool> CodeExistsAsync(string countCode, int? excludeId = null)
    {
        return await _unitOfWork.StockCounts.CodeExistsAsync(countCode, excludeId);
    }
}
