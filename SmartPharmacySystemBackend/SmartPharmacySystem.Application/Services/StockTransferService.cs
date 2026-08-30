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

    /// <summary>
    /// Gets all stock transfers based on the provided filters.
    /// </summary>
    public async Task<IEnumerable<StockTransferDto>> GetAllAsync(int? sourceWarehouseId = null, int? destinationWarehouseId = null, TransferStatus? status = null, DateTime? dateFrom = null, DateTime? dateTo = null, TransferType? transferType = null)
    {
        var transfers = await _unitOfWork.StockTransfers.GetAllAsync(sourceWarehouseId, destinationWarehouseId, status, dateFrom, dateTo, transferType);
        return transfers.Select(MapToDto);
    }

    public async Task<IEnumerable<StockTransferDto>> GetByBranchAsync(int branchId, TransferType? transferType = null)
    {
        var transfers = await _unitOfWork.StockTransfers.GetByBranchAsync(branchId, transferType);
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
        transfer.RequestedByUserId = requestedByUserId;
        transfer.TransferCode = $"TRF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        // Resolve branch IDs from warehouses
        var sourceWh = await _unitOfWork.Warehouses.GetByIdAsync(transfer.SourceWarehouseId)
            ?? throw new InvalidOperationException("المخزن المصدر غير موجود");
        int sourceBranchId = sourceWh.BranchId;
        var sourceBranch = await _unitOfWork.Branches.GetByIdAsync(sourceBranchId);
        bool isSourceMain = sourceBranch?.BranchType == BranchType.Main;

        // Resolve destination branch
        int? destBranchId = transfer.DestinationBranchId;
        if (!destBranchId.HasValue && transfer.DestinationWarehouseId.HasValue)
            destBranchId = (await _unitOfWork.Warehouses.GetByIdAsync(transfer.DestinationWarehouseId.Value))?.BranchId;

        // Always persist the resolved IDs
        transfer.BranchId = sourceBranchId;
        if (destBranchId.HasValue && !transfer.DestinationBranchId.HasValue)
            transfer.DestinationBranchId = destBranchId;

        // === Determine initial status and notification based on TransferType ===
        if (transfer.TransferType == TransferType.Internal)
        {
            // داخلي: يحتاج اعتماد مدير الفرع
            transfer.Status = TransferStatus.Requested;
        }
        else if (transfer.TransferType == TransferType.External)
        {
            if (isSourceMain)
            {
                // مركزي: المركز لديه السلطة — يُعتمد تلقائياً وجاهز للشحن
                transfer.Status = TransferStatus.Approved;
                transfer.ApprovedByUserId = requestedByUserId;
                transfer.ApprovedAt = DateTime.UtcNow;
            }
            else
            {
                // جانبي Push: يحتاج موافقة فرع الاستلام
                transfer.Status = TransferStatus.Requested;
            }
        }
        else if (transfer.TransferType == TransferType.BranchRequest)
        {
            // Pull: يحتاج موافقة فرع المصدر
            transfer.Status = TransferStatus.Requested;
        }
        else
        {
            transfer.Status = TransferStatus.Requested;
        }

        await _unitOfWork.StockTransfers.AddAsync(transfer);
        await _unitOfWork.SaveChangesAsync();

        // === إرسال الإشعارات ===
        if (transfer.TransferType == TransferType.External && !isSourceMain && destBranchId.HasValue)
        {
            // جانبي Push: نُبلّغ فرع الوجهة ليوافق
            await _notificationService.NotifyBranchAsync(
                destBranchId.Value,
                NotificationType.TransferRequest,
                "اقتراح تحويل وارد",
                $"فرع '{sourceBranch?.Name ?? "آخر"}' يرغب بإرسال بضاعة إليك. رقم الطلب: {transfer.TransferCode} — يرجى المراجعة والموافقة أو الرفض.",
                transfer.Id, "StockTransfer");
        }
        else if (transfer.TransferType == TransferType.BranchRequest && destBranchId.HasValue)
        {
            // Pull: نُبلّغ فرع المصدر بأن هناك طلب بضاعة منه
            await _notificationService.NotifyBranchAsync(
                sourceBranchId,
                NotificationType.TransferRequest,
                "طلب بضاعة جديد",
                $"فرع آخر يطلب منك بضاعة برقم {transfer.TransferCode}. يرجى الموافقة والشحن.",
                transfer.Id, "StockTransfer");
        }
        else if (transfer.TransferType == TransferType.External && isSourceMain && destBranchId.HasValue)
        {
            // مركزي: نُبلّغ فرع الوجهة بأن شحنة في الطريق
            await _notificationService.NotifyBranchAsync(
                destBranchId.Value,
                NotificationType.TransferRequest,
                "شحنة قادمة من المركز",
                $"المركز الرئيسي أنشأ سند تحويل رقم {transfer.TransferCode} وسيُشحن إليك قريباً.",
                transfer.Id, "StockTransfer");
        }

        return MapToDto(transfer);
    }

    public async Task<StockTransferDto> ApproveAsync(int id, int approvedByUserId)
    {
        var transfer = await _unitOfWork.StockTransfers.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("تحويل المخزون غير موجود");

        if (transfer.Status != TransferStatus.Requested)
            throw new InvalidOperationException("يمكن فقط اعتماد التحويلات في حالة 'بانتظار الاعتماد'");

        // === التحويل الداخلي: تنفيذ فوري ===
        if (transfer.TransferType == TransferType.Internal)
        {
            await ExecuteInternalTransferAsync(transfer, approvedByUserId);
            return MapToDto(transfer);
        }

        // === التحقق من صلاحية المعتمِد للتحويلات الخارجية ===
        bool isAdmin = await IsAdminAsync(approvedByUserId);
        int? approverBranchId = await GetUserBranchIdAsync(approvedByUserId);

        var sourceWh = await _unitOfWork.Warehouses.GetByIdAsync(transfer.SourceWarehouseId);
        int? sourceBranchId = sourceWh?.BranchId;
        int? destBranchId = transfer.DestinationBranchId
            ?? (transfer.DestinationWarehouseId.HasValue ? (await _unitOfWork.Warehouses.GetByIdAsync(transfer.DestinationWarehouseId.Value))?.BranchId : null);

        if (!isAdmin)
        {
            if (transfer.TransferType == TransferType.External)
            {
                // جانبي Push: يوافق فرع الوجهة (المستلم)
                if (approverBranchId != destBranchId)
                    throw new UnauthorizedAccessException("فقط فرع الاستلام يمكنه اعتماد هذا التحويل.");
            }
            else if (transfer.TransferType == TransferType.BranchRequest)
            {
                // Pull: يوافق فرع المصدر (المرسِل)
                if (approverBranchId != sourceBranchId)
                    throw new UnauthorizedAccessException("فقط فرع المصدر يمكنه اعتماد هذا الطلب.");
            }
        }

        transfer.Status = TransferStatus.Approved;
        transfer.ApprovedByUserId = approvedByUserId;
        transfer.ApprovedAt = DateTime.UtcNow;

        ClearNavigationProperties(transfer);
        await _unitOfWork.StockTransfers.UpdateAsync(transfer);
        await _unitOfWork.SaveChangesAsync();

        // === إرسال الإشعار الصحيح بعد الاعتماد ===
        if (transfer.TransferType == TransferType.External && sourceBranchId.HasValue)
        {
            // Push: الوجهة وافقت — نُبلّغ المصدر بأن عليه الشحن الآن
            await _notificationService.NotifyBranchAsync(
                sourceBranchId.Value,
                NotificationType.TransferRequest,
                "موافقة على تحويل — ابدأ الشحن",
                $"وافق فرع الاستلام على طلب التحويل {transfer.TransferCode}. يرجى تجهيز البضاعة وشحنها الآن.",
                transfer.Id, "StockTransfer");
        }
        else if (transfer.TransferType == TransferType.BranchRequest && destBranchId.HasValue)
        {
            // Pull: المصدر وافق — نُبلّغ الفرع الطالب بالموافقة وأن البضاعة ستُشحن قريباً
            await _notificationService.NotifyBranchAsync(
                destBranchId.Value,
                NotificationType.TransferRequest,
                "تمت الموافقة على طلبك",
                $"وافق فرع المصدر على طلبك رقم {transfer.TransferCode}. البضاعة ستُشحن إليك قريباً.",
                transfer.Id, "StockTransfer");
        }

        return MapToDto(transfer);
    }

    public async Task<StockTransferDto> DispatchAsync(int id, int dispatchedByUserId)
    {
        var transfer = await _unitOfWork.StockTransfers.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("تحويل المخزون غير موجود");

        if (transfer.Status != TransferStatus.Approved)
            throw new InvalidOperationException("يمكن فقط شحن التحويلات المعتمدة");

        if (transfer.Items == null || !transfer.Items.Any())
            throw new InvalidOperationException("لا توجد بنود في سند التحويل");

        // التحقق من صلاحية الشحن: فقط فرع المصدر أو Admin
        bool isAdmin = await IsAdminAsync(dispatchedByUserId);
        int? dispatcherBranchId = await GetUserBranchIdAsync(dispatchedByUserId);
        int sourceBranchId = transfer.BranchId;
        if (!isAdmin && dispatcherBranchId != sourceBranchId)
            throw new UnauthorizedAccessException("فقط فرع المصدر يمكنه شحن هذا التحويل.");

        int? destBranchId = transfer.DestinationBranchId;

        try
        {
            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
            var sourceStocks = new Dictionary<string, InventoryStock>();

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
                            $"الدواء '{item.Medicine?.Name ?? item.MedicineId.ToString()}' — التشغيلة '{item.BatchNumber}' غير موجود في المخزن المصدر");

                    sourceStocks[key] = stock;
                }

                if (stock.Quantity < item.QuantityRequested)
                    throw new InvalidOperationException(
                        $"الكمية غير كافية للدواء '{item.Medicine?.Name ?? item.MedicineId.ToString()}' — المطلوب: {item.QuantityRequested}، المتاح: {stock.Quantity}");

                stock.Quantity -= item.QuantityRequested;
                item.QuantityDispatched = item.QuantityRequested;
                item.Medicine = null;
                item.StockTransfer = null;
                await _unitOfWork.StockTransferItems.UpdateAsync(item);

                var movement = new InventoryMovement(
                    item.MedicineId, null,
                    StockMovementType.TransferOut, ReferenceType.BranchTransfer,
                    -item.QuantityRequested,
                    transfer.Id, transfer.TransferCode, dispatchedByUserId,
                    $"صادر تحويل — سند {transfer.TransferCode}");
                await _unitOfWork.InventoryMovements.AddAsync(movement);
            }

            foreach (var stock in sourceStocks.Values)
            {
                stock.Medicine = null;
                stock.Warehouse = null;
                await _unitOfWork.InventoryStocks.UpdateAsync(stock);
            }

            transfer.Status = TransferStatus.Dispatched;
            transfer.DispatchedByUserId = dispatchedByUserId;
            transfer.DispatchedAt = DateTime.UtcNow;

            ClearNavigationProperties(transfer);
            await _unitOfWork.StockTransfers.UpdateAsync(transfer);
            await _unitOfWork.SaveChangesAsync();
            });

            // إشعار فرع الوجهة بأن البضاعة في الطريق
            if (destBranchId.HasValue)
            {
                await _notificationService.NotifyBranchAsync(
                    destBranchId.Value,
                    NotificationType.TransferRequest,
                    "🚚 بضاعة في الطريق إليك",
                    $"تم شحن البضاعة الخاصة بسند التحويل {transfer.TransferCode}. يرجى الاستعداد للاستلام.",
                    transfer.Id, "StockTransfer");
            }

            _logger.LogInformation("StockTransfer {Code} dispatched by user {UserId}", transfer.TransferCode, dispatchedByUserId);
            return MapToDto(transfer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching StockTransfer {Id}", id);
            throw;
        }
    }

    public async Task<StockTransferDto> ReceiveAsync(int id, ReceiveStockTransferDto dto, int receivedByUserId)
    {
        var transfer = await _unitOfWork.StockTransfers.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("تحويل المخزون غير موجود");

        if (transfer.Status != TransferStatus.Dispatched)
            throw new InvalidOperationException("يمكن فقط استلام التحويلات التي تم شحنها");

        if (dto.Items == null || !dto.Items.Any())
            throw new InvalidOperationException("يجب إدخال الكميات المستلمة");

        // التحقق من صلاحية الاستلام: فقط فرع الوجهة أو Admin
        bool isAdmin = await IsAdminAsync(receivedByUserId);
        int? receiverBranchId = await GetUserBranchIdAsync(receivedByUserId);
        int? destBranchId = transfer.DestinationBranchId;
        if (!isAdmin && destBranchId.HasValue && receiverBranchId != destBranchId)
            throw new UnauthorizedAccessException("فقط فرع الوجهة يمكنه استلام هذه البضاعة.");

        int targetWarehouseId = dto.DestinationWarehouseId
            ?? transfer.DestinationWarehouseId
            ?? throw new InvalidOperationException("يجب تحديد مخزن الوجهة لإتمام الاستلام");

        bool hasVariance = false;
        try
        {
            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
            var destinationStocks = new Dictionary<string, InventoryStock>();

            if (transfer.DestinationWarehouseId == null)
                transfer.DestinationWarehouseId = targetWarehouseId;

            foreach (var itemDto in dto.Items)
            {
                var item = transfer.Items?.FirstOrDefault(i => i.Id == itemDto.StockTransferItemId);
                if (item == null) continue;

                var quantityReceived = itemDto.QuantityReceived;
                item.QuantityReceived = quantityReceived;

                if (quantityReceived > 0)
                {
                    var key = $"{item.MedicineId}_{item.BatchNumber}";
                    if (!destinationStocks.TryGetValue(key, out var destinationStock))
                    {
                        destinationStock = await _unitOfWork.InventoryStocks
                            .GetByWarehouseMedicineBatchAsync(targetWarehouseId, item.MedicineId, item.BatchNumber);
                        if (destinationStock != null)
                            destinationStocks[key] = destinationStock;
                    }

                    if (destinationStock != null)
                    {
                        destinationStock.Quantity += quantityReceived;
                    }
                    else
                    {
                        destinationStock = new InventoryStock
                        {
                            WarehouseId = targetWarehouseId,
                            MedicineId  = item.MedicineId,
                            BatchNumber = item.BatchNumber,
                            ExpiryDate  = item.ExpiryDate,
                            Quantity    = quantityReceived
                        };
                        destinationStocks[key] = destinationStock;
                        await _unitOfWork.InventoryStocks.AddAsync(destinationStock);
                    }

                    var movement = new InventoryMovement(
                        item.MedicineId, null,
                        StockMovementType.TransferIn, ReferenceType.BranchTransfer,
                        quantityReceived,
                        transfer.Id, transfer.TransferCode, receivedByUserId,
                        $"وارد تحويل — سند {transfer.TransferCode} — لمخزن {targetWarehouseId}");
                    await _unitOfWork.InventoryMovements.AddAsync(movement);
                }

                if (quantityReceived < item.QuantityDispatched)
                {
                    hasVariance = true;
                    _logger.LogWarning("Variance: Transfer={Code}, Medicine={MedicineId}, Dispatched={D}, Received={R}",
                        transfer.TransferCode, item.MedicineId, item.QuantityDispatched, quantityReceived);
                }
            }

            foreach (var stock in destinationStocks.Values)
            {
                if (stock.Id > 0)
                {
                    stock.Medicine = null;
                    stock.Warehouse = null;
                    await _unitOfWork.InventoryStocks.UpdateAsync(stock);
                }
            }

            transfer.Status = TransferStatus.Received;
            transfer.ReceivedByUserId = receivedByUserId;
            transfer.ReceivedAt = DateTime.UtcNow;

            ClearNavigationProperties(transfer);
            await _unitOfWork.StockTransfers.UpdateAsync(transfer);
            await _unitOfWork.SaveChangesAsync();
            });

            // إشعار فرع المصدر بتقرير الاستلام
            int? sourceBranchId = transfer.BranchId > 0 ? transfer.BranchId
                : (await _unitOfWork.Warehouses.GetByIdAsync(transfer.SourceWarehouseId))?.BranchId;

            if (sourceBranchId.HasValue)
            {
                string notifTitle = hasVariance ? "⚠️ استلام مع وجود فوارق" : "✅ اكتمل الاستلام";
                string notifBody = hasVariance
                    ? $"استلم فرع الوجهة بضاعة سند {transfer.TransferCode} مع وجود فوارق في الكميات. يرجى المراجعة المحاسبية."
                    : $"استلم فرع الوجهة بضاعة سند {transfer.TransferCode} بالكامل وبدون عجز.";

                await _notificationService.NotifyBranchAsync(
                    sourceBranchId.Value,
                    NotificationType.TransferRequest,
                    notifTitle, notifBody,
                    transfer.Id, "StockTransfer");
            }

            _logger.LogInformation("StockTransfer {Code} received. HasVariance={HasVariance}", transfer.TransferCode, hasVariance);
            return MapToDto(transfer);
        }
        catch (Exception ex)
        {
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

    /// <summary>Returns true if the user has the Admin role.</summary>
    private async Task<bool> IsAdminAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return false;
        var role = await _unitOfWork.Roles.GetByIdAsync(user.RoleId);
        return role?.Name == "Admin";
    }

    /// <summary>Returns the active branch assignment for the user, or null.</summary>
    private async Task<int?> GetUserBranchIdAsync(int userId)
    {
        var assignment = await _unitOfWork.EmployeeBranchAssignments.GetActiveAssignmentByUserIdAsync(userId);
        return assignment?.BranchId;
    }

    /// <summary>Clears all EF navigation properties to avoid update conflicts.</summary>
    private static void ClearNavigationProperties(StockTransfer transfer)
    {
        transfer.SourceWarehouse = null;
        transfer.DestinationWarehouse = null;
        transfer.DestinationBranch = null;
        transfer.RequestedByUser = null;
        transfer.ApprovedByUser = null;
        transfer.DispatchedByUser = null;
        transfer.ReceivedByUser = null;

        if (transfer.Items != null)
            foreach (var item in transfer.Items)
            {
                item.Medicine = null;
                item.StockTransfer = null;
            }
    }

    private async Task ExecuteInternalTransferAsync(StockTransfer transfer, int approvedByUserId)
    {
        if (transfer.DestinationWarehouseId == null)
            throw new InvalidOperationException("المخزن الوجهة غير محدد للتحويل الداخلي");
            
        try
        {
            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
            var sourceStocks = new Dictionary<string, InventoryStock>();
            var destinationStocks = new Dictionary<string, InventoryStock>();

            foreach (var item in transfer.Items)
            {
                var key = $"{item.MedicineId}_{item.BatchNumber}";
                
                // Deduct from Source
                if (!sourceStocks.TryGetValue(key, out var sourceStock))
                {
                    sourceStock = await _unitOfWork.InventoryStocks
                        .GetByWarehouseMedicineBatchAsync(transfer.SourceWarehouseId, item.MedicineId, item.BatchNumber);

                    if (sourceStock == null || sourceStock.Quantity < item.QuantityRequested)
                        throw new InvalidOperationException($"الكمية غير كافية للدواء في المخزن المصدر");

                    sourceStocks[key] = sourceStock;
                }
                
                sourceStock.Quantity -= item.QuantityRequested;
                item.QuantityDispatched = item.QuantityRequested;
                item.QuantityReceived = item.QuantityRequested;

                // Add to Destination
                if (!destinationStocks.TryGetValue(key, out var destinationStock))
                {
                    destinationStock = await _unitOfWork.InventoryStocks
                        .GetByWarehouseMedicineBatchAsync(transfer.DestinationWarehouseId.Value, item.MedicineId, item.BatchNumber);

                    if (destinationStock != null)
                    {
                        destinationStocks[key] = destinationStock;
                    }
                }

                if (destinationStock != null)
                {
                    destinationStock.Quantity += item.QuantityRequested;
                }
                else
                {
                    destinationStock = new InventoryStock
                    {
                        WarehouseId = transfer.DestinationWarehouseId.Value,
                        MedicineId = item.MedicineId,
                        BatchNumber = item.BatchNumber,
                        ExpiryDate = item.ExpiryDate,
                        Quantity = item.QuantityRequested
                    };
                    destinationStocks[key] = destinationStock;
                    await _unitOfWork.InventoryStocks.AddAsync(destinationStock);
                }

                item.Medicine = null;
                item.StockTransfer = null;
                await _unitOfWork.StockTransferItems.UpdateAsync(item);
                
                var movOut = new InventoryMovement(
                    item.MedicineId, null,
                    StockMovementType.TransferOut, ReferenceType.BranchTransfer,
                    -item.QuantityRequested,
                    transfer.Id, transfer.TransferCode, approvedByUserId,
                    $"صادر تحويل داخلي — سند {transfer.TransferCode}");
                await _unitOfWork.InventoryMovements.AddAsync(movOut);

                var movIn = new InventoryMovement(
                    item.MedicineId, null,
                    StockMovementType.TransferIn, ReferenceType.BranchTransfer,
                    item.QuantityRequested,
                    transfer.Id, transfer.TransferCode, approvedByUserId,
                    $"وارد تحويل داخلي — سند {transfer.TransferCode}");
                await _unitOfWork.InventoryMovements.AddAsync(movIn);
            }

            foreach (var stock in sourceStocks.Values)
            {
                stock.Medicine = null; stock.Warehouse = null;
                await _unitOfWork.InventoryStocks.UpdateAsync(stock);
            }
            foreach (var stock in destinationStocks.Values)
            {
                if (stock.Id > 0)
                {
                    stock.Medicine = null; stock.Warehouse = null;
                    await _unitOfWork.InventoryStocks.UpdateAsync(stock);
                }
            }

            transfer.Status = TransferStatus.Received; // Mark completely done
            transfer.ApprovedByUserId = approvedByUserId;
            transfer.ApprovedAt = DateTime.UtcNow;
            transfer.DispatchedByUserId = approvedByUserId;
            transfer.DispatchedAt = DateTime.UtcNow;
            transfer.ReceivedByUserId = approvedByUserId;
            transfer.ReceivedAt = DateTime.UtcNow;

            transfer.SourceWarehouse = null;
            transfer.DestinationWarehouse = null;
            transfer.RequestedByUser = null;

            await _unitOfWork.StockTransfers.UpdateAsync(transfer);
            await _unitOfWork.SaveChangesAsync();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing internal StockTransfer {Id}", transfer.Id);
            throw;
        }
    }
}
