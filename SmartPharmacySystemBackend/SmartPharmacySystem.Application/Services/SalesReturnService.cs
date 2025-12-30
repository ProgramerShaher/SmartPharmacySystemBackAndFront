using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.SalesReturns;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Services
{
    public class SalesReturnService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<SalesReturnService> logger,
        IStockMovementService stockMovementService,
        IFinancialService financialService) : ISalesReturnService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<SalesReturnService> _logger = logger;
        private readonly IStockMovementService _stockMovementService = stockMovementService;
        private readonly IFinancialService _financialService = financialService;

        public async Task<SalesReturnDto> CreateAsync(CreateSalesReturnDto dto, int userId)
        {
            if (dto.Details == null || !dto.Details.Any())
                throw new InvalidOperationException("يجب إضافة صنف واحد على الأقل للمرتجع.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var invoice = await _unitOfWork.SaleInvoices.GetByIdAsync(dto.SaleInvoiceId)
                    ?? throw new KeyNotFoundException($"فاتورة البيع {dto.SaleInvoiceId} غير موجودة");

                if (invoice.Status != DocumentStatus.Approved)
                    throw new InvalidOperationException("لا يمكن عمل مرتجع لفاتورة غير معتمدة أو ملغاة.");

                var entity = _mapper.Map<SalesReturn>(dto);
                entity.CreatedAt = DateTime.UtcNow;
                entity.CreatedBy = userId;
                entity.Status = DocumentStatus.Draft;
                entity.CustomerId = invoice.CustomerId;

                entity.TotalAmount = 0;
                entity.TotalCost = 0;
                entity.TotalProfit = 0;

                foreach (var detail in entity.SalesReturnDetails)
                {
                    var originalLine = invoice.SaleInvoiceDetails
                        .FirstOrDefault(d => d.MedicineId == detail.MedicineId && d.BatchId == detail.BatchId)
                        ?? throw new InvalidOperationException($"الصنف {detail.MedicineId} والتشغيلة {detail.BatchId} غير موجودين في الفاتورة الأصلية.");

                    if (detail.Quantity > originalLine.RemainingQtyToReturn)
                        throw new InvalidOperationException($"الكمية المرتجعة ({detail.Quantity}) تتجاوز الكمية المتاحة للإرجاع ({originalLine.RemainingQtyToReturn}) للصنف {detail.MedicineId}.");

                    detail.UnitCost = originalLine.UnitCost;
                    detail.SalePrice = originalLine.SalePrice;
                    detail.TotalCost = detail.Quantity * detail.UnitCost;
                    detail.TotalLineAmount = detail.Quantity * detail.SalePrice;
                    detail.Profit = detail.TotalLineAmount - detail.TotalCost;

                    entity.TotalAmount += detail.TotalLineAmount;
                    entity.TotalCost += detail.TotalCost;
                    entity.TotalProfit += detail.Profit;
                }

                await _unitOfWork.SalesReturns.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return _mapper.Map<SalesReturnDto>(entity);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error creating sales return drafted");
                throw;
            }
        }

        public async Task ApproveAsync(int id, int userId)
        {
            var ret = await _unitOfWork.SalesReturns.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"مرتجع المبيعات برقم {id} غير موجود");

            if (ret.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("المرتجع بالفعل معتمد أو ملغى.");

            var invoice = await _unitOfWork.SaleInvoices.GetByIdAsync(ret.SaleInvoiceId)
                ?? throw new KeyNotFoundException($"فاتورة البيع {ret.SaleInvoiceId} غير موجودة");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var detail in ret.SalesReturnDetails)
                {
                    var originalLine = invoice.SaleInvoiceDetails
                        .FirstOrDefault(d => d.MedicineId == detail.MedicineId && d.BatchId == detail.BatchId)
                        ?? throw new InvalidOperationException("فشل في ربط المرتجع بالفاتورة الأصلية.");

                    if (detail.Quantity > originalLine.RemainingQtyToReturn)
                        throw new InvalidOperationException($"فشل الاعتماد: الكمية المرتجعة أكبر من المتبقي. المتاح: {originalLine.RemainingQtyToReturn}");

                    // 1. Update Inventory State (Exactly the original batch)
                    var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(detail.BatchId)
                        ?? throw new KeyNotFoundException($"التشغيلة {detail.BatchId} غير موجودة");

                    batch.RemainingQuantity += detail.Quantity;
                    batch.SoldQuantity -= detail.Quantity;
                    await _unitOfWork.MedicineBatches.UpdateAsync(batch);

                    // 2. Update Original Invoice Remaining Qty
                    originalLine.RemainingQtyToReturn -= detail.Quantity;
                    await _unitOfWork.SaleInvoiceDetails.UpdateAsync(originalLine);
                }

                // 3. Profit Adjustment on Original Invoice (Optional: but good for gross profit reporting)
                invoice.TotalAmount -= ret.TotalAmount;
                invoice.TotalProfit -= ret.TotalProfit;
                invoice.TotalCost -= ret.TotalCost;
                await _unitOfWork.SaleInvoices.UpdateAsync(invoice);

                // 4. Financial Impact
                if (invoice.PaymentMethod == PaymentType.Credit && invoice.CustomerId.HasValue)
                {
                    // Credit Return: Decrease customer debt (Allowed to be negative)
                    await _unitOfWork.Customers.UpdateBalanceAsync(invoice.CustomerId.Value, -ret.TotalAmount);
                    _logger.LogInformation("Credit balance updated for customer {CustomerId} by -{Amount}", invoice.CustomerId, ret.TotalAmount);
                }
                else
                {
                    // Cash Return: Refund from physical vault
                    await _financialService.ProcessTransactionAsync(
                        accountId: 1, // Main Vault
                        amount: ret.TotalAmount,
                        type: FinancialTransactionType.Expense,
                        referenceType: ReferenceType.SalesReturn,
                        referenceId: ret.Id,
                        description: $"استرداد نقدي لمرتجع مبيعات - رقم المرتجع: {ret.Id}, فاتورة: {invoice.SaleInvoiceNumber}");
                }

                // 5. Update Status
                ret.Status = DocumentStatus.Approved;
                ret.ApprovedBy = userId;
                ret.ApprovedAt = DateTime.UtcNow;
                await _unitOfWork.SalesReturns.UpdateAsync(ret);

                await _unitOfWork.SaveChangesAsync();

                // 6. Movements History
                await _stockMovementService.ProcessDocumentMovementsAsync(id, ReferenceType.SalesReturn);

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error approving sales return {Id}", id);
                throw;
            }
        }

        public async Task CancelAsync(int id, int userId)
        {
            var ret = await _unitOfWork.SalesReturns.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"مرتجع المبيعات برقم {id} غير موجود");

            if (ret.Status == DocumentStatus.Cancelled)
                throw new InvalidOperationException("المرتجع ملغى بالفعل.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var wasApproved = ret.Status == DocumentStatus.Approved;
                ret.Status = DocumentStatus.Cancelled;
                ret.CancelledBy = userId;
                ret.CancelledAt = DateTime.UtcNow;

                if (wasApproved)
                {
                    var invoice = await _unitOfWork.SaleInvoices.GetByIdAsync(ret.SaleInvoiceId);

                    // Reverse Approved Return
                    foreach (var detail in ret.SalesReturnDetails)
                    {
                        var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(detail.BatchId);
                        if (batch != null)
                        {
                            if (batch.RemainingQuantity < detail.Quantity)
                                throw new InvalidOperationException($"لا يمكن إلغاء المرتجع: الرصيد في الدفعة {batch.CompanyBatchNumber} أقل من الكمية التي سيتم خصمها بالمرتجع.");

                            batch.RemainingQuantity -= detail.Quantity;
                            batch.SoldQuantity += detail.Quantity;
                            await _unitOfWork.MedicineBatches.UpdateAsync(batch);
                        }

                        var originalLine = invoice.SaleInvoiceDetails.FirstOrDefault(d => d.MedicineId == detail.MedicineId && d.BatchId == detail.BatchId);
                        if (originalLine != null)
                        {
                            originalLine.RemainingQtyToReturn += detail.Quantity;
                            await _unitOfWork.SaleInvoiceDetails.UpdateAsync(originalLine);
                        }
                    }

                    // Reverse Gross Impact
                    if (invoice != null)
                    {
                        invoice.TotalAmount += ret.TotalAmount;
                        invoice.TotalProfit += ret.TotalProfit;
                        invoice.TotalCost += ret.TotalCost;
                        await _unitOfWork.SaleInvoices.UpdateAsync(invoice);
                    }

                    await _stockMovementService.CancelDocumentMovementsAsync(id, ReferenceType.SalesReturn);

                    if (invoice != null && invoice.PaymentMethod == PaymentType.Credit && invoice.CustomerId.HasValue)
                    {
                        await _unitOfWork.Customers.UpdateBalanceAsync(invoice.CustomerId.Value, ret.TotalAmount);
                    }
                    else if (invoice != null)
                    {
                        await _financialService.ProcessTransactionAsync(
                            accountId: 1,
                            amount: ret.TotalAmount,
                            type: FinancialTransactionType.Income,
                            referenceType: ReferenceType.SalesReturn,
                            referenceId: ret.Id,
                            description: $"إلغاء مرتجع بيع (إرجاع مبلغ) - رقم: {id}");
                    }
                }

                await _unitOfWork.SalesReturns.UpdateAsync(ret);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error cancelling sales return {Id}", id);
                throw;
            }
        }

        public async Task UpdateAsync(int id, UpdateSalesReturnDto dto)
        {
            var entity = await _unitOfWork.SalesReturns.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"مرتجع المبيعات برقم {id} غير موجود");

            if (entity.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("التعديل مسموح فقط لحالة مسودة.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _mapper.Map(dto, entity);

                // Clear & Re-insert details
                entity.SalesReturnDetails ??= new List<SalesReturnDetail>();
                if (entity.SalesReturnDetails.Any())
                {
                    foreach (var detail in entity.SalesReturnDetails.ToList())
                    {
                        await _unitOfWork.SalesReturnDetails.DeleteAsync(detail.Id);
                    }
                    entity.SalesReturnDetails.Clear();
                }

                var invoice = await _unitOfWork.SaleInvoices.GetByIdAsync(entity.SaleInvoiceId);
                entity.TotalAmount = 0;
                entity.TotalCost = 0;
                entity.TotalProfit = 0;

                foreach (var itemDto in dto.Details)
                {
                    var originalLine = invoice.SaleInvoiceDetails
                        .FirstOrDefault(d => d.MedicineId == itemDto.MedicineId && d.BatchId == itemDto.BatchId)
                        ?? throw new InvalidOperationException("الصنف غير موجود في الفاتورة الأصلية.");

                    var detail = _mapper.Map<SalesReturnDetail>(itemDto);
                    detail.SalesReturnId = id;
                    detail.UnitCost = originalLine.UnitCost;
                    detail.SalePrice = originalLine.SalePrice;
                    detail.TotalCost = detail.Quantity * detail.UnitCost;
                    detail.TotalLineAmount = detail.Quantity * detail.SalePrice;
                    detail.Profit = detail.TotalLineAmount - detail.TotalCost;

                    entity.TotalAmount += detail.TotalLineAmount;
                    entity.TotalCost += detail.TotalCost;
                    entity.TotalProfit += detail.Profit;
                    entity.SalesReturnDetails.Add(detail);
                }

                await _unitOfWork.SalesReturns.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _unitOfWork.SalesReturns.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"المرتجع غير موجود");

            if (entity.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("لا يمكن حذف مرتجع معتمد.");

            await _unitOfWork.SalesReturns.SoftDeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<SalesReturnDto> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.SalesReturns.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"المرتجع غير موجود");
            return _mapper.Map<SalesReturnDto>(entity);
        }

        public async Task<IEnumerable<SalesReturnDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.SalesReturns.GetAllAsync();
            return _mapper.Map<IEnumerable<SalesReturnDto>>(entities);
        }
    }
}
