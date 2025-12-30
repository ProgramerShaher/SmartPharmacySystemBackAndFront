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
public class MedicineBatchService(
    IMedicineBatchRepository batchRepository,
    IMedicineRepository medicineRepository,
    IUnitOfWork unitOfWork,
    ILogger<MedicineBatchService> logger,
    IMapper mapper,
    IStockMovementService stockMovementService,
    INotificationService notificationService) : IMedicineBatchService
{
    // ===================== CRUD Operations =====================

    /// <inheritdoc/>
    /// <inheritdoc/>
    public async Task<MedicineBatchResponseDto> CreateBatchAsync(MedicineBatchCreateDto dto)
    {
        logger.LogInformation("Creating new batch for medicine {MedicineId} with Financial Sync", dto.MedicineId);

        // Validate medicine exists
        var medicine = await medicineRepository.GetByIdAsync(dto.MedicineId);
        if (medicine == null)
        {
            throw new KeyNotFoundException($"Medicine with ID {dto.MedicineId} not found | الدواء برقم {dto.MedicineId} غير موجود");
        }

        // Check for duplicate batch number
        if (await batchRepository.BatchNumberExistsAsync(dto.MedicineId, dto.CompanyBatchNumber))
        {
            throw new InvalidOperationException($"Batch number '{dto.CompanyBatchNumber}' already exists for this medicine | رقم الدفعة موجود بالفعل");
        }

        // Check for duplicate barcode
        if (!string.IsNullOrEmpty(dto.BatchBarcode) && await batchRepository.BarcodeExistsAsync(dto.BatchBarcode))
        {
            throw new InvalidOperationException($"Barcode '{dto.BatchBarcode}' already exists | الباركود موجود بالفعل");
        }

        await unitOfWork.BeginTransactionAsync();
        try
        {
            // 1. Calculate Total Cost
            var totalCost = dto.Quantity * dto.UnitPurchasePrice;

            // 2. Vault Check & Sync (Rule: Prevent purchase if insufficient funds)
            var vault = await unitOfWork.Financials.GetAccountByIdAsync(1); // Main Vault
            if (vault == null) throw new InvalidOperationException("Main Vault not found | الخزينة الرئيسية غير موجودة");

            if (vault.Balance < totalCost)
            {
                logger.LogWarning("Insufficient funds in vault. Required: {Required}, Available: {Available}", totalCost, vault.Balance);
                throw new InvalidOperationException($"Insufficient funds (Required: {totalCost:N2}, Available: {vault.Balance:N2}) | عذراً، الرصيد في الخزينة غير كافٍ لإتمام عملية الشراء");
            }

            // 3. Deduct from Vault
            vault.Balance -= totalCost;
            vault.LastUpdated = DateTime.UtcNow;
            await unitOfWork.Financials.UpdateAccountAsync(vault);

            // 4. Record Financial Transaction
            await unitOfWork.Financials.AddTransactionAsync(new FinancialTransaction
            {
                AccountId = 1,
                Amount = totalCost,
                Type = FinancialTransactionType.Expense,
                ReferenceType = ReferenceType.PurchaseInvoice, // Or Manual if no generic PurchaseInvoice context yet
                ReferenceId = 0, // Will update with BatchID or keep generic for direct batch creation
                Description = $"شراء دفعة أدوية جديدة - {medicine.Name} - باركود {dto.BatchBarcode ?? "N/A"}",
                TransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });

            // 5. Create Batch Entity
            var batch = new MedicineBatch
            {
                MedicineId = dto.MedicineId,
                CompanyBatchNumber = dto.CompanyBatchNumber,
                BatchBarcode = dto.BatchBarcode,
                Quantity = dto.Quantity,
                RemainingQuantity = dto.Quantity,
                SoldQuantity = 0,
                ExpiryDate = dto.ExpiryDate,
                UnitPurchasePrice = dto.UnitPurchasePrice,
                EntryDate = dto.EntryDate,
                StorageLocation = dto.StorageLocation,
                Status = "Active",
                CreatedBy = dto.CreatedBy,
                IsDeleted = false
            };

            await batchRepository.AddAsync(batch);
            await unitOfWork.SaveChangesAsync(); // Save to get Batch ID

            // 6. Record Initial Stock Movement
            var initialMovement = new InventoryMovement(
                batch.MedicineId,
                batch.Id,
                StockMovementType.Purchase,
                ReferenceType.Manual,
                batch.Quantity,
                batch.Id,
                "INIT-" + batch.CompanyBatchNumber,
                dto.CreatedBy,
                "إدخال رصيد أول المدة (شراء)"
            );

            await unitOfWork.InventoryMovements.AddAsync(initialMovement);
            await unitOfWork.SaveChangesAsync();

            // 7. Commit Transaction
            await unitOfWork.CommitAsync();

            // 8. SignalR Notification (Vault Update)
            await notificationService.SendNotificationAsync(
                "خصم مالي (شراء)",
                $"تم خصم {totalCost:N2} ريال لشراء دفعة جديدة. الرصيد الحالي: {vault.Balance:N2} ريال",
                "Info");

            logger.LogInformation("Batch created successfully with Financial Sync. Cost: {Cost}", totalCost);

            var result = await batchRepository.GetByIdAsync(batch.Id);
            return MapToResponseDto(result!, medicine);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "Error creating batch for medicine {MedicineId}", dto.MedicineId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<MedicineBatchResponseDto> UpdateBatchAsync(int batchId, MedicineBatchUpdateDto dto, int userId)
    {
        logger.LogInformation("Updating batch {BatchId} with Integrated Financial System", batchId);

        var batch = await batchRepository.GetByIdAsync(batchId)
            ?? throw new KeyNotFoundException($"Batch {batchId} not found");

        var oldRemainingQuantity = batch.RemainingQuantity;
        var oldExpiryDate = batch.ExpiryDate;

        await unitOfWork.BeginTransactionAsync();
        try
        {
            // ========== RULE 1: Adjustment Logic (Inventory <-> Vault Sync) ==========
            if (dto.RemainingQuantity.HasValue && dto.RemainingQuantity.Value != oldRemainingQuantity)
            {
                var newRemaining = dto.RemainingQuantity.Value;
                var adjustmentQty = newRemaining - oldRemainingQuantity; // Positive = Increase, Negative = Decrease
                var unitPrice = batch.UnitPurchasePrice;
                var financialValue = Math.Abs(adjustmentQty) * unitPrice;

                // A. Update Vault Balance Directly (Vault ID: 1)
                var vault = await unitOfWork.Financials.GetAccountByIdAsync(1);
                if (vault != null && financialValue > 0)
                {
                    // USER RULE: 
                    // Decrease (Loss) -> Deduct from Vault.
                    // Increase (Purchase) -> Deduct from Vault.
                    // Both deplete the Vault funds (Paying for missing items OR Paying for new items).
                    
                    if (vault.Balance < financialValue)
                    {
                         // Optional: Block if asking to pay for new items but no funds?
                         // User didn't strictly say to block update on insufficient funds like Create, 
                         // but for "Increase" it acts like Purchase.
                         // For "Decrease/Loss", it's a penalty.
                         // We will allow negative if configured, or block. 
                         // Assuming strictly following "Deduct immediately".
                    }

                    vault.Balance -= financialValue;
                    vault.LastUpdated = DateTime.UtcNow;
                    await unitOfWork.Financials.UpdateAccountAsync(vault);

                    logger.LogInformation("Vault balance deducted by {Amount} SAR due to inventory adjustment ({Type}). New Balance: {Balance}", 
                        financialValue, adjustmentQty > 0 ? "Increase" : "Loss", vault.Balance);
                }

                // B. Record Financial Transaction
                if (financialValue > 0)
                {
                    string desc = adjustmentQty > 0 
                        ? $"شراء/زيادة مخزنية للدفعة {batch.CompanyBatchNumber} - زيادة: {adjustmentQty} وحدة"
                        : $"تسوية مخزنية (نقص/تالف) للدفعة {batch.CompanyBatchNumber} - نقص: {Math.Abs(adjustmentQty)} وحدة";

                    await unitOfWork.Financials.AddTransactionAsync(new FinancialTransaction
                    {
                        AccountId = 1,
                        Amount = financialValue,
                        Type = FinancialTransactionType.Expense, // Both are outflows in this logic
                        ReferenceType = ReferenceType.ManualAdjustment,
                        ReferenceId = batch.Id,
                        Description = desc,
                        TransactionDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // C. Create Stock Movement Record
                await stockMovementService.CreateManualMovementAsync(new CreateManualMovementDto
                {
                    MedicineId = batch.MedicineId,
                    BatchId = batch.Id,
                    Quantity = Math.Abs(adjustmentQty),
                    Type = adjustmentQty > 0 ? StockMovementType.Adjustment : StockMovementType.Damage,
                    Reason = adjustmentQty > 0 ? "تعديل (زيادة) رصيد" : "تعديل (نقص) رصيد - تسوية",
                    ApprovedBy = userId
                });

                // Apply Quantity Update
                batch.RemainingQuantity = newRemaining;
            }

            // Update other fields
            if (dto.Quantity.HasValue) batch.Quantity = dto.Quantity.Value;
            if (dto.ExpiryDate.HasValue) batch.ExpiryDate = dto.ExpiryDate.Value;
            if (dto.UnitPurchasePrice.HasValue) batch.UnitPurchasePrice = dto.UnitPurchasePrice.Value;
            if (dto.StorageLocation != null) batch.StorageLocation = dto.StorageLocation;

            if (!string.IsNullOrEmpty(dto.Status))
            {
                if (dto.Status == "Active" && batch.IsExpired)
                    throw new InvalidOperationException("Cannot activate expired batch | لا يمكن تفعيل دفعة منتهية");
                batch.Status = dto.Status;
            }

            // Auto-Correct Status (Dynamic Sellable)
            if (batch.ExpiryDate < DateTime.UtcNow.Date || batch.RemainingQuantity == 0)
            {
                if (batch.Status == "Active")
                {
                    batch.Status = batch.ExpiryDate < DateTime.UtcNow.Date ? "Expired" : "OutOfStock";
                }
            }

            await batchRepository.UpdateAsync(batch);
            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitAsync();

            logger.LogInformation("Batch {BatchId} updated successfully.", batchId);
            return MapToResponseDto(batch, batch.Medicine);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "Error updating batch {BatchId}", batchId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteBatchAsync(int batchId, int deletedByUserId)
    {
        logger.LogInformation("Deleting batch {BatchId} with Smart Financial Reversal", batchId);

        var batch = await batchRepository.GetByIdAsync(batchId);
        if (batch == null) throw new KeyNotFoundException($"Batch {batchId} not found");

        await unitOfWork.BeginTransactionAsync();
        try
        {
            var residualValue = batch.RemainingQuantity * batch.UnitPurchasePrice;

            // MODIFIED: نرد المال فقط إذا كان الدواء غير منتهي الصلاحية
            bool isExpired = batch.ExpiryDate < DateTime.UtcNow.Date || batch.Status == "Expired";

            if (residualValue > 0 && !isExpired) // تمت إضافة شرط !isExpired هنا
            {
                // A. Refund to Vault (هذا الجزء سيعمل فقط للأدوية الصالحة)
                var vault = await unitOfWork.Financials.GetAccountByIdAsync(1);
                if (vault != null)
                {
                    vault.Balance += residualValue;
                    vault.LastUpdated = DateTime.UtcNow;
                    await unitOfWork.Financials.UpdateAccountAsync(vault);

                    logger.LogInformation("Vault balance refunded +{Amount} due to valid batch deletion.", residualValue);
                }

                // B. Record Transaction (Income)
                await unitOfWork.Financials.AddTransactionAsync(new FinancialTransaction
                {
                    AccountId = 1,
                    Amount = residualValue,
                    Type = FinancialTransactionType.Income,
                    Description = $"استرداد مالي (حذف خطأ إدخال) للدفعة {batch.CompanyBatchNumber}",
                    TransactionDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else if (isExpired && residualValue > 0)
            {
                // MODIFIED: إذا كان منتهي، نسجلها كخسارة ولا نلمس رصيد الخزينة (الرصيد يبقى ثابتاً)
                await unitOfWork.Financials.AddTransactionAsync(new FinancialTransaction
                {
                    AccountId = 1,
                    Amount = residualValue,
                    Type = FinancialTransactionType.Expense, // أو نوع مخصص للخسائر (Loss)
                    Description = $"تسجيل خسارة إعدام دفعة منتهية {batch.CompanyBatchNumber}",
                    TransactionDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });

                logger.LogInformation("Batch is expired. Recorded as loss, no money returned to vault.");
            }

            // Soft Delete والتحركات المخزنية تبقى كما هي
            batch.SoftDelete();
            await batchRepository.UpdateAsync(batch);

            await stockMovementService.CreateManualMovementAsync(new CreateManualMovementDto
            {
                MedicineId = batch.MedicineId,
                BatchId = batch.Id,
                Quantity = batch.RemainingQuantity,
                Type = isExpired ? StockMovementType.Damage : StockMovementType.Adjustment,
                Reason = isExpired ? "إعدام دواء منتهي" : $"حذف بواسطة {deletedByUserId}",
                ApprovedBy = deletedByUserId
            });

            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitAsync();

            return true;
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "Error deleting batch {BatchId}", batchId);
            throw;
        }
    }
    // ===================== Query Operations =====================

    /// <inheritdoc/>
    public async Task<MedicineBatchResponseDto?> GetBatchByIdAsync(int batchId)
    {
        logger.LogInformation("Getting batch {BatchId}", batchId);

        var batch = await batchRepository.GetByIdAsync(batchId);
        if (batch == null)
            return null;

        return MapToResponseDto(batch, batch.Medicine);
    }

    /// <inheritdoc/>
    public async Task<MedicineBatchResponseDto?> GetBatchByBarcodeAsync(string barcode)
    {
        logger.LogInformation("Getting batch by barcode {Barcode}", barcode);

        if (string.IsNullOrWhiteSpace(barcode))
        {
            throw new ArgumentException("Barcode cannot be empty | الباركود لا يمكن أن يكون فارغاً");
        }

        var batch = await batchRepository.GetByBarcodeAsync(barcode);
        if (batch == null)
        {
            logger.LogWarning("No batch found with barcode {Barcode}", barcode);
            throw new KeyNotFoundException($"No batch found with barcode '{barcode}' | لا توجد دفعة بالباركود '{barcode}'");
        }

        return MapToResponseDto(batch, batch.Medicine);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MedicineBatchResponseDto>> GetBatchesByMedicineIdAsync(int medicineId)
    {
        logger.LogInformation("Getting batches for medicine {MedicineId}", medicineId);

        var batches = await batchRepository.GetBatchesByMedicineIdAsync(medicineId);

        if (!batches.Any())
        {
            logger.LogInformation("No batches found for medicine {MedicineId}", medicineId);
        }

        return batches.Select(b => MapToResponseDto(b, b.Medicine));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MedicineBatchResponseDto>> GetAvailableBatchesAsync()
    {
        logger.LogInformation("Getting all available batches");

        var batches = await batchRepository.GetAvailableBatchesAsync();
        return batches.Select(b => MapToResponseDto(b, b.Medicine));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MedicineBatchResponseDto>> GetAvailableBatchesByMedicineIdAsync(int medicineId)
    {
        logger.LogInformation("Getting available batches for medicine {MedicineId} (FIFO order)", medicineId);

        var batches = await batchRepository.GetAvailableBatchesByMedicineIdAsync(medicineId);
        return batches.Select(b => MapToResponseDto(b, b.Medicine));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MedicineBatchResponseDto>> GetAllBatchesAsync(string? searchFilter = null)
    {
        logger.LogInformation("Getting all batches with filter: {Filter}", searchFilter ?? "none");

        var batches = await batchRepository.GetAllAsync(searchFilter);
        return batches.Select(b => MapToResponseDto(b, b.Medicine));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MedicineBatchResponseDto>> GetExpiringBatchesAsync(int daysThreshold = 60)
    {
        logger.LogInformation("Getting batches expiring within {Days} days", daysThreshold);

        var batches = await batchRepository.GetExpiringBatchesAsync(daysThreshold);
        return batches.Select(b => MapToResponseDto(b, b.Medicine));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MedicineBatchResponseDto>> GetExpiredBatchesAsync()
    {
        logger.LogInformation("Getting expired batches");

        var batches = await batchRepository.GetExpiredBatchesAsync();
        return batches.Select(b => MapToResponseDto(b, b.Medicine));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MedicineBatchResponseDto>> GetBatchesByStatusAsync(string status)
    {
        logger.LogInformation("Getting batches with status {Status}", status);

        var batches = await batchRepository.GetBatchesByStatusAsync(status);
        return batches.Select(b => MapToResponseDto(b, b.Medicine));
    }

    // ===================== Business Operations =====================

    /// <inheritdoc/>
    public async Task<BatchSaleResultDto> SellFromBatchFIFOAsync(int medicineId, int quantity, int userId)
    {
        logger.LogInformation("FIFO Selling {Quantity} units of medicine {MedicineId}", quantity, medicineId);

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than 0 | الكمية يجب أن تكون أكبر من صفر");
        }

        // Get available batches ordered by expiry date (FIFO)
        var availableBatches = (await batchRepository.GetAvailableBatchesByMedicineIdAsync(medicineId)).ToList();

        if (!availableBatches.Any())
        {
            logger.LogWarning("No available batches for medicine {MedicineId}", medicineId);
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
            logger.LogWarning("Insufficient quantity. Requested: {Requested}, Available: {Available}",
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
            await stockMovementService.CreateManualMovementAsync(new CreateManualMovementDto
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

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("FIFO Sale completed. Sold {Quantity} units from {BatchCount} batches",
            result.TotalQuantitySold, result.BatchDetails.Count);

        return result;
    }

    /// <inheritdoc/>
    public async Task<MedicineBatchResponseDto> SellFromBatchAsync(int batchId, int quantity, int userId)
    {
        logger.LogInformation("Selling {Quantity} units from batch {BatchId}", quantity, batchId);

        var validation = await ValidateBatchForSaleAsync(batchId, quantity);
        if (!validation.IsValid)
        {
            var errorMessage = string.Join("; ", validation.Errors);
            throw new InvalidOperationException(errorMessage);
        }

        var batch = await batchRepository.GetByIdAsync(batchId);
        if (batch == null)
        {
            throw new KeyNotFoundException($"Batch {batchId} not found | الدفعة غير موجودة");
        }

        // Trigger Audited Movement
        await stockMovementService.CreateManualMovementAsync(new CreateManualMovementDto
        {
            MedicineId = batch.MedicineId,
            BatchId = batch.Id,
            Quantity = quantity,
            Type = StockMovementType.Sale,
            Reason = "Direct Quick Sale",
            ApprovedBy = userId
        });

        logger.LogInformation("Sold {Quantity} units from batch {BatchId}. Remaining: {Remaining}",
            quantity, batchId, batch.RemainingQuantity);

        return MapToResponseDto(batch, batch.Medicine);
    }

    /// <inheritdoc/>
    public async Task<MedicineBatchResponseDto> ReturnToBatchAsync(int batchId, int quantity, int userId)
    {
        logger.LogInformation("Returning {Quantity} units to batch {BatchId}", quantity, batchId);

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than 0 | الكمية يجب أن تكون أكبر من صفر");
        }

        var batch = await batchRepository.GetByIdAsync(batchId);
        if (batch == null)
        {
            throw new KeyNotFoundException($"Batch {batchId} not found | الدفعة غير موجودة");
        }

        // Trigger Audited Movement
        await stockMovementService.CreateManualMovementAsync(new CreateManualMovementDto
        {
            MedicineId = batch.MedicineId,
            BatchId = batch.Id,
            Quantity = quantity,
            Type = StockMovementType.SalesReturn,
            Reason = "Direct Quick Return",
            ApprovedBy = userId
        });

        logger.LogInformation("Returned {Quantity} units to batch {BatchId}. New remaining: {Remaining}",
            quantity, batchId, batch.RemainingQuantity);

        return MapToResponseDto(batch, batch.Medicine);
    }

    /// <inheritdoc/>
    public async Task<MedicineBatchResponseDto> MarkBatchAsDamagedAsync(int batchId, string? reason, int userId)
    {
        logger.LogInformation("Marking batch {BatchId} as damaged. Reason: {Reason}", batchId, reason);

        var batch = await batchRepository.GetByIdAsync(batchId);
        if (batch == null)
        {
            throw new KeyNotFoundException($"Batch {batchId} not found | الدفعة غير موجودة");
        }

        // Trigger Audited Movement (Damage)
        await stockMovementService.CreateManualMovementAsync(new CreateManualMovementDto
        {
            MedicineId = batch.MedicineId,
            BatchId = batch.Id,
            Quantity = batch.RemainingQuantity, // Mark entire remaining as damaged if using this method
            Type = StockMovementType.Damage,
            Reason = reason ?? "Damaged (Legacy Mark)",
            ApprovedBy = userId
        });

        logger.LogInformation("Batch {BatchId} marked as damaged | تم وضع علامة على الدفعة كتالفة", batchId);

        return MapToResponseDto(batch, batch.Medicine);
    }

    /// <inheritdoc/>
    public async Task ScrapBatchAsync(int batchId, int userId, string reason)
    {
        var batch = await batchRepository.GetByIdAsync(batchId)
            ?? throw new KeyNotFoundException($"Batch {batchId} not found");

        if (batch.RemainingQuantity <= 0)
            throw new InvalidOperationException("لا يوجد مخزون متبقي للإعدام");

        await unitOfWork.BeginTransactionAsync();
        try
        {
            var price = batch.UnitPurchasePrice > 0 ? batch.UnitPurchasePrice : 0;
            var lossAmount = batch.RemainingQuantity * price;

            // 1. Create Financial Reversal (Loss)
            if (lossAmount > 0)
            {
                await unitOfWork.Financials.AddTransactionAsync(new FinancialTransaction
                {
                    AccountId = 1, // Main Vault/Treasury
                    Amount = lossAmount,
                    Type = FinancialTransactionType.Expense,
                    ReferenceType = ReferenceType.Expense,
                    ReferenceId = batch.Id,
                    Description = $"إعدام (Scraping) دفعة {batch.CompanyBatchNumber}: {reason}",
                    TransactionDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // 2. Create Stock Movement (Damage/Scrap)
            await stockMovementService.CreateManualMovementAsync(new CreateManualMovementDto
            {
                MedicineId = batch.MedicineId,
                BatchId = batch.Id,
                Quantity = batch.RemainingQuantity,
                Type = StockMovementType.Damage,
                Reason = $"إعدام: {reason}",
                ApprovedBy = userId
            });

            // 3. Update Batch Status
            batch.Status = "Scrapped";
            batch.RemainingQuantity = 0; // Explicitly zero out
            await batchRepository.UpdateAsync(batch);

            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitAsync();

            logger.LogInformation("Batch {BatchId} scrapped successfully. Financial loss: {LossAmount}", batchId, lossAmount);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "Error scrapping batch {BatchId}", batchId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task ProcessFinancialLossAsync(int batchId)
    {
        // ========== RULE 3: Expiry & Scrapping Logic ==========
        logger.LogInformation("Processing Expiry/Scrapping Loss for Batch {BatchId}", batchId);

        var batch = await batchRepository.GetByIdAsync(batchId)
            ?? throw new KeyNotFoundException($"Batch {batchId} not found");

        if (batch.RemainingQuantity <= 0) return;

        await unitOfWork.BeginTransactionAsync();
        try
        {
            var lossAmount = batch.RemainingQuantity * batch.UnitPurchasePrice;

            // 1. Deduct from Vault (Loss due to expiration)
            var vault = await unitOfWork.Financials.GetAccountByIdAsync(1);
            if (vault != null)
            {
                vault.Balance -= lossAmount;
                vault.LastUpdated = DateTime.UtcNow;
                await unitOfWork.Financials.UpdateAccountAsync(vault);
            }

            // 2. Financial Transaction (Expense)
            await unitOfWork.Financials.AddTransactionAsync(new FinancialTransaction
            {
                AccountId = 1,
                Amount = lossAmount,
                Type = FinancialTransactionType.Expense,
                ReferenceType = ReferenceType.Expense, // Clarified as Expense/Loss
                ReferenceId = batch.Id,
                Description = $"خسارة تلقائية (إعدام/تالف) - دفعة {batch.CompanyBatchNumber} - باركود {batch.BatchBarcode}",
                TransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });

            // 3. Update Batch (Scrap)
            var oldQty = batch.RemainingQuantity;
            batch.Status = "Scrapped";
            batch.RemainingQuantity = 0;
            await batchRepository.UpdateAsync(batch);

            // 4. Stock Movement Log
            await stockMovementService.CreateManualMovementAsync(new CreateManualMovementDto
            {
                MedicineId = batch.MedicineId,
                BatchId = batch.Id,
                Quantity = oldQty,
                Type = StockMovementType.Expiry,
                Reason = "Auto-Scrapping (Expiry)",
                ApprovedBy = 1 // System
            });

            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitAsync();

            // 5. SignalR Notification
            var msg = $"تنبيه: تم إعدام الدفعة {batch.BatchBarcode} وخصم قيمتها ({lossAmount:N2} ريال) من الخزينة.";
            await notificationService.SendNotificationAsync("خسارة مالية (تالف)", msg, "Warning");

            logger.LogInformation("Batch {BatchId} scrapped. Financial impact: {Amount}", batchId, lossAmount);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<int> UpdateExpiredBatchesAsync()
    {
        logger.LogInformation("Updating status of expired batches");

        var count = await batchRepository.UpdateExpiredBatchesStatusAsync();
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Updated {Count} expired batches | تم تحديث {Count} دفعة منتهية الصلاحية", count, count);

        return count;
    }

    /// <inheritdoc/>
    public async Task<int> GetTotalAvailableQuantityAsync(int medicineId)
    {
        return await batchRepository.GetTotalAvailableQuantityAsync(medicineId);
    }

    /// <inheritdoc/>
    public async Task<BatchValidationResultDto> ValidateBatchForSaleAsync(int batchId, int quantity)
    {
        var result = new BatchValidationResultDto { IsValid = true, Errors = new List<string>() };

        var batch = await batchRepository.GetByIdAsync(batchId);
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
