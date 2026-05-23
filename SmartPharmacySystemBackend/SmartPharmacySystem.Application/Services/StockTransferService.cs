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

        await _unitOfWork.StockTransfers.AddAsync(transfer);
        await _unitOfWork.SaveChangesAsync();

        if (dto.Items != null && dto.Items.Any())
        {
            foreach (var itemDto in dto.Items)
            {
                var item = _mapper.Map<StockTransferItem>(itemDto);
                item.StockTransferId = transfer.Id;
                await _unitOfWork.StockTransferItems.AddAsync(item);
            }
            await _unitOfWork.SaveChangesAsync();
        }

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
            // ===== خصم الكميات من مخزن المرسِل =====
            foreach (var item in transfer.Items)
            {
                // جلب سجل المخزون المطابق للتشغيلة في المخزن المرسِل
                var stock = await _unitOfWork.InventoryStocks
                    .GetByWarehouseMedicineBatchAsync(
                        transfer.SourceWarehouseId,
                        item.MedicineId,
                        item.BatchNumber);

                if (stock == null)
                    throw new InvalidOperationException(
                        $"الدواء '{item.Medicine?.Name ?? item.MedicineId.ToString()}' — التشغيلة '{item.BatchNumber}' غير موجود في المخزن المرسِل");

                if (stock.Quantity < item.QuantityRequested)
                    throw new InvalidOperationException(
                        $"الكمية غير كافية للدواء '{item.Medicine?.Name ?? item.MedicineId.ToString()}' — المطلوب: {item.QuantityRequested}، المتاح: {stock.Quantity}");

                // خصم الكمية
                stock.Quantity -= item.QuantityRequested;
                await _unitOfWork.InventoryStocks.UpdateAsync(stock);

                // تحديث QuantityDispatched في بند السند
                item.QuantityDispatched = item.QuantityRequested;
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

            // تحديث حالة السند
            transfer.Status = TransferStatus.Dispatched;
            transfer.DispatchedByUserId = dispatchedByUserId;
            transfer.DispatchedAt = DateTime.UtcNow;
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

            foreach (var itemDto in dto.Items)
            {
                // جلب بند السند المطابق
                var item = await _unitOfWork.StockTransferItems.GetByIdAsync(itemDto.StockTransferItemId);
                if (item == null) continue;

                var quantityReceived = itemDto.QuantityReceived;

                // تسجيل الكمية المستلمة في السند
                item.QuantityReceived = quantityReceived;
                await _unitOfWork.StockTransferItems.UpdateAsync(item);

                // ===== إضافة الكميات لمخزن الوجهة =====
                if (quantityReceived > 0)
                {
                    // البحث عن سجل المخزون في مخزن الوجهة لنفس الدواء والتشغيلة
                    var destStock = await _unitOfWork.InventoryStocks
                        .GetByWarehouseMedicineBatchAsync(
                            transfer.DestinationWarehouseId,
                            item.MedicineId,
                            item.BatchNumber);

                    if (destStock != null)
                    {
                        // التشغيلة موجودة في المخزن — زيادة الكمية
                        destStock.Quantity += quantityReceived;
                        await _unitOfWork.InventoryStocks.UpdateAsync(destStock);
                    }
                    else
                    {
                        // التشغيلة غير موجودة في الوجهة — إنشاء سجل جديد
                        var newStock = new InventoryStock
                        {
                            WarehouseId = transfer.DestinationWarehouseId,
                            MedicineId  = item.MedicineId,
                            BatchNumber = item.BatchNumber,
                            ExpiryDate  = item.ExpiryDate,
                            Quantity    = quantityReceived
                        };
                        await _unitOfWork.InventoryStocks.AddAsync(newStock);
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

            // تحديث حالة السند
            transfer.Status = TransferStatus.Received;
            transfer.ReceivedByUserId = receivedByUserId;
            transfer.ReceivedAt = DateTime.UtcNow;
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
