using AutoMapper;
using SmartPharmacySystem.Application.DTOs.DamagedGoods;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Application.DTOs.Financial;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace SmartPharmacySystem.Application.Services;

public class DamagedGoodsService : IDamagedGoodsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationService;
    private readonly IJournalEntryService _journalEntryService;
    private readonly ILogger<DamagedGoodsService> _logger;

    public DamagedGoodsService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService,
        INotificationService notificationService,
        IJournalEntryService journalEntryService,
        ILogger<DamagedGoodsService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
        _journalEntryService = journalEntryService;
        _logger = logger;
    }

    public async Task<DamagedGoodsRecordDto> GetByIdAsync(int id)
    {
        var record = await _unitOfWork.DamagedGoods.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("سجل التالف غير موجود");
        return _mapper.Map<DamagedGoodsRecordDto>(record);
    }

    public async Task<DamagedGoodsRecordDto> GetByCodeAsync(string damageCode)
    {
        var record = await _unitOfWork.DamagedGoods.GetByCodeAsync(damageCode)
            ?? throw new KeyNotFoundException("سجل التالف غير موجود");
        return _mapper.Map<DamagedGoodsRecordDto>(record);
    }

    public async Task<IEnumerable<DamagedGoodsRecordDto>> GetAllAsync(int? warehouseId = null, int? medicineId = null, DamageType? damageType = null, RecordStatus? status = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var records = await _unitOfWork.DamagedGoods.GetAllAsync(warehouseId, medicineId, damageType, status, dateFrom, dateTo);
        return _mapper.Map<IEnumerable<DamagedGoodsRecordDto>>(records);
    }

    public async Task<IEnumerable<DamagedGoodsRecordDto>> GetPendingApprovalAsync()
    {
        var records = await _unitOfWork.DamagedGoods.GetPendingApprovalAsync();
        return _mapper.Map<IEnumerable<DamagedGoodsRecordDto>>(records);
    }

    public async Task<IEnumerable<DamagedGoodsRecordDto>> GetApprovedAsync(DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var records = await _unitOfWork.DamagedGoods.GetApprovedAsync(dateFrom, dateTo);
        return _mapper.Map<IEnumerable<DamagedGoodsRecordDto>>(records);
    }

    public async Task<DamagedGoodsRecordDto> CreateAsync(CreateDamagedGoodsRecordDto dto)
    {
        var record = _mapper.Map<DamagedGoodsRecord>(dto);
        record.DamageCode = $"DMG-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
        record.Status = RecordStatus.PendingApproval;
        record.RecordedByUserId = _currentUserService.UserId ?? 0;
        record.DamageValue = await CalculateDamageValueAsync(dto.MedicineId, dto.BatchNumber, dto.Quantity);

        await _unitOfWork.DamagedGoods.AddAsync(record);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<DamagedGoodsRecordDto>(record);
    }

    public async Task UpdateAsync(DamagedGoodsRecordDto dto)
    {
        var record = await _unitOfWork.DamagedGoods.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException("سجل التالف غير موجود");

        _mapper.Map(dto, record);
        await _unitOfWork.DamagedGoods.UpdateAsync(record);
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// اعتماد إذن الإتلاف.
    /// عند الاعتماد:
    ///   1. تُخصم الكمية من InventoryStock في المخزن الأصلي.
    ///   2. تُضاف الكمية لمخزن التالف (WarehouseType=Damaged) للفرع نفسه.
    ///   3. تُسجَّل حركتان في InventoryMovement (صادر + وارد).
    /// </summary>
    public async Task<DamagedGoodsRecordDto> ApproveAsync(int id, int approvedByUserId)
    {
        var record = await _unitOfWork.DamagedGoods.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("سجل التالف غير موجود");

        if (record.Status != RecordStatus.PendingApproval)
            throw new InvalidOperationException("يمكن فقط اعتماد السجلات المعلقة");

        // جلب المخزن الأصلي لمعرفة الفرع المالك
        var sourceWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(record.SourceWarehouseId)
            ?? throw new InvalidOperationException(
                $"المخزن الأصلي (Id={record.SourceWarehouseId}) غير موجود");

        // جلب مخزن التالف الخاص بنفس الفرع
        var damagedWarehouse = await _unitOfWork.Warehouses
            .GetByBranchAndTypeAsync(sourceWarehouse.BranchId, WarehouseType.Damaged)
            ?? throw new InvalidOperationException(
                $"لا يوجد مخزن تالف (Damaged) مُعرَّف للفرع رقم {sourceWarehouse.BranchId}. " +
                "يرجى إنشاء مخزن تالف للفرع أولاً.");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // ===== 1. خصم الكمية من المخزن الأصلي =====
            var sourceStock = await _unitOfWork.InventoryStocks
                .GetByWarehouseMedicineBatchAsync(
                    record.SourceWarehouseId,
                    record.MedicineId,
                    record.BatchNumber);

            if (sourceStock == null)
                throw new InvalidOperationException(
                    $"التشغيلة '{record.BatchNumber}' للدواء (Id={record.MedicineId}) " +
                    $"غير موجودة في المخزن الأصلي (Id={record.SourceWarehouseId})");

            if (sourceStock.Quantity < record.Quantity)
                throw new InvalidOperationException(
                    $"كمية إذن الإتلاف ({record.Quantity}) أكبر من الكمية الفعلية في المخزن ({sourceStock.Quantity}). " +
                    "لا يمكن اعتماد الإذن.");

            sourceStock.Quantity -= record.Quantity;
            
            // Clear navigation properties to prevent EF Core tracking conflicts
            sourceStock.Medicine = null;
            sourceStock.Warehouse = null;
            await _unitOfWork.InventoryStocks.UpdateAsync(sourceStock);

            // حركة مخزنية — صادر (خسارة توالف من المخزن الأصلي)
            var outMovement = new InventoryMovement(
                record.MedicineId,
                null,
                StockMovementType.Damage,
                ReferenceType.Manual,
                -record.Quantity,
                record.Id,
                record.DamageCode,
                approvedByUserId,
                $"إذن إتلاف {record.DamageCode} — خسارة توالف — خصم من مخزن {record.SourceWarehouseId}");
            await _unitOfWork.InventoryMovements.AddAsync(outMovement);

            _logger.LogInformation(
                "Damaged goods approved: Record={Code}, Deducted {Qty} from SourceWarehouse={WH}",
                record.DamageCode, record.Quantity, record.SourceWarehouseId);

            // ===== 2. إضافة الكمية لمخزن التالف =====
            var damagedStock = await _unitOfWork.InventoryStocks
                .GetByWarehouseMedicineBatchAsync(
                    damagedWarehouse.Id,
                    record.MedicineId,
                    record.BatchNumber);

            if (damagedStock != null)
            {
                // التشغيلة موجودة مسبقاً في مخزن التالف — نزيد الكمية
                damagedStock.Quantity += record.Quantity;
                
                // Clear navigation properties
                damagedStock.Medicine = null;
                damagedStock.Warehouse = null;
                await _unitOfWork.InventoryStocks.UpdateAsync(damagedStock);
            }
            else
            {
                // إنشاء سجل مخزني جديد في مخزن التالف
                var newDamagedStock = new InventoryStock
                {
                    WarehouseId = damagedWarehouse.Id,
                    MedicineId  = record.MedicineId,
                    BatchNumber = record.BatchNumber,
                    ExpiryDate  = record.ExpiryDate,
                    Quantity    = record.Quantity
                };
                await _unitOfWork.InventoryStocks.AddAsync(newDamagedStock);
            }

            // حركة مخزنية — وارد (نقل للمخزن التالف)
            var inMovement = new InventoryMovement(
                record.MedicineId,
                null,
                StockMovementType.Damage,
                ReferenceType.Manual,
                record.Quantity,
                record.Id,
                record.DamageCode,
                approvedByUserId,
                $"إذن إتلاف {record.DamageCode} — نقل لمخزن التالف {damagedWarehouse.Id}");
            await _unitOfWork.InventoryMovements.AddAsync(inMovement);

            _logger.LogInformation(
                "Damaged goods: Added {Qty} to DamagedWarehouse={WH} (Branch={Branch})",
                record.Quantity, damagedWarehouse.Id, sourceWarehouse.BranchId);

            // ===== 3. تحديث حالة السجل =====
            record.Status = RecordStatus.Approved;
            record.ApprovedByUserId = approvedByUserId;
            record.ApprovedAt = DateTime.UtcNow;

            // Clear navigation properties
            record.Medicine = null;
            record.SourceWarehouse = null;
            record.RecordedByUser = null;
            record.ApprovedByUser = null;

            await _unitOfWork.DamagedGoods.UpdateAsync(record);

            if (record.DamageValue > 0)
            {
                var journalEntry = new JournalEntryDto
                {
                    EntryDate = DateTime.UtcNow,
                    VoucherNumber = record.DamageCode,
                    Description = $"قيد إتلاف أدوية آلي - إذن رقم: {record.DamageCode}",
                    Type = VoucherType.JournalEntry,
                    Lines = new List<JournalEntryLineDto>
                    {
                        new()
                        {
                            AccountId = 5206,
                            Debit = record.DamageValue,
                            Credit = 0,
                            Description = $"خسائر أدوية تالفة - {record.DamageCode}"
                        },
                        new()
                        {
                            AccountId = 1301,
                            Debit = 0,
                            Credit = record.DamageValue,
                            Description = $"تخفيض مخزون أدوية تالفة - {record.DamageCode}"
                        }
                    }
                };

                var createdEntry = await _journalEntryService.CreateAsync(journalEntry, approvedByUserId);
                await _journalEntryService.ApproveAsync(createdEntry.Id, approvedByUserId);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            // إشعار الإدارة بالخسارة المالية
            await _notificationService.SendNotificationAsync(
                "اعتماد إذن إتلاف",
                $"تم اعتماد إذن الإتلاف {record.DamageCode} — " +
                $"قيمة الخسارة: {record.DamageValue:N2} — " +
                $"الكمية: {record.Quantity} وحدة.",
                "warning");

            return _mapper.Map<DamagedGoodsRecordDto>(record);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error approving DamagedGoodsRecord {Id}", id);
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        var record = await _unitOfWork.DamagedGoods.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("سجل التالف غير موجود");
        await _unitOfWork.DamagedGoods.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<decimal> GetTotalDamageValueAsync(DateTime dateFrom, DateTime dateTo)
    {
        return await _unitOfWork.DamagedGoods.GetTotalDamageValueAsync(dateFrom, dateTo);
    }

    public async Task<int> GetCountByStatusAsync(RecordStatus status)
    {
        return await _unitOfWork.DamagedGoods.GetCountByStatusAsync(status);
    }

    public async Task<bool> CodeExistsAsync(string damageCode, int? excludeId = null)
    {
        return await _unitOfWork.DamagedGoods.CodeExistsAsync(damageCode, excludeId);
    }

    private async Task<decimal> CalculateDamageValueAsync(int medicineId, string batchNumber, int quantity)
    {
        var batch = await _unitOfWork.MedicineBatches.GetByMedicineIdAndBatchNumberAsync(medicineId, batchNumber);
        if (batch != null && batch.UnitPurchasePrice > 0)
            return Math.Round(batch.UnitPurchasePrice * quantity, 2);

        var medicine = await _unitOfWork.Medicines.GetByIdAsync(medicineId)
            ?? throw new KeyNotFoundException("Medicine was not found.");

        return Math.Round(medicine.MovingAverageCost * quantity, 2);
    }
}
