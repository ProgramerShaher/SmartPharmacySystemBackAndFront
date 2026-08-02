using AutoMapper;
using SmartPharmacySystem.Application.DTOs.StockTransfers;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace SmartPharmacySystem.Application.Services;

public class StockTransferService : IStockTransferService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly ILogger<StockTransferService> _logger;

    public StockTransferService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationService notificationService,
        ILogger<StockTransferService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<StockTransferDto> GetByIdAsync(int id)
    {
        var transfer = await _unitOfWork.StockTransfers.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("تحويل المخزون غير موجود");
        return MapToDto(transfer);
    }

    public async Task<StockTransferDto> GetByCodeAsync(string transferCode)
    {
        var transfer = await _unitOfWork.StockTransfers.GetByCodeAsync(transferCode)
            ?? throw new KeyNotFoundException("تحويل المخزون غير موجود");
        return MapToDto(transfer);
    }

    public async Task<IEnumerable<StockTransferDto>> GetAllAsync(int? sourceWarehouseId = null, int? destinationWarehouseId = null, TransferStatus? status = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var transfers = await _unitOfWork.StockTransfers.GetAllAsync(sourceWarehouseId, destinationWarehouseId, status, dateFrom, dateTo);
        return transfers.Select(MapToDto);
    }

    public async Task<IEnumerable<StockTransferDto>> GetByStatusAsync(TransferStatus status)
    {
        var transfers = await _unitOfWork.StockTransfers.GetByStatusAsync(status);
        return transfers.Select(MapToDto);
    }

    public async Task<IEnumerable<StockTransferDto>> GetPendingForApprovalAsync()
    {
        var transfers = await _unitOfWork.StockTransfers.GetPendingForApprovalAsync();
        return transfers.Select(MapToDto);
    }

    public async Task<IEnumerable<StockTransferDto>> GetPendingForDispatchAsync()
    {
        var transfers = await _unitOfWork.StockTransfers.GetPendingForDispatchAsync();
        return transfers.Select(MapToDto);
    }

    public async Task<IEnumerable<StockTransferDto>> GetPendingForReceiptAsync(int destinationWarehouseId)
    {
        var transfers = await _unitOfWork.StockTransfers.GetPendingForReceiptAsync(destinationWarehouseId);
        return transfers.Select(MapToDto);
    }

    public async Task<StockTransferDto> CreateAsync(CreateStockTransferDto dto, int requestedByUserId)
    {
        var transfer = _mapper.Map<StockTransfer>(dto);
        transfer.Status = TransferStatus.Requested;
        transfer.RequestedByUserId = requestedByUserId;
        transfer.TransferCode = $"TRF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        await _unitOfWork.StockTransfers.AddAsync(transfer);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(transfer);
    }

    public async Task<StockTransferDto> ApproveAsync(int id, int approvedByUserId)
    {
        var transfer = await _unitOfWork.StockTransfers.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("تحويل المخزون غير موجود");

        if (transfer.Status != TransferStatus.Requested)
            throw new InvalidOperationException("يمكن فقط اعتماد التحويلات المطلوبة");

        transfer.Status = TransferStatus.Approved;
        transfer.ApprovedByUserId = approvedByUserId;
        transfer.ApprovedAt = DateTime.UtcNow;

        // Clear navigation properties to prevent EF Core tracking conflicts
        transfer.SourceWarehouse = null;
        transfer.DestinationWarehouse = null;
        transfer.RequestedByUser = null;
        transfer.ApprovedByUser = null;
        transfer.DispatchedByUser = null;
        transfer.ReceivedByUser = null;

        if (transfer.Items != null)
        {
            foreach (var item in transfer.Items)
            {
                item.Medicine = null;
                item.StockTransfer = null;
            }
        }

        await _unitOfWork.StockTransfers.UpdateAsync(transfer);
        await _unitOfWork.SaveChangesAsync();
        return MapToDto(transfer);
    }

    public async Task<StockTransferDto> DispatchAsync(int id, int dispatchedByUserId)
    {
        var transfer = await _unitOfWork.StockTransfers.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("تحويل المخزون غير موجود");

        if (transfer.Status != TransferStatus.Approved)
            throw new InvalidOperationException("يمكن فقط صرف التحويلات المعتمدة");

        if (transfer.Items == null || !transfer.Items.Any())
            throw new InvalidOperationException("لا توجد بنود في سند التحويل");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var sourceStocks = new Dictionary<string, InventoryStock>();

            // ===== خصم الكميات من مخزن المرسِل =====
            foreach (var item in transfer.Items)
            {
                var key = $"{item.MedicineId}_{item.BatchNumber}";
                if (!sourceStocks.TryGetValue(key, out var stock))
                {
                    stock = await _unitOfWork.InventoryStocks
                        .GetByWarehouseMedicineBatchAsync(
                            transfer.SourceWarehouseId,
                            item.MedicineId,
                            item.BatchNumber);

                    if (stock == null)
                        throw new InvalidOperationException(
                            $"الدواء '{item.Medicine?.Name ?? item.MedicineId.ToString()}' — التشغيلة '{item.BatchNumber}' غير موجود في المخزن المرسِل");

                    sourceStocks[key] = stock;
                }

                if (stock.Quantity < item.QuantityRequested)
                    throw new InvalidOperationException(
                        $"الكمية غير كافية للدواء '{item.Medicine?.Name ?? item.MedicineId.ToString()}' — المطلوب: {item.QuantityRequested}، المتاح: {stock.Quantity}");

                // خصم الكمية
                stock.Quantity -= item.QuantityRequested;
                
                // تحديث QuantityDispatched في بند السند
                item.QuantityDispatched = item.QuantityRequested;
                
                // Clear navigation properties before UpdateAsync
                item.Medicine = null;
                item.StockTransfer = null;
                await _unitOfWork.StockTransferItems.UpdateAsync(item);

                // تسجيل حركة مخزنية (صادر — تحويل)
                var movement = new InventoryMovement(
                    item.MedicineId,
                    null,                             // BatchId في InventoryMovement يعني MedicineBatch.Id لا BatchNumber
                    StockMovementType.Damage,         // نستخدم Damage كـ placeholder — يمكن إضافة StockTransferOut لاحقاً
                    ReferenceType.Manual,
                    -item.QuantityRequested,          // سالب = صادر من المرسِل
                    transfer.Id,
                    transfer.TransferCode,
                    dispatchedByUserId,
                    $"صادر تحويل — سند {transfer.TransferCode} — من مخزن {transfer.SourceWarehouseId}");
                await _unitOfWork.InventoryMovements.AddAsync(movement);

                _logger.LogInformation(
                    "Dispatched item: Medicine={MedicineId}, Batch={Batch}, Qty={Qty}, SourceWarehouse={WH}",
                    item.MedicineId, item.BatchNumber, item.QuantityRequested, transfer.SourceWarehouseId);
            }

            // Update all modified source stocks once
            foreach (var stock in sourceStocks.Values)
            {
                stock.Medicine = null;
                stock.Warehouse = null;
                await _unitOfWork.InventoryStocks.UpdateAsync(stock);
            }

            // تحديث حالة السند
            transfer.Status = TransferStatus.Dispatched;
            transfer.DispatchedByUserId = dispatchedByUserId;
            transfer.DispatchedAt = DateTime.UtcNow;

            // Clear navigation properties to prevent EF Core tracking conflicts
            transfer.SourceWarehouse = null;
            transfer.DestinationWarehouse = null;
            transfer.RequestedByUser = null;
            transfer.ApprovedByUser = null;
            transfer.DispatchedByUser = null;
            transfer.ReceivedByUser = null;

            if (transfer.Items != null)
            {
                foreach (var item in transfer.Items)
                {
                    item.Medicine = null;
                    item.StockTransfer = null;
                }
            }

            await _unitOfWork.StockTransfers.UpdateAsync(transfer);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("StockTransfer {Code} dispatched successfully", transfer.TransferCode);
            return MapToDto(transfer);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error dispatching StockTransfer {Id}", id);
            throw;
        }
    }

    public async Task<StockTransferDto> ReceiveAsync(int id, ReceiveStockTransferDto dto, int receivedByUserId)
    {
        var transfer = await _unitOfWork.StockTransfers.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("تحويل المخزون غير موجود");

        if (transfer.Status != TransferStatus.Dispatched)
            throw new InvalidOperationException("يمكن فقط استلام التحويلات المصروفة");

        if (dto.Items == null || !dto.Items.Any())
            throw new InvalidOperationException("يجب إدخال الكميات المستلمة");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            bool hasVariance = false;
            var destinationStocks = new Dictionary<string, InventoryStock>();

            foreach (var itemDto in dto.Items)
            {
                // جلب بند السند المطابق من السند المحمل مسبقاً لمنع تعارض EF Core
                var item = transfer.Items?.FirstOrDefault(i => i.Id == itemDto.StockTransferItemId);
                if (item == null) continue;

                var quantityReceived = itemDto.QuantityReceived;

                // تسجيل الكمية المستلمة في السند
                item.QuantityReceived = quantityReceived;

                // ===== إضافة الكميات لمخزن الوجهة =====
                if (quantityReceived > 0)
                {
                    var key = $"{item.MedicineId}_{item.BatchNumber}";
                    if (!destinationStocks.TryGetValue(key, out var destinationStock))
                    {
                        destinationStock = await _unitOfWork.InventoryStocks
                            .GetByWarehouseMedicineBatchAsync(
                              (int)transfer.DestinationWarehouseId,
                                item.MedicineId,
                                item.BatchNumber);

                        if (destinationStock != null)
                        {
                            destinationStocks[key] = destinationStock;
                        }
                    }

                    if (destinationStock != null)
                    {
                        // التشغيلة موجودة مسبقاً في مخزن المستلِم — نزيد الكمية
                        destinationStock.Quantity += quantityReceived;
                    }
                    else
                    {
                        // التشغيلة غير موجودة في الوجهة — إنشاء سجل جديد
                        destinationStock = new InventoryStock
                        {
                            WarehouseId = (int)transfer.DestinationWarehouseId,
                            MedicineId  = item.MedicineId,
                            BatchNumber = item.BatchNumber,
                            ExpiryDate  = item.ExpiryDate,
                            Quantity    = quantityReceived
                        };
                        destinationStocks[key] = destinationStock;
                        await _unitOfWork.InventoryStocks.AddAsync(destinationStock);
                    }

                    // تسجيل حركة مخزنية (وارد — تحويل)
                    var movement = new InventoryMovement(
                        item.MedicineId,
                        null,
                        StockMovementType.SalesReturn,    // placeholder — وارد من تحويل
                        ReferenceType.Manual,
                        quantityReceived,                 // موجب = وارد للوجهة
                        transfer.Id,
                        transfer.TransferCode,
                        receivedByUserId,
                        $"وارد تحويل — سند {transfer.TransferCode} — لمخزن {transfer.DestinationWarehouseId}");
                    await _unitOfWork.InventoryMovements.AddAsync(movement);
                }

                // ===== كشف العجز وتنبيه الإدارة =====
                var dispatched = item.QuantityDispatched;
                if (quantityReceived < dispatched)
                {
                    hasVariance = true;
                    var variance = dispatched - quantityReceived;
                    _logger.LogWarning(
                        "Variance detected: Transfer={Code}, Medicine={MedicineId}, Batch={Batch}, Dispatched={D}, Received={R}, Shortage={V}",
                        transfer.TransferCode, item.MedicineId, item.BatchNumber, dispatched, quantityReceived, variance);
                }
            }

            // Update all modified destination stocks once
            foreach (var stock in destinationStocks.Values)
            {
                if (stock.Id > 0) // only update if it already existed in the DB
                {
                    stock.Medicine = null;
                    stock.Warehouse = null;
                    await _unitOfWork.InventoryStocks.UpdateAsync(stock);
                }
            }

            // تحديث حالة السند
            transfer.Status = TransferStatus.Received;
            transfer.ReceivedByUserId = receivedByUserId;
            transfer.ReceivedAt = DateTime.UtcNow;

            // Clear navigation properties to prevent EF Core tracking conflicts
            transfer.SourceWarehouse = null;
            transfer.DestinationWarehouse = null;
            transfer.RequestedByUser = null;
            transfer.ApprovedByUser = null;
            transfer.DispatchedByUser = null;
            transfer.ReceivedByUser = null;

            if (transfer.Items != null)
            {
                foreach (var item in transfer.Items)
                {
                    item.Medicine = null;
                    item.StockTransfer = null;
                }
            }

            await _unitOfWork.StockTransfers.UpdateAsync(transfer);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            // إرسال إشعار للإدارة عند وجود عجز
            if (hasVariance)
            {
                await _notificationService.SendNotificationAsync(
                    "عجز في تحويل مخزني",
                    $"تم رصد فارق في استلام سند التحويل {transfer.TransferCode}. يرجى المراجعة.",
                    "warning");
            }

            _logger.LogInformation("StockTransfer {Code} received successfully. HasVariance={HasVariance}",
                transfer.TransferCode, hasVariance);

            return MapToDto(transfer);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error receiving StockTransfer {Id}", id);
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        var transfer = await _unitOfWork.StockTransfers.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("تحويل المخزون غير موجود");
        await _unitOfWork.StockTransfers.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<int> GetCountByStatusAsync(TransferStatus status)
    {
        return await _unitOfWork.StockTransfers.GetCountByStatusAsync(status);
    }

    public async Task<decimal> GetTotalTransferredValueAsync(DateTime dateFrom, DateTime dateTo)
    {
        return await _unitOfWork.StockTransfers.GetTotalTransferredValueAsync(dateFrom, dateTo);
    }

    public async Task<bool> CodeExistsAsync(string transferCode, int? excludeId = null)
    {
        return await _unitOfWork.StockTransfers.CodeExistsAsync(transferCode, excludeId);
    }

    private StockTransferDto MapToDto(StockTransfer transfer)
    {
        var dto = _mapper.Map<StockTransferDto>(transfer);
        dto.RequestedAt = transfer.CreatedAt;
        return dto;
    }
}
