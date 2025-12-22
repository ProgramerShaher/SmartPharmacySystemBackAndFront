using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.MedicineBatch;
using SmartPharmacySystem.Application.DTOs.StockMovement;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

/// <summary>
/// Service implementation for medicine batch business operations.
/// تنفيذ الخدمة لعمليات الأعمال الخاصة بدفعات الأدوية.
/// </summary>
public class MedicineBatchService : IMedicineBatchService
{
    private readonly IMedicineBatchRepository _batchRepository;
    private readonly IMedicineRepository _medicineRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MedicineBatchService> _logger;
    private readonly IMapper _mapper;
    private readonly IStockMovementService _stockMovementService;

    public MedicineBatchService(
        IMedicineBatchRepository batchRepository,
        IMedicineRepository medicineRepository,
        IUnitOfWork unitOfWork,
        ILogger<MedicineBatchService> logger,
        IMapper mapper,
        IStockMovementService stockMovementService)
    {
        _batchRepository = batchRepository;
        _medicineRepository = medicineRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
        _stockMovementService = stockMovementService;
    }

    // ===================== CRUD Operations =====================

    /// <inheritdoc/>
    public async Task<MedicineBatchResponseDto> CreateBatchAsync(MedicineBatchCreateDto dto)
    {
        _logger.LogInformation("Creating new batch for medicine {MedicineId} with batch number {CompanyBatchNumber}",
            dto.MedicineId, dto.CompanyBatchNumber);

        // Validate medicine exists
        var medicine = await _medicineRepository.GetByIdAsync(dto.MedicineId);
        if (medicine == null)
        {
            _logger.LogWarning("Medicine {MedicineId} not found | الدواء غير موجود", dto.MedicineId);
            throw new KeyNotFoundException($"Medicine with ID {dto.MedicineId} not found | الدواء برقم {dto.MedicineId} غير موجود");
        }

        // Check for duplicate batch number for this medicine
        if (await _batchRepository.BatchNumberExistsAsync(dto.MedicineId, dto.CompanyBatchNumber))
        {
            _logger.LogWarning("Batch number {BatchNumber} already exists for medicine {MedicineId}",
                dto.CompanyBatchNumber, dto.MedicineId);
            throw new InvalidOperationException(
                $"Batch number '{dto.CompanyBatchNumber}' already exists for this medicine | رقم الدفعة '{dto.CompanyBatchNumber}' موجود بالفعل لهذا الدواء");
        }

        // Check for duplicate barcode
        if (!string.IsNullOrEmpty(dto.BatchBarcode) && await _batchRepository.BarcodeExistsAsync(dto.BatchBarcode))
        {
            _logger.LogWarning("Barcode {BatchBarcode} already exists", dto.BatchBarcode);
            throw new InvalidOperationException(
                $"Barcode '{dto.BatchBarcode}' already exists | الباركود '{dto.BatchBarcode}' موجود بالفعل");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Create entity
            var batch = new MedicineBatch
            {
                MedicineId = dto.MedicineId,
                CompanyBatchNumber = dto.CompanyBatchNumber,
                BatchBarcode = dto.BatchBarcode,
                Quantity = dto.Quantity,
                ExpiryDate = dto.ExpiryDate,
                UnitPurchasePrice = dto.UnitPurchasePrice,
                EntryDate = dto.EntryDate,
                StorageLocation = dto.StorageLocation,
                Status = "Active",
                CreatedBy = dto.CreatedBy,
                IsDeleted = false
            };

            await _batchRepository.AddAsync(batch);
            await _unitOfWork.SaveChangesAsync(); // Save to get the Batch ID

            // Initialize audited movement: Create a Purchase movement equal to the batch quantity
            var initialMovement = new InventoryMovement(
                batch.MedicineId,
                batch.Id,
                StockMovementType.Purchase,
                ReferenceType.Manual, // Direct entry via API
                batch.Quantity,
                batch.Id, // Link to the batch itself as reference if no invoice
                "INIT-" + batch.CompanyBatchNumber,
                dto.CreatedBy,
                "إدخال رصيد أول المدة / إضافة مخزون مباشر"
            );

            await _unitOfWork.InventoryMovements.AddAsync(initialMovement);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Batch created successfully with ID {BatchId} and initial movement recorded | تم إنشاء الدفعة بنجاح وتسجيل الحركة الأولية", batch.Id);

            // Re-fetch to include navigation properties and calculated fields
            var result = await _batchRepository.GetByIdAsync(batch.Id);
            return MapToResponseDto(result!, medicine);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error creating batch for medicine {MedicineId}", dto.MedicineId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<MedicineBatchResponseDto> UpdateBatchAsync(int batchId, MedicineBatchUpdateDto dto)
    {
        _logger.LogInformation("Updating batch {BatchId}", batchId);

        var batch = await _batchRepository.GetByIdAsync(batchId);
        if (batch == null)
        {
            _logger.LogWarning("Batch {BatchId} not found", batchId);
            throw new KeyNotFoundException($"Batch with ID {batchId} not found | الدفعة برقم {batchId} غير موجودة");
        }

        // Update only provided fields
        if (dto.Quantity.HasValue)
        {
            // Validate that new quantity is not less than sold quantity
            var soldQuantity = batch.Quantity - batch.RemainingQuantity;
            if (dto.Quantity.Value < soldQuantity)
            {
                throw new InvalidOperationException(
                    $"Cannot reduce quantity below sold amount ({soldQuantity}) | لا يمكن تقليل الكمية أقل من الكمية المباعة ({soldQuantity})");
            }
            batch.Quantity = dto.Quantity.Value;
        }

        if (dto.RemainingQuantity.HasValue)
        {
            // RemainingQuantity is now read-only and calculated dynamically. 
            // Direct update via this field is deprecated.
            _logger.LogWarning("Direct update of RemainingQuantity for batch {BatchId} is ignored in the audited system.", batchId);
        }

        if (dto.ExpiryDate.HasValue)
            batch.ExpiryDate = dto.ExpiryDate.Value;

        if (dto.UnitPurchasePrice.HasValue)
            batch.UnitPurchasePrice = dto.UnitPurchasePrice.Value;

        if (dto.StorageLocation != null)
            batch.StorageLocation = dto.StorageLocation;

        if (!string.IsNullOrEmpty(dto.Status))
        {
            // Prevent setting expired batch to Active
            if (dto.Status == "Active" && batch.IsExpired)
            {
                throw new InvalidOperationException(
                    "Cannot set expired batch to Active | لا يمكن تعيين دفعة منتهية الصلاحية كنشطة");
            }
            batch.Status = dto.Status;
        }

        // Auto-mark as expired if past expiry date
        if (batch.IsExpired && batch.Status == "Active")
        {
            batch.Status = "Expired";
        }

        await _batchRepository.UpdateAsync(batch);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Batch {BatchId} updated successfully | تم تحديث الدفعة بنجاح", batchId);

        return MapToResponseDto(batch, batch.Medicine);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteBatchAsync(int batchId, int deletedByUserId)
    {
        _logger.LogInformation("Deleting batch {BatchId}", batchId);

        var batch = await _batchRepository.GetByIdAsync(batchId);
        if (batch == null)
        {
            _logger.LogWarning("Batch {BatchId} not found", batchId);
            throw new KeyNotFoundException($"Batch with ID {batchId} not found | الدفعة برقم {batchId} غير موجودة");
        }

        batch.SoftDelete();
        await _batchRepository.UpdateAsync(batch);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Batch {BatchId} deleted successfully (soft delete) | تم حذف الدفعة بنجاح", batchId);

        return true;
    }

    // ===================== Query Operations =====================

    /// <inheritdoc/>
    public async Task<MedicineBatchResponseDto?> GetBatchByIdAsync(int batchId)
    {
        _logger.LogInformation("Getting batch {BatchId}", batchId);

        var batch = await _batchRepository.GetByIdAsync(batchId);
        if (batch == null)
            return null;

        return MapToResponseDto(batch, batch.Medicine);
    }

    /// <inheritdoc/>
    public async Task<MedicineBatchResponseDto?> GetBatchByBarcodeAsync(string barcode)
    {
        _logger.LogInformation("Getting batch by barcode {Barcode}", barcode);

        if (string.IsNullOrWhiteSpace(barcode))
        {
            throw new ArgumentException("Barcode cannot be empty | الباركود لا يمكن أن يكون فارغاً");
        }

        var batch = await _batchRepository.GetByBarcodeAsync(barcode);
        if (batch == null)
        {
            _logger.LogWarning("No batch found with barcode {Barcode}", barcode);
            throw new KeyNotFoundException($"No batch found with barcode '{barcode}' | لا توجد دفعة بالباركود '{barcode}'");
        }

        return MapToResponseDto(batch, batch.Medicine);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MedicineBatchResponseDto>> GetBatchesByMedicineIdAsync(int medicineId)
    {
        _logger.LogInformation("Getting batches for medicine {MedicineId}", medicineId);

        var batches = await _batchRepository.GetBatchesByMedicineIdAsync(medicineId);

        if (!batches.Any())
        {
            _logger.LogInformation("No batches found for medicine {MedicineId}", medicineId);
        }

        return batches.Select(b => MapToResponseDto(b, b.Medicine));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MedicineBatchResponseDto>> GetAvailableBatchesAsync()
    {
        _logger.LogInformation("Getting all available batches");

        var batches = await _batchRepository.GetAvailableBatchesAsync();
        return batches.Select(b => MapToResponseDto(b, b.Medicine));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MedicineBatchResponseDto>> GetAvailableBatchesByMedicineIdAsync(int medicineId)
    {
        _logger.LogInformation("Getting available batches for medicine {MedicineId} (FIFO order)", medicineId);

        var batches = await _batchRepository.GetAvailableBatchesByMedicineIdAsync(medicineId);
        return batches.Select(b => MapToResponseDto(b, b.Medicine));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MedicineBatchResponseDto>> GetAllBatchesAsync(string? searchFilter = null)
    {
        _logger.LogInformation("Getting all batches with filter: {Filter}", searchFilter ?? "none");

        var batches = await _batchRepository.GetAllAsync(searchFilter);
        return batches.Select(b => MapToResponseDto(b, b.Medicine));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MedicineBatchResponseDto>> GetExpiringBatchesAsync(int daysThreshold = 60)
    {
        _logger.LogInformation("Getting batches expiring within {Days} days", daysThreshold);

        var batches = await _batchRepository.GetExpiringBatchesAsync(daysThreshold);
        return batches.Select(b => MapToResponseDto(b, b.Medicine));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MedicineBatchResponseDto>> GetExpiredBatchesAsync()
    {
        _logger.LogInformation("Getting expired batches");

        var batches = await _batchRepository.GetExpiredBatchesAsync();
        return batches.Select(b => MapToResponseDto(b, b.Medicine));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MedicineBatchResponseDto>> GetBatchesByStatusAsync(string status)
    {
        _logger.LogInformation("Getting batches with status {Status}", status);

        var batches = await _batchRepository.GetBatchesByStatusAsync(status);
        return batches.Select(b => MapToResponseDto(b, b.Medicine));
    }

    // ===================== Business Operations =====================

    /// <inheritdoc/>
    public async Task<BatchSaleResultDto> SellFromBatchFIFOAsync(int medicineId, int quantity, int userId)
    {
        _logger.LogInformation("FIFO Selling {Quantity} units of medicine {MedicineId}", quantity, medicineId);

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than 0 | الكمية يجب أن تكون أكبر من صفر");
        }

        // Get available batches ordered by expiry date (FIFO)
        var availableBatches = (await _batchRepository.GetAvailableBatchesByMedicineIdAsync(medicineId)).ToList();

        if (!availableBatches.Any())
        {
            _logger.LogWarning("No available batches for medicine {MedicineId}", medicineId);
            return new BatchSaleResultDto
            {
                Success = false,
                Message = $"No available batches for this medicine | لا توجد دفعات متاحة لهذا الدواء",
                TotalQuantitySold = 0
            };
        }

        // Calculate total available quantity
        var totalAvailable = availableBatches.Sum(b => b.RemainingQuantity);
        if (totalAvailable < quantity)
        {
            _logger.LogWarning("Insufficient quantity. Requested: {Requested}, Available: {Available}",
                quantity, totalAvailable);
            return new BatchSaleResultDto
            {
                Success = false,
                Message = $"Insufficient quantity. Available: {totalAvailable}, Requested: {quantity} | الكمية غير كافية. المتاح: {totalAvailable}، المطلوب: {quantity}",
                TotalQuantitySold = 0
            };
        }

        var result = new BatchSaleResultDto
        {
            Success = true,
            Message = "Sale completed successfully | تمت عملية البيع بنجاح",
            BatchDetails = new List<BatchSaleDetailDto>()
        };

        var remainingToSell = quantity;

        foreach (var batch in availableBatches)
        {
            if (remainingToSell <= 0)
                break;

            var sellFromThisBatch = Math.Min(remainingToSell, batch.RemainingQuantity);

            // Trigger Audited Movement
            await _stockMovementService.CreateManualMovementAsync(new CreateManualMovementDto
            {
                MedicineId = batch.MedicineId,
                BatchId = batch.Id,
                Quantity = sellFromThisBatch,
                Type = StockMovementType.Sale, // Using Sale type for legacy FIFO quick sale
                Reason = "Quick Sale (FIFO Legacy)",
                ApprovedBy = userId
            });

            result.BatchDetails.Add(new BatchSaleDetailDto
            {
                BatchId = batch.Id,
                CompanyBatchNumber = batch.CompanyBatchNumber,
                QuantitySold = sellFromThisBatch,
                ExpiryDate = batch.ExpiryDate,
                RemainingQuantity = batch.RemainingQuantity
            });

            result.TotalQuantitySold += sellFromThisBatch;
            remainingToSell -= sellFromThisBatch;
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("FIFO Sale completed. Sold {Quantity} units from {BatchCount} batches",
            result.TotalQuantitySold, result.BatchDetails.Count);

        return result;
    }

    /// <inheritdoc/>
    public async Task<MedicineBatchResponseDto> SellFromBatchAsync(int batchId, int quantity, int userId)
    {
        _logger.LogInformation("Selling {Quantity} units from batch {BatchId}", quantity, batchId);

        var validation = await ValidateBatchForSaleAsync(batchId, quantity);
        if (!validation.IsValid)
        {
            var errorMessage = string.Join("; ", validation.Errors);
            throw new InvalidOperationException(errorMessage);
        }

        var batch = await _batchRepository.GetByIdAsync(batchId);
        if (batch == null)
        {
            throw new KeyNotFoundException($"Batch {batchId} not found | الدفعة غير موجودة");
        }

        // Trigger Audited Movement
        await _stockMovementService.CreateManualMovementAsync(new CreateManualMovementDto
        {
            MedicineId = batch.MedicineId,
            BatchId = batch.Id,
            Quantity = quantity,
            Type = StockMovementType.Sale,
            Reason = "Direct Quick Sale",
            ApprovedBy = userId
        });

        _logger.LogInformation("Sold {Quantity} units from batch {BatchId}. Remaining: {Remaining}",
            quantity, batchId, batch.RemainingQuantity);

        return MapToResponseDto(batch, batch.Medicine);
    }

    /// <inheritdoc/>
    public async Task<MedicineBatchResponseDto> ReturnToBatchAsync(int batchId, int quantity, int userId)
    {
        _logger.LogInformation("Returning {Quantity} units to batch {BatchId}", quantity, batchId);

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than 0 | الكمية يجب أن تكون أكبر من صفر");
        }

        var batch = await _batchRepository.GetByIdAsync(batchId);
        if (batch == null)
        {
            throw new KeyNotFoundException($"Batch {batchId} not found | الدفعة غير موجودة");
        }

        // Trigger Audited Movement
        await _stockMovementService.CreateManualMovementAsync(new CreateManualMovementDto
        {
            MedicineId = batch.MedicineId,
            BatchId = batch.Id,
            Quantity = quantity,
            Type = StockMovementType.SalesReturn,
            Reason = "Direct Quick Return",
            ApprovedBy = userId
        });

        _logger.LogInformation("Returned {Quantity} units to batch {BatchId}. New remaining: {Remaining}",
            quantity, batchId, batch.RemainingQuantity);

        return MapToResponseDto(batch, batch.Medicine);
    }

    /// <inheritdoc/>
    public async Task<MedicineBatchResponseDto> MarkBatchAsDamagedAsync(int batchId, string? reason, int userId)
    {
        _logger.LogInformation("Marking batch {BatchId} as damaged. Reason: {Reason}", batchId, reason);

        var batch = await _batchRepository.GetByIdAsync(batchId);
        if (batch == null)
        {
            throw new KeyNotFoundException($"Batch {batchId} not found | الدفعة غير موجودة");
        }

        // Trigger Audited Movement (Damage)
        await _stockMovementService.CreateManualMovementAsync(new CreateManualMovementDto
        {
            MedicineId = batch.MedicineId,
            BatchId = batch.Id,
            Quantity = batch.RemainingQuantity, // Mark entire remaining as damaged if using this method
            Type = StockMovementType.Damage,
            Reason = reason ?? "Damaged (Legacy Mark)",
            ApprovedBy = userId
        });

        _logger.LogInformation("Batch {BatchId} marked as damaged | تم وضع علامة على الدفعة كتالفة", batchId);

        return MapToResponseDto(batch, batch.Medicine);
    }

    /// <inheritdoc/>
    public async Task<int> UpdateExpiredBatchesAsync()
    {
        _logger.LogInformation("Updating status of expired batches");

        var count = await _batchRepository.UpdateExpiredBatchesStatusAsync();
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Updated {Count} expired batches | تم تحديث {Count} دفعة منتهية الصلاحية", count, count);

        return count;
    }

    /// <inheritdoc/>
    public async Task<int> GetTotalAvailableQuantityAsync(int medicineId)
    {
        return await _batchRepository.GetTotalAvailableQuantityAsync(medicineId);
    }

    /// <inheritdoc/>
    public async Task<BatchValidationResultDto> ValidateBatchForSaleAsync(int batchId, int quantity)
    {
        var result = new BatchValidationResultDto { IsValid = true, Errors = new List<string>() };

        var batch = await _batchRepository.GetByIdAsync(batchId);
        if (batch == null)
        {
            result.IsValid = false;
            result.Errors.Add($"Batch {batchId} not found | الدفعة غير موجودة");
            return result;
        }

        result.AvailableQuantity = batch.RemainingQuantity;

        if (batch.IsDeleted)
        {
            result.IsValid = false;
            result.Errors.Add("Batch is deleted | الدفعة محذوفة");
        }

        if (batch.Status == "Expired" || batch.IsExpired)
        {
            result.IsValid = false;
            result.Errors.Add("Cannot sell from expired batch | لا يمكن البيع من دفعة منتهية الصلاحية");
        }

        if (batch.Status == "Damaged")
        {
            result.IsValid = false;
            result.Errors.Add("Cannot sell from damaged batch | لا يمكن البيع من دفعة تالفة");
        }

        if (batch.Status == "Quarantined")
        {
            result.IsValid = false;
            result.Errors.Add("Cannot sell from quarantined batch | لا يمكن البيع من دفعة في الحجر الصحي");
        }

        if (batch.Status != "Active")
        {
            result.IsValid = false;
            result.Errors.Add($"Batch status is {batch.Status}, not Active | حالة الدفعة {batch.Status}، ليست نشطة");
        }

        if (quantity <= 0)
        {
            result.IsValid = false;
            result.Errors.Add("Quantity must be greater than 0 | الكمية يجب أن تكون أكبر من صفر");
        }

        if (quantity > batch.RemainingQuantity)
        {
            result.IsValid = false;
            result.Errors.Add($"Insufficient quantity. Available: {batch.RemainingQuantity}, Requested: {quantity} | الكمية غير كافية. المتاح: {batch.RemainingQuantity}، المطلوب: {quantity}");
        }

        return result;
    }

    // ===================== Helper Methods =====================

    /// <summary>
    /// Maps MedicineBatch entity to MedicineBatchResponseDto.
    /// يحول كيان دفعة الدواء إلى كائن نقل البيانات للاستجابة.
    /// </summary>
    private MedicineBatchResponseDto MapToResponseDto(MedicineBatch batch, Medicine? medicine)
    {
        return new MedicineBatchResponseDto
        {
            Id = batch.Id,
            MedicineId = batch.MedicineId,
            MedicineName = medicine?.Name ?? string.Empty,
            CompanyBatchNumber = batch.CompanyBatchNumber,
            BatchBarcode = batch.BatchBarcode ?? string.Empty,
            Quantity = batch.Quantity,
            RemainingQuantity = batch.RemainingQuantity,
            SoldQuantity = batch.SoldQuantity,
            ExpiryDate = batch.ExpiryDate,
            EntryDate = batch.EntryDate,
            UnitPurchasePrice = batch.UnitPurchasePrice,
            // SalePrice = 0, // Removed
            StorageLocation = batch.StorageLocation,
            Status = batch.Status,
            IsExpired = batch.IsExpired,
            IsExpiringSoon = batch.IsExpiringSoon,
            DaysUntilExpiry = batch.DaysUntilExpiry,
            IsSellable = batch.IsSellable,
            // CreatedAt = batch.CreatedAt, // Removed (uses EntryDate or default)
            CreatedBy = batch.CreatedBy,
            CreatedByUserName = batch.CreatedByUser?.FullName,
            IsDeleted = batch.IsDeleted
        };
    }
}
